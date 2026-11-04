using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParkGate.Application.Common;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.GiftVouchers.Services;
using ParkGate.Application.Security;
using ParkGate.Domain.Enums;
using ParkGate.WinForms;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;
using Shouldly;

namespace ParkGate.ArchitectureTests;

public sealed class GiftRedeemControlTests
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
    public void GiftRedeemControl_ParameterlessConstructor_ShouldBeDesignerCompatibleAndRtl()
    {
        using var control = new GiftRedeemControl();
        control.RightToLeft.ShouldBe(RightToLeft.Yes);

        var txtScan = GetField<TextBox>(control, "_txtScan");
        var btnRedeem = GetField<Button>(control, "_btnRedeem");
        var btnClear = GetField<Button>(control, "_btnClear");
        var banner = GetField<StatusBanner>(control, "_statusBanner");
        var grpDetails = GetField<GroupBox>(control, "_grpDetails");
        var grpToday = GetField<GroupBox>(control, "_grpToday");

        txtScan.Text.ShouldBeEmpty();
        btnRedeem.Text.ShouldBe(Strings.Btn_RedeemGift);
        btnClear.Text.ShouldBe(Strings.Btn_Clear);
        grpDetails.Text.ShouldBe(Strings.Group_RedeemedVoucherDetails);
        grpToday.Text.ShouldBe(Strings.Group_TodayRedemptions);
    }

    [Fact]
    public async Task GiftRedeemControl_RedeemAsync_WhenEmptyInput_ShouldShowWarningBanner()
    {
        var fakeGiftService = new FakeGiftVoucherService();
        using var control = new GiftRedeemControl(fakeGiftService, null, null, null, null, null, null);

        var txtScan = GetField<TextBox>(control, "_txtScan");
        txtScan.Text = "   ";

        await control.RedeemAsync();

        fakeGiftService.LastScannedText.ShouldBeNull();
        var banner = GetField<StatusBanner>(control, "_statusBanner");
        banner.Title.ShouldBe(Strings.Msg_GiftInputEmpty);
    }

    [Fact]
    public async Task GiftRedeemControl_RedeemAsync_ShouldNormalizeArabicIndicDigits()
    {
        var fakeGiftService = new FakeGiftVoucherService
        {
            RedeemResult = Result<GiftRedemptionDto>.Success(new GiftRedemptionDto(
                ReceiptNo: 1005,
                Value: 25m,
                HolderName: "أحمد علي",
                CardNumber: "100200",
                OrganizationName: "صندوق التضامن",
                IssuedAtUtc: DateTime.UtcNow.AddHours(-1),
                RedeemedAtUtc: DateTime.UtcNow))
        };

        using var control = new GiftRedeemControl(fakeGiftService, null, null, null, null, null, null);

        var txtScan = GetField<TextBox>(control, "_txtScan");
        txtScan.Text = "١٠٠٥"; // Arabic-Indic digits for 1005

        await control.RedeemAsync();

        fakeGiftService.LastScannedText.ShouldBe("1005");

        var lblReceiptNo = GetField<Label>(control, "_lblReceiptNo");
        var lblHolderName = GetField<Label>(control, "_lblHolderName");
        var lblCardNumber = GetField<Label>(control, "_lblCardNumber");
        var lblOrg = GetField<Label>(control, "_lblOrganization");
        var lblValue = GetField<Label>(control, "_lblValue");

        lblReceiptNo.Text.ShouldBe("1005");
        lblHolderName.Text.ShouldBe("أحمد علي");
        lblCardNumber.Text.ShouldBe("100200");
        lblOrg.Text.ShouldBe("صندوق التضامن");
        lblValue.Text.ShouldContain("25");

        // Scan box should be cleared for next scan
        txtScan.Text.ShouldBeEmpty();
    }

    [Fact]
    public async Task GiftRedeemControl_RedeemAsync_WhenFailure_ShouldShowRedBannerWithMappedError()
    {
        var fakeGiftService = new FakeGiftVoucherService
        {
            RedeemResult = Result<GiftRedemptionDto>.Failure(new Error("Gift.AlreadyRedeemed"))
        };

        using var control = new GiftRedeemControl(fakeGiftService, null, null, null, null, null, null);

        var txtScan = GetField<TextBox>(control, "_txtScan");
        txtScan.Text = "1005";

        await control.RedeemAsync();

        var banner = GetField<StatusBanner>(control, "_statusBanner");
        banner.Title.ShouldBe(Strings.Error_Gift_AlreadyRedeemed);
    }

    [Fact]
    public void GiftRedeemControl_ApplyPermissions_ShouldHideTodayGridWhenNoViewReportsPermission()
    {
        var fakeUser = new FakeCurrentUser(UserRole.Operator, [Permission.RedeemGift]); // lacks ViewReports
        using var control = new GiftRedeemControl(null, null, fakeUser, null, null, null, null);

        // Call private ApplyPermissions
        var method = control.GetType().GetMethod("ApplyPermissions", BindingFlags.NonPublic | BindingFlags.Instance)!;
        method.Invoke(control, null);

        var grpToday = GetField<GroupBox>(control, "_grpToday");
        grpToday.Visible.ShouldBeFalse();
    }

    [Fact]
    public void MainForm_MenuGiftRedemption_ShouldBeVisibleIffUserHasRedeemGiftPermission()
    {
        // 1. Operator without RedeemGift permission
        var opUser = new FakeCurrentUser(UserRole.Operator, [Permission.CheckIn]);
        using var form1 = new MainForm(opUser, null, null, null, null, null, null, null, null, null);
        form1.ApplyPermissions();

        var menuRedeem1 = GetField<ToolStripMenuItem>(form1, "_menuGiftRedemption");
        menuRedeem1.Available.ShouldBeFalse();

        // 2. Rides operator with RedeemGift permission
        var ridesUser = new FakeCurrentUser(UserRole.Operator, [Permission.RedeemGift]);
        using var form2 = new MainForm(ridesUser, null, null, null, null, null, null, null, null, null);
        form2.ApplyPermissions();

        var menuRedeem2 = GetField<ToolStripMenuItem>(form2, "_menuGiftRedemption");
        menuRedeem2.Available.ShouldBeTrue();
    }

    [Fact]
    public void DependencyInjection_ShouldRegisterGiftRedeemControl()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        services.AddParkGateServices(config);

        services.Any(s => s.ServiceType == typeof(GiftRedeemControl)).ShouldBeTrue();
    }

    private sealed class FakeGiftVoucherService : IGiftVoucherService
    {
        public string? LastScannedText { get; private set; }

        public Result<GiftRedemptionDto> RedeemResult { get; set; } =
            Result<GiftRedemptionDto>.Failure(new Error("Fake.NotConfigured"));

        public Task<Result<GiftEligibilityDto>> GetEligibilityAsync(Guid beneficiaryId, CancellationToken ct = default) =>
            Task.FromResult(Result<GiftEligibilityDto>.Failure(new Error("Fake.NotImplemented")));

        public Task<Result<IssuedGiftVoucherDto>> IssueAsync(IssueGiftVoucherRequest request, CancellationToken ct = default) =>
            Task.FromResult(Result<IssuedGiftVoucherDto>.Failure(new Error("Fake.NotImplemented")));

        public Task<Result<IssuedGiftVoucherDto>> IssueOverrideAsync(IssueGiftOverrideRequest request, CancellationToken ct = default) =>
            Task.FromResult(Result<IssuedGiftVoucherDto>.Failure(new Error("Fake.NotImplemented")));

        public Task<Result<GiftRedemptionDto>> RedeemAsync(string? scannedText, CancellationToken ct = default)
        {
            LastScannedText = scannedText;
            return Task.FromResult(RedeemResult);
        }

        public Task<Result> VoidAsync(VoidGiftVoucherRequest request, int? supervisorUserId = null, CancellationToken ct = default) =>
            Task.FromResult(Result.Success());
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        private readonly HashSet<Permission> _permissions;

        public FakeCurrentUser(UserRole role, IEnumerable<Permission> permissions)
        {
            Role = role;
            _permissions = [.. permissions];
        }

        public int? UserId => 1;
        public string? UserName => "testuser";
        public string? FullName => "مستخدم تجريبي";
        public UserRole? Role { get; }
        public bool IsAuthenticated => true;

        public bool Has(Permission permission) => _permissions.Contains(permission);
        public bool HasAny(params Permission[] permissions) => permissions.Any(p => _permissions.Contains(p));
        public bool IsInRole(UserRole role) => Role == role;
    }
}
