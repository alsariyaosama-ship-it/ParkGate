using ParkGate.Application.Auth.Services;
using ParkGate.Application.Security;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Re-authentication dialog for supervisor override per SPEC §5.2 A5 and BR-11.
/// Prompts for supervisor credentials and mandatory reason (>= 5 chars).
/// </summary>
public partial class SupervisorAuthDialog : RtlFormBase
{
    private readonly IAuthService? _authService;
    private readonly ICurrentUser? _currentUser;

    public int SupervisorUserId { get; private set; }
    public string SupervisorFullName { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;

    public SupervisorAuthDialog() : this(null, null)
    {
    }

    public SupervisorAuthDialog(IAuthService? authService, ICurrentUser? currentUser)
    {
        InitializeComponent();

        _authService = authService;
        _currentUser = currentUser;

        InitializeFormText();
        WireEvents();
    }

    public void SetContext(string? defaultReason = null, bool prefillCurrentUserIfSupervisor = true)
    {
        if (prefillCurrentUserIfSupervisor && _currentUser != null &&
            (_currentUser.IsInRole(UserRole.Supervisor) || _currentUser.IsInRole(UserRole.Admin)))
        {
            _txtUsername.Text = _currentUser.UserName ?? string.Empty;
            _txtPassword.Focus();
        }
        else
        {
            _txtUsername.Focus();
        }

        if (!string.IsNullOrWhiteSpace(defaultReason))
        {
            _txtReason.Text = defaultReason;
        }
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_SupervisorAuth;
        _lblHeader.Text = Strings.Header_SupervisorAuth;
        _lblUsername.Text = Strings.Label_SupervisorUsername;
        _lblPassword.Text = Strings.Label_SupervisorPassword;
        _lblReason.Text = Strings.Label_SupervisorReason;
        _btnApprove.Text = Strings.Btn_Approve;
        _btnCancel.Text = Strings.Btn_Cancel;
    }

    private void WireEvents()
    {
        _btnApprove.Click += async (_, _) => await SubmitAsync();
        _btnCancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
    }

    public async Task SubmitAsync()
    {
        await OnApproveAsync();
    }

    private async Task OnApproveAsync()
    {
        var username = _txtUsername.Text.Trim();
        var password = _txtPassword.Text;
        var reason = _txtReason.Text.Trim();

        _lblError.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(username))
        {
            _lblError.Text = Strings.Val_UserNameRequired;
            _txtUsername.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            _lblError.Text = Strings.Val_PasswordRequired;
            _txtPassword.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            _lblError.Text = Strings.Val_SupervisorReasonRequired;
            _txtReason.Focus();
            return;
        }

        if (reason.Length < 5)
        {
            _lblError.Text = Strings.Val_SupervisorReasonTooShort;
            _txtReason.Focus();
            return;
        }

        if (_authService == null)
        {
            SupervisorUserId = 1;
            SupervisorFullName = username;
            Reason = reason;
            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        using (new BusyScope(this, _btnApprove, _btnCancel, _txtUsername, _txtPassword, _txtReason))
        {
            try
            {
                var result = await _authService.ValidateSupervisorCredentialsAsync(username, password);
                if (result.IsSuccess)
                {
                    SupervisorUserId = result.Value.Id;
                    SupervisorFullName = result.Value.FullName;
                    Reason = reason;

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    _lblError.Text = ErrorMessages.GetMessage(result.Error);
                    _txtPassword.Clear();
                    _txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }
    }
}
