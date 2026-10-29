using ParkGate.Domain.Enums;

namespace ParkGate.Domain.Entities;

/// <summary>
/// Gift voucher issued to a beneficiary as a monetary reward.
/// </summary>
public sealed class GiftVoucher
{
    public const int MaxOverrideReasonLength = 250;
    public const int MinOverrideReasonLength = 5;
    public const int MinVoidReasonLength = 5;

    public int Id { get; private set; }
    public Guid Token { get; private set; }
    public byte KeyId { get; private set; }
    public Guid BeneficiaryId { get; private set; }
    public int OrganizationId { get; private set; }
    public decimal Value { get; private set; }
    public DateTime IssuedAtUtc { get; private set; }
    public DateOnly BusinessDate { get; private set; }
    public DateOnly ExpiresOn { get; private set; }
    public GiftVoucherStatus Status { get; private set; }
    public int IssuedByUserId { get; private set; }
    public int IssuedWorkstationId { get; private set; }
    public bool IsOverride { get; private set; }
    public int? OverrideByUserId { get; private set; }
    public string? OverrideReason { get; private set; }
    public DateTime? RedeemedAtUtc { get; private set; }
    public DateOnly? RedeemedBusinessDate { get; private set; }
    public int? RedeemedByUserId { get; private set; }
    public int? RedeemedWorkstationId { get; private set; }
    public DateTime? VoidedAtUtc { get; private set; }
    public int? VoidedByUserId { get; private set; }
    public string? VoidReason { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private GiftVoucher() { }

    public static GiftVoucher Issue(
        Guid token,
        byte keyId,
        Guid beneficiaryId,
        int organizationId,
        decimal value,
        DateTime issuedAtUtc,
        DateOnly businessDate,
        int issuedByUserId,
        int issuedWorkstationId,
        int? overrideByUserId = null,
        string? overrideReason = null)
    {
        if (token == Guid.Empty)
        {
            throw new ArgumentException("Token must not be empty.", nameof(token));
        }

        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than zero.");
        }

        var trimmedReason = overrideReason?.Trim();

        if (overrideByUserId.HasValue && string.IsNullOrWhiteSpace(trimmedReason))
        {
            throw new ArgumentException("Override reason is required when overrideByUserId is provided.", nameof(overrideReason));
        }

        if (overrideByUserId.HasValue && trimmedReason!.Length < MinOverrideReasonLength)
        {
            throw new ArgumentException($"Override reason must be at least {MinOverrideReasonLength} characters.", nameof(overrideReason));
        }

        if (overrideByUserId.HasValue && trimmedReason!.Length > MaxOverrideReasonLength)
        {
            throw new ArgumentException($"Override reason cannot exceed {MaxOverrideReasonLength} characters.", nameof(overrideReason));
        }

        if (!overrideByUserId.HasValue && !string.IsNullOrWhiteSpace(overrideReason))
        {
            throw new ArgumentException("OverrideByUserId is required when overrideReason is provided.", nameof(overrideByUserId));
        }

        return new GiftVoucher
        {
            Token = token,
            KeyId = keyId,
            BeneficiaryId = beneficiaryId,
            OrganizationId = organizationId,
            Value = value,
            IssuedAtUtc = issuedAtUtc,
            BusinessDate = businessDate,
            ExpiresOn = businessDate,
            Status = GiftVoucherStatus.Issued,
            IssuedByUserId = issuedByUserId,
            IssuedWorkstationId = issuedWorkstationId,
            IsOverride = overrideByUserId.HasValue,
            OverrideByUserId = overrideByUserId,
            OverrideReason = overrideByUserId.HasValue ? trimmedReason : null,
        };
    }

    /// <summary>
    /// Marks the voucher as redeemed. Only valid from Issued status.
    /// </summary>
    public void Redeem(int userId, int workstationId, DateTime nowUtc, DateOnly businessDate)
    {
        if (Status != GiftVoucherStatus.Issued)
        {
            throw new InvalidOperationException("Voucher is not in Issued status.");
        }

        Status = GiftVoucherStatus.Redeemed;
        RedeemedAtUtc = nowUtc;
        RedeemedBusinessDate = businessDate;
        RedeemedByUserId = userId;
        RedeemedWorkstationId = workstationId;
    }

    /// <summary>
    /// Voids the voucher. Only valid from Issued status.
    /// </summary>
    public void Void(int userId, string reason, DateTime nowUtc)
    {
        if (Status != GiftVoucherStatus.Issued)
        {
            throw new InvalidOperationException("Voucher is not in Issued status.");
        }

        var trimmed = reason?.Trim() ?? string.Empty;

        if (trimmed.Length < MinVoidReasonLength)
        {
            throw new ArgumentException($"Void reason must be at least {MinVoidReasonLength} characters.", nameof(reason));
        }

        Status = GiftVoucherStatus.Voided;
        VoidedAtUtc = nowUtc;
        VoidedByUserId = userId;
        VoidReason = trimmed;
    }

    /// <summary>
    /// Returns true if the voucher's expiry date has passed (today > ExpiresOn).
    /// </summary>
    public bool IsExpired(DateOnly today) => today > ExpiresOn;
}
