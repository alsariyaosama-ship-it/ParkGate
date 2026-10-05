using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;

namespace ParkGate.Infrastructure.Data;

/// <summary>
/// Implements database connectivity and pending migrations inspection for application startup.
/// </summary>
public sealed class DatabaseStartupService : IDatabaseStartupService
{
    private readonly IDbContextFactory<ParkGateDbContext> _contextFactory;
    private readonly IAppConnectionStringProvider _connectionStringProvider;

    public DatabaseStartupService(
        IDbContextFactory<ParkGateDbContext> contextFactory,
        IAppConnectionStringProvider connectionStringProvider)
    {
        _contextFactory = contextFactory;
        _connectionStringProvider = connectionStringProvider;
    }

    public async Task<bool> CanConnectAsync(string? connectionString = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
                return await context.Database.CanConnectAsync(cancellationToken);
            }
            else
            {
                var options = new DbContextOptionsBuilder<ParkGateDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                await using var testContext = new ParkGateDbContext(options);
                return await testContext.Database.CanConnectAsync(cancellationToken);
            }
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var pending = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        return pending.ToList();
    }
}
