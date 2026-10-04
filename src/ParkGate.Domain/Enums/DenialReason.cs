namespace ParkGate.Domain.Enums;

public enum DenialReason : byte
{
    CooldownActive = 1,
    HolderSuspended = 2,
    HolderBlocked = 3,
    CardExpired = 4,
    OrganizationInactive = 5
}
