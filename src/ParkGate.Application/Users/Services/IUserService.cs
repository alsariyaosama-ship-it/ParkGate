using ParkGate.Application.Common;
using ParkGate.Application.Users.DTOs;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.Users.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateRoleAsync(int id, UserRole newRole, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(int id, string temporaryPassword, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default);
}
