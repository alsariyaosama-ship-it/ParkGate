using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.CardPatterns;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Data;
using ParkGate.Infrastructure.Repositories;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>F8, T8.2b: <c>OrganizationCardPatterns</c> and <c>CardPatternAlerts</c> (DATABASE.md §3.13, §3.14).</summary>
[Trait("Category", "Integration")]
public sealed class CardPatternMappingTests : IClassFixture<LocalDbTestFixture>
{
    private static readonly DateTime Now = new(2026, 11, 20, 9, 30, 15, 250, DateTimeKind.Utc);
    private readonly LocalDbTestFixture _fixture;

    public CardPatternMappingTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    private sealed record Seed(int OrganizationId, Guid BeneficiaryId, string CardNumber, int UserId, int WorkstationId);

    private static async Task<Seed> SeedAsync(ParkGateDbContext context, string? card = null)
    {
        var org = new Organization($"PAT_{Guid.NewGuid():N}"[..10], "جهة الأنماط");
        var user = new User($"pat_{Guid.NewGuid():N}"[..12], "مدير", "hash", UserRole.Admin);
        var desk = new Workstation($"PWS_{Guid.NewGuid():N}"[..10], "شباك");
        context.AddRange(org, user, desk);
        await context.SaveChangesAsync();

        var number = card ?? $"AB-{Random.Shared.Next(10_000):D4}";
        var holder = new Beneficiary(Guid.NewGuid(), org.Id, CardNumber.Create(number), "زائر", PhoneNumber.Create($"091{Random.Shared.Next(1_000_000):D7}"));
        context.Beneficiaries.Add(holder);
        await context.SaveChangesAsync();
        return new Seed(org.Id, holder.Id, number, user.Id, desk.Id);
    }

    [Fact]
    public async Task Pattern_RoundTripsEveryColumn()
    {
        int id;
        Seed seed;
        await using (var context = _fixture.CreateDbContext())
        {
            seed = await SeedAsync(context);
            var pattern = OrganizationCardPattern.Propose(seed.OrganizationId, "DDDDDD", "10", CardPatternSource.NewSeries, 7, Now);
            pattern.Approve(seed.UserId, Now.AddMinutes(5));
            context.OrganizationCardPatterns.Add(pattern);
            await context.SaveChangesAsync();
            id = pattern.Id;
        }

        await using (var context = _fixture.CreateDbContext())
        {
            var loaded = await context.OrganizationCardPatterns.AsNoTracking().SingleAsync(p => p.Id == id);

            loaded.OrganizationId.ShouldBe(seed.OrganizationId);
            loaded.Shape.ShouldBe("DDDDDD");
            loaded.Prefix.ShouldBe("10");
            loaded.Status.ShouldBe(CardPatternStatus.Approved);
            loaded.Source.ShouldBe(CardPatternSource.NewSeries);
            loaded.SampleCount.ShouldBe(7);
            loaded.ProposedAtUtc.ShouldBe(Now);
            loaded.DecidedAtUtc.ShouldBe(Now.AddMinutes(5));
            loaded.DecidedByUserId.ShouldBe(seed.UserId);
            loaded.RowVersion.ShouldNotBeEmpty();
        }
    }

    [Fact]
    public async Task Pattern_SameShapeWithoutPrefixTwice_IsRejected()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context);
        context.OrganizationCardPatterns.Add(OrganizationCardPattern.Propose(seed.OrganizationId, "DDDD", null, CardPatternSource.Dominant, 10, Now));
        await context.SaveChangesAsync();

        context.OrganizationCardPatterns.Add(OrganizationCardPattern.Propose(seed.OrganizationId, "DDDD", null, CardPatternSource.NewSeries, 5, Now));

        var ex = await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync());
        ex.InnerException!.Message.ShouldContain("UX_OrganizationCardPatterns_Org_Shape_Prefix");
    }

    [Fact]
    public async Task Alert_RoundTripsAndReview()
    {
        long id;
        Seed seed;
        await using (var context = _fixture.CreateDbContext())
        {
            seed = await SeedAsync(context);
            var alert = CardPatternAlert.Raise(seed.OrganizationId, seed.BeneficiaryId, seed.CardNumber, Now, seed.UserId, seed.WorkstationId);
            context.CardPatternAlerts.Add(alert);
            await context.SaveChangesAsync();
            alert.Review(seed.UserId, Now.AddHours(1), "بطاقة صحيحة");
            await context.SaveChangesAsync();
            id = alert.Id;
        }

        await using (var context = _fixture.CreateDbContext())
        {
            var loaded = await context.CardPatternAlerts.AsNoTracking().SingleAsync(a => a.Id == id);

            loaded.BeneficiaryId.ShouldBe(seed.BeneficiaryId);
            loaded.CardNumber.ShouldBe(seed.CardNumber);
            loaded.Shape.ShouldBe("LL-DDDD");
            loaded.CreatedAtUtc.ShouldBe(Now);
            loaded.CreatedByUserId.ShouldBe(seed.UserId);
            loaded.WorkstationId.ShouldBe(seed.WorkstationId);
            loaded.Status.ShouldBe(CardPatternAlertStatus.Reviewed);
            loaded.ReviewedByUserId.ShouldBe(seed.UserId);
            loaded.ReviewNote.ShouldBe("بطاقة صحيحة");
        }
    }

    [Fact]
    public async Task Repositories_FindOpenAlertsAndCardNumbers()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context, "AB-0001");
        var reviewed = CardPatternAlert.Raise(seed.OrganizationId, seed.BeneficiaryId, "AB-0002", Now, seed.UserId, seed.WorkstationId);
        reviewed.Review(seed.UserId, Now, null);
        context.CardPatternAlerts.AddRange(
            CardPatternAlert.Raise(seed.OrganizationId, seed.BeneficiaryId, "AB-0001", Now, seed.UserId, seed.WorkstationId),
            reviewed);
        await context.SaveChangesAsync();

        var alerts = new CardPatternAlertRepository(context);
        var holders = new BeneficiaryRepository(context);

        (await alerts.GetOpenCardNumbersAsync(seed.OrganizationId, "LL-DDDD")).ShouldBe(["AB-0001"]);
        (await alerts.GetBeneficiariesWithOpenAlertsAsync([seed.BeneficiaryId, Guid.NewGuid()])).ShouldBe([seed.BeneficiaryId]);
        (await alerts.ListAsync(openOnly: true, maxRows: 100)).ShouldContain(a => a.CardNumber == "AB-0001");
        (await alerts.ListAsync(openOnly: true, maxRows: 100)).ShouldNotContain(a => a.CardNumber == "AB-0002");
        (await holders.GetCardNumbersByOrganizationAsync(seed.OrganizationId)).ShouldBe(["AB-0001"]);
    }
}
