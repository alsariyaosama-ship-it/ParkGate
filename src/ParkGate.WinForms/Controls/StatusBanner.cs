using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Controls;

public enum BannerState
{
    Neutral = 0,
    Granted = 1,
    Denied = 2,
    Warning = 3
}

/// <summary>
/// Status banner control conforming to SPEC §5.3 zone 3 and AGENTS.md §5.10:
/// Granted (#1B873F), Denied (#C62828), Warning (#B26A00), Neutral (#455A64).
/// Text size >= 20 pt bold with clear Arabic wording.
/// </summary>
public partial class StatusBanner : UserControl
{
    public static readonly Color ColorGranted = Color.FromArgb(27, 135, 63);   // #1B873F
    public static readonly Color ColorDenied = Color.FromArgb(198, 40, 40);    // #C62828
    public static readonly Color ColorWarning = Color.FromArgb(178, 106, 0);   // #B26A00
    public static readonly Color ColorNeutral = Color.FromArgb(69, 90, 100);   // #455A64

    private BannerState _state = BannerState.Neutral;

    public StatusBanner()
    {
        InitializeComponent();
        Reset();
    }

    [Browsable(true)]
    [Category("Appearance")]
    public BannerState State => _state;

    [Browsable(true)]
    [Category("Appearance")]
    public string Title => _lblTitle.Text;

    [Browsable(true)]
    [Category("Appearance")]
    public string? Details => _lblDetails.Text;

    public void SetStatus(BannerState state, string message, string? details = null)
    {
        _state = state;
        _lblTitle.Text = message;

        if (string.IsNullOrWhiteSpace(details))
        {
            _lblDetails.Text = string.Empty;
            _lblDetails.Visible = false;
            _layoutPanel.RowStyles[0].Height = 100F;
            _layoutPanel.RowStyles[1].Height = 0F;
        }
        else
        {
            _lblDetails.Text = details;
            _lblDetails.Visible = true;
            _layoutPanel.RowStyles[0].Height = 58F;
            _layoutPanel.RowStyles[1].Height = 42F;
        }

        BackColor = GetStateColor(state);
    }

    public void SetGranted(string? message = null, string? details = null)
    {
        SetStatus(BannerState.Granted, message ?? Strings.Banner_Granted, details);
    }

    public void SetDenied(string? message = null, string? details = null)
    {
        SetStatus(BannerState.Denied, message ?? Strings.Banner_Denied, details);
    }

    public void SetWarning(string? message = null, string? details = null)
    {
        SetStatus(BannerState.Warning, message ?? Strings.Banner_Warning, details);
    }

    public void Reset(string? message = null)
    {
        SetStatus(BannerState.Neutral, message ?? Strings.Banner_Neutral, null);
    }

    public static Color GetStateColor(BannerState state) => state switch
    {
        BannerState.Granted => ColorGranted,
        BannerState.Denied => ColorDenied,
        BannerState.Warning => ColorWarning,
        _ => ColorNeutral
    };
}
