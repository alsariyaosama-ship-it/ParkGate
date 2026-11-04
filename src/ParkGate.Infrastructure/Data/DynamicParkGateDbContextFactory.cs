using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;

namespace ParkGate.Infrastructure.Data;

/// <summary>
/// A dynamic <see cref="IDbContextFactory{TContext}"/> implementation that resolves the active
/// connection string on every invocation from <see cref="IAppConnectionStringProvider"/>.
/// This ensures runtime connection string updates immediately apply to subsequent DbContext instances.
/// </summary>
public sealed class DynamicParkGateDbContextFactory : IDbContextFactory<ParkGateDbContext>
{
    private readonly IAppConnectionStringProvider _connectionProvider;

    public DynamicParkGateDbContextFactory(IAppConnectionStringProvider connectionProvider)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
    }

    public ParkGateDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ParkGateDbContext>();
        optionsBuilder.UseSqlServer(_connectionProvider.CurrentConnectionString, sql =>
        {
            sql.MigrationsAssembly(typeof(ParkGateDbContext).Assembly.FullName);
        });

        return new ParkGateDbContext(optionsBuilder.Options);
    }

    public Task<ParkGateDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateDbContext());
    }
}
