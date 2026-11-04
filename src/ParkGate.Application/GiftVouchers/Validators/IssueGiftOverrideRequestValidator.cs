using FluentValidation;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.GiftVouchers.Validators;

public sealed class IssueGiftOverrideRequestValidator : AbstractValidator<IssueGiftOverrideRequest>
{
    public IssueGiftOverrideRequestValidator()
    {
        RuleFor(x => x.BeneficiaryId)
            .NotEmpty()
            .WithErrorCode("BeneficiaryIdRequired");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithErrorCode("OverrideReasonRequired")
            .MinimumLength(GiftVoucher.MinOverrideReasonLength)
            .WithErrorCode("OverrideReasonTooShort")
            .MaximumLength(GiftVoucher.MaxOverrideReasonLength)
            .WithErrorCode("OverrideReasonTooLong")
            .When(x => x.Reason is not null);
    }
}
