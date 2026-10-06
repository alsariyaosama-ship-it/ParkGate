using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Services;
using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Users.Services;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Management form for users (FR-ADM-04).
/// Admin only.
/// </summary>
public partial class UsersForm : RtlFormBase
{
    private readonly IUserService? _userService;
    private readonly ICurrentUser? _currentUser;
    private readonly ISettingsService? _settingsService;
    private readonly IFormFactory? _formFactory;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<UsersForm>? _logger;

    private readonly List<UserDto> _users = [];
    private UserDto? _selectedUser;
    private string _timeZoneId = "Libya Standard Time";

    public UsersForm() : this(null, null, null, null, null, null)
    {
    }

    public UsersForm(
        IUserService? userService,
        ICurrentUser? currentUser,
        ISettingsService? settingsService,
        IFormFactory? formFactory,
        INotifyService? notifyService,
        ILogger<UsersForm>? logger)
    {
        InitializeComponent();

        _userService = userService;
        _currentUser = currentUser;
        _settingsService = settingsService;
        _formFactory = formFactory;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_Users;
        _lblHeader.Text = Strings.Title_Users;
        _btnAddUser.Text = Strings.Title_CreateUser;
        _btnChangeRole.Text = Strings.Btn_ChangeRole;
        _btnResetPassword.Text = Strings.Btn_ResetPassword;
        _btnToggleActive.Text = Strings.Btn_Deactivate;
        _btnRefresh.Text = Strings.Btn_Refresh;
        _btnClose.Text = Strings.Btn_Close;

        _colUsername.HeaderText = Strings.Col_User_Username;
        _colFullName.HeaderText = Strings.Col_User_FullName;
        _colRole.HeaderText = Strings.Col_User_Role;
        _colStatus.HeaderText = Strings.Col_User_Status;
        _colMustChangePass.HeaderText = Strings.Col_User_MustChangePass;
        _colLastLogin.HeaderText = Strings.Col_User_LastLogin;
    }

