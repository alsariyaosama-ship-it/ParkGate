using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ParkGate.WinForms.Common;

namespace ParkGate.WinForms.Controls;

/// <summary>
/// Group box that draws its own frame and caption (R5.12). The WinForms group box measures an Arabic caption too
/// narrow in right-to-left layout and cuts its last letters («بيانات الزائر» shows «بيانات الزا»).
/// Layout (padding, display rectangle) stays the base class's, so designers and docking behave as before.
/// </summary>
[ToolboxItem(true)]
[DesignerCategory("Code")]
public class RtlGroupBox : GroupBox
{
    private const int CaptionInsetLogical = 8;
    private const int CaptionGapLogical = 4;

    public RtlGroupBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    /// <summary>Frame colour; the caption uses <see cref="Control.ForeColor"/>.</summary>
    [Category("Appearance")]
    public Color BorderColor { get; set; } = Theme.Border;

    private bool ShouldSerializeBorderColor() => BorderColor != Theme.Border;

    private void ResetBorderColor() => BorderColor = Theme.Border;

    /// <summary>Caption bounds in client coordinates (logical, before any window mirroring). Empty when there is no text.</summary>
    internal Rectangle CaptionBounds
    {
        get
        {
            if (string.IsNullOrEmpty(Text))
            {
                return Rectangle.Empty;
            }

            var size = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue), CaptionFlags);
            var inset = LogicalToDeviceUnits(CaptionInsetLogical);
            var gap = LogicalToDeviceUnits(CaptionGapLogical);
            var width = Math.Min(size.Width + (2 * gap), Math.Max(0, ClientSize.Width - (2 * inset)));
            var x = IsCaptionOnTheRight ? ClientSize.Width - inset - width : inset;
            return new Rectangle(x, 0, width, Math.Max(size.Height, Font.Height));
        }
    }

    private TextFormatFlags CaptionFlags
    {
        get
        {
            var flags = TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            return RightToLeft == RightToLeft.Yes ? flags | TextFormatFlags.RightToLeft : flags;
        }
    }

    // A mirrored window (RightToLeftLayout inherited from the form) already flips x, so the caption is drawn on
    // the logical left and appears on the right.
    private bool IsCaptionOnTheRight => RightToLeft == RightToLeft.Yes && !IsMirrored;

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var caption = CaptionBounds;
        var top = caption.IsEmpty ? 0 : caption.Height / 2;

        using (var pen = new Pen(BorderColor))
        {
            g.DrawRectangle(pen, 0, top, ClientSize.Width - 1, ClientSize.Height - top - 1);
        }

        if (caption.IsEmpty)
        {
            return;
        }

        using (var back = new SolidBrush(BackColor))
        {
            g.FillRectangle(back, caption);
        }

        TextRenderer.DrawText(g, Text, Font, caption, Enabled ? ForeColor : SystemColors.GrayText, CaptionFlags);
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
}
