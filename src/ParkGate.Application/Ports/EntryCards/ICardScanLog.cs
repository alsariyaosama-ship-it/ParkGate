using ParkGate.Domain.EntryCards;

namespace ParkGate.Application.Ports.EntryCards;

public sealed record CardScanLogEntry(string StationName, CardScanStatus Status, Guid? CardId, string? RawPrefix);

public interface ICardScanLog
{
    Task WriteAsync(CardScanLogEntry entry, CancellationToken ct = default);
}
