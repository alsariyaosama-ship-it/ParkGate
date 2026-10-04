using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Repositories;

public sealed class OrganizationRepository : IOrganizationRepository
{
    private readonly ParkGateDbContext _context;

    public OrganizationRepository(ParkGateDbContext context)
    {
        _context = context;
    }

    public async Task<Organization?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Organization?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .SingleOrDefaultAsync(o => o.IsDefault, cancellationToken);
    }

    public async Task<IReadOnlyList<Organization>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Organizations.AsQueryable();
        if (activeOnly)
        {
            query = query.Where(o => o.IsActive);
        }

        return await query
            .OrderBy(o => o.Code)
            .ToListAsync(cancellationToken);
    }

    public void Add(Organization organization)
    {
        _context.Organizations.Add(organization);
    }

    public void Update(Organization organization)
    {
        _context.Organizations.Update(organization);
    }
}
