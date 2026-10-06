using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Users.Services;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Modal dialog for an administrator to reset a user's password (FR-ADM-04, BR-17).
/// </summary>
public partial class ResetUserPasswordDialog : RtlFormBase
{
    private readonly IUserService? _userService;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<ResetUserPasswordDialog>? _logger;

    private UserDto? _targetUser;

    public ResetUserPasswordDialog() : this(null, null, null)
    {
    }

    public ResetUserPasswordDialog(
        IUserService? userService,
        INotifyService? notifyService,
        ILogger<ResetUserPasswordDialog>? logger)
    {
        InitializeComponent();

        _userService = userService;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_ResetUserPassword;
        _lblHeader.Text = Strings.Title_ResetUserPassword;
        _lblNewPasswordPrompt.Text = Strings.Label_User_NewTempPassword;
        _lblConfirmPasswordPrompt.Text = Strings.Label_User_ConfirmPassword;
        _btnSave.Text = Strings.Btn_ResetPassword;
        _btnCancel.Text = Strings.Btn_Cancel;
    }

    private void WireEvents()
    {
        _btnSave.Click += async (_, _) => await SaveAsync();
    }

    public void SetUser(UserDto user)
    {
        _targetUser = user;
        _lblUserInfo.Text = $"{user.FullName} ({user.UserName})";
    }

    public async Task SaveAsync()
    {
        if (_userService == null || _targetUser == null)
        {
            return;
        }

        var newPassword = _txtNewPassword.Text;
        var confirmPassword = _txtConfirmPassword.Text;

        if (newPassword.Length < 6)
        {
            ShowWarning("كلمة المرور المؤقتة يجب ألا تقل عن 6 أحرف.");
            _txtNewPassword.Focus();
            return;
        }

        if (newPassword != confirmPassword)
        {
            ShowWarning("كلمتا المرور غير متطابقتين.");
            _txtConfirmPassword.Focus();
            return;
        }

        using (new BusyScope(this, _btnSave, _btnCancel))
        {
            try
            {
                var result = await _userService.ResetPasswordAsync(_targetUser.Id, newPassword);
                if (result.IsSuccess)
                {
                    _notifyService?.Info(Strings.Msg_PasswordResetSuccess);
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
                _logger?.LogError(ex, "Error resetting password.");
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
}
