namespace ParkGate.Application.Common;

/// <summary>
/// Represents a domain or application error with a stable code and optional message/details.
/// Error codes are mapped to Arabic UI messages in presentation layer.
/// </summary>
public sealed record Error(
    string Code,
    string Message = "",
    IReadOnlyList<ValidationError>? ValidationErrors = null)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public bool IsEmpty => string.IsNullOrEmpty(Code);

    public static implicit operator Error(string code) => new(code);
}

/// <summary>
/// Represents a field-level validation error.
/// </summary>
public sealed record ValidationError(string PropertyName, string Code, string Message = "");
