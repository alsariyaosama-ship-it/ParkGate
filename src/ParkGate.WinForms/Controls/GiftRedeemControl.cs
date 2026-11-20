using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Common;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.GiftVouchers.Services;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Services;
using ParkGate.Domain.Common;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Controls;

/// <summary>
/// Gift voucher redemption control at rides ticket window per T-G9.
/// Allows scanning QR code or typing receipt number with live status and today's redemptions log.
/// </summary>
public partial class GiftRedeemControl : UserControl
{
    private readonly IGiftVoucherService _giftVoucherService;
    private readonly IGiftReportService _giftReportService;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;
    private readonly ISettingsService _settingsService;
    private readonly INotifyService _notifyService;
    private readonly ILogger<GiftRedeemControl> _logger;

    private bool _isBusy;
    private bool _denialSoundEnabled = true;
    private bool _successSoundEnabled = true;
    private readonly IDeskSounds _deskSounds;
    private string? _timeZoneId = "Libya Standard Time";

    public GiftRedeemControl(
        IGiftVoucherService giftVoucherService,
        IGiftReportService giftReportService,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        ISettingsService settingsService,
        INotifyService notifyService,
        ILogger<GiftRedeemControl> logger,
        IDeskSounds deskSounds)
    {
        InitializeComponent();

        _giftVoucherService = giftVoucherService ?? throw new ArgumentNullException(nameof(giftVoucherService));
        _giftReportService = giftReportService ?? throw new ArgumentNullException(nameof(giftReportService));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _currentWorkstation = currentWorkstation ?? throw new ArgumentNullException(nameof(currentWorkstation));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _notifyService = notifyService ?? throw new ArgumentNullException(nameof(notifyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _deskSounds = deskSounds ?? throw new ArgumentNullException(nameof(deskSounds));

        InitializeControlText();
        Theme.ApplyGridStyle(_gridToday);
        WireEvents();
    }

    private void InitializeControlText()
    {
        _lblScan.Text = Strings.Label_ScanGiftQr;
        _btnRedeem.Text = Strings.Btn_RedeemGift;
        _btnClear.Text = Strings.Btn_Clear;
        _grpDetails.Text = Strings.Group_RedeemedVoucherDetails;
        _lblReceiptNoTitle.Text = Strings.Label_ReceiptNoTitle;
        _lblHolderNameTitle.Text = Strings.Label_HolderNameTitle;
        _lblCardNumberTitle.Text = Strings.Label_CardNumberTitle;
        _lblOrganizationTitle.Text = Strings.Label_OrganizationTitle;
        _lblValueTitle.Text = Strings.Label_ValueTitle;
        _lblRedeemedAtTitle.Text = Strings.Col_RedeemedTime + ":";
        _grpToday.Text = Strings.Group_TodayRedemptions;
        _colRedeemedTime.HeaderText = Strings.Col_RedeemedTime;
        _colReceiptNo.HeaderText = Strings.Col_ReceiptNo;
        _colHolderName.HeaderText = Strings.Col_BeneficiaryName;
        _colCardNumber.HeaderText = Strings.Col_CardNumber;
        _colOrganization.HeaderText = Strings.Col_Organization;
        _colValue.HeaderText = Strings.Col_Value;
        _lblTodaySummary.Text = string.Format(Strings.Label_TodayRedemptionsSummary, 0, 0, Strings.Unit_Currency);

        _statusBanner.Reset(Strings.Banner_ScanReady);
    }

    private void WireEvents()
    {
        _btnRedeem.Click += async (_, _) => await RedeemAsync();
        _btnClear.Click += (_, _) => Clear();

        _txtScan.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await RedeemAsync();
            }
        };

        Load += async (_, _) => await OnControlShownAsync();
    }

    public async Task OnControlShownAsync()
    {
        await LoadSettingsAsync();
        ApplyPermissions();
        await RefreshTodayRedemptionsAsync();
        FocusScanBox();
    }

