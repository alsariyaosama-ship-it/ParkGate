using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.Common;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Services;
using ParkGate.Application.Ports.Audit;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Security;
using ParkGate.Application.Security;
using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Users.Services;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using Shouldly;

namespace ParkGate.Application.Tests.Services;

public sealed class InactiveEntityUniquenessTests
{
    private readonly IUnitOfWorkFactory _uowFactory = Substitute.For<IUnitOfWorkFactory>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly ICurrentWorkstation _currentWorkstation = Substitute.For<ICurrentWorkstation>();
    private readonly IAuditWriter _auditWriter = Substitute.For<IAuditWriter>();

    public InactiveEntityUniquenessTests()
    {
        _uowFactory.Create().Returns(_uow);
        _currentUser.Has(Arg.Any<Permission>()).Returns(true);
    }

    [Fact]
    public async Task CreateOrganization_WhenCodeExistsAndInactive_ReturnsCodeAlreadyExistsInactive()
    {
        var orgRepo = Substitute.For<IOrganizationRepository>();
        _uow.Organizations.Returns(orgRepo);

        var inactiveOrg = new Organization("OLD_ORG", "الجهة القديمة", isActive: false);
        orgRepo.GetByCodeAsync("OLD_ORG", Arg.Any<CancellationToken>()).Returns(inactiveOrg);

        var validator = Substitute.For<IValidator<CreateOrganizationRequest>>();
        validator.ValidateAsync(Arg.Any<CreateOrganizationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var service = new OrganizationService(
            _uowFactory,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            validator,
            Substitute.For<IValidator<UpdateOrganizationRequest>>());

        var result = await service.CreateAsync(new CreateOrganizationRequest("OLD_ORG", "اسم جديد", null, false));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Organization.CodeAlreadyExistsInactive.Code);
    }

    [Fact]
    public async Task CreateOrganization_WhenNameExistsAndInactive_ReturnsNameAlreadyExistsInactive()
    {
        var orgRepo = Substitute.For<IOrganizationRepository>();
        _uow.Organizations.Returns(orgRepo);

        var inactiveOrg = new Organization("NEW_CODE", "الجهة المعطلة", isActive: false);
        orgRepo.GetByCodeAsync("NEW_CODE", Arg.Any<CancellationToken>()).Returns((Organization?)null);
        orgRepo.GetByNameAsync("الجهة المعطلة", Arg.Any<CancellationToken>()).Returns(inactiveOrg);

        var validator = Substitute.For<IValidator<CreateOrganizationRequest>>();
        validator.ValidateAsync(Arg.Any<CreateOrganizationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var service = new OrganizationService(
            _uowFactory,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            validator,
            Substitute.For<IValidator<UpdateOrganizationRequest>>());

        var result = await service.CreateAsync(new CreateOrganizationRequest("NEW_CODE", "الجهة المعطلة", null, false));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Organization.NameAlreadyExistsInactive.Code);
    }

    [Fact]
    public async Task CreateUser_WhenUsernameExistsAndInactive_ReturnsUsernameAlreadyExistsInactive()
    {
        var userRepo = Substitute.For<IUserRepository>();
        _uow.Users.Returns(userRepo);

        var inactiveUser = new User("inactive.operator", "علي سالم", "hash123", UserRole.Operator, isActive: false);
        userRepo.GetByUserNameAsync("inactive.operator", Arg.Any<CancellationToken>()).Returns(inactiveUser);

        var validator = Substitute.For<IValidator<CreateUserRequest>>();
        validator.ValidateAsync(Arg.Any<CreateUserRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var service = new UserService(
            _uowFactory,
            Substitute.For<IPasswordHasher>(),
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            validator);

        var result = await service.CreateAsync(new CreateUserRequest("inactive.operator", "مستخدم جديد", "Pass@1234", UserRole.Operator));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.User.UsernameAlreadyExistsInactive.Code);
    }

    [Fact]
    public async Task CreateAgeCategory_WhenNameExistsAndInactive_ReturnsNameAlreadyExistsInactive()
    {
        var ageRepo = Substitute.For<IAgeCategoryRepository>();
        _uow.AgeCategories.Returns(ageRepo);

        var inactiveCat = new AgeCategory("فئة قديمة", "وصف", 1, isActive: false);
        ageRepo.GetByNameAsync("فئة قديمة", Arg.Any<CancellationToken>()).Returns(inactiveCat);

        var validator = Substitute.For<IValidator<CreateAgeCategoryRequest>>();
        validator.ValidateAsync(Arg.Any<CreateAgeCategoryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var service = new AgeCategoryService(
            _uowFactory,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            validator,
            Substitute.For<IValidator<UpdateAgeCategoryRequest>>());

        var result = await service.CreateAsync(new CreateAgeCategoryRequest("فئة قديمة", "وصف جديد", 2));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.AgeCategory.NameAlreadyExistsInactive.Code);
    }
}
