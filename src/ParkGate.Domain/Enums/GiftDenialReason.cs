namespace ParkGate.Domain.Enums;

public enum GiftDenialReason : byte
{
    OrganizationInactive = 1,
    GiftsNotEnabled = 2,
    HolderBlocked = 3,
    HolderSuspended = 4,
    IntervalNotElapsed = 5,
}
