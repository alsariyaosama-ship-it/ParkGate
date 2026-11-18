using System.Reflection;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParkGate.WinForms;

namespace ParkGate.ArchitectureTests;

/// <summary>
/// R7.9: a form or control that silently built itself with null services hid features when a registration was
/// missing (the DI container falls back to the parameterless constructor). Services are now required.
/// </summary>
public sealed class UiCompositionTests
{
    private static readonly NullabilityInfoContext Nullability = new();

    public static IEnumerable<Type> DesignerTypes() =>
        typeof(WinForms.AssemblyReference).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(Control).IsAssignableFrom(t) &&
                        t.GetMethod("InitializeComponent", BindingFlags.Instance | BindingFlags.NonPublic) != null)
            .OrderBy(t => t.Name);

    [Fact]
    public void FormsAndControls_HaveOneConstructor_WithRequiredServicesOnly()
    {
        var offenders = new List<string>();
        foreach (var type in DesignerTypes())
        {
            var ctors = type.GetConstructors();
            if (ctors.Length != 1)
            {
                offenders.Add($"{type.Name}: {ctors.Length} public constructors");
            }

            var methods = ctors.Cast<MethodBase>()
                .Concat(type.GetMethods().Where(m => m.Name == "InitializeServices"));
            offenders.AddRange(methods
                .SelectMany(m => m.GetParameters())
                .Where(p => !p.ParameterType.IsValueType && p.ParameterType != typeof(string) &&
                            Nullability.Create(p).WriteState == NullabilityState.Nullable)
                .Select(p => $"{type.Name}: '{p.Name}' is nullable"));
        }

        offenders.ShouldBeEmpty();
    }

    [Fact]
    public void FormsAndControlsWithServices_AreRegistered()
    {
        var services = BuildServices();
        var missing = DesignerTypes()
            .Where(t => t.GetConstructors().Any(c => c.GetParameters().Length > 0))
            .Where(t => services.All(s => s.ServiceType != t))
            .Select(t => t.Name)
            .ToList();

        missing.ShouldBeEmpty();
    }

    [Fact]
    public void Container_CanBuildEveryFormAndControl()
    {
        // Walks each window's constructor graph through the registrations without creating anything. Not
        // ValidateOnBuild: that also checks the sync agent's CloudPushService, which the desk never resolves.
        var services = BuildServices();
        var problems = new List<string>();
        foreach (var type in DesignerTypes().Where(t => services.Any(s => s.ServiceType == t)))
        {
            Walk(type, services, [], problems, type.Name);
        }

        problems.ShouldBeEmpty();
    }

    private static void Walk(Type type, ServiceCollection services, HashSet<Type> seen, List<string> problems, string path)
    {
        if (!seen.Add(type) || type == typeof(IServiceProvider) ||
            (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Microsoft.Extensions.Logging.ILogger<>)))
        {
            return;
        }

        var descriptor = services.LastOrDefault(s => s.ServiceType == type);
        if (descriptor == null)
        {
            problems.Add($"{path}: {type.Name} is not registered");
            return;
        }

        if (descriptor.ImplementationType == null)
        {
            return; // instance or factory registration
        }

        // Like the container: the longest constructor whose parameters are all registered (or optional).
        var ctor = descriptor.ImplementationType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault(c => c.GetParameters().All(p => p.HasDefaultValue || IsKnown(p.ParameterType, services)));
        if (ctor == null)
        {
            problems.Add($"{path}: no constructor of {descriptor.ImplementationType.Name} can be satisfied");
            return;
        }

        foreach (var parameter in ctor.GetParameters().Where(p => !p.HasDefaultValue))
        {
            Walk(parameter.ParameterType, services, seen, problems, $"{path} > {parameter.ParameterType.Name}");
        }
    }

    [Fact]
    public void WinFormsSource_HasNoNullServiceFallbacks()
    {
        var root = FindRepoRoot();
        var fallback = new System.Text.RegularExpressions.Regex(
            @"\?\?\s*new\s+[\w.]+(Form|Dialog|Control|Panel)\(|_formFactory\?\.");
        var offenders = Directory
            .EnumerateFiles(Path.Combine(root, "src", "ParkGate.WinForms"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Where(f => fallback.IsMatch(File.ReadAllText(f)))
            .Select(f => Path.GetRelativePath(root, f))
            .ToList();

        offenders.ShouldBeEmpty("Create forms and controls through IFormFactory; no fallback construction.");
    }

    private static bool IsKnown(Type type, ServiceCollection services) =>
        type == typeof(IServiceProvider) || services.Any(s => s.ServiceType == type) ||
        (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Microsoft.Extensions.Logging.ILogger<>));

    private static ServiceCollection BuildServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddParkGateServices(new ConfigurationBuilder().Build());
        return services;
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
