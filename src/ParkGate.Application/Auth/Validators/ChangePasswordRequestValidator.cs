using FluentValidation;
using ParkGate.Application.Auth.DTOs;

namespace ParkGate.Application.Auth.Validators;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithErrorCode("CurrentPasswordRequired");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithErrorCode("NewPasswordRequired")
            .MinimumLength(8).WithErrorCode("PasswordTooShort");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithErrorCode("PasswordsDoNotMatch");
    }
}
