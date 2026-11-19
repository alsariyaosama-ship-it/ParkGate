using System.Windows.Forms;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Controls;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Pop-up host for the desk's registration panel (R5.15). After R5.8 the desk's middle area is too short for the
/// registration fields, so the same panel is moved into this dialog while a new visitor is registered; the desk keeps
/// the save logic.
/// </summary>
public partial class RegisterVisitorDialog : RtlFormBase
{
    public RegisterVisitorDialog()
    {
        InitializeComponent();
    }

    /// <summary>Asked before Esc or the close box discards the form; true lets it close.</summary>
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Func<bool>? ConfirmDiscard { get; set; }

    /// <summary>Puts <paramref name="panel"/> in the dialog, filling it; its buttons become Enter and Esc (R7.4).</summary>
    public void Host(InlineRegistrationPanel panel)
    {
        panel.Dock = DockStyle.Fill;
        panel.Visible = true;
        Controls.Add(panel);
        AcceptButton = panel.SaveButton;
        CancelButton = panel.CancelButtonControl;
    }

    /// <summary>Takes the hosted panel out again, so it can go back to the desk.</summary>
    public void Release(InlineRegistrationPanel panel)
    {
        AcceptButton = null;
        CancelButton = null;
        Controls.Remove(panel);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            Close();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult != DialogResult.OK && ConfirmDiscard is { } confirm && !confirm())
        {
            e.Cancel = true;
        }

        base.OnFormClosing(e);
    }
}
