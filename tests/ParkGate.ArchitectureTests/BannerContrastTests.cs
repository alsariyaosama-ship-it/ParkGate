using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ParkGate.WinForms.Controls;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>R5.9: banner text must reach 4.5:1 (WCAG AA) on every state colour.</summary>
public sealed class BannerContrastTests
{
    [Theory]
    [InlineData(BannerState.Granted)]
    [InlineData(BannerState.Denied)]
    [InlineData(BannerState.Warning)]
    [InlineData(BannerState.Neutral)]
    public void BannerText_HasAtLeastAA_Contrast(BannerState state)
    {
        using var banner = new StatusBanner();
        banner.SetStatus(state, "x", "y");

        foreach (var name in new[] { "_lblTitle", "_lblDetails" })
        {
            var label = (Label)typeof(StatusBanner).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(banner)!;
            Contrast(label.ForeColor, banner.BackColor).ShouldBeGreaterThanOrEqualTo(4.5, $"{name} on {state}");
        }
    }

    [Fact]
    public void Warning_IsTheDarkerAmber()
    {
        StatusBanner.ColorWarning.ShouldBe(Color.FromArgb(154, 91, 0)); // #9A5B00, AGENTS.md §5.10
    }

    private static double Contrast(Color a, Color b)
    {
        var (l1, l2) = (Luminance(a), Luminance(b));
        return (Math.Max(l1, l2) + 0.05) / (Math.Min(l1, l2) + 0.05);
    }

    private static double Luminance(Color c)
    {
        static double Channel(int v)
        {
            var s = v / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(c.R)) + (0.7152 * Channel(c.G)) + (0.0722 * Channel(c.B));
    }
}
