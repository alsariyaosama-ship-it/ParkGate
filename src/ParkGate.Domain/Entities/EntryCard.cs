namespace ParkGate.Domain.Entities;

/// <summary>
/// A QR entry card issued to a holder. A holder has at most one active card; issuing a new one revokes the old,
/// so a photo of the previous card stops working.
/// </summary>
public sealed class EntryCard
{
    public const int MaxRevokeReasonLength = 200;

    /// <summary>Reason stored when a card is replaced by a newly issued one.</summary>
    public const string ReplacedReason = "استُبدلت ببطاقة جديدة";

    public int Id { get; private set; }
    public Guid CardId { get; private set; }
    public Guid BeneficiaryId { get; private set; }
    public byte KeyId { get; private set; }

    /// <summary>Set by the database default (BR-05).</summary>
    public DateTime IssuedAtUtc { get; private set; }

    public DateOnly ExpiresOn { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevokeReason { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public bool IsRevoked => RevokedAtUtc.HasValue;

    private EntryCard() { }

    public static EntryCard Issue(Guid cardId, Guid beneficiaryId, byte keyId, DateOnly expiresOn)
    {
        if (cardId == Guid.Empty)
        {
            throw new ArgumentException("Card id must not be empty.", nameof(cardId));
        }

        if (beneficiaryId == Guid.Empty)
        {
            throw new ArgumentException("Beneficiary id must not be empty.", nameof(beneficiaryId));
        }

        if (keyId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(keyId), "Key id starts at 1.");
        }

        return new EntryCard
        {
            CardId = cardId,
            BeneficiaryId = beneficiaryId,
            KeyId = keyId,
            ExpiresOn = expiresOn,
        };
    }

    public void Revoke(DateTime nowUtc, string reason)
    {
        if (IsRevoked)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Revoke reason is required.", nameof(reason));
        }

        var trimmed = reason.Trim();
        RevokedAtUtc = nowUtc;
        RevokeReason = trimmed.Length > MaxRevokeReasonLength ? trimmed[..MaxRevokeReasonLength] : trimmed;
    }
}
