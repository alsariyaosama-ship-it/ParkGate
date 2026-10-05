using ParkGate.Domain.Enums;

namespace ParkGate.Application.Workstations.DTOs;

public sealed record UpdateWorkstationRequest(
    int Id,
    string DisplayName,
    WorkstationType Type);
