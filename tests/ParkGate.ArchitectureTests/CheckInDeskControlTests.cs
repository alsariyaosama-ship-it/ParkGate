using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Resources;
using Shouldly;

namespace ParkGate.ArchitectureTests;

public sealed class CheckInDeskControlTests
{
    private static TControl GetField<TControl>(object target, string fieldName) where TControl : class
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.ShouldNotBeNull($"Field '{fieldName}' was not found on '{target.GetType().Name}'.");
        var val = field.GetValue(target) as TControl;
        val.ShouldNotBeNull($"Field '{fieldName}' could not be cast to '{typeof(TControl).Name}'.");
        return val;
    }

    [Fact]
    public void CheckInDeskControl_InitialState_ShouldHaveZonesConfiguredAndRtl()
    {
        using var control = TestUi.Create<CheckInDeskControl>();

        control.RightToLeft.ShouldBe(RightToLeft.Yes);

        var txtCard = GetField<TextBox>(control, "_txtCardNumber");
        var btnSearch = GetField<Button>(control, "_btnSearch");
        var btnClear = GetField<Button>(control, "_btnClear");
        var btnNewHolder = GetField<Button>(control, "_btnNewHolder");
        var cmbOrg = GetField<ComboBox>(control, "_cmbOrganization");
        var banner = GetField<StatusBanner>(control, "_statusBanner");
        var grpHolder = GetField<GroupBox>(control, "_grpHolder");
        var grpReg = GetField<InlineRegistrationPanel>(control, "_registrationPanel");
        var grpCompanions = GetField<GroupBox>(control, "_grpCompanions");
        var btnRecord = GetField<Button>(control, "_btnRecordEntry");
        var btnOverride = GetField<Button>(control, "_btnOverride");
        var grpToday = GetField<GroupBox>(GetField<TodayEntriesPanel>(control, "_todayPanel"), "_grpToday");
        var gridToday = GetField<DataGridView>(GetField<TodayEntriesPanel>(control, "_todayPanel"), "_gridToday");
        var lblTotalPersons = GetField<Label>(control, "_lblTotalPersons");

        txtCard.Text.ShouldBeEmpty();
        btnSearch.Text.ShouldBe(Strings.Btn_Search);
        btnClear.Text.ShouldBe(Strings.Btn_Clear);
        btnNewHolder.Text.ShouldBe(Strings.Btn_NewHolder);
        btnRecord.Text.ShouldBe(Strings.Btn_RecordEntry);
        btnOverride.Text.ShouldBe(Strings.Btn_Override);

        btnRecord.Enabled.ShouldBeFalse();
        btnOverride.Enabled.ShouldBeFalse();

        grpHolder.Visible.ShouldBeTrue();
        grpReg.Visible.ShouldBeFalse();

        gridToday.ReadOnly.ShouldBeTrue();
        gridToday.Columns.Count.ShouldBe(6);
        gridToday.Columns[0].HeaderText.ShouldBe(Strings.Col_Time);
        gridToday.Columns[1].HeaderText.ShouldBe(Strings.Col_CardNumber);
        gridToday.Columns[2].HeaderText.ShouldBe(Strings.Col_HolderName);
        gridToday.Columns[3].HeaderText.ShouldBe(Strings.Col_Org);
        gridToday.Columns[4].HeaderText.ShouldBe(Strings.Col_Persons);
        gridToday.Columns[5].HeaderText.ShouldBe(Strings.Col_Result);

        lblTotalPersons.Text.ShouldContain("1"); // 1 person initially (holder alone)
    }

    [Fact]
    public void CheckInDeskControl_ShowHolderInfo_WhenEligible_ShouldEnableRecordEntry()
    {
        using var control = TestUi.Create<CheckInDeskControl>();

        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "100200",
            FullName: "محمد أحمد علي",
            Phone: "0912345678",
            NationalId: "119900123456",
            CardExpiryDate: new DateOnly(2027, 12, 31),
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: DateTime.UtcNow.AddDays(-2),
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow.AddDays(-100));

        var eligibility = new EligibilityPreviewDto(
            IsEligible: true,
            Result: EntryResult.Allowed,
            DenialReason: null,
            DenialReasonText: string.Empty,
            CanBeOverridden: false,
            EffectiveCooldownMinutes: 720,
            LastGrantedEntryUtc: holder.LastGrantedEntryUtc,
            NextAllowedEntryUtc: null,
            RemainingCooldownMinutes: null);

        control.ShowHolderInfo(holder, eligibility);

        var lblName = GetField<Label>(control, "_lblHolderName");
        var lblPhone = GetField<Label>(control, "_lblHolderPhone");
        var lblOrg = GetField<Label>(control, "_lblHolderOrg");
        var btnRecord = GetField<Button>(control, "_btnRecordEntry");
        var btnOverride = GetField<Button>(control, "_btnOverride");

        lblName.Text.ShouldBe("محمد أحمد علي");
        lblPhone.Text.ShouldBe("0912345678");
        lblOrg.Text.ShouldBe("صندوق التضامن");
        btnRecord.Enabled.ShouldBeTrue();
        btnOverride.Enabled.ShouldBeFalse();
    }

    [Theory]
    [InlineData(1024)]
    [InlineData(1366)]
    [InlineData(1920)]
    public void CheckInDeskControl_SearchRow_ControlsNeverOverlap(int width)
    {
        // R5.11: the row used absolute positions, and at 125 % the card box ran under its label.
        using var host = new Form { ClientSize = new Size(width, 700) };
        using var control = TestUi.Create<CheckInDeskControl>();
        control.Dock = DockStyle.Fill;
        host.Controls.Add(control);
        host.PerformLayout();

        var row = GetField<TableLayoutPanel>(control, "_tblSearch");
        GetField<Panel>(control, "_pnlSearch").Controls.Cast<Control>().ShouldBe([row]);

        Control[] inOrder =
        [
            GetField<Label>(control, "_lblCardNumber"),
            GetField<TextBox>(control, "_txtCardNumber"),
            GetField<Label>(control, "_lblOrganization"),
            GetField<ComboBox>(control, "_cmbOrganization"),
            GetField<Button>(control, "_btnSearch"),
        ];
        for (var i = 0; i < inOrder.Length; i++)
        {
            row.GetColumn(inOrder[i]).ShouldBe(i, inOrder[i].Name);
            for (var j = i + 1; j < inOrder.Length; j++)
            {
                inOrder[i].Bounds.IntersectsWith(inOrder[j].Bounds).ShouldBeFalse($"{inOrder[i].Name} / {inOrder[j].Name}");
            }
        }
    }

    [Fact]
    public void CheckInDeskControl_IdentityText_IsLargerThanBodyText()
    {
        // R5.3: the operator reads the name and the card number at a glance.
        using var control = TestUi.Create<CheckInDeskControl>();

        var name = GetField<Label>(control, "_lblHolderName").Font;
        var card = GetField<TextBox>(control, "_txtCardNumber").Font;

        name.SizeInPoints.ShouldBe(14F);
        name.Bold.ShouldBeTrue();
        card.SizeInPoints.ShouldBe(16F);
        card.Bold.ShouldBeTrue();
    }

    [Fact]
    public void CheckInDeskControl_ShowHolderInfo_WhenCooldownActive_ShouldDisableRecordEntryAndEnableOverride()
    {
        using var control = TestUi.Create<CheckInDeskControl>();

        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "100200",
            FullName: "محمد أحمد علي",
            Phone: "0912345678",
            NationalId: "119900123456",
            CardExpiryDate: new DateOnly(2027, 12, 31),
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: DateTime.UtcNow.AddHours(-2),
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow.AddDays(-100));

        var eligibility = new EligibilityPreviewDto(
            IsEligible: false,
            Result: EntryResult.Denied,
            DenialReason: DenialReason.CooldownActive,
            DenialReasonText: "فترة الانتظار مفعلة",
            CanBeOverridden: true,
            EffectiveCooldownMinutes: 720,
            LastGrantedEntryUtc: holder.LastGrantedEntryUtc,
            NextAllowedEntryUtc: DateTime.UtcNow.AddHours(10),
            RemainingCooldownMinutes: 600);

        control.ShowHolderInfo(holder, eligibility);

        var btnRecord = GetField<Button>(control, "_btnRecordEntry");
        var btnOverride = GetField<Button>(control, "_btnOverride");
        var banner = GetField<StatusBanner>(control, "_statusBanner");

        btnRecord.Enabled.ShouldBeFalse();
        btnOverride.Enabled.ShouldBeTrue();
        banner.State.ShouldBe(BannerState.Denied);
        banner.Details.ShouldNotBeNull();
        banner.Details.ShouldContain("10:00");
    }

    [Fact]
    public void CheckInDeskControl_ShowInlineRegistration_ShouldToggleVisibility()
    {
        using var control = TestUi.Create<CheckInDeskControl>();

        var grpHolder = GetField<GroupBox>(control, "_grpHolder");
        var grpReg = GetField<InlineRegistrationPanel>(control, "_registrationPanel");

        control.ShowInlineRegistration("999888", 1);
        grpHolder.Visible.ShouldBeFalse();
        grpReg.Visible.ShouldBeTrue();

        control.HideInlineRegistration();
        grpHolder.Visible.ShouldBeTrue();
        grpReg.Visible.ShouldBeFalse();
    }

    [Fact]
    public void CheckInDeskControl_ResetDesk_ShouldClearStateAndFocusCard()
    {
        using var control = TestUi.Create<CheckInDeskControl>();

        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "100200",
            FullName: "محمد أحمد علي",
            Phone: "0912345678",
            NationalId: null,
            CardExpiryDate: null,
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow);

        var eligibility = new EligibilityPreviewDto(
            IsEligible: true,
            Result: EntryResult.Allowed,
            DenialReason: null,
            DenialReasonText: string.Empty,
            CanBeOverridden: false,
            EffectiveCooldownMinutes: 720,
            LastGrantedEntryUtc: null,
            NextAllowedEntryUtc: null,
            RemainingCooldownMinutes: null);

        control.ShowHolderInfo(holder, eligibility);
        var btnRecord = GetField<Button>(control, "_btnRecordEntry");
        btnRecord.Enabled.ShouldBeTrue();

        control.ResetDesk();

        btnRecord.Enabled.ShouldBeFalse();
        var lblName = GetField<Label>(control, "_lblHolderName");
        lblName.Text.ShouldBe("-");
    }

    [Fact]
    public void CheckInDeskControl_PopulateAgeCategories_ShouldCreateCounterInputs()
    {
        using var control = TestUi.Create<CheckInDeskControl>();

        control.PopulateAgeCategories(
        [
            new AgeCategoryDto(1, "بالغ", "12 سنة فما فوق", 1, true),
            new AgeCategoryDto(2, "طفل", "من 3 إلى 12 سنة", 2, true)
        ]);

        var flowCounters = GetField<FlowLayoutPanel>(control, "_flowCounters");
        flowCounters.Controls.Count.ShouldBe(2);

        var firstCounter = flowCounters.Controls[0].ShouldBeOfType<CounterInput>();
        firstCounter.Title.ShouldBe("بالغ (Alt+1):");
        firstCounter.Value.ShouldBe(0);
    }

    [Fact]
    public void CheckInDeskControl_CompanionsExceedingMaxGroupSize_ShouldDisableRecordEntry()
    {
        using var control = TestUi.Create<CheckInDeskControl>();

        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "100200",
            FullName: "محمد أحمد علي",
            Phone: "0912345678",
            NationalId: null,
            CardExpiryDate: null,
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow);

        var eligibility = new EligibilityPreviewDto(
            IsEligible: true,
            Result: EntryResult.Allowed,
            DenialReason: null,
            DenialReasonText: string.Empty,
            CanBeOverridden: false,
            EffectiveCooldownMinutes: 720,
            LastGrantedEntryUtc: null,
            NextAllowedEntryUtc: null,
            RemainingCooldownMinutes: null);

        control.PopulateAgeCategories(
        [
            new AgeCategoryDto(1, "بالغ", "12 سنة فما فوق", 1, true),
            new AgeCategoryDto(2, "طفل", "من 3 إلى 12 سنة", 2, true)
        ]);

        control.ShowHolderInfo(holder, eligibility);
        var btnRecord = GetField<Button>(control, "_btnRecordEntry");
        btnRecord.Enabled.ShouldBeTrue();

        var counterInputs = GetField<List<CounterInput>>(control, "_counterInputs");
        // Total persons = 1 holder + 6 adults + 5 children = 12 > 10 (max group size)
        counterInputs[0].Value = 6;
        counterInputs[1].Value = 5;

        btnRecord.Enabled.ShouldBeFalse();
        var lblTotalPersons = GetField<Label>(control, "_lblTotalPersons");
        lblTotalPersons.ForeColor.ShouldBe(StatusBanner.ColorDenied);
    }

    [Fact]
    public void CheckInDeskControl_OperatorRole_ShowsTheIdMaskedByTheService()
    {
        // R6.16: Application masks the ID for operators; the desk shows exactly what it receives.
        var operatorUser = new FakeCurrentUser { Role = UserRole.Operator, UserId = 1, UserName = "op1" };
        using var control = TestUi.Create<CheckInDeskControl>(operatorUser);

        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "100200",
            FullName: "محمد أحمد علي",
            Phone: "0912345678",
            NationalId: "********3456",
            CardExpiryDate: null,
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow);

        control.ShowHolderInfo(holder, null);

        var lblNatId = GetField<Label>(control, "_lblHolderNatId");
        lblNatId.Text.ShouldBe("********3456");
    }

    [Fact]
    public void CheckInDeskControl_SupervisorRole_ShouldNotMaskNationalId()
    {
        var supervisorUser = new FakeCurrentUser { Role = UserRole.Supervisor, UserId = 2, UserName = "sup1" };
        using var control = TestUi.Create<CheckInDeskControl>(supervisorUser);

        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "100200",
            FullName: "محمد أحمد علي",
            Phone: "0912345678",
            NationalId: "119900123456",
            CardExpiryDate: null,
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow);

        control.ShowHolderInfo(holder, null);

        var lblNatId = GetField<Label>(control, "_lblHolderNatId");
        lblNatId.Text.ShouldBe("119900123456");
    }

    [Fact]
    public async Task CheckInDeskControl_LookupCardAsync_WhenCardNotFound_ShowsInlineRegistration()
    {
        var fakeBeneficiaryService = new FakeBeneficiaryService();
        using var control = TestUi.Create<CheckInDeskControl>(fakeBeneficiaryService);

        var txtCard = GetField<TextBox>(control, "_txtCardNumber");
        txtCard.Text = "998877";

        await control.LookupCardAsync();

        var grpHolder = GetField<GroupBox>(control, "_grpHolder");
        var grpReg = GetField<InlineRegistrationPanel>(control, "_registrationPanel");
        var banner = GetField<StatusBanner>(control, "_statusBanner");

        grpHolder.Visible.ShouldBeFalse();
        grpReg.Visible.ShouldBeTrue();
        banner.State.ShouldBe(BannerState.Warning);
        fakeBeneficiaryService.LastCardNumberLookup.ShouldBe("998877");
    }

    [Fact]
    public async Task CheckInDeskControl_LookupCardAsync_NormalizesArabicDigits()
    {
        var fakeBeneficiaryService = new FakeBeneficiaryService();
        using var control = TestUi.Create<CheckInDeskControl>(fakeBeneficiaryService);

        var txtCard = GetField<TextBox>(control, "_txtCardNumber");
        txtCard.Text = "١٢٣٤٥";

        await control.LookupCardAsync();

        fakeBeneficiaryService.LastCardNumberLookup.ShouldBe("12345");
        txtCard.Text.ShouldBe("12345");
    }

    [Fact]
    public async Task CheckInDeskControl_LookupCardAsync_WhenFound_ShowsHolderAndEligibility()
    {
        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "100200",
            FullName: "محمد أحمد علي",
            Phone: "0912345678",
            NationalId: "119900123456",
            CardExpiryDate: new DateOnly(2028, 1, 1),
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow);

        var eligibility = new EligibilityPreviewDto(
            IsEligible: true,
            Result: EntryResult.Allowed,
            DenialReason: null,
            DenialReasonText: string.Empty,
            CanBeOverridden: false,
            EffectiveCooldownMinutes: 720,
            LastGrantedEntryUtc: null,
            NextAllowedEntryUtc: null,
            RemainingCooldownMinutes: null);

        var fakeBeneficiaryService = new FakeBeneficiaryService
        {
            LookupReturnResult = [new BeneficiaryLookupResultDto(holder, eligibility, [])]
        };

        using var control = TestUi.Create<CheckInDeskControl>(fakeBeneficiaryService);

        var txtCard = GetField<TextBox>(control, "_txtCardNumber");
        txtCard.Text = "100200";

        await control.LookupCardAsync();

        var grpHolder = GetField<GroupBox>(control, "_grpHolder");
        var grpReg = GetField<InlineRegistrationPanel>(control, "_registrationPanel");
        var lblName = GetField<Label>(control, "_lblHolderName");
        var banner = GetField<StatusBanner>(control, "_statusBanner");
        var btnRecord = GetField<Button>(control, "_btnRecordEntry");

        grpHolder.Visible.ShouldBeTrue();
        grpReg.Visible.ShouldBeFalse();
        lblName.Text.ShouldBe("محمد أحمد علي");
        banner.State.ShouldBe(BannerState.Granted);
        btnRecord.Enabled.ShouldBeTrue();
        txtCard.SelectionLength.ShouldBe(txtCard.Text.Length);
    }

    [Theory]
    [InlineData(0)]   // the desk is freed for the next visitor
    [InlineData(-1)]  // the holder stays on screen
    public async Task CheckInDeskControl_AfterAGrantedEntry_OffersTheGiftWithoutASecondScan(int autoResetDelayMs)
    {
        // R5.14: BR-30 needs today's granted entry, so before the entry the gift is not offered; after it the
        // operator presses F6 for the same visitor instead of scanning the card again.
        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(), OrganizationId: 1, OrganizationName: "صندوق التضامن", CardNumber: "100200",
            FullName: "محمد أحمد علي", Phone: "0912345678", NationalId: null, CardExpiryDate: null,
            Status: BeneficiaryStatus.Active, StatusReason: null, LastGrantedEntryUtc: null, Notes: null,
            CreatedAtUtc: DateTime.UtcNow);
        var eligibility = new EligibilityPreviewDto(
            IsEligible: true, Result: EntryResult.Allowed, DenialReason: null, DenialReasonText: string.Empty,
            CanBeOverridden: false, EffectiveCooldownMinutes: 720, LastGrantedEntryUtc: null,
            NextAllowedEntryUtc: null, RemainingCooldownMinutes: null);
        var gifts = new CountingGiftService();
        using var control = TestUi.Create<CheckInDeskControl>(new FakeCheckInService(), gifts);
        control.AutoResetDelayMs = autoResetDelayMs;
        control.ShowHolderInfo(holder, eligibility);
        gifts.EntryRecorded = true;

        await control.RecordEntryAsync();

        gifts.LastBeneficiaryId.ShouldBe(holder.Id);
        var panel = GetField<GiftOfferPanel>(control, "_giftPanel");
        GetField<Button>(panel, "_btnIssue").Enabled.ShouldBeTrue();
        GetField<Label>(panel, "_lblDetails").Text.ShouldContain(holder.FullName);
    }

    [Fact]
    public async Task GiftOfferPanel_StaleLoadAfterANewScan_IsDiscarded()
    {
        var gifts = new CountingGiftService { EntryRecorded = true, Gate = new TaskCompletionSource() };
        var panel = TestUi.CreateGiftPanel(gifts);

        var stale = panel.LoadAsync(Guid.NewGuid(), "زائر سابق");
        panel.Clear(); // the next visitor's lookup clears the panel
        gifts.Gate.SetResult();
        await stale;

        GetField<Button>(panel, "_btnIssue").Enabled.ShouldBeFalse();
        GetField<Label>(panel, "_lblDetails").Text.ShouldNotContain("زائر سابق");
    }

    private sealed class CountingGiftService : ParkGate.Application.GiftVouchers.Services.IGiftVoucherService
    {
        public bool EntryRecorded { get; set; }

        public Guid? LastBeneficiaryId { get; private set; }

        public TaskCompletionSource? Gate { get; set; }

        public async Task<ParkGate.Application.Common.Result<ParkGate.Application.GiftVouchers.DTOs.GiftEligibilityDto>> GetEligibilityAsync(Guid beneficiaryId, CancellationToken ct = default)
        {
            LastBeneficiaryId = beneficiaryId;
            if (Gate != null)
            {
                await Gate.Task;
            }

            return ParkGate.Application.Common.Result.Success(EntryRecorded
                ? new ParkGate.Application.GiftVouchers.DTOs.GiftEligibilityDto(true, null, null, null, false, 10m, null)
                : new ParkGate.Application.GiftVouchers.DTOs.GiftEligibilityDto(false, GiftDenialReason.NoEntryToday, "Gift.NoEntryToday", null, false, 10m, null));
        }

        public Task<ParkGate.Application.Common.Result<ParkGate.Application.GiftVouchers.DTOs.IssuedGiftVoucherDto>> IssueAsync(ParkGate.Application.GiftVouchers.DTOs.IssueGiftVoucherRequest request, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result<ParkGate.Application.GiftVouchers.DTOs.IssuedGiftVoucherDto>> IssueOverrideAsync(ParkGate.Application.GiftVouchers.DTOs.IssueGiftOverrideRequest request, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result<ParkGate.Application.GiftVouchers.DTOs.GiftRedemptionDto>> RedeemAsync(string? scannedText, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result> VoidAsync(ParkGate.Application.GiftVouchers.DTOs.VoidGiftVoucherRequest request, string? supervisorUserId = null, CancellationToken ct = default) => throw new NotImplementedException();
    }

    [Fact]
    public async Task CheckInDeskControl_CompanionsFromTheLastVisit_AreMarkedUntilTheOperatorChangesOne()
    {
        // R5.5: prefilled counts must not pass unnoticed; they are marked until the operator touches a counter.
        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(), OrganizationId: 1, OrganizationName: "صندوق التضامن", CardNumber: "100200",
            FullName: "محمد أحمد علي", Phone: "0912345678", NationalId: null, CardExpiryDate: null,
            Status: BeneficiaryStatus.Active, StatusReason: null, LastGrantedEntryUtc: DateTime.UtcNow.AddDays(-3),
            Notes: null, CreatedAtUtc: DateTime.UtcNow);
        var eligibility = new EligibilityPreviewDto(
            IsEligible: true, Result: EntryResult.Allowed, DenialReason: null, DenialReasonText: string.Empty,
            CanBeOverridden: false, EffectiveCooldownMinutes: 720, LastGrantedEntryUtc: holder.LastGrantedEntryUtc,
            NextAllowedEntryUtc: null, RemainingCooldownMinutes: null);
        var beneficiaries = new FakeBeneficiaryService
        {
            LookupReturnResult = [new BeneficiaryLookupResultDto(holder, eligibility, [new CompanionCountDto(1, "طفل", 2)])]
        };
        using var control = TestUi.Create<CheckInDeskControl>(beneficiaries);
        control.PopulateAgeCategories([new AgeCategoryDto(1, "طفل", "أطفال", 1, true), new AgeCategoryDto(2, "بالغ", "بالغون", 2, true)]);
        typeof(CheckInDeskControl).GetField("_prefillFromLastVisit", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(control, true);
        GetField<TextBox>(control, "_txtCardNumber").Text = "100200";

        await control.LookupCardAsync();

        var counters = GetField<List<CounterInput>>(control, "_counterInputs");
        var total = GetField<Label>(control, "_lblTotalPersons");
        counters[0].Value.ShouldBe(2);
        total.Text.ShouldContain(Strings.Label_FromLastVisit);
        counters.ShouldAllBe(c => c.IsMarkedAsPrefilled);

        counters[1].Increment();

        total.Text.ShouldNotContain(Strings.Label_FromLastVisit);
        counters.ShouldAllBe(c => !c.IsMarkedAsPrefilled);
    }

    [Fact]
    public async Task CheckInDeskControl_NoCompanionsLastVisit_ShowsNoMark()
    {
        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(), OrganizationId: 1, OrganizationName: "صندوق التضامن", CardNumber: "100201",
            FullName: "سالم علي", Phone: "0912345678", NationalId: null, CardExpiryDate: null,
            Status: BeneficiaryStatus.Active, StatusReason: null, LastGrantedEntryUtc: null,
            Notes: null, CreatedAtUtc: DateTime.UtcNow);
        var eligibility = new EligibilityPreviewDto(
            IsEligible: true, Result: EntryResult.Allowed, DenialReason: null, DenialReasonText: string.Empty,
            CanBeOverridden: false, EffectiveCooldownMinutes: 720, LastGrantedEntryUtc: null,
            NextAllowedEntryUtc: null, RemainingCooldownMinutes: null);
        var beneficiaries = new FakeBeneficiaryService { LookupReturnResult = [new BeneficiaryLookupResultDto(holder, eligibility, [])] };
        using var control = TestUi.Create<CheckInDeskControl>(beneficiaries);
        control.PopulateAgeCategories([new AgeCategoryDto(1, "طفل", "أطفال", 1, true)]);
        typeof(CheckInDeskControl).GetField("_prefillFromLastVisit", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(control, true);
        GetField<TextBox>(control, "_txtCardNumber").Text = "100201";

        await control.LookupCardAsync();

        GetField<Label>(control, "_lblTotalPersons").Text.ShouldNotContain(Strings.Label_FromLastVisit);
    }

    [Fact]
    public async Task CheckInDeskControl_RecordEntryAsync_SendsRequestAndRefreshesDesk()
    {
        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "100200",
            FullName: "محمد أحمد علي",
            Phone: "0912345678",
            NationalId: null,
            CardExpiryDate: null,
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow);

        var eligibility = new EligibilityPreviewDto(
            IsEligible: true,
            Result: EntryResult.Allowed,
            DenialReason: null,
            DenialReasonText: string.Empty,
            CanBeOverridden: false,
            EffectiveCooldownMinutes: 720,
            LastGrantedEntryUtc: null,
            NextAllowedEntryUtc: null,
            RemainingCooldownMinutes: null);

        var fakeCheckInService = new FakeCheckInService();
        using var control = TestUi.Create<CheckInDeskControl>(fakeCheckInService);
        control.AutoResetDelayMs = -1;

        control.PopulateAgeCategories(
        [
            new AgeCategoryDto(1, "طفل", "أطفال", 1, true)
        ]);

        control.ShowHolderInfo(holder, eligibility);

        var counterInputs = GetField<List<CounterInput>>(control, "_counterInputs");
        counterInputs[0].Value = 2;

        await control.RecordEntryAsync();

        fakeCheckInService.LastRecordRequest.ShouldNotBeNull();
        fakeCheckInService.LastRecordRequest.BeneficiaryId.ShouldBe(holder.Id);
        fakeCheckInService.LastRecordRequest.Companions.ShouldNotBeNull();
        fakeCheckInService.LastRecordRequest.Companions.Count.ShouldBe(1);
        fakeCheckInService.LastRecordRequest.Companions[0].Count.ShouldBe(2);

        var banner = GetField<StatusBanner>(control, "_statusBanner");
        banner.State.ShouldBe(BannerState.Granted);
    }

    [Fact]
    public async Task CheckInDeskControl_RecordEntryAsync_AfterSuccessAndReset_BothActionButtonsStayDisabled()
    {
        // R4.1: BusyScope used to re-enable the buttons on the empty desk after ResetDesk disabled them.
        var holder = new BeneficiaryDto(
            Guid.NewGuid(), 1, "صندوق التضامن", "100201", "محمد أحمد علي", "0912345678", null, null,
            BeneficiaryStatus.Active, null, null, null, DateTime.UtcNow);
        var eligibility = new EligibilityPreviewDto(true, EntryResult.Allowed, null, string.Empty, false, 720, null, null, null);

        using var control = TestUi.Create<CheckInDeskControl>(new FakeCheckInService());
        control.AutoResetDelayMs = 0;
        control.ShowHolderInfo(holder, eligibility);
        GetField<Button>(control, "_btnRecordEntry").Enabled.ShouldBeTrue();

        await control.RecordEntryAsync();

        GetField<Button>(control, "_btnRecordEntry").Enabled.ShouldBeFalse();
        GetField<Button>(control, "_btnOverride").Enabled.ShouldBeFalse();
    }

    // ── R4.3: inline registration carries an explicit organization ──────────

    private static (CheckInDeskControl Control, FakeBeneficiaryService Beneficiaries) DeskWithTwoOrganizations()
    {
        var beneficiaries = new FakeBeneficiaryService();
        var orgs = new FakeOrganizationService(
            new ParkGate.Application.Organizations.DTOs.OrganizationDto(1, "SHA", "هيئة الشهداء", 720, IsDefault: false, IsActive: true),
            new ParkGate.Application.Organizations.DTOs.OrganizationDto(2, "SSF", "صندوق التضامن", 720, IsDefault: true, IsActive: true));
        var control = TestUi.Create<CheckInDeskControl>(beneficiaries, orgs);
        return (control, beneficiaries);
    }

    private static async Task FillAndSaveRegistrationAsync(CheckInDeskControl control)
    {
        GetField<TextBox>(GetField<InlineRegistrationPanel>(control, "_registrationPanel"), "_txtRegFullName").Text = "سالم محمود علي الورفلي";
        GetField<TextBox>(GetField<InlineRegistrationPanel>(control, "_registrationPanel"), "_txtRegPhone").Text = "0912345678";
        await control.SaveInlineRegistrationAsync();
    }

    [Fact]
    public async Task CheckInDeskControl_InlineRegistration_FilterAll_UsesTheDefaultOrganization()
    {
        var (control, beneficiaries) = DeskWithTwoOrganizations();
        using var _ = control;
        await control.OnDeskLoadedAsync();
        GetField<ComboBox>(control, "_cmbOrganization").SelectedIndex = 0; // «الكل»
        GetField<TextBox>(control, "_txtCardNumber").Text = "555";

        control.ShowInlineRegistration("555");
        await FillAndSaveRegistrationAsync(control);

        beneficiaries.LastRegisterRequest.ShouldNotBeNull();
        beneficiaries.LastRegisterRequest.OrganizationId.ShouldBe(2);
    }

    [Fact]
    public async Task CheckInDeskControl_InlineRegistration_Success_SaysSoInTheBannerWithoutAMessageBox()
    {
        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(), OrganizationId: 2, OrganizationName: "صندوق التضامن", CardNumber: "560",
            FullName: "سالم محمود علي الورفلي", Phone: "0912345678", NationalId: null,
            CardExpiryDate: null, Status: BeneficiaryStatus.Active, StatusReason: null,
            LastGrantedEntryUtc: null, Notes: null, CreatedAtUtc: DateTime.UtcNow);
        var eligibility = new EligibilityPreviewDto(
            IsEligible: true, Result: EntryResult.Allowed, DenialReason: null, DenialReasonText: string.Empty,
            CanBeOverridden: false, EffectiveCooldownMinutes: 720, LastGrantedEntryUtc: null,
            NextAllowedEntryUtc: null, RemainingCooldownMinutes: null);
        var beneficiaries = new FakeBeneficiaryService { RegisterSucceedsWith = new BeneficiaryLookupResultDto(holder, eligibility, []) };
        var notify = new FakeNotifyService(confirmAnswer: true);
        var orgs = new FakeOrganizationService(
            new ParkGate.Application.Organizations.DTOs.OrganizationDto(2, "SSF", "صندوق التضامن", 720, IsDefault: true, IsActive: true));
        using var control = TestUi.Create<CheckInDeskControl>(beneficiaries, orgs, notify);
        await control.OnDeskLoadedAsync();
        GetField<TextBox>(control, "_txtCardNumber").Text = "560";

        control.ShowInlineRegistration("560", orgId: 2);
        await FillAndSaveRegistrationAsync(control);

        notify.InfoCalls.ShouldBe(0);
        var banner = GetField<StatusBanner>(control, "_statusBanner");
        banner.State.ShouldBe(BannerState.Granted);
        banner.Details.ShouldBe(Strings.Ui_CheckInDesk_10);
        GetField<GroupBox>(control, "_grpHolder").Visible.ShouldBeTrue();
        GetField<Button>(control, "_btnRecordEntry").Enabled.ShouldBeTrue();
    }

    [Fact]
    public async Task CheckInDeskControl_InlineRegistration_PreselectsTheFilteredOrganization()
    {
        var (control, beneficiaries) = DeskWithTwoOrganizations();
        using var _ = control;
        await control.OnDeskLoadedAsync();
        GetField<TextBox>(control, "_txtCardNumber").Text = "556";

        control.ShowInlineRegistration("556", orgId: 1);
        await FillAndSaveRegistrationAsync(control);

        beneficiaries.LastRegisterRequest!.OrganizationId.ShouldBe(1);
    }

    [Fact]
    public async Task CheckInDeskControl_InlineRegistration_OperatorCanChooseAnotherOrganization()
    {
        var (control, beneficiaries) = DeskWithTwoOrganizations();
        using var _ = control;
        await control.OnDeskLoadedAsync();
        GetField<TextBox>(control, "_txtCardNumber").Text = "557";

        control.ShowInlineRegistration("557");
        GetField<ComboBox>(GetField<InlineRegistrationPanel>(control, "_registrationPanel"), "_cmbRegOrganization").SelectedIndex = 0; // first item: هيئة الشهداء (id 1)
        await FillAndSaveRegistrationAsync(control);

        beneficiaries.LastRegisterRequest!.OrganizationId.ShouldBe(1);
    }

    // ── R4.4: a card registered under another organization ──────────────────

    [Fact]
    public async Task CheckInDeskControl_CardOnlyInAnotherOrganization_SaysWhereAndSecondEnterLoadsIt()
    {
        var holderInOrg2 = new BeneficiaryDto(
            Guid.NewGuid(), 2, "صندوق التضامن", "777", "خالد عمر", "0912345678", null, null,
            BeneficiaryStatus.Active, null, null, null, DateTime.UtcNow);
        var eligibility = new EligibilityPreviewDto(true, EntryResult.Allowed, null, string.Empty, false, 720, null, null, null);
        var beneficiaries = new FakeBeneficiaryService
        {
            LookupReturnResult = [new BeneficiaryLookupResultDto(holderInOrg2, eligibility, [])]
        };
        var orgs = new FakeOrganizationService(
            new ParkGate.Application.Organizations.DTOs.OrganizationDto(1, "SHA", "هيئة الشهداء", 720, IsDefault: false, IsActive: true),
            new ParkGate.Application.Organizations.DTOs.OrganizationDto(2, "SSF", "صندوق التضامن", 720, IsDefault: true, IsActive: true));
        using var control = TestUi.Create<CheckInDeskControl>(beneficiaries, orgs);
        await control.OnDeskLoadedAsync();
        var filter = GetField<ComboBox>(control, "_cmbOrganization");
        filter.SelectedIndex = 1; // هيئة الشهداء (id 1)
        GetField<TextBox>(control, "_txtCardNumber").Text = "777";

        await control.LookupCardAsync();

        var banner = GetField<StatusBanner>(control, "_statusBanner");
        banner.State.ShouldBe(BannerState.Warning);
        banner.Title.ShouldContain("صندوق التضامن");
        GetField<InlineRegistrationPanel>(control, "_registrationPanel").Visible.ShouldBeFalse();

        await control.LookupCardAsync();

        beneficiaries.LastOrganizationLookup.ShouldBe(2);
        GetField<ComboBox>(control, "_cmbOrganization").SelectedIndex.ShouldBe(2);
        banner.State.ShouldBe(BannerState.Granted);
    }

    // ── R4.5: Esc does not silently discard a half-filled registration ──────

    private static bool PressEscape(CheckInDeskControl control) =>
        (bool)typeof(CheckInDeskControl)
            .GetMethod("ProcessCmdKey", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(control, [new Message(), Keys.Escape])!;

    [Theory]
    [InlineData(false, true)]  // operator says "keep": the form and the typed name stay
    [InlineData(true, false)]  // operator confirms: the desk resets
    public void CheckInDeskControl_Escape_OnHalfFilledRegistration_AsksFirst(bool confirm, bool formStays)
    {
        var notify = new FakeNotifyService(confirm);
        using var control = TestUi.Create<CheckInDeskControl>(notify);
        control.ShowInlineRegistration("888");
        GetField<TextBox>(GetField<InlineRegistrationPanel>(control, "_registrationPanel"), "_txtRegFullName").Text = "سالم محمود";

        PressEscape(control).ShouldBeTrue();

        notify.ConfirmCalls.ShouldBe(1);
        GetField<InlineRegistrationPanel>(control, "_registrationPanel").Visible.ShouldBe(formStays);
        GetField<TextBox>(GetField<InlineRegistrationPanel>(control, "_registrationPanel"), "_txtRegFullName").Text.ShouldBe(formStays ? "سالم محمود" : string.Empty);
    }

    [Fact]
    public void CheckInDeskControl_Escape_OnEmptyRegistration_ResetsWithoutAsking()
    {
        var notify = new FakeNotifyService(confirmAnswer: false);
        using var control = TestUi.Create<CheckInDeskControl>(notify);
        control.ShowInlineRegistration("889");

        PressEscape(control).ShouldBeTrue();

        notify.ConfirmCalls.ShouldBe(0);
        GetField<InlineRegistrationPanel>(control, "_registrationPanel").Visible.ShouldBeFalse();
    }

    // ── R4.6: choosing among several organizations ─────────────────────────

    private static BeneficiaryLookupResultDto MatchIn(int orgId, string orgName, string name) => new(
        new BeneficiaryDto(Guid.NewGuid(), orgId, orgName, "999", name, "0912345678", null, null,
            BeneficiaryStatus.Active, null, null, null, DateTime.UtcNow),
        new EligibilityPreviewDto(true, EntryResult.Allowed, null, string.Empty, false, 720, null, null, null),
        []);

    [Fact]
    public void SelectOrganizationDialog_HasEnterAndEscButtons_AndChoosesTheSelectedRow()
    {
        using var dialog = new ParkGate.WinForms.Forms.SelectOrganizationDialog();
        var first = MatchIn(1, "هيئة الشهداء", "أحمد");
        var second = MatchIn(2, "صندوق التضامن", "سالم");
        dialog.SetMatches([first, second]);

        dialog.AcceptButton.ShouldBe(GetField<Button>(dialog, "_btnSelect"));
        dialog.CancelButton.ShouldBe(GetField<Button>(dialog, "_btnCancel"));
        dialog.SelectedMatch.ShouldBeNull();

        GetField<ListBox>(dialog, "_lstOrganizations").SelectedIndex = 1;
        dialog.Choose();

        dialog.DialogResult.ShouldBe(DialogResult.OK);
        dialog.SelectedMatch.ShouldBe(second);
    }

    [Fact]
    public async Task CheckInDeskControl_SeveralOrganizationsAndChooserCancelled_ShowsNobody()
    {
        // R4.6: closing the chooser used to load matches[0].
        var beneficiaries = new FakeBeneficiaryService
        {
            LookupReturnResult = [MatchIn(1, "هيئة الشهداء", "أحمد"), MatchIn(2, "صندوق التضامن", "سالم")]
        };
        using var control = TestUi.Create<CheckInDeskControl>(beneficiaries);
        control.ChooseOrganizationOverride = _ => null;
        GetField<TextBox>(control, "_txtCardNumber").Text = "999";

        await control.LookupCardAsync();

        GetField<StatusBanner>(control, "_statusBanner").State.ShouldBe(BannerState.Neutral);
        GetField<Button>(control, "_btnRecordEntry").Enabled.ShouldBeFalse();
        GetField<Label>(control, "_lblHolderName").Text.ShouldNotContain("أحمد");
    }

    // ── R4.9: no invented age categories ───────────────────────────────────

    private sealed class FailingAgeCategoryService : ParkGate.Application.AgeCategories.Services.IAgeCategoryService
    {
        public int Calls { get; private set; }
        public bool Fail { get; set; } = true;

        public Task<IReadOnlyList<AgeCategoryDto>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            Calls++;
            return Fail
                ? throw new InvalidOperationException("database is down")
                : Task.FromResult<IReadOnlyList<AgeCategoryDto>>([new AgeCategoryDto(7, "طفل", null, 1, true)]);
        }

        public Task<IReadOnlyList<AgeCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result<AgeCategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result<AgeCategoryDto>> CreateAsync(CreateAgeCategoryRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result<AgeCategoryDto>> UpdateAsync(UpdateAgeCategoryRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result> DeactivateAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result> ReactivateAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    [Fact]
    public async Task CheckInDeskControl_AgeCategoriesFailToLoad_NoFakeCountersAndRecordingDisabled()
    {
        var categories = new FailingAgeCategoryService();
        using var control = TestUi.Create<CheckInDeskControl>(categories);

        await control.LoadAgeCategoriesAsync();

        GetField<List<CounterInput>>(control, "_counterInputs").ShouldBeEmpty();
        GetField<StatusBanner>(control, "_statusBanner").State.ShouldBe(BannerState.Denied);

        control.ShowHolderInfo(
            new BeneficiaryDto(Guid.NewGuid(), 1, "صندوق التضامن", "321", "علي", "0912345678", null, null,
                BeneficiaryStatus.Active, null, null, null, DateTime.UtcNow),
            new EligibilityPreviewDto(true, EntryResult.Allowed, null, string.Empty, false, 720, null, null, null));

        GetField<Button>(control, "_btnRecordEntry").Enabled.ShouldBeFalse();
    }

    [Fact]
    public async Task CheckInDeskControl_AgeCategoriesFailed_NextLookupRetriesLoading()
    {
        var categories = new FailingAgeCategoryService();
        using var control = TestUi.Create<CheckInDeskControl>(new FakeBeneficiaryService(), categories);
        // Creating the handle raises Load, which loads the desk once; measure only what the lookup does.
        // Fakes complete synchronously, so Load runs to the end here, on the test thread.
        control.CreateControl();
        await control.LoadAgeCategoriesAsync();
        var callsBeforeLookup = categories.Calls;
        categories.Fail = false;
        GetField<TextBox>(control, "_txtCardNumber").Text = "322";

        await control.LookupCardAsync();

        categories.Calls.ShouldBe(callsBeforeLookup + 1);
        GetField<List<CounterInput>>(control, "_counterInputs").Count.ShouldBe(1);
    }

    // ── R4.2: nothing is lost during the success banner ────────────────────

    private static (CheckInDeskControl Control, FakeBeneficiaryService Beneficiaries, TaskCompletionSource Delay) DeskWithHolderShown(string nextHolderName)
    {
        // The success delay is a task the test completes itself, so everything stays on the test thread.
        var delay = new TaskCompletionSource();
        var next = new BeneficiaryDto(Guid.NewGuid(), 1, "صندوق التضامن", "777", nextHolderName, "0912345678", null, null,
            BeneficiaryStatus.Active, null, null, null, DateTime.UtcNow);
        var eligibility = new EligibilityPreviewDto(true, EntryResult.Allowed, null, string.Empty, false, 720, null, null, null);
        var beneficiaries = new FakeBeneficiaryService { LookupReturnResult = [new BeneficiaryLookupResultDto(next, eligibility, [])] };
        var control = TestUi.Create<CheckInDeskControl>(beneficiaries, new FakeCheckInService());
        control.AutoResetDelayMs = 300;
        control.SuccessDelay = _ => delay.Task;
        control.ShowHolderInfo(
            new BeneficiaryDto(Guid.NewGuid(), 1, "صندوق التضامن", "100", "الحامل الأول", "0912345678", null, null,
                BeneficiaryStatus.Active, null, null, null, DateTime.UtcNow),
            eligibility);
        return (control, beneficiaries, delay);
    }

    [Fact]
    public async Task CheckInDeskControl_ScanDuringSuccessDelay_IsShownAndNotWipedWhenTheDelayEnds()
    {
        var (control, _, delay) = DeskWithHolderShown("الحامل الثاني");
        using var _c = control;

        var recording = control.RecordEntryAsync();          // waits in the success delay
        GetField<TextBox>(control, "_txtCardNumber").Text = "777";
        await control.LookupCardAsync();                      // the scanner's Enter
        delay.SetResult();
        await recording;

        GetField<TextBox>(control, "_txtCardNumber").Text.ShouldBe("777");
        GetField<Label>(control, "_lblHolderName").Text.ShouldContain("الحامل الثاني");
        GetField<Button>(control, "_btnRecordEntry").Enabled.ShouldBeTrue();
    }

    [Fact]
    public async Task CheckInDeskControl_TypingDuringSuccessDelay_KeepsTheText()
    {
        var (control, beneficiaries, delay) = DeskWithHolderShown("الحامل الثاني");
        using var _c = control;

        var recording = control.RecordEntryAsync();
        GetField<TextBox>(control, "_txtCardNumber").Text.ShouldBeEmpty(); // freed at once for the next card
        GetField<TextBox>(control, "_txtCardNumber").Text = "12";          // typed, no Enter yet
        delay.SetResult();
        await recording;

        GetField<TextBox>(control, "_txtCardNumber").Text.ShouldBe("12");
        beneficiaries.LookupCalls.ShouldBe(0); // a half-typed number is not looked up
        GetField<StatusBanner>(control, "_statusBanner").State.ShouldBe(BannerState.Neutral);
    }

    [Fact]
    public async Task CheckInDeskControl_LookupWhileSearching_IsQueuedNotDropped()
    {
        var beneficiaries = new FakeBeneficiaryService { LookupGate = new TaskCompletionSource() };
        using var control = TestUi.Create<CheckInDeskControl>(beneficiaries);
        var cardBox = GetField<TextBox>(control, "_txtCardNumber");

        cardBox.Text = "111";
        var first = control.LookupCardAsync();
        cardBox.Text = "222";
        await control.LookupCardAsync();   // arrives while the first is still running
        beneficiaries.LookupGate.SetResult();
        await first;

        beneficiaries.LookupCalls.ShouldBe(2);
        beneficiaries.LastCardNumberLookup.ShouldBe("222");
    }

    [Fact]
    public void BusyScope_RestoresEachControlsPreviousEnabledState()
    {
        using var enabled = new Button { Enabled = true };
        using var disabled = new Button { Enabled = false };

        using (new ParkGate.WinForms.Common.BusyScope(null, enabled, disabled))
        {
            enabled.Enabled.ShouldBeFalse();
            disabled.Enabled.ShouldBeFalse();
        }

        enabled.Enabled.ShouldBeTrue();
        disabled.Enabled.ShouldBeFalse();
    }

    [Fact]
    public async Task CheckInDeskControl_RefreshTodayDeskSummaryAsync_UpdatesSummaryStripAndBreakdown()
    {
        var fakeCheckInService = new FakeCheckInService();
        using var control = TestUi.Create<CheckInDeskControl>(fakeCheckInService);

        await control.RefreshTodayDeskSummaryAsync();

        var lblTodaySummary = GetField<Label>(GetField<TodayEntriesPanel>(control, "_todayPanel"), "_lblTodaySummary");
        lblTodaySummary.Text.ShouldContain("المقبول:");
        lblTodaySummary.Text.ShouldContain("إجمالي الأفراد:");
    }

    [Fact]
    public void CheckInDeskControl_ProcessCmdKey_F2_StartsManualRegistration()
    {
        using var control = TestUi.Create<CheckInDeskControl>();
        var txtCard = GetField<TextBox>(control, "_txtCardNumber");
        txtCard.Text = "554433";

        var msg = new Message();
        var handled = typeof(CheckInDeskControl)
            .GetMethod("ProcessCmdKey", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(control, [msg, Keys.F2]);

        handled.ShouldBe(true);
        var grpReg = GetField<InlineRegistrationPanel>(control, "_registrationPanel");
        grpReg.Visible.ShouldBeTrue();
    }

    [Fact]
    public void CheckInDeskControl_ProcessCmdKey_Escape_ResetsDesk()
    {
        using var control = TestUi.Create<CheckInDeskControl>();
        var txtCard = GetField<TextBox>(control, "_txtCardNumber");
        txtCard.Text = "554433";

        var msg = new Message();
        var handled = typeof(CheckInDeskControl)
            .GetMethod("ProcessCmdKey", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(control, [msg, Keys.Escape]);

        handled.ShouldBe(true);
        txtCard.Text.ShouldBeEmpty();
    }

    [Fact]
    public async Task CheckInDeskControl_LookupCardAsync_WhenPreviousHolderDisplayed_ClearsPreviousHolderDataBeforeLookup()
    {
        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "100200",
            FullName: "محمد أحمد علي",
            Phone: "0912345678",
            NationalId: "119900123456",
            CardExpiryDate: new DateOnly(2028, 1, 1),
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow);

        var eligibility = new EligibilityPreviewDto(
            IsEligible: true,
            Result: EntryResult.Allowed,
            DenialReason: null,
            DenialReasonText: string.Empty,
            CanBeOverridden: false,
            EffectiveCooldownMinutes: 720,
            LastGrantedEntryUtc: null,
            NextAllowedEntryUtc: null,
            RemainingCooldownMinutes: null);

        var fakeBeneficiaryService = new FakeBeneficiaryService();
        using var control = TestUi.Create<CheckInDeskControl>(fakeBeneficiaryService);

        control.ShowHolderInfo(holder, eligibility);
        var lblName = GetField<Label>(control, "_lblHolderName");
        var btnRecord = GetField<Button>(control, "_btnRecordEntry");
        lblName.Text.ShouldBe("محمد أحمد علي");
        btnRecord.Enabled.ShouldBeTrue();

        var txtCard = GetField<TextBox>(control, "_txtCardNumber");
        txtCard.Text = "998877";

        await control.LookupCardAsync();

        lblName.Text.ShouldBe("-");
        btnRecord.Enabled.ShouldBeFalse();
    }

    [Fact]
    public void CheckInDeskControl_TextChanged_WhenCardNumberDiffersFromActiveHolder_ClearsPreviousHolderDataImmediately()
    {
        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "100200",
            FullName: "محمد أحمد علي",
            Phone: "0912345678",
            NationalId: "119900123456",
            CardExpiryDate: new DateOnly(2028, 1, 1),
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow);

        var eligibility = new EligibilityPreviewDto(
            IsEligible: true,
            Result: EntryResult.Allowed,
            DenialReason: null,
            DenialReasonText: string.Empty,
            CanBeOverridden: false,
            EffectiveCooldownMinutes: 720,
            LastGrantedEntryUtc: null,
            NextAllowedEntryUtc: null,
            RemainingCooldownMinutes: null);

        using var control = TestUi.Create<CheckInDeskControl>();
        control.ShowHolderInfo(holder, eligibility);

        var lblName = GetField<Label>(control, "_lblHolderName");
        var btnRecord = GetField<Button>(control, "_btnRecordEntry");
        var banner = GetField<StatusBanner>(control, "_statusBanner");
        lblName.Text.ShouldBe("محمد أحمد علي");
        btnRecord.Enabled.ShouldBeTrue();

        var txtCard = GetField<TextBox>(control, "_txtCardNumber");
        txtCard.Text = "999999";

        lblName.Text.ShouldBe("-");
        btnRecord.Enabled.ShouldBeFalse();
        banner.State.ShouldBe(BannerState.Neutral);
    }

    private sealed class FakeCurrentUser : ParkGate.Application.Security.ICurrentUser
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public UserRole? Role { get; set; }
        public bool IsAuthenticated => UserId.HasValue;
        public bool Has(ParkGate.Application.Security.Permission permission) => true;
        public bool HasAny(params ParkGate.Application.Security.Permission[] permissions) => true;
        public bool IsInRole(UserRole role) => Role == role;
    }

    private sealed class FakeBeneficiaryService : ParkGate.Application.Beneficiaries.Services.IBeneficiaryService
    {
        public string? LastCardNumberLookup { get; private set; }
        public IReadOnlyList<BeneficiaryLookupResultDto> LookupReturnResult { get; set; } = [];

        public int? LastOrganizationLookup { get; private set; }

        public int LookupCalls { get; private set; }

        /// <summary>When set, lookups wait for it: lets a test start a second lookup while the first is running.</summary>
        public TaskCompletionSource? LookupGate { get; set; }

        public async Task<ParkGate.Application.Common.Result<CardLookupDto>> LookupByCardNumberAsync(string cardNumber, int? organizationId = null, CancellationToken cancellationToken = default)
        {
            LookupCalls++;
            LastCardNumberLookup = cardNumber;
            LastOrganizationLookup = organizationId;
            if (LookupGate != null)
            {
                await LookupGate.Task;
            }

            // Same split as BeneficiaryService (R3.8).
            var inSelected = LookupReturnResult.Where(m => organizationId is null || m.Beneficiary.OrganizationId == organizationId).ToList();
            var elsewhere = LookupReturnResult
                .Where(m => organizationId is not null && m.Beneficiary.OrganizationId != organizationId)
                .Select(m => new OrganizationRefDto(m.Beneficiary.OrganizationId, m.Beneficiary.OrganizationName))
                .ToList();
            return ParkGate.Application.Common.Result.Success(new CardLookupDto(inSelected, elsewhere));
        }

        public Task<ParkGate.Application.Common.Result<BeneficiaryLookupResultDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public RegisterBeneficiaryRequest? LastRegisterRequest { get; private set; }

        /// <summary>When set, registration succeeds and the holder is found on the next lookup.</summary>
        public BeneficiaryLookupResultDto? RegisterSucceedsWith { get; set; }

        public Task<ParkGate.Application.Common.Result<BeneficiaryDto>> RegisterAsync(RegisterBeneficiaryRequest request, CancellationToken cancellationToken = default)
        {
            LastRegisterRequest = request;
            if (RegisterSucceedsWith is { } registered)
            {
                LookupReturnResult = [registered];
                return Task.FromResult(ParkGate.Application.Common.Result.Success(registered.Beneficiary));
            }

            return Task.FromResult(ParkGate.Application.Common.Result.Failure<BeneficiaryDto>(ParkGate.Application.Common.Errors.General.NotFound));
        }

        public Task<ParkGate.Application.Common.Result<BeneficiaryDto>> UpdateAsync(UpdateBeneficiaryRequest request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<ParkGate.Application.Common.Result<BeneficiaryDto>> ChangeStatusAsync(ChangeBeneficiaryStatusRequest request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<ParkGate.Application.Common.Result<IReadOnlyList<BeneficiaryDto>>> SearchAsync(string searchTerm, int maxResults = 50, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<ParkGate.Application.Common.Result<IReadOnlyList<EntryAttemptDto>>> GetHistoryAsync(Guid beneficiaryId, int maxResults = 100, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class FakeOrganizationService(params ParkGate.Application.Organizations.DTOs.OrganizationDto[] organizations)
        : ParkGate.Application.Organizations.Services.IOrganizationService
    {
        public Task<IReadOnlyList<ParkGate.Application.Organizations.DTOs.OrganizationDto>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ParkGate.Application.Organizations.DTOs.OrganizationDto>>(organizations);

        public Task<ParkGate.Application.Common.Result<ParkGate.Application.Organizations.DTOs.OrganizationDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result<ParkGate.Application.Organizations.DTOs.OrganizationDto>> CreateAsync(ParkGate.Application.Organizations.DTOs.CreateOrganizationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result<ParkGate.Application.Organizations.DTOs.OrganizationDto>> UpdateAsync(ParkGate.Application.Organizations.DTOs.UpdateOrganizationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result> DeactivateAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result> ReactivateAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ParkGate.Application.Common.Result<ParkGate.Application.Organizations.DTOs.OrganizationDto>> SetPolicyAsync(ParkGate.Application.Organizations.DTOs.SetOrganizationPolicyRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeNotifyService(bool confirmAnswer) : ParkGate.WinForms.Services.INotifyService
    {
        public int ConfirmCalls { get; private set; }

        public int InfoCalls { get; private set; }

        public void Info(string message, string? title = null) => InfoCalls++;
        public void Warning(string message, string? title = null) { }
        public void Error(string message, string? title = null) { }

        public bool Confirm(string message, string? title = null)
        {
            ConfirmCalls++;
            return confirmAnswer;
        }
    }

    private sealed class FakeCheckInService : ParkGate.Application.CheckIn.Services.ICheckInService
    {
        public RecordEntryRequest? LastRecordRequest { get; private set; }

        public Task<ParkGate.Application.Common.Result<CheckInResultDto>> RecordEntryAsync(RecordEntryRequest request, CancellationToken cancellationToken = default)
        {
            LastRecordRequest = request;
            var result = new CheckInResultDto(
                AttemptId: Guid.NewGuid(),
                Result: EntryResult.Allowed,
                DenialReason: null,
                DenialReasonText: null,
                AttemptedAtUtc: DateTime.UtcNow,
                BusinessDate: DateOnly.FromDateTime(DateTime.UtcNow),
                TotalPersons: 1 + (request.Companions?.Sum(c => c.Count) ?? 0),
                EffectiveCooldownMinutes: 720,
                NextEligibleAtUtc: DateTime.UtcNow.AddHours(12),
                CanBeOverridden: false,
                Companions: []);

            return Task.FromResult(ParkGate.Application.Common.Result.Success(result));
        }

        public Task<ParkGate.Application.Common.Result<CheckInResultDto>> OverrideEntryAsync(OverrideEntryRequest request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<ParkGate.Application.Common.Result> VoidEntryAsync(VoidEntryRequest request, string? supervisorUserId = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<ParkGate.Application.Common.Result<TodayDeskSummaryDto>> GetTodayDeskEntriesAsync(int? workstationId = null, CancellationToken cancellationToken = default)
        {
            var summary = new TodayDeskSummaryDto(
                BusinessDate: DateOnly.FromDateTime(DateTime.UtcNow),
                WorkstationId: 1,
                WorkstationName: "Desk 1",
                TotalGranted: 1,
                TotalDenied: 0,
                TotalOverridden: 0,
                TotalPersons: 3,
                PersonsPerCategory: [],
                RecentAttempts: []);

            return Task.FromResult(ParkGate.Application.Common.Result.Success(summary));
        }
    }
}
