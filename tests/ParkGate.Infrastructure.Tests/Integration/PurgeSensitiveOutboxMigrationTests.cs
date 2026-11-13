using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Data.Migrations;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class PurgeSensitiveOutboxMigrationTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public PurgeSensitiveOutboxMigrationTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PurgeSql_ReplacesPendingBeneficiaryRowsWithOneCleanRowPerHolder()
    {
        await using var context = _fixture.CreateDbContext();

        var holderId = Guid.NewGuid();
        context.Beneficiaries.Add(new Beneficiary(
            id: holderId,
            organizationId: 1,
            cardNumber: CardNumber.Create("CARD-PURGE-1"),
            fullName: "سالم محمود",
            phone: PhoneNumber.Create("0923456789"),
            nationalId: NationalId.Create("119951234567"),
            cardExpiryDate: new DateOnly(2028, 1, 1),
            notes: "ملاحظة"));
        await context.SaveChangesAsync();

        // Old-style pending rows that still carry the sensitive fields (as the agent used to queue them).
        var staleInsert = SyncOutbox.Create(
            "Beneficiary", holderId.ToString(), "Insert",
            "{\"Id\":\"x\",\"Phone\":\"ENCRYPTED\",\"NationalId\":\"ENCRYPTED\"}", DateTime.UtcNow);
        var staleDelete = SyncOutbox.Create(
            "Beneficiary", Guid.NewGuid().ToString(), "Delete",
            "{\"Id\":\"y\",\"Phone\":\"0911111111\",\"NationalId\":\"119900000000\"}", DateTime.UtcNow);
        context.SyncOutboxes.AddRange(staleInsert, staleDelete);
        await context.SaveChangesAsync();

        await context.Database.ExecuteSqlRawAsync(PurgeSensitiveBeneficiaryOutbox.PurgeSql);

        var rows = await context.SyncOutboxes
            .AsNoTracking()
            .Where(x => x.EntityType == nameof(Beneficiary) && !x.IsProcessed)
            .ToListAsync();

        rows.ShouldNotContain(r => r.Id == staleInsert.Id);
        rows.Count(r => r.EntityId == holderId.ToString()).ShouldBe(1);

        var requeued = rows.Single(r => r.EntityId == holderId.ToString());
        requeued.OperationType.ShouldBe("Update");
        using var doc = JsonDocument.Parse(requeued.PayloadJson);
        var root = doc.RootElement;
        root.TryGetProperty("Phone", out _).ShouldBeFalse();
        root.TryGetProperty("NationalId", out _).ShouldBeFalse();
        root.GetProperty("Id").GetString().ShouldBe(holderId.ToString());
        root.GetProperty("CardNumber").GetString().ShouldBe("CARD-PURGE-1");
        root.GetProperty("FullName").GetString().ShouldBe("سالم محمود");
        root.GetProperty("CardExpiryDate").GetString().ShouldBe("2028-01-01");
        root.GetProperty("Notes").GetString().ShouldBe("ملاحظة");
        root.GetProperty("Status").GetInt32().ShouldBe(1);
        root.GetProperty("CreatedAtUtc").GetString().ShouldEndWith("Z");

        var strippedDelete = rows.Single(r => r.Id == staleDelete.Id);
        strippedDelete.PayloadJson.ShouldNotContain("0911111111");
        strippedDelete.PayloadJson.ShouldNotContain("119900000000");
    }
}
