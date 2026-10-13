using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Ports.Settings;

namespace ParkGate.SyncAgent.Workers;

/// <summary>
/// Writes periodic heartbeats to AppSettings ("SyncAgent.LastHeartbeatUtc")
/// allowing administrative screens or watchdogs to monitor the health of the SyncAgent service.
/// </summary>
public sealed class HeartbeatWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);

    private readonly IAppSettingsStore _settingsStore;
    private readonly ILogger<HeartbeatWorker> _logger;

    public HeartbeatWorker(IAppSettingsStore settingsStore, ILogger<HeartbeatWorker> logger)
    {
        _settingsStore = settingsStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ParkGate.SyncAgent HeartbeatWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                await _settingsStore.SetValueAsync(
                    "SyncAgent.LastHeartbeatUtc",
                    now.ToString("O"),
                    updatedByUserId: null,
                    cancellationToken: stoppingToken);

                _logger.LogDebug("Heartbeat pulse recorded: {Timestamp}", now);
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
}
