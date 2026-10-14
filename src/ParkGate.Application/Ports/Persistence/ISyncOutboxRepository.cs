using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface ISyncOutboxRepository
{
    Task<IReadOnlyList<SyncOutbox>> GetPendingBatchAsync(int batchSize, int maxRetries, CancellationToken cancellationToken = default);
    Task AddAsync(SyncOutbox outbox, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<SyncOutbox> outboxes, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<SyncOutbox> outboxes, CancellationToken cancellationToken = default);
    Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default);
}
