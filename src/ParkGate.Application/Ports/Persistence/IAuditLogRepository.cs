using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface IAuditLogRepository
{
    void Add(AuditLog log);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
}
