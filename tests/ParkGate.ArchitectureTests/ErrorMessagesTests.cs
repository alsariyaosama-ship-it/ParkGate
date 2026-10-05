using ParkGate.Application.Common;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;

namespace ParkGate.ArchitectureTests;

public sealed class ErrorMessagesTests
{
    [Theory]
    [InlineData("Auth.InvalidCredentials")]
    [InlineData("Auth.AccountLocked")]
    [InlineData("Auth.AccountInactive")]
    [InlineData("Auth.MustChangePassword")]
    [InlineData("Auth.PasswordMismatch")]
    [InlineData("Auth.FirstRunAlreadyCompleted")]
    [InlineData("Auth.WorkstationNotFound")]
    [InlineData("Auth.WorkstationInactive")]
    [InlineData("Auth.SupervisorRequired")]
    [InlineData("General.NotFound")]
    [InlineData("General.Unauthorized")]
    [InlineData("General.Forbidden")]
    [InlineData("General.ConcurrencyConflict")]
    public void GetMessage_ShouldReturnLocalizedArabicMessage_ForStandardErrorCodes(string errorCode)
    {
        var error = new Error(errorCode);

        var message = ErrorMessages.GetMessage(error);

        message.ShouldNotBeNullOrWhiteSpace();
        message.ShouldNotBe(errorCode);
    }

    [Fact]
    public void GetMessage_ShouldFormatValidationErrors()
    {
        var validationErrors = new List<ValidationError>
        {
            new("UserName", "UserNameRequired", "Username is required"),
            new("Password", "PasswordTooShort", "Password is too short")
        };
        var error = Errors.General.ValidationFailed(validationErrors);

        var message = ErrorMessages.GetMessage(error);

        message.ShouldContain(Strings.Val_UserNameRequired);
        message.ShouldContain(Strings.Val_PasswordTooShort);
    }

    [Fact]
    public void GetMessage_ShouldFallbackToErrorMessage_WhenCodeNotExplicitlyMapped()
    {
        var error = new Error("Custom.UnknownError", "Custom error detail");

        var message = ErrorMessages.GetMessage(error);

        message.ShouldBe("Custom error detail");
    }
}
