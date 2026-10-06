using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Users.Services;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Modal dialog for updating a user's role (FR-ADM-04).
/// Admin only.
/// </summary>
public partial class ChangeUserRoleDialog : RtlFormBase
{
    private readonly IUserService? _userService;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<ChangeUserRoleDialog>? _logger;

    private UserDto? _targetUser;

    public UserDto? UpdatedUser { get; private set; }

    public ChangeUserRoleDialog() : this(null, null, null)
    {
    }

    public ChangeUserRoleDialog(
        IUserService? userService,
        INotifyService? notifyService,
        ILogger<ChangeUserRoleDialog>? logger)
    {
        InitializeComponent();

        _userService = userService;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        PopulateRolesCombo();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_ChangeUserRole;
        _lblHeader.Text = Strings.Title_ChangeUserRole;
        _lblRolePrompt.Text = Strings.Label_User_Role;
        _btnSave.Text = Strings.Btn_Save;
        _btnCancel.Text = Strings.Btn_Cancel;
    }

    private void PopulateRolesCombo()
    {
        _cmbRole.Items.Clear();
        _cmbRole.Items.Add(new ComboBoxItem<UserRole>(UserRole.Operator, Strings.Role_Operator));
        _cmbRole.Items.Add(new ComboBoxItem<UserRole>(UserRole.Supervisor, Strings.Role_Supervisor));
        _cmbRole.Items.Add(new ComboBoxItem<UserRole>(UserRole.Admin, Strings.Role_Admin));
        _cmbRole.SelectedIndex = 0;
    }

    private void WireEvents()
    {
        _btnSave.Click += async (_, _) => await SaveAsync();
    }

    public void SetUser(UserDto user)
    {
        _targetUser = user;
        _lblUserInfo.Text = $"{user.FullName} ({user.UserName})";

        for (var i = 0; i < _cmbRole.Items.Count; i++)
        {
            if (_cmbRole.Items[i] is ComboBoxItem<UserRole> item && item.Value == user.Role)
            {
                _cmbRole.SelectedIndex = i;
                break;
            }
        }
    }

    public async Task SaveAsync()
    {
        if (_userService == null || _targetUser == null)
        {
            return;
        }

        var newRole = (_cmbRole.SelectedItem as ComboBoxItem<UserRole>)?.Value ?? UserRole.Operator;

        using (new BusyScope(this, _btnSave, _btnCancel))
        {
            try
            {
                var result = await _userService.UpdateRoleAsync(_targetUser.Id, newRole);
                if (result.IsSuccess)
                {
                    UpdatedUser = result.Value;
                    _notifyService?.Info(Strings.Msg_RoleChangeSuccess);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    ShowWarning(ErrorMessages.GetMessage(result.Error));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating user role.");
                ShowError(Strings.Msg_UnexpectedError + Environment.NewLine + ex.Message);
            }
        }
    }

    private void ShowWarning(string message)
    {
        if (_notifyService != null)
        {
            _notifyService.Warning(message);
        }
        else
        {
            MessageBox.Show(
                message,
                Strings.Title_Warning,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void ShowError(string message)
    {
        if (_notifyService != null)
        {
            _notifyService.Error(message);
        }
        else
        {
            MessageBox.Show(
                message,
                Strings.Title_Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private sealed class ComboBoxItem<T>
    {
        public T Value { get; }
        public string Text { get; }

        public ComboBoxItem(T value, string text)
        {
            Value = value;
            Text = text;
        }

        public override string ToString() => Text;
    }
}
