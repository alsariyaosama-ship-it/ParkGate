using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Infrastructure.Data.Interceptors;

namespace ParkGate.Infrastructure.Data;

/// <summary>
/// A dynamic <see cref="IDbContextFactory{TContext}"/> implementation that resolves the active
/// connection string on every invocation from <see cref="IAppConnectionStringProvider"/>.
/// This ensures runtime connection string updates immediately apply to subsequent DbContext instances.
/// </summary>
public sealed class DynamicParkGateDbContextFactory : IDbContextFactory<ParkGateDbContext>
{
    private readonly IAppConnectionStringProvider _connectionProvider;

    // One instance for every context: EF caches its internal services per interceptor instance.
    private readonly AuditSaveChangesInterceptor _auditInterceptor;

    /// <param name="currentUser">The signed-in desk user for the audit columns; absent in the sync agent.</param>
    public DynamicParkGateDbContextFactory(IAppConnectionStringProvider connectionProvider, ICurrentUser? currentUser = null)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _auditInterceptor = new AuditSaveChangesInterceptor(() => currentUser?.UserId);
    }

    public ParkGateDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ParkGateDbContext>();
        optionsBuilder.UseSqlServer(_connectionProvider.CurrentConnectionString, sql =>
        {
            sql.MigrationsAssembly(typeof(ParkGateDbContext).Assembly.FullName);
        });
        optionsBuilder.AddInterceptors(_auditInterceptor);

        return new ParkGateDbContext(optionsBuilder.Options);
    }

    public Task<ParkGateDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateDbContext());
    }
}
