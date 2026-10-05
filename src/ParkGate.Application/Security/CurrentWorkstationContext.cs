using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.Security;

/// <summary>
/// In-memory context identifying the workstation on which the application is executing.
/// </summary>
public sealed class CurrentWorkstationContext : ICurrentWorkstation
{
    private readonly object _lock = new();

    public int? WorkstationId { get; private set; }
    public string MachineName { get; private set; }
    public string? DisplayName { get; private set; }
    public WorkstationType? Type { get; private set; }
    public bool IsRegistered => WorkstationId.HasValue;

    public CurrentWorkstationContext(string? machineName = null)
    {
        MachineName = (machineName ?? Environment.MachineName).Trim().ToUpperInvariant();
    }

    public void SetWorkstation(Workstation workstation)
    {
        ArgumentNullException.ThrowIfNull(workstation);

        lock (_lock)
        {
            WorkstationId = workstation.Id;
            MachineName = workstation.MachineName;
            DisplayName = workstation.DisplayName;
            Type = workstation.Type;
        }
    }

    public void SetWorkstation(int workstationId, string machineName, string displayName, WorkstationType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(machineName);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        lock (_lock)
        {
            WorkstationId = workstationId;
            MachineName = machineName.Trim().ToUpperInvariant();
            DisplayName = displayName.Trim();
            Type = type;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            WorkstationId = null;
            DisplayName = null;
            Type = null;
        }
    }
}
