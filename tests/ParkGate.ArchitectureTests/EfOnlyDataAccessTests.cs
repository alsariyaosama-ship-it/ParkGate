using System.Text.RegularExpressions;

namespace ParkGate.ArchitectureTests;

/// <summary>
/// R6.14 / ADR-025: all data access goes through EF Core LINQ. SQL text is allowed only in the backup service
/// (EF cannot express <c>BACKUP DATABASE</c>) and in migrations pushed before the rule.
/// </summary>
public sealed class EfOnlyDataAccessTests
{
    /// <summary>Migrations up to this timestamp were pushed before ADR-025 and may not be edited.</summary>
    private const string LastMigrationBeforeRule = "20261113205308";

    private static readonly Regex RawSql = new(
        @"\b(FromSql\w*|SqlQuery\w*|ExecuteSql\w*|SqlCommand|SqlConnection|DbCommand|CommandText)\b|migrationBuilder\.Sql\(",
        RegexOptions.Compiled);

    private static readonly Regex MigrationFile = new(@"^(?<stamp>\d{14})_", RegexOptions.Compiled);

    [Fact]
    public void SourceCode_UsesNoSqlText_ExceptBackupAndOldMigrations()
    {
        var root = FindRepoRoot();
        var offenders = Directory
            .EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Where(f => !IsExempt(f))
            .SelectMany(f => File.ReadAllLines(f)
                .Select((line, index) => (File: f, Line: index + 1, Text: line.Trim()))
                .Where(l => !l.Text.StartsWith("//", StringComparison.Ordinal) &&
                            !l.Text.StartsWith("///", StringComparison.Ordinal) &&
                            RawSql.IsMatch(l.Text)))
            .Select(l => $"{Path.GetRelativePath(root, l.File)}:{l.Line}")
            .ToList();

        offenders.ShouldBeEmpty("Use EF LINQ and migrations (ADR-025); only DatabaseBackupService may send SQL text.");
    }

    private static bool IsExempt(string path)
    {
        var name = Path.GetFileName(path);
        if (name == "DatabaseBackupService.cs")
        {
            return true;
        }

        var match = MigrationFile.Match(name);
        return match.Success && string.CompareOrdinal(match.Groups["stamp"].Value, LastMigrationBeforeRule) <= 0;
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
