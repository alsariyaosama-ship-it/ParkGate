using Microsoft.EntityFrameworkCore;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;
using ParkGate.Infrastructure.Data;
using ParkGate.Infrastructure.Reports;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class GiftReportQueriesTests : IClassFixture<LocalDbTestFixture>
{
    private readonly LocalDbTestFixture _fixture;
    private readonly GiftReportQueries _sut;

    public GiftReportQueriesTests(LocalDbTestFixture fixture)
    {
        _fixture = fixture;
        var factory = new TestDbContextFactory(fixture);
        _sut = new GiftReportQueries(factory);
    }

    [Fact]
    public async Task GetReceiptsAsync_WithVariousFilters_ShouldReturnAccurateRowsAndTotals()
    {
        await using var context = _fixture.CreateDbContext();

        // 1. Seed Organizations
        var orgA = new Organization($"GA_{Guid.NewGuid():N}"[..10], "منظمة الهدايا أ");
        orgA.ConfigureGift(true, 25.0m, 30);
        var orgB = new Organization($"GB_{Guid.NewGuid():N}"[..10], "منظمة الهدايا ب");
        orgB.ConfigureGift(true, 50.0m, 30);
        context.Organizations.AddRange(orgA, orgB);

        // 2. Seed Users & Workstations
        var deskUser = new User($"gdesk_{Guid.NewGuid():N}"[..12], "موظف الاستقبال", "hash", UserRole.Operator);
        var ridesUser = new User($"grides_{Guid.NewGuid():N}"[..12], "موظف الألعاب", "hash", UserRole.Operator);
        context.Users.AddRange(deskUser, ridesUser);

        var deskWs = new Workstation($"WS_D_{Guid.NewGuid():N}"[..10], "نقطة البوابة", WorkstationType.Desk);
        var ridesWs = new Workstation($"WS_R_{Guid.NewGuid():N}"[..10], "شباك الألعاب", WorkstationType.Desk);
        context.Workstations.AddRange(deskWs, ridesWs);

        await context.SaveChangesAsync();

        // 3. Seed Beneficiaries
        var benA = new Beneficiary(
            Guid.NewGuid(),
            orgA.Id,
            CardNumber.Create($"CARD-A-{Guid.NewGuid():N}"[..12]),
            "مستفيد أ",
            PhoneNumber.Create($"091{Random.Shared.Next(1_000_000):D7}"));
        var benB = new Beneficiary(
            Guid.NewGuid(),
            orgB.Id,
            CardNumber.Create($"CARD-B-{Guid.NewGuid():N}"[..12]),
            "مستفيد ب",
            PhoneNumber.Create($"092{Random.Shared.Next(1_000_000):D7}"));
        context.Beneficiaries.AddRange(benA, benB);
        await context.SaveChangesAsync();

        var date1 = new DateOnly(2026, 11, 1);
        var date2 = new DateOnly(2026, 11, 2);
        var time1Utc = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);
        var time2Utc = new DateTime(2026, 11, 2, 0, 30, 0, DateTimeKind.Utc); // after midnight

        // Voucher 1: Issued (OrgA, Date 1, Value 20)
        var v1 = GiftVoucher.Issue(
            Guid.NewGuid(), 1, benA.Id, orgA.Id, 20.0m, time1Utc, date1, deskUser.Id, deskWs.Id);

        // Voucher 2: Redeemed on Date 1 (OrgA, Date 1, Value 30)
        var v2 = GiftVoucher.Issue(
            Guid.NewGuid(), 1, benA.Id, orgA.Id, 30.0m, time1Utc, date1, deskUser.Id, deskWs.Id);
        v2.Redeem(ridesUser.Id, ridesWs.Id, time1Utc.AddHours(2), date1);

        // Voucher 3: Voided on Date 1 (OrgA, Date 1, Value 15)
        var v3 = GiftVoucher.Issue(
            Guid.NewGuid(), 1, benA.Id, orgA.Id, 15.0m, time1Utc, date1, deskUser.Id, deskWs.Id);
        v3.Void(deskUser.Id, "خطأ في الطباعة", time1Utc.AddMinutes(30));

        // Voucher 4: Issued on Date 2 (OrgB, Date 2, Value 50)
        var v4 = GiftVoucher.Issue(
            Guid.NewGuid(), 1, benB.Id, orgB.Id, 50.0m, time2Utc.AddHours(8), date2, deskUser.Id, deskWs.Id);

        // Voucher 5: Issued on Date 1, but redeemed on Date 2 after midnight (OrgA, Value 40)
        var v5 = GiftVoucher.Issue(
            Guid.NewGuid(), 1, benA.Id, orgA.Id, 40.0m, time1Utc, date1, deskUser.Id, deskWs.Id);
        v5.Redeem(ridesUser.Id, ridesWs.Id, time2Utc, date2);

        context.GiftVouchers.AddRange(v1, v2, v3, v4, v5);
        await context.SaveChangesAsync();

        // TEST 1: Filter All for Date 1
        var reportAllDate1 = await _sut.GetReceiptsAsync(date1, date1, filter: GiftReceiptFilter.All);
        reportAllDate1.TotalCount.ShouldBe(4); // v1, v2, v3, v5
        reportAllDate1.TotalValue.ShouldBe(20.0m + 30.0m + 15.0m + 40.0m);
        reportAllDate1.Rows.Count.ShouldBe(4);
        reportAllDate1.Rows[0].ReceiptNo.ShouldBeGreaterThan(reportAllDate1.Rows[1].ReceiptNo); // Ordered by Id descending

        // Check Status totals
        var issuedTotal = reportAllDate1.StatusTotals.First(s => s.Status == GiftVoucherStatus.Issued);
        issuedTotal.Count.ShouldBe(1);
        issuedTotal.TotalValue.ShouldBe(20.0m);

        var redeemedTotal = reportAllDate1.StatusTotals.First(s => s.Status == GiftVoucherStatus.Redeemed);
        redeemedTotal.Count.ShouldBe(2);
        redeemedTotal.TotalValue.ShouldBe(70.0m);

        var voidedTotal = reportAllDate1.StatusTotals.First(s => s.Status == GiftVoucherStatus.Voided);
        voidedTotal.Count.ShouldBe(1);
        voidedTotal.TotalValue.ShouldBe(15.0m);

        // TEST 2: Filter Redeemed for Date 2 (Verifies voucher redeemed after midnight is reported under RedeemedBusinessDate)
        var reportRedeemedDate2 = await _sut.GetReceiptsAsync(date2, date2, filter: GiftReceiptFilter.Redeemed);
        reportRedeemedDate2.TotalCount.ShouldBe(1);
        reportRedeemedDate2.Rows.Count.ShouldBe(1);
        var midnightRow = reportRedeemedDate2.Rows[0];
        midnightRow.ReceiptNo.ShouldBe(v5.Id);
        midnightRow.Value.ShouldBe(40.0m);
        midnightRow.Status.ShouldBe(GiftVoucherStatus.Redeemed);
        midnightRow.RedeemedByUserName.ShouldBe(ridesUser.FullName);
        midnightRow.RedeemedWorkstationName.ShouldBe(ridesWs.DisplayName);

        // TEST 3: Filter Redeemed for Date 1
        var reportRedeemedDate1 = await _sut.GetReceiptsAsync(date1, date1, filter: GiftReceiptFilter.Redeemed);
        reportRedeemedDate1.TotalCount.ShouldBe(1);
        reportRedeemedDate1.Rows[0].ReceiptNo.ShouldBe(v2.Id);
        reportRedeemedDate1.TotalValue.ShouldBe(30.0m);

        // TEST 4: Filter Issued for Date 2
        var reportIssuedDate2 = await _sut.GetReceiptsAsync(date2, date2, filter: GiftReceiptFilter.Issued);
        reportIssuedDate2.TotalCount.ShouldBe(1);
        reportIssuedDate2.Rows[0].ReceiptNo.ShouldBe(v4.Id);
        reportIssuedDate2.Rows[0].OrganizationName.ShouldBe(orgB.NameAr);
        reportIssuedDate2.TotalValue.ShouldBe(50.0m);

        // TEST 5: Filter Voided for Date 1
        var reportVoidedDate1 = await _sut.GetReceiptsAsync(date1, date1, filter: GiftReceiptFilter.Voided);
        reportVoidedDate1.TotalCount.ShouldBe(1);
        reportVoidedDate1.Rows[0].ReceiptNo.ShouldBe(v3.Id);
        reportVoidedDate1.Rows[0].VoidReason.ShouldBe("خطأ في الطباعة");
        reportVoidedDate1.Rows[0].VoidedByUserName.ShouldBe(deskUser.FullName);

        // TEST 6: Filter by Organization
        var reportOrgB = await _sut.GetReceiptsAsync(date1, date2, organizationId: orgB.Id);
        reportOrgB.TotalCount.ShouldBe(1);
        reportOrgB.Rows[0].ReceiptNo.ShouldBe(v4.Id);
        reportOrgB.OrganizationTotals.Count.ShouldBe(1);
        reportOrgB.OrganizationTotals[0].OrganizationId.ShouldBe(orgB.Id);
        reportOrgB.OrganizationTotals[0].TotalValue.ShouldBe(50.0m);

        // TEST 7: Filter by Workstation on Redeemed filter
        var reportWs = await _sut.GetReceiptsAsync(date1, date2, workstationId: ridesWs.Id, filter: GiftReceiptFilter.Redeemed);
        reportWs.TotalCount.ShouldBe(2); // v2 and v5
        reportWs.TotalValue.ShouldBe(70.0m);
    }

    private sealed class TestDbContextFactory(LocalDbTestFixture fixture) : IDbContextFactory<ParkGateDbContext>
    {
        public ParkGateDbContext CreateDbContext() => fixture.CreateDbContext();
        public Task<ParkGateDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(fixture.CreateDbContext());
    }
}
