using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Sync;
using ParkGate.Application.Sync;
using ParkGate.Domain.Entities;

namespace ParkGate.SyncAgent.Workers;

public sealed class OutboxProcessorWorker : BackgroundService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly ICloudPushService _pushService;
    private readonly IOptionsMonitor<SyncOptions> _syncOptions;
    private readonly ILogger<OutboxProcessorWorker> _logger;

    public OutboxProcessorWorker(
        IUnitOfWorkFactory uowFactory,
        ICloudPushService pushService,
        IOptionsMonitor<SyncOptions> syncOptions,
        ILogger<OutboxProcessorWorker> logger)
    {
        _uowFactory = uowFactory;
        _pushService = pushService;
        _syncOptions = syncOptions;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ParkGate.SyncAgent OutboxProcessorWorker started. Interval: {Interval}s, BatchSize: {BatchSize}.",
            _syncOptions.CurrentValue.IntervalSeconds,
            _syncOptions.CurrentValue.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _syncOptions.CurrentValue;

            if (options.Enabled)
            {
                try
                {
                    await ProcessBatchAsync(options, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception during sync outbox batch processing.");
                }
            }

            var delay = TimeSpan.FromSeconds(Math.Max(5, options.IntervalSeconds));
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("ParkGate.SyncAgent OutboxProcessorWorker stopping.");
    }

    internal async Task ProcessBatchAsync(SyncOptions options, CancellationToken cancellationToken)
    {
        await using var uow = _uowFactory.Create();

        var pendingRecords = await uow.SyncOutboxes.GetPendingBatchAsync(
            options.BatchSize,
            options.MaxRetries,
            cancellationToken);

        if (pendingRecords.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Found {Count} pending outbox records to process.", pendingRecords.Count);

        // 1. Deduplicate using Last-Write-Wins
        var deduplicated = OutboxDeduplicator.Deduplicate(pendingRecords);
        var obsoleteCount = pendingRecords.Count - deduplicated.Count;

        if (obsoleteCount > 0)
        {
            _logger.LogInformation("Deduplication pruned {Count} obsolete intermediate mutations.", obsoleteCount);
            var obsoleteRecords = pendingRecords.Except(deduplicated).ToList();
            await uow.SyncOutboxes.DeleteRangeAsync(obsoleteRecords, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
        }

        // 2. Push batch to cloud. The payload is sent exactly as stored (it never contains
        // phone or national ID: see OutboxEntityExtractor).
        var validRecords = deduplicated;
        var pushResult = await _pushService.PushBatchAsync(validRecords, cancellationToken);

        // 3. Update status or prune upon success
        if (pushResult.IsSuccess)
        {
            _logger.LogInformation(
                "Batch of {Count} records synced successfully. Pruning records from local database.",
                validRecords.Count);
            await uow.SyncOutboxes.DeleteRangeAsync(validRecords, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
            return;
        }

        // If network outage, timeout, or authorization failure, do NOT enter individual fallback
        // and do NOT increment failure counters on records (the network/credentials are at fault, not the records).
        if (pushResult.FailureKind == PushFailureKind.NetworkOrTimeout)
        {
            _logger.LogWarning(
                "Network outage or timeout communicating with cloud API ({Error}). Aborting cycle without incrementing record failure counters.",
                pushResult.ErrorMessage);
            return;
        }

        if (pushResult.FailureKind == PushFailureKind.Unauthorized)
        {
            _logger.LogError(
                "Cloud API rejected authentication ({Error}). Verify API key configuration. Aborting cycle without incrementing record failure counters.",
                pushResult.ErrorMessage);
            return;
        }

        // Failure is PayloadRejected or ServerError
        if (validRecords.Count > 1)
        {
            _logger.LogWarning(
                "Batch push failed ({FailureKind}: {Error}) for {Count} records. Attempting single-record fallback to isolate failures...",
                pushResult.FailureKind,
                pushResult.ErrorMessage,
                validRecords.Count);

            var individualSuccesses = new List<SyncOutbox>();
            int consecutiveServerErrors = 0;

            foreach (var record in validRecords)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var singleResult = await _pushService.PushBatchAsync([record], cancellationToken);
                if (singleResult.IsSuccess)
                {
                    individualSuccesses.Add(record);
                    consecutiveServerErrors = 0;
                }
                else
                {
                    // Circuit breaker 1: if network drops during fallback, stop immediately
                    if (singleResult.FailureKind == PushFailureKind.NetworkOrTimeout)
                    {
                        _logger.LogWarning("Network disconnected during single-record fallback. Tripping circuit breaker.");
                        break;
                    }

                    // Circuit breaker 2: if credentials revoked
                    if (singleResult.FailureKind == PushFailureKind.Unauthorized)
                    {
                        _logger.LogError("Authentication failed during single-record fallback. Aborting.");
                        break;
                    }

                    // Circuit breaker 3: if 3 consecutive server errors occur
                    if (singleResult.FailureKind == PushFailureKind.ServerError)
                    {
                        consecutiveServerErrors++;
                        record.RecordFailure($"Cloud server error: {singleResult.ErrorMessage}");
                        if (consecutiveServerErrors >= 3)
                        {
                            _logger.LogWarning("Cloud API encountered 3 consecutive server errors (5xx). Tripping circuit breaker to prevent server overload.");
                            break;
                        }
                    }
                    else
                    {
                        // PayloadRejected
                        consecutiveServerErrors = 0;
                        record.RecordFailure($"Cloud API rejected record: {singleResult.ErrorMessage}");
                    }
                }
            }

            if (individualSuccesses.Count > 0)
            {
                _logger.LogInformation(
                    "Fallback isolated {SuccessCount} successful records and {FailureCount} failures.",
                    individualSuccesses.Count,
                    validRecords.Count - individualSuccesses.Count);

                await uow.SyncOutboxes.DeleteRangeAsync(individualSuccesses, cancellationToken);
            }
        }
        else
        {
            _logger.LogWarning(
                "Failed to push single record {Id} to cloud ({FailureKind}: {Error}). Recording failure counter.",
                validRecords[0].Id,
                pushResult.FailureKind,
                pushResult.ErrorMessage);
            validRecords[0].RecordFailure($"Cloud API error ({pushResult.FailureKind}): {pushResult.ErrorMessage}");
        }

        await uow.SaveChangesAsync(cancellationToken);
    }
}
