using FluentValidation;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.AgeCategories.Validators;

public sealed class UpdateAgeCategoryRequestValidator : AbstractValidator<UpdateAgeCategoryRequest>
{
    public UpdateAgeCategoryRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithErrorCode("IdRequired");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithErrorCode("NameRequired")
            .MaximumLength(AgeCategory.MaxNameLength).WithErrorCode("NameTooLong");

        RuleFor(x => x.DescriptionAr)
            .MaximumLength(AgeCategory.MaxDescriptionLength).WithErrorCode("DescriptionTooLong")
            .When(x => !string.IsNullOrEmpty(x.DescriptionAr));

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithErrorCode("InvalidSortOrder");
    }
}