    private void WireEvents()
    {
        Load += async (_, _) =>
        {
            if (_settingsService != null)
            {
                try
                {
                    var settings = await _settingsService.GetSettingsAsync();
                    _timeZoneId = settings.TimeZoneId;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to load time zone settings.");
                }
            }
            await LoadUsersAsync();
        };

        _btnAddUser.Click += async (_, _) => await AddUserAsync();
        _btnChangeRole.Click += async (_, _) => await ChangeRoleAsync();
        _btnResetPassword.Click += async (_, _) => await ResetPasswordAsync();
        _btnToggleActive.Click += async (_, _) => await ToggleActiveAsync();
        _btnRefresh.Click += async (_, _) => await LoadUsersAsync();
        _btnClose.Click += (_, _) => Close();

        _gridUsers.SelectionChanged += (_, _) => OnSelectionChanged();
        _gridUsers.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0)
            {
                await ChangeRoleAsync();
            }
        };
    }

    public async Task LoadUsersAsync()
    {
        if (_userService == null)
        {
            return;
        }

        try
        {
            var list = await _userService.GetAllAsync();

            _users.Clear();
            _users.AddRange(list);

            PopulateGrid(_users);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading users.");
            _notifyService?.Error(Strings.Msg_UnexpectedError);
        }
    }

    private void PopulateGrid(IReadOnlyList<UserDto> list)
    {
        _gridUsers.SuspendLayout();
        _gridUsers.Rows.Clear();

        foreach (var user in list)
        {
            var roleText = user.Role switch
            {
                UserRole.Operator => Strings.Role_Operator,
                UserRole.Supervisor => Strings.Role_Supervisor,
                UserRole.Admin => Strings.Role_Admin,
                _ => user.Role.ToString()
            };

            var statusText = user.IsActive ? Strings.Status_Active : Strings.Status_Inactive;
            var mustChangePassText = user.MustChangePassword ? "نعم" : "لا";

            var lastLoginText = user.LastLoginUtc.HasValue
                ? DateTimeDisplayHelper.FormatDateTime(DateTimeDisplayHelper.ToParkLocalTime(user.LastLoginUtc.Value, _timeZoneId))
                : "-";

            var rowIndex = _gridUsers.Rows.Add(
                user.UserName,
                user.FullName,
                roleText,
                statusText,
                mustChangePassText,
                lastLoginText);

            var row = _gridUsers.Rows[rowIndex];
            row.Tag = user;

            if (user.IsActive)
            {
                row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorGranted;
            }
            else
            {
                row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorDenied;
            }
        }

        _gridUsers.ResumeLayout(true);
        _lblCount.Text = $"عدد المستخدمين: {list.Count}";

        if (_gridUsers.Rows.Count > 0)
        {
            _gridUsers.Rows[0].Selected = true;
        }
        else
        {
            OnSelectionChanged();
        }
    }

    private void OnSelectionChanged()
    {
        if (_gridUsers.SelectedRows.Count == 0)
        {
            _selectedUser = null;
            _btnChangeRole.Enabled = false;
            _btnResetPassword.Enabled = false;
            _btnToggleActive.Enabled = false;
            return;
        }

        _selectedUser = _gridUsers.SelectedRows[0].Tag as UserDto;
        _btnChangeRole.Enabled = _selectedUser != null;
        _btnResetPassword.Enabled = _selectedUser != null;
        _btnToggleActive.Enabled = _selectedUser != null;

        if (_selectedUser != null)
        {
            _btnToggleActive.Text = _selectedUser.IsActive
                ? Strings.Btn_Deactivate
                : Strings.Btn_Reactivate;
        }
    }

    public async Task AddUserAsync()
    {
        var dialog = _formFactory?.Create<CreateUserDialog>() ??
                     new CreateUserDialog(_userService, _notifyService, null);

        using (dialog)
        {
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await LoadUsersAsync();
            }
        }
    }

    public async Task ChangeRoleAsync()
    {
        if (_selectedUser == null)
        {
            return;
        }

        if (_currentUser?.UserId.HasValue == true && _currentUser.UserId.Value == _selectedUser.Id)
        {
            _notifyService?.Warning(Strings.Msg_CannotChangeSelfRole);
            return;
        }

        var dialog = _formFactory?.Create<ChangeUserRoleDialog>() ??
                     new ChangeUserRoleDialog(_userService, _notifyService, null);

        using (dialog)
        {
            dialog.SetUser(_selectedUser);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await LoadUsersAsync();
            }
        }
    }

    public async Task ResetPasswordAsync()
    {
        if (_selectedUser == null)
        {
            return;
        }

        var dialog = _formFactory?.Create<ResetUserPasswordDialog>() ??
                     new ResetUserPasswordDialog(_userService, _notifyService, null);

        using (dialog)
        {
            dialog.SetUser(_selectedUser);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await LoadUsersAsync();
            }
        }
    }

    public async Task ToggleActiveAsync()
    {
        if (_selectedUser == null || _userService == null)
        {
            return;
        }

        if (_currentUser?.UserId.HasValue == true && _currentUser.UserId.Value == _selectedUser.Id)
        {
            _notifyService?.Warning(Strings.Msg_CannotDeactivateSelf);
            return;
        }

        var user = _selectedUser;
        var confirmMsg = user.IsActive
            ? Strings.Msg_ConfirmDeactivateUser
            : Strings.Msg_ConfirmReactivateUser;

        var confirmed = _notifyService?.Confirm(confirmMsg, Strings.Title_Confirm) ??
                        (MessageBox.Show(
                            confirmMsg,
                            Strings.Title_Confirm,
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign) == DialogResult.Yes);

        if (!confirmed)
        {
            return;
        }

        try
        {
            var result = user.IsActive
                ? await _userService.DeactivateAsync(user.Id)
                : await _userService.ReactivateAsync(user.Id);

            if (result.IsSuccess)
            {
                await LoadUsersAsync();
            }
            else
            {
                _notifyService?.Warning(ErrorMessages.GetMessage(result.Error));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error toggling user active state.");
            _notifyService?.Error(Strings.Msg_UnexpectedError);
        }
    }
}
