using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.AgeCategories.Validators;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.Beneficiaries.Validators;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.CheckIn.Services;
using ParkGate.Application.CheckIn.Validators;
using ParkGate.Application.Common;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Services;
using ParkGate.Application.Organizations.Validators;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Models;
using ParkGate.Application.Settings.Services;
using ParkGate.Application.Settings.Validators;
using ParkGate.Application.Tests.Common;
using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Users.Services;
using ParkGate.Application.Users.Validators;
using ParkGate.Application.Workstations.DTOs;
using ParkGate.Application.Workstations.Services;
using ParkGate.Application.Workstations.Validators;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using Shouldly;
using Xunit;

namespace ParkGate.Application.Tests.Security;

public sealed class PermissionEnforcementTests
{
    private readonly List<User> _users = [];
    private readonly List<Workstation> _workstations = [];
    private readonly List<Organization> _organizations = [];
    private readonly List<Beneficiary> _beneficiaries = [];
    private readonly List<EntryAttempt> _attempts = [];
    private readonly List<AgeCategory> _categories = [];
    private readonly FakeAppSettingRepository _settings = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeAuditWriter _auditWriter = new();
    private readonly CurrentUserContext _operatorUser = new();
    private readonly CurrentWorkstationContext _currentWorkstation = new("GATE-01");
    private readonly FakeUnitOfWorkFactory _uowFactory;

    public PermissionEnforcementTests()
    {
        var op = new User("operator1", "Desk Operator", "hash", UserRole.Operator);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(op, 2);
        _users.Add(op);

        var ws = new Workstation("GATE-01", "Gate Desk", WorkstationType.Desk, isActive: true);
        typeof(Workstation).GetProperty(nameof(Workstation.Id))!.SetValue(ws, 1);
        _workstations.Add(ws);

        var org = new Organization("SSF", "صندوق التضامن", cooldownMinutes: 720, isDefault: true, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(org, 1);
        _organizations.Add(org);

        _operatorUser.SetUser(op);
        _currentWorkstation.SetWorkstation(ws);

        _uowFactory = new FakeUnitOfWorkFactory(
            _users,
            _workstations,
            _settings,
            _organizations,
            _beneficiaries,
            _attempts,
            _categories);
    }

    [Fact]
    public async Task UserService_WhenOperatorAttemptsUserManagement_ShouldReturnForbidden()
    {
        var sut = new UserService(
            _uowFactory,
            _passwordHasher,
            _auditWriter,
            _operatorUser,
            _currentWorkstation,
            new CreateUserRequestValidator());

        var createRes = await sut.CreateAsync(new CreateUserRequest("newuser", "Name", "Pass12345", UserRole.Operator));
        createRes.IsFailure.ShouldBeTrue();
        createRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);

        var roleRes = await sut.UpdateRoleAsync(2, UserRole.Supervisor);
        roleRes.IsFailure.ShouldBeTrue();
        roleRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);

