using FluentValidation;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.Organizations.Validators;

public sealed class UpdateOrganizationRequestValidator : AbstractValidator<UpdateOrganizationRequest>
{
    public UpdateOrganizationRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithErrorCode("IdRequired");

        RuleFor(x => x.Code)
            .NotEmpty().WithErrorCode("CodeRequired")
            .MaximumLength(Organization.MaxCodeLength).WithErrorCode("CodeTooLong");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithErrorCode("NameRequired")
            .MaximumLength(Organization.MaxNameLength).WithErrorCode("NameTooLong");

        RuleFor(x => x.CooldownMinutes)
            .InclusiveBetween(Organization.MinCooldownMinutes, Organization.MaxCooldownMinutes)
            .WithErrorCode("InvalidCooldownMinutes")
            .When(x => x.CooldownMinutes.HasValue);
    }
}
