using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Settings;
using ParkGate.Application.Ports.Sync;
using ParkGate.Application.Sync;

namespace ParkGate.SyncAgent.Workers;

/// <summary>
/// Once a minute: records the agent's heartbeat and the outbox pending/failed counts in AppSettings
/// (read by the desk UI) and tells the cloud the same (read by the dashboard). Rows that reached the
/// retry limit stay in the outbox forever, so the failed count is what makes them visible (R2.5).
/// </summary>
public sealed class HeartbeatWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);

    private readonly IAppSettingsStore _settingsStore;
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly ICloudPushService _pushService;
    private readonly IOptionsMonitor<SyncOptions>? _syncOptions;
    private readonly ILogger<HeartbeatWorker> _logger;

    public HeartbeatWorker(
        IAppSettingsStore settingsStore,
        IUnitOfWorkFactory uowFactory,
        ICloudPushService pushService,
        ILogger<HeartbeatWorker> logger,
        IOptionsMonitor<SyncOptions>? syncOptions = null)
    {
        _settingsStore = settingsStore;
        _uowFactory = uowFactory;
        _pushService = pushService;
        _syncOptions = syncOptions;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ParkGate.SyncAgent HeartbeatWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RecordHeartbeatAsync(_syncOptions?.CurrentValue ?? new SyncOptions(), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to record SyncAgent heartbeat pulse to database.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("ParkGate.SyncAgent HeartbeatWorker stopping.");
    }

    internal async Task RecordHeartbeatAsync(SyncOptions options, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        int pending;
        int failed;
        await using (var uow = _uowFactory.Create())
        {
            (pending, failed) = await uow.SyncOutboxes.GetPendingAndFailedCountsAsync(options.MaxRetries, cancellationToken);
        }

        await _settingsStore.SetValueAsync(SyncSettingKeys.LastHeartbeatUtc, now.ToString("O"), null, cancellationToken);
        await _settingsStore.SetValueAsync(SyncSettingKeys.PendingCount, pending.ToString(), null, cancellationToken);
        await _settingsStore.SetValueAsync(SyncSettingKeys.FailedCount, failed.ToString(), null, cancellationToken);

        _logger.LogDebug("Heartbeat recorded: {Pending} pending, {Failed} failed.", pending, failed);

        if (failed > 0)
        {
            _logger.LogWarning("{Failed} outbox rows reached the retry limit and will not be sent again.", failed);
        }

        if (!options.Enabled)
        {
            return;
        }

        var result = await _pushService.SendHeartbeatAsync(pending, failed, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Cloud heartbeat failed ({FailureKind}).", result.FailureKind);
        }
    }
}
