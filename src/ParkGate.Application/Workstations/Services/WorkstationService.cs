using FluentValidation;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Audit;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Application.Workstations.DTOs;

namespace ParkGate.Application.Workstations.Services;

public sealed class WorkstationService : IWorkstationService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IAuditWriter _auditWriter;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;
    private readonly IValidator<UpdateWorkstationRequest> _updateValidator;

    public WorkstationService(
        IUnitOfWorkFactory uowFactory,
        IAuditWriter auditWriter,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        IValidator<UpdateWorkstationRequest> updateValidator)
    {
        _uowFactory = uowFactory;
        _auditWriter = auditWriter;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<WorkstationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uowFactory.Create();
        var workstations = await uow.Workstations.GetAllAsync(cancellationToken);
        return workstations.Select(w => w.ToDto()).ToList();
    }

    public async Task<Result<WorkstationDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var uow = _uowFactory.Create();
        var workstation = await uow.Workstations.GetByIdAsync(id, cancellationToken);
        if (workstation is null)
        {
            return Errors.Workstation.NotFound;
        }

        return workstation.ToDto();
    }

    public async Task<Result<WorkstationDto>> UpdateAsync(UpdateWorkstationRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageWorkstations))
        {
            return Errors.General.Forbidden;
        }

        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        await using var uow = _uowFactory.Create();
        var workstation = await uow.Workstations.GetByIdAsync(request.Id, cancellationToken);
        if (workstation is null)
        {
            return Errors.Workstation.NotFound;
        }

        workstation.Update(request.DisplayName, request.Type);
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "WorkstationUpdated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(Domain.Entities.Workstation),
            entityId: workstation.Id.ToString(),
            detailsJson: $"{{\"DisplayName\":\"{workstation.DisplayName}\",\"Type\":\"{workstation.Type}\"}}",
            cancellationToken: cancellationToken);

        return workstation.ToDto();
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageWorkstations))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var workstation = await uow.Workstations.GetByIdAsync(id, cancellationToken);
        if (workstation is null)
        {
            return Errors.Workstation.NotFound;
        }

        workstation.Deactivate();
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "WorkstationDeactivated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(Domain.Entities.Workstation),
            entityId: workstation.Id.ToString(),
            cancellationToken: cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageWorkstations))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var workstation = await uow.Workstations.GetByIdAsync(id, cancellationToken);
        if (workstation is null)
        {
            return Errors.Workstation.NotFound;
        }

        workstation.Activate();
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "WorkstationReactivated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(Domain.Entities.Workstation),
            entityId: workstation.Id.ToString(),
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
