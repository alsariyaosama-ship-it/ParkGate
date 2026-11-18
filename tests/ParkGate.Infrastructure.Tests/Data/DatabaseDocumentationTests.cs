using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Tests.Data;

/// <summary>
/// <c>docs/DATABASE.md</c> is the agreed data model (AGENTS.md §7). These tests fail when the EF model and the
/// document drift apart (R6.4). Sections and rows marked "proposed" describe approved designs not built yet.
/// </summary>
public sealed partial class DatabaseDocumentationTests
{
    private static readonly string[] AuditColumns = ["CreatedAtUtc", "CreatedByUserId", "UpdatedAtUtc", "UpdatedByUserId"];

    private static readonly Lazy<string> Document = new(() => File.ReadAllText(Path.Combine(FindRepositoryRoot(), "docs", "DATABASE.md")));

    private static readonly Lazy<IModel> Model = new(() =>
    {
        var options = new DbContextOptionsBuilder<ParkGateDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ParkGate_Dummy;Trusted_Connection=True;")
            .Options;
        using var context = new ParkGateDbContext(options);
        return context.GetService<IDesignTimeModel>().Model;
    });

    [Fact]
    public void DocumentedTables_ShouldMatchTheEfModel()
    {
        var documented = DocumentedTables().Keys.Order().ToList();
        var model = ModelTables().Keys.Order().ToList();

        documented.ShouldBe(model);
    }

    [Fact]
    public void DocumentedColumns_ShouldMatchTheEfModel()
    {
        var documented = DocumentedTables();
        var mismatches = new List<string>();

        foreach (var (table, columns) in ModelTables())
        {
            var doc = documented.GetValueOrDefault(table, []);
            mismatches.AddRange(columns.Except(doc).Select(c => $"{table}.{c} is in the model but not documented"));
            mismatches.AddRange(doc.Except(columns).Select(c => $"{table}.{c} is documented but not in the model"));
        }

        mismatches.ShouldBeEmpty();
    }

    [Fact]
    public void EveryCheckConstraint_ShouldBeDocumentedByName()
    {
        var missing = Model.Value.GetEntityTypes()
            .SelectMany(e => e.GetCheckConstraints())
            .Select(c => c.ModelName)
            .Where(name => !Document.Value.Contains(name, StringComparison.Ordinal))
            .ToList();

        missing.ShouldBeEmpty();
    }

    [Fact]
    public void EverySeededSetting_ShouldBeDocumentedInTheSettingsSection()
    {
        var section = SettingsSection();
        var missing = Model.Value.FindEntityType(typeof(AppSetting))!.GetSeedData()
            .Select(row => (string)row[nameof(AppSetting.Key)]!)
            .Where(key => !section.Contains($"`{key}`", StringComparison.Ordinal))
            .ToList();

        missing.ShouldBeEmpty();
    }

    private static Dictionary<string, HashSet<string>> ModelTables() =>
        Model.Value.GetEntityTypes()
            .Where(e => e.GetTableName() is not null && !e.IsOwned())
            .GroupBy(e => e.GetTableName()!)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(e => e.GetProperties()
                        .Select(p => p.GetColumnName(StoreObjectIdentifier.Table(g.Key, e.GetSchema()))))
                    .OfType<string>()
                    .ToHashSet());

    /// <summary>Tables of §3: a <c>### 3.x `Name`</c> heading followed by a Column | Type | Notes table.</summary>
    private static Dictionary<string, HashSet<string>> DocumentedTables()
    {
        var tables = new Dictionary<string, HashSet<string>>();
        HashSet<string>? current = null;

        foreach (var line in Document.Value.Split('\n').Select(l => l.TrimEnd('\r')))
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                current = null;
                continue;
            }

            var heading = TableHeading().Match(line);
            if (heading.Success)
            {
                current = line.Contains("proposed", StringComparison.OrdinalIgnoreCase) ? null : [];
                if (current is not null)
                {
                    tables[heading.Groups["name"].Value] = current;
                }

                continue;
            }

            if (current is null || !line.StartsWith('|') || line.StartsWith("|---", StringComparison.Ordinal))
            {
                continue;
            }

            var cells = line.Split('|');
            var first = cells[1].Trim();
            if (first == "Column" || cells[^2].Contains("proposed", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var name in first.Split(',').Select(n => n.Trim()))
            {
                if (name == "audit columns")
                {
                    current.UnionWith(AuditColumns);
                }
                else
                {
                    current.Add(name);
                }
            }
        }

        return tables;
    }

    private static string SettingsSection()
    {
        var text = Document.Value;
        var start = text.IndexOf("## 5. Settings keys", StringComparison.Ordinal);
        var end = text.IndexOf("\n## 6.", start, StringComparison.Ordinal);
        return text[start..end];
    }

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ParkGate.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("ParkGate.sln not found above the test folder.");
    }

    [GeneratedRegex(@"^### 3\.\d+ `(?<name>\w+)`")]
    private static partial Regex TableHeading();
}
