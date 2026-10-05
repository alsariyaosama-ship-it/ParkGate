using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.Security;

/// <summary>
/// In-memory singleton context managing the active user session.
/// </summary>
public sealed class CurrentUserContext : ICurrentUserSession
{
    private readonly object _lock = new();

    public int? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? FullName { get; private set; }
    public UserRole? Role { get; private set; }
    public bool IsAuthenticated => UserId.HasValue;

    public void SetUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        lock (_lock)
        {
            UserId = user.Id;
            UserName = user.UserName;
            FullName = user.FullName;
            Role = user.Role;
        }
    }

    public void SetUser(int userId, string userName, string fullName, UserRole role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        lock (_lock)
        {
            UserId = userId;
            UserName = userName;
            FullName = fullName;
            Role = role;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            UserId = null;
            UserName = null;
            FullName = null;
            Role = null;
        }
    }

    public bool Has(Permission permission)
    {
        lock (_lock)
        {
            return Role.HasValue && RolePermissions.HasPermission(Role.Value, permission);
        }
    }

    public bool HasAny(params Permission[] permissions)
    {
        lock (_lock)
        {
            if (!Role.HasValue)
            {
                return false;
            }

            return permissions.Any(p => RolePermissions.HasPermission(Role.Value, p));
        }
    }

    public bool IsInRole(UserRole role)
    {
        lock (_lock)
        {
            return Role.HasValue && Role.Value == role;
        }
    }
}
