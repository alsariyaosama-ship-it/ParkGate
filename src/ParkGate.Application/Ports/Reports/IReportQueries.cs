using ParkGate.Application.Reports.DTOs;

namespace ParkGate.Application.Ports.Reports;

/// <summary>
/// Port for executing report analytical queries with AsNoTracking and projections on BusinessDate.
/// </summary>
public interface IReportQueries
{
    Task<DailySummaryReportDto> GetDailySummaryAsync(
        DateOnly date,
        int? organizationId = null,
        int? workstationId = null,
        CancellationToken cancellationToken = default);

    Task<DateRangeSummaryReportDto> GetDateRangeSummaryAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        CancellationToken cancellationToken = default);

    Task<OrganizationStatisticsReportDto> GetOrganizationStatisticsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        CancellationToken cancellationToken = default);

    Task<OperatorWorkstationReportDto> GetOperatorWorkstationStatisticsAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    Task<DenialsAndOverridesReportDto> GetDenialsAndOverridesLogAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        DenialOverrideFilter filter = DenialOverrideFilter.All,
        CancellationToken cancellationToken = default);

    Task<HourlyDistributionReportDto> GetHourlyDistributionAsync(
        DateOnly fromDate,
        DateOnly toDate,
        int? organizationId = null,
        string? timeZoneId = null,
        CancellationToken cancellationToken = default);
}
