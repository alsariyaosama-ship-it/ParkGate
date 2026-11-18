namespace ParkGate.Domain.Enums;

public enum GiftDenialReason : byte
{
    OrganizationInactive = 1,
    GiftsNotEnabled = 2,
    HolderBlocked = 3,
    HolderSuspended = 4,
    IntervalNotElapsed = 5,

    /// <summary>No granted or overridden, non-voided entry on today's business date. Not overridable.</summary>
    NoEntryToday = 6,
}
