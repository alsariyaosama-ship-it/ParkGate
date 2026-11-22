using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Repositories;

public sealed class CityRepository : ICityRepository
{
    private readonly ParkGateDbContext _context;

    public CityRepository(ParkGateDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<City>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Cities
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<City>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Cities
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<City?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Cities
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
