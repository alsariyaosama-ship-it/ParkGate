using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;

namespace ParkGate.Infrastructure.Data;

public sealed class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbContextFactory<ParkGateDbContext> _contextFactory;

    public UnitOfWorkFactory(IDbContextFactory<ParkGateDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public IUnitOfWork Create()
    {
        var context = _contextFactory.CreateDbContext();
        return new UnitOfWork(context);
    }
}
