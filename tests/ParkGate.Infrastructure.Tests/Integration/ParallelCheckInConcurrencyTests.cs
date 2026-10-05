using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Data;

namespace ParkGate.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class ParallelCheckInConcurrencyTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;
    private readonly IEntryEligibilityPolicy _policy = new RollingWindowEligibilityPolicy();

    public ParallelCheckInConcurrencyTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TwoDesks_CheckingInSameHolderConcurrently_ExactlyOneAllowedAndOneDenied_20Runs()
    {
        var uowFactory = new SimpleUnitOfWorkFactory(_fixture);

        // Pre-create shared Org, Workstations, and Users once
        await using var setupContext = _fixture.CreateDbContext();
        var org = new Organization($"CONC_{Guid.NewGuid():N}"[..10], "جهة التزامن", cooldownMinutes: 720);
        var ws1 = new Workstation($"WS1_{Guid.NewGuid():N}"[..10], "شباك 1");
        var ws2 = new Workstation($"WS2_{Guid.NewGuid():N}"[..10], "شباك 2");
        var user1 = new User($"op1_{Guid.NewGuid():N}"[..10], "مشغل 1", "hash", UserRole.Operator);
        var user2 = new User($"op2_{Guid.NewGuid():N}"[..10], "مشغل 2", "hash", UserRole.Operator);

        setupContext.Organizations.Add(org);
        setupContext.Workstations.AddRange(ws1, ws2);
        setupContext.Users.AddRange(user1, user2);
        await setupContext.SaveChangesAsync();

        // Run 20 consecutive concurrency trials per ROADMAP.md T2.6
        for (var run = 1; run <= 20; run++)
        {
            // Create a fresh holder for each trial with no prior entries
            var holderId = Guid.NewGuid();
            var holder = new Beneficiary(
                holderId,
                org.Id,
                CardNumber.Create($"CARD-CONC-{run:D2}"),
                $"حامل التزامن {run}",
                PhoneNumber.Create($"091000{run:D4}"));

            setupContext.Beneficiaries.Add(holder);
            await setupContext.SaveChangesAsync();

            // Launch two parallel check-in requests competing for the same holder row
            using var barrier = new Barrier(2);

            var task1 = Task.Run(async () =>
            {
                barrier.SignalAndWait();
                return await ExecuteCheckInAsync(uowFactory, _policy, holderId, ws1.Id, user1.Id, 720);
            });

            var task2 = Task.Run(async () =>
            {
                barrier.SignalAndWait();
                return await ExecuteCheckInAsync(uowFactory, _policy, holderId, ws2.Id, user2.Id, 720);
            });

            var results = await Task.WhenAll(task1, task2);

            // Assert: Exactly one Allowed, and exactly one Denied (CooldownActive)
            results.Count(r => r.Result == EntryResult.Allowed).ShouldBe(1, $"Run {run}: Expected exactly one Allowed");
            results.Count(r => r.Result == EntryResult.Denied).ShouldBe(1, $"Run {run}: Expected exactly one Denied");

            var deniedResult = results.Single(r => r.Result == EntryResult.Denied);
            deniedResult.DenialReason.ShouldBe(DenialReason.CooldownActive, $"Run {run}: Expected DenialReason.CooldownActive");
        }
    }

    private static async Task<(EntryResult Result, DenialReason? DenialReason)> ExecuteCheckInAsync(
        IUnitOfWorkFactory uowFactory,
        IEntryEligibilityPolicy policy,
        Guid beneficiaryId,
        int workstationId,
        int userId,
        int defaultCooldownMinutes)
    {
        await using var uow = uowFactory.Create();
        await uow.BeginTransactionAsync();

        // 1) Lock the holder row with UPDLOCK, ROWLOCK per DATABASE.md §4
        var holder = await uow.Beneficiaries.GetForUpdateAsync(beneficiaryId);
        if (holder == null)
        {
            throw new InvalidOperationException("Holder not found");
        }

        var org = await uow.Organizations.GetByIdAsync(holder.OrganizationId);
        if (org == null)
        {
            throw new InvalidOperationException("Organization not found");
        }

        // 2) Authoritative DB time per ADR-004 & BR-05
        var nowUtc = await uow.GetDbUtcNowAsync();
        var businessDate = DateOnly.FromDateTime(nowUtc);

        // 3) Domain policy evaluation
        var eligibilityContext = new EligibilityContext(org, holder, defaultCooldownMinutes, nowUtc, businessDate);
        var effectiveCooldown = org.CooldownMinutes ?? defaultCooldownMinutes;

        var decision = policy.Evaluate(eligibilityContext);

        if (decision.IsEligible)
        {
            var attempt = EntryAttempt.CreateAllowed(
                id: Guid.NewGuid(),
                beneficiaryId: holder.Id,
                organizationId: org.Id,
                attemptedAtUtc: nowUtc,
                businessDate: businessDate,
                cooldownMinutesApplied: effectiveCooldown,
                nextEligibleAtUtc: decision.NextEligibleAtUtc!.Value,
                workstationId: workstationId,
                userId: userId,
                channel: EntryChannel.Desk);

            uow.EntryAttempts.Add(attempt);
            holder.RecordGrantedEntry(nowUtc);

            await uow.SaveChangesAsync();
            await uow.CommitAsync();

            return (EntryResult.Allowed, null);
        }
        else
        {
            var attempt = EntryAttempt.CreateDenied(
                id: Guid.NewGuid(),
                beneficiaryId: holder.Id,
                organizationId: org.Id,
                attemptedAtUtc: nowUtc,
                businessDate: businessDate,
                denialReason: decision.DenialReason!.Value,
                cooldownMinutesApplied: effectiveCooldown,
                nextEligibleAtUtc: decision.NextEligibleAtUtc,
                workstationId: workstationId,
                userId: userId,
                channel: EntryChannel.Desk);

            uow.EntryAttempts.Add(attempt);

            await uow.SaveChangesAsync();
            await uow.CommitAsync();

            return (EntryResult.Denied, decision.DenialReason);
        }
    }

    private sealed class SimpleUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly LocalDbTestFixture _fixture;

        public SimpleUnitOfWorkFactory(LocalDbTestFixture fixture)
        {
            _fixture = fixture;
        }

        public IUnitOfWork Create()
        {
            return _fixture.CreateUnitOfWork();
        }
    }
}
