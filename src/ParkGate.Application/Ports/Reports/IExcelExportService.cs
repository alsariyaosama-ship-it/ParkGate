using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.Reports.DTOs;

namespace ParkGate.Application.Ports.Reports;

/// <summary>
/// Port for exporting reports to formatted Excel (.xlsx) workbooks with RTL sheets and styled tables.
/// </summary>
public interface IExcelExportService
{
    byte[] ExportDailySummary(DailySummaryReportDto report, IReadOnlyList<AgeCategoryDto> categories);
    byte[] ExportDateRangeSummary(DateRangeSummaryReportDto report, IReadOnlyList<AgeCategoryDto> categories);
    byte[] ExportOrganizationStatistics(OrganizationStatisticsReportDto report, IReadOnlyList<AgeCategoryDto> categories);
    byte[] ExportOperatorWorkstationStatistics(OperatorWorkstationReportDto report);
    byte[] ExportDenialsAndOverrides(DenialsAndOverridesReportDto report, string? timeZoneId = null);
    byte[] ExportHourlyDistribution(HourlyDistributionReportDto report);
}
