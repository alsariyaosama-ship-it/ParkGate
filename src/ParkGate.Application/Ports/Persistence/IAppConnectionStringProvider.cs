namespace ParkGate.Application.Ports.Persistence;

/// <summary>
/// Provides and dynamically updates the active SQL Server connection string used by DbContext factory.
/// </summary>
public interface IAppConnectionStringProvider
{
    /// <summary>
    /// Gets the current active connection string (from DPAPI, config, or runtime override).
    /// </summary>
    string CurrentConnectionString { get; }

    /// <summary>
    /// Updates the active connection string at runtime.
    /// </summary>
    void UpdateConnectionString(string connectionString);
}
