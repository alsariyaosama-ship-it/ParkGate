using FluentValidation;
using ParkGate.Application.Auth.DTOs;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Audit;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Security;
using ParkGate.Application.Security;
using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Workstations.DTOs;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditWriter _auditWriter;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly ICurrentWorkstationSession _workstationSession;
    private readonly IValidator<FirstRunAdminRequest> _firstRunValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    public AuthService(
        IUnitOfWorkFactory uowFactory,
        IPasswordHasher passwordHasher,
        IAuditWriter auditWriter,
        ICurrentUserSession currentUserSession,
        ICurrentWorkstationSession workstationSession,
        IValidator<FirstRunAdminRequest> firstRunValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator)
    {
        _uowFactory = uowFactory;
        _passwordHasher = passwordHasher;
        _auditWriter = auditWriter;
        _currentUserSession = currentUserSession;
        _workstationSession = workstationSession;
        _firstRunValidator = firstRunValidator;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    public async Task<bool> IsFirstRunAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uowFactory.Create();
        var count = await uow.Users.CountAsync(cancellationToken);
        return count == 0;
    }

    public async Task<Result<UserDto>> CompleteFirstRunAsync(FirstRunAdminRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _firstRunValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        await using var uow = _uowFactory.Create();
        var existingCount = await uow.Users.CountAsync(cancellationToken);
        if (existingCount > 0)
        {
            return Errors.Auth.FirstRunAlreadyCompleted;
        }

        var normalizedUserName = request.UserName.Trim().ToLowerInvariant();
        var existingUser = await uow.Users.GetByUserNameAsync(normalizedUserName, cancellationToken);
        if (existingUser is not null)
        {
            return Errors.User.UsernameAlreadyExists;
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var admin = new User(
            normalizedUserName,
            request.FullName.Trim(),
            passwordHash,
            UserRole.Admin,
            mustChangePassword: false,
            isActive: true);

        uow.Users.Add(admin);
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "FirstRunAdminCreated",
            userId: admin.Id,
            workstationId: _workstationSession.WorkstationId,
            entityName: nameof(User),
            entityId: admin.Id.ToString(),
            cancellationToken: cancellationToken);

        return admin.ToDto();
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        await using var uow = _uowFactory.Create();
        var nowUtc = await uow.GetDbUtcNowAsync(cancellationToken);

        int maxFailedLogins = 5;
        var maxFailedSetting = await uow.AppSettings.GetByKeyAsync("Security.MaxFailedLogins", cancellationToken);
        if (maxFailedSetting is not null && int.TryParse(maxFailedSetting.Value, out var parsedMax))
        {
            maxFailedLogins = parsedMax;
        }

        int lockoutMinutes = 15;
        var lockoutSetting = await uow.AppSettings.GetByKeyAsync("Security.LockoutMinutes", cancellationToken);
        if (lockoutSetting is not null && int.TryParse(lockoutSetting.Value, out var parsedMinutes))
        {
            lockoutMinutes = parsedMinutes;
        }

        var normalizedUserName = request.UserName.Trim().ToLowerInvariant();
        var user = await uow.Users.GetByUserNameAsync(normalizedUserName, cancellationToken);
        if (user is null)
        {
            return Errors.Auth.InvalidCredentials;
        }

        if (!user.IsActive)
        {
            return Errors.Auth.AccountInactive;
        }

        if (user.IsLockedOut(nowUtc))
        {
            return Errors.Auth.AccountLocked;
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash, out var needsRehash);
        if (!isPasswordValid)
        {
            user.RecordFailedLogin(nowUtc, maxFailedLogins, lockoutMinutes);
            await uow.SaveChangesAsync(cancellationToken);

            if (user.IsLockedOut(nowUtc))
            {
                await _auditWriter.WriteAsync(
                    action: "UserLockedOut",
                    userId: user.Id,
                    workstationId: _workstationSession.WorkstationId,
                    entityName: nameof(User),
                    entityId: user.Id.ToString(),
                    cancellationToken: cancellationToken);

                return Errors.Auth.AccountLocked;
            }

            return Errors.Auth.InvalidCredentials;
        }

        if (needsRehash)
        {
            user.ChangePassword(_passwordHasher.HashPassword(request.Password), user.MustChangePassword);
        }

        user.RecordSuccessfulLogin(nowUtc);

        var machineName = (request.MachineName ?? _workstationSession.MachineName ?? Environment.MachineName).Trim().ToUpperInvariant();
        var workstation = await uow.Workstations.GetByMachineNameAsync(machineName, cancellationToken);
        if (workstation is null)
        {
            workstation = new Workstation(machineName, machineName, WorkstationType.Desk, isActive: true);
            workstation.RecordHeartbeat(nowUtc);
            uow.Workstations.Add(workstation);
        }
        else
        {
            if (!workstation.IsActive)
            {
                return Errors.Auth.WorkstationInactive;
            }

            workstation.RecordHeartbeat(nowUtc);
        }

        await uow.SaveChangesAsync(cancellationToken);

        _currentUserSession.SetUser(user);
        _workstationSession.SetWorkstation(workstation);

        await _auditWriter.WriteAsync(
            action: "UserLogin",
            userId: user.Id,
            workstationId: workstation.Id,
            entityName: nameof(User),
            entityId: user.Id.ToString(),
            cancellationToken: cancellationToken);

        return new LoginResponse(user.ToDto(), workstation.ToDto(), user.MustChangePassword);
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _changePasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        var isSelf = _currentUserSession.UserId.HasValue && _currentUserSession.UserId.Value == request.UserId;
        var hasManageUsers = _currentUserSession.Has(Permission.ManageUsers);

        if (!isSelf && !hasManageUsers)
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var user = await uow.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Errors.User.NotFound;
        }

        if (!isSelf && hasManageUsers)
        {
            // Admin reset: allows setting new password without providing old password if empty
            // But if CurrentPassword is provided, verify it
            if (!string.IsNullOrEmpty(request.CurrentPassword) &&
                !_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash, out _))
            {
                return Errors.Auth.PasswordMismatch;
            }
        }
        else
        {
            if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash, out _))
            {
                return Errors.Auth.PasswordMismatch;
            }
        }

        var newHash = _passwordHasher.HashPassword(request.NewPassword);
        user.ChangePassword(newHash, mustChangePassword: false);

        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "PasswordChanged",
            userId: _currentUserSession.UserId ?? user.Id,
            workstationId: _workstationSession.WorkstationId,
            entityName: nameof(User),
            entityId: user.Id.ToString(),
            cancellationToken: cancellationToken);

        return Result.Success();
    }

    public async Task<Result<UserDto>> ValidateSupervisorCredentialsAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            return Errors.Auth.InvalidCredentials;
        }

        await using var uow = _uowFactory.Create();
        var nowUtc = await uow.GetDbUtcNowAsync(cancellationToken);

        var normalizedUserName = userName.Trim().ToLowerInvariant();
        var user = await uow.Users.GetByUserNameAsync(normalizedUserName, cancellationToken);
        if (user is null || !user.IsActive || user.IsLockedOut(nowUtc))
        {
            return Errors.Auth.InvalidCredentials;
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash, out _))
        {
            return Errors.Auth.InvalidCredentials;
        }

        if (user.Role != UserRole.Supervisor && user.Role != UserRole.Admin)
        {
            return Errors.Auth.SupervisorRequired;
        }

        return user.ToDto();
    }

    public async Task<Result<WorkstationDto>> EnsureWorkstationRegisteredAsync(string? machineName = null, CancellationToken cancellationToken = default)
    {
        await using var uow = _uowFactory.Create();
        var nowUtc = await uow.GetDbUtcNowAsync(cancellationToken);

        var targetMachine = (machineName ?? _workstationSession.MachineName ?? Environment.MachineName).Trim().ToUpperInvariant();
        var workstation = await uow.Workstations.GetByMachineNameAsync(targetMachine, cancellationToken);
        if (workstation is null)
        {
            workstation = new Workstation(targetMachine, targetMachine, WorkstationType.Desk, isActive: true);
            workstation.RecordHeartbeat(nowUtc);
            uow.Workstations.Add(workstation);
        }
        else
        {
            workstation.RecordHeartbeat(nowUtc);
        }

        await uow.SaveChangesAsync(cancellationToken);
        _workstationSession.SetWorkstation(workstation);

        return workstation.ToDto();
    }

    public void Logout()
    {
        _currentUserSession.Clear();
    }
}
