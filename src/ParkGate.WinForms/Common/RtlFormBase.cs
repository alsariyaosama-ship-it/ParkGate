using System.Drawing;
using System.Windows.Forms;

namespace ParkGate.WinForms.Common;

/// <summary>
/// Base form establishing RTL layout, app font, DPI scaling, and designer compatibility.
/// </summary>
public class RtlFormBase : Form
{
    public RtlFormBase()
    {
        InitializeRtlDefaults();
    }

    private void InitializeRtlDefaults()
    {
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        KeyPreview = true;
        StartPosition = FormStartPosition.CenterScreen;
    }
}
