using System.Text;

namespace ParkGate.Domain.CardPatterns;

/// <summary>
/// The shape of a normalized card number (F8, BR-29): one class per character, <c>D</c> digit, <c>L</c> letter,
/// <c>-</c> dash, <c>O</c> anything else. <c>"AB-1234"</c> has the shape <c>"LL-DDDD"</c>.
/// </summary>
public static class CardShape
{
    public const int MaxLength = 50;

    public const char Digit = 'D';
    public const char Letter = 'L';
    public const char Dash = '-';
    public const char Other = 'O';

    public static string Of(string cardNumber)
    {
        ArgumentNullException.ThrowIfNull(cardNumber);

        var shape = new StringBuilder(cardNumber.Length);
        foreach (var c in cardNumber)
        {
            shape.Append(c switch
            {
                >= '0' and <= '9' => Digit,
                '-' => Dash,
                _ when char.IsLetter(c) => Letter,
                _ => Other,
            });
        }

        return shape.ToString();
    }

    /// <summary>
    /// A readable form for the admin screens: the prefix as it is, then <c>#</c> per digit and <c>A</c> per letter.
    /// Shape <c>DDDDDD</c> with prefix <c>10</c> reads <c>10####</c>.
    /// </summary>
    public static string Describe(string shape, string? prefix)
    {
        ArgumentNullException.ThrowIfNull(shape);

        var text = new StringBuilder(shape.Length);
        var start = 0;
        if (!string.IsNullOrEmpty(prefix) && prefix.Length <= shape.Length)
        {
            text.Append(prefix);
            start = prefix.Length;
        }

        for (var i = start; i < shape.Length; i++)
        {
            text.Append(shape[i] switch
            {
                Digit => '#',
                Letter => 'A',
                Dash => '-',
                _ => '?',
            });
        }

        return text.ToString();
    }
}
