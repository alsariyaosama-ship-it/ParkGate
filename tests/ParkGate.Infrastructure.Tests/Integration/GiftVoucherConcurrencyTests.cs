using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>
/// Verifies the two atomic gift flows (BR-22 issue, BR-24 redeem) under real parallel load
/// against SQL Server, mirroring <see cref="ParallelCheckInConcurrencyTests"/>.
/// </summary>
[Trait("Category", "Integration")]
public sealed class GiftVoucherConcurrencyTests : IClassFixture<LocalDbTestFixture>
{
    private const int Parallelism = 10;
    private const int Runs = 20;

    private readonly LocalDbTestFixture _fixture;
    private readonly IGiftEligibilityPolicy _policy = new RollingIntervalGiftEligibilityPolicy();

    public GiftVoucherConcurrencyTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ParallelRedeem_SameToken_ExactlyOneSucceeds_20Runs()
    {
        var factory = new SimpleUnitOfWorkFactory(_fixture);

        await using var setupContext = _fixture.CreateDbContext();
        var org = new Organization($"RED_{Guid.NewGuid():N}"[..10], "جهة الاسترداد");
        org.ConfigureGift(true, 10m, 30);
        var user = new User($"red_{Guid.NewGuid():N}"[..12], "موظف الاسترداد", "hash", UserRole.Supervisor);
        var ws = new Workstation($"RWS_{Guid.NewGuid():N}"[..10], "شباك الألعاب");
        setupContext.Organizations.Add(org);
        setupContext.Users.Add(user);
        setupContext.Workstations.Add(ws);
        await setupContext.SaveChangesAsync();

        var holder = new Beneficiary(
            Guid.NewGuid(),
            org.Id,
            CardNumber.Create($"RED-{Guid.NewGuid():N}"[..12]),
            "زبون الاسترداد",
            PhoneNumber.Create($"091{Random.Shared.Next(1_000_000):D7}"));
        setupContext.Beneficiaries.Add(holder);
        await setupContext.SaveChangesAsync();

        for (var run = 1; run <= Runs; run++)
        {
            var token = Guid.NewGuid();
            var nowUtc = DateTime.UtcNow;
            var voucher = GiftVoucher.Issue(
                token, 1, holder.Id, org.Id, 10m, nowUtc, DateOnly.FromDateTime(nowUtc), user.Id, ws.Id);

            await using (var issueContext = _fixture.CreateDbContext())
            {
                issueContext.GiftVouchers.Add(voucher);
                await issueContext.SaveChangesAsync();
            }

            var results = await RunInParallelAsync(
                () => RedeemOnceAsync(factory, token, user.Id, ws.Id));

            results.Count(success => success).ShouldBe(
                1, $"Run {run}: exactly one redemption must succeed, got {results.Count(s => s)}");

            await using var verifyContext = _fixture.CreateDbContext();
            var stored = await verifyContext.GiftVouchers.SingleAsync(v => v.Token == token);
            stored.Status.ShouldBe(GiftVoucherStatus.Redeemed);
        }
    }

    [Fact]
    public async Task ParallelIssue_SameHolder_ExactlyOneVoucher_20Runs()
    {
        var factory = new SimpleUnitOfWorkFactory(_fixture);

        await using var setupContext = _fixture.CreateDbContext();
        var org = new Organization($"ISS_{Guid.NewGuid():N}"[..10], "جهة الإصدار");
        org.ConfigureGift(true, 15m, 30);
        var user = new User($"iss_{Guid.NewGuid():N}"[..12], "موظف الإصدار", "hash", UserRole.Supervisor);
        var ws = new Workstation($"IWS_{Guid.NewGuid():N}"[..10], "شباك البوابة");
        setupContext.Organizations.Add(org);
        setupContext.Users.Add(user);
        setupContext.Workstations.Add(ws);
        await setupContext.SaveChangesAsync();

        for (var run = 1; run <= Runs; run++)
        {
            var holder = new Beneficiary(
                Guid.NewGuid(),
                org.Id,
                CardNumber.Create($"ISS-{Guid.NewGuid():N}"[..12]),
                $"زبون الإصدار {run}",
                PhoneNumber.Create($"092{Random.Shared.Next(1_000_000):D7}"));

            setupContext.Beneficiaries.Add(holder);
            await setupContext.SaveChangesAsync();

            var results = await RunInParallelAsync(
                () => IssueOnceAsync(factory, holder.Id, user.Id, ws.Id));

            results.Count(success => success).ShouldBe(
                1, $"Run {run}: exactly one voucher must be issued, got {results.Count(s => s)}");

            await using var verifyContext = _fixture.CreateDbContext();
            var count = await verifyContext.GiftVouchers.CountAsync(v => v.BeneficiaryId == holder.Id);
            count.ShouldBe(1, $"Run {run}: the database must hold exactly one voucher for the holder");
        }
    }

