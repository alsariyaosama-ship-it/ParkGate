using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Entities;

public sealed class UserTests
{
    private static User CreateSampleUser()
    {
        return new User(
            "admin",
            "مدير النظام",
            "PBKDF2$SHA256$600000$salt$hash",
            UserRole.Admin);
    }

    [Fact]
    public void Constructor_ValidInputs_InitializesCorrectly()
    {
        var user = new User("  Admin_1  ", "  أحمد علي  ", "HASH", UserRole.Operator, mustChangePassword: true);

        user.UserName.ShouldBe("admin_1");
        user.FullName.ShouldBe("أحمد علي");
        user.PasswordHash.ShouldBe("HASH");
        user.Role.ShouldBe(UserRole.Operator);
        user.MustChangePassword.ShouldBeTrue();
        user.IsActive.ShouldBeTrue();
        user.FailedLoginCount.ShouldBe(0);
        user.LockoutUntilUtc.ShouldBeNull();
        user.LastLoginUtc.ShouldBeNull();
    }

    [Fact]
    public void RecordSuccessfulLogin_ClearsLockoutAndSetsLastLogin()
    {
        var user = CreateSampleUser();
        var nowUtc = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);

        user.RecordFailedLogin(nowUtc, maxFailedLogins: 5, lockoutMinutes: 15);
        user.FailedLoginCount.ShouldBe(1);

        user.RecordSuccessfulLogin(nowUtc);
        user.FailedLoginCount.ShouldBe(0);
        user.LastLoginUtc.ShouldBe(nowUtc);
        user.LockoutUntilUtc.ShouldBeNull();
    }

    [Fact]
    public void RecordFailedLogin_ReachingThreshold_LocksOutUser()
    {
        var user = CreateSampleUser();
        var nowUtc = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);

        // 4 failed logins (threshold is 5)
        for (var i = 0; i < 4; i++)
        {
            user.RecordFailedLogin(nowUtc, maxFailedLogins: 5, lockoutMinutes: 15);
            user.IsLockedOut(nowUtc).ShouldBeFalse();
        }

        // 5th failed login triggers lockout (BR-18)
        user.RecordFailedLogin(nowUtc, maxFailedLogins: 5, lockoutMinutes: 15);
        user.FailedLoginCount.ShouldBe(5);
        user.IsLockedOut(nowUtc).ShouldBeTrue();
        user.LockoutUntilUtc.ShouldBe(nowUtc.AddMinutes(15));

        // After 15 minutes, no longer locked out
        user.IsLockedOut(nowUtc.AddMinutes(16)).ShouldBeFalse();
    }

    [Fact]
    public void ResetLockout_ClearsFailedCountAndLockoutDate()
    {
        var user = CreateSampleUser();
        var nowUtc = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);

        for (var i = 0; i < 5; i++)
        {
            user.RecordFailedLogin(nowUtc, maxFailedLogins: 5, lockoutMinutes: 15);
        }

        user.IsLockedOut(nowUtc).ShouldBeTrue();

        user.ResetLockout();
        user.FailedLoginCount.ShouldBe(0);
        user.IsLockedOut(nowUtc).ShouldBeFalse();
    }

    [Fact]
    public void ChangePassword_UpdatesHashAndMustChangeFlag()
    {
        var user = CreateSampleUser();
        user.ChangePassword("NEW_HASH", mustChangePassword: true);

        user.PasswordHash.ShouldBe("NEW_HASH");
        user.MustChangePassword.ShouldBeTrue();
    }
}
