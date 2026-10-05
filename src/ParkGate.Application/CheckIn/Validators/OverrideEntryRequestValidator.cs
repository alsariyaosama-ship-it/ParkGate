using FluentValidation;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.CheckIn.Validators;

public sealed class OverrideEntryRequestValidator : AbstractValidator<OverrideEntryRequest>
{
    public OverrideEntryRequestValidator()
    {
        RuleFor(x => x.BeneficiaryId)
            .NotEmpty().WithErrorCode("BeneficiaryRequired");

        RuleFor(x => x.Reason)
            .NotEmpty().WithErrorCode("OverrideReasonRequired")
            .MinimumLength(EntryAttempt.MinOverrideReasonLength).WithErrorCode("OverrideReasonTooShort")
            .MaximumLength(EntryAttempt.MaxOverrideReasonLength).WithErrorCode("OverrideReasonTooLong");

        RuleForEach(x => x.Companions)
            .ChildRules(c =>
            {
                c.RuleFor(x => x.Count)
                    .GreaterThanOrEqualTo(0).WithErrorCode("NegativeCompanions");
                c.RuleFor(x => x.AgeCategoryId)
                    .GreaterThan(0).WithErrorCode("AgeCategoryRequired");
            });
    }
}
