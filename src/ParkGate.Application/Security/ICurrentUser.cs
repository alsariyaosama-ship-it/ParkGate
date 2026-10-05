using ParkGate.Domain.Enums;

namespace ParkGate.Application.Security;

/// <summary>
/// Provides information about the currently authenticated user in the active context.
/// </summary>
public interface ICurrentUser
{
    int? UserId { get; }
    string? UserName { get; }
    string? FullName { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }

    bool Has(Permission permission);
    bool HasAny(params Permission[] permissions);
    bool IsInRole(UserRole role);
}

/// <summary>
/// Session-management interface for mutating the current user session on login/logout.
/// </summary>
public interface ICurrentUserSession : ICurrentUser
{
    void SetUser(ParkGate.Domain.Entities.User user);
    void SetUser(int userId, string userName, string fullName, UserRole role);
    void Clear();
}
