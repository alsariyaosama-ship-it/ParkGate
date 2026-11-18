using ParkGate.Domain.EntryCards;

namespace ParkGate.Domain.Entities;

/// <summary>
/// One scan of an entry-card QR, accepted or not (table <c>CardScanLog</c>). Append-only.
/// </summary>
public sealed class CardScan
{
    public const int MaxStationNameLength = 100;
    public const int MaxRawPrefixLength = 32;

    public long Id { get; private set; }

    /// <summary>Set by the database default (BR-05).</summary>
    public DateTime ScannedAtUtc { get; private set; }

    public string StationName { get; private set; } = string.Empty;

    /// <summary>The desk that scanned, when it is a registered workstation.</summary>
    public int? WorkstationId { get; private set; }

    public CardScanStatus Status { get; private set; }
    public Guid? CardId { get; private set; }
    public string? RawPrefix { get; private set; }

    private CardScan() { }

    public static CardScan Record(string stationName, int? workstationId, CardScanStatus status, Guid? cardId, string? rawPrefix)
    {
        if (string.IsNullOrWhiteSpace(stationName))
        {
            throw new ArgumentException("Station name is required.", nameof(stationName));
        }

        var name = stationName.Trim();
        return new CardScan
        {
            StationName = name.Length > MaxStationNameLength ? name[..MaxStationNameLength] : name,
            WorkstationId = workstationId,
            Status = status,
            CardId = cardId,
            RawPrefix = rawPrefix is { Length: > MaxRawPrefixLength } ? rawPrefix[..MaxRawPrefixLength] : rawPrefix,
        };
    }
}
