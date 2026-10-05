using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.Users.DTOs;

public sealed record UserDto(
    int Id,
    string UserName,
    string FullName,
    UserRole Role,
    bool IsActive,
    bool MustChangePassword,
    DateTime? LastLoginUtc);

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user) =>
        new(
            user.Id,
            user.UserName,
            user.FullName,
            user.Role,
            user.IsActive,
            user.MustChangePassword,
            user.LastLoginUtc);
}
