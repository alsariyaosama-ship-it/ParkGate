using System.Reflection;
using System.Windows.Forms;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Controls;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>R5.4: the companion counters work from the keyboard.</summary>
public sealed class CounterKeyboardTests
{
    [Theory]
    [InlineData(Keys.Add, 3)]
    [InlineData(Keys.Oemplus, 3)]
    [InlineData(Keys.Oemplus | Keys.Shift, 3)]
    [InlineData(Keys.Subtract, 1)]
    [InlineData(Keys.OemMinus, 1)]
    public void Counter_PlusAndMinus_ChangeTheValueAndTypeNothing(Keys key, int expected)
    {
        using var counter = new CounterInput { Value = 2 };

        var e = PressKey(counter, key);

        counter.Value.ShouldBe(expected);
        e.SuppressKeyPress.ShouldBeTrue();
    }

    [Fact]
    public void Counter_Enter_RaisesConfirm()
    {
        using var counter = new CounterInput();
        var confirmed = 0;
        counter.ConfirmRequested += (_, _) => confirmed++;

        PressKey(counter, Keys.Enter).SuppressKeyPress.ShouldBeTrue();

        confirmed.ShouldBe(1);
    }

    [Fact]
    public void Desk_CounterTitles_ShowTheirShortcut()
    {
        using var desk = DeskWithCounters();

        var counters = Counters(desk);
        counters[0].Title.ShouldBe("بالغ (Alt+1):");
        counters[1].Title.ShouldBe("طفل (Alt+2):");
    }

    [Theory]
    [InlineData(Keys.Alt | Keys.D1, true)]
    [InlineData(Keys.Alt | Keys.NumPad2, true)]
    [InlineData(Keys.Alt | Keys.D3, false)] // only two counters
    public void Desk_AltNumber_GoesToThatCounter(Keys keys, bool handled)
    {
        using var desk = DeskWithCounters();

        ProcessCmdKey(desk, keys).ShouldBe(handled);
    }

    [Fact]
    public void Desk_EnterInACounter_RecordsTheEntry()
    {
        var checkIn = new RecordingCheckInService();
        using var desk = DeskWithCounters(checkIn);
        desk.AutoResetDelayMs = -1;
        desk.ShowHolderInfo(Holder(), Eligible());
        var counters = Counters(desk);
        counters[1].Value = 2;

        PressKey(counters[1], Keys.Enter);

        checkIn.LastRequest.ShouldNotBeNull();
        checkIn.LastRequest.Companions!.Single().Count.ShouldBe(2);
    }

    [Fact]
    public void Desk_EnterInACounter_WithoutAVisitor_DoesNothing()
    {
        var checkIn = new RecordingCheckInService();
        using var desk = DeskWithCounters(checkIn);

        PressKey(Counters(desk)[0], Keys.Enter);

        checkIn.LastRequest.ShouldBeNull();
    }

    private static CheckInDeskControl DeskWithCounters(params object[] services)
    {
        var desk = TestUi.Create<CheckInDeskControl>(services);
        desk.PopulateAgeCategories(
        [
            new AgeCategoryDto(1, "بالغ", null, 1, true),
            new AgeCategoryDto(2, "طفل", null, 2, true),
        ]);
        return desk;
    }

    private static List<CounterInput> Counters(CheckInDeskControl desk) =>
        (List<CounterInput>)typeof(CheckInDeskControl).GetField("_counterInputs", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(desk)!;

    private static KeyEventArgs PressKey(CounterInput counter, Keys keys)
    {
        var e = new KeyEventArgs(keys);
        typeof(CounterInput).GetMethod("OnTextBoxKeyDown", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(counter, [null, e]);
        return e;
    }

    private static bool ProcessCmdKey(CheckInDeskControl desk, Keys keys) =>
        (bool)typeof(CheckInDeskControl).GetMethod("ProcessCmdKey", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(desk, [new Message(), keys])!;

    private static BeneficiaryDto Holder() => new(
        Guid.NewGuid(), 1, "صندوق التضامن", "100200", "محمد أحمد علي", "0912345678", null, null,
        BeneficiaryStatus.Active, null, null, null, DateTime.UtcNow);

    private static EligibilityPreviewDto Eligible() => new(
        true, EntryResult.Allowed, null, string.Empty, false, 720, null, null, null);

    private sealed class RecordingCheckInService : ParkGate.Application.CheckIn.Services.ICheckInService
    {
        public RecordEntryRequest? LastRequest { get; private set; }

        public Task<ParkGate.Application.Common.Result<CheckInResultDto>> RecordEntryAsync(RecordEntryRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(ParkGate.Application.Common.Result.Success(new CheckInResultDto(
                Guid.NewGuid(), EntryResult.Allowed, null, null, DateTime.UtcNow, DateOnly.FromDateTime(DateTime.UtcNow),
                1 + (request.Companions?.Sum(c => c.Count) ?? 0), 720, DateTime.UtcNow.AddHours(12), false, [])));
        }

        public Task<ParkGate.Application.Common.Result<CheckInResultDto>> OverrideEntryAsync(OverrideEntryRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<ParkGate.Application.Common.Result> VoidEntryAsync(VoidEntryRequest request, string? supervisorUserId = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<ParkGate.Application.Common.Result<TodayDeskSummaryDto>> GetTodayDeskEntriesAsync(int? workstationId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(ParkGate.Application.Common.Result.Success(new TodayDeskSummaryDto(
                DateOnly.FromDateTime(DateTime.UtcNow), 1, "Desk 1", 0, 0, 0, 0, [], [])));
    }
}
