using FluentValidation.Results;

namespace ParkGate.Application.Common;

public static class ValidationExtensions
{
    public static Error ToError(this ValidationResult result)
    {
        var errors = result.Errors
            .Select(e => new ValidationError(
                e.PropertyName,
                string.IsNullOrEmpty(e.ErrorCode) ? "Invalid" : e.ErrorCode,
                e.ErrorMessage))
            .ToList();

        return Errors.General.ValidationFailed(errors);
    }
}
