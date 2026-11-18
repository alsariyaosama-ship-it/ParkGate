namespace ParkGate.Application.Common;

/// <summary>Masks a national ID for users without <c>ViewFullNationalId</c> (SPEC §8, R6.16).</summary>
public static class NationalIdMask
{
    /// <summary>Digits left visible at the end, enough for a holder to confirm their own number.</summary>
    public const int VisibleDigits = 4;

    public static string? Mask(string? nationalId)
    {
        if (string.IsNullOrEmpty(nationalId))
        {
            return nationalId;
        }

        if (nationalId.Length <= VisibleDigits)
        {
            return new string('*', nationalId.Length);
        }

        return new string('*', nationalId.Length - VisibleDigits) + nationalId[^VisibleDigits..];
    }
}
