using System.Reflection;
using System.Windows.Forms;
using ParkGate.Application.Auth.DTOs;
using ParkGate.Application.Auth.Services;
using ParkGate.Application.Common;
using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Workstations.DTOs;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;
using Shouldly;

namespace ParkGate.ArchitectureTests;

public sealed class SupervisorAuthDialogTests
{
    private static TControl GetField<TControl>(object target, string fieldName) where TControl : class
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.ShouldNotBeNull($"Field '{fieldName}' was not found on '{target.GetType().Name}'.");
        var val = field.GetValue(target) as TControl;
        val.ShouldNotBeNull($"Field '{fieldName}' could not be cast to '{typeof(TControl).Name}'.");
        return val;
    }

    [Fact]
    public void SupervisorAuthDialog_InitialState_ShouldHaveArabicLabelsAndEmptyError()
    {
        using var form = new SupervisorAuthDialog();

        form.Text.ShouldBe(Strings.Title_SupervisorAuth);

        var lblHeader = GetField<Label>(form, "_lblHeader");
        var lblUsername = GetField<Label>(form, "_lblUsername");
        var lblPassword = GetField<Label>(form, "_lblPassword");
        var lblReason = GetField<Label>(form, "_lblReason");
        var btnApprove = GetField<Button>(form, "_btnApprove");
        var btnCancel = GetField<Button>(form, "_btnCancel");
        var lblError = GetField<Label>(form, "_lblError");

        lblHeader.Text.ShouldBe(Strings.Header_SupervisorAuth);
        lblUsername.Text.ShouldBe(Strings.Label_SupervisorUsername);
        lblPassword.Text.ShouldBe(Strings.Label_SupervisorPassword);
        lblReason.Text.ShouldBe(Strings.Label_SupervisorReason);
        btnApprove.Text.ShouldBe(Strings.Btn_Approve);
        btnCancel.Text.ShouldBe(Strings.Btn_Cancel);
        lblError.Text.ShouldBeEmpty();
    }

    [Fact]
    public async Task SupervisorAuthDialog_Validation_RequiresUsername()
    {
        var authService = new TestAuthService();
        using var form = new SupervisorAuthDialog(authService, null);

        var lblError = GetField<Label>(form, "_lblError");

        await form.SubmitAsync();

        lblError.Text.ShouldBe(Strings.Val_UserNameRequired);
        authService.ValidateCredentialsCallCount.ShouldBe(0);
    }

    [Fact]
    public async Task SupervisorAuthDialog_Validation_RequiresPassword()
    {
        var authService = new TestAuthService();
        using var form = new SupervisorAuthDialog(authService, null);

        var txtUsername = GetField<TextBox>(form, "_txtUsername");
        var lblError = GetField<Label>(form, "_lblError");

        txtUsername.Text = "supervisor1";
        await form.SubmitAsync();

        lblError.Text.ShouldBe(Strings.Val_PasswordRequired);
        authService.ValidateCredentialsCallCount.ShouldBe(0);
    }

    [Fact]
    public async Task SupervisorAuthDialog_Validation_RequiresReasonAtLeast5Chars()
    {
        var authService = new TestAuthService();
        using var form = new SupervisorAuthDialog(authService, null);

        var txtUsername = GetField<TextBox>(form, "_txtUsername");
        var txtPassword = GetField<TextBox>(form, "_txtPassword");
        var txtReason = GetField<TextBox>(form, "_txtReason");
        var lblError = GetField<Label>(form, "_lblError");

        txtUsername.Text = "supervisor1";
        txtPassword.Text = "pass1234";

        // Empty reason
        txtReason.Text = "";
        await form.SubmitAsync();
        lblError.Text.ShouldBe(Strings.Val_SupervisorReasonRequired);

        // Reason too short (< 5 chars)
        txtReason.Text = "علة";
        await form.SubmitAsync();
        lblError.Text.ShouldBe(Strings.Val_SupervisorReasonTooShort);

        authService.ValidateCredentialsCallCount.ShouldBe(0);
    }

    [Fact]
    public async Task SupervisorAuthDialog_SuccessfulValidation_SetsOutputProperties()
    {
        var authService = new TestAuthService
        {
            ValidateResult = Result<UserDto>.Success(new UserDto(
                5,
                "super",
                "المشرف كمال",
                UserRole.Supervisor,
                true,
                false,
                null))
        };

        using var form = new SupervisorAuthDialog(authService, null);

        var txtUsername = GetField<TextBox>(form, "_txtUsername");
        var txtPassword = GetField<TextBox>(form, "_txtPassword");
        var txtReason = GetField<TextBox>(form, "_txtReason");

        txtUsername.Text = "super";
        txtPassword.Text = "superpass";
        txtReason.Text = "تجاوز استثنائي بموافقة الإدارة";

        await form.SubmitAsync();

        form.DialogResult.ShouldBe(DialogResult.OK);
        form.SupervisorUserId.ShouldBe(5);
        form.SupervisorFullName.ShouldBe("المشرف كمال");
        form.Reason.ShouldBe("تجاوز استثنائي بموافقة الإدارة");
        authService.ValidateCredentialsCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task SupervisorAuthDialog_InvalidCredentials_DisplaysErrorMessage()
    {
        var authService = new TestAuthService
        {
            ValidateResult = Result<UserDto>.Failure(Errors.Auth.InvalidCredentials)
        };

        using var form = new SupervisorAuthDialog(authService, null);

        var txtUsername = GetField<TextBox>(form, "_txtUsername");
        var txtPassword = GetField<TextBox>(form, "_txtPassword");
        var txtReason = GetField<TextBox>(form, "_txtReason");
        var lblError = GetField<Label>(form, "_lblError");

        txtUsername.Text = "super";
        txtPassword.Text = "wrongpass";
        txtReason.Text = "تجاوز استثنائي بموافقة الإدارة";

        await form.SubmitAsync();

        lblError.Text.ShouldBe(Strings.Error_Auth_InvalidCredentials);
        txtPassword.Text.ShouldBeEmpty();
        authService.ValidateCredentialsCallCount.ShouldBe(1);
    }

    private sealed class TestAuthService : IAuthService
    {
        public int ValidateCredentialsCallCount { get; private set; }
        public Result<UserDto> ValidateResult { get; set; } =
            Result<UserDto>.Success(new UserDto(1, "admin", "Admin", UserRole.Admin, true, false, null));

        public Task<Result<UserDto>> ValidateSupervisorCredentialsAsync(string userName, string password, CancellationToken cancellationToken = default)
        {
            ValidateCredentialsCallCount++;
            return Task.FromResult(ValidateResult);
        }

        public Task<bool> IsFirstRunAsync(CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<Result<UserDto>> CompleteFirstRunAsync(FirstRunAdminRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(ValidateResult);
        public Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
        public Task<Result> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success());
        public Task<Result<WorkstationDto>> EnsureWorkstationRegisteredAsync(string? machineName = null, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
        public void Logout() { }
    }
}
