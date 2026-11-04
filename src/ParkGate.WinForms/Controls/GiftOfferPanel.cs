using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Common;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.GiftVouchers.Services;
using ParkGate.Application.Security;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Controls;

/// <summary>
/// Panel embedded on the check-in desk displaying gift eligibility and issue buttons (T-G8).
/// </summary>
public partial class GiftOfferPanel : UserControl
{
    private IGiftVoucherService? _giftVoucherService;
    private ICurrentUser? _currentUser;
    private IFormFactory? _formFactory;
    private INotifyService? _notifyService;
    private ILogger<GiftOfferPanel>? _logger;

    private Guid? _currentBeneficiaryId;
    private GiftEligibilityDto? _eligibility;
    private string? _timeZoneId = "Libya Standard Time";

    public GiftOfferPanel() : this(null, null, null, null, null)
    {
    }

    public GiftOfferPanel(
        IGiftVoucherService? giftVoucherService,
        ICurrentUser? currentUser,
        IFormFactory? formFactory,
        INotifyService? notifyService,
        ILogger<GiftOfferPanel>? logger)
    {
        InitializeComponent();

        _giftVoucherService = giftVoucherService;
        _currentUser = currentUser;
        _formFactory = formFactory;
        _notifyService = notifyService;
        _logger = logger;

        InitializeControlText();
        WireEvents();
        Visible = false;
    }

