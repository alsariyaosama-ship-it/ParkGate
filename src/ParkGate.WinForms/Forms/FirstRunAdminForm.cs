using ParkGate.Application.Auth.DTOs;
using ParkGate.Application.Auth.Services;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Forms;

public partial class FirstRunAdminForm : RtlFormBase
{
    private readonly IAuthService? _authService;

    public FirstRunAdminForm() : this(null)
    {
    }

    public FirstRunAdminForm(IAuthService? authService)
    {
        InitializeComponent();

        _authService = authService;

        InitializeFormText();
        txtUsername.Text = "admin";
        btnCreate.Click += async (_, _) => await OnCreateAdminAsync();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_FirstRunAdmin;
        lblHeader.Text = Strings.Header_FirstRunAdmin;
        lblFullName.Text = Strings.Label_AdminFullName;
        lblUsername.Text = Strings.Label_AdminUsername;
        lblPassword.Text = Strings.Label_AdminPassword;
        lblConfirmPassword.Text = Strings.Label_AdminConfirmPassword;
        btnCreate.Text = Strings.Btn_CreateAdmin;
        btnCancel.Text = Strings.Btn_Cancel;
    }

    private async Task OnCreateAdminAsync()
    {
        var fullName = txtFullName.Text.Trim();
        var username = txtUsername.Text.Trim();
        var password = txtPassword.Text;
        var confirmPassword = txtConfirmPassword.Text;

        if (_authService == null)
        {
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        SetBusy(true);
        try
        {
            var request = new FirstRunAdminRequest(username, fullName, password, confirmPassword);
            var result = await _authService.CompleteFirstRunAsync(request);

            if (result.IsSuccess)
            {
                Notify.Info(Strings.Msg_FirstRunAdminSuccess, Strings.Title_Info, this);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                Notify.Warning(ErrorMessages.GetMessage(result.Error), Strings.Title_Warning, this);
                txtPassword.Clear();
                txtConfirmPassword.Clear();
                txtPassword.Focus();
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
        btnCreate.Enabled = !busy;
        btnCancel.Enabled = !busy;
        txtFullName.Enabled = !busy;
        txtUsername.Enabled = !busy;
        txtPassword.Enabled = !busy;
        txtConfirmPassword.Enabled = !busy;
    }
}
