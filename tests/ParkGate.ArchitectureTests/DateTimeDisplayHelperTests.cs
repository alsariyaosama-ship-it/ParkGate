using ParkGate.WinForms.Common;
using Shouldly;

namespace ParkGate.ArchitectureTests;

public sealed class DateTimeDisplayHelperTests
{
    [Fact]
    public void FormatDate_ShouldFormatWithWesternDigitsAndCorrectPattern()
    {
        var date = new DateTime(2026, 10, 5, 14, 30, 0, DateTimeKind.Utc);
        var formatted = DateTimeDisplayHelper.FormatDate(date);

        formatted.ShouldBe("05/10/2026");
        // Verify digits are 0-9 ASCII
        formatted.All(c => c == '/' || (c >= '0' && c <= '9')).ShouldBeTrue();
    }

    [Fact]
    public void FormatTime_ShouldFormatWithWesternDigitsAndArabicAmPm()
    {
        var morning = new DateTime(2026, 10, 5, 9, 15, 0);
        var evening = new DateTime(2026, 10, 5, 21, 45, 0);

        var formattedMorning = DateTimeDisplayHelper.FormatTime(morning);
        var formattedEvening = DateTimeDisplayHelper.FormatTime(evening);

        formattedMorning.ShouldContain("09:15");
        formattedMorning.ShouldContain("ص");

        formattedEvening.ShouldContain("09:45");
        formattedEvening.ShouldContain("م");
    }

    [Fact]
    public void ToParkLocalTime_ShouldConvertUtcToLibyaTime()
    {
        // Libya is UTC+2 with no daylight saving time (standard UTC+2)
        var utc = new DateTime(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);
        var local = DateTimeDisplayHelper.ToParkLocalTime(utc, "Libya Standard Time");

        local.Hour.ShouldBe(14); // 12 + 2 = 14
        local.Day.ShouldBe(5);
    }
}
