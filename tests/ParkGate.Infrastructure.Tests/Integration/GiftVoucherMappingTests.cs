using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class GiftVoucherMappingTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public GiftVoucherMappingTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    private sealed record Seed(int OrganizationId, Guid BeneficiaryId, int UserId, int WorkstationId);

    private static async Task<Seed> SeedAsync(ParkGateDbContext context, bool giftEnabled = true, decimal giftValue = 12.345m, int intervalDays = 30)
    {
        var org = new Organization($"GFT_{Guid.NewGuid():N}"[..10], "جهة الهدايا");
        org.ConfigureGift(giftEnabled, giftValue, intervalDays);

        var user = new User($"gift_{Guid.NewGuid():N}"[..12], "موظف الهدايا", "hash", UserRole.Supervisor);
        var ws = new Workstation($"GWS_{Guid.NewGuid():N}"[..10], "شباك الهدايا");

        context.Organizations.Add(org);
        context.Users.Add(user);
        context.Workstations.Add(ws);
        await context.SaveChangesAsync();

        var holder = new Beneficiary(
            Guid.NewGuid(),
            org.Id,
            CardNumber.Create($"GFT-{Guid.NewGuid():N}"[..12]),
            "زبون الهدية",
            PhoneNumber.Create($"091{Random.Shared.Next(1_000_000):D7}"));

        context.Beneficiaries.Add(holder);
        await context.SaveChangesAsync();

        return new Seed(org.Id, holder.Id, user.Id, ws.Id);
    }

    private static GiftVoucher NewVoucher(Seed seed, decimal value = 12.345m, DateTime? issuedAtUtc = null)
    {
        return GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: seed.BeneficiaryId,
            organizationId: seed.OrganizationId,
            value: value,
            issuedAtUtc: issuedAtUtc ?? DateTime.UtcNow,
            businessDate: DateOnly.FromDateTime(DateTime.UtcNow),
            issuedByUserId: seed.UserId,
            issuedWorkstationId: seed.WorkstationId);
    }

    [Fact]
    public async Task GiftVoucher_RoundTripAllColumns_ShouldPersist()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context);

        var issuedAt = new DateTime(2026, 11, 1, 9, 30, 15, 250, DateTimeKind.Utc);
        var voucher = GiftVoucher.Issue(
            token: Guid.NewGuid(),
            keyId: 1,
            beneficiaryId: seed.BeneficiaryId,
            organizationId: seed.OrganizationId,
            value: 12.345m,
            issuedAtUtc: issuedAt,
            businessDate: new DateOnly(2026, 11, 1),
            issuedByUserId: seed.UserId,
            issuedWorkstationId: seed.WorkstationId,
            overrideByUserId: seed.UserId,
            overrideReason: "تجاوز الفترة بقرار المشرف");

        context.GiftVouchers.Add(voucher);
        await context.SaveChangesAsync();

        voucher.Id.ShouldBeGreaterThan(0);
        voucher.ExpiresOn.ShouldBe(new DateOnly(2026, 11, 1));

        await using var reloadContext = _fixture.CreateDbContext();
        var reloaded = await reloadContext.GiftVouchers.SingleAsync(v => v.Id == voucher.Id);

        reloaded.Token.ShouldBe(voucher.Token);
        reloaded.KeyId.ShouldBe((byte)1);
        reloaded.BeneficiaryId.ShouldBe(seed.BeneficiaryId);
        reloaded.OrganizationId.ShouldBe(seed.OrganizationId);
        reloaded.Value.ShouldBe(12.345m);
        reloaded.IssuedAtUtc.ShouldBe(issuedAt);
        reloaded.BusinessDate.ShouldBe(new DateOnly(2026, 11, 1));
        reloaded.ExpiresOn.ShouldBe(new DateOnly(2026, 11, 1));
        reloaded.Status.ShouldBe(GiftVoucherStatus.Issued);
        reloaded.IssuedByUserId.ShouldBe(seed.UserId);
        reloaded.IssuedWorkstationId.ShouldBe(seed.WorkstationId);
        reloaded.IsOverride.ShouldBeTrue();
        reloaded.OverrideByUserId.ShouldBe(seed.UserId);
        reloaded.OverrideReason.ShouldBe("تجاوز الفترة بقرار المشرف");
        reloaded.RedeemedAtUtc.ShouldBeNull();
        reloaded.RedeemedBusinessDate.ShouldBeNull();
        reloaded.RedeemedByUserId.ShouldBeNull();
        reloaded.RedeemedWorkstationId.ShouldBeNull();
        reloaded.VoidedAtUtc.ShouldBeNull();
        reloaded.VoidedByUserId.ShouldBeNull();
        reloaded.VoidReason.ShouldBeNull();
        reloaded.RowVersion.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GiftVoucher_RedeemAndVoid_ShouldPersistStatusTransitions()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context);

        var toRedeem = NewVoucher(seed);
        var toVoid = NewVoucher(seed);
        context.GiftVouchers.AddRange(toRedeem, toVoid);
        await context.SaveChangesAsync();

        var redeemedAt = new DateTime(2026, 11, 1, 13, 0, 0, DateTimeKind.Utc);
        toRedeem.Redeem(seed.UserId, seed.WorkstationId, redeemedAt, new DateOnly(2026, 11, 1));
        toVoid.Void(seed.UserId, "خطأ في الطباعة", redeemedAt);

        await context.SaveChangesAsync();

        await using var reloadContext = _fixture.CreateDbContext();
        var redeemed = await reloadContext.GiftVouchers.SingleAsync(v => v.Id == toRedeem.Id);
        redeemed.Status.ShouldBe(GiftVoucherStatus.Redeemed);
        redeemed.RedeemedAtUtc.ShouldBe(redeemedAt);
        redeemed.RedeemedBusinessDate.ShouldBe(new DateOnly(2026, 11, 1));
        redeemed.RedeemedByUserId.ShouldBe(seed.UserId);
        redeemed.RedeemedWorkstationId.ShouldBe(seed.WorkstationId);

        var voided = await reloadContext.GiftVouchers.SingleAsync(v => v.Id == toVoid.Id);
        voided.Status.ShouldBe(GiftVoucherStatus.Voided);
        voided.VoidReason.ShouldBe("خطأ في الطباعة");
        voided.VoidedByUserId.ShouldBe(seed.UserId);
    }

    [Fact]
    public async Task GiftVoucher_Ids_ShouldIncrease()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context);

        var first = NewVoucher(seed);
        context.GiftVouchers.Add(first);
        await context.SaveChangesAsync();

        var second = NewVoucher(seed);
        context.GiftVouchers.Add(second);
        await context.SaveChangesAsync();

        second.Id.ShouldBeGreaterThan(first.Id);
    }

    [Fact]
    public async Task GiftVoucher_DuplicateToken_ShouldThrowDbUpdateException()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context);

        var token = Guid.NewGuid();
        var first = GiftVoucher.Issue(
            token, 1, seed.BeneficiaryId, seed.OrganizationId, 5m,
            DateTime.UtcNow, DateOnly.FromDateTime(DateTime.UtcNow), seed.UserId, seed.WorkstationId);
        context.GiftVouchers.Add(first);
        await context.SaveChangesAsync();

        var duplicate = GiftVoucher.Issue(
            token, 1, seed.BeneficiaryId, seed.OrganizationId, 5m,
            DateTime.UtcNow, DateOnly.FromDateTime(DateTime.UtcNow), seed.UserId, seed.WorkstationId);
        context.GiftVouchers.Add(duplicate);

        await Should.ThrowAsync<DbUpdateException>(async () => await context.SaveChangesAsync());
    }

    [Fact]
    public async Task GiftVoucher_StatusCheck_RejectsRedeemedWithoutRedeemer()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context);

        var voucher = NewVoucher(seed);
        context.GiftVouchers.Add(voucher);
        await context.SaveChangesAsync();

        await Should.ThrowAsync<SqlException>(async () =>
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE dbo.GiftVouchers SET [Status] = 2 WHERE [Id] = {voucher.Id}"));
    }

    [Fact]
    public async Task GiftVoucher_OverrideCheck_RejectsOverrideWithoutReason()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context);

        var voucher = NewVoucher(seed);
        context.GiftVouchers.Add(voucher);
        await context.SaveChangesAsync();

        await Should.ThrowAsync<SqlException>(async () =>
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE dbo.GiftVouchers SET [IsOverride] = 1, [OverrideByUserId] = {seed.UserId}, [OverrideReason] = NULL WHERE [Id] = {voucher.Id}"));
    }

    [Fact]
    public async Task GiftVoucher_ValueCheck_RejectsZeroValue()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context);

        var voucher = NewVoucher(seed);
        context.GiftVouchers.Add(voucher);
        await context.SaveChangesAsync();

        await Should.ThrowAsync<SqlException>(async () =>
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE dbo.GiftVouchers SET [Value] = 0 WHERE [Id] = {voucher.Id}"));
    }

    [Fact]
    public async Task Organization_GiftPolicyColumns_ShouldRoundTrip()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context, giftEnabled: true, giftValue: 25.500m, intervalDays: 45);

        await using var reloadContext = _fixture.CreateDbContext();
        var org = await reloadContext.Organizations.SingleAsync(o => o.Id == seed.OrganizationId);

        org.GiftEnabled.ShouldBeTrue();
        org.GiftValue.ShouldBe(25.500m);
        org.GiftIntervalDays.ShouldBe(45);
        org.MaxFreeCompanions.ShouldBeNull();
    }

    [Fact]
    public async Task Organization_MaxFreeCompanions_ShouldRoundTrip()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context);

        await using (var updateContext = _fixture.CreateDbContext())
        {
            var org = await updateContext.Organizations.SingleAsync(o => o.Id == seed.OrganizationId);
            org.SetMaxFreeCompanions(3);
            await updateContext.SaveChangesAsync();
        }

        await using var reloadContext = _fixture.CreateDbContext();
        var reloaded = await reloadContext.Organizations.SingleAsync(o => o.Id == seed.OrganizationId);
        reloaded.MaxFreeCompanions.ShouldBe(3);
    }

    [Fact]
    public async Task Organization_GiftIntervalDaysCheck_RejectsZero()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context);

        await Should.ThrowAsync<SqlException>(async () =>
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE dbo.Organizations SET [GiftIntervalDays] = 0 WHERE [Id] = {seed.OrganizationId}"));
    }

    [Fact]
    public async Task Organization_GiftValueCheck_RejectsEnabledWithZeroValue()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context);

        await Should.ThrowAsync<SqlException>(async () =>
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE dbo.Organizations SET [GiftEnabled] = 1, [GiftValue] = 0 WHERE [Id] = {seed.OrganizationId}"));
    }

    [Fact]
    public async Task Organization_MaxFreeCompanionsCheck_RejectsOutOfRange()
    {
        await using var context = _fixture.CreateDbContext();
        var seed = await SeedAsync(context);

        await Should.ThrowAsync<SqlException>(async () =>
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE dbo.Organizations SET [MaxFreeCompanions] = 51 WHERE [Id] = {seed.OrganizationId}"));
    }

    [Fact]
    public async Task SeededAgeCategories_InfantShouldBeExemptAndOthersNot()
    {
        await using var context = _fixture.CreateDbContext();

        var categories = await context.AgeCategories
            .Where(c => c.Id == 1 || c.Id == 2 || c.Id == 3)
            .ToListAsync();

        categories.Single(c => c.Id == 1).IsCompanionLimitExempt.ShouldBeFalse();
        categories.Single(c => c.Id == 2).IsCompanionLimitExempt.ShouldBeFalse();
        categories.Single(c => c.Id == 3).IsCompanionLimitExempt.ShouldBeTrue();
        categories.Single(c => c.Id == 3).NameAr.ShouldBe("رضيع");
    }

    [Fact]
    public async Task AgeCategory_CompanionLimitExempt_ShouldPersist()
    {
        await using var context = _fixture.CreateDbContext();

        var category = new AgeCategory($"فئة {Guid.NewGuid():N}"[..12], "وصف", 90, isActive: true);
        category.SetCompanionLimitExempt(true);
        context.AgeCategories.Add(category);
        await context.SaveChangesAsync();

        await using var reloadContext = _fixture.CreateDbContext();
        var reloaded = await reloadContext.AgeCategories.SingleAsync(c => c.Id == category.Id);
        reloaded.IsCompanionLimitExempt.ShouldBeTrue();
    }
}
