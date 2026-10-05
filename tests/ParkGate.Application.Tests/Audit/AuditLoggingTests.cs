using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.AgeCategories.Validators;
using ParkGate.Application.Auth.DTOs;
using ParkGate.Application.Auth.Services;
using ParkGate.Application.Auth.Validators;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.Beneficiaries.Validators;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.CheckIn.Services;
using ParkGate.Application.CheckIn.Validators;
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

namespace ParkGate.Application.Tests.Audit;

public sealed class AuditLoggingTests
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
    private readonly CurrentWorkstationContext _currentWorkstation = new("GATE-01");
    private readonly FakeUnitOfWorkFactory _uowFactory;

    public AuditLoggingTests()
    {
        var admin = new User("admin", "Administrator", _passwordHasher.HashPassword("AdminPass123"), UserRole.Admin);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(admin, 1);
        _users.Add(admin);

        var org = new Organization("SSF", "صندوق التضامن", cooldownMinutes: 720, isDefault: true, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(org, 1);
        _organizations.Add(org);

        var ws = new Workstation("GATE-01", "Gate Desk 1", WorkstationType.Desk, isActive: true);
        typeof(Workstation).GetProperty(nameof(Workstation.Id))!.SetValue(ws, 1);
        _workstations.Add(ws);

        var cat = new AgeCategory("بالغ", "13 سنة فأكثر", sortOrder: 1, isActive: true);
        typeof(AgeCategory).GetProperty(nameof(AgeCategory.Id))!.SetValue(cat, 1);
        _categories.Add(cat);

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
    public async Task AuthService_ShouldAuditFirstRun_Login_Lockout_AndWorkstationRegistration()
    {
        var authService = new AuthService(
            _uowFactory,
            _passwordHasher,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new FirstRunAdminRequestValidator(),
            new LoginRequestValidator(),
            new ChangePasswordRequestValidator());

        // Login success audit
        var loginResult = await authService.LoginAsync(new LoginRequest("admin", "AdminPass123"));
        loginResult.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "UserLogin" && w.UserId == 1);

        // Login failure audit
        var failedResult = await authService.LoginAsync(new LoginRequest("admin", "WrongPass"));
        failedResult.IsFailure.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "UserLoginFailed" && w.UserId == 1);

        // Auto-register workstation audit
        var wsResult = await authService.EnsureWorkstationRegisteredAsync("NEW-DESK-09");
        wsResult.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "WorkstationAutoRegistered");
    }

    [Fact]
    public async Task BeneficiaryAndCheckInService_ShouldAuditHolders_Entries_Overrides_AndVoids()
    {
        var benService = new BeneficiaryService(
            _uowFactory,
            new RollingWindowEligibilityPolicy(),
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new RegisterBeneficiaryRequestValidator(),
            new UpdateBeneficiaryRequestValidator(),
            new ChangeBeneficiaryStatusRequestValidator());

        // Register holder
        var reg = await benService.RegisterAsync(new RegisterBeneficiaryRequest(
            OrganizationId: 1,
            CardNumber: "12345",
            FullName: "محمد علي",
            Phone: "0912345678",
            NationalId: "119900123456",
            Notes: null,
            CardExpiryDate: null));

        reg.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "BeneficiaryRegistered" && w.EntityName == nameof(Beneficiary));

        // Update holder
        var update = await benService.UpdateAsync(new UpdateBeneficiaryRequest(
            Id: reg.Value.Id,
            OrganizationId: 1,
            CardNumber: "12345",
            FullName: "محمد علي أحمد",
            Phone: "0912345678",
            NationalId: "119900123456",
            Notes: "تعديل بيانات",
            CardExpiryDate: null));

        update.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "BeneficiaryUpdated" && w.EntityName == nameof(Beneficiary));

        // Change status (Suspend)
        var susp = await benService.ChangeStatusAsync(new ChangeBeneficiaryStatusRequest(
            Id: reg.Value.Id,
            TargetStatus: BeneficiaryStatus.Suspended,
            Reason: "بطاقة مفقودة"));

        susp.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "BeneficiaryStatusChanged");

        // Reactivate
        var react = await benService.ChangeStatusAsync(new ChangeBeneficiaryStatusRequest(
            Id: reg.Value.Id,
            TargetStatus: BeneficiaryStatus.Active,
            Reason: "استرجاع البطاقة"));

        react.IsSuccess.ShouldBeTrue();

        // Check-in granted
        var checkInService = new CheckInService(
            _uowFactory,
            new RollingWindowEligibilityPolicy(),
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new RecordEntryRequestValidator(),
            new OverrideEntryRequestValidator(),
            new VoidEntryRequestValidator());

        var entry = await checkInService.RecordEntryAsync(new RecordEntryRequest(
            BeneficiaryId: reg.Value.Id,
            Companions: [new CompanionRequestDto(1, 1)],
            Channel: EntryChannel.Desk));

        entry.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "EntryGranted" && w.EntityName == nameof(EntryAttempt));

        // Override entry (when cooldown active)
        var ovr = await checkInService.OverrideEntryAsync(new OverrideEntryRequest(
            BeneficiaryId: reg.Value.Id,
            Reason: "استثناء بأمر الإدارة",
            SupervisorUserId: 1,
            Companions: [],
            Channel: EntryChannel.Desk));

        ovr.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "EntryOverridden" && w.EntityName == nameof(EntryAttempt));

        // Void entry
        var voidRes = await checkInService.VoidEntryAsync(new VoidEntryRequest(
            AttemptId: entry.Value.AttemptId,
            Reason: "خطأ في تسجيل عدد المرافقين"));

        voidRes.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "EntryVoided" && w.EntityName == nameof(EntryAttempt));
    }

    [Fact]
    public async Task ManagementServices_ShouldAuditSettings_Orgs_Categories_Users_AndWorkstations()
    {
        // Settings updated
        var settingsService = new SettingsService(
            _settings,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new UpdateSettingsRequestValidator());

        var settings = await settingsService.GetSettingsAsync();
        var setRes = await settingsService.UpdateSettingsAsync(new UpdateSettingsRequest(
            settings.DefaultCooldownMinutes,
            settings.MaxGroupSize,
            settings.DenialDedupMinutes,
            settings.PrefillCompanionsFromLastVisit,
            settings.DenialSoundEnabled,
            settings.MaxFailedLogins,
            settings.LockoutMinutes,
            settings.AutoLockMinutes,
            "حديقة طرابلس",
            settings.TimeZoneId));

        setRes.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "SettingsUpdated");

        // Organization audit
        var orgService = new OrganizationService(
            _uowFactory,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new CreateOrganizationRequestValidator(),
            new UpdateOrganizationRequestValidator());

        var org = await orgService.CreateAsync(new CreateOrganizationRequest("MOH", "وزارة الصحة", 720));
        org.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "OrganizationCreated");

        var orgUpd = await orgService.UpdateAsync(new UpdateOrganizationRequest(org.Value.Id, "MOH", "وزارة الصحة الليبية", 360, false));
        orgUpd.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "OrganizationUpdated");

        var orgDeact = await orgService.DeactivateAsync(org.Value.Id);
        orgDeact.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "OrganizationDeactivated");

        var orgReact = await orgService.ReactivateAsync(org.Value.Id);
        orgReact.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "OrganizationReactivated");

        // AgeCategory audit
        var catService = new AgeCategoryService(
            _uowFactory,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new CreateAgeCategoryRequestValidator(),
            new UpdateAgeCategoryRequestValidator());

        var cat = await catService.CreateAsync(new CreateAgeCategoryRequest("طفل", "3 إلى 12", 2));
        cat.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "AgeCategoryCreated");

        var catUpd = await catService.UpdateAsync(new UpdateAgeCategoryRequest(cat.Value.Id, "طفل", "3 إلى 12 سنة", 2));
        catUpd.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "AgeCategoryUpdated");

        var catDeact = await catService.DeactivateAsync(cat.Value.Id);
        catDeact.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "AgeCategoryDeactivated");

        var catReact = await catService.ReactivateAsync(cat.Value.Id);
        catReact.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "AgeCategoryReactivated");

        // User audit
        var userService = new UserService(
            _uowFactory,
            _passwordHasher,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new CreateUserRequestValidator());

        var u = await userService.CreateAsync(new CreateUserRequest("supervisor1", "Supervisor One", "SuperPass99", UserRole.Supervisor));
        u.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "UserCreated");

        var uRole = await userService.UpdateRoleAsync(u.Value.Id, UserRole.Operator);
        uRole.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "UserRoleChanged");

        var uReset = await userService.ResetPasswordAsync(u.Value.Id, "NewTempPass8");
        uReset.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "UserPasswordReset");

        var uDeact = await userService.DeactivateAsync(u.Value.Id);
        uDeact.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "UserDeactivated");

        var uReact = await userService.ReactivateAsync(u.Value.Id);
        uReact.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "UserReactivated");

        // Workstation audit
        var wsService = new WorkstationService(
            _uowFactory,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new UpdateWorkstationRequestValidator());

        var wsUpd = await wsService.UpdateAsync(new UpdateWorkstationRequest(1, "Gate Desk 1 Renovated", WorkstationType.Desk));
        wsUpd.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "WorkstationUpdated");

        // Deactivate another workstation
        var ws2 = new Workstation("GATE-02", "Gate 2", WorkstationType.Desk, isActive: true);
        typeof(Workstation).GetProperty(nameof(Workstation.Id))!.SetValue(ws2, 2);
        _workstations.Add(ws2);

        var wsDeact = await wsService.DeactivateAsync(2);
        wsDeact.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "WorkstationDeactivated");

        var wsReact = await wsService.ReactivateAsync(2);
        wsReact.IsSuccess.ShouldBeTrue();
        _auditWriter.Writes.ShouldContain(w => w.Action == "WorkstationReactivated");
    }
}
