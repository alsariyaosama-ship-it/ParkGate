using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Repositories;

namespace ParkGate.Infrastructure.Tests.Integration;

/// <summary>
/// R3.1: the desk totals are aggregated in SQL over every attempt of the day, not over the rows the grid shows.
/// </summary>
[Trait("Category", "Integration")]
public sealed class TodayTotalsQueryTests : IClassFixture<LocalDbTestFixture>
{
    private static readonly DateOnly Today = new(2026, 11, 14);
    private static readonly DateTime Noon = new(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc);

    private readonly LocalDbTestFixture _fixture;

    public TodayTotalsQueryTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetTodayTotalsAsync_SixtyGrantedPlusOthers_CountsEveryAttempt()
    {
        await using var context = _fixture.CreateDbContext();
        var org = new Organization("TOT", "جهة الإجماليات");
        var user = new User("tot_user", "مشغل", "hash", UserRole.Supervisor);
        var desk = new Workstation("TOT-DESK", "شباك الإجماليات");
        var otherDesk = new Workstation("TOT-OTHER", "شباك آخر");
        var child = new AgeCategory("طفل اختبار الإجماليات", null, 91);
        var adult = new AgeCategory("بالغ اختبار الإجماليات", null, 92);
        context.AddRange(org, user, desk, otherDesk, child, adult);
        await context.SaveChangesAsync();

        var holder = new Beneficiary(Guid.NewGuid(), org.Id, CardNumber.Create("TOT-1"), "حامل البطاقة", PhoneNumber.Create("0911111111"));
        context.Beneficiaries.Add(holder);
        await context.SaveChangesAsync();

        // 60 granted with one child each: more than the grid shows and more than the old cap of 50.
        for (var i = 0; i < 60; i++)
        {
            context.EntryAttempts.Add(Allowed(holder, org, desk, user, Noon.AddMinutes(-i), [new EntryAttemptCompanion(child.Id, 1)]));
        }

        // One overridden with two adults, one voided granted, two denied.
        context.EntryAttempts.Add(EntryAttempt.CreateOverridden(
            Guid.NewGuid(), holder.Id, org.Id, Noon.AddMinutes(5), Today, 720, Noon.AddHours(12),
            desk.Id, user.Id, user.Id, "حالة خاصة", EntryChannel.Desk, [new EntryAttemptCompanion(adult.Id, 2)]));
        var voided = Allowed(holder, org, desk, user, Noon.AddMinutes(6), [new EntryAttemptCompanion(child.Id, 4)]);
        voided.Void(user.Id, "إدخال بالخطأ", Noon.AddMinutes(7));
        context.EntryAttempts.Add(voided);
        for (var i = 0; i < 2; i++)
        {
            context.EntryAttempts.Add(EntryAttempt.CreateDenied(
                Guid.NewGuid(), holder.Id, org.Id, Noon.AddMinutes(8 + i), Today, DenialReason.CooldownActive, 720, null,
                desk.Id, user.Id, EntryChannel.Desk));
        }

        // Noise that must not be counted: another desk today, and this desk yesterday.
        context.EntryAttempts.Add(Allowed(holder, org, otherDesk, user, Noon, []));
        context.EntryAttempts.Add(EntryAttempt.CreateAllowed(
            Guid.NewGuid(), holder.Id, org.Id, Noon.AddDays(-1), Today.AddDays(-1), 720, Noon, desk.Id, user.Id, EntryChannel.Desk));
        await context.SaveChangesAsync();

        await using var readContext = _fixture.CreateDbContext();
        var totals = await new EntryAttemptRepository(readContext).GetTodayTotalsAsync(desk.Id, Today);

        totals.Granted.ShouldBe(60);
        totals.Overridden.ShouldBe(1);
        totals.Denied.ShouldBe(2);
        totals.Persons.ShouldBe(60 * 2 + 3);
        totals.CompanionsByCategory[child.Id].ShouldBe(60);
        totals.CompanionsByCategory[adult.Id].ShouldBe(2);
    }

    [Fact]
    public async Task GetTodayTotalsAsync_NoAttempts_ReturnsZeros()
    {
        await using var context = _fixture.CreateDbContext();

        var totals = await new EntryAttemptRepository(context).GetTodayTotalsAsync(int.MaxValue, Today);

        totals.Granted.ShouldBe(0);
        totals.Overridden.ShouldBe(0);
        totals.Denied.ShouldBe(0);
        totals.Persons.ShouldBe(0);
        totals.CompanionsByCategory.ShouldBeEmpty();
    }

    private static EntryAttempt Allowed(
        Beneficiary holder, Organization org, Workstation desk, User user, DateTime atUtc, IEnumerable<EntryAttemptCompanion> companions) =>
        EntryAttempt.CreateAllowed(
            Guid.NewGuid(), holder.Id, org.Id, atUtc, Today, 720, atUtc.AddHours(12), desk.Id, user.Id, EntryChannel.Desk, companions);
}
