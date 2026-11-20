namespace ParkGate.Domain.Enums;

public enum DenialReason : byte
{
    CooldownActive = 1,
    HolderSuspended = 2,
    HolderBlocked = 3,
    CardExpired = 4,
    OrganizationInactive = 5,

    /// <summary>F8, BR-27: today is not one of the organization's entry days. Overridable.</summary>
    DayNotAllowed = 6,
}
