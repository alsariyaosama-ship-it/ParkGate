using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.AgeCategories.Validators;
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

namespace ParkGate.Application.Tests.Management;

public sealed class ManagementServicesTests
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
    private readonly CurrentUserContext _currentUser = new();
    private readonly CurrentWorkstationContext _currentWorkstation = new("ADMIN-PC");
    private readonly FakeUnitOfWorkFactory _uowFactory;

    public ManagementServicesTests()
    {
        var admin = new User("admin", "Administrator", "hash", UserRole.Admin);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(admin, 1);
        _users.Add(admin);

        var org = new Organization("SOC", "صندوق التضامن", cooldownMinutes: 720, isDefault: true, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(org, 1);
        _organizations.Add(org);

        var ws = new Workstation("ADMIN-PC", "Admin PC", WorkstationType.Desk);
        typeof(Workstation).GetProperty(nameof(Workstation.Id))!.SetValue(ws, 1);
        _workstations.Add(ws);

        _currentUser.SetUser(admin);
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
    public async Task SettingsService_ShouldGetDefaults_AndAllowAdminUpdate()
    {
        var sut = new SettingsService(
            _settings,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new UpdateSettingsRequestValidator());

        var initial = await sut.GetSettingsAsync();
        initial.DefaultCooldownMinutes.ShouldBe(720);
        initial.MaxGroupSize.ShouldBe(10);
        initial.ParkName.ShouldBe("صندوق التضامن الاجتماعي — متنزه العائلة");

        var updateReq = new UpdateSettingsRequest(
            DefaultCooldownMinutes: 480,
            MaxGroupSize: 15,
            DenialDedupMinutes: 10,
            PrefillCompanionsFromLastVisit: false,
            DenialSoundEnabled: true,
            MaxFailedLogins: 3,
            LockoutMinutes: 30,
            AutoLockMinutes: 5,
            ParkName: "New Park Name",
            TimeZoneId: "Libya Standard Time");

        var result = await sut.UpdateSettingsAsync(updateReq);
        result.IsSuccess.ShouldBeTrue();

        var updated = await sut.GetSettingsAsync();
        updated.DefaultCooldownMinutes.ShouldBe(480);
        updated.MaxGroupSize.ShouldBe(15);
        updated.ParkName.ShouldBe("New Park Name");

        _auditWriter.Writes.ShouldContain(w => w.Action == "SettingsUpdated");
    }

    [Fact]
    public async Task OrganizationService_ShouldPreventDeactivatingDefaultOrganization()
    {
        var sut = new OrganizationService(
            _uowFactory,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new CreateOrganizationRequestValidator(),
            new UpdateOrganizationRequestValidator());

        var result = await sut.DeactivateAsync(1);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Organization.CannotDeactivateDefault.Code);
    }

    [Fact]
    public async Task OrganizationService_CreateAndUpdate_ShouldWorkCorrectly()
    {
        var sut = new OrganizationService(
            _uowFactory,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new CreateOrganizationRequestValidator(),
            new UpdateOrganizationRequestValidator());

        var createResult = await sut.CreateAsync(new CreateOrganizationRequest("EDU", "وزارة التعليم", 360));
        createResult.IsSuccess.ShouldBeTrue();
        createResult.Value.Code.ShouldBe("EDU");

        var updateResult = await sut.UpdateAsync(new UpdateOrganizationRequest(createResult.Value.Id, "EDU", "وزارة التربية والتعليم", 480, false));
        updateResult.IsSuccess.ShouldBeTrue();
        updateResult.Value.NameAr.ShouldBe("وزارة التربية والتعليم");

        var deactResult = await sut.DeactivateAsync(createResult.Value.Id);
        deactResult.IsSuccess.ShouldBeTrue();

        var reactResult = await sut.ReactivateAsync(createResult.Value.Id);
        reactResult.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AgeCategoryService_ShouldCreateAndManageAgeCategories()
    {
        var sut = new AgeCategoryService(
            _uowFactory,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new CreateAgeCategoryRequestValidator(),
            new UpdateAgeCategoryRequestValidator());

        var createResult = await sut.CreateAsync(new CreateAgeCategoryRequest("رضيع", "أقل من سنتين", 1));
        createResult.IsSuccess.ShouldBeTrue();
        createResult.Value.NameAr.ShouldBe("رضيع");

        var all = await sut.GetAllAsync();
        all.Count.ShouldBe(1);

        var updateResult = await sut.UpdateAsync(new UpdateAgeCategoryRequest(createResult.Value.Id, "رضيع", "أقل من 3 سنوات", 1));
        updateResult.IsSuccess.ShouldBeTrue();
        updateResult.Value.DescriptionAr.ShouldBe("أقل من 3 سنوات");
    }

    [Fact]
    public async Task UserService_ShouldPreventDeactivatingSelf_AndSupportPasswordReset()
    {
        var sut = new UserService(
            _uowFactory,
            _passwordHasher,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new CreateUserRequestValidator());

        // Admin cannot deactivate self
        var selfDeact = await sut.DeactivateAsync(1);
        selfDeact.IsFailure.ShouldBeTrue();
        selfDeact.Error.Code.ShouldBe(Errors.User.CannotDeactivateSelf.Code);

        // Admin creates a new operator
        var createResult = await sut.CreateAsync(new CreateUserRequest("op2", "Operator Two", "StrongPass1", UserRole.Operator));
        createResult.IsSuccess.ShouldBeTrue();
        createResult.Value.MustChangePassword.ShouldBeTrue();

        // Admin resets operator password
        var resetResult = await sut.ResetPasswordAsync(createResult.Value.Id, "NewTempPass8");
        resetResult.IsSuccess.ShouldBeTrue();

        // Admin changes operator role to supervisor
        var roleResult = await sut.UpdateRoleAsync(createResult.Value.Id, UserRole.Supervisor);
        roleResult.IsSuccess.ShouldBeTrue();
        roleResult.Value.Role.ShouldBe(UserRole.Supervisor);
    }

    [Fact]
    public async Task WorkstationService_ShouldUpdateAndToggleActiveStatus()
    {
        var sut = new WorkstationService(
            _uowFactory,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new UpdateWorkstationRequestValidator());

        var updateResult = await sut.UpdateAsync(new UpdateWorkstationRequest(1, "Gate 1 Main", WorkstationType.Gate));
        updateResult.IsSuccess.ShouldBeTrue();
        updateResult.Value.DisplayName.ShouldBe("Gate 1 Main");
        updateResult.Value.Type.ShouldBe(WorkstationType.Gate);

        var deactResult = await sut.DeactivateAsync(1);
        deactResult.IsSuccess.ShouldBeTrue();

        var reactResult = await sut.ReactivateAsync(1);
        reactResult.IsSuccess.ShouldBeTrue();
    }
}
