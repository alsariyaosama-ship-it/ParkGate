using ParkGate.Application.Common;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;

namespace ParkGate.Application.Maintenance.Services;

/// <summary>Database size against the SQL Server Express limit, shown on the admin backup action (R6.8).</summary>
public sealed record DatabaseSizeDto(long DataBytes, long LimitBytes, int PercentUsed, bool IsNearLimit);

/// <summary>Reports the database size so an admin sees the Express limit coming (R6.8).</summary>
public interface IDatabaseSizeService
{
    Task<Result<DatabaseSizeDto>> GetAsync(CancellationToken cancellationToken = default);
}

public sealed class DatabaseSizeService : IDatabaseSizeService
{
    /// <summary>SQL Server Express caps each database's data at 10 GB; the park runs on Express (AGENTS.md §2).</summary>
    public const long ExpressDataLimitBytes = 10L * 1024 * 1024 * 1024;

    /// <summary>Warn early enough to agree a retention policy before the database stops accepting writes.</summary>
    public const int WarningPercent = 70;

    private readonly IDatabaseSizeQueries _queries;
    private readonly ICurrentUser _currentUser;

    public DatabaseSizeService(IDatabaseSizeQueries queries, ICurrentUser currentUser)
    {
        _queries = queries;
        _currentUser = currentUser;
    }

    public async Task<Result<DatabaseSizeDto>> GetAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.TriggerBackup))
        {
            return Errors.General.Forbidden;
        }

        var bytes = await _queries.GetDataFilesSizeBytesAsync(cancellationToken);
        var percent = (int)(bytes * 100 / ExpressDataLimitBytes);
        return new DatabaseSizeDto(bytes, ExpressDataLimitBytes, percent, percent >= WarningPercent);
    }
}
