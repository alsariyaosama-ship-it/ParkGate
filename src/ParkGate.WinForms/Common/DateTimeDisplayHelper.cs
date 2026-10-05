using System.Globalization;

namespace ParkGate.WinForms.Common;

/// <summary>
/// Formats dates and times per AGENTS.md §5.8:
/// Western digits 0-9, dd/MM/yyyy, hh:mm tt with ص/م (ar-LY culture with Latin digits).
/// Also converts UTC timestamps to park local time.
/// </summary>
public static class DateTimeDisplayHelper
{
    private static readonly CultureInfo ArabicLibyaCultureWithLatinDigits;

    static DateTimeDisplayHelper()
    {
        var culture = (CultureInfo)CultureInfo.GetCultureInfo("ar-LY").Clone();
        culture.NumberFormat.DigitSubstitution = DigitShapes.None;
        culture.NumberFormat.NativeDigits = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];
        culture.DateTimeFormat.AMDesignator = "ص";
        culture.DateTimeFormat.PMDesignator = "م";
        culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
        culture.DateTimeFormat.ShortTimePattern = "hh:mm tt";
        culture.DateTimeFormat.LongTimePattern = "hh:mm:ss tt";
        ArabicLibyaCultureWithLatinDigits = culture;
    }

    public static CultureInfo Culture => ArabicLibyaCultureWithLatinDigits;

    public static string FormatDate(DateTime dateTime)
    {
        return dateTime.ToString("dd'/'MM'/'yyyy", ArabicLibyaCultureWithLatinDigits);
    }

    public static string FormatTime(DateTime dateTime)
    {
        return dateTime.ToString("hh':'mm tt", ArabicLibyaCultureWithLatinDigits);
    }

    public static string FormatTimeWithSeconds(DateTime dateTime)
    {
        return dateTime.ToString("hh':'mm':'ss tt", ArabicLibyaCultureWithLatinDigits);
    }

    public static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("dd'/'MM'/'yyyy hh':'mm':'ss tt", ArabicLibyaCultureWithLatinDigits);
    }

    public static DateTime ToParkLocalTime(DateTime utcTime, string? timeZoneId)
    {
        var tz = ResolveTimeZone(timeZoneId);
        var specifiedUtc = utcTime.Kind == DateTimeKind.Utc 
            ? utcTime 
            : DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(specifiedUtc, tz);
    }

    public static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback below
            }
            catch (InvalidTimeZoneException)
            {
                // Fallback below
            }
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Libya Standard Time");
        }
        catch
        {
            return TimeZoneInfo.Local;
        }
    }
}
