using ParkGate.Application.Common;
using ParkGate.Application.Organizations.DTOs;

namespace ParkGate.Application.Organizations.Services;

public interface IOrganizationService
{
    Task<IReadOnlyList<OrganizationDto>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
    Task<Result<OrganizationDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<OrganizationDto>> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrganizationDto>> UpdateAsync(UpdateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default);
}
