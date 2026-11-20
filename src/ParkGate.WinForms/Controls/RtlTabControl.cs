using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ParkGate.WinForms.Controls;

/// <summary>
/// Tab control whose tabs are all as wide as the longest tab text (R5.13). The native tabs measure Arabic Cairo
/// text too narrow at 125 % and cut the first letters («المرفوضات» showed «رفوضات»).
/// </summary>
[ToolboxItem(true)]
[DesignerCategory("Code")]
public class RtlTabControl : TabControl
{
    private const int TabPaddingLogical = 24;

    /// <summary>Width every tab gets: the longest text plus padding.</summary>
    internal int FittedTabWidth =>
        TabPages.Cast<TabPage>()
            .Select(p => TextRenderer.MeasureText(p.Text, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width)
            .DefaultIfEmpty(0)
            .Max() + LogicalToDeviceUnits(TabPaddingLogical);

    /// <summary>Call after changing tab texts at run time.</summary>
    public void FitTabsToText()
    {
        if (TabCount == 0)
        {
            return;
        }

        SizeMode = TabSizeMode.Fixed;
        ItemSize = new Size(FittedTabWidth, ItemSize.Height);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        FitTabsToText();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        FitTabsToText();
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        FitTabsToText();
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        if (IsHandleCreated)
        {
            FitTabsToText();
        }
    }
}
