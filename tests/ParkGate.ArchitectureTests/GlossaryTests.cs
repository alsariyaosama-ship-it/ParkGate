using System.Text.RegularExpressions;

namespace ParkGate.ArchitectureTests;

/// <summary>
/// R7.1 (Q-R4, Q-R4b defaults): one word per concept in everything the user reads. The card holder is «الزائر»
/// («الزوار»), the cooldown is «فترة الانتظار», the desk is «الشباك», the gift slip is «قسيمة».
/// </summary>
public sealed class GlossaryTests
{
    // Arabic letters around a match mean it is part of another word (for example «الوصول» is not «وصل»).
    private const string NotLetter = "(?<![ء-ي])";
    private const string NotLetterAfter = "(?![ء-ي])";

    private static readonly Regex Retired = new(
        string.Join("|",
            "زبون", "زبائن", "مستفيد",
            "فترة التهدئة", "فترة السماح", "فترة سماح",
            "محطة", "محطات", "بوابة الدخول",
            "كوبون", "وصولات", "إيصال", "ايصال",
            $"{NotLetter}(?:ال|ب|و|ل)?وصل{NotLetterAfter}"),
        RegexOptions.Compiled);

    public static TheoryData<string> UiFiles()
    {
        var root = FindRepoRoot();
        var data = new TheoryData<string>
        {
            Path.Combine("src", "ParkGate.WinForms", "Resources", "Strings.resx"),
            Path.Combine("docs", "USER_GUIDE_AR.md"),
            Path.Combine("docs", "TEST_SCRIPT.md"),
        };
        foreach (var file in Directory.EnumerateFiles(
                     Path.Combine(root, "src", "ParkGate.WinForms"), "*.Designer.cs", SearchOption.AllDirectories)
                 .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                             !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                             !f.EndsWith("Strings.Designer.cs", StringComparison.Ordinal)))
        {
            data.Add(Path.GetRelativePath(root, file));
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(UiFiles))]
    public void UiText_UsesOnlyGlossaryTerms(string relativePath)
    {
        var lines = File.ReadAllLines(Path.Combine(FindRepoRoot(), relativePath));
        var offenders = lines
            .Select((line, index) => (Line: index + 1, Match: Retired.Match(line)))
            .Where(x => x.Match.Success)
            .Select(x => $"{relativePath}:{x.Line} «{x.Match.Value}»")
            .ToList();

        offenders.ShouldBeEmpty();
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
