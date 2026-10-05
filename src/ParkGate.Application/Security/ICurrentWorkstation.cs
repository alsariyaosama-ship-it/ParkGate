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
