using FluentValidation;
using ParkGate.Application.Workstations.DTOs;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.Workstations.Validators;

public sealed class UpdateWorkstationRequestValidator : AbstractValidator<UpdateWorkstationRequest>
{
    public UpdateWorkstationRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithErrorCode("IdRequired");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithErrorCode("DisplayNameRequired")
            .MaximumLength(Workstation.MaxDisplayNameLength).WithErrorCode("DisplayNameTooLong");

        RuleFor(x => x.Type)
            .IsInEnum().WithErrorCode("InvalidWorkstationType");
    }
}
