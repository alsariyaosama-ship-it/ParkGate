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
        Font = AppFont.CreateFont(10F, FontStyle.Regular);
        KeyPreview = true;
        StartPosition = FormStartPosition.CenterScreen;
        if (AppIcon.Current != null)
        {
            Icon = AppIcon.Current;
        }
    }
}
