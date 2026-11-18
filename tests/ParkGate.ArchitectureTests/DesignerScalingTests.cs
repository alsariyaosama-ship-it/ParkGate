using System.Globalization;
using System.Text.RegularExpressions;

namespace ParkGate.ArchitectureTests;

/// <summary>
/// R4.11: with <c>AutoScaleMode.Dpi</c> the designer must store the DPI (for example 96 x 96), not font metrics
/// (for example 7 x 17). A mismatch makes WinForms multiply every size by dpi / 17 when the control is created.
/// </summary>
public sealed class DesignerScalingTests
{
    private static readonly Regex Dimensions = new(
        @"AutoScaleDimensions\s*=\s*new\s+(?:System\.Drawing\.)?SizeF\(\s*(?<w>[\d.]+)F\s*,\s*(?<h>[\d.]+)F\s*\)",
        RegexOptions.Compiled);

    private static readonly Regex DpiMode = new(@"AutoScaleMode\s*=\s*(?:System\.Windows\.Forms\.)?AutoScaleMode\.Dpi\b", RegexOptions.Compiled);

    [Fact]
    public void Designers_UsingDpiScaling_StoreEqualDpiDimensions()
    {
        var root = FindRepoRoot();
        var offenders = new List<string>();

        foreach (var file in Directory.EnumerateFiles(
                     Path.Combine(root, "src", "ParkGate.WinForms"), "*.Designer.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            {
                continue;
            }

            var text = File.ReadAllText(file);
            if (!DpiMode.IsMatch(text))
            {
                continue;
            }

            var match = Dimensions.Match(text);
            if (!match.Success)
            {
                offenders.Add($"{Path.GetRelativePath(root, file)}: AutoScaleDimensions is missing");
                continue;
            }

            var width = double.Parse(match.Groups["w"].Value, CultureInfo.InvariantCulture);
            var height = double.Parse(match.Groups["h"].Value, CultureInfo.InvariantCulture);
            if (width != height)
            {
                offenders.Add($"{Path.GetRelativePath(root, file)}: {width} x {height}");
            }
        }

        offenders.ShouldBeEmpty("Use AutoScaleDimensions = new SizeF(96F, 96F) with AutoScaleMode.Dpi.");
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ParkGate.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("ParkGate.sln not found above the test output.");
    }
}
