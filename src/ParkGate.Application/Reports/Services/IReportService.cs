using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.Common;
using ParkGate.Application.Reports.DTOs;

namespace ParkGate.Application.Reports.Services;

public interface IReportService
{
    Task<Result<DailySummaryReportDto>> GetDailySummaryAsync(
        DateOnly date,
        int? organizationId = null,
        int? workstationId = null,
        CancellationToken cancellationToken = default);

    Task<Result<DateRangeSummaryReportDto>> GetDateRangeSummaryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        CancellationToken cancellationToken = default);

    Task<Result<OrganizationStatisticsReportDto>> GetOrganizationStatisticsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        CancellationToken cancellationToken = default);

    Task<Result<OperatorWorkstationReportDto>> GetOperatorWorkstationStatisticsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    Task<Result<DenialsAndOverridesReportDto>> GetDenialsAndOverridesLogAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        DenialOverrideFilter filter = DenialOverrideFilter.All,
        CancellationToken cancellationToken = default);

    Task<Result<HourlyDistributionReportDto>> GetHourlyDistributionAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        string? timeZoneId = null,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<AgeCategoryDto>>> GetActiveAgeCategoriesAsync(
        CancellationToken cancellationToken = default);
}
