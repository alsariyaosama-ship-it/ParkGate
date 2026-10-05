using ParkGate.Application.Auth.DTOs;
using ParkGate.Application.Auth.Services;
using ParkGate.Application.Security;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Forms;

public partial class ChangePasswordForm : RtlFormBase
{
    private readonly IAuthService? _authService;
    private readonly ICurrentUser? _currentUser;
    private int? _targetUserId;

    public ChangePasswordForm() : this(null, null)
    {
    }

    public ChangePasswordForm(IAuthService? authService, ICurrentUser? currentUser)
    {
        InitializeComponent();

        _authService = authService;
        _currentUser = currentUser;

        InitializeFormText();
        btnChange.Click += async (_, _) => await OnChangePasswordAsync();
    }

    public void SetContext(int userId, bool isMandatoryChange)
    {
        _targetUserId = userId;
        if (isMandatoryChange)
        {
            lblHeader.Text = Strings.Header_MustChangePassword;
            btnCancel.Visible = false; // Cannot cancel mandatory change
            ControlBox = false;
        }
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_ChangePassword;
        lblHeader.Text = Strings.Title_ChangePassword;
        lblCurrentPassword.Text = Strings.Label_CurrentPassword;
        lblNewPassword.Text = Strings.Label_NewPassword;
        lblConfirmNewPassword.Text = Strings.Label_ConfirmNewPassword;
        btnChange.Text = Strings.Btn_ChangePassword;
        btnCancel.Text = Strings.Btn_Cancel;
    }

    private async Task OnChangePasswordAsync()
    {
        var currentPassword = txtCurrentPassword.Text;
        var newPassword = txtNewPassword.Text;
        var confirmNewPassword = txtConfirmNewPassword.Text;

        var userId = _targetUserId ?? _currentUser?.UserId;
        if (!userId.HasValue || _authService == null)
        {
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        SetBusy(true);
        try
        {
            var request = new ChangePasswordRequest(userId.Value, currentPassword, newPassword, confirmNewPassword);
            var result = await _authService.ChangePasswordAsync(request);

            if (result.IsSuccess)
            {
                Notify.Info(Strings.Msg_PasswordChangedSuccess, Strings.Title_Info, this);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                Notify.Warning(ErrorMessages.GetMessage(result.Error), Strings.Title_Warning, this);
                txtCurrentPassword.Clear();
                txtNewPassword.Clear();
                txtConfirmNewPassword.Clear();
                txtCurrentPassword.Focus();
            }
        }
        catch (Exception ex)
        {
            Notify.Error(ex.Message, Strings.Title_Error, this);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        btnChange.Enabled = !busy;
        btnCancel.Enabled = !busy;
        txtCurrentPassword.Enabled = !busy;
        txtNewPassword.Enabled = !busy;
        txtConfirmNewPassword.Enabled = !busy;
    }
}
