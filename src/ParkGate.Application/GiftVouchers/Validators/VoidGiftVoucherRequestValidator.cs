using FluentValidation;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.GiftVouchers.Validators;

public sealed class VoidGiftVoucherRequestValidator : AbstractValidator<VoidGiftVoucherRequest>
{
    public VoidGiftVoucherRequestValidator()
    {
        RuleFor(x => x.VoucherId)
            .GreaterThan(0)
            .WithErrorCode("VoucherIdRequired");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithErrorCode("VoidReasonRequired")
            .MinimumLength(GiftVoucher.MinVoidReasonLength)
            .WithErrorCode("VoidReasonTooShort")
            .When(x => x.Reason is not null);
    }
}
