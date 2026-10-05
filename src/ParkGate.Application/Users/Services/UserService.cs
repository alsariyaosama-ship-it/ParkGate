using FluentValidation;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Audit;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Security;
using ParkGate.Application.Security;
using ParkGate.Application.Users.DTOs;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.Users.Services;

public sealed class UserService : IUserService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditWriter _auditWriter;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;
    private readonly IValidator<CreateUserRequest> _createValidator;

    public UserService(
        IUnitOfWorkFactory uowFactory,
        IPasswordHasher passwordHasher,
        IAuditWriter auditWriter,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        IValidator<CreateUserRequest> createValidator)
    {
        _uowFactory = uowFactory;
        _passwordHasher = passwordHasher;
        _auditWriter = auditWriter;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
        _createValidator = createValidator;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uowFactory.Create();
        var users = await uow.Users.GetAllAsync(cancellationToken);
        return users.Select(u => u.ToDto()).ToList();
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var uow = _uowFactory.Create();
        var user = await uow.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return Errors.User.NotFound;
        }

        return user.ToDto();
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageUsers))
        {
            return Errors.General.Forbidden;
        }

        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        await using var uow = _uowFactory.Create();
        var normalizedUserName = request.UserName.Trim().ToLowerInvariant();
        var existing = await uow.Users.GetByUserNameAsync(normalizedUserName, cancellationToken);
        if (existing is not null)
        {
            return Errors.User.UsernameAlreadyExists;
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = new User(
            normalizedUserName,
            request.FullName.Trim(),
            passwordHash,
            request.Role,
            mustChangePassword: true,
            isActive: true);

        uow.Users.Add(user);
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "UserCreated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(User),
            entityId: user.Id.ToString(),
            detailsJson: $"{{\"UserName\":\"{user.UserName}\",\"Role\":\"{user.Role}\"}}",
            cancellationToken: cancellationToken);

        return user.ToDto();
    }

    public async Task<Result<UserDto>> UpdateRoleAsync(int id, UserRole newRole, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageUsers))
        {
            return Errors.General.Forbidden;
        }

        if (_currentUser.UserId.HasValue && _currentUser.UserId.Value == id)
        {
            return Errors.User.CannotChangeOwnRole;
        }

        await using var uow = _uowFactory.Create();
        var user = await uow.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return Errors.User.NotFound;
        }

        user.SetRole(newRole);
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "UserRoleChanged",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(User),
            entityId: user.Id.ToString(),
            detailsJson: $"{{\"NewRole\":\"{newRole}\"}}",
            cancellationToken: cancellationToken);

        return user.ToDto();
    }

    public async Task<Result> ResetPasswordAsync(int id, string temporaryPassword, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageUsers))
        {
            return Errors.General.Forbidden;
        }

        if (string.IsNullOrWhiteSpace(temporaryPassword) || temporaryPassword.Length < 8)
        {
            return Errors.General.ValidationFailed([new ValidationError("temporaryPassword", "PasswordTooShort", "Password must be at least 8 characters.")]);
        }

        await using var uow = _uowFactory.Create();
        var user = await uow.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return Errors.User.NotFound;
        }

        var hash = _passwordHasher.HashPassword(temporaryPassword);
        user.ChangePassword(hash, mustChangePassword: true);
        user.ResetLockout();

        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "UserPasswordReset",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(User),
            entityId: user.Id.ToString(),
            cancellationToken: cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageUsers))
        {
            return Errors.General.Forbidden;
        }

        if (_currentUser.UserId.HasValue && _currentUser.UserId.Value == id)
        {
            return Errors.User.CannotDeactivateSelf;
        }

        await using var uow = _uowFactory.Create();
        var user = await uow.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return Errors.User.NotFound;
        }

        user.Deactivate();
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "UserDeactivated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(User),
            entityId: user.Id.ToString(),
            cancellationToken: cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageUsers))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var user = await uow.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return Errors.User.NotFound;
        }

        user.Activate();
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "UserReactivated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(User),
            entityId: user.Id.ToString(),
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
