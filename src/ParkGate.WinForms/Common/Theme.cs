using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ParkGate.WinForms.Common;

/// <summary>
/// Central design tokens and styling helpers implementing winforms-ui-design skill §3 & §6.
/// Single source of truth for colors, metrics, and standard control styling.
/// </summary>
public static class Theme
{
    // Brand & Accent (ParkGate Royal Emerald)
    public static readonly Color Primary = Color.FromArgb(27, 135, 63);
    public static readonly Color PrimaryHover = Color.FromArgb(21, 109, 51);
    public static readonly Color PrimaryPressed = Color.FromArgb(17, 87, 41);
    public static readonly Color OnPrimary = Color.White;
    public static readonly Color Selection = Color.FromArgb(220, 252, 231);
    public static readonly Color SelectionText = Color.FromArgb(15, 23, 42);

    // Neutrals
    public static readonly Color Background = Color.FromArgb(248, 250, 252);
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceAlt = Color.FromArgb(248, 250, 252);
    public static readonly Color Border = Color.FromArgb(226, 232, 240);
    public static readonly Color TextPrimary = Color.FromArgb(30, 41, 59);
    public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);

    // Semantic Status Colors
    public static readonly Color Success = Color.FromArgb(27, 135, 63);
    public static readonly Color Warning = Color.FromArgb(178, 106, 0);
    public static readonly Color Danger = Color.FromArgb(198, 40, 40);
    public static readonly Color Info = Color.FromArgb(2, 132, 199);

    // Typography
    public static Font Body => AppFont.CreateFont(10F, FontStyle.Regular);
    public static Font BodyBold => AppFont.CreateFont(10F, FontStyle.Bold);
    public static Font Section => AppFont.CreateFont(12F, FontStyle.Bold);
    public static Font Title => AppFont.CreateFont(14F, FontStyle.Bold);
    public static Font Caption => AppFont.CreateFont(8.5F, FontStyle.Regular);

    // Spacing (4 px grid)
    public const int S1 = 4;
    public const int S2 = 8;
    public const int S3 = 12;
    public const int S4 = 16;
    public const int S5 = 24;
    public const int S6 = 32;
    public const int ControlHeight = 36;

    /// <summary>
    /// Applies the standard modern DataGridView styling per winforms-ui-design skill §6.
    /// Sets flat headers, comfortable 40px row height, soft borders, light emerald selection,
    /// and activates double-buffering via reflection to eliminate flickering.
    /// </summary>
    public static void ApplyGridStyle(DataGridView g)
    {
        g.AutoGenerateColumns = false;
        g.BorderStyle = BorderStyle.None;
        g.BackgroundColor = Surface;
        g.GridColor = Border;
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        g.RowHeadersVisible = false;
        g.AllowUserToAddRows = false;
        g.AllowUserToDeleteRows = false;
        g.AllowUserToResizeRows = false;
        g.ReadOnly = true;
        g.MultiSelect = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        g.EnableHeadersVisualStyles = false;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        g.ColumnHeadersHeight = 44;
        g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
        g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
        g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(71, 85, 105);
        g.ColumnHeadersDefaultCellStyle.Font = AppFont.CreateFont(10F, FontStyle.Bold);
        g.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 12, 0);

        g.RowTemplate.Height = 40;
        g.DefaultCellStyle.BackColor = Surface;
        g.DefaultCellStyle.ForeColor = TextPrimary;
        g.DefaultCellStyle.SelectionBackColor = Selection;
        g.DefaultCellStyle.SelectionForeColor = SelectionText;
        g.DefaultCellStyle.Font = AppFont.CreateFont(9.5F, FontStyle.Regular);
        g.DefaultCellStyle.Padding = new Padding(12, 0, 12, 0);
        g.AlternatingRowsDefaultCellStyle.BackColor = SurfaceAlt;

        // Reduce flicker on large grids via protected DoubleBuffered property
        typeof(DataGridView)
            .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(g, true, null);
    }

    /// <summary>
    /// Applies primary accent button styling (Emerald Green, Flat, Hand cursor).
    /// </summary>
    public static void ApplyPrimaryButtonStyle(Button b)
    {
        b.BackColor = Primary;
        b.ForeColor = OnPrimary;
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.Cursor = Cursors.Hand;
        b.Font = AppFont.CreateFont(10F, FontStyle.Bold);
    }

    /// <summary>
    /// Applies secondary button styling (Surface / Slate with border, Hand cursor).
    /// </summary>
    public static void ApplySecondaryButtonStyle(Button b)
    {
        b.BackColor = Color.FromArgb(241, 245, 249);
        b.ForeColor = TextPrimary;
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        b.Cursor = Cursors.Hand;
        b.Font = AppFont.CreateFont(9.5F, FontStyle.Regular);
    }
}
