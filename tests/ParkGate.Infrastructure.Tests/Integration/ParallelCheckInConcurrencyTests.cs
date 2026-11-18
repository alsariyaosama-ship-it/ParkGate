using Microsoft.EntityFrameworkCore;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.Common;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>
/// BR-12: two desks checking in the same holder at the same moment never both get in. Since R6.13 this runs the
/// real <c>CheckInService</c>, whose optimistic concurrency and retry replace the row lock (ADR-026).
/// </summary>
[Trait("Category", "Integration")]
public sealed class ParallelCheckInConcurrencyTests : IClassFixture<LocalDbTestFixture>
{
    private const int Runs = 20;

    private readonly LocalDbTestFixture _fixture;

    public ParallelCheckInConcurrencyTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TwoDesks_CheckingInSameHolderConcurrently_ExactlyOneAllowedAndOneDenied_20Runs()
    {
        using var harness = new ServiceHarness(_fixture);

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

        var desks = new[] { harness.CheckIn(user1, ws1), harness.CheckIn(user2, ws2) };

        for (var run = 1; run <= Runs; run++)
        {
            var holder = new Beneficiary(
                Guid.NewGuid(),
                org.Id,
                CardNumber.Create($"CARD-CONC-{run:D2}"),
                $"حامل التزامن {run}",
                PhoneNumber.Create($"091000{run:D4}"));
            setupContext.Beneficiaries.Add(holder);
            await setupContext.SaveChangesAsync();

            var results = await ServiceHarness.RunTogetherAsync(
                2, desk => desks[desk].RecordEntryAsync(new RecordEntryRequest(holder.Id)));

            results.Count(r => r.IsSuccess && r.Value.Result == EntryResult.Allowed)
                .ShouldBe(1, $"Run {run}: expected exactly one Allowed");
            results.Count(r => r.IsFailure && r.Error.Code == Errors.CheckIn.CooldownActive.Code)
                .ShouldBe(1, $"Run {run}: expected exactly one denial for the active cooldown");

            await using var verify = _fixture.CreateDbContext();
            var attempts = await verify.EntryAttempts.Where(a => a.BeneficiaryId == holder.Id).ToListAsync();
            attempts.Count(a => a.Result == EntryResult.Allowed).ShouldBe(1, $"Run {run}: one Allowed row");
            attempts.Count(a => a.Result == EntryResult.Denied && a.DenialReason == DenialReason.CooldownActive)
                .ShouldBe(1, $"Run {run}: one Denied/CooldownActive row");

            var stored = await verify.Beneficiaries.SingleAsync(b => b.Id == holder.Id);
            stored.LastGrantedEntryUtc.ShouldBe(attempts.Single(a => a.Result == EntryResult.Allowed).AttemptedAtUtc);
        }
    }

    [Fact]
    public async Task VoidAndCheckIn_AtTheSameTime_LeaveLastGrantedEntryConsistent()
    {
        using var harness = new ServiceHarness(_fixture);

        await using var setupContext = _fixture.CreateDbContext();
        var org = new Organization($"VCI_{Guid.NewGuid():N}"[..10], "جهة الإلغاء المتزامن", cooldownMinutes: 1);
        var desk = new Workstation($"VCD_{Guid.NewGuid():N}"[..10], "شباك الإلغاء");
        var supervisor = new User($"vsp_{Guid.NewGuid():N}"[..10], "مشرف", "hash", UserRole.Supervisor);
        setupContext.Organizations.Add(org);
        setupContext.Workstations.Add(desk);
        setupContext.Users.Add(supervisor);
        await setupContext.SaveChangesAsync();

        var holder = new Beneficiary(
            Guid.NewGuid(), org.Id, CardNumber.Create($"VCI-{Guid.NewGuid():N}"[..12]), "حامل الإلغاء", PhoneNumber.Create("0913334444"));
        setupContext.Beneficiaries.Add(holder);
        await setupContext.SaveChangesAsync();

        var service = harness.CheckIn(supervisor, desk);
        var first = await service.RecordEntryAsync(new RecordEntryRequest(holder.Id));
        first.IsSuccess.ShouldBeTrue();

        // Void the entry on one desk while a supervisor overrides the cooldown on another.
        var outcomes = await ServiceHarness.RunTogetherAsync<Result>(2, i => i == 0
            ? service.VoidEntryAsync(new VoidEntryRequest(first.Value.AttemptId, "إدخال بالخطأ"))
            : OverrideAsync(service, holder.Id));

        outcomes.ShouldAllBe(r => r.IsSuccess);

        await using var verify = _fixture.CreateDbContext();
        var latestGranted = await verify.EntryAttempts
            .Where(a => a.BeneficiaryId == holder.Id && !a.IsVoided && a.Result != EntryResult.Denied)
            .MaxAsync(a => (DateTime?)a.AttemptedAtUtc);
        var stored = await verify.Beneficiaries.SingleAsync(b => b.Id == holder.Id);
        stored.LastGrantedEntryUtc.ShouldBe(latestGranted);
    }

    private static async Task<Result> OverrideAsync(Application.CheckIn.Services.CheckInService service, Guid holderId)
    {
        var result = await service.OverrideEntryAsync(new OverrideEntryRequest(holderId, "زيارة استثنائية"));
        return result.IsSuccess ? Result.Success() : result.Error;
    }
}
