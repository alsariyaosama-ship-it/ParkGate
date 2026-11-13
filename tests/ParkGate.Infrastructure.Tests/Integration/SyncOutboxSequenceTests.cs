using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Sync;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Repositories;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class SyncOutboxSequenceTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public SyncOutboxSequenceTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Sequence_IsAssignedByTheDatabaseInInsertOrder()
    {
        await using var context = _fixture.CreateDbContext();
        var entityId = $"SEQ-{Guid.NewGuid():N}";
        var first = SyncOutbox.Create("Beneficiary", entityId, "Insert", "{\"n\":1}", DateTime.UtcNow);
        var second = SyncOutbox.Create("Beneficiary", entityId, "Update", "{\"n\":2}", DateTime.UtcNow);

        context.SyncOutboxes.Add(first);
        await context.SaveChangesAsync();
        context.SyncOutboxes.Add(second);
        await context.SaveChangesAsync();

        first.Sequence.ShouldBeGreaterThan(0);
        second.Sequence.ShouldBeGreaterThan(first.Sequence);
    }

    [Fact]
    public async Task GetPendingBatch_LaterInsertedRowWithEarlierClientClock_IsLastAndWinsDeduplication()
    {
        await using var context = _fixture.CreateDbContext();
        var entityId = $"SKEW-{Guid.NewGuid():N}";
        var now = DateTime.UtcNow;

        // Desk A writes first; desk B writes later but its clock runs 10 minutes behind.
        var deskA = SyncOutbox.Create("Beneficiary", entityId, "Update", "{\"from\":\"A\"}", now);
        var deskB = SyncOutbox.Create("Beneficiary", entityId, "Update", "{\"from\":\"B\"}", now.AddMinutes(-10));
        context.SyncOutboxes.Add(deskA);
        await context.SaveChangesAsync();
        context.SyncOutboxes.Add(deskB);
        await context.SaveChangesAsync();

        var repository = new SyncOutboxRepository(_fixture.CreateDbContext());
        var pending = (await repository.GetPendingBatchAsync(500, 10))
            .Where(x => x.EntityId == entityId)
            .ToList();

        pending.Select(x => x.PayloadJson).ShouldBe(new[] { "{\"from\":\"A\"}", "{\"from\":\"B\"}" });

        var winner = OutboxDeduplicator.Deduplicate(pending).Single();
        winner.PayloadJson.ShouldBe("{\"from\":\"B\"}");
    }

    [Fact]
    public async Task CreatedAtUtc_HasADatabaseDefault()
    {
        await using var context = _fixture.CreateDbContext();

        var defaultSql = await context.Database
            .SqlQueryRaw<string>(
                "SELECT OBJECT_DEFINITION(dc.object_id) AS [Value] FROM sys.default_constraints dc " +
                "JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id " +
                "WHERE dc.parent_object_id = OBJECT_ID(N'SyncOutboxes') AND c.name = N'CreatedAtUtc'")
            .SingleAsync();

        defaultSql.ShouldContain("sysutcdatetime", Case.Insensitive);
    }
}
