using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface IAppSettingRepository
{
    Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(AppSetting setting);
    void Update(AppSetting setting);
}
