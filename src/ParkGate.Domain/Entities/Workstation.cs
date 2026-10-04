using ParkGate.Domain.Enums;

namespace ParkGate.Domain.Entities;

/// <summary>
/// Workstation (نقطة الدخول) representing a physical PC or gate terminal.
/// </summary>
public sealed class Workstation
{
    public const int MaxMachineNameLength = 64;
    public const int MaxDisplayNameLength = 100;

    public int Id { get; private set; }
    public string MachineName { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public WorkstationType Type { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastSeenUtc { get; private set; }

    private Workstation() { }

    public Workstation(
        string machineName,
        string displayName,
        WorkstationType type = WorkstationType.Desk,
        bool isActive = true)
    {
        ValidateMachineName(machineName);
        ValidateDisplayName(displayName);

        MachineName = machineName.Trim().ToUpperInvariant();
        DisplayName = displayName.Trim();
        Type = type;
        IsActive = isActive;
        LastSeenUtc = null;
    }

    public void Update(string displayName, WorkstationType type)
    {
        ValidateDisplayName(displayName);
        DisplayName = displayName.Trim();
        Type = type;
    }

    public void RecordHeartbeat(DateTime seenAtUtc)
    {
        if (seenAtUtc.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Timestamp must have UTC kind.", nameof(seenAtUtc));
        }

        LastSeenUtc = seenAtUtc;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static void ValidateMachineName(string machineName)
    {
        if (string.IsNullOrWhiteSpace(machineName) || machineName.Trim().Length > MaxMachineNameLength)
        {
            throw new ArgumentException($"Machine name must be between 1 and {MaxMachineNameLength} characters.", nameof(machineName));
        }
    }

    private static void ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Trim().Length > MaxDisplayNameLength)
        {
            throw new ArgumentException($"Display name must be between 1 and {MaxDisplayNameLength} characters.", nameof(displayName));
        }
    }
}
