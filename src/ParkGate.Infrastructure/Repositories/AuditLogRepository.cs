using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly ParkGateDbContext _context;

    public AuditLogRepository(ParkGateDbContext context)
    {
        _context = context;
    }

    public void Add(AuditLog log)
    {
        _context.AuditLogs.Add(log);
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.AtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
