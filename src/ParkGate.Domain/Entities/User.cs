using ParkGate.Domain.Common;
using ParkGate.Domain.Enums;

namespace ParkGate.Domain.Entities;

/// <summary>
/// User account (مستخدم النظام) with authentication and lockout tracking (BR-18).
/// </summary>
public sealed class User : IAuditableEntity
{
    public const int MaxUserNameLength = 50;
    public const int MaxFullNameLength = 150;
    public const int MaxPasswordHashLength = 300;

    public int Id { get; private set; }
    public string UserName { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public bool MustChangePassword { get; private set; }
    public int FailedLoginCount { get; private set; }
    public DateTime? LockoutUntilUtc { get; private set; }
    public DateTime? LastLoginUtc { get; private set; }

    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
    public byte[] RowVersion { get; set; } = [];

    private User() { }

    public User(
        string userName,
        string fullName,
        string passwordHash,
        UserRole role,
        bool mustChangePassword = false,
        bool isActive = true)
    {
        ValidateUserName(userName);
        ValidateFullName(fullName);
        ValidatePasswordHash(passwordHash);

        UserName = userName.Trim().ToLowerInvariant();
        FullName = fullName.Trim();
        PasswordHash = passwordHash;
        Role = role;
        MustChangePassword = mustChangePassword;
        IsActive = isActive;
        FailedLoginCount = 0;
        LockoutUntilUtc = null;
        LastLoginUtc = null;
    }

    public void UpdateProfile(string fullName)
    {
        ValidateFullName(fullName);
        FullName = fullName.Trim();
    }

    public void ChangePassword(string newPasswordHash, bool mustChangePassword = false)
    {
        ValidatePasswordHash(newPasswordHash);
        PasswordHash = newPasswordHash;
        MustChangePassword = mustChangePassword;
    }

    public void SetRole(UserRole role)
    {
        Role = role;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public bool IsLockedOut(DateTime nowUtc)
    {
        return LockoutUntilUtc.HasValue && LockoutUntilUtc.Value > nowUtc;
    }

    public void RecordSuccessfulLogin(DateTime nowUtc)
    {
        FailedLoginCount = 0;
        LockoutUntilUtc = null;
        LastLoginUtc = nowUtc;
    }

    public void RecordFailedLogin(DateTime nowUtc, int maxFailedLogins, int lockoutMinutes)
    {
        FailedLoginCount++;
        if (FailedLoginCount >= maxFailedLogins)
        {
            LockoutUntilUtc = nowUtc.AddMinutes(lockoutMinutes);
        }
    }

    public void ResetLockout()
    {
        FailedLoginCount = 0;
        LockoutUntilUtc = null;
    }

    private static void ValidateUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName) || userName.Trim().Length > MaxUserNameLength)
        {
            throw new ArgumentException($"User name must be between 1 and {MaxUserNameLength} characters.", nameof(userName));
        }
    }

    private static void ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length > MaxFullNameLength)
        {
            throw new ArgumentException($"Full name must be between 1 and {MaxFullNameLength} characters.", nameof(fullName));
        }
    }

    private static void ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length > MaxPasswordHashLength)
        {
            throw new ArgumentException($"Password hash cannot be empty or exceed {MaxPasswordHashLength} characters.", nameof(passwordHash));
        }
    }
}
