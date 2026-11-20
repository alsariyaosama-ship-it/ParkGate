using ParkGate.Domain.CardPatterns;
using ParkGate.Domain.Common;

namespace ParkGate.Domain.Entities;

/// <summary>
/// A usual card-number pattern of an organization (F8, BR-29, DATABASE.md §3.13): a shape and an optional prefix.
/// Proposed by the system, decided by an Admin only.
/// </summary>
public sealed class OrganizationCardPattern : IAuditableEntity
{
    public const int MaxPrefixLength = 10;

    public int Id { get; private set; }
    public int OrganizationId { get; private set; }
    public string Shape { get; private set; } = null!;
    public string? Prefix { get; private set; }
    public CardPatternStatus Status { get; private set; }
    public CardPatternSource Source { get; private set; }
    public int SampleCount { get; private set; }
    public DateTime ProposedAtUtc { get; private set; }
    public DateTime? DecidedAtUtc { get; private set; }
    public int? DecidedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    private OrganizationCardPattern() { }

    private OrganizationCardPattern(int organizationId, string shape, string? prefix, CardPatternSource source, int sampleCount, DateTime proposedAtUtc)
    {
        if (string.IsNullOrEmpty(shape) || shape.Length > CardShape.MaxLength)
        {
            throw new ArgumentException($"Shape must be between 1 and {CardShape.MaxLength} characters.", nameof(shape));
        }

        if (prefix is not null && (prefix.Length == 0 || prefix.Length > MaxPrefixLength || prefix.Length > shape.Length))
        {
            throw new ArgumentException("Prefix must be shorter than the shape and at most 10 characters.", nameof(prefix));
        }

        if (sampleCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleCount));
        }

        OrganizationId = organizationId;
        Shape = shape;
        Prefix = prefix;
        Source = source;
        SampleCount = sampleCount;
        ProposedAtUtc = RequireUtc(proposedAtUtc, nameof(proposedAtUtc));
        Status = CardPatternStatus.Proposed;
    }

    public static OrganizationCardPattern Propose(int organizationId, string shape, string? prefix, CardPatternSource source, int sampleCount, DateTime proposedAtUtc) =>
        new(organizationId, shape, prefix, source, sampleCount, proposedAtUtc);

    /// <summary>
    /// Numbers that match an active pattern raise no alert: an approved pattern, or the first (dominant) proposal
    /// while the Admin has not decided. A new-series proposal is not active until approved.
    /// </summary>
    public bool IsActive =>
        Status == CardPatternStatus.Approved ||
        (Status == CardPatternStatus.Proposed && Source == CardPatternSource.Dominant);

    public bool Matches(string cardNumber)
    {
        ArgumentNullException.ThrowIfNull(cardNumber);
        return CardShape.Of(cardNumber) == Shape && (Prefix is null || cardNumber.StartsWith(Prefix, StringComparison.Ordinal));
    }

    public void Approve(int adminUserId, DateTime nowUtc) => Decide(CardPatternStatus.Approved, adminUserId, nowUtc);

    public void Reject(int adminUserId, DateTime nowUtc) => Decide(CardPatternStatus.Rejected, adminUserId, nowUtc);

    private void Decide(CardPatternStatus status, int adminUserId, DateTime nowUtc)
    {
        if (Status != CardPatternStatus.Proposed)
        {
            throw new InvalidOperationException("Only a proposed pattern can be approved or rejected.");
        }

        Status = status;
        DecidedByUserId = adminUserId;
        DecidedAtUtc = RequireUtc(nowUtc, nameof(nowUtc));
    }

    private static DateTime RequireUtc(DateTime value, string name) =>
        value.Kind == DateTimeKind.Unspecified ? throw new ArgumentException("Timestamp must have UTC kind.", name) : value;
}
