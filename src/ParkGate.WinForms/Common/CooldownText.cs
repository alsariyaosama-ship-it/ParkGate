using System.Globalization;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Common;

/// <summary>
/// Denial banner text for an active waiting period: when entry is allowed again and how long is left (SPEC A3, R5.1).
/// </summary>
public static class CooldownText
{
    private const int MinutesPerHour = 60;

    /// <param name="nextAllowedLocal">Next allowed entry in park local time.</param>
    /// <param name="remainingMinutes">Minutes left, from the database clock; null when unknown.</param>
    public static string Format(DateTime nextAllowedLocal, int? remainingMinutes)
    {
        var date = DateTimeDisplayHelper.FormatDate(nextAllowedLocal);
        var time = DateTimeDisplayHelper.FormatTime(nextAllowedLocal);

        if (remainingMinutes is not { } minutes)
        {
            return string.Format(CultureInfo.InvariantCulture, Strings.Desk_CooldownAllowedOn, date, time);
        }

        // "Now" is derived from the database values, so the desk clock never decides which day it is.
        var nowLocal = nextAllowedLocal.AddMinutes(-minutes);
        var remaining = FormatRemaining(minutes);

        return nowLocal.Date == nextAllowedLocal.Date
            ? string.Format(CultureInfo.InvariantCulture, Strings.Desk_CooldownAllowedAtToday, time, remaining)
            : string.Format(CultureInfo.InvariantCulture, Strings.Desk_CooldownAllowedAtOtherDay, date, time, remaining);
    }

    /// <summary>Hours and two-digit minutes, e.g. 65 → "1:05". Hours keep counting past a day.</summary>
    public static string FormatRemaining(int minutes)
    {
        var safe = Math.Max(0, minutes);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{safe / MinutesPerHour}:{safe % MinutesPerHour:00}");
    }
}
