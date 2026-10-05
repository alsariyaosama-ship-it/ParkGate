using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface IAgeCategoryRepository
{
    Task<IReadOnlyList<AgeCategory>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<AgeCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
