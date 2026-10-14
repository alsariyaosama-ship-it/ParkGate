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
