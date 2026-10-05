using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Repositories;

public sealed class AppSettingRepository : IAppSettingRepository
{
    private readonly ParkGateDbContext _context;

    public AppSettingRepository(ParkGateDbContext context)
    {
        _context = context;
    }

    public async Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.AppSettings
            .SingleOrDefaultAsync(s => s.Key == key, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AppSettings
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);
    }

    public void Add(AppSetting setting)
    {
        _context.AppSettings.Add(setting);
    }

    public void Update(AppSetting setting)
    {
        _context.AppSettings.Update(setting);
    }
}
