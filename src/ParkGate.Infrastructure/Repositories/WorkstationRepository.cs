using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Repositories;

public sealed class WorkstationRepository : IWorkstationRepository
{
    private readonly ParkGateDbContext _context;

    public WorkstationRepository(ParkGateDbContext context)
    {
        _context = context;
    }

    public async Task<Workstation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Workstations
            .SingleOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Workstation?> GetByMachineNameAsync(string machineName, CancellationToken cancellationToken = default)
    {
        var normalized = machineName.Trim().ToUpperInvariant();
        return await _context.Workstations
            .SingleOrDefaultAsync(w => w.MachineName == normalized, cancellationToken);
    }

    public void Add(Workstation workstation)
    {
        _context.Workstations.Add(workstation);
    }

    public void Update(Workstation workstation)
    {
        _context.Workstations.Update(workstation);
    }
}
