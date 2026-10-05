using FluentValidation;
using ParkGate.Application.Auth.DTOs;

namespace ParkGate.Application.Auth.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithErrorCode("UserNameRequired");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("PasswordRequired");
    }
}
