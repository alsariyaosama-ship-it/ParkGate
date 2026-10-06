using System.Reflection;
using System.Windows.Forms;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.Common;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Services;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Models;
using ParkGate.Application.Settings.Services;
using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Users.Services;
using ParkGate.Application.Workstations.DTOs;
using ParkGate.Application.Workstations.Services;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;
using Shouldly;

namespace ParkGate.ArchitectureTests;

public sealed class ManagementFormsTests
{
    private static TControl GetField<TControl>(object target, string fieldName) where TControl : class
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.ShouldNotBeNull($"Field '{fieldName}' was not found on '{target.GetType().Name}'.");
        var val = field.GetValue(target) as TControl;
        val.ShouldNotBeNull($"Field '{fieldName}' could not be cast to '{typeof(TControl).Name}'.");
        return val;
    }

    #region Organizations Tests

    [Fact]
    public void EditOrganizationDialog_ParameterlessCtor_ShouldInitializeRtl()
    {
        using var dialog = new EditOrganizationDialog();
        dialog.RightToLeft.ShouldBe(RightToLeft.Yes);
        dialog.Text.ShouldBe(Strings.Title_AddOrganization);
    }

    [Fact]
    public async Task EditOrganizationDialog_CreateAndSave_ShouldCallCreateAsync()
    {
        var fakeService = new FakeOrganizationService();
        var fakeNotify = new FakeNotifyService();

        using var dialog = new EditOrganizationDialog(fakeService, fakeNotify, null);

        var txtCode = GetField<TextBox>(dialog, "_txtCode");
        var txtName = GetField<TextBox>(dialog, "_txtNameAr");
        var chkCustomCooldown = GetField<CheckBox>(dialog, "_chkCustomCooldown");
        var numCooldown = GetField<NumericUpDown>(dialog, "_numCooldownHours");

        txtCode.Text = "ORG1";
        txtName.Text = "جهة التجربة الأولى";
        chkCustomCooldown.Checked = true;
        numCooldown.Value = 24;

        await dialog.SaveAsync();

        dialog.DialogResult.ShouldBe(DialogResult.OK);
        dialog.SavedOrganization.ShouldNotBeNull();
        dialog.SavedOrganization!.Code.ShouldBe("ORG1");
        dialog.SavedOrganization.CooldownMinutes.ShouldBe(1440);
        fakeService.Created.Count.ShouldBe(1);
    }

    [Fact]
    public async Task OrganizationsForm_LoadAndToggleActive_ShouldWork()
    {
        var org = new OrganizationDto(1, "TADAMUN", "صندوق التضامن", 720, true, true);
        var fakeService = new FakeOrganizationService([org]);
        var fakeNotify = new FakeNotifyService { NextConfirmResult = true };

        using var form = new OrganizationsForm(fakeService, null, fakeNotify, null);
        await form.LoadOrganizationsAsync();

        var grid = GetField<DataGridView>(form, "_gridOrganizations");
        grid.Rows.Count.ShouldBe(1);
        grid.Rows[0].Cells[0].Value.ShouldBe("TADAMUN");

        await form.ToggleActiveAsync();

        fakeService.DeactivatedIds.ShouldContain(1);
    }

    #endregion

    #region AgeCategories Tests

    [Fact]
    public void EditAgeCategoryDialog_ParameterlessCtor_ShouldInitializeRtl()
    {
        using var dialog = new EditAgeCategoryDialog();
        dialog.RightToLeft.ShouldBe(RightToLeft.Yes);
        dialog.Text.ShouldBe(Strings.Title_AddAgeCategory);
    }

    [Fact]
    public async Task EditAgeCategoryDialog_CreateAndSave_ShouldCallCreateAsync()
    {
        var fakeService = new FakeAgeCategoryService();
        var fakeNotify = new FakeNotifyService();

        using var dialog = new EditAgeCategoryDialog(fakeService, fakeNotify, null);

        var txtName = GetField<TextBox>(dialog, "_txtNameAr");
        var txtDesc = GetField<TextBox>(dialog, "_txtDescriptionAr");
        var numSort = GetField<NumericUpDown>(dialog, "_numSortOrder");

        txtName.Text = "شاب";
        txtDesc.Text = "من 13 إلى 18 سنة";
        numSort.Value = 2;

        await dialog.SaveAsync();

        dialog.DialogResult.ShouldBe(DialogResult.OK);
        dialog.SavedCategory.ShouldNotBeNull();
        dialog.SavedCategory!.NameAr.ShouldBe("شاب");
        fakeService.Created.Count.ShouldBe(1);
    }

    [Fact]
    public async Task AgeCategoriesForm_LoadAndToggleActive_ShouldWork()
    {
        var cat = new AgeCategoryDto(1, "بالغ", "أكبر من 18 سنة", 1, true);
        var fakeService = new FakeAgeCategoryService([cat]);
        var fakeNotify = new FakeNotifyService { NextConfirmResult = true };

        using var form = new AgeCategoriesForm(fakeService, null, fakeNotify, null);
        await form.LoadAgeCategoriesAsync();

        var grid = GetField<DataGridView>(form, "_gridAgeCategories");
        grid.Rows.Count.ShouldBe(1);
        grid.Rows[0].Cells[0].Value.ShouldBe("بالغ");

        await form.ToggleActiveAsync();

        fakeService.DeactivatedIds.ShouldContain(1);
    }

    #endregion

    #region Workstations Tests

    [Fact]
    public void EditWorkstationDialog_ParameterlessCtor_ShouldInitializeRtl()
    {
        using var dialog = new EditWorkstationDialog();
        dialog.RightToLeft.ShouldBe(RightToLeft.Yes);
        dialog.Text.ShouldBe(Strings.Title_EditWorkstation);
    }

    [Fact]
    public async Task EditWorkstationDialog_UpdateAndSave_ShouldCallUpdateAsync()
    {
        var ws = new WorkstationDto(1, "DESK-1", "شباك 1", WorkstationType.Desk, true, DateTime.UtcNow);
        var fakeService = new FakeWorkstationService([ws]);
        var fakeNotify = new FakeNotifyService();

        using var dialog = new EditWorkstationDialog(fakeService, fakeNotify, null);
        dialog.SetWorkstation(ws);

        var txtName = GetField<TextBox>(dialog, "_txtDisplayName");
        txtName.Text = "شباك المدخل الرئيسي";

        await dialog.SaveAsync();

        dialog.DialogResult.ShouldBe(DialogResult.OK);
        dialog.SavedWorkstation.ShouldNotBeNull();
        dialog.SavedWorkstation!.DisplayName.ShouldBe("شباك المدخل الرئيسي");
        fakeService.Updated.Count.ShouldBe(1);
    }

    [Fact]
    public async Task WorkstationsForm_LoadAndToggleActive_ShouldWork()
    {
        var ws = new WorkstationDto(1, "DESK-1", "شباك 1", WorkstationType.Desk, true, DateTime.UtcNow);
        var fakeService = new FakeWorkstationService([ws]);
        var fakeNotify = new FakeNotifyService { NextConfirmResult = true };

        using var form = new WorkstationsForm(fakeService, null, null, fakeNotify, null);
        await form.LoadWorkstationsAsync();

        var grid = GetField<DataGridView>(form, "_gridWorkstations");
        grid.Rows.Count.ShouldBe(1);
        grid.Rows[0].Cells[0].Value.ShouldBe("DESK-1");

        await form.ToggleActiveAsync();

        fakeService.DeactivatedIds.ShouldContain(1);
    }

    #endregion

    #region Users Tests

    [Fact]
    public void CreateUserDialog_ParameterlessCtor_ShouldInitializeRtl()
    {
        using var dialog = new CreateUserDialog();
        dialog.RightToLeft.ShouldBe(RightToLeft.Yes);
        dialog.Text.ShouldBe(Strings.Title_CreateUser);
    }

    [Fact]
    public async Task CreateUserDialog_ValidationAndCreate_ShouldWork()
    {
        var fakeService = new FakeUserService();
        var fakeNotify = new FakeNotifyService();

        using var dialog = new CreateUserDialog(fakeService, fakeNotify, null);

        var txtUser = GetField<TextBox>(dialog, "_txtUserName");
        var txtFull = GetField<TextBox>(dialog, "_txtFullName");
        var txtPass = GetField<TextBox>(dialog, "_txtPassword");
        var txtConfirm = GetField<TextBox>(dialog, "_txtConfirmPassword");

        txtUser.Text = "op"; // short
        txtFull.Text = "Operator One";
        txtPass.Text = "pass123";
        txtConfirm.Text = "pass123";

        await dialog.SaveAsync();
        dialog.DialogResult.ShouldBe(DialogResult.None);
        fakeNotify.Warnings.Count.ShouldBeGreaterThan(0);

        txtUser.Text = "operator1";
        txtPass.Text = "password";
        txtConfirm.Text = "different";

        await dialog.SaveAsync();
        dialog.DialogResult.ShouldBe(DialogResult.None);

        txtConfirm.Text = "password";
        await dialog.SaveAsync();

        dialog.DialogResult.ShouldBe(DialogResult.OK);
        dialog.CreatedUser.ShouldNotBeNull();
        dialog.CreatedUser!.UserName.ShouldBe("operator1");
        fakeService.Created.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ChangeUserRoleDialog_SetAndChangeRole_ShouldCallUpdateRoleAsync()
    {
        var user = new UserDto(1, "supervisor1", "Supervisor", UserRole.Supervisor, true, false, null);
        var fakeService = new FakeUserService([user]);
        var fakeNotify = new FakeNotifyService();

        using var dialog = new ChangeUserRoleDialog(fakeService, fakeNotify, null);
        dialog.SetUser(user);

        var cmbRole = GetField<ComboBox>(dialog, "_cmbRole");
        cmbRole.SelectedIndex = 2; // Admin

        await dialog.SaveAsync();

        dialog.DialogResult.ShouldBe(DialogResult.OK);
        dialog.UpdatedUser.ShouldNotBeNull();
        dialog.UpdatedUser!.Role.ShouldBe(UserRole.Admin);
    }

    [Fact]
    public async Task ResetUserPasswordDialog_ValidationAndReset_ShouldWork()
    {
        var user = new UserDto(1, "operator1", "Operator", UserRole.Operator, true, false, null);
        var fakeService = new FakeUserService([user]);
        var fakeNotify = new FakeNotifyService();

        using var dialog = new ResetUserPasswordDialog(fakeService, fakeNotify, null);
        dialog.SetUser(user);

        var txtPass = GetField<TextBox>(dialog, "_txtNewPassword");
        var txtConfirm = GetField<TextBox>(dialog, "_txtConfirmPassword");

        txtPass.Text = "123"; // short
        txtConfirm.Text = "123";
        await dialog.SaveAsync();
        dialog.DialogResult.ShouldBe(DialogResult.None);

        txtPass.Text = "NewTempPass123";
        txtConfirm.Text = "NewTempPass123";
        await dialog.SaveAsync();

        dialog.DialogResult.ShouldBe(DialogResult.OK);
        fakeService.ResetPasswords.ShouldContainKey(1);
    }

    [Fact]
    public async Task UsersForm_DeactivateSelf_ShouldBeBlocked()
    {
        var user = new UserDto(5, "admin", "Admin User", UserRole.Admin, true, false, null);
        var fakeService = new FakeUserService([user]);
        var fakeNotify = new FakeNotifyService();
        var currentUser = new FakeCurrentUser(5, "admin", "Admin User", UserRole.Admin);

        using var form = new UsersForm(fakeService, currentUser, null, null, fakeNotify, null);
        await form.LoadUsersAsync();

        await form.ToggleActiveAsync();

        fakeNotify.Warnings.ShouldContain(Strings.Msg_CannotDeactivateSelf);
        fakeService.DeactivatedIds.ShouldBeEmpty();
    }

    #endregion

    #region Settings Tests

    [Fact]
    public void SettingsForm_ParameterlessCtor_ShouldInitializeRtl()
    {
        using var form = new SettingsForm();
        form.RightToLeft.ShouldBe(RightToLeft.Yes);
        form.Text.ShouldBe(Strings.Title_Settings);
    }

    [Fact]
    public async Task SettingsForm_LoadAndSave_ShouldConvertHoursToMinutes()
    {
        var initialSettings = new AppSettingsModel(
            DefaultCooldownMinutes: 720,
            MaxGroupSize: 10,
            DenialDedupMinutes: 5,
            PrefillCompanionsFromLastVisit: true,
            DenialSoundEnabled: true,
            MaxFailedLogins: 5,
            LockoutMinutes: 15,
            AutoLockMinutes: 30,
            ParkName: "منتزه المرح",
            TimeZoneId: "Libya Standard Time");

        var fakeSettingsService = new FakeSettingsService(initialSettings);
        var fakeNotify = new FakeNotifyService();

        using var form = new SettingsForm(fakeSettingsService, fakeNotify, null);
        await form.LoadSettingsAsync();

        var numCooldown = GetField<NumericUpDown>(form, "_numCooldownHours");
        var txtParkName = GetField<TextBox>(form, "_txtParkName");

        numCooldown.Value.ShouldBe(12);
        txtParkName.Text.ShouldBe("منتزه المرح");

        // Change cooldown to 24 hours
        numCooldown.Value = 24;
        txtParkName.Text = "منتزه المرح العائلي الجديد";

        await form.SaveAsync();

        form.DialogResult.ShouldBe(DialogResult.OK);
        fakeSettingsService.Current.DefaultCooldownMinutes.ShouldBe(1440);
        fakeSettingsService.Current.ParkName.ShouldBe("منتزه المرح العائلي الجديد");
        fakeNotify.Infos.ShouldContain(Strings.Msg_SaveSettingsSuccess);
    }

    #endregion

    #region Fakes

    private sealed class FakeOrganizationService : IOrganizationService
    {
        private readonly List<OrganizationDto> _orgs;
        public List<CreateOrganizationRequest> Created { get; } = [];
        public List<int> DeactivatedIds { get; } = [];

        public FakeOrganizationService(IEnumerable<OrganizationDto>? orgs = null)
        {
            _orgs = orgs?.ToList() ?? [];
        }

        public Task<IReadOnlyList<OrganizationDto>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<OrganizationDto>>(_orgs);

        public Task<Result<OrganizationDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Result<OrganizationDto>> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default)
        {
            Created.Add(request);
            var created = new OrganizationDto(_orgs.Count + 1, request.Code, request.NameAr, request.CooldownMinutes, request.IsDefault, true);
            _orgs.Add(created);
            return Task.FromResult(Result<OrganizationDto>.Success(created));
        }

        public Task<Result<OrganizationDto>> UpdateAsync(UpdateOrganizationRequest request, CancellationToken cancellationToken = default)
        {
            var match = _orgs.First(o => o.Id == request.Id);
            var updated = match with
            {
                Code = request.Code,
                NameAr = request.NameAr,
                CooldownMinutes = request.CooldownMinutes,
                IsDefault = request.IsDefault
            };
            return Task.FromResult(Result<OrganizationDto>.Success(updated));
        }

        public Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
        {
            DeactivatedIds.Add(id);
            return Task.FromResult(Result.Success());
        }

        public Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    private sealed class FakeAgeCategoryService : IAgeCategoryService
    {
        private readonly List<AgeCategoryDto> _categories;
        public List<CreateAgeCategoryRequest> Created { get; } = [];
        public List<int> DeactivatedIds { get; } = [];

        public FakeAgeCategoryService(IEnumerable<AgeCategoryDto>? cats = null)
        {
            _categories = cats?.ToList() ?? [];
        }

        public Task<IReadOnlyList<AgeCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AgeCategoryDto>>(_categories);

        public Task<IReadOnlyList<AgeCategoryDto>> GetActiveAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AgeCategoryDto>>(_categories.Where(c => c.IsActive).ToList());

        public Task<Result<AgeCategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Result<AgeCategoryDto>> CreateAsync(CreateAgeCategoryRequest request, CancellationToken cancellationToken = default)
        {
            Created.Add(request);
            var created = new AgeCategoryDto(_categories.Count + 1, request.NameAr, request.DescriptionAr, request.SortOrder, true);
            _categories.Add(created);
            return Task.FromResult(Result<AgeCategoryDto>.Success(created));
        }

        public Task<Result<AgeCategoryDto>> UpdateAsync(UpdateAgeCategoryRequest request, CancellationToken cancellationToken = default)
        {
            var match = _categories.First(c => c.Id == request.Id);
            var updated = match with
            {
                NameAr = request.NameAr,
                DescriptionAr = request.DescriptionAr,
                SortOrder = request.SortOrder
            };
            return Task.FromResult(Result<AgeCategoryDto>.Success(updated));
        }

        public Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
        {
            DeactivatedIds.Add(id);
            return Task.FromResult(Result.Success());
        }

        public Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    private sealed class FakeWorkstationService : IWorkstationService
    {
        private readonly List<WorkstationDto> _workstations;
        public List<UpdateWorkstationRequest> Updated { get; } = [];
        public List<int> DeactivatedIds { get; } = [];

        public FakeWorkstationService(IEnumerable<WorkstationDto>? ws = null)
        {
            _workstations = ws?.ToList() ?? [];
        }

        public Task<IReadOnlyList<WorkstationDto>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<WorkstationDto>>(_workstations);

        public Task<Result<WorkstationDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Result<WorkstationDto>> UpdateAsync(UpdateWorkstationRequest request, CancellationToken cancellationToken = default)
        {
            Updated.Add(request);
            var match = _workstations.First(w => w.Id == request.Id);
            var updated = match with
            {
                DisplayName = request.DisplayName,
                Type = request.Type
            };
            return Task.FromResult(Result<WorkstationDto>.Success(updated));
        }

        public Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
        {
            DeactivatedIds.Add(id);
            return Task.FromResult(Result.Success());
        }

        public Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    private sealed class FakeUserService : IUserService
    {
        private readonly List<UserDto> _users;
        public List<CreateUserRequest> Created { get; } = [];
        public Dictionary<int, string> ResetPasswords { get; } = new();
        public List<int> DeactivatedIds { get; } = [];

        public FakeUserService(IEnumerable<UserDto>? users = null)
        {
            _users = users?.ToList() ?? [];
        }

        public Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserDto>>(_users);

        public Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            Created.Add(request);
            var created = new UserDto(_users.Count + 1, request.UserName, request.FullName, request.Role, true, false, null);
            _users.Add(created);
            return Task.FromResult(Result<UserDto>.Success(created));
        }

        public Task<Result<UserDto>> UpdateRoleAsync(int id, UserRole newRole, CancellationToken cancellationToken = default)
        {
            var match = _users.First(u => u.Id == id);
            var updated = match with { Role = newRole };
            return Task.FromResult(Result<UserDto>.Success(updated));
        }

        public Task<Result> ResetPasswordAsync(int id, string temporaryPassword, CancellationToken cancellationToken = default)
        {
            ResetPasswords[id] = temporaryPassword;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
        {
            DeactivatedIds.Add(id);
            return Task.FromResult(Result.Success());
        }

        public Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    private sealed class FakeSettingsService : ISettingsService
    {
        public AppSettingsModel Current { get; private set; }

        public FakeSettingsService(AppSettingsModel initial)
        {
            Current = initial;
        }

        public Task<AppSettingsModel> GetSettingsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Current);

        public Task<Result> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken cancellationToken = default)
        {
            Current = new AppSettingsModel(
                request.DefaultCooldownMinutes,
                request.MaxGroupSize,
                request.DenialDedupMinutes,
                request.PrefillCompanionsFromLastVisit,
                request.DenialSoundEnabled,
                request.MaxFailedLogins,
                request.LockoutMinutes,
                request.AutoLockMinutes,
                request.ParkName,
                request.TimeZoneId);

            return Task.FromResult(Result.Success());
        }

        public void InvalidateCache() { }
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public FakeCurrentUser(int userId, string userName, string fullName, UserRole role)
        {
            UserId = userId;
            UserName = userName;
            FullName = fullName;
            Role = role;
        }

        public int? UserId { get; }
        public string? UserName { get; }
        public string? FullName { get; }
        public UserRole? Role { get; }
        public bool IsAuthenticated => true;

        public bool Has(Permission permission) => RolePermissions.GetPermissions(Role ?? UserRole.Operator).Contains(permission);
        public bool HasAny(params Permission[] permissions) => permissions.Any(Has);
        public bool IsInRole(UserRole role) => Role == role;
    }

    private sealed class FakeNotifyService : INotifyService
    {
        public List<string> Infos { get; } = [];
        public List<string> Warnings { get; } = [];
        public List<string> Errors { get; } = [];
        public bool NextConfirmResult { get; set; } = true;

        public void Info(string message, string? title = null) => Infos.Add(message);
        public void Warning(string message, string? title = null) => Warnings.Add(message);
        public void Error(string message, string? title = null) => Errors.Add(message);
        public bool Confirm(string message, string? title = null) => NextConfirmResult;
    }

    #endregion
}