        var resetRes = await sut.ResetPasswordAsync(2, "NewTempPass8");
        resetRes.IsFailure.ShouldBeTrue();
        resetRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);

        var deactRes = await sut.DeactivateAsync(2);
        deactRes.IsFailure.ShouldBeTrue();
        deactRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);

        var reactRes = await sut.ReactivateAsync(2);
        reactRes.IsFailure.ShouldBeTrue();
        reactRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
    }

    [Fact]
    public async Task OrganizationService_WhenOperatorAttemptsOrgManagement_ShouldReturnForbidden()
    {
        var sut = new OrganizationService(
            _uowFactory,
            _auditWriter,
            _operatorUser,
            _currentWorkstation,
            new CreateOrganizationRequestValidator(),
            new UpdateOrganizationRequestValidator());

        var createRes = await sut.CreateAsync(new CreateOrganizationRequest("NEW", "منظمة جديدة", 360));
        createRes.IsFailure.ShouldBeTrue();
        createRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);

        var updateRes = await sut.UpdateAsync(new UpdateOrganizationRequest(1, "NEW", "تعديل", 360, false));
        updateRes.IsFailure.ShouldBeTrue();
        updateRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);

        var deactRes = await sut.DeactivateAsync(1);
        deactRes.IsFailure.ShouldBeTrue();
        deactRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);

        var reactRes = await sut.ReactivateAsync(1);
        reactRes.IsFailure.ShouldBeTrue();
        reactRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
    }

    [Fact]
    public async Task AgeCategoryService_WhenOperatorAttemptsAgeCategoryManagement_ShouldReturnForbidden()
    {
        var sut = new AgeCategoryService(
            _uowFactory,
            _auditWriter,
            _operatorUser,
            _currentWorkstation,
            new CreateAgeCategoryRequestValidator(),
            new UpdateAgeCategoryRequestValidator());

        var createRes = await sut.CreateAsync(new CreateAgeCategoryRequest("فئة", "وصف", 1));
        createRes.IsFailure.ShouldBeTrue();
        createRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);

        var updateRes = await sut.UpdateAsync(new UpdateAgeCategoryRequest(1, "فئة", "وصف جديد", 1));
        updateRes.IsFailure.ShouldBeTrue();
        updateRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);

        var deactRes = await sut.DeactivateAsync(1);
        deactRes.IsFailure.ShouldBeTrue();
        deactRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);

        var reactRes = await sut.ReactivateAsync(1);
        reactRes.IsFailure.ShouldBeTrue();
        reactRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
    }

    [Fact]
    public async Task WorkstationService_WhenOperatorAttemptsWorkstationManagement_ShouldReturnForbidden()
    {
        var sut = new WorkstationService(
            _uowFactory,
            _auditWriter,
            _operatorUser,
            _currentWorkstation,
            new UpdateWorkstationRequestValidator());

        var updRes = await sut.UpdateAsync(new UpdateWorkstationRequest(1, "اسم جديد", WorkstationType.Desk));
        updRes.IsFailure.ShouldBeTrue();
        updRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);

        var deactRes = await sut.DeactivateAsync(1);
        deactRes.IsFailure.ShouldBeTrue();
        deactRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);

        var reactRes = await sut.ReactivateAsync(1);
        reactRes.IsFailure.ShouldBeTrue();
        reactRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
    }

    [Fact]
    public async Task SettingsService_WhenOperatorAttemptsUpdate_ShouldReturnForbidden()
    {
        var sut = new SettingsService(
            _settings,
            _auditWriter,
            _operatorUser,
            _currentWorkstation,
            new UpdateSettingsRequestValidator());

        var settings = await sut.GetSettingsAsync();
        var updRes = await sut.UpdateSettingsAsync(new UpdateSettingsRequest(
            settings.DefaultCooldownMinutes,
            settings.MaxGroupSize,
            settings.DenialDedupMinutes,
            settings.PrefillCompanionsFromLastVisit,
            settings.DenialSoundEnabled,
            settings.MaxFailedLogins,
            settings.LockoutMinutes,
            settings.AutoLockMinutes,
            "اسم جديد",
            settings.TimeZoneId));

        updRes.IsFailure.ShouldBeTrue();
        updRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
    }

    [Fact]
    public async Task CheckInService_WhenOperatorAttemptsVoid_ShouldReturnForbidden()
    {
        var sut = new CheckInService(
            _uowFactory,
            new RollingWindowEligibilityPolicy(),
            _auditWriter,
            _operatorUser,
            _currentWorkstation,
            new RecordEntryRequestValidator(),
            new OverrideEntryRequestValidator(),
            new VoidEntryRequestValidator());

        var voidRes = await sut.VoidEntryAsync(new VoidEntryRequest(Guid.NewGuid(), "سبب الإلغاء"));
        voidRes.IsFailure.ShouldBeTrue();
        voidRes.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
    }
}
