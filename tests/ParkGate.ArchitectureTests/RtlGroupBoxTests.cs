using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Controls;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>R5.12: WinForms cut the last letters of Arabic group box captions in right-to-left layout.</summary>
public sealed partial class RtlGroupBoxTests
{
    [Theory]
    [InlineData("بيانات الزائر")]
    [InlineData("المرافقون مع الزائر")]
    [InlineData("سجل الدخول لهذا اليوم بهذا الشباك (آخر 20 محاولة)")]
    public void Caption_IsDrawnInFull(string caption)
    {
        using var box = new RtlGroupBox
        {
            RightToLeft = RightToLeft.Yes,
            Font = AppFont.CreateFont(10F, FontStyle.Bold),
            BackColor = Color.White,
            ForeColor = Color.Black,
            Size = new Size(700, 80),
            Text = caption,
        };
        var needed = TextRenderer.MeasureText(caption, box.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.RightToLeft).Width;

        var (left, right) = CaptionInk(box);

        (right - left + 1).ShouldBeGreaterThanOrEqualTo(needed - 4, "the caption lost letters");
        right.ShouldBeGreaterThan(box.Width / 2, "an RTL caption starts on the right");
    }

    [Fact]
    public void Caption_LeftToRight_StartsOnTheLeft()
    {
        using var box = new RtlGroupBox
        {
            RightToLeft = RightToLeft.No,
            Font = AppFont.CreateFont(10F, FontStyle.Bold),
            BackColor = Color.White,
            ForeColor = Color.Black,
            Size = new Size(700, 80),
            Text = "Today",
        };

        CaptionInk(box).Left.ShouldBeLessThan(box.Width / 2);
    }

    [Fact]
    public void NoCaption_KeepsNoSpaceForOne()
    {
        using var withCaption = new RtlGroupBox { Size = new Size(300, 80), Padding = new Padding(4), Text = "بيانات الزائر", Font = AppFont.CreateFont(10F, FontStyle.Bold) };
        using var without = new RtlGroupBox { Size = new Size(300, 80), Padding = new Padding(4), Font = AppFont.CreateFont(10F, FontStyle.Bold) };

        without.DisplayRectangle.Top.ShouldBe(5);
        without.DisplayRectangle.Height.ShouldBe(80 - 8 - 2);
        withCaption.DisplayRectangle.Top.ShouldBeGreaterThan(without.DisplayRectangle.Top);
    }

    [Fact]
    public void Designers_UseRtlGroupBox()
    {
        var root = FindRepoRoot();
        var offenders = Directory.EnumerateFiles(Path.Combine(root, "src", "ParkGate.WinForms"), "*.Designer.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Where(f => PlainGroupBox().IsMatch(File.ReadAllText(f)))
            .Select(f => Path.GetRelativePath(root, f))
            .ToList();

        offenders.ShouldBeEmpty("Use ParkGate.WinForms.Controls.RtlGroupBox: the WinForms group box cuts Arabic captions.");
    }

    /// <summary>Leftmost and rightmost dark columns in the caption band.</summary>
    private static (int Left, int Right) CaptionInk(RtlGroupBox box)
    {
        using var bmp = new Bitmap(box.Width, box.Height);
        box.DrawToBitmap(bmp, new Rectangle(Point.Empty, box.Size));
        var band = box.Font.Height;
        int left = int.MaxValue, right = -1;
        for (var x = 0; x < bmp.Width; x++)
        {
            for (var y = 0; y < band; y++)
            {
                var p = bmp.GetPixel(x, y);
                if (p.R < 110 && p.G < 110 && p.B < 110)
                {
                    left = Math.Min(left, x);
                    right = Math.Max(right, x);
                    break;
                }
            }
        }

        right.ShouldBeGreaterThan(-1, "no caption was drawn");
        return (left, right);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ParkGate.sln")))
        {
            dir = dir.Parent;
        }

        dir.ShouldNotBeNull();
        return dir.FullName;
    }

    [GeneratedRegex(@"new System\.Windows\.Forms\.GroupBox\(\)|System\.Windows\.Forms\.GroupBox\s+_")]
    private static partial Regex PlainGroupBox();
}
