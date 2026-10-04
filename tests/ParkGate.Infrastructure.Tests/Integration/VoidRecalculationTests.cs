using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class VoidRecalculationTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;

    public VoidRecalculationTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VoidingAttempts_ShouldRecalculateLastGrantedEntryUtcPerBusinessRules()
    {
        await using var context = _fixture.CreateDbContext();

        // Setup base data
        var org = new Organization($"VOID_{Guid.NewGuid():N}"[..10], "جهة الإلغاء");
        var user = new User($"sup_{Guid.NewGuid():N}"[..10], "المشرف", "hash", UserRole.Supervisor);
        var ws = new Workstation($"WS_{Guid.NewGuid():N}"[..10], "شباك الإلغاء");
        context.Organizations.Add(org);
        context.Users.Add(user);
        context.Workstations.Add(ws);
        await context.SaveChangesAsync();

        var beneficiaryId = Guid.NewGuid();
        var holder = new Beneficiary(
            beneficiaryId,
            org.Id,
            CardNumber.Create("CARD-VOID-01"),
            "سالم مسعود",
            PhoneNumber.Create("0919999999"));
        context.Beneficiaries.Add(holder);
        await context.SaveChangesAsync();

        holder.LastGrantedEntryUtc.ShouldBeNull();

        // 1. First entry at 10:00 UTC (Allowed)
        var t1 = new DateTime(2026, 10, 5, 10, 0, 0, DateTimeKind.Utc);
        var attempt1 = EntryAttempt.CreateAllowed(
            Guid.NewGuid(),
            holder.Id,
            org.Id,
            t1,
            new DateOnly(2026, 10, 5),
            720,
            t1.AddMinutes(720),
            ws.Id,
            user.Id,
            EntryChannel.Desk);
        context.EntryAttempts.Add(attempt1);
        holder.RecordGrantedEntry(t1);
        await context.SaveChangesAsync();

        // 2. Second entry at 14:00 UTC (Overridden)
        var t2 = new DateTime(2026, 10, 5, 14, 0, 0, DateTimeKind.Utc);
        var attempt2 = EntryAttempt.CreateOverridden(
            Guid.NewGuid(),
            holder.Id,
            org.Id,
            t2,
            new DateOnly(2026, 10, 5),
            720,
            t2.AddMinutes(720),
            ws.Id,
            user.Id,
            user.Id,
            "استثناء لغرض الاختبار",
            EntryChannel.Desk);
        context.EntryAttempts.Add(attempt2);
        holder.RecordGrantedEntry(t2);
        await context.SaveChangesAsync();

        // Verify state: LastGrantedEntryUtc is t2
        await using (var uowCheck = _fixture.CreateUnitOfWork())
        {
            var h = await uowCheck.Beneficiaries.GetByIdAsync(beneficiaryId);
            h!.LastGrantedEntryUtc.ShouldBe(t2);
        }

        // 3. Void Attempt 2 using UnitOfWork and recalculation logic (BR-13)
        await using (var uow = _fixture.CreateUnitOfWork())
        {
            await uow.BeginTransactionAsync();

            var h = await uow.Beneficiaries.GetForUpdateAsync(beneficiaryId);
            var a2 = await uow.EntryAttempts.GetByIdAsync(attempt2.Id);

            a2!.Void(user.Id, "إلغاء دخول بالخطأ للمحاولة الثانية", new DateTime(2026, 10, 5, 14, 15, 0, DateTimeKind.Utc));
            await uow.SaveChangesAsync();

            var latestRemaining = await uow.Beneficiaries.GetLatestGrantedEntryUtcAsync(beneficiaryId);
            h!.RecalculateLastGrantedEntry(latestRemaining);

            await uow.SaveChangesAsync();
            await uow.CommitAsync();
        }

        // Verify state: LastGrantedEntryUtc rolled back to t1
        await using (var uowCheck = _fixture.CreateUnitOfWork())
        {
            var h = await uowCheck.Beneficiaries.GetByIdAsync(beneficiaryId);
            h!.LastGrantedEntryUtc.ShouldBe(t1);
        }

        // 4. Void Attempt 1 using UnitOfWork and recalculation logic (BR-13)
        await using (var uow = _fixture.CreateUnitOfWork())
        {
            await uow.BeginTransactionAsync();

            var h = await uow.Beneficiaries.GetForUpdateAsync(beneficiaryId);
            var a1 = await uow.EntryAttempts.GetByIdAsync(attempt1.Id);

            a1!.Void(user.Id, "إلغاء دخول بالخطأ للمحاولة الأولى", new DateTime(2026, 10, 5, 15, 0, 0, DateTimeKind.Utc));
            await uow.SaveChangesAsync();

            var latestRemaining = await uow.Beneficiaries.GetLatestGrantedEntryUtcAsync(beneficiaryId);
            h!.RecalculateLastGrantedEntry(latestRemaining);

            await uow.SaveChangesAsync();
            await uow.CommitAsync();
        }

        // Verify state: LastGrantedEntryUtc rolled back to null
        await using (var uowCheck = _fixture.CreateUnitOfWork())
        {
            var h = await uowCheck.Beneficiaries.GetByIdAsync(beneficiaryId);
            h!.LastGrantedEntryUtc.ShouldBeNull();
        }
    }
}
