using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ParkGate.Domain.ValueObjects;

/// <summary>
/// Value object representing a normalized membership card number (BR-01).
/// </summary>
public sealed record CardNumber
{
    public const int MaxLength = 50;

    public string Value { get; }

    private CardNumber(string value)
    {
        Value = value;
    }

    public static bool TryCreate(string? raw, [NotNullWhen(true)] out CardNumber? cardNumber, [NotNullWhen(false)] out string? error)
    {
        cardNumber = null;
        error = null;

        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "Card number cannot be empty.";
            return false;
        }

        var trimmed = raw.Trim();
        var sb = new StringBuilder(trimmed.Length);
        var inWhitespace = false;

        foreach (var c in trimmed)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!inWhitespace)
                {
                    sb.Append(' ');
                    inWhitespace = true;
                }
            }
            else
            {
                inWhitespace = false;
                var normalized = c switch
                {
                    >= '\u0660' and <= '\u0669' => (char)('0' + (c - '\u0660')),
                    >= '\u06F0' and <= '\u06F9' => (char)('0' + (c - '\u06F0')),
                    _ => char.ToUpperInvariant(c)
                };
                sb.Append(normalized);
            }
        }

        var normalizedString = sb.ToString();

        if (normalizedString.Length == 0)
        {
            error = "Card number cannot be empty.";
            return false;
        }

        if (normalizedString.Length > MaxLength)
        {
            error = $"Card number exceeds maximum length of {MaxLength} characters.";
            return false;
        }

        cardNumber = new CardNumber(normalizedString);
        return true;
    }

    public static CardNumber Create(string raw)
    {
        if (!TryCreate(raw, out var cardNumber, out var error))
        {
            throw new ArgumentException(error, nameof(raw));
        }

        return cardNumber;
    }

    public override string ToString() => Value;

    public static implicit operator string(CardNumber cardNumber) => cardNumber.Value;
}
