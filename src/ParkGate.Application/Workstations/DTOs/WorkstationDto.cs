using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.Workstations.DTOs;

public sealed record WorkstationDto(
    int Id,
    string MachineName,
    string DisplayName,
    WorkstationType Type,
    bool IsActive,
    DateTime? LastSeenUtc);

public static class WorkstationMappingExtensions
{
    public static WorkstationDto ToDto(this Workstation workstation) =>
        new(
            workstation.Id,
            workstation.MachineName,
            workstation.DisplayName,
            workstation.Type,
            workstation.IsActive,
            workstation.LastSeenUtc);
}
