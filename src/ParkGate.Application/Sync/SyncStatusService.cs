using System.Globalization;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Settings;
using ParkGate.Application.Security;

namespace ParkGate.Application.Sync;

public sealed class SyncStatusService : ISyncStatusService
{
    private readonly IAppSettingsStore _settingsStore;
    private readonly ICurrentUser _currentUser;

    public SyncStatusService(IAppSettingsStore settingsStore, ICurrentUser currentUser)
    {
        _settingsStore = settingsStore;
        _currentUser = currentUser;
    }

    public async Task<Result<SyncStatusDto>> GetAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageSettings))
        {
            return Result.Failure<SyncStatusDto>(Errors.General.Forbidden);
        }

        var pending = ParseCount(await _settingsStore.GetValueAsync(SyncSettingKeys.PendingCount, cancellationToken));
        var failed = ParseCount(await _settingsStore.GetValueAsync(SyncSettingKeys.FailedCount, cancellationToken));
        var heartbeat = ParseUtc(await _settingsStore.GetValueAsync(SyncSettingKeys.LastHeartbeatUtc, cancellationToken));

        return Result.Success(new SyncStatusDto(pending, failed, heartbeat));
    }

    private static int ParseCount(string? raw) =>
        int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var value) ? value : 0;

    private static DateTime? ParseUtc(string? raw) =>
        DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value)
            ? value
            : null;
}