    private void ApplyPermissions()
    {
        // ViewReports controls visibility of today's workstation grid per T-G9
        var canViewReports = _currentUser.Has(Permission.ViewReports);
        _grpToday.Visible = canViewReports;
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            _denialSoundEnabled = settings.DenialSoundEnabled;
            _successSoundEnabled = settings.SuccessSoundEnabled;
            _timeZoneId = settings.TimeZoneId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load settings in GiftRedeemControl.");
        }
    }

    public void FocusScanBox()
    {
        if (_txtScan.CanFocus)
        {
            _txtScan.Focus();
            _txtScan.SelectAll();
        }
    }

    public void Clear()
    {
        _txtScan.Clear();
        _lblReceiptNo.Text = "-";
        _lblHolderName.Text = "-";
        _lblCardNumber.Text = "-";
        _lblOrganization.Text = "-";
        _lblValue.Text = "-";
        _lblRedeemedAt.Text = "-";
        _statusBanner.Reset(Strings.Banner_ScanReady);
        FocusScanBox();
    }

    public async Task RedeemAsync()
    {
        if (_isBusy)
        {
            return;
        }

        var rawInput = _txtScan.Text.Trim();
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            _statusBanner.SetWarning(Strings.Msg_GiftInputEmpty, null);
            FocusScanBox();
            return;
        }

        var normalizedInput = DigitNormalizer.NormalizeDigits(rawInput);

        _isBusy = true;
        _txtScan.Enabled = false;
        _btnRedeem.Enabled = false;

        try
        {
            var result = await _giftVoucherService.RedeemAsync(normalizedInput);
            if (result.IsSuccess)
            {
                var voucher = result.Value;

                _statusBanner.SetGranted(
                    string.Format(Strings.Banner_GiftRedeemedSuccess, voucher.Value, Strings.Unit_Currency),
                    string.Format(Strings.Label_ReceiptNo, voucher.ReceiptNo));

                _lblReceiptNo.Text = voucher.ReceiptNo.ToString();
                _lblHolderName.Text = voucher.HolderName;
                _lblCardNumber.Text = voucher.CardNumber;
                _lblOrganization.Text = voucher.OrganizationName;
                _lblValue.Text = $"{voucher.Value:0.###} {Strings.Unit_Currency}";

                var redeemedLocal = DateTimeDisplayHelper.ToParkLocalTime(voucher.RedeemedAtUtc, _timeZoneId);
                _lblRedeemedAt.Text = DateTimeDisplayHelper.FormatDateTime(redeemedLocal);

                _txtScan.Clear();
                if (_successSoundEnabled)
                {
                    _deskSounds.PlayGranted();
                }

                _ = RefreshTodayRedemptionsAsync();
            }
            else
            {
                var errorMsg = ErrorMessages.GetMessage(result.Error);
                _statusBanner.SetDenied(errorMsg, null);

                if (_denialSoundEnabled)
                {
                    _deskSounds.PlayDenied();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error redeeming gift voucher.");
            _statusBanner.SetDenied(Strings.Msg_UnexpectedError, null);
        }
        finally
        {
            _isBusy = false;
            _txtScan.Enabled = true;
            _btnRedeem.Enabled = true;
            FocusScanBox();
        }
    }

    public async Task RefreshTodayRedemptionsAsync()
    {
        if (!_currentUser.Has(Permission.ViewReports))
        {
            return;
        }

        try
        {
            // Business date per BR-05 (use park local time for date filter)
            var nowLocal = DateTimeDisplayHelper.ToParkLocalTime(DateTime.UtcNow, _timeZoneId);
            var today = DateOnly.FromDateTime(nowLocal);

            var reportResult = await _giftReportService.GetReceiptsAsync(
                today,
                today,
                organizationId: null,
                workstationId: _currentWorkstation.WorkstationId,
                filter: GiftReceiptFilter.Redeemed);

            if (reportResult.IsSuccess)
            {
                var report = reportResult.Value;
                _gridToday.SuspendLayout();
                _gridToday.Rows.Clear();

                foreach (var row in report.Rows.OrderByDescending(r => r.RedeemedAtUtc).Take(20))
                {
                    var timeStr = row.RedeemedAtUtc.HasValue
                        ? DateTimeDisplayHelper.FormatTime(DateTimeDisplayHelper.ToParkLocalTime(row.RedeemedAtUtc.Value, _timeZoneId))
                        : "-";

                    _gridToday.Rows.Add(
                        timeStr,
                        row.ReceiptNo,
                        row.HolderName,
                        row.CardNumber,
                        row.OrganizationName,
                        row.Value.ToString("0.###"));
                }

                _gridToday.ResumeLayout(true);

                var totalCount = report.Rows.Count;
                var totalValue = report.Rows.Sum(r => r.Value);
                _lblTodaySummary.Text = string.Format(Strings.Label_TodayRedemptionsSummary, totalCount, totalValue, Strings.Unit_Currency);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh today's gift redemptions.");
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Escape:
                Clear();
                return true;

            case Keys.F5:
                _ = RefreshTodayRedemptionsAsync();
                return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}
