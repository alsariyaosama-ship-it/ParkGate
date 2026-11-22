using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>F9, BR-15 and BR-32: the seeded cities, the optional phone and the optional city of a holder.</summary>
[Trait("Category", "Integration")]
public sealed class CityMappingTests : IClassFixture<LocalDbTestFixture>
{
    private const int SeededCityCount = 57;

    private readonly LocalDbTestFixture _fixture;

    public CityMappingTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Cities_AfterMigration_Has57SeededRowsInTheFixedOrder()
    {
        await using var context = _fixture.CreateDbContext();

        var cities = await context.Cities.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync();

        cities.Count.ShouldBe(SeededCityCount);
        cities.First().NameAr.ShouldBe("الخمس");
        cities.Last().NameAr.ShouldBe("يفرن");
        cities.Select(c => c.SortOrder).ShouldBe(Enumerable.Range(1, SeededCityCount));
        cities.Select(c => c.Id).ShouldBe(Enumerable.Range(1, SeededCityCount));
        cities.ShouldAllBe(c => c.IsActive);
        cities.Select(c => c.NameAr).Distinct().Count().ShouldBe(SeededCityCount);
    }

    [Fact]
    public async Task CityRepository_GetAllActiveAsync_ReturnsTheSeedInItsFixedOrder()
    {
        await using var uow = _fixture.CreateUnitOfWork();

        var cities = await uow.Cities.GetAllActiveAsync();

        cities.Count.ShouldBe(SeededCityCount);
        cities.Take(4).Select(c => c.NameAr).ShouldBe(["الخمس", "زليتن", "مصراتة", "طرابلس"]);
        (await uow.Cities.GetByIdAsync(1))!.NameAr.ShouldBe("الخمس");
        (await uow.Cities.GetByIdAsync(SeededCityCount + 1)).ShouldBeNull();
    }

    [Fact]
    public async Task Beneficiary_WithNoPhoneAndACity_ShouldRoundTrip()
    {
        var orgId = await AddOrganizationAsync("CITY1", "جهة المدن 1");
        var beneficiaryId = Guid.NewGuid();
        await using (var context = _fixture.CreateDbContext())
        {
            var beneficiary = new Beneficiary(
                beneficiaryId, orgId, CardNumber.Create("CITY-0001"), "سالم علي", phone: null, cityId: 3);
            context.Beneficiaries.Add(beneficiary);
            await context.SaveChangesAsync();
        }

        await using var readContext = _fixture.CreateDbContext();
        var loaded = await readContext.Beneficiaries.AsNoTracking().SingleAsync(b => b.Id == beneficiaryId);

        loaded.Phone.ShouldBeNull();
        loaded.CityId.ShouldBe(3);
    }

    [Fact]
    public async Task Beneficiary_WithNoPhoneAndNoCity_ShouldRoundTrip()
    {
        var orgId = await AddOrganizationAsync("CITY2", "جهة المدن 2");
        var beneficiaryId = Guid.NewGuid();
        await using (var context = _fixture.CreateDbContext())
        {
            context.Beneficiaries.Add(new Beneficiary(beneficiaryId, orgId, CardNumber.Create("CITY-0002"), "سالم علي", null));
            await context.SaveChangesAsync();
        }

        await using var readContext = _fixture.CreateDbContext();
        var loaded = await readContext.Beneficiaries.AsNoTracking().SingleAsync(b => b.Id == beneficiaryId);

        loaded.Phone.ShouldBeNull();
        loaded.CityId.ShouldBeNull();
    }

    [Fact]
    public async Task Beneficiary_WithAGivenPhone_StillRoundTripsNormalized()
    {
        var orgId = await AddOrganizationAsync("CITY3", "جهة المدن 3");
        var beneficiaryId = Guid.NewGuid();
        await using (var context = _fixture.CreateDbContext())
        {
            context.Beneficiaries.Add(new Beneficiary(
                beneficiaryId, orgId, CardNumber.Create("CITY-0003"), "سالم علي", PhoneNumber.Create("+218912345678")));
            await context.SaveChangesAsync();
        }

        await using var readContext = _fixture.CreateDbContext();
        var loaded = await readContext.Beneficiaries.AsNoTracking().SingleAsync(b => b.Id == beneficiaryId);

        loaded.Phone!.Value.ShouldBe("0912345678");
    }

    [Fact]
    public async Task Beneficiary_WithUnknownCityId_ShouldBeRejectedByTheForeignKey()
    {
        var orgId = await AddOrganizationAsync("CITY4", "جهة المدن 4");
        await using var context = _fixture.CreateDbContext();
        context.Beneficiaries.Add(new Beneficiary(
            Guid.NewGuid(), orgId, CardNumber.Create("CITY-0004"), "سالم علي", null, cityId: 9999));

        await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private async Task<int> AddOrganizationAsync(string code, string name)
    {
        await using var context = _fixture.CreateDbContext();
        var org = new Organization(code, name, cooldownMinutes: 120, isDefault: false, isActive: true);
        context.Organizations.Add(org);
        await context.SaveChangesAsync();
        return org.Id;
    }
}
