namespace ParkGate.Domain.Common;

/// <summary>
/// Utility for normalizing Arabic-Indic (٠-٩) and Eastern Arabic-Indic / Persian (۰-۹) digits to Western Latin digits (0-9).
/// </summary>
public static class DigitNormalizer
{
    public static string NormalizeDigits(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var chars = new char[input.Length];
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c is >= '\u0660' and <= '\u0669')
            {
                chars[i] = (char)('0' + (c - '\u0660'));
            }
            else if (c is >= '\u06F0' and <= '\u06F9')
            {
                chars[i] = (char)('0' + (c - '\u06F0'));
            }
            else
            {
                chars[i] = c;
            }
        }

        return new string(chars);
    }
}
