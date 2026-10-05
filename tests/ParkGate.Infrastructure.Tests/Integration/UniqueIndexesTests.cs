using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class UniqueIndexesTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public UniqueIndexesTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DuplicateOrganizationCode_ShouldThrowDbUpdateException()
    {
        await using var context = _fixture.CreateDbContext();
        var org1 = new Organization("DUPORG", "الجهة الأولى");
        var org2 = new Organization("DUPORG", "الجهة الثانية");

        context.Organizations.Add(org1);
        await context.SaveChangesAsync();

        context.Organizations.Add(org2);
        await Should.ThrowAsync<DbUpdateException>(async () =>
        {
            await context.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task DuplicateOrgAndCard_ShouldThrowDbUpdateException()
    {
        await using var context = _fixture.CreateDbContext();
        var org = new Organization($"ORG_{Guid.NewGuid():N}"[..10], "جهة البطاقات");
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        var card = CardNumber.Create("CARD-SAME-01");
        var b1 = new Beneficiary(Guid.NewGuid(), org.Id, card, "حامل 1", PhoneNumber.Create("0911111111"));
        var b2 = new Beneficiary(Guid.NewGuid(), org.Id, card, "حامل 2", PhoneNumber.Create("0922222222"));

        context.Beneficiaries.Add(b1);
        await context.SaveChangesAsync();

        context.Beneficiaries.Add(b2);
        await Should.ThrowAsync<DbUpdateException>(async () =>
        {
            await context.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task SameCardNumber_InDifferentOrganizations_ShouldSucceed()
    {
        await using var context = _fixture.CreateDbContext();
        var orgA = new Organization($"ORG_A_{Guid.NewGuid():N}"[..10], "جهة أ");
        var orgB = new Organization($"ORG_B_{Guid.NewGuid():N}"[..10], "جهة ب");
        context.Organizations.AddRange(orgA, orgB);
        await context.SaveChangesAsync();

        var card = CardNumber.Create("SHARED-CARD-01");
        var b1 = new Beneficiary(Guid.NewGuid(), orgA.Id, card, "حامل جهة أ", PhoneNumber.Create("0913333333"));
        var b2 = new Beneficiary(Guid.NewGuid(), orgB.Id, card, "حامل جهة ب", PhoneNumber.Create("0924444444"));

        context.Beneficiaries.AddRange(b1, b2);
        await context.SaveChangesAsync();

        b1.Id.ShouldNotBe(Guid.Empty);
        b2.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task DuplicateNationalId_ShouldThrowDbUpdateException()
    {
        await using var context = _fixture.CreateDbContext();
        var org = new Organization($"ORG_{Guid.NewGuid():N}"[..10], "جهة الوطنية");
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        var nid = NationalId.Create("119950555555");
        var b1 = new Beneficiary(Guid.NewGuid(), org.Id, CardNumber.Create("NID-CARD-1"), "حامل 1", PhoneNumber.Create("0915555555"), nid);
        var b2 = new Beneficiary(Guid.NewGuid(), org.Id, CardNumber.Create("NID-CARD-2"), "حامل 2", PhoneNumber.Create("0926666666"), nid);

        context.Beneficiaries.Add(b1);
        await context.SaveChangesAsync();

        context.Beneficiaries.Add(b2);
        await Should.ThrowAsync<DbUpdateException>(async () =>
        {
            await context.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task MultipleNullNationalIds_ShouldBeAllowedByFilteredIndex()
    {
        await using var context = _fixture.CreateDbContext();
        var org = new Organization($"ORG_{Guid.NewGuid():N}"[..10], "جهة بدون رقم وطني");
        context.Organizations.Add(org);
        await context.SaveChangesAsync();

        var b1 = new Beneficiary(Guid.NewGuid(), org.Id, CardNumber.Create("NONID-1"), "حامل بدون رقم 1", PhoneNumber.Create("0917777777"), nationalId: null);
        var b2 = new Beneficiary(Guid.NewGuid(), org.Id, CardNumber.Create("NONID-2"), "حامل بدون رقم 2", PhoneNumber.Create("0928888888"), nationalId: null);

        context.Beneficiaries.AddRange(b1, b2);
        await context.SaveChangesAsync();

        b1.Id.ShouldNotBe(Guid.Empty);
        b2.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task MultipleDefaultOrganizations_ShouldThrowDbUpdateException()
    {
        await using var context = _fixture.CreateDbContext();
        // Database already has one seeded default organization (SSF)
        var secondDefaultOrg = new Organization($"DEF_{Guid.NewGuid():N}"[..10], "جهة افتراضية ثانية", isDefault: true);

        context.Organizations.Add(secondDefaultOrg);
        await Should.ThrowAsync<DbUpdateException>(async () =>
        {
            await context.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task DuplicateUserName_ShouldThrowDbUpdateException()
    {
        await using var context = _fixture.CreateDbContext();
        var u1 = new User("admin_dup", "مدير 1", "hash", UserRole.Admin);
        var u2 = new User("ADMIN_DUP", "مدير 2", "hash", UserRole.Admin);

        context.Users.Add(u1);
        await context.SaveChangesAsync();

        context.Users.Add(u2);
        await Should.ThrowAsync<DbUpdateException>(async () =>
        {
            await context.SaveChangesAsync();
        });
    }
}
