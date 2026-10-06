using NSubstitute;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Reports;
using ParkGate.Application.Reports.DTOs;
using ParkGate.Application.Reports.Services;
using ParkGate.Application.Security;
using Shouldly;

namespace ParkGate.Application.Tests.Reports;

public sealed class ReportServiceTests
{
    private readonly IReportQueries _reportQueries = Substitute.For<IReportQueries>();
    private readonly IAgeCategoryService _ageCategoryService = Substitute.For<IAgeCategoryService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly ReportService _sut;

    public ReportServiceTests()
    {
        _currentUser.Has(Permission.ViewReports).Returns(true);
        _sut = new ReportService(_reportQueries, _ageCategoryService, _currentUser);
    }

    [Fact]
    public async Task GetDailySummaryAsync_WhenUserLacksPermission_ReturnsForbidden()
    {
        _currentUser.Has(Permission.ViewReports).Returns(false);

        var result = await _sut.GetDailySummaryAsync(new DateOnly(2026, 10, 5));

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(Errors.General.Forbidden);
    }

    [Fact]
    public async Task GetDailySummaryAsync_WhenAuthorized_CallsQueriesAndReturnsSuccess()
    {
        var expected = new DailySummaryReportDto(
            new DateOnly(2026, 10, 5), null, null, null, null,
            10, 8, 1, 9, 1, 0, 9, 15, 24, [], []);

        _reportQueries.GetDailySummaryAsync(new DateOnly(2026, 10, 5), null, null)
            .Returns(expected);

        var result = await _sut.GetDailySummaryAsync(new DateOnly(2026, 10, 5));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    [Fact]
    public async Task GetDateRangeSummaryAsync_WhenFromDateGreaterThanToDate_ReturnsInvalidDateRange()
    {
        var result = await _sut.GetDateRangeSummaryAsync(
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 5));

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(Errors.Reports.InvalidDateRange);
    }

    [Fact]
    public async Task GetDateRangeSummaryAsync_WhenValid_ReturnsSuccess()
    {
        var expected = new DateRangeSummaryReportDto(
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 10, 5),
            null, null, [],
            new DateRangeTotalsDto(0, 0, 0, 0, 0, 0, 0, 0, 0, []));

        _reportQueries.GetDateRangeSummaryAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5), null)
            .Returns(expected);

        var result = await _sut.GetDateRangeSummaryAsync(new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 5));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expected);
    }

    [Fact]
    public async Task GetOrganizationStatisticsAsync_WhenFromDateGreaterThanToDate_ReturnsInvalidDateRange()
    {
        var result = await _sut.GetOrganizationStatisticsAsync(
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 5));

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(Errors.Reports.InvalidDateRange);
    }

    [Fact]
    public async Task GetOperatorWorkstationStatisticsAsync_WhenFromDateGreaterThanToDate_ReturnsInvalidDateRange()
    {
        var result = await _sut.GetOperatorWorkstationStatisticsAsync(
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 5));

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(Errors.Reports.InvalidDateRange);
    }

    [Fact]
    public async Task GetDenialsAndOverridesLogAsync_WhenFromDateGreaterThanToDate_ReturnsInvalidDateRange()
    {
        var result = await _sut.GetDenialsAndOverridesLogAsync(
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 5));

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(Errors.Reports.InvalidDateRange);
    }

    [Fact]
    public async Task GetHourlyDistributionAsync_WhenFromDateGreaterThanToDate_ReturnsInvalidDateRange()
    {
        var result = await _sut.GetHourlyDistributionAsync(
            new DateOnly(2026, 10, 10),
            new DateOnly(2026, 10, 5));

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(Errors.Reports.InvalidDateRange);
    }

    [Fact]
    public async Task GetActiveAgeCategoriesAsync_WhenAuthorized_ReturnsCategories()
    {
        var list = new List<AgeCategoryDto>
        {
            new(1, "أطفال", null, 1, true)
        };

        _ageCategoryService.GetActiveAsync().Returns(list);

        var result = await _sut.GetActiveAgeCategoriesAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
    }
}
