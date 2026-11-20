using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface IAuditLogRepository
{
    void Add(AuditLog log);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// R5.20: rows with <c>fromUtc &lt;= AtUtc &lt; toUtc</c>, newest first, at most <paramref name="maxRows"/>.
    /// <paramref name="actions"/> null = every action; <paramref name="detailsFragment"/> is matched inside the details JSON.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> SearchAsync(DateTime fromUtc, DateTime toUtc, IReadOnlyCollection<string>? actions, string? detailsFragment, int maxRows, CancellationToken cancellationToken = default);
}
