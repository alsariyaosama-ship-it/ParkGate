using System.Reflection;
using System.Windows.Forms;
using NSubstitute;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Services;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>R5.7: distinct success and denial sounds, each with its own setting.</summary>
public sealed class DeskSoundsTests
{
    [Fact]
    public void ToneWave_IsAPlayableMonoPcmWave()
    {
        var wave = ToneWave.Create((880, 100), (1320, 120));

        System.Text.Encoding.ASCII.GetString(wave, 0, 4).ShouldBe("RIFF");
        System.Text.Encoding.ASCII.GetString(wave, 8, 4).ShouldBe("WAVE");
        BitConverter.ToInt16(wave, 20).ShouldBe((short)1);  // PCM
        BitConverter.ToInt16(wave, 22).ShouldBe((short)1);  // mono
        var dataBytes = BitConverter.ToInt32(wave, 40);
        dataBytes.ShouldBe(ToneWave.SampleRate * 220 / 1000 * 2);
        wave.Length.ShouldBe(44 + dataBytes);
        BitConverter.ToInt32(wave, 4).ShouldBe(wave.Length - 8);
    }

    [Fact]
    public void ToneWave_StartsAndEndsSilent_SoItDoesNotClick()
    {
        var wave = ToneWave.Create((440, 50));

        BitConverter.ToInt16(wave, 44).ShouldBe((short)0);
        Math.Abs((int)BitConverter.ToInt16(wave, wave.Length - 2)).ShouldBeLessThan(200);
    }

    [Fact]
    public void DeskSounds_TheTwoSoundsDiffer()
    {
        DeskSounds.GrantedWave.SequenceEqual(DeskSounds.DeniedWave).ShouldBeFalse();
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public async Task Desk_GrantedEntry_PlaysTheSuccessSoundWhenOn(bool enabled, int plays)
    {
        var sounds = Substitute.For<IDeskSounds>();
        using var desk = TestUi.Create<CheckInDeskControl>(sounds, new FakeCheckIn());
        SetField(desk, "_successSoundEnabled", enabled);
        desk.AutoResetDelayMs = -1;
        desk.PopulateAgeCategories([new AgeCategoryDto(1, "بالغ", null, 1, true)]);
        desk.ShowHolderInfo(Holder(), new EligibilityPreviewDto(true, EntryResult.Allowed, null, string.Empty, false, 720, null, null, null));

        await desk.RecordEntryAsync();

        sounds.Received(plays).PlayGranted();
        sounds.DidNotReceive().PlayDenied();
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void Desk_Denial_PlaysTheDenialSoundWhenOn(bool enabled, int plays)
    {
        var sounds = Substitute.For<IDeskSounds>();
        using var desk = TestUi.Create<CheckInDeskControl>(sounds);
        SetField(desk, "_denialSoundEnabled", enabled);

        desk.ShowHolderInfo(Holder(), new EligibilityPreviewDto(
            false, EntryResult.Denied, DenialReason.CooldownActive, string.Empty, true, 720,
            DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(11), 660));

        sounds.Received(plays).PlayDenied();
        sounds.DidNotReceive().PlayGranted();
    }

    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(target, value);

    private static BeneficiaryDto Holder() => new(
        Guid.NewGuid(), 1, "صندوق التضامن", "100200", "محمد أحمد علي", "0912345678", null, null,
        BeneficiaryStatus.Active, null, null, null, DateTime.UtcNow);

    private sealed class FakeCheckIn : ParkGate.Application.CheckIn.Services.ICheckInService
    {
        public Task<ParkGate.Application.Common.Result<CheckInResultDto>> RecordEntryAsync(RecordEntryRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(ParkGate.Application.Common.Result.Success(new CheckInResultDto(
                Guid.NewGuid(), EntryResult.Allowed, null, null, DateTime.UtcNow, DateOnly.FromDateTime(DateTime.UtcNow),
                1, 720, DateTime.UtcNow.AddHours(12), false, [])));

        public Task<ParkGate.Application.Common.Result<CheckInResultDto>> OverrideEntryAsync(OverrideEntryRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<ParkGate.Application.Common.Result> VoidEntryAsync(VoidEntryRequest request, string? supervisorUserId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<ParkGate.Application.Common.Result<TodayDeskSummaryDto>> GetTodayDeskEntriesAsync(int? workstationId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(ParkGate.Application.Common.Result.Success(new TodayDeskSummaryDto(
                DateOnly.FromDateTime(DateTime.UtcNow), 1, "Desk 1", 0, 0, 0, 0, [], [])));
    }
}