    private static async Task<bool[]> RunInParallelAsync(Func<Task<bool>> action)
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var tasks = Enumerable.Range(0, Parallelism)
            .Select(_ => Task.Run(async () =>
            {
                await gate.Task;
                return await action();
            }))
            .ToArray();

        // Give every worker time to park on the gate so the database work really overlaps.
        await Task.Delay(100);
        gate.SetResult();

        return await Task.WhenAll(tasks);
    }

    private static async Task<bool> RedeemOnceAsync(
        IUnitOfWorkFactory factory,
        Guid token,
        int userId,
        int workstationId)
    {
        await using var uow = factory.Create();
        await uow.BeginTransactionAsync();

        try
        {
            var voucher = await uow.GiftVouchers.GetByTokenForUpdateAsync(token);
            if (voucher is null || voucher.Status != GiftVoucherStatus.Issued)
            {
                await uow.RollbackAsync();
                return false;
            }

            var nowUtc = await uow.GetDbUtcNowAsync();
            voucher.Redeem(userId, workstationId, nowUtc, DateOnly.FromDateTime(nowUtc));

            await uow.SaveChangesAsync();
            await uow.CommitAsync();
            return true;
        }
        catch (SqlException ex) when (ex.Number == 1205)
        {
            // Deadlock victim: the winner already redeemed the voucher.
            await uow.RollbackAsync();
            return false;
        }
    }

    private async Task<bool> IssueOnceAsync(
        IUnitOfWorkFactory factory,
        Guid holderId,
        int userId,
        int workstationId)
    {
        await using var uow = factory.Create();
        await uow.BeginTransactionAsync();

        try
        {
            var holder = await uow.Beneficiaries.GetForUpdateAsync(holderId);
            if (holder is null)
            {
                await uow.RollbackAsync();
                return false;
            }

            var org = await uow.Organizations.GetByIdAsync(holder.OrganizationId);
            if (org is null)
            {
                await uow.RollbackAsync();
                return false;
            }

            var nowUtc = await uow.GetDbUtcNowAsync();
            var lastIssuedUtc = await uow.GiftVouchers.GetLatestIssuedUtcAsync(holderId);
            var decision = _policy.Evaluate(new GiftEligibilityContext(org, holder, lastIssuedUtc, nowUtc));

            if (!decision.IsEligible)
            {
                await uow.RollbackAsync();
                return false;
            }

            var voucher = GiftVoucher.Issue(
                Guid.NewGuid(), 1, holderId, org.Id, org.GiftValue,
                nowUtc, DateOnly.FromDateTime(nowUtc), userId, workstationId);

            uow.GiftVouchers.Add(voucher);
            await uow.SaveChangesAsync();
            await uow.CommitAsync();
            return true;
        }
        catch (SqlException ex) when (ex.Number == 1205)
        {
            await uow.RollbackAsync();
            return false;
        }
    }

    private sealed class SimpleUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly LocalDbTestFixture _fixture;

        public SimpleUnitOfWorkFactory(LocalDbTestFixture fixture)
        {
            _fixture = fixture;
        }

        public IUnitOfWork Create() => _fixture.CreateUnitOfWork();
    }
}
