using ParkGate.Application.Common;
using ParkGate.Application.Workstations.DTOs;

namespace ParkGate.Application.Workstations.Services;

public interface IWorkstationService
{
    Task<IReadOnlyList<WorkstationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<WorkstationDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<WorkstationDto>> UpdateAsync(UpdateWorkstationRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default);
}
