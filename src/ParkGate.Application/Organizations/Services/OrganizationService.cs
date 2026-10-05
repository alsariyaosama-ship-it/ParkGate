using FluentValidation;
using ParkGate.Application.Common;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Ports.Audit;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.Organizations.Services;

public sealed class OrganizationService : IOrganizationService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IAuditWriter _auditWriter;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;
    private readonly IValidator<CreateOrganizationRequest> _createValidator;
    private readonly IValidator<UpdateOrganizationRequest> _updateValidator;

    public OrganizationService(
        IUnitOfWorkFactory uowFactory,
        IAuditWriter auditWriter,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        IValidator<CreateOrganizationRequest> createValidator,
        IValidator<UpdateOrganizationRequest> updateValidator)
    {
        _uowFactory = uowFactory;
        _auditWriter = auditWriter;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<OrganizationDto>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        await using var uow = _uowFactory.Create();
        var orgs = await uow.Organizations.GetAllAsync(activeOnly, cancellationToken);
        return orgs.Select(o => o.ToDto()).ToList();
    }

    public async Task<Result<OrganizationDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var uow = _uowFactory.Create();
        var org = await uow.Organizations.GetByIdAsync(id, cancellationToken);
        if (org is null)
        {
            return Errors.Organization.NotFound;
        }

        return org.ToDto();
    }

    public async Task<Result<OrganizationDto>> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageOrganizations))
        {
            return Errors.General.Forbidden;
        }

        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        await using var uow = _uowFactory.Create();

        var existingCode = await uow.Organizations.GetByCodeAsync(request.Code, cancellationToken);
        if (existingCode is not null)
        {
            return Errors.Organization.CodeAlreadyExists;
        }

        var existingName = await uow.Organizations.GetByNameAsync(request.NameAr, cancellationToken);
        if (existingName is not null)
        {
            return Errors.Organization.NameAlreadyExists;
        }

        var org = new Organization(
            request.Code,
            request.NameAr,
            request.CooldownMinutes,
            request.IsDefault,
            isActive: true);

        uow.Organizations.Add(org);
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "OrganizationCreated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(Organization),
            entityId: org.Id.ToString(),
            cancellationToken: cancellationToken);

        return org.ToDto();
    }

    public async Task<Result<OrganizationDto>> UpdateAsync(UpdateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageOrganizations))
        {
            return Errors.General.Forbidden;
        }

        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        await using var uow = _uowFactory.Create();
        var org = await uow.Organizations.GetByIdAsync(request.Id, cancellationToken);
        if (org is null)
        {
            return Errors.Organization.NotFound;
        }

        var existingCode = await uow.Organizations.GetByCodeAsync(request.Code, cancellationToken);
        if (existingCode is not null && existingCode.Id != org.Id)
        {
            return Errors.Organization.CodeAlreadyExists;
        }

        var existingName = await uow.Organizations.GetByNameAsync(request.NameAr, cancellationToken);
        if (existingName is not null && existingName.Id != org.Id)
        {
            return Errors.Organization.NameAlreadyExists;
        }

        org.Update(request.Code, request.NameAr, request.CooldownMinutes);

        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "OrganizationUpdated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(Organization),
            entityId: org.Id.ToString(),
            cancellationToken: cancellationToken);

        return org.ToDto();
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageOrganizations))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var org = await uow.Organizations.GetByIdAsync(id, cancellationToken);
        if (org is null)
        {
            return Errors.Organization.NotFound;
        }

        if (org.IsDefault)
        {
            return Errors.Organization.CannotDeactivateDefault;
        }

        org.Deactivate();
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "OrganizationDeactivated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(Organization),
            entityId: org.Id.ToString(),
            cancellationToken: cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageOrganizations))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var org = await uow.Organizations.GetByIdAsync(id, cancellationToken);
        if (org is null)
        {
            return Errors.Organization.NotFound;
        }

        org.Activate();
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "OrganizationReactivated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(Organization),
            entityId: org.Id.ToString(),
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
