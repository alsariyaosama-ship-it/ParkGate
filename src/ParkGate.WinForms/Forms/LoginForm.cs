using ParkGate.Application.Auth.DTOs;
using ParkGate.Application.Auth.Services;
using ParkGate.Application.Security;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

public partial class LoginForm : RtlFormBase
{
    private readonly IAuthService? _authService;
    private readonly ICurrentWorkstation? _currentWorkstation;
    private readonly IFormFactory? _formFactory;

    public LoginForm() : this(null, null, null)
    {
    }

    public LoginForm(
        IAuthService? authService,
        ICurrentWorkstation? currentWorkstation,
        IFormFactory? formFactory)
    {
        InitializeComponent();

        _authService = authService;
        _currentWorkstation = currentWorkstation;
        _formFactory = formFactory;

        InitializeFormText();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_Login;
        lblTitle.Text = Strings.Header_Login;
        lblUsername.Text = Strings.Label_LoginUsername;
        lblPassword.Text = Strings.Label_LoginPassword;
        btnLogin.Text = Strings.Btn_Login;
        btnExit.Text = Strings.Btn_Exit;

        var wsName = _currentWorkstation?.MachineName ?? Environment.MachineName;
        lblWorkstation.Text = string.Format(Strings.Status_Workstation, wsName);
    }

    private void WireEvents()
    {
        btnLogin.Click += async (_, _) => await OnLoginAsync();
        btnExit.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
    }

    private async Task OnLoginAsync()
    {
        var username = txtUsername.Text.Trim();
        var password = txtPassword.Text;

        if (string.IsNullOrWhiteSpace(username))
        {
            lblError.Text = Strings.Val_UserNameRequired;
            txtUsername.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            lblError.Text = Strings.Val_PasswordRequired;
            txtPassword.Focus();
            return;
        }

        if (_authService == null)
        {
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        SetBusy(true);
        lblError.Text = string.Empty;

        try
        {
            var machineName = _currentWorkstation?.MachineName ?? Environment.MachineName;
            var request = new LoginRequest(username, password, machineName);
            var result = await _authService.LoginAsync(request);

            if (result.IsSuccess)
            {
                var loginResponse = result.Value;
                if (loginResponse.MustChangePassword)
                {
                    var changeForm = _formFactory?.Create<ChangePasswordForm>() ?? new ChangePasswordForm(_authService, null);
                    changeForm.SetContext(loginResponse.User.Id, isMandatoryChange: true);

                    var changeResult = changeForm.ShowDialog(this);
                    if (changeResult != DialogResult.OK)
                    {
                        lblError.Text = Strings.Error_Auth_MustChangePassword;
                        _authService.Logout();
                        txtPassword.Clear();
                        txtPassword.Focus();
                        return;
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblError.Text = ErrorMessages.GetMessage(result.Error);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }
        catch (Exception ex)
        {
            lblError.Text = ex.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        btnLogin.Enabled = !busy;
        btnExit.Enabled = !busy;
        txtUsername.Enabled = !busy;
        txtPassword.Enabled = !busy;
    }
}
