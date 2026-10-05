using FluentValidation;
using ParkGate.Application.Auth.DTOs;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.Auth.Validators;

public sealed class FirstRunAdminRequestValidator : AbstractValidator<FirstRunAdminRequest>
{
    public FirstRunAdminRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithErrorCode("UserNameRequired")
            .MaximumLength(User.MaxUserNameLength).WithErrorCode("UserNameTooLong");

        RuleFor(x => x.FullName)
            .NotEmpty().WithErrorCode("FullNameRequired")
            .MaximumLength(User.MaxFullNameLength).WithErrorCode("FullNameTooLong");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("PasswordRequired")
            .MinimumLength(8).WithErrorCode("PasswordTooShort");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithErrorCode("PasswordsDoNotMatch");
    }
}
