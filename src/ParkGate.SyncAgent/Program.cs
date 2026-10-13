using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParkGate.Application.Ports.Sync;
using ParkGate.Application.Sync;
using ParkGate.Infrastructure;
using ParkGate.Infrastructure.Sync;
using ParkGate.SyncAgent.Workers;
using Serilog;

namespace ParkGate.SyncAgent;

public static class Program
{
    private static readonly string LogDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ParkGate", "logs");

    public static async Task<int> Main(string[] args)
    {
        Directory.CreateDirectory(LogDir);
        var logFile = Path.Combine(LogDir, "sync-agent-.txt");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                logFile,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting ParkGate.SyncAgent Host...");
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "ParkGate.SyncAgent host terminated unexpectedly.");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseContentRoot(AppContext.BaseDirectory)
            .UseWindowsService(options =>
            {
                options.ServiceName = "ParkGate SyncAgent";
            })
            .UseSerilog((_, _, configuration) =>
            {
                var logFile = Path.Combine(LogDir, "sync-agent-.txt");
                configuration
                    .MinimumLevel.Information()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File(
                        logFile,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                var localSettings = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "ParkGate",
                    "appsettings.local.json");

                if (File.Exists(localSettings))
                {
                    config.AddJsonFile(localSettings, optional: true, reloadOnChange: true);
                }

                config.AddEnvironmentVariables(prefix: "PARKGATE_");
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddInfrastructure(context.Configuration);

                services.Configure<SyncOptions>(context.Configuration.GetSection("SyncOptions"));
                services.Configure<CloudApiOptions>(context.Configuration.GetSection("CloudApi"));

                services.AddHttpClient<ICloudPushService, CloudPushService>();

                services.AddHostedService<OutboxProcessorWorker>();
                services.AddHostedService<HeartbeatWorker>();
            });
}
