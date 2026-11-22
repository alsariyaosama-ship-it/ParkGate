using System.Text;
using ParkGate.Domain.Common;

namespace ParkGate.WinForms.Common;

/// <summary>
/// Normalizes Arabic text for type-to-search lists (BR-32 city box), so that a city typed with ha instead of ta
/// marbuta, or without its definite article, is still found: hamza forms of alef become a plain alef, ta marbuta
/// becomes ha, alef maqsura becomes ya, and tatweel, harakat and extra spaces are dropped. Digits are turned into
/// Western digits like every numeric input (AGENTS.md section 5). The characters are written as escapes because
/// Arabic text lives in the resources only.
/// </summary>
public static class ArabicSearchText
{
    private const char Tatweel = '\u0640';
    private const char FirstHaraka = '\u064B';
    private const char LastHaraka = '\u0652';

    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var digits = DigitNormalizer.NormalizeDigits(text);
        var builder = new StringBuilder(digits.Length);
        var pendingSpace = false;

        foreach (var c in digits)
        {
            if (c == Tatweel || c is >= FirstHaraka and <= LastHaraka)
            {
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(MapLetter(c));
        }

        return builder.ToString();
    }

    private static char MapLetter(char c) => c switch
    {
        '\u0623' or '\u0625' or '\u0622' => '\u0627', // alef with hamza above, hamza below or madda -> alef
        '\u0629' => '\u0647', // ta marbuta -> ha
        '\u0649' => '\u064A', // alef maqsura -> ya
        _ => c
    };
}
