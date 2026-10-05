namespace ParkGate.WinForms.Services;

public enum StartupStatus
{
    Success,
    Aborted,
    BlockedByMigrations,
    Failed
}

public interface IStartupCoordinator
{
    Task<StartupStatus> RunStartupSequenceAsync(CancellationToken cancellationToken = default);
}
