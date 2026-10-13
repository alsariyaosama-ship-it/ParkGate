using ParkGate.Application.Sync;
using ParkGate.Domain.Entities;
using Shouldly;

namespace ParkGate.Application.Tests.Sync;

public sealed class OutboxDeduplicatorTests
{
    private static SyncOutbox MakeRecord(string entityType, string entityId, DateTime createdAt, string op = "Update") =>
        SyncOutbox.Create(entityType, entityId, op, "{}", createdAt);

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
        var r1 = MakeRecord("Beneficiary", "ID-1", now.AddMinutes(-5), "Insert");
        var r2 = MakeRecord("Beneficiary", "ID-1", now.AddMinutes(-2), "Update");
        var r3 = MakeRecord("Beneficiary", "ID-1", now, "Update");

        var result = OutboxDeduplicator.Deduplicate(new[] { r1, r2, r3 });

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(r3.Id);
        result[0].CreatedAtUtc.ShouldBe(now);
    }

    [Fact]
    public void Deduplicate_DifferentEntities_PreservesAll()
    {
        var now = DateTime.UtcNow;
        var r1 = MakeRecord("Beneficiary", "ID-1", now.AddMinutes(-5));
        var r2 = MakeRecord("Beneficiary", "ID-2", now.AddMinutes(-4));
        var r3 = MakeRecord("Organization", "ORG-1", now.AddMinutes(-3));

        var result = OutboxDeduplicator.Deduplicate(new[] { r1, r2, r3 });

        result.Count.ShouldBe(3);
    }

    [Fact]
    public void Deduplicate_MixedBatches_DeduplicatesCorrectlyAndMaintainsChronologicalOrder()
    {
        var now = DateTime.UtcNow;
        var b1Old = MakeRecord("Beneficiary", "ID-1", now.AddMinutes(-10));
        var b2 = MakeRecord("Beneficiary", "ID-2", now.AddMinutes(-8));
        var b1New = MakeRecord("Beneficiary", "ID-1", now.AddMinutes(-5));
        var o1 = MakeRecord("Organization", "ORG-1", now.AddMinutes(-2));

        var result = OutboxDeduplicator.Deduplicate(new[] { b1Old, b2, b1New, o1 });

        result.Count.ShouldBe(3);
        result.Select(r => r.Id).ShouldBe(new[] { b2.Id, b1New.Id, o1.Id });
    }
}
