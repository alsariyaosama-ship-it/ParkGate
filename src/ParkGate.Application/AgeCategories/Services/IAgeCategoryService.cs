using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.Common;

namespace ParkGate.Application.AgeCategories.Services;

public interface IAgeCategoryService
{
    Task<IReadOnlyList<AgeCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgeCategoryDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Result<AgeCategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<AgeCategoryDto>> CreateAsync(CreateAgeCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<AgeCategoryDto>> UpdateAsync(UpdateAgeCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default);
}
