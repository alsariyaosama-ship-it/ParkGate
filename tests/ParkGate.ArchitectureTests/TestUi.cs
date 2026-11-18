using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace ParkGate.ArchitectureTests;

/// <summary>
/// R7.9: forms and controls take required services only. Tests pass the fakes they care about; every other
/// constructor parameter gets an NSubstitute fake (loggers get <see cref="NullLogger{T}"/>).
/// </summary>
internal static class TestUi
{
    public static T Create<T>(params object[] services) where T : class => (T)Create(typeof(T), services);

    public static object Create(Type type, params object[] services)
    {
        var ctor = type.GetConstructors().Single();
        var args = ctor.GetParameters().Select(p => Resolve(p.ParameterType, services)).ToArray();
        try
        {
            return ctor.Invoke(args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    /// <summary>Fakes for <see cref="WinForms.Controls.GiftOfferPanel.InitializeServices"/>.</summary>
    public static void InitializeServices(WinForms.Controls.GiftOfferPanel panel, params object[] services)
    {
        var method = typeof(WinForms.Controls.GiftOfferPanel).GetMethod(nameof(panel.InitializeServices))!;
        var args = method.GetParameters()
            .Select(p => p.ParameterType == typeof(string) ? null : Resolve(p.ParameterType, services))
            .ToArray();
        method.Invoke(panel, args);
    }

    public static WinForms.Controls.GiftOfferPanel CreateGiftPanel(params object[] services)
    {
        var panel = new WinForms.Controls.GiftOfferPanel();
        InitializeServices(panel, services);
        return panel;
    }

    private static object Resolve(Type type, object[] services)
    {
        var given = services.FirstOrDefault(type.IsInstanceOfType);
        if (given != null)
        {
            return given;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ILogger<>))
        {
            return Activator.CreateInstance(typeof(NullLogger<>).MakeGenericType(type.GetGenericArguments()))!;
        }

        return Substitute.For([type], []);
    }
}
