using FluentValidation;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Domain.Entities;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Application.Beneficiaries.Validators;

public sealed class RegisterBeneficiaryRequestValidator : AbstractValidator<RegisterBeneficiaryRequest>
{
    public RegisterBeneficiaryRequestValidator()
    {
        RuleFor(x => x.OrganizationId)
            .GreaterThan(0).WithErrorCode("OrganizationRequired");

        RuleFor(x => x.CardNumber)
            .NotEmpty().WithErrorCode("CardNumberRequired")
            .Must(c => CardNumber.TryCreate(c, out _)).WithErrorCode("InvalidCardNumber");

        RuleFor(x => x.FullName)
            .NotEmpty().WithErrorCode("FullNameRequired")
            .MaximumLength(Beneficiary.MaxFullNameLength).WithErrorCode("FullNameTooLong");

        RuleFor(x => x.Phone)
            .NotEmpty().WithErrorCode("PhoneRequired")
            .Must(p => PhoneNumber.TryCreate(p, out _)).WithErrorCode("InvalidPhone");

        RuleFor(x => x.NationalId)
            .Must(n => string.IsNullOrWhiteSpace(n) || NationalId.TryCreate(n, out _))
            .WithErrorCode("InvalidNationalId");
    }
}
