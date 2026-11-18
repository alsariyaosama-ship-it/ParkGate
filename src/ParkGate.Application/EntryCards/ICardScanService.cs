namespace ParkGate.Application.EntryCards;

/// <summary>
/// Verifies a scanned entry-card QR text and logs the attempt (see <see cref="CardScanService"/>).
/// </summary>
public interface ICardScanService
{
    /// <summary>Checks signature, expiry and the card's database state; every attempt is logged.</summary>
    Task<CardScanResult> ScanAsync(string? scannedText, CancellationToken ct = default);
}
