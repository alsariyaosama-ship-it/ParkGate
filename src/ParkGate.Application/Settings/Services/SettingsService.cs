using System.Globalization;
using FluentValidation;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Settings;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Models;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.Settings.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly IAppSettingsStore _settingsStore;
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;
    private readonly IValidator<UpdateSettingsRequest> _validator;

    /// <summary>
    /// Each desk caches its own copy, so a change saved on another desk reaches this one within this time
    /// (R3.6). Check-in reads the cache instead of querying inside its locked transaction.
    /// </summary>
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(1);

    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private volatile CachedSettings? _cached;

    public SettingsService(
        IAppSettingsStore settingsStore,
        IUnitOfWorkFactory uowFactory,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        IValidator<UpdateSettingsRequest> validator,
        TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
        _settingsStore = settingsStore;
        _uowFactory = uowFactory;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
        _validator = validator;
    }

    public async Task<AppSettingsModel> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (TryGetFresh() is { } fresh)
        {
            return fresh;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (TryGetFresh() is { } loadedMeanwhile)
            {
                return loadedMeanwhile;
            }

            var dict = await _settingsStore.GetAllAsync(cancellationToken);

            var settings = new AppSettingsModel(
                DefaultCooldownMinutes: GetInt(dict, "Entry.DefaultCooldownMinutes", 720),
                MaxGroupSize: GetInt(dict, "Entry.MaxGroupSize", 10),
                DenialDedupMinutes: GetInt(dict, "Entry.DenialDedupMinutes", 5),
                PrefillCompanionsFromLastVisit: GetBool(dict, "Entry.PrefillCompanionsFromLastVisit", true),
                DenialSoundEnabled: GetBool(dict, "Entry.DenialSoundEnabled", true),
                MaxFailedLogins: GetInt(dict, "Security.MaxFailedLogins", 5),
                LockoutMinutes: GetInt(dict, "Security.LockoutMinutes", 15),
                AutoLockMinutes: GetInt(dict, "Security.AutoLockMinutes", 0),
                ParkName: GetString(dict, "Park.Name", "صندوق التضامن الاجتماعي — متنزه العائلة"),
                TimeZoneId: GetString(dict, "Park.TimeZoneId", "Libya Standard Time")
            );

            _cached = new CachedSettings(settings, _timeProvider.GetUtcNow());
            return settings;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private AppSettingsModel? TryGetFresh()
    {
        var cached = _cached;
        return cached is not null && _timeProvider.GetUtcNow() - cached.LoadedAt < CacheLifetime
            ? cached.Settings
            : null;
    }

    private sealed record CachedSettings(AppSettingsModel Settings, DateTimeOffset LoadedAt);

    public async Task<Result> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageSettings))
        {
            return Errors.General.Forbidden;
        }

        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var userId = _currentUser.UserId;

        var values = new (string Key, string Value)[]
        {
            ("Entry.DefaultCooldownMinutes", request.DefaultCooldownMinutes.ToString(CultureInfo.InvariantCulture)),
            ("Entry.MaxGroupSize", request.MaxGroupSize.ToString(CultureInfo.InvariantCulture)),
            ("Entry.DenialDedupMinutes", request.DenialDedupMinutes.ToString(CultureInfo.InvariantCulture)),
            ("Entry.PrefillCompanionsFromLastVisit", request.PrefillCompanionsFromLastVisit.ToString()),
            ("Entry.DenialSoundEnabled", request.DenialSoundEnabled.ToString()),
            ("Security.MaxFailedLogins", request.MaxFailedLogins.ToString(CultureInfo.InvariantCulture)),
            ("Security.LockoutMinutes", request.LockoutMinutes.ToString(CultureInfo.InvariantCulture)),
            ("Security.AutoLockMinutes", request.AutoLockMinutes.ToString(CultureInfo.InvariantCulture)),
            ("Park.Name", request.ParkName),
            ("Park.TimeZoneId", request.TimeZoneId),
        };

        // All values and the audit row are saved together, stamped with the database clock (R6.11, BR-05).
        await using (var uow = _uowFactory.Create())
        {
            var nowUtc = await uow.GetDbUtcNowAsync(cancellationToken);
            foreach (var (key, value) in values)
            {
                var setting = await uow.AppSettings.GetByKeyAsync(key, cancellationToken);
                if (setting is null)
                {
                    uow.AppSettings.Add(new AppSetting(key, value, nowUtc, userId));
                }
                else
                {
                    setting.Update(value, nowUtc, userId);
                    uow.AppSettings.Update(setting);
                }
            }

            await uow.AddAuditAsync(
                action: "SettingsUpdated",
                userId: userId,
                workstationId: _currentWorkstation.WorkstationId,
                cancellationToken: cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
        }

        InvalidateCache();

        return Result.Success();
    }

    public void InvalidateCache()
    {
        _cached = null;
    }

    private static int GetInt(IReadOnlyDictionary<string, string> dict, string key, int defaultValue) =>
        dict.TryGetValue(key, out var val) && int.TryParse(val, out var parsed) ? parsed : defaultValue;

    private static bool GetBool(IReadOnlyDictionary<string, string> dict, string key, bool defaultValue) =>
        dict.TryGetValue(key, out var val) && bool.TryParse(val, out var parsed) ? parsed : defaultValue;

    private static string GetString(IReadOnlyDictionary<string, string> dict, string key, string defaultValue) =>
        dict.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val) ? val : defaultValue;
}
