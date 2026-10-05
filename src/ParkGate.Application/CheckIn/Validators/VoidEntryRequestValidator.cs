using FluentValidation;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.CheckIn.Validators;

public sealed class VoidEntryRequestValidator : AbstractValidator<VoidEntryRequest>
{
    public VoidEntryRequestValidator()
    {
        RuleFor(x => x.AttemptId)
            .NotEmpty().WithErrorCode("AttemptRequired");

        RuleFor(x => x.Reason)
            .NotEmpty().WithErrorCode("VoidReasonRequired")
            .MinimumLength(EntryAttempt.MinVoidReasonLength).WithErrorCode("VoidReasonTooShort")
            .MaximumLength(EntryAttempt.MaxVoidReasonLength).WithErrorCode("VoidReasonTooLong");
    }
}
