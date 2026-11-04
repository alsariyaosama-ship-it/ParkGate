using NSubstitute;
using ParkGate.Application.Common;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.GiftVouchers.Services;
using ParkGate.Application.Ports.Reports;
using ParkGate.Application.Security;
using Shouldly;

namespace ParkGate.Application.Tests.GiftVouchers;

public sealed class GiftReportServiceTests
{
    private readonly IGiftReportQueries _reportQueries = Substitute.For<IGiftReportQueries>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly GiftReportService _sut;

    public GiftReportServiceTests()
    {
        _currentUser.Has(Permission.ViewReports).Returns(true);
        _sut = new GiftReportService(_reportQueries, _currentUser);
    }

    [Fact]
    public async Task GetReceiptsAsync_WhenUserLacksViewReportsPermission_ReturnsForbidden()
    {
        _currentUser.Has(Permission.ViewReports).Returns(false);

        var result = await _sut.GetReceiptsAsync(
            new DateOnly(2026, 11, 1),
            new DateOnly(2026, 11, 2));

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(Errors.General.Forbidden);
        await _reportQueries.DidNotReceiveWithAnyArgs()
            .GetReceiptsAsync(default, default, default, default, default, default);
    }

    [Fact]
    public async Task GetReceiptsAsync_WhenFromDateGreaterThanToDate_ReturnsInvalidDateRange()
    {
        var result = await _sut.GetReceiptsAsync(
            new DateOnly(2026, 11, 5),
            new DateOnly(2026, 11, 1));

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(Errors.Reports.InvalidDateRange);
        await _reportQueries.DidNotReceiveWithAnyArgs()
            .GetReceiptsAsync(default, default, default, default, default, default);
    }

    [Fact]
    public async Task GetReceiptsAsync_WhenAuthorizedAndValidDateRange_CallsQueriesAndReturnsSuccess()
    {
        var fromDate = new DateOnly(2026, 11, 1);
        var toDate = new DateOnly(2026, 11, 2);
        var expectedReport = new GiftReceiptReportDto(
            fromDate,
            toDate,
            OrganizationId: 10,
            WorkstationId: 5,
            Filter: GiftReceiptFilter.Redeemed,
            Rows: [],
            StatusTotals: [],
            OrganizationTotals: [],
            TotalCount: 0,
            TotalValue: 0m);

        _reportQueries.GetReceiptsAsync(fromDate, toDate, 10, 5, GiftReceiptFilter.Redeemed, Arg.Any<CancellationToken>())
            .Returns(expectedReport);

        var result = await _sut.GetReceiptsAsync(fromDate, toDate, 10, 5, GiftReceiptFilter.Redeemed);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedReport);
    }
}
