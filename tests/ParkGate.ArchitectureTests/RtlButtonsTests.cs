using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Controls;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>
/// R5.13: at 125 % buttons and check boxes cut the last letters of Arabic text («عرض التقرير» → «عرض التقر»), even
/// when wide enough. The fault shows only on a real 125 % screen (checked there), not at the test host's 96 dpi, so
/// these tests guard the designers and the text layout.
/// </summary>
public sealed partial class RtlButtonsTests
{
    [Fact]
    public void Designers_UseTheRtlButtonControls()
    {
        var root = FindRepoRoot();
        var offenders = Directory.EnumerateFiles(Path.Combine(root, "src", "ParkGate.WinForms"), "*.Designer.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .SelectMany(f => PlainButton().Matches(File.ReadAllText(f)).Select(m => $"{Path.GetRelativePath(root, f)}: {m.Value}"))
            .ToList();

        offenders.ShouldBeEmpty("Use RtlButton / RtlCheckBox / RtlRadioButton: the WinForms ones cut Arabic text.");
    }

    [Fact]
    public void Designers_UseRtlTabControl()
    {
        var root = FindRepoRoot();
        var offenders = Directory.EnumerateFiles(Path.Combine(root, "src", "ParkGate.WinForms"), "*.Designer.cs", SearchOption.AllDirectories)
            .Where(f => Regex.IsMatch(File.ReadAllText(f), @"new (System\.Windows\.Forms\.)?TabControl\(\)"))
            .Select(f => Path.GetRelativePath(root, f))
            .ToList();

        offenders.ShouldBeEmpty("Use RtlTabControl: native tabs cut Arabic tab text.");
    }

    [Fact]
    public void Tabs_AreAsWideAsTheLongestText()
    {
        using var tabs = new RtlTabControl { Font = AppFont.CreateFont(10F, FontStyle.Regular) };
        tabs.TabPages.Add("الملخص اليومي");
        tabs.TabPages.Add("المرفوضات والاستثناءات");
        var longest = TextRenderer.MeasureText("المرفوضات والاستثناءات", tabs.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;

        tabs.FitTabsToText();

        tabs.SizeMode.ShouldBe(TabSizeMode.Fixed);
        tabs.ItemSize.Width.ShouldBeGreaterThan(longest);
    }

    [Fact]
    public void Text_OutsidePainting_IsTheRealText()
    {
        using var button = new RtlButton { Text = "عرض التقرير" };
        using var check = new RtlCheckBox { Text = "بيانات الزائر" };

        button.Text.ShouldBe("عرض التقرير");
        check.Text.ShouldBe("بيانات الزائر");
    }

    [Fact]
    public void AutoSizeCheckBox_LeavesRoomForTheWholeTextBesideTheGlyph()
    {
        using var check = new RtlCheckBox { Text = "تحديد فترة انتظار خاصة بهذه الجهة", AutoSize = true, Font = AppFont.CreateFont(10F, FontStyle.Regular), RightToLeft = RightToLeft.Yes };
        var needed = TextRenderer.MeasureText(check.Text, check.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;

        check.Size = check.GetPreferredSize(Size.Empty);

        check.TextBounds.Width.ShouldBeGreaterThanOrEqualTo(needed);
    }

    [Theory]
    [InlineData(RightToLeft.Yes, true)]
    [InlineData(RightToLeft.No, false)]
    public void CheckBoxText_SitsOnTheSideAwayFromTheGlyph(RightToLeft rtl, bool textOnTheLeft)
    {
        using var check = new RtlCheckBox { Text = "x", RightToLeft = rtl, Size = new Size(200, 30) };

        var bounds = check.TextBounds;

        (bounds.Left == 0).ShouldBe(textOnTheLeft);
        (bounds.Right == check.ClientSize.Width).ShouldBe(!textOnTheLeft);
    }

    [Fact]
    public void ButtonText_StaysInsideTheBorderAndPadding()
    {
        using var button = new RtlButton { Size = new Size(150, 40), Padding = new Padding(5) };
        Theme.ApplyPrimaryButtonStyle(button);

        var bounds = button.TextBounds;

        bounds.Left.ShouldBeGreaterThanOrEqualTo(5);
        bounds.Right.ShouldBeLessThanOrEqualTo(145);
        bounds.Width.ShouldBeGreaterThan(100);
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

    [GeneratedRegex(@"new (System\.Windows\.Forms\.)?(Button|CheckBox|RadioButton)\(\)|(private|internal|protected) (System\.Windows\.Forms\.)?(Button|CheckBox|RadioButton) _")]
    private static partial Regex PlainButton();
}
