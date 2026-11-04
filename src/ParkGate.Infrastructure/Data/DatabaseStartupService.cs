using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;

namespace ParkGate.Infrastructure.Data;

/// <summary>
/// Implements database connectivity and pending migrations inspection for application startup.
/// </summary>
public sealed class DatabaseStartupService : IDatabaseStartupService
{
    private readonly IAppConnectionStringProvider _connectionStringProvider;

    public DatabaseStartupService(
        IAppConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    public async Task<bool> CanConnectAsync(string? connectionString = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var effectiveConnectionString = string.IsNullOrWhiteSpace(connectionString)
                ? _connectionStringProvider.CurrentConnectionString
                : connectionString;

            await using var context = CreateContext(effectiveConnectionString);
            return await context.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = CreateContext(_connectionStringProvider.CurrentConnectionString);
        var pending = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        return pending.ToList();
    }

    public async Task EnsureEntryCardsSchemaAsync(CancellationToken cancellationToken = default)
    {
        await EntryCards.EntryCardSchemaInitializer.EnsureSchemaAsync(
            _connectionStringProvider.CurrentConnectionString,
            cancellationToken).ConfigureAwait(false);
    }

    private static ParkGateDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<ParkGateDbContext>()
            .UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(ParkGateDbContext).Assembly.FullName);
            })
            .Options;

        return new ParkGateDbContext(options);
    }
}
