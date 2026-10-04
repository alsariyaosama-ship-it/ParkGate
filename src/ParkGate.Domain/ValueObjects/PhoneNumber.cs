using System.Diagnostics.CodeAnalysis;
using System.Text;
using ParkGate.Domain.Common;

namespace ParkGate.Domain.ValueObjects;

/// <summary>
/// Value object representing a normalized Libyan mobile phone number (BR-15).
/// Format is always 09XXXXXXXX (10 digits).
/// </summary>
public sealed record PhoneNumber
{
    public const int RequiredLength = 10;
    public const string Prefix = "09";

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static bool TryCreate(string? raw, [NotNullWhen(true)] out PhoneNumber? phoneNumber, [NotNullWhen(false)] out string? error)
    {
        phoneNumber = null;
        error = null;

        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "Phone number is required.";
            return false;
        }

        var normalizedDigits = DigitNormalizer.NormalizeDigits(raw);

        // Strip allowable formatting symbols: +, -, (, ), ., and whitespace
        var sb = new StringBuilder(normalizedDigits.Length);
        foreach (var c in normalizedDigits)
        {
            if (char.IsWhiteSpace(c) || c is '-' or '(' or ')' or '.' or '+')
            {
                continue;
            }

            if (!char.IsAsciiDigit(c))
            {
                error = "Phone number contains invalid characters.";
                return false;
            }

            sb.Append(c);
        }

        var digits = sb.ToString();

        // Handle international prefixes
        if (digits.StartsWith("00218", StringComparison.Ordinal))
        {
            digits = digits["00218".Length..];
        }
        else if (digits.StartsWith("218", StringComparison.Ordinal))
        {
            digits = digits["218".Length..];
        }

        // If it starts with 9 and has 9 digits, prepend 0
        if (digits.Length == 9 && digits.StartsWith('9'))
        {
            digits = "0" + digits;
        }

        // Validate final format: must start with 09 and be exactly 10 digits
        if (digits.Length != RequiredLength || !digits.StartsWith(Prefix, StringComparison.Ordinal))
        {
            error = "Phone number must be a valid 10-digit Libyan mobile starting with 09 (e.g. 091XXXXXXX).";
            return false;
        }

        phoneNumber = new PhoneNumber(digits);
        return true;
    }

    public static PhoneNumber Create(string raw)
    {
        if (!TryCreate(raw, out var phoneNumber, out var error))
        {
            throw new ArgumentException(error, nameof(raw));
        }

        return phoneNumber;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
