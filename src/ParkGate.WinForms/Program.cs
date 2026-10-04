using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParkGate.WinForms.Services;
using Serilog;
using WinFormsApp = System.Windows.Forms.Application;

namespace ParkGate.WinForms;

internal static class Program
{
    private static IHost? _host;

    [STAThread]
    private static void Main()
    {
        // 1. High DPI initialization
        ApplicationConfiguration.Initialize();
        WinFormsApp.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // 2. Logging directory: %ProgramData%\ParkGate\logs
        var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var logDirectory = Path.Combine(programData, "ParkGate", "logs");
        Directory.CreateDirectory(logDirectory);

        var logPath = Path.Combine(logDirectory, "parkgate-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        // 3. Global unhandled exception handlers
        WinFormsApp.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        WinFormsApp.ThreadException += (sender, e) => HandleException("ThreadException", e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => HandleException("AppDomainUnhandledException", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            HandleException("UnobservedTaskException", e.Exception);
            e.SetObserved();
        };

        try
        {
            Log.Information("ParkGate starting up...");

            // 4. Build Generic Host
            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services, context.Configuration);
                })
                .Build();

            _host.Start();

            var formFactory = _host.Services.GetRequiredService<IFormFactory>();
            var mainForm = formFactory.Create<Form1>();

            WinFormsApp.Run(mainForm);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "ParkGate terminated unexpectedly during startup.");
            ShowFatalErrorMessage(ex);
        }
        finally
        {
            _host?.Dispose();
            Log.Information("ParkGate shutdown complete.");
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Core services
        services.AddSingleton<IFormFactory, FormFactory>();

        // Forms (Transient per AGENTS.md §4.5)
        services.AddTransient<Form1>();
    }

    private static void HandleException(string source, Exception? exception)
    {
        Log.Error(exception, "Unhandled exception captured in {Source}", source);

        MessageBox.Show(
            "حدث خطأ غير متوقع في النظام. تم تسجيل تفاصيل الخطأ في ملف السجل.",
            "خطأ في النظام",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
    }

    private static void ShowFatalErrorMessage(Exception exception)
    {
        MessageBox.Show(
            $"تعذر بدء تشغيل البرنامج:\n{exception.Message}",
            "خطأ حرج",
            MessageBoxButtons.OK,
            MessageBoxIcon.Stop,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
    }
}