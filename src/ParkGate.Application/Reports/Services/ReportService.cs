using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Reports;
using ParkGate.Application.Reports.DTOs;
using ParkGate.Application.Security;

namespace ParkGate.Application.Reports.Services;

public sealed class ReportService : IReportService
{
    private readonly IReportQueries _reportQueries;
    private readonly IAgeCategoryService _ageCategoryService;
    private readonly ICurrentUser _currentUser;

    public ReportService(
        IReportQueries reportQueries,
        IAgeCategoryService ageCategoryService,
        ICurrentUser currentUser)
    {
        _reportQueries = reportQueries ?? throw new ArgumentNullException(nameof(reportQueries));
        _ageCategoryService = ageCategoryService ?? throw new ArgumentNullException(nameof(ageCategoryService));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<DailySummaryReportDto>> GetDailySummaryAsync(
        DateOnly date,
        int? organizationId = null,
        int? workstationId = null,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ViewReports))
        {
            return Result.Failure<DailySummaryReportDto>(Errors.General.Forbidden);
        }

        var report = await _reportQueries.GetDailySummaryAsync(date, organizationId, workstationId, cancellationToken);
        return Result.Success(report);
    }

    public async Task<Result<DateRangeSummaryReportDto>> GetDateRangeSummaryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ViewReports))
        {
            return Result.Failure<DateRangeSummaryReportDto>(Errors.General.Forbidden);
        }

        if (fromDate > toDate)
        {
            return Result.Failure<DateRangeSummaryReportDto>(Errors.Reports.InvalidDateRange);
        }

        var report = await _reportQueries.GetDateRangeSummaryAsync(fromDate, toDate, organizationId, cancellationToken);
        return Result.Success(report);
    }

    public async Task<Result<OrganizationStatisticsReportDto>> GetOrganizationStatisticsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ViewReports))
        {
            return Result.Failure<OrganizationStatisticsReportDto>(Errors.General.Forbidden);
        }

        if (fromDate > toDate)
        {
            return Result.Failure<OrganizationStatisticsReportDto>(Errors.Reports.InvalidDateRange);
        }

        var report = await _reportQueries.GetOrganizationStatisticsAsync(fromDate, toDate, organizationId, cancellationToken);
        return Result.Success(report);
    }

    public async Task<Result<OperatorWorkstationReportDto>> GetOperatorWorkstationStatisticsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ViewReports))
        {
            return Result.Failure<OperatorWorkstationReportDto>(Errors.General.Forbidden);
        }

        if (fromDate > toDate)
        {
            return Result.Failure<OperatorWorkstationReportDto>(Errors.Reports.InvalidDateRange);
        }

        var report = await _reportQueries.GetOperatorWorkstationStatisticsAsync(fromDate, toDate, cancellationToken);
        return Result.Success(report);
    }

    public async Task<Result<DenialsAndOverridesReportDto>> GetDenialsAndOverridesLogAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        DenialOverrideFilter filter = DenialOverrideFilter.All,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ViewReports))
        {
            return Result.Failure<DenialsAndOverridesReportDto>(Errors.General.Forbidden);
        }

        if (fromDate > toDate)
        {
            return Result.Failure<DenialsAndOverridesReportDto>(Errors.Reports.InvalidDateRange);
        }

        var report = await _reportQueries.GetDenialsAndOverridesLogAsync(fromDate, toDate, organizationId, filter, cancellationToken);
        return Result.Success(report);
    }

    public async Task<Result<HourlyDistributionReportDto>> GetHourlyDistributionAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        string? timeZoneId = null,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ViewReports))
        {
            return Result.Failure<HourlyDistributionReportDto>(Errors.General.Forbidden);
        }

        if (fromDate > toDate)
        {
            return Result.Failure<HourlyDistributionReportDto>(Errors.Reports.InvalidDateRange);
        }

        var report = await _reportQueries.GetHourlyDistributionAsync(fromDate, toDate, organizationId, timeZoneId, cancellationToken);
        return Result.Success(report);
    }

    public async Task<Result<IReadOnlyList<AgeCategoryDto>>> GetActiveAgeCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ViewReports))
        {
            return Result.Failure<IReadOnlyList<AgeCategoryDto>>(Errors.General.Forbidden);
        }

        var categories = await _ageCategoryService.GetActiveAsync(cancellationToken);
        return Result.Success(categories);
    }
}
