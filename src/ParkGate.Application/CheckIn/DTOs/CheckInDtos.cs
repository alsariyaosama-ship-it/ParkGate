using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.CheckIn.DTOs;

public sealed record CompanionRequestDto(
    int AgeCategoryId,
    int Count);

public sealed record RecordEntryRequest(
    Guid BeneficiaryId,
    IReadOnlyList<CompanionRequestDto>? Companions = null,
    EntryChannel Channel = EntryChannel.Desk);

public sealed record OverrideEntryRequest(
    Guid BeneficiaryId,
    string Reason,
    int? SupervisorUserId = null,
    IReadOnlyList<CompanionRequestDto>? Companions = null,
    EntryChannel Channel = EntryChannel.Desk);

public sealed record VoidEntryRequest(
    Guid AttemptId,
    string Reason);

public sealed record CheckInResultDto(
    Guid AttemptId,
    EntryResult Result,
    DenialReason? DenialReason,
    string? DenialReasonText,
    DateTime AttemptedAtUtc,
    DateOnly BusinessDate,
    int TotalPersons,
    int EffectiveCooldownMinutes,
    DateTime? NextEligibleAtUtc,
    bool CanBeOverridden,
    IReadOnlyList<CompanionCountDto> Companions);

public sealed record TodayDeskAttemptDto(
    Guid Id,
    DateTime AttemptedAtUtc,
    string CardNumber,
    string BeneficiaryFullName,
    string OrganizationName,
    int TotalPersons,
    EntryResult Result,
    DenialReason? DenialReason,
    bool IsVoided);

public sealed record TodayDeskSummaryDto(
    DateOnly BusinessDate,
    int WorkstationId,
    string WorkstationName,
    int TotalGranted,
    int TotalDenied,
    int TotalOverridden,
    int TotalPersons,
    IReadOnlyList<CompanionCountDto> PersonsPerCategory,
    IReadOnlyList<TodayDeskAttemptDto> RecentAttempts);

