using System.Reflection;
using NSubstitute;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.CheckIn.Services;
using ParkGate.Application.Common;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Models;
using ParkGate.Application.Settings.Services;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Printing;
using ParkGate.WinForms.Resources;

namespace ParkGate.ArchitectureTests;

/// <summary>
/// T10.2 / T10.3, BR-33: the desk prints the entry receipt after a granted entry (record and override), only when the
/// setting is on, and never stops the desk when printing fails. Every fake answers synchronously, so no awaited
/// continuation leaves the test thread.
/// </summary>
public sealed class EntryReceiptDeskTests
{
    private static readonly DateTime EnteredAtUtc = new(2026, 11, 22, 13, 5, 0, DateTimeKind.Utc); // 15:05 park time (UTC+2)

    private static BeneficiaryDto Holder() => new(
        Guid.NewGuid(), 1, "صندوق التضامن", "100200", "محمد أحمد علي", "0912345678", null, null,
        BeneficiaryStatus.Active, null, null, null, DateTime.UtcNow);

    private static ICurrentWorkstation Workstation()
    {
        var workstation = Substitute.For<ICurrentWorkstation>();
        workstation.DisplayName.Returns("شباك 1");
        workstation.MachineName.Returns("DESK-01");
        return workstation;
    }

    private static CheckInResultDto Granted(EntryResult result) => new(
        Guid.NewGuid(), result, null, null, EnteredAtUtc, new DateOnly(2026, 11, 22),
        TotalPersons: 4, EffectiveCooldownMinutes: 720, NextEligibleAtUtc: EnteredAtUtc.AddHours(12), CanBeOverridden: false,
        Companions: [new(1, "بالغ", 1), new(2, "طفل", 1), new(3, "رضيع", 1)],
        ReceiptNo: 57, CountedPersons: 3);

    private static CheckInDeskControl Desk(FakeEntryReceiptPrinter printer, EntryResult result = EntryResult.Allowed)
    {
        var desk = TestUi.Create<CheckInDeskControl>(printer, Workstation(), new FakeCheckIn(Granted(result)));
        desk.AutoResetDelayMs = -1;
        desk.PopulateAgeCategories([new AgeCategoryDto(1, "بالغ", null, 1, true)]);
        desk.ShowHolderInfo(Holder(), new EligibilityPreviewDto(true, EntryResult.Allowed, null, string.Empty, false, 720, null, null, null));
        return desk;
    }

    private static string BannerDetails(CheckInDeskControl desk) =>
        GetField<StatusBanner>(desk, "_statusBanner").Details ?? string.Empty;

    private static T GetField<T>(object target, string name) where T : class =>
        (T)target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(target)!;

    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(target, value);

    [Fact]
    public async Task Desk_GrantedEntry_PrintsOneReceiptWithTheCountedTotal()
    {
        var printer = new FakeEntryReceiptPrinter();
        using var desk = Desk(printer);

        await desk.RecordEntryAsync();

        var texts = printer.Slips.ShouldHaveSingleItem().Items.OfType<SlipText>().Select(t => t.Text).ToList();
        texts.ShouldContain("عدد الأشخاص: 3");
        texts.ShouldContain("صاحب البطاقة + مرافقان");
        texts.ShouldContain("المرافقون: بالغ 1 · طفل 1 · رضيع 1");
        texts.ShouldContain("22/11/2026  03:05 م");
        texts.ShouldContain("شباك 1   واصل رقم 57");
        texts.ShouldContain("صندوق التضامن");
        BannerDetails(desk).ShouldNotContain(Strings.Banner_ReceiptPrintFailed);
    }

    [Fact]
    public async Task Desk_Override_PrintsTheReceiptToo()
    {
        var printer = new FakeEntryReceiptPrinter();
        using var desk = Desk(printer, EntryResult.Overridden);
        desk.AskSupervisorOverride = () => ("token", "المشرف", "حالة خاصة جدا");

        await desk.OverrideEntryAsync();

        var texts = printer.Slips.ShouldHaveSingleItem().Items.OfType<SlipText>().Select(t => t.Text).ToList();
        texts.ShouldContain("عدد الأشخاص: 3");
        texts.ShouldContain("شباك 1   واصل رقم 57");
    }

