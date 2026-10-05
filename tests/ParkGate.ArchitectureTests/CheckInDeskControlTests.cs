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
        using var control = new CheckInDeskControl();

        control.RightToLeft.ShouldBe(RightToLeft.Yes);

        var txtCard = GetField<TextBox>(control, "_txtCardNumber");
        var btnSearch = GetField<Button>(control, "_btnSearch");
        var btnClear = GetField<Button>(control, "_btnClear");
        var btnNewHolder = GetField<Button>(control, "_btnNewHolder");
        var cmbOrg = GetField<ComboBox>(control, "_cmbOrganization");
        var banner = GetField<StatusBanner>(control, "_statusBanner");
        var grpHolder = GetField<GroupBox>(control, "_grpHolder");
        var grpReg = GetField<GroupBox>(control, "_grpRegistration");
        var grpCompanions = GetField<GroupBox>(control, "_grpCompanions");
        var btnRecord = GetField<Button>(control, "_btnRecordEntry");
        var btnOverride = GetField<Button>(control, "_btnOverride");
        var grpToday = GetField<GroupBox>(control, "_grpToday");
        var gridToday = GetField<DataGridView>(control, "_gridToday");
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
        using var control = new CheckInDeskControl();

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

    [Fact]
    public void CheckInDeskControl_ShowHolderInfo_WhenCooldownActive_ShouldDisableRecordEntryAndEnableOverride()
    {
        using var control = new CheckInDeskControl();

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
    }

    [Fact]
    public void CheckInDeskControl_ShowInlineRegistration_ShouldToggleVisibility()
    {
        using var control = new CheckInDeskControl();

        var grpHolder = GetField<GroupBox>(control, "_grpHolder");
        var grpReg = GetField<GroupBox>(control, "_grpRegistration");

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
        using var control = new CheckInDeskControl();

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
        using var control = new CheckInDeskControl();

        control.PopulateAgeCategories(
        [
            new AgeCategoryDto(1, "بالغ", "12 سنة فما فوق", 1, true),
            new AgeCategoryDto(2, "طفل", "من 3 إلى 12 سنة", 2, true)
        ]);

        var flowCounters = GetField<FlowLayoutPanel>(control, "_flowCounters");
        flowCounters.Controls.Count.ShouldBe(2);

        var firstCounter = flowCounters.Controls[0].ShouldBeOfType<CounterInput>();
        firstCounter.Title.ShouldBe("بالغ:");
        firstCounter.Value.ShouldBe(0);
    }

    [Fact]
    public void CheckInDeskControl_CompanionsExceedingMaxGroupSize_ShouldDisableRecordEntry()
    {
        using var control = new CheckInDeskControl();

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
    public void CheckInDeskControl_OperatorRole_ShouldMaskNationalId()
    {
        var operatorUser = new FakeCurrentUser { Role = UserRole.Operator, UserId = 1, UserName = "op1" };
        using var control = new CheckInDeskControl(null, null, null, null, null, operatorUser, null, null, null, null);

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
        lblNatId.Text.ShouldBe("********3456");
    }

    [Fact]
    public void CheckInDeskControl_SupervisorRole_ShouldNotMaskNationalId()
    {
        var supervisorUser = new FakeCurrentUser { Role = UserRole.Supervisor, UserId = 2, UserName = "sup1" };
        using var control = new CheckInDeskControl(null, null, null, null, null, supervisorUser, null, null, null, null);

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
}
