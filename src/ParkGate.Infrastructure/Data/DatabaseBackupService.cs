using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Persistence;

namespace ParkGate.Infrastructure.Data;

public sealed class DatabaseBackupService : IDatabaseBackupService
{
    private readonly IDbContextFactory<ParkGateDbContext> _contextFactory;
    private readonly ILogger<DatabaseBackupService> _logger;

    public DatabaseBackupService(
        IDbContextFactory<ParkGateDbContext> contextFactory,
        ILogger<DatabaseBackupService> logger)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<string>> BackupDatabaseAsync(string? targetFolder = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var connection = context.Database.GetDbConnection();
            var dbName = connection.Database;

            if (string.IsNullOrWhiteSpace(dbName))
            {
                dbName = "ParkGate";
            }

            // Determine SQL Server's guaranteed accessible backup directory
            var serverGuaranteedFolder = await GetDefaultBackupDirectoryAsync(context, cancellationToken);
            if (string.IsNullOrWhiteSpace(serverGuaranteedFolder))
            {
                serverGuaranteedFolder = @"C:\ParkGate_Backups";
            }

            // Ensure directory exists if on the same local machine
            try
            {
                if (!Directory.Exists(serverGuaranteedFolder))
                {
                    Directory.CreateDirectory(serverGuaranteedFolder);
                }
            }
            catch
            {
                // Might be on remote machine or restricted; SQL Server engine may create file or fail gracefully
            }

            var fileName = $"{dbName}_Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak";
            var sqlFullPath = Path.Combine(serverGuaranteedFolder, fileName).Replace("/", "\\");

            // Check if SQL Server edition supports BACKUP WITH COMPRESSION (SQL Express throws error if COMPRESSION is used)
            var supportsCompression = await SupportsBackupCompressionAsync(context, cancellationToken);
            var compressionClause = supportsCompression ? "COMPRESSION, " : string.Empty;

            var backupSql = $@"
                BACKUP DATABASE [{dbName}] 
                TO DISK = @path 
                WITH FORMAT, INIT, CHECKSUM, {compressionClause}
                NAME = @backupName;
            ";

            var pathParam = new SqlParameter("@path", SqlDbType.NVarChar, 500) { Value = sqlFullPath };
            var nameParam = new SqlParameter("@backupName", SqlDbType.NVarChar, 100) { Value = $"{dbName} Manual Backup" };

            _logger.LogInformation(
                "Starting database backup for {Database} to SQL Server path {Path} (Compression={SupportsCompression})...",
                dbName, sqlFullPath, supportsCompression);

            await context.Database.ExecuteSqlRawAsync(backupSql, [pathParam, nameParam], cancellationToken);

            _logger.LogInformation("Database backup command executed successfully on server: {Path}", sqlFullPath);

            // If a custom target folder was requested (e.g. user selected Desktop/Documents or USB drive),
            // copy the file using application process privileges to prevent SQL Server 'Operating system error 5 (Access is denied)'.
            var customFolder = targetFolder?.Trim();
            if (!string.IsNullOrWhiteSpace(customFolder))
            {
                var isDifferent = true;
                try
                {
                    var normCustom = Path.GetFullPath(customFolder).TrimEnd('\\');
                    var normServer = Path.GetFullPath(serverGuaranteedFolder).TrimEnd('\\');
                    isDifferent = !string.Equals(normCustom, normServer, StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    // Ignore path normalization errors
                }

                if (isDifferent)
                {
                    try
                    {
                        if (!Directory.Exists(customFolder))
                        {
                            Directory.CreateDirectory(customFolder);
                        }

                        var destinationPath = Path.Combine(customFolder, fileName).Replace("/", "\\");

                        if (File.Exists(sqlFullPath))
                        {
                            _logger.LogInformation("Copying backup from {Source} to target folder {Destination}...", sqlFullPath, destinationPath);
                            File.Copy(sqlFullPath, destinationPath, overwrite: true);

                            try
                            {
                                File.Delete(sqlFullPath);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Could not remove temporary backup file at {Path}", sqlFullPath);
                            }

                            return Result.Success(destinationPath);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Database backup file was created on remote server at {ServerPath}. Cannot access it via local filesystem.",
                                sqlFullPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to copy backup file to custom folder {Folder}. Returning server path.", customFolder);
                    }
                }
            }

            return Result.Success(sqlFullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute database backup.");
            return Result.Failure<string>(new Error("Database.BackupFailed", $"فشل النسخ الاحتياطي لقاعدة البيانات: {ex.Message}"));
        }
    }

    private static async Task<bool> SupportsBackupCompressionAsync(ParkGateDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            // EngineEdition:
            // 2 = Standard, 3 = Enterprise, 8 = Managed Instance
            // 4 = Express (SQL Server Express does not support BACKUP DATABASE WITH COMPRESSION)
            var edition = await context.Database
                .SqlQueryRaw<int>("SELECT CAST(SERVERPROPERTY('EngineEdition') AS INT) AS Value")
                .FirstOrDefaultAsync(cancellationToken);

            return edition is 2 or 3 or 8;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<string> GetDefaultBackupDirectoryAsync(ParkGateDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            var result = await context.Database
                .SqlQueryRaw<string>("SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(500)) AS Value")
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }
        }
        catch
        {
            // Fall back to querying data files directory
        }

        try
        {
            // Query the directory containing the primary database data file (.mdf).
            // The SQL Server service account is guaranteed to have write permission here.
            var mdfPath = await context.Database
                .SqlQueryRaw<string>("SELECT TOP (1) CAST(physical_name AS NVARCHAR(500)) AS Value FROM sys.database_files WHERE type = 0")
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(mdfPath))
            {
                var dir = Path.GetDirectoryName(mdfPath);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    return dir;
                }
            }
        }
        catch
        {
            // Ignore
        }

        return string.Empty;
    }
}
