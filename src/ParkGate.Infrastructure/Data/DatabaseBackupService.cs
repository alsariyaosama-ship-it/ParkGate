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

            // Determine target backup path on the SQL Server machine
            string folder = targetFolder?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(folder))
            {
                // Query SQL Server instance default backup directory
                folder = await GetDefaultBackupDirectoryAsync(context, cancellationToken);
            }

            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = @"C:\ParkGate_Backups";
            }

            // Ensure directory exists if we are running locally on the same server
            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            catch
            {
                // Directory creation might be on remote server, proceed with T-SQL
            }

            var fileName = $"{dbName}_Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak";
            var fullPath = Path.Combine(folder, fileName).Replace("/", "\\");

            var backupSql = $@"
                BACKUP DATABASE [{dbName}] 
                TO DISK = @path 
                WITH FORMAT, INIT, CHECKSUM, COMPRESSION, 
                NAME = @backupName;
            ";

            var pathParam = new SqlParameter("@path", SqlDbType.NVarChar, 500) { Value = fullPath };
            var nameParam = new SqlParameter("@backupName", SqlDbType.NVarChar, 100) { Value = $"{dbName} Manual Backup" };

            _logger.LogInformation("Starting database backup for {Database} to {Path}...", dbName, fullPath);

            await context.Database.ExecuteSqlRawAsync(backupSql, [pathParam, nameParam], cancellationToken);

            _logger.LogInformation("Database backup completed successfully: {Path}", fullPath);
            return Result.Success(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute database backup.");
            return Result.Failure<string>(new Error("Database.BackupFailed", $"فشل النسخ الاحتياطي لقاعدة البيانات: {ex.Message}"));
        }
    }

    private static async Task<string> GetDefaultBackupDirectoryAsync(ParkGateDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            var result = await context.Database
                .SqlQueryRaw<string>("SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(500)) AS Value")
                .FirstOrDefaultAsync(cancellationToken);

            return result ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
