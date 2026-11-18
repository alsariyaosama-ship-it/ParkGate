using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>R5.1: the denial banner says when entry is allowed and how long is left (SPEC A3).</summary>
public sealed class CooldownTextTests
{
    [Theory]
    [InlineData(600, "10:00")]
    [InlineData(65, "1:05")]
    [InlineData(1, "0:01")]
    [InlineData(0, "0:00")]
    public void FormatRemaining_Minutes_ShowsHoursAndTwoDigitMinutes(int minutes, string expected)
    {
        CooldownText.FormatRemaining(minutes).ShouldBe(expected);
    }

    [Fact]
    public void FormatRemaining_MoreThanOneDay_KeepsCountingHours()
    {
        CooldownText.FormatRemaining((26 * 60) + 7).ShouldBe("26:07");
    }

    [Fact]
    public void Format_SameDay_ShowsTimeAndRemainingWithoutDate()
    {
        var next = new DateTime(2026, 11, 18, 20, 30, 0);

        var text = CooldownText.Format(next, remainingMinutes: 125);

        text.ShouldContain(DateTimeDisplayHelper.FormatTime(next));
        text.ShouldContain("2:05");
        text.ShouldNotContain(DateTimeDisplayHelper.FormatDate(next));
    }

    [Fact]
    public void Format_NextDay_AlsoShowsTheDate()
    {
        var next = new DateTime(2026, 11, 19, 9, 0, 0);

        var text = CooldownText.Format(next, remainingMinutes: 13 * 60);

        text.ShouldContain(DateTimeDisplayHelper.FormatDate(next));
        text.ShouldContain(DateTimeDisplayHelper.FormatTime(next));
        text.ShouldContain("13:00");
    }

    [Fact]
    public void Format_RemainingUnknown_ShowsDateAndTimeOnly()
    {
        var next = new DateTime(2026, 11, 18, 20, 30, 0);

        var text = CooldownText.Format(next, remainingMinutes: null);

        text.ShouldBe(string.Format(
            Strings.Desk_CooldownAllowedOn,
            DateTimeDisplayHelper.FormatDate(next),
            DateTimeDisplayHelper.FormatTime(next)));
    }
}
