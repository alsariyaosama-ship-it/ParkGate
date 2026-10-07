using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Services;
using Serilog;
using WinFormsApp = System.Windows.Forms.Application;

namespace ParkGate.WinForms;

internal static class Program
{
    private static IHost? _host;

    [STAThread]
    private static async Task Main()
    {
        // 1. High DPI initialization
        ApplicationConfiguration.Initialize();
        WinFormsApp.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // Cairo Typography initialization
        AppFont.Initialize();
        WinFormsApp.SetDefaultFont(AppFont.CreateFont(10F, System.Drawing.FontStyle.Regular));

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
                    services.AddParkGateServices(context.Configuration);
                })
                .Build();

            await _host.StartAsync();

            // 5. Run startup coordinator (T4.1)
            var coordinator = _host.Services.GetRequiredService<IStartupCoordinator>();
            var status = await coordinator.RunStartupSequenceAsync();

            if (status == StartupStatus.Success)
            {
                var formFactory = _host.Services.GetRequiredService<IFormFactory>();
                var mainForm = formFactory.Create<Forms.MainForm>();

                WinFormsApp.Run(mainForm);
            }
            else
            {
                Log.Information("ParkGate startup finished with status {Status}. Exiting application.", status);
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "ParkGate terminated unexpectedly during startup.");
            ShowFatalErrorMessage(ex);
        }
        finally
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            Log.Information("ParkGate shutdown complete.");
            Log.CloseAndFlush();
        }
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