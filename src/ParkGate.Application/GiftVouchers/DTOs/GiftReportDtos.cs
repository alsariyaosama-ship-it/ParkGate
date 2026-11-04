using ParkGate.Domain.Enums;

namespace ParkGate.Application.GiftVouchers.DTOs;

public enum GiftReceiptFilter
{
    All = 0,
    Redeemed = 1,
    Issued = 2,
    Voided = 3
}

public sealed record GiftReceiptRowDto(
    int ReceiptNo,
    Guid BeneficiaryId,
    string HolderName,
    string CardNumber,
    int OrganizationId,
    string OrganizationName,
    decimal Value,
    DateTime IssuedAtUtc,
    string IssuedByUserName,
    DateTime? RedeemedAtUtc,
    string? RedeemedByUserName,
    string? RedeemedWorkstationName,
    GiftVoucherStatus Status,
    bool IsOverride,
    string? OverrideReason,
    DateTime? VoidedAtUtc = null,
    string? VoidedByUserName = null,
    string? VoidReason = null);

public sealed record GiftReceiptStatusTotalDto(
    GiftVoucherStatus Status,
    int Count,
    decimal TotalValue);

public sealed record GiftReceiptOrganizationTotalDto(
    int OrganizationId,
    string OrganizationName,
    int Count,
    decimal TotalValue);

public sealed record GiftReceiptReportDto(
    DateOnly FromDate,
    DateOnly ToDate,
    int? OrganizationId,
    int? WorkstationId,
    GiftReceiptFilter Filter,
    IReadOnlyList<GiftReceiptRowDto> Rows,
    IReadOnlyList<GiftReceiptStatusTotalDto> StatusTotals,
    IReadOnlyList<GiftReceiptOrganizationTotalDto> OrganizationTotals,
    int TotalCount,
    decimal TotalValue);
