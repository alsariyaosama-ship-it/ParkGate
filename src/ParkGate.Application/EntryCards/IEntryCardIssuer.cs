namespace ParkGate.Application.EntryCards;

/// <summary>
/// Issues a signed entry-card QR for a holder (see <see cref="EntryCardIssuer"/>).
/// </summary>
public interface IEntryCardIssuer
{
    /// <summary>Signs a new card for the holder and stores it; <paramref name="expiresOn"/> defaults to the validity setting.</summary>
    Task<IssuedEntryCard> IssueAsync(Guid beneficiaryId, DateOnly? expiresOn = null, CancellationToken ct = default);
}
