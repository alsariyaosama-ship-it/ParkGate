using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Settings;
using ParkGate.Application.Ports.Sync;
using ParkGate.Application.Sync;
using ParkGate.SyncAgent.Workers;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Sync;

public sealed class HeartbeatWorkerTests
{
    private readonly IUnitOfWorkFactory _uowFactory = Substitute.For<IUnitOfWorkFactory>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ISyncOutboxRepository _outboxRepo = Substitute.For<ISyncOutboxRepository>();
    private readonly IAppSettingsStore _settings = Substitute.For<IAppSettingsStore>();
    private readonly ICloudPushService _pushService = Substitute.For<ICloudPushService>();

    public HeartbeatWorkerTests()
    {
        _uow.SyncOutboxes.Returns(_outboxRepo);
        _uowFactory.Create().Returns(_uow);
        _outboxRepo.GetPendingAndFailedCountsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((7, 2));
        _pushService.SendHeartbeatAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Success());
    }

    private HeartbeatWorker CreateWorker() =>
        new(_settings, _uowFactory, _pushService, NullLogger<HeartbeatWorker>.Instance);

    [Fact]
    public async Task RecordHeartbeatAsync_WritesHeartbeatAndCountsToSettingsForTheDesk()
    {
        await CreateWorker().RecordHeartbeatAsync(new SyncOptions { MaxRetries = 10 }, CancellationToken.None);

        await _settings.Received(1).SetValueAsync(SyncSettingKeys.LastHeartbeatUtc, Arg.Any<string>(), null, Arg.Any<CancellationToken>());
        await _settings.Received(1).SetValueAsync(SyncSettingKeys.PendingCount, "7", null, Arg.Any<CancellationToken>());
        await _settings.Received(1).SetValueAsync(SyncSettingKeys.FailedCount, "2", null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordHeartbeatAsync_CountsRowsAgainstTheConfiguredRetryLimit()
    {
        await CreateWorker().RecordHeartbeatAsync(new SyncOptions { MaxRetries = 4 }, CancellationToken.None);

        await _outboxRepo.Received(1).GetPendingAndFailedCountsAsync(4, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordHeartbeatAsync_WhenSyncEnabled_SendsCountsToTheCloud()
    {
        await CreateWorker().RecordHeartbeatAsync(new SyncOptions { Enabled = true }, CancellationToken.None);

        await _pushService.Received(1).SendHeartbeatAsync(7, 2, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordHeartbeatAsync_WhenSyncDisabled_DoesNotCallTheCloud()
    {
        await CreateWorker().RecordHeartbeatAsync(new SyncOptions { Enabled = false }, CancellationToken.None);

        await _pushService.DidNotReceive().SendHeartbeatAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _settings.Received().SetValueAsync(SyncSettingKeys.FailedCount, "2", null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordHeartbeatAsync_WhenCloudIsUnreachable_StillRecordsLocallyAndDoesNotThrow()
    {
        _pushService.SendHeartbeatAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(PushBatchResult.Failure(PushFailureKind.NetworkOrTimeout, "down"));

        await Should.NotThrowAsync(() => CreateWorker().RecordHeartbeatAsync(new SyncOptions(), CancellationToken.None));

        await _settings.Received(1).SetValueAsync(SyncSettingKeys.PendingCount, "7", null, Arg.Any<CancellationToken>());
    }
}
