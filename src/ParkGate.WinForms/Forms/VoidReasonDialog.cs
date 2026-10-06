using System.Windows.Forms;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Dialog for entering a mandatory reason to void a granted entry (SPEC §6 BR-13, §7.2 FR-CHK-09).
/// </summary>
public partial class VoidReasonDialog : RtlFormBase
{
    private readonly INotifyService? _notifyService;

    public string Reason { get; private set; } = string.Empty;

    public VoidReasonDialog() : this(null)
    {
    }

    public VoidReasonDialog(INotifyService? notifyService)
    {
        InitializeComponent();

        _notifyService = notifyService;

        InitializeFormText();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_VoidEntry;
        _lblHeader.Text = Strings.Header_VoidEntry;
        _lblReasonPrompt.Text = Strings.Label_VoidReasonPrompt;
        _btnConfirm.Text = Strings.Btn_ConfirmVoid;
        _btnCancel.Text = Strings.Btn_Cancel;
    }

    private void WireEvents()
    {
        _btnConfirm.Click += (_, _) => Confirm();
    }

    public void SetContext(string cardNumber, string holderName, string attemptTime, int totalPersons)
    {
        _lblAttemptInfo.Text = $"العملية: بطاقة {cardNumber} · {holderName} · {attemptTime} · {totalPersons} أفراد";
        _txtReason.Focus();
    }

    public void Confirm()
    {
        var reason = _txtReason.Text.Trim();
        if (reason.Length < 5)
        {
            var msg = "سبب الإلغاء مطلوب ويجب ألا يقل عن 5 أحرف.";
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

        Reason = reason;
        DialogResult = DialogResult.OK;
        Close();
    }
}
