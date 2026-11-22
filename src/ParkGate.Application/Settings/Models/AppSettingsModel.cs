namespace ParkGate.Application.Settings.Models;

public sealed record AppSettingsModel(
    int DefaultCooldownMinutes,
    int MaxGroupSize,
    int DenialDedupMinutes,
    bool PrefillCompanionsFromLastVisit,
    bool DenialSoundEnabled,
    int MaxFailedLogins,
    int LockoutMinutes,
    int AutoLockMinutes,
    string ParkName,
    string TimeZoneId,
    bool SuccessSoundEnabled = true,
    bool PrintEntryReceipt = true);

public sealed record UpdateSettingsRequest(
    int DefaultCooldownMinutes,
    int MaxGroupSize,
    int DenialDedupMinutes,
    bool PrefillCompanionsFromLastVisit,
    bool DenialSoundEnabled,
    int MaxFailedLogins,
    int LockoutMinutes,
    int AutoLockMinutes,
    string ParkName,
    string TimeZoneId,
    bool SuccessSoundEnabled = true,
    bool PrintEntryReceipt = true);
