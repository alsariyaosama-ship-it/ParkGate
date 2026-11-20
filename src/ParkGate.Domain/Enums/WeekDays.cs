namespace ParkGate.Domain.Enums;

/// <summary>
/// Weekdays as a bit mask (F8, BR-27): bit 0 = Sunday … bit 6 = Saturday, matching <see cref="DayOfWeek"/>.
/// Stored in <c>Organizations.AllowedEntryDays</c>.
/// </summary>
[Flags]
public enum WeekDays : byte
{
    None = 0,
    Sunday = 1 << DayOfWeek.Sunday,
    Monday = 1 << DayOfWeek.Monday,
    Tuesday = 1 << DayOfWeek.Tuesday,
    Wednesday = 1 << DayOfWeek.Wednesday,
    Thursday = 1 << DayOfWeek.Thursday,
    Friday = 1 << DayOfWeek.Friday,
    Saturday = 1 << DayOfWeek.Saturday,
    All = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday,
}

public static class WeekDaysExtensions
{
    /// <summary>The flag of one weekday.</summary>
    public static WeekDays ToWeekDays(this DayOfWeek day) => (WeekDays)(1 << (int)day);

    /// <summary>True when <paramref name="day"/> is one of <paramref name="days"/>.</summary>
    public static bool Includes(this WeekDays days, DayOfWeek day) => (days & day.ToWeekDays()) != 0;

    /// <summary>The weekdays in the mask, Saturday first (the park's week starts on Saturday).</summary>
    public static IReadOnlyList<DayOfWeek> ToDays(this WeekDays days) =>
        new[] { DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }
            .Where(day => days.Includes(day))
            .ToList();

    /// <summary>A valid set of entry days: at least one day and no unknown bits.</summary>
    public static bool IsValidEntryDays(this WeekDays days) => days != WeekDays.None && (days & ~WeekDays.All) == 0;
}
