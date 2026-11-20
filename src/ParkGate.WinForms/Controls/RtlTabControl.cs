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

    private bool _fitPending;

    public RtlTabControl()
    {
        // Set once, before any handle exists: changing SizeMode later recreates the native handle, and doing that
        // while a window is being created on a 125 % screen failed with "Error creating window handle" (T8.4b).
        SizeMode = TabSizeMode.Fixed;
    }

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

        var width = FittedTabWidth;
        if (ItemSize.Width != width)
        {
            ItemSize = new Size(width, ItemSize.Height);
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ScheduleFit();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        ScheduleFit();
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        ScheduleFit();
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        ScheduleFit();
    }

    /// <summary>
    /// Without a handle the size is only stored. With one, the resize waits until the current message is done, so it
    /// never runs in the middle of the window's own creation or DPI scaling.
    /// </summary>
    private void ScheduleFit()
    {
        if (!IsHandleCreated)
        {
            FitTabsToText();
            return;
        }

        if (_fitPending)
        {
            return;
        }

        _fitPending = true;
        BeginInvoke(() =>
        {
            _fitPending = false;
            if (!IsDisposed)
            {
                FitTabsToText();
            }
        });
    }
}
