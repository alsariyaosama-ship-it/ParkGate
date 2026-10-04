using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    void Add(User user);
    void Update(User user);
}
