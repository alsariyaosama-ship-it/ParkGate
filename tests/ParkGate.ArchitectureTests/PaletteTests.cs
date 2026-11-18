using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using ParkGate.WinForms.Common;

namespace ParkGate.ArchitectureTests;

/// <summary>
/// R7.3: one palette. Code names Theme colors; designer files (which cannot reference Theme) may only use
/// values that Theme defines. StatusBanner keeps its four AGENTS.md §5.10 colors, which Theme mirrors.
/// </summary>
public sealed class PaletteTests
{
    private static readonly Regex Literal = new(
        @"Color\.FromArgb\(\s*(?:\(\(int\)\(\(\(byte\)\()?(?<r>\d+)(?:\)\)\)\))?\s*,\s*(?:\(\(int\)\(\(\(byte\)\()?(?<g>\d+)(?:\)\)\)\))?\s*,\s*(?:\(\(int\)\(\(\(byte\)\()?(?<b>\d+)(?:\)\)\)\))?\s*\)",
        RegexOptions.Compiled);

    [Fact]
    public void Code_UsesThemeColorsOnly()
    {
        var root = FindRepoRoot();
        string[] allowed = ["Theme.cs", "StatusBanner.cs"];
        var offenders = SourceFiles(root)
            .Where(f => !f.EndsWith(".Designer.cs", StringComparison.Ordinal) && !allowed.Contains(Path.GetFileName(f)))
            .SelectMany(f => Literal.Matches(File.ReadAllText(f)).Select(m => $"{Path.GetRelativePath(root, f)}: {Hex(m)}"))
            .ToList();

        offenders.ShouldBeEmpty("Use a Theme color instead of Color.FromArgb.");
    }

    [Fact]
    public void Designers_UseThemeValuesOnly()
    {
        var palette = typeof(Theme).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(Color))
            .Select(f => ((Color)f.GetValue(null)!).ToArgb() & 0xFFFFFF)
            .ToHashSet();
        var root = FindRepoRoot();
        var offenders = SourceFiles(root)
            .Where(f => f.EndsWith(".Designer.cs", StringComparison.Ordinal))
            .SelectMany(f => Literal.Matches(File.ReadAllText(f))
                .Where(m => !palette.Contains((int.Parse(m.Groups["r"].Value) << 16) | (int.Parse(m.Groups["g"].Value) << 8) | int.Parse(m.Groups["b"].Value)))
                .Select(m => $"{Path.GetRelativePath(root, f)}: {Hex(m)}"))
            .ToList();

        offenders.ShouldBeEmpty("Designer colors must be values defined in Theme.");
    }

    private static string Hex(Match m) =>
        $"#{int.Parse(m.Groups["r"].Value):X2}{int.Parse(m.Groups["g"].Value):X2}{int.Parse(m.Groups["b"].Value):X2}";

    private static IEnumerable<string> SourceFiles(string root) =>
        Directory.EnumerateFiles(Path.Combine(root, "src", "ParkGate.WinForms"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

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
