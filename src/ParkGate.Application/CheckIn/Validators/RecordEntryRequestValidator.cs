using FluentValidation;
using ParkGate.Application.CheckIn.DTOs;

namespace ParkGate.Application.CheckIn.Validators;

public sealed class RecordEntryRequestValidator : AbstractValidator<RecordEntryRequest>
{
    public RecordEntryRequestValidator()
    {
        RuleFor(x => x.BeneficiaryId)
            .NotEmpty().WithErrorCode("BeneficiaryRequired");

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
