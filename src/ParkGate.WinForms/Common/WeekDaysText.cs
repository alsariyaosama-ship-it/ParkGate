using ParkGate.Domain.Enums;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Common;

/// <summary>Arabic weekday names for the organization's entry days (F8, BR-27).</summary>
public static class WeekDaysText
{
    /// <summary>Arabic name of one weekday, from the display culture (no literals in code).</summary>
    public static string Name(DayOfWeek day) => DateTimeDisplayHelper.Culture.DateTimeFormat.GetDayName(day);

    /// <summary>The days Saturday first, e.g. «السبت، الأربعاء»; every day gives «كل الأيام».</summary>
    public static string Format(WeekDays days) =>
        days == WeekDays.All
            ? Strings.Text_EveryDay
            : string.Join(Strings.Text_ListSeparator, days.ToDays().Select(Name));
}
