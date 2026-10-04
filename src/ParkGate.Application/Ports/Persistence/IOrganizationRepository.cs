using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface IOrganizationRepository
{
    Task<Organization?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Organization?> GetDefaultAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Organization>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default);
    void Add(Organization organization);
    void Update(Organization organization);
}
