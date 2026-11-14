using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.Common;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.GiftVouchers.Services;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.WinForms;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;
using Shouldly;

namespace ParkGate.ArchitectureTests;

public sealed class GiftDeskIntegrationTests
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
    public void GiftOfferPanel_ParameterlessConstructor_ShouldBeDesignerCompatible()
    {
        using var panel = new GiftOfferPanel();
        panel.ShouldNotBeNull();
        panel.Visible.ShouldBeFalse();
        panel.CanIssue.ShouldBeFalse();
        panel.CanIssueOverride.ShouldBeFalse();
    }

    [Fact]
    public void GiftQrDialog_ParameterlessConstructor_ShouldBeRtlFormBase()
    {
        using var dialog = new GiftQrDialog();
        dialog.ShouldNotBeNull();
        dialog.RightToLeft.ShouldBe(RightToLeft.Yes);
        dialog.RightToLeftLayout.ShouldBeTrue();
        dialog.Text.ShouldBe(Strings.Title_GiftQrDialog);
    }

    [Fact]
    public void GiftQrDialog_SetVoucher_ShouldPopulateLabelsCorrectly()
    {
        using var dialog = new GiftQrDialog();
        var voucher = new IssuedGiftVoucherDto(
            ReceiptNo: 1001,
            BeneficiaryId: Guid.NewGuid(),
            HolderName: "علي عمر",
            CardNumber: "100200",
            OrganizationName: "صندوق التضامن",
            Value: 15.5m,
            IssuedAtUtc: DateTime.UtcNow,
            ExpiresOn: DateOnly.FromDateTime(DateTime.UtcNow),
            IsOverride: true,
            QrText: "PGV1.test.signature");

        dialog.SetVoucher(voucher);

        var lblReceiptNo = GetField<Label>(dialog, "_lblReceiptNo");
        var lblCustomerName = GetField<Label>(dialog, "_lblCustomerName");
        var lblOrganization = GetField<Label>(dialog, "_lblOrganization");
        var lblValue = GetField<Label>(dialog, "_lblValue");
        var lblOverride = GetField<Label>(dialog, "_lblOverrideNotice");

        lblReceiptNo.Text.ShouldContain("1001");
        lblCustomerName.Text.ShouldContain("علي عمر");
        lblOrganization.Text.ShouldContain("صندوق التضامن");
        lblValue.Text.ShouldContain("15.5");

        var isStateVisible = (bool)typeof(Control).GetMethod("GetState", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(lblOverride, [2])!; // 2 = STATE_VISIBLE in System.Windows.Forms.Control
        isStateVisible.ShouldBeTrue();
    }

    [Fact]
    public async Task GiftOfferPanel_LoadAsync_WhenEligible_ShouldEnableIssueButton()
    {
        var beneficiaryId = Guid.NewGuid();
        var fakeService = new FakeGiftVoucherService
        {
            EligibilityResult = Result<GiftEligibilityDto>.Success(new GiftEligibilityDto(
                IsEligible: true,
                Reason: null,
                ReasonCode: null,
                NextEligibleAtUtc: null,
                CanBeOverridden: false,
                GiftValue: 20m,
                LastGiftIssuedUtc: null))
        };

        using var panel = new GiftOfferPanel(fakeService, null, null, null, null);

        await panel.LoadAsync(beneficiaryId);

        panel.Visible.ShouldBeTrue();
        panel.CanIssue.ShouldBeTrue();
        panel.CanIssueOverride.ShouldBeFalse();

        var lblStatus = GetField<Label>(panel, "_lblStatus");
        lblStatus.Text.ShouldContain("20");
    }

    [Fact]
    public async Task GiftOfferPanel_LoadAsync_WhenCooldown_ShouldShowOverrideOption()
    {
        var beneficiaryId = Guid.NewGuid();
        var fakeService = new FakeGiftVoucherService
        {
            EligibilityResult = Result<GiftEligibilityDto>.Success(new GiftEligibilityDto(
                IsEligible: false,
                Reason: GiftDenialReason.IntervalNotElapsed,
                ReasonCode: "Gift.IntervalNotElapsed",
                NextEligibleAtUtc: DateTime.UtcNow.AddDays(5),
                CanBeOverridden: true,
                GiftValue: 20m,
                LastGiftIssuedUtc: DateTime.UtcNow.AddDays(-10)))
        };

        using var panel = new GiftOfferPanel(fakeService, null, null, null, null);

        await panel.LoadAsync(beneficiaryId);

        panel.Visible.ShouldBeTrue();
        panel.CanIssue.ShouldBeFalse();
        panel.CanIssueOverride.ShouldBeTrue();
    }

    [Fact]
    public async Task GiftOfferPanel_LoadAsync_WhenGiftsNotEnabled_ShouldHidePanel()
    {
        var beneficiaryId = Guid.NewGuid();
        var fakeService = new FakeGiftVoucherService
        {
            EligibilityResult = Result<GiftEligibilityDto>.Success(new GiftEligibilityDto(
                IsEligible: false,
                Reason: GiftDenialReason.GiftsNotEnabled,
                ReasonCode: "Gift.NotEnabled",
                NextEligibleAtUtc: null,
                CanBeOverridden: false,
                GiftValue: 0m,
                LastGiftIssuedUtc: null))
        };

        using var panel = new GiftOfferPanel(fakeService, null, null, null, null);

        await panel.LoadAsync(beneficiaryId);

        panel.Visible.ShouldBeFalse();
        panel.CanIssue.ShouldBeFalse();
        panel.CanIssueOverride.ShouldBeFalse();
    }

    [Fact]
    public void CheckInDeskControl_FreeCompanionLimit_ShouldBlockWhenLimitExceeded()
    {
        using var desk = new CheckInDeskControl();

        var orgList = new List<OrganizationDto>
        {
            new(Id: 10, Code: "TADAMON", NameAr: "التضامن", CooldownMinutes: 1440, IsDefault: true, IsActive: true, MaxFreeCompanions: 2)
        };

        var catAdult = new AgeCategoryDto(Id: 1, NameAr: "بالغ", DescriptionAr: null, SortOrder: 1, IsActive: true, IsCompanionLimitExempt: false);
        var catInfant = new AgeCategoryDto(Id: 3, NameAr: "رضيع", DescriptionAr: null, SortOrder: 2, IsActive: true, IsCompanionLimitExempt: true);

        // Populate org and categories into desk
        var orgField = desk.GetType().GetField("_organizations", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var deskOrgs = (List<OrganizationDto>)orgField.GetValue(desk)!;
        deskOrgs.Clear();
        deskOrgs.AddRange(orgList);

        desk.PopulateAgeCategories([catAdult, catInfant]);

        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 10,
            OrganizationName: "التضامن",
            CardNumber: "123456",
            FullName: "محمد علي",
            Phone: "0912345678",
            NationalId: "119850000000",
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
            EffectiveCooldownMinutes: 1440,
            LastGrantedEntryUtc: holder.LastGrantedEntryUtc,
            NextAllowedEntryUtc: null,
            RemainingCooldownMinutes: null);

        desk.ShowHolderInfo(holder, eligibility);

        var btnRecord = GetField<Button>(desk, "_btnRecordEntry");
        var lblFree = GetField<Label>(desk, "_lblFreeCompanions");
        var counters = (List<CounterInput>)desk.GetType().GetField("_counterInputs", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(desk)!;

        var adultCounter = counters.First(c => (int)c.Tag! == 1);
        var infantCounter = counters.First(c => (int)c.Tag! == 3);

        // Case 1: 0 companions => free limit: 0 of 2 => Allowed
        btnRecord.Enabled.ShouldBeTrue();
        lblFree.Visible.ShouldBeTrue();
        lblFree.Text.ShouldContain("0");
        lblFree.Text.ShouldContain("2");

        // Case 2: 2 adult companions => 2 of 2 => Allowed
        adultCounter.Value = 2;
        btnRecord.Enabled.ShouldBeTrue();
        lblFree.Text.ShouldContain("2");

        // Case 3: 2 adult companions + 3 infants => infants are EXEMPT! => still 2 of 2 => Allowed!
        infantCounter.Value = 3;
        btnRecord.Enabled.ShouldBeTrue();
        lblFree.Text.ShouldContain("2");

        // Case 4: 3 adult companions => 3 of 2 => EXCEEDED!
        adultCounter.Value = 3;
        btnRecord.Enabled.ShouldBeFalse();
        lblFree.ForeColor.ShouldBe(StatusBanner.ColorDenied);
        lblFree.Text.ShouldContain("تجاوز الحد المجاني");

        // Reset clears state
        desk.ResetDesk();
        lblFree.Visible.ShouldBeFalse();
        btnRecord.Enabled.ShouldBeFalse();
    }

    [Fact]
    public void DependencyInjection_ShouldRegisterGiftPanelAndGiftQrDialog()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        services.AddParkGateServices(config);

        services.Any(s => s.ServiceType == typeof(GiftOfferPanel)).ShouldBeTrue();
        services.Any(s => s.ServiceType == typeof(GiftQrDialog)).ShouldBeTrue();
    }

    private sealed class FakeGiftVoucherService : IGiftVoucherService
    {
        public Result<GiftEligibilityDto> EligibilityResult { get; set; } =
            Result<GiftEligibilityDto>.Failure(new Error("Fake.NotConfigured"));

        public Task<Result<GiftEligibilityDto>> GetEligibilityAsync(Guid beneficiaryId, CancellationToken ct = default) =>
            Task.FromResult(EligibilityResult);

        public Task<Result<IssuedGiftVoucherDto>> IssueAsync(IssueGiftVoucherRequest request, CancellationToken ct = default) =>
            Task.FromResult(Result<IssuedGiftVoucherDto>.Failure(new Error("Fake.NotImplemented")));

        public Task<Result<IssuedGiftVoucherDto>> IssueOverrideAsync(IssueGiftOverrideRequest request, CancellationToken ct = default) =>
            Task.FromResult(Result<IssuedGiftVoucherDto>.Failure(new Error("Fake.NotImplemented")));

        public Task<Result<GiftRedemptionDto>> RedeemAsync(string? scannedText, CancellationToken ct = default) =>
            Task.FromResult(Result<GiftRedemptionDto>.Failure(new Error("Fake.NotImplemented")));

        public Task<Result> VoidAsync(VoidGiftVoucherRequest request, string? supervisorUserId = null, CancellationToken ct = default) =>
            Task.FromResult(Result.Success());
    }
}
