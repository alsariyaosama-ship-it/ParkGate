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
