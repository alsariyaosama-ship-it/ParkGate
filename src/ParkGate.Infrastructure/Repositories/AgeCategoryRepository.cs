using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Repositories;

public sealed class AgeCategoryRepository : IAgeCategoryRepository
{
    private readonly ParkGateDbContext _context;

    public AgeCategoryRepository(ParkGateDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AgeCategory>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AgeCategories
            .Where(a => a.IsActive)
            .OrderBy(a => a.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<AgeCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.AgeCategories
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}
