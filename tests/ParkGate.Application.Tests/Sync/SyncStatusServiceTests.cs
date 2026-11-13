using NSubstitute;
using ParkGate.Application.Common;
using ParkGate.Application.Security;
using ParkGate.Application.Sync;
using ParkGate.Application.Tests.Common;
using Shouldly;

namespace ParkGate.Application.Tests.Sync;

public sealed class SyncStatusServiceTests
{
    private readonly FakeAppSettingRepository _settings = new();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private SyncStatusService CreateService() => new(_settings, _currentUser);

    [Fact]
    public async Task GetAsync_WithoutPermission_ReturnsForbidden()
    {
        _currentUser.Has(Permission.ManageSettings).Returns(false);

        var result = await CreateService().GetAsync();

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Errors.General.Forbidden);
    }

    [Fact]
    public async Task GetAsync_WhenAgentNeverReported_ReturnsZerosAndNoHeartbeat()
    {
        _currentUser.Has(Permission.ManageSettings).Returns(true);

        var result = await CreateService().GetAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(new SyncStatusDto(0, 0, null));
    }

    [Fact]
    public async Task GetAsync_ReadsCountsAndHeartbeatWrittenByTheAgent()
    {
        _currentUser.Has(Permission.ManageSettings).Returns(true);
        var beat = new DateTime(2026, 11, 13, 10, 30, 0, DateTimeKind.Utc);
        await _settings.SetValueAsync(SyncSettingKeys.PendingCount, "7");
        await _settings.SetValueAsync(SyncSettingKeys.FailedCount, "2");
        await _settings.SetValueAsync(SyncSettingKeys.LastHeartbeatUtc, beat.ToString("O"));

        var result = await CreateService().GetAsync();

        result.Value.ShouldBe(new SyncStatusDto(7, 2, beat));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-3")]
    [InlineData("")]
    public async Task GetAsync_WithUnreadableCount_TreatsItAsZero(string raw)
    {
        _currentUser.Has(Permission.ManageSettings).Returns(true);
        await _settings.SetValueAsync(SyncSettingKeys.FailedCount, raw);

        var result = await CreateService().GetAsync();

        result.Value.FailedCount.ShouldBe(0);
    }
}
