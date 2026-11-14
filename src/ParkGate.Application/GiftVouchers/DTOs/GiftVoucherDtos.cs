using ParkGate.Domain.Enums;

namespace ParkGate.Application.GiftVouchers.DTOs;

public sealed record GiftEligibilityDto(
    bool IsEligible,
    GiftDenialReason? Reason,
    string? ReasonCode,
    DateTime? NextEligibleAtUtc,
    bool CanBeOverridden,
    decimal GiftValue,
    DateTime? LastGiftIssuedUtc);

public sealed record IssueGiftVoucherRequest(Guid BeneficiaryId);

public sealed record IssueGiftOverrideRequest(
    Guid BeneficiaryId,
    string Reason,
    string? SupervisorApprovalToken = null);

public sealed record IssuedGiftVoucherDto(
    int ReceiptNo,
    Guid BeneficiaryId,
    string HolderName,
    string CardNumber,
    string OrganizationName,
    decimal Value,
    DateTime IssuedAtUtc,
    DateOnly ExpiresOn,
    bool IsOverride,
    string QrText);

public sealed record GiftRedemptionDto(
    int ReceiptNo,
    decimal Value,
    string HolderName,
    string CardNumber,
    string OrganizationName,
    DateTime IssuedAtUtc,
    DateTime RedeemedAtUtc);

public sealed record VoidGiftVoucherRequest(int VoucherId, string Reason);
