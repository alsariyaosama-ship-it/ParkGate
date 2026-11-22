using FluentValidation;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Domain.Entities;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Application.Beneficiaries.Validators;

public sealed class UpdateBeneficiaryRequestValidator : AbstractValidator<UpdateBeneficiaryRequest>
{
    public UpdateBeneficiaryRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode("IdRequired");

        RuleFor(x => x.OrganizationId)
            .GreaterThan(0).WithErrorCode("OrganizationRequired");

        RuleFor(x => x.CardNumber)
            .NotEmpty().WithErrorCode("CardNumberRequired")
            .Must(c => CardNumber.TryCreate(c, out _)).WithErrorCode("InvalidCardNumber");

        RuleFor(x => x.FullName)
            .NotEmpty().WithErrorCode("FullNameRequired")
            .MaximumLength(Beneficiary.MaxFullNameLength).WithErrorCode("FullNameTooLong");

        // BR-15: the phone is optional; when given it is validated like before.
        RuleFor(x => x.Phone)
            .Must(p => string.IsNullOrWhiteSpace(p) || PhoneNumber.TryCreate(p, out _))
            .WithErrorCode("InvalidPhone");

        RuleFor(x => x.CityId)
            .GreaterThan(0).When(x => x.CityId is not null)
            .WithErrorCode("InvalidCity");

        RuleFor(x => x.NationalId)
            .Must(n => string.IsNullOrWhiteSpace(n) || NationalId.TryCreate(n, out _))
            .WithErrorCode("InvalidNationalId");
    }
}
