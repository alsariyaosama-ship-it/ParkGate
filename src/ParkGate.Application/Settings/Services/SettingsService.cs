using FluentValidation;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Audit;
using ParkGate.Application.Ports.Settings;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Models;

namespace ParkGate.Application.Settings.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly IAppSettingsStore _settingsStore;
    private readonly IAuditWriter _auditWriter;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;
    private readonly IValidator<UpdateSettingsRequest> _validator;

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private volatile AppSettingsModel? _cached;

    public SettingsService(
        IAppSettingsStore settingsStore,
        IAuditWriter auditWriter,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        IValidator<UpdateSettingsRequest> validator)
    {
        _settingsStore = settingsStore;
        _auditWriter = auditWriter;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
        _validator = validator;
    }

    public async Task<AppSettingsModel> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (_cached is not null)
        {
            return _cached;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_cached is not null)
            {
                return _cached;
            }

            var dict = await _settingsStore.GetAllAsync(cancellationToken);

            _cached = new AppSettingsModel(
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

            return _cached;
        }
        finally
        {
            _semaphore.Release();
        }
    }

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

        await _settingsStore.SetValueAsync("Entry.DefaultCooldownMinutes", request.DefaultCooldownMinutes.ToString(), userId, cancellationToken);
        await _settingsStore.SetValueAsync("Entry.MaxGroupSize", request.MaxGroupSize.ToString(), userId, cancellationToken);
        await _settingsStore.SetValueAsync("Entry.DenialDedupMinutes", request.DenialDedupMinutes.ToString(), userId, cancellationToken);
        await _settingsStore.SetValueAsync("Entry.PrefillCompanionsFromLastVisit", request.PrefillCompanionsFromLastVisit.ToString(), userId, cancellationToken);
        await _settingsStore.SetValueAsync("Entry.DenialSoundEnabled", request.DenialSoundEnabled.ToString(), userId, cancellationToken);
        await _settingsStore.SetValueAsync("Security.MaxFailedLogins", request.MaxFailedLogins.ToString(), userId, cancellationToken);
        await _settingsStore.SetValueAsync("Security.LockoutMinutes", request.LockoutMinutes.ToString(), userId, cancellationToken);
        await _settingsStore.SetValueAsync("Security.AutoLockMinutes", request.AutoLockMinutes.ToString(), userId, cancellationToken);
        await _settingsStore.SetValueAsync("Park.Name", request.ParkName, userId, cancellationToken);
        await _settingsStore.SetValueAsync("Park.TimeZoneId", request.TimeZoneId, userId, cancellationToken);

        InvalidateCache();

        await _auditWriter.WriteAsync(
            action: "SettingsUpdated",
            userId: userId,
            workstationId: _currentWorkstation.WorkstationId,
            cancellationToken: cancellationToken);

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
