using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Reports;
using ParkGate.Application.Reports.DTOs;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Data;
using ParkGate.Infrastructure.Reports;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class ReportQueriesTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;
    private readonly ReportQueries _sut;

    public ReportQueriesTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
        var factory = new TestDbContextFactory(fixture);
        _sut = new ReportQueries(factory);
    }

    [Fact]
    public async Task ReportQueries_AllSixReports_ShouldReturnAccurateAggregations()
    {
        await using var context = _fixture.CreateDbContext();

        // 1. Seed base entities
        var org = new Organization("RPT-ORG", "جهة التقارير", 120, false, true);
        context.Organizations.Add(org);

        var operatorUser = new User("rpt_operator", "مشغل التقارير", "PBKDF2$SHA256$600000$c2FsdA==$aGFzaA==", UserRole.Operator, false, true);
        var supervisorUser = new User("rpt_supervisor", "مشرف التقارير", "PBKDF2$SHA256$600000$c2FsdA==$aGFzaA==", UserRole.Supervisor, false, true);
        context.Users.AddRange(operatorUser, supervisorUser);

        var workstation = new Workstation("RPT-PC-01", "شباك التقارير", WorkstationType.Desk, true);
        context.Workstations.Add(workstation);

        var category = await context.AgeCategories.FirstAsync(c => c.IsActive);

        await context.SaveChangesAsync();

        var b1 = new Beneficiary(
            Guid.NewGuid(),
            org.Id,
            CardNumber.Create("RPT-CARD-001"),
            "أحمد التقريري",
            PhoneNumber.Create("0910001111"));
        context.Beneficiaries.Add(b1);
        await context.SaveChangesAsync();

        var testDate = new DateOnly(2026, 10, 5);
        var baseUtc = new DateTime(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc); // 14:00 in Libya (UTC+2)

        // Attempt 1: Allowed with 2 companions (total 3 persons)
        var attempt1 = EntryAttempt.CreateAllowed(
            Guid.NewGuid(),
            b1.Id,
            org.Id,
            baseUtc,
            testDate,
            120,
            baseUtc.AddHours(2),
            workstation.Id,
            operatorUser.Id,
            EntryChannel.Desk,
            [new EntryAttemptCompanion(category.Id, 2)]);

        // Attempt 2: Overridden with 1 companion (total 2 persons)
        var attempt2 = EntryAttempt.CreateOverridden(
            Guid.NewGuid(),
            b1.Id,
            org.Id,
            baseUtc.AddMinutes(15),
            testDate,
            120,
            baseUtc.AddHours(2),
            workstation.Id,
            operatorUser.Id,
            supervisorUser.Id,
            "استثناء لغرض الاختبار",
            EntryChannel.Desk,
            [new EntryAttemptCompanion(category.Id, 1)]);

        // Attempt 3: Denied
        var attempt3 = EntryAttempt.CreateDenied(
            Guid.NewGuid(),
            b1.Id,
            org.Id,
            baseUtc.AddMinutes(30),
            testDate,
            DenialReason.CooldownActive,
            120,
            baseUtc.AddHours(2),
            workstation.Id,
            operatorUser.Id,
            EntryChannel.Desk);

        // Attempt 4: Voided entry
        var attempt4 = EntryAttempt.CreateAllowed(
            Guid.NewGuid(),
            b1.Id,
            org.Id,
            baseUtc.AddMinutes(45),
            testDate,
            120,
            baseUtc.AddHours(2),
            workstation.Id,
            operatorUser.Id,
            EntryChannel.Desk);
        attempt4.Void(supervisorUser.Id, "إلغاء لخطأ في الإدخال", baseUtc.AddMinutes(50));

        context.EntryAttempts.AddRange(attempt1, attempt2, attempt3, attempt4);
        await context.SaveChangesAsync();

        // TEST 1: Daily Summary
        var daily = await _sut.GetDailySummaryAsync(testDate, org.Id);
        daily.TotalAttempts.ShouldBe(4);
        daily.AllowedEntries.ShouldBe(1);
        daily.OverriddenEntries.ShouldBe(1);
        daily.TotalVisits.ShouldBe(2);
        daily.DeniedEntries.ShouldBe(1);
        daily.VoidedEntries.ShouldBe(1);
        daily.TotalPersons.ShouldBe(5); // 3 (attempt1) + 2 (attempt2)
        daily.TotalHolders.ShouldBe(2);
        daily.TotalCompanions.ShouldBe(3);
        daily.AgeCategoryBreakdown.First(c => c.CategoryId == category.Id).Count.ShouldBe(3);
        daily.OrganizationBreakdown.Count.ShouldBe(1);
        daily.OrganizationBreakdown[0].TotalPersons.ShouldBe(5);

        // TEST 2: Date Range Summary
        var dateRange = await _sut.GetDateRangeSummaryAsync(testDate.AddDays(-1), testDate.AddDays(1), org.Id);
        dateRange.Rows.Count.ShouldBe(1);
        dateRange.Rows[0].BusinessDate.ShouldBe(testDate);
        dateRange.Rows[0].TotalAttempts.ShouldBe(4);
        dateRange.Rows[0].AllowedVisits.ShouldBe(1);
        dateRange.Rows[0].OverriddenVisits.ShouldBe(1);
        dateRange.Rows[0].TotalVisits.ShouldBe(2);
        dateRange.Rows[0].TotalPersons.ShouldBe(5);
        dateRange.Totals.AllowedVisits.ShouldBe(1);
        dateRange.Totals.OverriddenVisits.ShouldBe(1);
        dateRange.Totals.TotalVisits.ShouldBe(2);
        dateRange.Totals.TotalPersons.ShouldBe(5);

        // TEST 3: Organization Statistics
        var orgStats = await _sut.GetOrganizationStatisticsAsync(testDate, testDate, org.Id);
        orgStats.Rows.Count.ShouldBe(1);
        orgStats.Rows[0].TotalVisits.ShouldBe(2);
        orgStats.Rows[0].UniqueBeneficiariesCount.ShouldBe(1);
        orgStats.Rows[0].TotalPersons.ShouldBe(5);
        orgStats.Rows[0].TotalCompanions.ShouldBe(3);

        // TEST 4: Operator & Workstation Statistics
        var opWsStats = await _sut.GetOperatorWorkstationStatisticsAsync(testDate, testDate);
        var opRow = opWsStats.OperatorRows.FirstOrDefault(r => r.UserId == operatorUser.Id);
        opRow.ShouldNotBeNull();
        opRow.TotalAttempts.ShouldBe(4);
        opRow.TotalVisits.ShouldBe(2);
        opRow.TotalPersons.ShouldBe(5);

        var wsRow = opWsStats.WorkstationRows.FirstOrDefault(r => r.WorkstationId == workstation.Id);
        wsRow.ShouldNotBeNull();
        wsRow.TotalAttempts.ShouldBe(4);
        wsRow.TotalVisits.ShouldBe(2);

        // TEST 5: Denials & Overrides Log
        var logAll = await _sut.GetDenialsAndOverridesLogAsync(testDate, testDate, org.Id, DenialOverrideFilter.All);
        logAll.Items.Count.ShouldBe(2); // 1 denied + 1 overridden

        var logDeniedOnly = await _sut.GetDenialsAndOverridesLogAsync(testDate, testDate, org.Id, DenialOverrideFilter.DeniedOnly);
        logDeniedOnly.Items.Count.ShouldBe(1);
        logDeniedOnly.Items[0].Result.ShouldBe(EntryResult.Denied);
        logDeniedOnly.Items[0].DenialReason.ShouldBe(DenialReason.CooldownActive);

        var logOverriddenOnly = await _sut.GetDenialsAndOverridesLogAsync(testDate, testDate, org.Id, DenialOverrideFilter.OverriddenOnly);
        logOverriddenOnly.Items.Count.ShouldBe(1);
        logOverriddenOnly.Items[0].Result.ShouldBe(EntryResult.Overridden);
        logOverriddenOnly.Items[0].SupervisorName.ShouldBe(supervisorUser.FullName);

        // TEST 6: Hourly Distribution
        var hourly = await _sut.GetHourlyDistributionAsync(testDate, testDate, org.Id, "Libya Standard Time");
        hourly.TotalVisits.ShouldBe(2);
        hourly.TotalPersons.ShouldBe(5);
        // UTC 12:00 = 14:00 in Libya Standard Time (hour 14)
        var hour14 = hourly.Rows.First(r => r.Hour == 14);
        hour14.VisitCount.ShouldBe(2);
        hour14.TotalPersons.ShouldBe(5);
    }

    private sealed class TestDbContextFactory(LocalDbTestFixture fixture) : IDbContextFactory<ParkGateDbContext>
    {
        public ParkGateDbContext CreateDbContext() => fixture.CreateDbContext();
        public Task<ParkGateDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(fixture.CreateDbContext());
    }
}
