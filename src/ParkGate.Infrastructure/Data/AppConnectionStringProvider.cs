using Microsoft.Extensions.Configuration;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Security;

namespace ParkGate.Infrastructure.Data;

/// <summary>
/// Manages the active database connection string, resolving in priority order:
/// 1. In-memory runtime override (e.g. from connection settings dialog)
/// 2. DPAPI encrypted %ProgramData%\ParkGate\connection.dat
/// 3. Configuration fallback (DefaultConnection in appsettings.json or LocalDB)
/// </summary>
public sealed class AppConnectionStringProvider : IAppConnectionStringProvider
{
    private const string FallbackDevConnection =
        "Server=(localdb)\\mssqllocaldb;Database=ParkGate_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    private readonly IConnectionSettingsStore _settingsStore;
    private readonly string _defaultConnectionString;
    private readonly object _lock = new();
    private string? _runtimeOverride;

    public AppConnectionStringProvider(IConnectionSettingsStore settingsStore, IConfiguration? configuration = null)
    {
        _settingsStore = settingsStore;
        _defaultConnectionString = configuration?.GetConnectionString("DefaultConnection") ?? FallbackDevConnection;
    }

    public string CurrentConnectionString
    {
        get
        {
            lock (_lock)
            {
                if (!string.IsNullOrWhiteSpace(_runtimeOverride))
                {
                    return _runtimeOverride;
                }

                var stored = _settingsStore.GetConnectionString();
                if (!string.IsNullOrWhiteSpace(stored))
                {
                    return stored;
                }

                return _defaultConnectionString;
            }
        }
    }

    public void UpdateConnectionString(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        lock (_lock)
        {
            _runtimeOverride = connectionString;
        }
    }
}
