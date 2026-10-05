using ParkGate.Domain.Enums;

namespace ParkGate.Application.Security;

/// <summary>
/// Provides information about the physical workstation executing the application.
/// </summary>
public interface ICurrentWorkstation
{
    int? WorkstationId { get; }
    string MachineName { get; }
    string? DisplayName { get; }
    WorkstationType? Type { get; }
    bool IsRegistered { get; }
}

/// <summary>
/// Session-management interface for mutating the current workstation state on initialization/registration.
/// </summary>
public interface ICurrentWorkstationSession : ICurrentWorkstation
{
    void SetWorkstation(ParkGate.Domain.Entities.Workstation workstation);
    void SetWorkstation(int workstationId, string machineName, string displayName, WorkstationType type);
    void Clear();
}
