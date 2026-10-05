using ParkGate.Application.Auth.DTOs;
using ParkGate.Application.Auth.Services;
using ParkGate.Application.Auth.Validators;
using ParkGate.Application.Common;
using ParkGate.Application.Security;
using ParkGate.Application.Tests.Common;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.Tests.Auth;

public sealed class AuthServiceTests
{
    private readonly List<User> _users = [];
    private readonly List<Workstation> _workstations = [];
    private readonly FakeAppSettingRepository _settings = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeAuditWriter _auditWriter = new();
    private readonly CurrentUserContext _currentUserSession = new();
    private readonly CurrentWorkstationContext _workstationSession = new("GATE-01");
    private readonly FakeUnitOfWorkFactory _uowFactory;

    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _uowFactory = new FakeUnitOfWorkFactory(_users, _workstations, _settings);

        _sut = new AuthService(
            _uowFactory,
            _passwordHasher,
            _auditWriter,
            _currentUserSession,
            _workstationSession,
            new FirstRunAdminRequestValidator(),
            new LoginRequestValidator(),
            new ChangePasswordRequestValidator());
    }

    [Fact]
    public async Task IsFirstRunAsync_WhenNoUsersExist_ShouldReturnTrue()
    {
        var result = await _sut.IsFirstRunAsync();
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task IsFirstRunAsync_WhenUsersExist_ShouldReturnFalse()
    {
        _users.Add(new User("admin", "Admin User", "hash", UserRole.Admin));

        var result = await _sut.IsFirstRunAsync();
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CompleteFirstRunAsync_WithValidAdmin_ShouldCreateAdminAndAudit()
    {
        var request = new FirstRunAdminRequest("admin", "Admin User", "StrongPass123", "StrongPass123");

        var result = await _sut.CompleteFirstRunAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.UserName.ShouldBe("admin");
        result.Value.FullName.ShouldBe("Admin User");
        result.Value.Role.ShouldBe(UserRole.Admin);
        result.Value.MustChangePassword.ShouldBeFalse();

        _users.Count.ShouldBe(1);
        _users[0].PasswordHash.ShouldBe("HASHED_StrongPass123");

        _auditWriter.Writes.ShouldContain(w => w.Action == "FirstRunAdminCreated");
    }

    [Fact]
    public async Task CompleteFirstRunAsync_WhenAlreadyCompleted_ShouldReturnError()
    {
        _users.Add(new User("admin", "Admin User", "hash", UserRole.Admin));
        var request = new FirstRunAdminRequest("admin2", "Admin Two", "StrongPass123", "StrongPass123");

        var result = await _sut.CompleteFirstRunAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Auth.FirstRunAlreadyCompleted.Code);
    }

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ShouldReturnInvalidCredentials()
    {
        var request = new LoginRequest("unknown", "password");

        var result = await _sut.LoginAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Auth.InvalidCredentials.Code);
    }

    [Fact]
    public async Task LoginAsync_WhenUserInactive_ShouldReturnAccountInactive()
    {
        var user = new User("ahmed", "Ahmed", _passwordHasher.HashPassword("pass1234"), UserRole.Operator, isActive: false);
        _users.Add(user);

        var result = await _sut.LoginAsync(new LoginRequest("ahmed", "pass1234"));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Auth.AccountInactive.Code);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIncorrect_ShouldIncrementFailedCountAndEventuallyLockout()
    {
        var user = new User("operator1", "Desk Operator", _passwordHasher.HashPassword("secret123"), UserRole.Operator);
        _users.Add(user);

        // Attempt 1..4 failures
        for (int i = 1; i <= 4; i++)
        {
            var r = await _sut.LoginAsync(new LoginRequest("operator1", "wrongpassword"));
            r.IsFailure.ShouldBeTrue();
            r.Error.Code.ShouldBe(Errors.Auth.InvalidCredentials.Code);
            user.FailedLoginCount.ShouldBe(i);
        }

        // Attempt 5 failure -> triggers lockout (BR-18)
        var lockResult = await _sut.LoginAsync(new LoginRequest("operator1", "wrongpassword"));
        lockResult.IsFailure.ShouldBeTrue();
        lockResult.Error.Code.ShouldBe(Errors.Auth.AccountLocked.Code);
        user.IsLockedOut(_uowFactory.CurrentUow.CurrentDbTime).ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "UserLockedOut");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldAutoRegisterWorkstationAndSetSession()
    {
        var user = new User("supervisor", "Park Supervisor", _passwordHasher.HashPassword("superpass"), UserRole.Supervisor);
        _users.Add(user);

        var result = await _sut.LoginAsync(new LoginRequest("supervisor", "superpass", "DESK-PC-99"));

        result.IsSuccess.ShouldBeTrue();
        result.Value.User.UserName.ShouldBe("supervisor");
        result.Value.Workstation.MachineName.ShouldBe("DESK-PC-99");

        _currentUserSession.IsAuthenticated.ShouldBeTrue();
        _currentUserSession.UserName.ShouldBe("supervisor");
        _currentUserSession.Role.ShouldBe(UserRole.Supervisor);

        _workstationSession.IsRegistered.ShouldBeTrue();
        _workstationSession.MachineName.ShouldBe("DESK-PC-99");

        _auditWriter.Writes.ShouldContain(w => w.Action == "UserLogin");
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenPasswordsDoNotMatchCurrent_ShouldFail()
    {
        var user = new User("user1", "User One", _passwordHasher.HashPassword("oldpass123"), UserRole.Operator);
        _users.Add(user);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, 10);

        _currentUserSession.SetUser(user);

        var request = new ChangePasswordRequest(10, "wrongold", "NewPass1234", "NewPass1234");
        var result = await _sut.ChangePasswordAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Auth.PasswordMismatch.Code);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenValid_ShouldUpdateHashAndClearMustChangePassword()
    {
        var user = new User("user1", "User One", _passwordHasher.HashPassword("oldpass123"), UserRole.Operator, mustChangePassword: true);
        _users.Add(user);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, 10);

        _currentUserSession.SetUser(user);

        var request = new ChangePasswordRequest(10, "oldpass123", "NewPass1234", "NewPass1234");
        var result = await _sut.ChangePasswordAsync(request);

        result.IsSuccess.ShouldBeTrue();
        user.PasswordHash.ShouldBe("HASHED_NewPass1234");
        user.MustChangePassword.ShouldBeFalse();
        _auditWriter.Writes.ShouldContain(w => w.Action == "PasswordChanged");
    }

    [Fact]
    public async Task ValidateSupervisorCredentialsAsync_ShouldAcceptOnlySupervisorOrAdmin()
    {
        var op = new User("op", "Operator", _passwordHasher.HashPassword("oppass"), UserRole.Operator);
        var sup = new User("sup", "Supervisor", _passwordHasher.HashPassword("suppass"), UserRole.Supervisor);
        _users.AddRange([op, sup]);

        var opResult = await _sut.ValidateSupervisorCredentialsAsync("op", "oppass");
        opResult.IsFailure.ShouldBeTrue();
        opResult.Error.Code.ShouldBe(Errors.Auth.SupervisorRequired.Code);

        var supResult = await _sut.ValidateSupervisorCredentialsAsync("sup", "suppass");
        supResult.IsSuccess.ShouldBeTrue();
        supResult.Value.UserName.ShouldBe("sup");
        supResult.Value.Role.ShouldBe(UserRole.Supervisor);
    }
}
