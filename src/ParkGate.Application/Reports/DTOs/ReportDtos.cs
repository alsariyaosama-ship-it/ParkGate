using ParkGate.Domain.Enums;

namespace ParkGate.Application.Reports.DTOs;

public enum DenialOverrideFilter
{
    All = 0,
    DeniedOnly = 1,
    OverriddenOnly = 2
}

public sealed record AgeCategorySummaryDto(
    int CategoryId,
    string CategoryName,
    int Count);

// 1. Daily Summary (FR-RPT-01)
public sealed record DailySummaryReportDto(
    DateOnly Date,
    int? OrganizationId,
    string? OrganizationName,
    int? WorkstationId,
    string? WorkstationName,
    int TotalAttempts,
    int AllowedEntries,
    int OverriddenEntries,
    int TotalVisits,
    int DeniedEntries,
    int VoidedEntries,
    int TotalHolders,
    int TotalCompanions,
    int TotalPersons,
    IReadOnlyList<AgeCategorySummaryDto> AgeCategoryBreakdown,
    IReadOnlyList<DailySummaryOrganizationRowDto> OrganizationBreakdown);

public sealed record DailySummaryOrganizationRowDto(
    int OrganizationId,
    string OrganizationCode,
    string OrganizationName,
    int AllowedVisits,
    int OverriddenVisits,
    int TotalVisits,
    int TotalHolders,
    int TotalCompanions,
    int TotalPersons);

// 2. Date Range Daily Summary (FR-RPT-02)
public sealed record DateRangeSummaryReportDto(
    DateOnly FromDate,
    DateOnly ToDate,
    int? OrganizationId,
    string? OrganizationName,
    IReadOnlyList<DateRangeSummaryRowDto> Rows,
    DateRangeTotalsDto Totals);

public sealed record DateRangeSummaryRowDto(
    DateOnly BusinessDate,
    string DayOfWeekArabic,
    int TotalAttempts,
    int AllowedVisits,
    int OverriddenVisits,
    int TotalVisits,
    int DeniedAttempts,
    int VoidedAttempts,
    int TotalHolders,
    int TotalCompanions,
    int TotalPersons,
    IReadOnlyList<AgeCategorySummaryDto> CompanionsByAgeCategory);

public sealed record DateRangeTotalsDto(
    int TotalAttempts,
    int AllowedVisits,
    int OverriddenVisits,
    int TotalVisits,
    int DeniedAttempts,
    int VoidedAttempts,
    int TotalHolders,
    int TotalCompanions,
    int TotalPersons,
    IReadOnlyList<AgeCategorySummaryDto> CompanionsByAgeCategory);

// 3. Organization Statistics (FR-RPT-03, Q8 statistical only)
public sealed record OrganizationStatisticsReportDto(
    DateOnly FromDate,
    DateOnly ToDate,
    int? OrganizationId,
    string? OrganizationName,
    int TotalParkVisits,
    int TotalParkPersons,
    IReadOnlyList<OrganizationStatisticsRowDto> Rows,
    OrganizationStatisticsTotalsDto Totals);

public sealed record OrganizationStatisticsRowDto(
    int OrganizationId,
    string OrganizationCode,
    string OrganizationName,
    int TotalVisits,
    int UniqueBeneficiariesCount,
    int TotalHolders,
    int TotalCompanions,
    int TotalPersons,
    double PercentageShare,
    IReadOnlyList<AgeCategorySummaryDto> CompanionsByAgeCategory);

public sealed record OrganizationStatisticsTotalsDto(
    int TotalVisits,
    int UniqueBeneficiariesCount,
    int TotalHolders,
    int TotalCompanions,
    int TotalPersons,
    IReadOnlyList<AgeCategorySummaryDto> CompanionsByAgeCategory);

// 4. Operator and Workstation Statistics (FR-RPT-04)
public sealed record OperatorWorkstationReportDto(
    DateOnly FromDate,
    DateOnly ToDate,
    IReadOnlyList<OperatorStatisticsRowDto> OperatorRows,
    IReadOnlyList<WorkstationStatisticsRowDto> WorkstationRows);

public sealed record OperatorStatisticsRowDto(
    int UserId,
    string UserName,
    string FullName,
    int TotalAttempts,
    int AllowedEntries,
    int OverriddenEntries,
    int TotalVisits,
    int DeniedEntries,
    int VoidedEntries,
    int TotalPersons);

public sealed record WorkstationStatisticsRowDto(
    int WorkstationId,
    string MachineName,
    string DisplayName,
    int TotalAttempts,
    int AllowedEntries,
    int OverriddenEntries,
    int TotalVisits,
    int DeniedEntries,
    int VoidedEntries,
    int TotalPersons);

// 5. Denials and Overrides Log (FR-RPT-05)
public sealed record DenialsAndOverridesReportDto(
    DateOnly FromDate,
    DateOnly ToDate,
    int? OrganizationId,
    string? OrganizationName,
    DenialOverrideFilter Filter,
    IReadOnlyList<DenialOverrideLogItemDto> Items);

public sealed record DenialOverrideLogItemDto(
    Guid AttemptId,
    DateTime AttemptedAtUtc,
    DateOnly BusinessDate,
    string CardNumber,
    string BeneficiaryName,
    int OrganizationId,
    string OrganizationName,
    EntryResult Result,
    DenialReason? DenialReason,
    string? OverrideReason,
    string? SupervisorName,
    string OperatorName,
    string WorkstationName,
    int TotalPersons);

// 6. Hourly Distribution (FR-RPT-06)
public sealed record HourlyDistributionReportDto(
    DateOnly FromDate,
    DateOnly ToDate,
    int? OrganizationId,
    string? OrganizationName,
    int TotalVisits,
    int TotalPersons,
    IReadOnlyList<HourlyDistributionRowDto> Rows);

public sealed record HourlyDistributionRowDto(
    int Hour,
    string HourLabel,
    int VisitCount,
    int TotalPersons,
    double AverageGroupSize,
    double PercentageShare);
