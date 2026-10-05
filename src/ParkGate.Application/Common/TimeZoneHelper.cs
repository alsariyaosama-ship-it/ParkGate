namespace ParkGate.Application.Common;

/// <summary>
/// Time zone conversion utility for park local time calculations (BR-05).
/// </summary>
public static class TimeZoneHelper
{
    public static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            timeZoneId = "Libya Standard Time";
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback for cross-platform environments where Windows time zone IDs may not be registered
            try
            {
                // Try IANA equivalent for Libya Standard Time
                return TimeZoneInfo.FindSystemTimeZoneById("Africa/Tripoli");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.Utc;
            }
        }
    }

    public static DateOnly GetLocalBusinessDate(DateTime utcDateTime, string? timeZoneId = null)
    {
        var tz = ResolveTimeZone(timeZoneId);
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tz);
        return DateOnly.FromDateTime(localTime);
    }
}
