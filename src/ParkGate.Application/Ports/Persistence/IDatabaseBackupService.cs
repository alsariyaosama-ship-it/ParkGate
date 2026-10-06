using ParkGate.Application.Common;

namespace ParkGate.Application.Ports.Persistence;

public interface IDatabaseBackupService
{
    Task<Result<string>> BackupDatabaseAsync(string? targetFolder = null, CancellationToken cancellationToken = default);
}
