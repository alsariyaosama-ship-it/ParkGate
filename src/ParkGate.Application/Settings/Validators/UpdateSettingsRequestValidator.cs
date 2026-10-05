using FluentValidation;
using ParkGate.Application.Settings.Models;

namespace ParkGate.Application.Settings.Validators;

public sealed class UpdateSettingsRequestValidator : AbstractValidator<UpdateSettingsRequest>
{
    public UpdateSettingsRequestValidator()
    {
        RuleFor(x => x.DefaultCooldownMinutes)
            .InclusiveBetween(1, 10080).WithErrorCode("InvalidDefaultCooldownMinutes");

        RuleFor(x => x.MaxGroupSize)
            .InclusiveBetween(1, 100).WithErrorCode("InvalidMaxGroupSize");

        RuleFor(x => x.DenialDedupMinutes)
            .InclusiveBetween(0, 60).WithErrorCode("InvalidDenialDedupMinutes");

        RuleFor(x => x.MaxFailedLogins)
            .InclusiveBetween(1, 20).WithErrorCode("InvalidMaxFailedLogins");

        RuleFor(x => x.LockoutMinutes)
            .InclusiveBetween(1, 1440).WithErrorCode("InvalidLockoutMinutes");

        RuleFor(x => x.AutoLockMinutes)
            .GreaterThanOrEqualTo(0).WithErrorCode("InvalidAutoLockMinutes");

        RuleFor(x => x.ParkName)
            .NotEmpty().WithErrorCode("ParkNameRequired")
            .MaximumLength(150).WithErrorCode("ParkNameTooLong");

        RuleFor(x => x.TimeZoneId)
            .NotEmpty().WithErrorCode("TimeZoneRequired");
    }
}
