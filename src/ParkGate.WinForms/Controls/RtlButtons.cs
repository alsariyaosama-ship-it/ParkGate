using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ParkGate.WinForms.Controls;

/// <summary>
/// Draws button and check box text on one line with <see cref="TextRenderer"/> (R5.13). At 125 % WinForms (and the
/// native button) cut the last letters of Arabic Cairo text even in a wide button («عرض التقرير» → «عرض التقر»);
/// the single-line path that <see cref="RtlGroupBox"/> uses draws it whole. The base class still paints the
/// background, border, focus cue and check glyph: during its paint <c>Text</c> reads as empty.
/// </summary>
internal static class RtlButtonText
{
    /// <summary>Space between a check glyph and its text, in logical pixels.</summary>
    public const int GlyphGapLogical = 6;

    public static TextFormatFlags Flags(ContentAlignment align, RightToLeft rtl)
    {
        var flags = TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding |
                    TextFormatFlags.HidePrefix;

        flags |= align switch
        {
            ContentAlignment.TopLeft or ContentAlignment.TopCenter or ContentAlignment.TopRight => TextFormatFlags.Top,
            ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight => TextFormatFlags.Bottom,
            _ => TextFormatFlags.VerticalCenter,
        };

        // WinForms mirrors Left/Right for RTL controls; TextRenderer does the same with the RightToLeft flag.
        flags |= align switch
        {
            ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter => TextFormatFlags.HorizontalCenter,
            ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight => TextFormatFlags.Right,
            _ => TextFormatFlags.Left,
        };

        return rtl == RightToLeft.Yes ? flags | TextFormatFlags.RightToLeft : flags;
    }

    public static Color TextColor(Control c) => c.Enabled ? c.ForeColor : SystemColors.GrayText;

    public static Size MeasureText(Control c, string text) =>
        TextRenderer.MeasureText(text, c.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
}

/// <summary>Button that draws its own text (R5.13). Drop-in replacement for <see cref="Button"/>.</summary>
[ToolboxItem(true)]
[DesignerCategory("Code")]
public class RtlButton : Button
{
    private bool _painting;

    [AllowNull]
    public override string Text
    {
        get => _painting ? string.Empty : base.Text;
        set => base.Text = value;
    }

    /// <summary>Area the text is drawn in, inside the border and padding.</summary>
    internal Rectangle TextBounds
    {
        get
        {
            var border = FlatStyle == FlatStyle.Flat ? FlatAppearance.BorderSize : LogicalToDeviceUnits(3);
            var r = ClientRectangle;
            r.Inflate(-border - 1, -border - 1);
            return new Rectangle(r.X + Padding.Left, r.Y + Padding.Top, Math.Max(0, r.Width - Padding.Horizontal), Math.Max(0, r.Height - Padding.Vertical));
        }
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var text = base.Text;
        _painting = true;
        try
        {
            base.OnPaint(pevent);
        }
        finally
        {
            _painting = false;
        }

        if (!string.IsNullOrEmpty(text))
        {
            TextRenderer.DrawText(pevent.Graphics, text, Font, TextBounds, RtlButtonText.TextColor(this), RtlButtonText.Flags(TextAlign, RightToLeft));
        }
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var size = base.GetPreferredSize(proposedSize);
        if (!AutoSize || string.IsNullOrEmpty(base.Text))
        {
            return size;
        }

        var needed = RtlButtonText.MeasureText(this, base.Text).Width + Padding.Horizontal + LogicalToDeviceUnits(16);
        return new Size(Math.Max(size.Width, needed), size.Height);
    }
}

/// <summary>Check box that draws its own text (R5.13). Drop-in replacement for <see cref="CheckBox"/>.</summary>
[ToolboxItem(true)]
[DesignerCategory("Code")]
public class RtlCheckBox : CheckBox
{
    private bool _painting;

    [AllowNull]
    public override string Text
    {
        get => _painting ? string.Empty : base.Text;
        set => base.Text = value;
    }

    internal Rectangle TextBounds => RtlCheckText.Bounds(this);

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var text = base.Text;
        _painting = true;
        try
        {
            base.OnPaint(pevent);
        }
        finally
        {
            _painting = false;
        }

        RtlCheckText.Draw(this, pevent.Graphics, text);
    }

    public override Size GetPreferredSize(Size proposedSize) => RtlCheckText.PreferredSize(this, base.GetPreferredSize(proposedSize), base.Text);
}

/// <summary>Radio button that draws its own text (R5.13). Drop-in replacement for <see cref="RadioButton"/>.</summary>
[ToolboxItem(true)]
[DesignerCategory("Code")]
public class RtlRadioButton : RadioButton
{
    private bool _painting;

    [AllowNull]
    public override string Text
    {
        get => _painting ? string.Empty : base.Text;
        set => base.Text = value;
    }

    internal Rectangle TextBounds => RtlCheckText.Bounds(this);

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var text = base.Text;
        _painting = true;
        try
        {
            base.OnPaint(pevent);
        }
        finally
        {
            _painting = false;
        }

        RtlCheckText.Draw(this, pevent.Graphics, text);
    }

    public override Size GetPreferredSize(Size proposedSize) => RtlCheckText.PreferredSize(this, base.GetPreferredSize(proposedSize), base.Text);
}

/// <summary>Shared text layout for check boxes and radio buttons: the text sits beside the glyph.</summary>
internal static class RtlCheckText
{
    private const int GlyphLogical = 13;

    public static Rectangle Bounds(ButtonBase c)
    {
        var glyph = c.LogicalToDeviceUnits(GlyphLogical + RtlButtonText.GlyphGapLogical);
        var r = c.ClientRectangle;
        r = new Rectangle(r.X + c.Padding.Left, r.Y + c.Padding.Top, Math.Max(0, r.Width - c.Padding.Horizontal), Math.Max(0, r.Height - c.Padding.Vertical));

        // The glyph is on the leading side: the right in RTL. A mirrored window flips x itself.
        var glyphOnTheRight = c.RightToLeft == RightToLeft.Yes && !c.IsMirrored;
        return glyphOnTheRight
            ? new Rectangle(r.X, r.Y, Math.Max(0, r.Width - glyph), r.Height)
            : new Rectangle(r.X + glyph, r.Y, Math.Max(0, r.Width - glyph), r.Height);
    }

    public static void Draw(ButtonBase c, Graphics g, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        // The text starts next to the glyph: leading alignment.
        var flags = RtlButtonText.Flags(ContentAlignment.MiddleLeft, c.RightToLeft);
        if (c.RightToLeft == RightToLeft.Yes && !c.IsMirrored)
        {
            flags = (flags & ~TextFormatFlags.Left) | TextFormatFlags.Right;
        }

        TextRenderer.DrawText(g, text, c.Font, Bounds(c), RtlButtonText.TextColor(c), flags);
    }

    public static Size PreferredSize(ButtonBase c, Size basePreferred, string text)
    {
        if (!c.AutoSize || string.IsNullOrEmpty(text))
        {
            return basePreferred;
        }

        var needed = RtlButtonText.MeasureText(c, text).Width + c.Padding.Horizontal +
                     c.LogicalToDeviceUnits(GlyphLogical + RtlButtonText.GlyphGapLogical + 4);
        return new Size(Math.Max(basePreferred.Width, needed), basePreferred.Height);
    }
}
