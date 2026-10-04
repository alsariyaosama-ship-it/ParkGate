namespace ParkGate.Application.Ports.Settings;

public interface IAppSettingsStore
{
    Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SetValueAsync(string key, string value, int? updatedByUserId = null, CancellationToken cancellationToken = default);
}
