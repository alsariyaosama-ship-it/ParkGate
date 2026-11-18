using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Common;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>
/// The two atomic gift flows (BR-22 issue, BR-24 redeem) under real parallel load against SQL Server. Since R6.13
/// they run the real <c>GiftVoucherService</c>, whose optimistic concurrency and retry replace the row lock (ADR-026).
/// </summary>
[Trait("Category", "Integration")]
public sealed class GiftVoucherConcurrencyTests : IClassFixture<LocalDbTestFixture>
{
    private const int Parallelism = 10;
    private const int Runs = 20;

    private readonly LocalDbTestFixture _fixture;

    public GiftVoucherConcurrencyTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ParallelRedeem_SameToken_ExactlyOneSucceeds_20Runs()
    {
        using var harness = new ServiceHarness(_fixture);
        var (org, user, desk) = await SeedAsync("RED", 10m);
        var gifts = harness.Gifts(user, desk);

        for (var run = 1; run <= Runs; run++)
        {
            var holder = await AddHolderAsync(org, $"زبون الاسترداد {run}");
            var issued = await gifts.IssueAsync(new IssueGiftVoucherRequest(holder.Id));
            issued.IsSuccess.ShouldBeTrue($"Run {run}: the voucher must be issued");

            var results = await ServiceHarness.RunTogetherAsync(Parallelism, _ => gifts.RedeemAsync(issued.Value.QrText));

            results.Count(r => r.IsSuccess).ShouldBe(1, $"Run {run}: exactly one redemption must succeed");
            results.Where(r => r.IsFailure).ShouldAllBe(r => r.Error.Code == Errors.Gift.AlreadyRedeemed.Code);

            await using var verify = _fixture.CreateDbContext();
            var voucher = await verify.GiftVouchers.SingleAsync(v => v.Id == issued.Value.ReceiptNo);
            voucher.Status.ShouldBe(GiftVoucherStatus.Redeemed);
        }
    }

    [Fact]
    public async Task ParallelIssue_SameHolder_ExactlyOneVoucher_20Runs()
    {
        using var harness = new ServiceHarness(_fixture);
        var (org, user, desk) = await SeedAsync("ISS", 15m);
        var gifts = harness.Gifts(user, desk);

        for (var run = 1; run <= Runs; run++)
        {
            var holder = await AddHolderAsync(org, $"زبون الإصدار {run}");

            var results = await ServiceHarness.RunTogetherAsync(
                Parallelism, _ => gifts.IssueAsync(new IssueGiftVoucherRequest(holder.Id)));

            results.Count(r => r.IsSuccess).ShouldBe(1, $"Run {run}: exactly one voucher must be issued");
            results.Where(r => r.IsFailure).ShouldAllBe(r => r.Error.Code == Errors.Gift.IntervalNotElapsed.Code);

            await using var verify = _fixture.CreateDbContext();
            var vouchers = await verify.GiftVouchers.Where(v => v.BeneficiaryId == holder.Id).ToListAsync();
            vouchers.Count.ShouldBe(1, $"Run {run}: the database must hold exactly one voucher for the holder");
            var stored = await verify.Beneficiaries.SingleAsync(b => b.Id == holder.Id);
            stored.LastGiftIssuedUtc.ShouldBe(vouchers[0].IssuedAtUtc);
        }
    }

    [Fact]
    public async Task VoidGift_ResetsLastGiftIssuedToTheRemainingVoucher()
    {
        using var harness = new ServiceHarness(_fixture);
        var (org, supervisor, desk) = await SeedAsync("VGF", 5m, UserRole.Supervisor);
        var gifts = harness.Gifts(supervisor, desk);
        var holder = await AddHolderAsync(org, "زبون إلغاء الهدية");

        var issued = await gifts.IssueAsync(new IssueGiftVoucherRequest(holder.Id));
        issued.IsSuccess.ShouldBeTrue();

        var voided = await gifts.VoidAsync(new VoidGiftVoucherRequest(issued.Value.ReceiptNo, "إصدار بالخطأ"));

        voided.IsSuccess.ShouldBeTrue();
        await using var verify = _fixture.CreateDbContext();
        (await verify.Beneficiaries.SingleAsync(b => b.Id == holder.Id)).LastGiftIssuedUtc.ShouldBeNull();
    }

    private async Task<(Organization Org, User User, Workstation Desk)> SeedAsync(string prefix, decimal value, UserRole role = UserRole.Operator)
    {
        await using var context = _fixture.CreateDbContext();
        var org = new Organization($"{prefix}_{Guid.NewGuid():N}"[..10], $"جهة {prefix}");
        org.ConfigureGift(true, value, 30);
        var user = new User($"{prefix.ToLowerInvariant()}_{Guid.NewGuid():N}"[..12], $"موظف {prefix}", "hash", role);
        var desk = new Workstation($"{prefix}W_{Guid.NewGuid():N}"[..10], $"شباك {prefix}");
        context.Organizations.Add(org);
        context.Users.Add(user);
        context.Workstations.Add(desk);
        await context.SaveChangesAsync();
        return (org, user, desk);
    }

    private async Task<Beneficiary> AddHolderAsync(Organization org, string name)
    {
        await using var context = _fixture.CreateDbContext();
        var holder = new Beneficiary(
            Guid.NewGuid(),
            org.Id,
            CardNumber.Create($"G-{Guid.NewGuid():N}"[..12]),
            name,
            PhoneNumber.Create($"092{Random.Shared.Next(1_000_000):D7}"));
        context.Beneficiaries.Add(holder);
        await context.SaveChangesAsync();
        return holder;
    }
}
