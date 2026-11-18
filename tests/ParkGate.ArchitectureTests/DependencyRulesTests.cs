using System.Reflection;

namespace ParkGate.ArchitectureTests;

public sealed class DependencyRulesTests
{
    private static readonly Assembly DomainAssembly = typeof(Domain.AssemblyReference).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.AssemblyReference).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.AssemblyReference).Assembly;
    private static readonly Assembly WinFormsAssembly = typeof(WinForms.AssemblyReference).Assembly;

    [Fact]
    public void Domain_ShouldNotReferenceAnyOtherProject()
    {
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies();

        referencedAssemblies.ShouldNotContain(a => a.Name == "ParkGate.Application");
        referencedAssemblies.ShouldNotContain(a => a.Name == "ParkGate.Infrastructure");
        referencedAssemblies.ShouldNotContain(a => a.Name == "ParkGate.WinForms");
    }

    [Fact]
    public void Domain_ShouldNotReferenceThirdPartyPackages()
    {
        var referencedAssemblies = DomainAssembly.GetReferencedAssemblies();

        referencedAssemblies.ShouldNotContain(a => a.Name != null && a.Name.StartsWith("Microsoft.EntityFrameworkCore"));
        referencedAssemblies.ShouldNotContain(a => a.Name != null && a.Name.StartsWith("FluentValidation"));
        referencedAssemblies.ShouldNotContain(a => a.Name != null && a.Name.StartsWith("Microsoft.Data.SqlClient"));
        referencedAssemblies.ShouldNotContain(a => a.Name != null && a.Name.StartsWith("System.Data.SqlClient"));
        referencedAssemblies.ShouldNotContain(a => a.Name != null && a.Name.StartsWith("System.Windows.Forms"));
    }

    [Fact]
    public void Application_ShouldNotReferenceInfrastructureOrWinForms()
    {
        var referencedAssemblies = ApplicationAssembly.GetReferencedAssemblies();

        referencedAssemblies.ShouldNotContain(a => a.Name == "ParkGate.Infrastructure");
        referencedAssemblies.ShouldNotContain(a => a.Name == "ParkGate.WinForms");
    }

    [Fact]
    public void Application_ShouldNotReferenceForbiddenTechnologies()
    {
        var referencedAssemblies = ApplicationAssembly.GetReferencedAssemblies();

        referencedAssemblies.ShouldNotContain(a => a.Name != null && a.Name.StartsWith("Microsoft.EntityFrameworkCore"));
        referencedAssemblies.ShouldNotContain(a => a.Name != null && a.Name.StartsWith("Microsoft.Data.SqlClient"));
        referencedAssemblies.ShouldNotContain(a => a.Name != null && a.Name.StartsWith("System.Data.SqlClient"));
        referencedAssemblies.ShouldNotContain(a => a.Name != null && a.Name.StartsWith("System.Windows.Forms"));
        referencedAssemblies.ShouldNotContain(a => a.Name != null && a.Name.StartsWith("System.Drawing"));
    }

    [Fact]
    public void Infrastructure_ShouldNotReferenceWinForms()
    {
        var referencedAssemblies = InfrastructureAssembly.GetReferencedAssemblies();

        referencedAssemblies.ShouldNotContain(a => a.Name == "ParkGate.WinForms");
        referencedAssemblies.ShouldNotContain(a => a.Name != null && a.Name.StartsWith("System.Windows.Forms"));
    }

    [Fact]
    public void WinForms_FormsAndControls_ShouldNotDirectlyReferenceEFOrInfrastructure()
    {
        var nonCompositionTypes = WinFormsAssembly.GetTypes()
            .Where(t => t.FullName != null &&
                        !t.FullName.EndsWith(".Program") &&
                        !t.FullName.Contains("DependencyInjection"));

        foreach (var type in nonCompositionTypes)
        {
            var fieldTypes = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Select(f => f.FieldType);

            var propTypes = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Select(p => p.PropertyType);

            var ctorParamTypes = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .SelectMany(c => c.GetParameters().Select(p => p.ParameterType));

            var methodParamTypes = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .SelectMany(m => m.GetParameters().Select(p => p.ParameterType));

            var methodReturnTypes = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Select(m => m.ReturnType);

            var allTypes = fieldTypes
                .Concat(propTypes)
                .Concat(ctorParamTypes)
                .Concat(methodParamTypes)
                .Concat(methodReturnTypes);

            foreach (var usedType in allTypes)
            {
                var ns = usedType.Namespace ?? string.Empty;
                ns.StartsWith("Microsoft.EntityFrameworkCore")
                    .ShouldBeFalse($"Type '{type.FullName}' should not use EF Core type '{usedType.FullName}'.");
                ns.StartsWith("ParkGate.Infrastructure")
                    .ShouldBeFalse($"Type '{type.FullName}' should not directly use Infrastructure type '{usedType.FullName}'.");
            }
        }
    }

    [Fact]
    public void WinFormsSource_OnlyCompositionRootMentionsInfrastructure()
    {
        // R7.6 / AGENTS.md §3: reflection above misses static calls inside method bodies, so scan the source too.
        var root = FindRepoRoot();
        var winForms = Path.Combine(root, "src", "ParkGate.WinForms");
        string[] compositionRoot = ["Program.cs", "DependencyInjection.cs"];
        var offenders = Directory
            .EnumerateFiles(winForms, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Where(f => !compositionRoot.Contains(Path.GetRelativePath(winForms, f)))
            .Where(f => File.ReadAllText(f).Contains("ParkGate.Infrastructure", StringComparison.Ordinal))
            .Select(f => Path.GetRelativePath(root, f))
            .ToList();

        offenders.ShouldBeEmpty("Use an Application port; only the composition root may reference Infrastructure.");
    }

    [Fact]
    public void ProgramMain_IsSynchronousAndSta()
    {
        // R7.11: [STAThread] is ignored on an async Main; after the first await the UI ran on an MTA
        // thread-pool thread, and SaveFileDialog/PrintDialog threw ThreadStateException.
        var main = WinFormsAssembly.GetType("ParkGate.WinForms.Program", throwOnError: true)!
            .GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)!;

        main.ReturnType.ShouldBe(typeof(void));
        main.GetCustomAttribute<STAThreadAttribute>().ShouldNotBeNull();
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

    [Fact]
    public void Solution_ShouldNotExceedApprovedProductionProjects()
    {
        // Enforce maximum 5 production projects approved by ADR-018 (Domain, Application, Infrastructure, WinForms, SyncAgent)
        var srcDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src"));
        if (Directory.Exists(srcDir))
        {
            var projectFiles = Directory.GetFiles(srcDir, "*.csproj", SearchOption.AllDirectories);
            projectFiles.Length.ShouldBeLessThanOrEqualTo(5);

            var allowedProjectNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ParkGate.Domain.csproj",
                "ParkGate.Application.csproj",
                "ParkGate.Infrastructure.csproj",
                "ParkGate.WinForms.csproj",
                "ParkGate.SyncAgent.csproj"
            };

            foreach (var file in projectFiles)
            {
                allowedProjectNames.ShouldContain(Path.GetFileName(file));
            }
        }
    }
}
