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
/// Modal dialog for creating a new system user (FR-ADM-04).
/// Admin only.
/// </summary>
public partial class CreateUserDialog : RtlFormBase
{
    private readonly IUserService? _userService;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<CreateUserDialog>? _logger;

    public UserDto? CreatedUser { get; private set; }

    public CreateUserDialog() : this(null, null, null)
    {
    }

    public CreateUserDialog(
        IUserService? userService,
        INotifyService? notifyService,
        ILogger<CreateUserDialog>? logger)
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
        Text = Strings.Title_CreateUser;
        _lblHeader.Text = Strings.Title_CreateUser;
        _lblUserName.Text = Strings.Label_User_Username;
        _lblFullName.Text = Strings.Label_User_FullName;
        _lblPassword.Text = Strings.Label_User_Password;
        _lblConfirmPassword.Text = Strings.Label_User_ConfirmPassword;
        _lblRole.Text = Strings.Label_User_Role;
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

    public async Task SaveAsync()
    {
        if (_userService == null)
        {
            return;
        }

        var userName = _txtUserName.Text.Trim();
        var fullName = _txtFullName.Text.Trim();
        var password = _txtPassword.Text;
        var confirmPassword = _txtConfirmPassword.Text;

        if (userName.Length < 3)
        {
            ShowWarning("اسم المستخدم مطلوب ويجب ألا يقل عن 3 أحرف.");
            _txtUserName.Focus();
            return;
        }

        if (fullName.Length < 3)
        {
            ShowWarning("الاسم الكامل مطلوب ويجب ألا يقل عن 3 أحرف.");
            _txtFullName.Focus();
            return;
        }

        if (password.Length < 6)
        {
            ShowWarning("كلمة المرور يجب ألا تقل عن 6 أحرف.");
            _txtPassword.Focus();
            return;
        }

        if (password != confirmPassword)
        {
            ShowWarning("كلمتا المرور غير متطابقتين.");
            _txtConfirmPassword.Focus();
            return;
        }

        var role = (_cmbRole.SelectedItem as ComboBoxItem<UserRole>)?.Value ?? UserRole.Operator;

        using (new BusyScope(this, _btnSave, _btnCancel))
        {
            try
            {
                var request = new CreateUserRequest(userName, fullName, password, role);
                var result = await _userService.CreateAsync(request);
                if (result.IsSuccess)
                {
                    CreatedUser = result.Value;
                    _notifyService?.Info(Strings.Msg_CreateUserSuccess);
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
                _logger?.LogError(ex, "Error creating user.");
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
