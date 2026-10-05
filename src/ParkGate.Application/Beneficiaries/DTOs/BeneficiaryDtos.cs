using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.Beneficiaries.DTOs;

public sealed record BeneficiaryDto(
    Guid Id,
    int OrganizationId,
    string OrganizationName,
    string CardNumber,
    string FullName,
    string Phone,
    string? NationalId,
    DateOnly? CardExpiryDate,
    BeneficiaryStatus Status,
    string? StatusReason,
    DateTime? LastGrantedEntryUtc,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record CompanionCountDto(
    int AgeCategoryId,
    string CategoryName,
    int Count);

public sealed record EligibilityPreviewDto(
    bool IsEligible,
    EntryResult Result,
    DenialReason? DenialReason,
    string DenialReasonText,
    bool CanBeOverridden,
    int EffectiveCooldownMinutes,
    DateTime? LastGrantedEntryUtc,
    DateTime? NextAllowedEntryUtc,
    int? RemainingCooldownMinutes);

public sealed record BeneficiaryLookupResultDto(
    BeneficiaryDto Beneficiary,
    EligibilityPreviewDto Eligibility,
    IReadOnlyList<CompanionCountDto> LastCompanions);

public sealed record RegisterBeneficiaryRequest(
    int OrganizationId,
    string CardNumber,
    string FullName,
    string Phone,
    string? NationalId = null,
    DateOnly? CardExpiryDate = null,
    string? Notes = null);

public sealed record UpdateBeneficiaryRequest(
    Guid Id,
    int OrganizationId,
    string CardNumber,
    string FullName,
    string Phone,
    string? NationalId = null,
    DateOnly? CardExpiryDate = null,
    string? Notes = null);

public sealed record ChangeBeneficiaryStatusRequest(
    Guid Id,
    BeneficiaryStatus TargetStatus,
    string Reason);

public sealed record EntryAttemptDto(
    Guid Id,
    DateTime AttemptedAtUtc,
    DateOnly BusinessDate,
    int WorkstationId,
    int UserId,
    Guid BeneficiaryId,
    EntryResult Result,
    DenialReason? DenialReason,
    string? OverrideReason,
    int? OverrideByUserId,
    int TotalPersons,
    bool IsVoided,
    string? VoidReason,
    DateTime? VoidedAtUtc,
    int? VoidedByUserId,
    IReadOnlyList<CompanionCountDto> Companions);

public static class BeneficiaryMappingExtensions
{
    public static BeneficiaryDto ToDto(this Beneficiary b, string organizationName) =>
        new(
            b.Id,
            b.OrganizationId,
            organizationName,
            b.CardNumber.Value,
            b.FullName,
            b.Phone.Value,
            b.NationalId?.Value,
            b.CardExpiryDate,
            b.Status,
            b.StatusReason,
            b.LastGrantedEntryUtc,
            b.Notes,
            b.CreatedAtUtc);
}
