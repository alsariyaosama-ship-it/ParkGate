using ParkGate.Domain.CardPatterns;

namespace ParkGate.Domain.Entities;

/// <summary>
/// A card number that does not look like its organization's usual numbers (F8, BR-29, DATABASE.md §3.14).
/// It never blocks the registration; the Admin reviews it.
/// </summary>
public sealed class CardPatternAlert
{
    public const int MaxReviewNoteLength = 250;

    public long Id { get; private set; }
    public int OrganizationId { get; private set; }
    public Guid BeneficiaryId { get; private set; }
    public string CardNumber { get; private set; } = null!;
    public string Shape { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedByUserId { get; private set; }
    public int WorkstationId { get; private set; }
    public CardPatternAlertStatus Status { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public int? ReviewedByUserId { get; private set; }
    public string? ReviewNote { get; private set; }

    private CardPatternAlert() { }

    private CardPatternAlert(int organizationId, Guid beneficiaryId, string cardNumber, DateTime createdAtUtc, int createdByUserId, int workstationId)
    {
        if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length > ValueObjects.CardNumber.MaxLength)
        {
            throw new ArgumentException("Card number is required.", nameof(cardNumber));
        }

        if (createdAtUtc.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Timestamp must have UTC kind.", nameof(createdAtUtc));
        }

        OrganizationId = organizationId;
        BeneficiaryId = beneficiaryId;
        CardNumber = cardNumber;
        Shape = CardShape.Of(cardNumber);
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
        WorkstationId = workstationId;
        Status = CardPatternAlertStatus.Open;
    }

    public static CardPatternAlert Raise(int organizationId, Guid beneficiaryId, string cardNumber, DateTime createdAtUtc, int createdByUserId, int workstationId) =>
        new(organizationId, beneficiaryId, cardNumber, createdAtUtc, createdByUserId, workstationId);

    public void Review(int adminUserId, DateTime nowUtc, string? note)
    {
        if (Status != CardPatternAlertStatus.Open)
        {
            throw new InvalidOperationException("The alert was already reviewed.");
        }

        if (nowUtc.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Timestamp must have UTC kind.", nameof(nowUtc));
        }

        var trimmed = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (trimmed is { Length: > MaxReviewNoteLength })
        {
            throw new ArgumentException($"Review note cannot exceed {MaxReviewNoteLength} characters.", nameof(note));
        }

        Status = CardPatternAlertStatus.Reviewed;
        ReviewedAtUtc = nowUtc;
        ReviewedByUserId = adminUserId;
        ReviewNote = trimmed;
    }
}
