using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>F8, T8.2a: <c>Organizations.AllowedEntryDays</c> and <c>GiftAnyDay</c> (DATABASE.md §3.1).</summary>
[Trait("Category", "Integration")]
public sealed class OrganizationEntryDaysMappingTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public OrganizationEntryDaysMappingTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EntryDays_RoundTrip_ShouldPersist()
    {
        int id;
        await using (var context = _fixture.CreateDbContext())
        {
            var org = new Organization($"DAY_{Guid.NewGuid():N}"[..10], "جهة الأيام");
            org.SetEntryDays(WeekDays.Saturday | WeekDays.Wednesday, giftAnyDay: true);
            context.Organizations.Add(org);
            await context.SaveChangesAsync();
            id = org.Id;
        }

        await using (var context = _fixture.CreateDbContext())
        {
            var loaded = await context.Organizations.AsNoTracking().SingleAsync(o => o.Id == id);

            loaded.AllowedEntryDays.ShouldBe(WeekDays.Saturday | WeekDays.Wednesday);
            loaded.GiftAnyDay.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task NewOrganization_DefaultsToEveryDay()
    {
        await using var context = _fixture.CreateDbContext();
        var org = new Organization($"ALL_{Guid.NewGuid():N}"[..10], "جهة كل الأيام");
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        var loaded = await context.Organizations.AsNoTracking().SingleAsync(o => o.Id == org.Id);

        loaded.AllowedEntryDays.ShouldBe(WeekDays.All);
        loaded.GiftAnyDay.ShouldBeFalse();
    }

    [Fact]
    public async Task SeededOrganization_AllowsEveryDay()
    {
        await using var context = _fixture.CreateDbContext();

        var seeded = await context.Organizations.AsNoTracking().SingleAsync(o => o.Code == "SSF");

        seeded.AllowedEntryDays.ShouldBe(WeekDays.All);
    }

    [Fact]
    public async Task NoEntryDay_IsRejectedByTheCheckConstraint()
    {
        await using var context = _fixture.CreateDbContext();
        var org = new Organization($"NON_{Guid.NewGuid():N}"[..10], "جهة بلا أيام");
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        // The domain never allows it; the database refuses it too.
        context.Entry(org).Property(nameof(Organization.AllowedEntryDays)).CurrentValue = WeekDays.None;

        var ex = await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync());
        ex.InnerException!.Message.ShouldContain("CK_Organizations_AllowedEntryDays");
    }
}