    public void InitializeServices(
        IGiftVoucherService? giftVoucherService,
        ICurrentUser? currentUser,
        IFormFactory? formFactory,
        INotifyService? notifyService,
        ILogger<GiftOfferPanel>? logger,
        string? timeZoneId = null)
    {
        _giftVoucherService = giftVoucherService;
        _currentUser = currentUser;
        _formFactory = formFactory;
        _notifyService = notifyService;
        _logger = logger;
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            _timeZoneId = timeZoneId;
        }
    }

    public void SetTimeZone(string? timeZoneId)
    {
        _timeZoneId = timeZoneId;
    }

    private void InitializeControlText()
    {
        _grpMain.Text = Strings.Title_GiftOffer;
        _btnIssue.Text = Strings.Btn_IssueGift;
        _btnIssueOverride.Text = Strings.Btn_IssueGiftOverride;
    }

    private void WireEvents()
    {
        _btnIssue.Click += async (_, _) => await IssueAsync();
        _btnIssueOverride.Click += async (_, _) => await IssueOverrideAsync();
    }

    public bool CanIssue => Visible && _btnIssue.Enabled;

    public bool CanIssueOverride => Visible && _btnIssueOverride.Enabled;

    public void Clear()
    {
        _currentBeneficiaryId = null;
        _eligibility = null;
        Visible = false;
        _btnIssue.Enabled = false;
        _btnIssueOverride.Enabled = false;
        _btnIssueOverride.Visible = false;
    }

    public async Task LoadAsync(Guid beneficiaryId)
    {
        if (_giftVoucherService == null)
        {
            Visible = false;
            return;
        }

        try
        {
            var result = await _giftVoucherService.GetEligibilityAsync(beneficiaryId);
            if (!result.IsSuccess)
            {
                // Any error or failure (e.g. Forbidden) hides the panel and never blocks entry
                _logger?.LogDebug("Gift eligibility returned failure: {ErrorCode}", result.Error.Code);
                Visible = false;
                return;
            }

            var eligibility = result.Value;
            if (eligibility.Reason == Domain.Enums.GiftDenialReason.GiftsNotEnabled)
            {
                // Organization does not have gifts enabled — hide panel
                Visible = false;
                return;
            }

            _currentBeneficiaryId = beneficiaryId;
            _eligibility = eligibility;
            Visible = true;

            if (eligibility.IsEligible)
            {
                _grpMain.BackColor = Color.FromArgb(240, 253, 244);
                _lblStatus.Text = string.Format(Strings.Label_GiftEligible, eligibility.GiftValue.ToString("0.###"), Strings.Unit_Currency);
                _lblStatus.ForeColor = Color.FromArgb(27, 135, 63);
                _lblDetails.Text = string.Empty;

                _btnIssue.Enabled = true;
                _btnIssueOverride.Enabled = false;
                _btnIssueOverride.Visible = false;
            }
            else
            {
                _grpMain.BackColor = Color.FromArgb(254, 252, 232);
                var reasonError = new Error(eligibility.ReasonCode ?? "Gift.NotEligible");
                _lblStatus.Text = string.Format(Strings.Label_GiftNotEligible, ErrorMessages.GetMessage(reasonError));
                _lblStatus.ForeColor = Color.FromArgb(178, 106, 0);

                if (eligibility.NextEligibleAtUtc.HasValue)
                {
                    var nextLocal = DateTimeDisplayHelper.ToParkLocalTime(eligibility.NextEligibleAtUtc.Value, _timeZoneId);
                    _lblDetails.Text = string.Format(Strings.Label_GiftNextEligible, DateTimeDisplayHelper.FormatDateTime(nextLocal));
                }
                else
                {
                    _lblDetails.Text = string.Empty;
                }

                _btnIssue.Enabled = false;
                _btnIssueOverride.Enabled = eligibility.CanBeOverridden;
                _btnIssueOverride.Visible = eligibility.CanBeOverridden;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load gift eligibility for beneficiary {BeneficiaryId}", beneficiaryId);
            Visible = false;
        }
    }

    public async Task IssueAsync()
    {
        if (_currentBeneficiaryId == null || _giftVoucherService == null || !_btnIssue.Enabled)
        {
            return;
        }

        using (new BusyScope(this, _btnIssue, _btnIssueOverride))
        {
            try
            {
                var request = new IssueGiftVoucherRequest(_currentBeneficiaryId.Value);
                var result = await _giftVoucherService.IssueAsync(request);
                if (result.IsSuccess)
                {
                    ShowGiftDialog(result.Value);
                    await LoadAsync(_currentBeneficiaryId.Value);
                }
                else
                {
                    ShowWarning(ErrorMessages.GetMessage(result.Error));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error issuing gift voucher.");
                ShowError(Strings.Msg_UnexpectedError);
            }
        }
    }

    public async Task IssueOverrideAsync()
    {
        if (_currentBeneficiaryId == null || _giftVoucherService == null || !_btnIssueOverride.Enabled)
        {
            return;
        }

        var supervisorDialog = _formFactory?.Create<SupervisorAuthDialog>() ??
                               new SupervisorAuthDialog(null, _currentUser);

        supervisorDialog.SetContext(defaultReason: "تجاوز مدة انتظار الهدية استثنائياً");

        if (supervisorDialog.ShowDialog(this.FindForm()) != DialogResult.OK)
        {
            return;
        }

        using (new BusyScope(this, _btnIssue, _btnIssueOverride))
        {
            try
            {
                var request = new IssueGiftOverrideRequest(
                    _currentBeneficiaryId.Value,
                    supervisorDialog.Reason,
                    supervisorDialog.SupervisorUserId);

                var result = await _giftVoucherService.IssueOverrideAsync(request);
                if (result.IsSuccess)
                {
                    ShowGiftDialog(result.Value);
                    await LoadAsync(_currentBeneficiaryId.Value);
                }
                else
                {
                    ShowWarning(ErrorMessages.GetMessage(result.Error));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error issuing override gift voucher.");
                ShowError(Strings.Msg_UnexpectedError);
            }
        }
    }

    private void ShowGiftDialog(IssuedGiftVoucherDto voucher)
    {
        var dialog = _formFactory?.Create<GiftQrDialog>() ?? new GiftQrDialog(null, _notifyService, null);
        dialog.SetVoucher(voucher);
        dialog.ShowDialog(this.FindForm());
    }

    private void ShowWarning(string message)
    {
        if (_notifyService != null)
        {
            _notifyService.Warning(message);
        }
        else
        {
            MessageBox.Show(
                message,
                Strings.Title_Warning,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void ShowError(string message)
    {
        if (_notifyService != null)
        {
            _notifyService.Error(message);
        }
        else
        {
            MessageBox.Show(
                message,
                Strings.Title_Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
}
