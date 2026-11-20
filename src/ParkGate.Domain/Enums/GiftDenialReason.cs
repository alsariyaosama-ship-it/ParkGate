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

    /// <summary>F8, BR-28: today is not an entry day of the organization and gifts are not allowed on any day. Overridable.</summary>
    DayNotAllowed = 7,
}
