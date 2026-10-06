using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Dialog form for changing beneficiary status (Active/Suspended/Blocked) per SPEC §7.3 FR-BEN-04.
/// Supervisor/Admin only. Requires mandatory reason (>= 5 chars) for Suspended or Blocked.
/// </summary>
public partial class ChangeBeneficiaryStatusDialog : RtlFormBase
{
    private readonly IBeneficiaryService? _beneficiaryService;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<ChangeBeneficiaryStatusDialog>? _logger;

    private BeneficiaryDto? _beneficiary;

    public BeneficiaryDto? UpdatedBeneficiary { get; private set; }

    public ChangeBeneficiaryStatusDialog() : this(null, null, null)
    {
    }

    public ChangeBeneficiaryStatusDialog(
        IBeneficiaryService? beneficiaryService,
        INotifyService? notifyService,
        ILogger<ChangeBeneficiaryStatusDialog>? logger)
    {
        InitializeComponent();

        _beneficiaryService = beneficiaryService;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_ChangeStatus;
        _lblHeader.Text = Strings.Header_ChangeStatus;
        _lblTargetStatusPrompt.Text = Strings.Label_TargetStatus;
        _lblReasonPrompt.Text = Strings.Label_StatusReasonPrompt;
        _radActive.Text = Strings.Status_Active;
        _radSuspended.Text = Strings.Status_Suspended;
        _radBlocked.Text = Strings.Status_Blocked;
        _btnConfirm.Text = Strings.Btn_ConfirmVoid;
        _btnCancel.Text = Strings.Btn_Cancel;
    }

    private void WireEvents()
    {
        _radActive.CheckedChanged += (_, _) => OnStatusSelectionChanged();
        _radSuspended.CheckedChanged += (_, _) => OnStatusSelectionChanged();
        _radBlocked.CheckedChanged += (_, _) => OnStatusSelectionChanged();

        _btnConfirm.Click += async (_, _) => await ConfirmChangeAsync();
    }

    public void SetBeneficiary(BeneficiaryDto beneficiary)
    {
        _beneficiary = beneficiary;

        _lblBeneficiaryInfo.Text = $"{beneficiary.FullName} (بطاقة: {beneficiary.CardNumber})";
        _lblCurrentStatus.Text = string.Format(Strings.Label_CurrentStatus, GetStatusArabic(beneficiary.Status));

        switch (beneficiary.Status)
        {
            case BeneficiaryStatus.Active:
                _radSuspended.Checked = true;
                break;
            case BeneficiaryStatus.Suspended:
                _radActive.Checked = true;
                break;
            case BeneficiaryStatus.Blocked:
                _radActive.Checked = true;
                break;
        }

        OnStatusSelectionChanged();
    }

    private void OnStatusSelectionChanged()
    {
        if (_radActive.Checked)
        {
            _txtReason.PlaceholderText = "ملاحظات التنشيط (اختياري)...";
        }
        else if (_radSuspended.Checked)
        {
            _txtReason.PlaceholderText = "سبب الإيقاف المؤقت (مطلوب - 5 أحرف على الأقل)...";
        }
        else if (_radBlocked.Checked)
        {
            _txtReason.PlaceholderText = "سبب الحظر النهائي (مطلوب - 5 أحرف على الأقل)...";
        }
    }

    public async Task ConfirmChangeAsync()
    {
        if (_beneficiary == null || _beneficiaryService == null)
        {
            return;
        }

        var targetStatus = _radActive.Checked
            ? BeneficiaryStatus.Active
            : _radSuspended.Checked
                ? BeneficiaryStatus.Suspended
                : BeneficiaryStatus.Blocked;

        var reason = _txtReason.Text.Trim();

        if (targetStatus != BeneficiaryStatus.Active && reason.Length < 5)
        {
            var msg = "سبب تغيير الحالة مطلوب ويجب ألا يقل عن 5 أحرف.";
            if (_notifyService != null)
            {
                _notifyService.Warning(msg);
            }
            else
            {
                MessageBox.Show(
                    msg,
                    Strings.Title_Warning,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            }
            _txtReason.Focus();
            return;
        }

        var request = new ChangeBeneficiaryStatusRequest(
            Id: _beneficiary.Id,
            TargetStatus: targetStatus,
            Reason: reason);

        using (new BusyScope(this, _btnConfirm, _btnCancel))
        {
            try
            {
                var result = await _beneficiaryService.ChangeStatusAsync(request);
                if (result.IsSuccess)
                {
                    UpdatedBeneficiary = result.Value;
                    if (_notifyService != null)
                    {
                        _notifyService.Info(Strings.Msg_StatusChangeSuccess);
                    }
                    else
                    {
                        MessageBox.Show(
                            Strings.Msg_StatusChangeSuccess,
                            Strings.Title_Info,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    var msg = ErrorMessages.GetMessage(result.Error);
                    if (_notifyService != null)
                    {
                        _notifyService.Warning(msg);
                    }
                    else
                    {
                        MessageBox.Show(
                            msg,
                            Strings.Title_Warning,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error changing beneficiary status.");
                var msg = Strings.Msg_UnexpectedError + Environment.NewLine + ex.Message;
                if (_notifyService != null)
                {
                    _notifyService.Error(msg);
                }
                else
                {
                    MessageBox.Show(
                        msg,
                        Strings.Title_Error,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                }
            }
        }
    }

    private static string GetStatusArabic(BeneficiaryStatus status) => status switch
    {
        BeneficiaryStatus.Active => Strings.Status_Active,
        BeneficiaryStatus.Suspended => Strings.Status_Suspended,
        BeneficiaryStatus.Blocked => Strings.Status_Blocked,
        _ => status.ToString()
    };
}
