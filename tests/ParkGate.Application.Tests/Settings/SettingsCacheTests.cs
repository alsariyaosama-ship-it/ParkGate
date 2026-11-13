using ParkGate.Application.Security;
using ParkGate.Application.Settings.Services;
using ParkGate.Application.Settings.Validators;
using ParkGate.Application.Tests.Common;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.Tests.Settings;

/// <summary>
/// R3.6: check-in reads the cached settings. Every desk has its own cache, so a change saved on another desk
/// must reach this one within a short lifetime instead of waiting for a restart.
/// </summary>
public sealed class SettingsCacheTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 14, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeAppSettingRepository _store = new();
    private readonly ManualTimeProvider _clock = new(Start);
    private readonly SettingsService _sut;

    public SettingsCacheTests()
    {
        _store.Add(new AppSetting("Entry.MaxGroupSize", "10", Start.UtcDateTime, null));
        _sut = new SettingsService(
            _store, new FakeAuditWriter(), new CurrentUserContext(), new CurrentWorkstationContext("DESK-B"),
            new UpdateSettingsRequestValidator(), _clock);
    }

    [Fact]
    public async Task GetSettingsAsync_WithinCacheLifetime_DoesNotRereadTheStore()
    {
        (await _sut.GetSettingsAsync()).MaxGroupSize.ShouldBe(10);
        ChangeStoredGroupSize("4");

        _clock.Now = Start.AddSeconds(59);

        (await _sut.GetSettingsAsync()).MaxGroupSize.ShouldBe(10);
    }

    [Fact]
    public async Task GetSettingsAsync_AfterCacheLifetime_SeesAChangeSavedOnAnotherDesk()
    {
        (await _sut.GetSettingsAsync()).MaxGroupSize.ShouldBe(10);
        ChangeStoredGroupSize("4");

        _clock.Now = Start.AddSeconds(61);

        (await _sut.GetSettingsAsync()).MaxGroupSize.ShouldBe(4);
    }

    private void ChangeStoredGroupSize(string value) =>
        _store.Add(new AppSetting("Entry.MaxGroupSize", value, Start.UtcDateTime, null));
}
