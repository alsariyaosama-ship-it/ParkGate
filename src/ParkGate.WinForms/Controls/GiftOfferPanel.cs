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
    // Hosted in the CheckInDeskControl designer, so the services arrive through InitializeServices (R7.9).
    private PanelServices? _services;

    private Guid? _currentBeneficiaryId;
    private GiftEligibilityDto? _eligibility;
    private string? _timeZoneId = "Libya Standard Time";

    public GiftOfferPanel()
    {
        InitializeComponent();
        InitializeControlText();
        WireEvents();
        Visible = false;
    }

    private PanelServices Services =>
        _services ?? throw new InvalidOperationException("GiftOfferPanel.InitializeServices was not called.");

    public void InitializeServices(
        IGiftVoucherService giftVoucherService,
        ICurrentUser currentUser,
        IFormFactory formFactory,
        INotifyService notifyService,
        ILogger<GiftOfferPanel> logger,
        string? timeZoneId = null)
    {
        _services = new PanelServices(
            giftVoucherService ?? throw new ArgumentNullException(nameof(giftVoucherService)),
            currentUser ?? throw new ArgumentNullException(nameof(currentUser)),
            formFactory ?? throw new ArgumentNullException(nameof(formFactory)),
            notifyService ?? throw new ArgumentNullException(nameof(notifyService)),
            logger ?? throw new ArgumentNullException(nameof(logger)));
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
        var services = Services;
        try
        {
            var result = await services.GiftVouchers.GetEligibilityAsync(beneficiaryId);
            if (!result.IsSuccess)
            {
                // Any error or failure (e.g. Forbidden) hides the panel and never blocks entry
                services.Logger.LogDebug("Gift eligibility returned failure: {ErrorCode}", result.Error.Code);
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
            services.Logger.LogError(ex, "Failed to load gift eligibility for beneficiary {BeneficiaryId}", beneficiaryId);
            Visible = false;
        }
    }

    public async Task IssueAsync()
    {
        if (_currentBeneficiaryId == null || !_btnIssue.Enabled)
        {
            return;
        }

        using (new BusyScope(this, _btnIssue, _btnIssueOverride))
        {
            try
            {
                var request = new IssueGiftVoucherRequest(_currentBeneficiaryId.Value);
                var result = await Services.GiftVouchers.IssueAsync(request);
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
                Services.Logger.LogError(ex, "Error issuing gift voucher.");
                ShowError(Strings.Msg_UnexpectedError);
            }
        }
    }

    public async Task IssueOverrideAsync()
    {
        if (_currentBeneficiaryId == null || !_btnIssueOverride.Enabled)
        {
            return;
        }

        using var supervisorDialog = Services.FormFactory.Create<SupervisorAuthDialog>();

        supervisorDialog.SetContext(defaultReason: Strings.Ui_GiftOffer_01);

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
                    supervisorDialog.ApprovalToken);

                var result = await Services.GiftVouchers.IssueOverrideAsync(request);
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
                Services.Logger.LogError(ex, "Error issuing override gift voucher.");
                ShowError(Strings.Msg_UnexpectedError);
            }
        }
    }

    private void ShowGiftDialog(IssuedGiftVoucherDto voucher)
    {
        using var dialog = Services.FormFactory.Create<GiftQrDialog>();
        dialog.SetVoucher(voucher);
        dialog.ShowDialog(this.FindForm());
    }

    private void ShowWarning(string message)
    {
        Services.Notify.Warning(message);
    }

    private void ShowError(string message)
    {
        Services.Notify.Error(message);
    }

    private sealed record PanelServices(
        IGiftVoucherService GiftVouchers,
        ICurrentUser CurrentUser,
        IFormFactory FormFactory,
        INotifyService Notify,
        ILogger<GiftOfferPanel> Logger);
}
