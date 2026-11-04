using FluentValidation;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.Organizations.Validators;

public sealed class SetOrganizationPolicyRequestValidator : AbstractValidator<SetOrganizationPolicyRequest>
{
    public SetOrganizationPolicyRequestValidator()
    {
        RuleFor(x => x.MaxFreeCompanions)
            .InclusiveBetween(0, Organization.MaxFreeCompanionsLimit)
            .WithErrorCode("MaxFreeCompanionsOutOfRange")
            .When(x => x.MaxFreeCompanions.HasValue);

        RuleFor(x => x.GiftIntervalDays)
            .InclusiveBetween(Organization.MinGiftIntervalDays, Organization.MaxGiftIntervalDays)
            .WithErrorCode("GiftIntervalOutOfRange");

        RuleFor(x => x.GiftValue)
            .GreaterThan(0m)
            .WithErrorCode("GiftValueRequired")
            .When(x => x.GiftEnabled);

        RuleFor(x => x.GiftValue)
            .GreaterThanOrEqualTo(0m)
            .WithErrorCode("GiftValueNegative")
            .When(x => !x.GiftEnabled);
    }
}
