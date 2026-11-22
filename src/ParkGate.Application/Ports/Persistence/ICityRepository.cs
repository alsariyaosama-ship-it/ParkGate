using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface ICityRepository
{
    /// <summary>Active cities in their fixed display order (SortOrder, then Id).</summary>
    Task<IReadOnlyList<City>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Every city, active or not, in display order. Used to resolve names of saved holders.</summary>
    Task<IReadOnlyList<City>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<City?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
