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
    private readonly IPayloadEncryptionService _encryptionService;
    private readonly IOptionsMonitor<SyncOptions> _syncOptions;
    private readonly ILogger<OutboxProcessorWorker> _logger;

    public OutboxProcessorWorker(
        IUnitOfWorkFactory uowFactory,
        ICloudPushService pushService,
        IPayloadEncryptionService encryptionService,
        IOptionsMonitor<SyncOptions> syncOptions,
        ILogger<OutboxProcessorWorker> logger)
    {
        _uowFactory = uowFactory;
        _pushService = pushService;
        _encryptionService = encryptionService;
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

    private async Task ProcessBatchAsync(SyncOptions options, CancellationToken cancellationToken)
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

        // 2. Encrypt sensitive fields
        var validRecords = new List<SyncOutbox>(deduplicated.Count);
        foreach (var record in deduplicated)
        {
            try
            {
                var encryptedPayload = _encryptionService.EncryptSensitiveFields(record.EntityType, record.PayloadJson);
                record.UpdatePayload(encryptedPayload);
                validRecords.Add(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Encryption error on outbox record {Id}. Skipping this cycle.", record.Id);
                record.RecordFailure($"Encryption error: {ex.Message}");
            }
        }

        if (validRecords.Count == 0)
        {
            await uow.SaveChangesAsync(cancellationToken);
            return;
        }

        // 3. Push batch to cloud
        var success = await _pushService.PushBatchAsync(validRecords, cancellationToken);

        // 4. Update status or prune upon success
        if (success)
        {
            _logger.LogInformation(
                "Batch of {Count} records synced successfully. Pruning records from local database.",
                validRecords.Count);
            await uow.SyncOutboxes.DeleteRangeAsync(validRecords, cancellationToken);
        }
        else
        {
            _logger.LogWarning(
                "Failed to push batch of {Count} records to cloud. Recording failure counter.",
                validRecords.Count);
            foreach (var record in validRecords)
            {
                record.RecordFailure("Cloud API request failed or returned error.");
            }
        }

        await uow.SaveChangesAsync(cancellationToken);
    }
}
