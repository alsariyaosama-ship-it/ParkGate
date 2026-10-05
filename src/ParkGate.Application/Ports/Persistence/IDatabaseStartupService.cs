namespace ParkGate.Application.Ports.Persistence;

/// <summary>
/// Service checking database connectivity and pending migrations during application startup.
/// </summary>
public interface IDatabaseStartupService
{
    /// <summary>
    /// Checks whether a connection can be established to the database.
    /// If connectionString is provided, tests that connection string; otherwise tests the active one.
    /// </summary>
    Task<bool> CanConnectAsync(string? connectionString = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the list of any pending EF Core migrations that have not yet been applied.
    /// </summary>
    Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default);
}
