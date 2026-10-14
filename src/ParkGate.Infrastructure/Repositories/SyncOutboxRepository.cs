using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Repositories;

public sealed class SyncOutboxRepository : ISyncOutboxRepository
{
    private readonly ParkGateDbContext _context;

    public SyncOutboxRepository(ParkGateDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SyncOutbox>> GetPendingBatchAsync(
        int batchSize,
        int maxRetries,
        CancellationToken cancellationToken = default)
    {
        return await _context.SyncOutboxes
            .Where(x => !x.IsProcessed && x.RetryCount < maxRetries)
            .OrderBy(x => x.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SyncOutbox outbox, CancellationToken cancellationToken = default)
    {
        await _context.SyncOutboxes.AddAsync(outbox, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<SyncOutbox> outboxes, CancellationToken cancellationToken = default)
    {
        await _context.SyncOutboxes.AddRangeAsync(outboxes, cancellationToken);
    }

    public Task DeleteRangeAsync(IEnumerable<SyncOutbox> outboxes, CancellationToken cancellationToken = default)
    {
        _context.SyncOutboxes.RemoveRange(outboxes);
        return Task.CompletedTask;
    }

    public async Task<int> GetPendingCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SyncOutboxes
            .Where(x => !x.IsProcessed)
            .CountAsync(cancellationToken);
    }
}
