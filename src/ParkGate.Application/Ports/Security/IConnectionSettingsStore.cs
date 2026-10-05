namespace ParkGate.Application.Ports.Security;

public interface IConnectionSettingsStore
{
    bool Exists();
    string? GetConnectionString();
    void SaveConnectionString(string connectionString);
    void Clear();
}
