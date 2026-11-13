using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Sync;
using ParkGate.Application.Sync;
using ParkGate.Domain.Entities;
using ParkGate.SyncAgent.Workers;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Sync;

public sealed class OutboxProcessorWorkerTests
{
    private readonly IUnitOfWorkFactory _uowFactory = Substitute.For<IUnitOfWorkFactory>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ISyncOutboxRepository _outboxRepo = Substitute.For<ISyncOutboxRepository>();
    private readonly ICloudPushService _pushService = Substitute.For<ICloudPushService>();
    private readonly IOptionsMonitor<SyncOptions> _optionsMonitor = Substitute.For<IOptionsMonitor<SyncOptions>>();

    public OutboxProcessorWorkerTests()
    {
        _uow.SyncOutboxes.Returns(_outboxRepo);
        _uowFactory.Create().Returns(_uow);
        _optionsMonitor.CurrentValue.Returns(new SyncOptions
        {
            Enabled = true,
            IntervalSeconds = 5,
            BatchSize = 10,
            MaxRetries = 3
        });
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenBatchSucceeds_DeletesAllRecordsAndSaves()
    {
        var records = new List<SyncOutbox>
        {
            SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{\"name\":\"A\"}", DateTime.UtcNow),
            SyncOutbox.Create("Beneficiary", "ID-2", "Insert", "{\"name\":\"B\"}", DateTime.UtcNow)
        };

        _outboxRepo.GetPendingBatchAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(records);

        _pushService.PushBatchAsync(Arg.Any<IReadOnlyList<SyncOutbox>>(), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Success());

        var worker = new OutboxProcessorWorker(
            _uowFactory,
            _pushService,
            _optionsMonitor,
            NullLogger<OutboxProcessorWorker>.Instance);

        await worker.ProcessBatchAsync(_optionsMonitor.CurrentValue, CancellationToken.None);

        await _outboxRepo.Received(1).DeleteRangeAsync(
            Arg.Is<IEnumerable<SyncOutbox>>(list => list.Count() == 2),
            Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenBatchFailsWithNetworkOrTimeout_DoesNotEnterFallbackAndDoesNotIncrementRetryCount()
    {
        var r1 = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{\"name\":\"1\"}", DateTime.UtcNow);
        var r2 = SyncOutbox.Create("Beneficiary", "ID-2", "Insert", "{\"name\":\"2\"}", DateTime.UtcNow);
        var records = new List<SyncOutbox> { r1, r2 };

        _outboxRepo.GetPendingBatchAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(records);

        _pushService.PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 2), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Failure(PushFailureKind.NetworkOrTimeout, "Connection timed out"));

        var worker = new OutboxProcessorWorker(
            _uowFactory,
            _pushService,
            _optionsMonitor,
            NullLogger<OutboxProcessorWorker>.Instance);

        await worker.ProcessBatchAsync(_optionsMonitor.CurrentValue, CancellationToken.None);

        // Push should only be called once (no fallback)
        await _pushService.Received(1).PushBatchAsync(Arg.Any<IReadOnlyList<SyncOutbox>>(), Arg.Any<CancellationToken>());

        // No records should be deleted
        await _outboxRepo.DidNotReceive().DeleteRangeAsync(Arg.Any<IEnumerable<SyncOutbox>>(), Arg.Any<CancellationToken>());

        // Crucial: RetryCount must NOT be incremented for network issues!
        r1.RetryCount.ShouldBe(0);
        r2.RetryCount.ShouldBe(0);

        // SaveChanges should not be called since no failure was recorded
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenBatchFailsWithUnauthorized_DoesNotEnterFallbackAndDoesNotIncrementRetryCount()
    {
        var r1 = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{\"name\":\"1\"}", DateTime.UtcNow);
        var records = new List<SyncOutbox> { r1 };

        _outboxRepo.GetPendingBatchAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(records);

        _pushService.PushBatchAsync(Arg.Any<IReadOnlyList<SyncOutbox>>(), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Failure(PushFailureKind.Unauthorized, "401 Unauthorized"));

        var worker = new OutboxProcessorWorker(
            _uowFactory,
            _pushService,
            _optionsMonitor,
            NullLogger<OutboxProcessorWorker>.Instance);

        await worker.ProcessBatchAsync(_optionsMonitor.CurrentValue, CancellationToken.None);

        r1.RetryCount.ShouldBe(0);
        await _pushService.Received(1).PushBatchAsync(Arg.Any<IReadOnlyList<SyncOutbox>>(), Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenMultiRecordBatchFailsWithPayloadRejected_ExecutesSingleRecordFallback_PrunesSuccessesAndMarksFailureOnlyForPoisonRecord()
    {
        var good1 = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{\"name\":\"Good1\"}", DateTime.UtcNow);
        var poison = SyncOutbox.Create("Beneficiary", "ID-POISON", "Insert", "{\"name\":\"Poison\"}", DateTime.UtcNow);
        var good2 = SyncOutbox.Create("Beneficiary", "ID-2", "Insert", "{\"name\":\"Good2\"}", DateTime.UtcNow);

        var records = new List<SyncOutbox> { good1, poison, good2 };

        _outboxRepo.GetPendingBatchAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(records);

        // Entire batch fails with PayloadRejected
        _pushService.PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 3), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Failure(PushFailureKind.PayloadRejected, "422 Unprocessable Entity"));

        // Single-record fallback: good1 succeeds, poison fails, good2 succeeds
        _pushService.PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 1 && b[0].EntityId == "ID-1"), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Success());
        _pushService.PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 1 && b[0].EntityId == "ID-POISON"), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Failure(PushFailureKind.PayloadRejected, "Malformed format"));
        _pushService.PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 1 && b[0].EntityId == "ID-2"), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Success());

        var worker = new OutboxProcessorWorker(
            _uowFactory,
            _pushService,
            _optionsMonitor,
            NullLogger<OutboxProcessorWorker>.Instance);

        await worker.ProcessBatchAsync(_optionsMonitor.CurrentValue, CancellationToken.None);

        // Successful records should be pruned from local outbox
        await _outboxRepo.Received(1).DeleteRangeAsync(
            Arg.Is<IEnumerable<SyncOutbox>>(deleted =>
                deleted.Count() == 2 &&
                deleted.Contains(good1) &&
                deleted.Contains(good2)),
            Arg.Any<CancellationToken>());

        // Poison record should NOT be deleted, but should have its failure counter incremented
        poison.RetryCount.ShouldBe(1);
        poison.LastError.ShouldNotBeNull();
        poison.LastError.ShouldContain("rejected");

        // The good records should not have been marked as failed
        good1.RetryCount.ShouldBe(0);
        good2.RetryCount.ShouldBe(0);

        // Changes must be saved
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenFallbackEncountersNetworkOrTimeout_TripsCircuitBreakerImmediately()
    {
        var r1 = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{\"name\":\"1\"}", DateTime.UtcNow);
        var r2 = SyncOutbox.Create("Beneficiary", "ID-2", "Insert", "{\"name\":\"2\"}", DateTime.UtcNow);
        var r3 = SyncOutbox.Create("Beneficiary", "ID-3", "Insert", "{\"name\":\"3\"}", DateTime.UtcNow);

        var records = new List<SyncOutbox> { r1, r2, r3 };

        _outboxRepo.GetPendingBatchAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(records);

        _pushService.PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 3), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Failure(PushFailureKind.ServerError, "500 Server Error"));

        // First item in fallback encounters NetworkOrTimeout -> circuit breaker trips!
        _pushService.PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 1 && b[0].EntityId == "ID-1"), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Failure(PushFailureKind.NetworkOrTimeout, "Timeout"));

        var worker = new OutboxProcessorWorker(
            _uowFactory,
            _pushService,
            _optionsMonitor,
            NullLogger<OutboxProcessorWorker>.Instance);

        await worker.ProcessBatchAsync(_optionsMonitor.CurrentValue, CancellationToken.None);

        // Push should have been called only twice: once for batch, once for r1 (then circuit breaker tripped)
        await _pushService.Received(2).PushBatchAsync(Arg.Any<IReadOnlyList<SyncOutbox>>(), Arg.Any<CancellationToken>());
        await _pushService.DidNotReceive().PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 1 && b[0].EntityId == "ID-2"), Arg.Any<CancellationToken>());
        await _pushService.DidNotReceive().PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 1 && b[0].EntityId == "ID-3"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenFallbackEncounters3ConsecutiveServerErrors_TripsCircuitBreaker()
    {
        var r1 = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", "{\"name\":\"1\"}", DateTime.UtcNow);
        var r2 = SyncOutbox.Create("Beneficiary", "ID-2", "Insert", "{\"name\":\"2\"}", DateTime.UtcNow);
        var r3 = SyncOutbox.Create("Beneficiary", "ID-3", "Insert", "{\"name\":\"3\"}", DateTime.UtcNow);
        var r4 = SyncOutbox.Create("Beneficiary", "ID-4", "Insert", "{\"name\":\"4\"}", DateTime.UtcNow);

        var records = new List<SyncOutbox> { r1, r2, r3, r4 };

        _outboxRepo.GetPendingBatchAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(records);

        _pushService.PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 4), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Failure(PushFailureKind.ServerError, "500 Server Error"));

        _pushService.PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 1), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Failure(PushFailureKind.ServerError, "500 Server Error"));

        var worker = new OutboxProcessorWorker(
            _uowFactory,
            _pushService,
            _optionsMonitor,
            NullLogger<OutboxProcessorWorker>.Instance);

        await worker.ProcessBatchAsync(_optionsMonitor.CurrentValue, CancellationToken.None);

        // 1 batch + 3 fallback calls = 4 total calls (r4 should never be called)
        await _pushService.Received(4).PushBatchAsync(Arg.Any<IReadOnlyList<SyncOutbox>>(), Arg.Any<CancellationToken>());
        await _pushService.DidNotReceive().PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 1 && b[0].EntityId == "ID-4"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessBatchAsync_WhenSingleRecordBatchFails_MarksFailureDirectlyWithoutFallbackLoop()
    {
        var single = SyncOutbox.Create("Beneficiary", "ID-SINGLE", "Insert", "{\"name\":\"Single\"}", DateTime.UtcNow);
        var records = new List<SyncOutbox> { single };

        _outboxRepo.GetPendingBatchAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(records);

        _pushService.PushBatchAsync(Arg.Is<IReadOnlyList<SyncOutbox>>(b => b.Count == 1), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Failure(PushFailureKind.PayloadRejected, "Rejected"));

        var worker = new OutboxProcessorWorker(
            _uowFactory,
            _pushService,
            _optionsMonitor,
            NullLogger<OutboxProcessorWorker>.Instance);

        await worker.ProcessBatchAsync(_optionsMonitor.CurrentValue, CancellationToken.None);

        // Should NOT call DeleteRangeAsync
        await _outboxRepo.DidNotReceive().DeleteRangeAsync(Arg.Any<IEnumerable<SyncOutbox>>(), Arg.Any<CancellationToken>());

        // Single record should have failure recorded
        single.RetryCount.ShouldBe(1);
        single.LastError.ShouldNotBeNull();

        // PushBatchAsync should have been called only once (no fallback for batch of 1)
        await _pushService.Received(1).PushBatchAsync(Arg.Any<IReadOnlyList<SyncOutbox>>(), Arg.Any<CancellationToken>());

        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessBatchAsync_AfterServerErrorThenSuccess_SendsTheStoredPayloadUnchangedBothTimes()
    {
        const string storedPayload = "{\"Id\":\"A\",\"FullName\":\"سالم\",\"Status\":1}";
        var record = SyncOutbox.Create("Beneficiary", "ID-1", "Insert", storedPayload, DateTime.UtcNow);

        _outboxRepo.GetPendingBatchAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<SyncOutbox> { record });

        var sentPayloads = new List<string>();
        _pushService.PushBatchAsync(Arg.Do<IReadOnlyList<SyncOutbox>>(b => sentPayloads.Add(b[0].PayloadJson)), Arg.Any<CancellationToken>())
            .Returns(
                PushBatchResult.Failure(PushFailureKind.ServerError, "500 Server Error"),
                PushBatchResult.Success());

        var worker = new OutboxProcessorWorker(
            _uowFactory,
            _pushService,
            _optionsMonitor,
            NullLogger<OutboxProcessorWorker>.Instance);

        await worker.ProcessBatchAsync(_optionsMonitor.CurrentValue, CancellationToken.None);
        var storedAfterFailure = record.PayloadJson;
        await worker.ProcessBatchAsync(_optionsMonitor.CurrentValue, CancellationToken.None);

        storedAfterFailure.ShouldBe(storedPayload);
        sentPayloads.Count.ShouldBe(2);
        sentPayloads.ShouldAllBe(p => p == storedPayload);
        record.RetryCount.ShouldBe(1);
    }
}
