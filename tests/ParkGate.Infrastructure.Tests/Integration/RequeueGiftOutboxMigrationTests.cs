using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Data.Migrations;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class RequeueGiftOutboxMigrationTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public RequeueGiftOutboxMigrationTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RequeueSql_QueuesOrganizationsAgeCategoriesAndVouchersWithoutTheToken()
    {
        await using var context = _fixture.CreateDbContext();

        var organization = new Organization($"RQ_{Guid.NewGuid():N}"[..10], "جهة إعادة الإدراج");
        organization.SetMaxFreeCompanions(4);
        organization.ConfigureGift(true, 12.5m, 14);
        var category = new AgeCategory($"RQ{Guid.NewGuid():N}"[..10], null, 5);
        category.SetCompanionLimitExempt(true);
        var user = new User($"rq_{Guid.NewGuid():N}"[..12], "موظف", "hash", ParkGate.Domain.Enums.UserRole.Supervisor);
        var workstation = new Workstation($"RQW_{Guid.NewGuid():N}"[..10], "شباك");
        context.AddRange(organization, category, user, workstation);
        await context.SaveChangesAsync();

        var holder = new Beneficiary(Guid.NewGuid(), organization.Id,
            CardNumber.Create($"RQ-{Guid.NewGuid():N}"[..12]), "زبون",
            PhoneNumber.Create($"091{Random.Shared.Next(1_000_000):D7}"));
        context.Beneficiaries.Add(holder);
        await context.SaveChangesAsync();

        var token = Guid.NewGuid();
        var voucher = GiftVoucher.Issue(token, 1, holder.Id, organization.Id, 12.5m,
            new DateTime(2026, 11, 13, 9, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 11, 13),
            user.Id, workstation.Id);
        context.GiftVouchers.Add(voucher);
        await context.SaveChangesAsync();

        await context.Database.ExecuteSqlRawAsync(RequeueGiftAndPolicyOutbox.RequeueSql);

        var rows = await context.SyncOutboxes.AsNoTracking()
            .Where(x => !x.IsProcessed && x.OperationType == "Update")
            .ToListAsync();

        var orgRow = rows.Last(x => x.EntityType == nameof(Organization) && x.EntityId == organization.Id.ToString());
        using (var doc = JsonDocument.Parse(orgRow.PayloadJson))
        {
            var root = doc.RootElement;
            root.GetProperty("Code").GetString().ShouldBe(organization.Code);
            root.GetProperty("MaxFreeCompanions").GetInt32().ShouldBe(4);
            root.GetProperty("GiftEnabled").GetBoolean().ShouldBeTrue();
            root.GetProperty("GiftValue").GetDecimal().ShouldBe(12.5m);
            root.GetProperty("GiftIntervalDays").GetInt32().ShouldBe(14);
        }

        var categoryRow = rows.Last(x => x.EntityType == nameof(AgeCategory) && x.EntityId == category.Id.ToString());
        using (var doc = JsonDocument.Parse(categoryRow.PayloadJson))
        {
            doc.RootElement.GetProperty("IsCompanionLimitExempt").GetBoolean().ShouldBeTrue();
        }

        var voucherRow = rows.Single(x => x.EntityType == nameof(GiftVoucher) && x.EntityId == voucher.Id.ToString());
        voucherRow.PayloadJson.ShouldNotContain(token.ToString());
        using (var doc = JsonDocument.Parse(voucherRow.PayloadJson))
        {
            var root = doc.RootElement;
            root.TryGetProperty("Token", out _).ShouldBeFalse();
            root.GetProperty("Id").GetInt32().ShouldBe(voucher.Id);
            root.GetProperty("BeneficiaryId").GetString().ShouldBe(holder.Id.ToString());
            root.GetProperty("OrganizationId").GetInt32().ShouldBe(organization.Id);
            root.GetProperty("Value").GetDecimal().ShouldBe(12.5m);
            root.GetProperty("Status").GetInt32().ShouldBe(1);
            root.GetProperty("BusinessDate").GetString().ShouldBe("2026-11-13");
            root.GetProperty("ExpiresOn").GetString().ShouldBe("2026-11-13");
            root.GetProperty("IssuedAtUtc").GetString().ShouldEndWith("Z");
            root.GetProperty("IsOverride").GetBoolean().ShouldBeFalse();
            root.GetProperty("RedeemedAtUtc").ValueKind.ShouldBe(JsonValueKind.Null);
        }
    }
}