    [Fact]
    public async Task Desk_Override_Cancelled_PrintsNothing()
    {
        var printer = new FakeEntryReceiptPrinter();
        using var desk = Desk(printer, EntryResult.Overridden);
        desk.AskSupervisorOverride = () => null;

        await desk.OverrideEntryAsync();

        printer.Slips.ShouldBeEmpty();
    }

    [Fact]
    public async Task Desk_WhenTheSettingIsOff_PrintsNothing()
    {
        var printer = new FakeEntryReceiptPrinter();
        using var desk = Desk(printer);
        SetField(desk, "_printEntryReceipt", false);

        await desk.RecordEntryAsync();

        printer.Slips.ShouldBeEmpty();
        GetField<StatusBanner>(desk, "_statusBanner").State.ShouldBe(BannerState.Granted);
        BannerDetails(desk).ShouldNotContain(Strings.Banner_ReceiptPrintFailed);
    }

    [Fact]
    public async Task Desk_LoadsThePrintSettingWithTheOtherSettings()
    {
        var settings = Substitute.For<ISettingsService>();
        settings.GetSettingsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new AppSettingsModel(
            720, 10, 5, true, true, 5, 15, 0, "park", "Libya Standard Time", SuccessSoundEnabled: true, PrintEntryReceipt: false)));
        var printer = new FakeEntryReceiptPrinter();
        using var desk = TestUi.Create<CheckInDeskControl>(printer, Workstation(), settings, new FakeCheckIn(Granted(EntryResult.Allowed)));
        desk.AutoResetDelayMs = -1;
        desk.PopulateAgeCategories([new AgeCategoryDto(1, "بالغ", null, 1, true)]);

        await (Task)typeof(CheckInDeskControl).GetMethod("LoadSettingsAsync", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(desk, null)!;
        desk.ShowHolderInfo(Holder(), new EligibilityPreviewDto(true, EntryResult.Allowed, null, string.Empty, false, 720, null, null, null));
        await desk.RecordEntryAsync();

        printer.Slips.ShouldBeEmpty();
    }

    [Fact]
    public async Task Desk_WhenPrintingFails_TheEntryStandsAndTheBannerSaysSo()
    {
        var printer = new FakeEntryReceiptPrinter { Succeeds = false };
        using var desk = Desk(printer);

        await desk.RecordEntryAsync();

        printer.Slips.Count.ShouldBe(1);
        var banner = GetField<StatusBanner>(desk, "_statusBanner");
        banner.State.ShouldBe(BannerState.Granted);
        BannerDetails(desk).ShouldContain(Strings.Banner_ReceiptPrintFailed);
    }

    [Fact]
    public async Task Desk_WhenPrintingFailsOnOverride_TheBannerSaysSo()
    {
        var printer = new FakeEntryReceiptPrinter { Succeeds = false };
        using var desk = Desk(printer, EntryResult.Overridden);
        desk.AskSupervisorOverride = () => ("token", "المشرف", "حالة خاصة جدا");

        await desk.OverrideEntryAsync();

        BannerDetails(desk).ShouldContain(Strings.Banner_ReceiptPrintFailed);
    }

    private sealed class FakeCheckIn(CheckInResultDto granted) : ICheckInService
    {
        public Task<Result<CheckInResultDto>> RecordEntryAsync(RecordEntryRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success(granted));

        public Task<Result<CheckInResultDto>> OverrideEntryAsync(OverrideEntryRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success(granted));

        public Task<Result> VoidEntryAsync(VoidEntryRequest request, string? supervisorUserId = null, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Result<TodayDeskSummaryDto>> GetTodayDeskEntriesAsync(int? workstationId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success(new TodayDeskSummaryDto(
                DateOnly.FromDateTime(DateTime.UtcNow), 1, "Desk 1", 0, 0, 0, 0, [], [])));
    }
}
