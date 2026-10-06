using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParkGate.Application.Ports.Audit;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Security;
using ParkGate.Application.Ports.Settings;
using ParkGate.Infrastructure.Data;
using ParkGate.Infrastructure.Security;

namespace ParkGate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // 1. Connection protection (DPAPI on Windows, in-memory fallback for non-Windows platforms)
        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<IConnectionSettingsStore, DpapiConnectionSettingsStore>();
        }
        else
        {
            services.AddSingleton<IConnectionSettingsStore, InMemoryConnectionSettingsStore>();
        }

        // Active connection provider
        services.AddSingleton<IAppConnectionStringProvider>(sp =>
        {
            var store = sp.GetRequiredService<IConnectionSettingsStore>();
            return new AppConnectionStringProvider(store, configuration);
        });

        // 2. DbContextFactory (ADR-003)
        services.AddDbContextFactory<ParkGateDbContext>((sp, options) =>
        {
            var connectionProvider = sp.GetRequiredService<IAppConnectionStringProvider>();
            options.UseSqlServer(connectionProvider.CurrentConnectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(ParkGateDbContext).Assembly.FullName);
            });
        });

        // 3. Infrastructure singletons & services
        services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAuditWriter, AuditWriter>();
        services.AddSingleton<IAppSettingsStore, AppSettingsStore>();
        services.AddTransient<IDatabaseStartupService, DatabaseStartupService>();
        services.AddTransient<ParkGate.Application.Ports.Persistence.IDatabaseBackupService, ParkGate.Infrastructure.Data.DatabaseBackupService>();
        services.AddTransient<ParkGate.Application.Ports.Reports.IReportQueries, ParkGate.Infrastructure.Reports.ReportQueries>();
        services.AddTransient<ParkGate.Application.Ports.Reports.IExcelExportService, ParkGate.Infrastructure.Reports.ClosedXmlExcelExportService>();

        return services;
    }

    private sealed class InMemoryConnectionSettingsStore : IConnectionSettingsStore
    {
        private string? _connectionString;
        public bool Exists() => _connectionString != null;
        public string? GetConnectionString() => _connectionString;
        public void SaveConnectionString(string connectionString) => _connectionString = connectionString;
        public void Clear() => _connectionString = null;
    }
}
