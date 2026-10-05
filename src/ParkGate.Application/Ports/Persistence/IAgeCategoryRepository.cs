using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface IAgeCategoryRepository
{
    Task<IReadOnlyList<AgeCategory>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgeCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AgeCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AgeCategory?> GetByNameAsync(string nameAr, CancellationToken cancellationToken = default);
    void Add(AgeCategory category);
    void Update(AgeCategory category);
}
