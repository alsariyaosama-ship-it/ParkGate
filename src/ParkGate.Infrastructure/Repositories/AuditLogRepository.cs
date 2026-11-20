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

    public async Task<IReadOnlyList<AuditLog>> SearchAsync(DateTime fromUtc, DateTime toUtc, IReadOnlyCollection<string>? actions, string? detailsFragment, int maxRows, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking().Where(a => a.AtUtc >= fromUtc && a.AtUtc < toUtc);
        if (actions is not null)
        {
            query = query.Where(a => actions.Contains(a.Action));
        }

        if (!string.IsNullOrEmpty(detailsFragment))
        {
            query = query.Where(a => a.DetailsJson != null && a.DetailsJson.Contains(detailsFragment));
        }

        return await query.OrderByDescending(a => a.AtUtc).ThenByDescending(a => a.Id).Take(maxRows).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.AtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
