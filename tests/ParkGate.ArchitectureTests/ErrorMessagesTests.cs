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
    [InlineData("CheckIn.ExceedsFreeCompanionLimit")]
    [InlineData("Gift.OrganizationInactive")]
    [InlineData("Gift.NotEnabled")]
    [InlineData("Gift.HolderBlocked")]
    [InlineData("Gift.HolderSuspended")]
    [InlineData("Gift.IntervalNotElapsed")]
    [InlineData("Gift.OverrideReasonRequired")]
    [InlineData("Gift.OverrideNotAllowed")]
    [InlineData("Gift.NotFound")]
    [InlineData("Gift.AlreadyRedeemed")]
    [InlineData("Gift.Voided")]
    [InlineData("Gift.Expired")]
    [InlineData("Gift.InvalidCode")]
    [InlineData("Gift.Forged")]
    [InlineData("Gift.CannotVoidRedeemed")]
    [InlineData("Gift.VoidReasonRequired")]
    [InlineData("Gift.InvalidPolicy")]
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
            new("Password", "PasswordTooShort", "Password is too short"),
            new("MaxFreeCompanions", "MaxFreeCompanionsOutOfRange", "Max free companions out of range"),
            new("GiftIntervalDays", "GiftIntervalOutOfRange", "Gift interval out of range"),
            new("GiftValue", "GiftValueRequired", "Gift value is required"),
            new("GiftValue", "GiftValueNegative", "Gift value cannot be negative")
        };
        var error = Errors.General.ValidationFailed(validationErrors);

        var message = ErrorMessages.GetMessage(error);

        message.ShouldContain(Strings.Val_UserNameRequired);
        message.ShouldContain(Strings.Val_PasswordTooShort);
        message.ShouldContain(Strings.Val_MaxFreeCompanionsOutOfRange);
        message.ShouldContain(Strings.Val_GiftIntervalOutOfRange);
        message.ShouldContain(Strings.Val_GiftValueRequired);
        message.ShouldContain(Strings.Val_GiftValueNegative);
    }

    [Fact]
    public void GetMessage_ShouldFallbackToErrorMessage_WhenCodeNotExplicitlyMapped()
    {
        var error = new Error("Custom.UnknownError", "Custom error detail");

        var message = ErrorMessages.GetMessage(error);

        message.ShouldBe("Custom error detail");
    }
}
