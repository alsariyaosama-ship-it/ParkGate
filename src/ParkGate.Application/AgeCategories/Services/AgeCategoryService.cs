using FluentValidation;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Audit;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.AgeCategories.Services;

public sealed class AgeCategoryService : IAgeCategoryService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IAuditWriter _auditWriter;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;
    private readonly IValidator<CreateAgeCategoryRequest> _createValidator;
    private readonly IValidator<UpdateAgeCategoryRequest> _updateValidator;

    public AgeCategoryService(
        IUnitOfWorkFactory uowFactory,
        IAuditWriter auditWriter,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        IValidator<CreateAgeCategoryRequest> createValidator,
        IValidator<UpdateAgeCategoryRequest> updateValidator)
    {
        _uowFactory = uowFactory;
        _auditWriter = auditWriter;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<AgeCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uowFactory.Create();
        var list = await uow.AgeCategories.GetAllAsync(cancellationToken);
        return list.Select(c => c.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<AgeCategoryDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uowFactory.Create();
        var list = await uow.AgeCategories.GetAllActiveAsync(cancellationToken);
        return list.Select(c => c.ToDto()).ToList();
    }

    public async Task<Result<AgeCategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var uow = _uowFactory.Create();
        var cat = await uow.AgeCategories.GetByIdAsync(id, cancellationToken);
        if (cat is null)
        {
            return Errors.AgeCategory.NotFound;
        }

        return cat.ToDto();
    }

    public async Task<Result<AgeCategoryDto>> CreateAsync(CreateAgeCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageAgeCategories))
        {
            return Errors.General.Forbidden;
        }

        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        await using var uow = _uowFactory.Create();
        var existing = await uow.AgeCategories.GetByNameAsync(request.NameAr, cancellationToken);
        if (existing is not null)
        {
            return Errors.AgeCategory.NameAlreadyExists;
        }

        var category = new AgeCategory(
            request.NameAr,
            request.DescriptionAr,
            request.SortOrder,
            isActive: true);

        uow.AgeCategories.Add(category);
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "AgeCategoryCreated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(AgeCategory),
            entityId: category.Id.ToString(),
            cancellationToken: cancellationToken);

        return category.ToDto();
    }

    public async Task<Result<AgeCategoryDto>> UpdateAsync(UpdateAgeCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageAgeCategories))
        {
            return Errors.General.Forbidden;
        }

        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        await using var uow = _uowFactory.Create();
        var category = await uow.AgeCategories.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
        {
            return Errors.AgeCategory.NotFound;
        }

        var existing = await uow.AgeCategories.GetByNameAsync(request.NameAr, cancellationToken);
        if (existing is not null && existing.Id != category.Id)
        {
            return Errors.AgeCategory.NameAlreadyExists;
        }

        category.Update(request.NameAr, request.DescriptionAr, request.SortOrder);
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "AgeCategoryUpdated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(AgeCategory),
            entityId: category.Id.ToString(),
            cancellationToken: cancellationToken);

        return category.ToDto();
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageAgeCategories))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var category = await uow.AgeCategories.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return Errors.AgeCategory.NotFound;
        }

        category.Deactivate();
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "AgeCategoryDeactivated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(AgeCategory),
            entityId: category.Id.ToString(),
            cancellationToken: cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ManageAgeCategories))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var category = await uow.AgeCategories.GetByIdAsync(id, cancellationToken);
        if (category is null)
        {
            return Errors.AgeCategory.NotFound;
        }

        category.Activate();
        await uow.SaveChangesAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "AgeCategoryReactivated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(AgeCategory),
            entityId: category.Id.ToString(),
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}
