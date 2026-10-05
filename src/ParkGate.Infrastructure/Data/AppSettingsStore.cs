using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Settings;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data;

public sealed class AppSettingsStore : IAppSettingsStore
{
    private readonly IDbContextFactory<ParkGateDbContext> _contextFactory;
    private readonly TimeProvider _timeProvider;

    public AppSettingsStore(
        IDbContextFactory<ParkGateDbContext> contextFactory,
        TimeProvider? timeProvider = null)
    {
        _contextFactory = contextFactory;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var setting = await context.AppSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Key == key, cancellationToken);

        return setting?.Value;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.AppSettings
            .AsNoTracking()
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);
    }

    public async Task SetValueAsync(string key, string value, int? updatedByUserId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

        var setting = await context.AppSettings
            .SingleOrDefaultAsync(s => s.Key == key, cancellationToken);

        if (setting == null)
        {
            setting = new AppSetting(key, value, nowUtc, updatedByUserId);
            context.AppSettings.Add(setting);
        }
        else
        {
            setting.Update(value, nowUtc, updatedByUserId);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
