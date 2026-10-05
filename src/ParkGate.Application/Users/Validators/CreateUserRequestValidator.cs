using FluentValidation;
using ParkGate.Application.Users.DTOs;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.Users.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
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

        RuleFor(x => x.Role)
            .IsInEnum().WithErrorCode("InvalidRole");
    }
}
