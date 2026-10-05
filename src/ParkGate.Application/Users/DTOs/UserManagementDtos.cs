using ParkGate.Domain.Enums;

namespace ParkGate.Application.Users.DTOs;

public sealed record CreateUserRequest(
    string UserName,
    string FullName,
    string Password,
    UserRole Role);

public sealed record UpdateUserRoleRequest(
    int Id,
    UserRole Role);

public sealed record ResetUserPasswordRequest(
    int Id,
    string NewPassword);
