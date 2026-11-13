using ParkGate.Domain.Entities;

namespace ParkGate.Application.Sync;

/// <summary>
/// Deduplicates outbox records using the Last-Write-Wins strategy.
/// When multiple mutations exist for the same (EntityType, EntityId) in a batch,
/// only the newest state is preserved, discarding obsolete intermediate entries.
/// "Newest" is the database <see cref="SyncOutbox.Sequence"/>, never a client clock (R2.2).
/// </summary>
public static class OutboxDeduplicator
{
    public static IReadOnlyList<SyncOutbox> Deduplicate(IEnumerable<SyncOutbox>? records)
    {
        if (records == null)
        {
            return Array.Empty<SyncOutbox>();
        }

        var list = records as IReadOnlyList<SyncOutbox> ?? records.ToList();
        if (list.Count == 0)
        {
            return Array.Empty<SyncOutbox>();
        }

        return list
            .GroupBy(r => new { r.EntityType, r.EntityId })
            .Select(g => g.OrderByDescending(r => r.Sequence).First())
            .OrderBy(r => r.Sequence)
            .ToList();
    }
}
