using FluentValidation;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.Beneficiaries.Validators;

public sealed class ChangeBeneficiaryStatusRequestValidator : AbstractValidator<ChangeBeneficiaryStatusRequest>
{
    public ChangeBeneficiaryStatusRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode("IdRequired");

        RuleFor(x => x.Reason)
            .NotEmpty().WithErrorCode("ReasonRequired")
            .MinimumLength(5).WithErrorCode("ReasonTooShort")
            .MaximumLength(Beneficiary.MaxStatusReasonLength).WithErrorCode("ReasonTooLong")
            .When(x => x.TargetStatus != BeneficiaryStatus.Active);
    }
}
