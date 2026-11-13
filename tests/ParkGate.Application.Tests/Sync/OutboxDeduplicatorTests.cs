using ParkGate.Application.Sync;
using ParkGate.Domain.Entities;
using Shouldly;

namespace ParkGate.Application.Tests.Sync;

public sealed class OutboxDeduplicatorTests
{
    /// <summary>
    /// <paramref name="sequence"/> stands in for the database IDENTITY value (R2.2); it is private-set
    /// on the entity because only the database assigns it.
    /// </summary>
    private static SyncOutbox MakeRecord(string entityType, string entityId, long sequence, DateTime createdAt, string op = "Update")
    {
        var record = SyncOutbox.Create(entityType, entityId, op, "{}", createdAt);
        typeof(SyncOutbox).GetProperty(nameof(SyncOutbox.Sequence))!.SetValue(record, sequence);
        return record;
    }

    [Fact]
    public void Deduplicate_NullOrEmptyList_ReturnsEmpty()
    {
        OutboxDeduplicator.Deduplicate(null).ShouldBeEmpty();
        OutboxDeduplicator.Deduplicate(Array.Empty<SyncOutbox>()).ShouldBeEmpty();
    }

    [Fact]
    public void Deduplicate_MultipleUpdatesSameEntity_KeepsNewestOnly()
    {
        var now = DateTime.UtcNow;
        var r1 = MakeRecord("Beneficiary", "ID-1", 1, now.AddMinutes(-5), "Insert");
        var r2 = MakeRecord("Beneficiary", "ID-1", 2, now.AddMinutes(-2), "Update");
        var r3 = MakeRecord("Beneficiary", "ID-1", 3, now, "Update");

        var result = OutboxDeduplicator.Deduplicate(new[] { r1, r2, r3 });

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(r3.Id);
    }

    [Fact]
    public void Deduplicate_DifferentEntities_PreservesAll()
    {
        var now = DateTime.UtcNow;
        var r1 = MakeRecord("Beneficiary", "ID-1", 1, now.AddMinutes(-5));
        var r2 = MakeRecord("Beneficiary", "ID-2", 2, now.AddMinutes(-4));
        var r3 = MakeRecord("Organization", "ORG-1", 3, now.AddMinutes(-3));

        var result = OutboxDeduplicator.Deduplicate(new[] { r1, r2, r3 });

        result.Count.ShouldBe(3);
    }

    [Fact]
    public void Deduplicate_MixedBatches_DeduplicatesCorrectlyAndMaintainsSequenceOrder()
    {
        var now = DateTime.UtcNow;
        var b1Old = MakeRecord("Beneficiary", "ID-1", 1, now.AddMinutes(-10));
        var b2 = MakeRecord("Beneficiary", "ID-2", 2, now.AddMinutes(-8));
        var b1New = MakeRecord("Beneficiary", "ID-1", 3, now.AddMinutes(-5));
        var o1 = MakeRecord("Organization", "ORG-1", 4, now.AddMinutes(-2));

        var result = OutboxDeduplicator.Deduplicate(new[] { b1Old, b2, b1New, o1 });

        result.Count.ShouldBe(3);
        result.Select(r => r.Id).ShouldBe(new[] { b2.Id, b1New.Id, o1.Id });
    }

    [Fact]
    public void Deduplicate_LaterInsertedRowWithEarlierClientClock_Wins()
    {
        // Two desks, one clock running 10 minutes behind: the row inserted later carries the
        // earlier CreatedAtUtc. The database sequence, not the client clock, decides (R2.2).
        var now = DateTime.UtcNow;
        var earlierWrite = MakeRecord("Beneficiary", "ID-1", 10, now, "Update");
        var laterWriteSkewedClock = MakeRecord("Beneficiary", "ID-1", 11, now.AddMinutes(-10), "Update");

        var result = OutboxDeduplicator.Deduplicate(new[] { earlierWrite, laterWriteSkewedClock });

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(laterWriteSkewedClock.Id);
    }

    [Fact]
    public void Deduplicate_OutputOrder_FollowsSequenceNotClientClock()
    {
        var now = DateTime.UtcNow;
        var first = MakeRecord("Beneficiary", "ID-1", 1, now, "Insert");
        var second = MakeRecord("EntryAttempt", "ATT-1", 2, now.AddMinutes(-30), "Insert");

        var result = OutboxDeduplicator.Deduplicate(new[] { second, first });

        result.Select(r => r.Id).ShouldBe(new[] { first.Id, second.Id });
    }
}
