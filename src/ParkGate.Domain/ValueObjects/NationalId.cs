using System.Diagnostics.CodeAnalysis;
using System.Text;
using ParkGate.Domain.Common;

namespace ParkGate.Domain.ValueObjects;

/// <summary>
/// Value object representing a Libyan National Identification Number (الرقم الوطني) (BR-16).
/// Must be exactly 12 digits.
/// </summary>
public sealed record NationalId
{
    public const int RequiredLength = 12;

    public string Value { get; }

    private NationalId(string value)
    {
        Value = value;
    }

    public static bool TryCreate(string? raw, [NotNullWhen(true)] out NationalId? nationalId, [NotNullWhen(false)] out string? error)
    {
        nationalId = null;
        error = null;

        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "National ID cannot be empty.";
            return false;
        }

        var normalizedDigits = DigitNormalizer.NormalizeDigits(raw);

        var sb = new StringBuilder(normalizedDigits.Length);
        foreach (var c in normalizedDigits)
        {
            if (char.IsWhiteSpace(c) || c == '-')
            {
                continue;
            }

            if (!char.IsAsciiDigit(c))
            {
                error = "National ID must contain digits only.";
                return false;
            }

            sb.Append(c);
        }

        var digits = sb.ToString();

        if (digits.Length != RequiredLength)
        {
            error = $"National ID must be exactly {RequiredLength} digits.";
            return false;
        }

        nationalId = new NationalId(digits);
        return true;
    }

    public static NationalId Create(string raw)
    {
        if (!TryCreate(raw, out var nationalId, out var error))
        {
            throw new ArgumentException(error, nameof(raw));
        }

        return nationalId;
    }

    public override string ToString() => Value;

    public static implicit operator string(NationalId nationalId) => nationalId.Value;
}
