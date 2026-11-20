using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Policies;

/// <summary>F8, BR-27 and BR-28: entry and gifts only on the organization's weekdays.</summary>
public sealed class EntryDaysTests
{
    // 2026-11-21 is a Saturday, 2026-11-22 a Sunday, 2026-11-25 a Wednesday.
    private static readonly DateOnly Saturday = new(2026, 11, 21);
    private static readonly DateOnly Sunday = new(2026, 11, 22);
    private static readonly DateOnly Wednesday = new(2026, 11, 25);
    private const WeekDays SaturdayAndWednesday = WeekDays.Saturday | WeekDays.Wednesday;

    private readonly RollingWindowEligibilityPolicy _entryPolicy = new();
    private readonly RollingIntervalGiftEligibilityPolicy _giftPolicy = new();

    [Theory]
    [InlineData(DayOfWeek.Sunday, WeekDays.Sunday)]
    [InlineData(DayOfWeek.Wednesday, WeekDays.Wednesday)]
    [InlineData(DayOfWeek.Saturday, WeekDays.Saturday)]
    public void ToWeekDays_MatchesDayOfWeekBits(DayOfWeek day, WeekDays expected) =>
        day.ToWeekDays().ShouldBe(expected);

    [Fact]
    public void ToDays_StartsTheWeekOnSaturday() =>
        (WeekDays.Sunday | WeekDays.Wednesday | WeekDays.Saturday).ToDays()
            .ShouldBe([DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Wednesday]);

    [Theory]
    [InlineData((byte)0, false)]
    [InlineData((byte)1, true)]
    [InlineData((byte)127, true)]
    [InlineData((byte)128, false)]
    public void IsValidEntryDays_NeedsAtLeastOneKnownDay(byte mask, bool expected) =>
        ((WeekDays)mask).IsValidEntryDays().ShouldBe(expected);

    [Fact]
    public void NewOrganization_AllowsEveryDayAndNoGiftOnAnyDay()
    {
        var org = new Organization("SSF", "Fund");

        org.AllowedEntryDays.ShouldBe(WeekDays.All);
        org.GiftAnyDay.ShouldBeFalse();
    }

    [Fact]
    public void SetEntryDays_StoresDaysAndGiftOption()
    {
        var org = new Organization("SSF", "Fund");

        org.SetEntryDays(SaturdayAndWednesday, giftAnyDay: true);

        org.AllowedEntryDays.ShouldBe(SaturdayAndWednesday);
        org.GiftAnyDay.ShouldBeTrue();
        org.IsEntryDay(Saturday).ShouldBeTrue();
        org.IsEntryDay(Sunday).ShouldBeFalse();
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)200)]
    public void SetEntryDays_WithoutAValidDay_Throws(byte mask) =>
        Should.Throw<ArgumentOutOfRangeException>(() => new Organization("SSF", "Fund").SetEntryDays((WeekDays)mask, false));

    [Theory]
    [InlineData("2026-11-21", true)]   // Saturday
    [InlineData("2026-11-22", false)]  // Sunday
    [InlineData("2026-11-25", true)]   // Wednesday
    [InlineData("2026-11-27", false)]  // Friday
    public void Entry_OnlyOnAllowedDays(string date, bool expectedEligible)
    {
        var decision = _entryPolicy.Evaluate(EntryContext(DateOnly.Parse(date), BeneficiaryStatus.Active, SaturdayAndWednesday));

        decision.IsEligible.ShouldBe(expectedEligible);
        if (!expectedEligible)
        {
            decision.DenialReason.ShouldBe(DenialReason.DayNotAllowed);
            decision.CanBeOverridden.ShouldBeTrue();
        }
    }

    [Fact]
    public void Entry_BlockedHolderOnAnotherDay_IsDeniedAsBlockedSoNoOverrideLetsThemIn()
    {
        var decision = _entryPolicy.Evaluate(EntryContext(Sunday, BeneficiaryStatus.Blocked, SaturdayAndWednesday));

        decision.DenialReason.ShouldBe(DenialReason.HolderBlocked);
        decision.CanBeOverridden.ShouldBeFalse();
    }

    [Fact]
    public void Entry_InactiveOrganizationOnAnotherDay_IsDeniedAsInactive()
    {
        var context = new EligibilityContext(false, null, 720, BeneficiaryStatus.Active, null, null, NowOn(Sunday), Sunday, SaturdayAndWednesday);

        _entryPolicy.Evaluate(context).DenialReason.ShouldBe(DenialReason.OrganizationInactive);
    }

    [Fact]
    public void Entry_SuspendedHolderOnAnotherDay_SaysTheDayFirst()
    {
        var decision = _entryPolicy.Evaluate(EntryContext(Sunday, BeneficiaryStatus.Suspended, SaturdayAndWednesday));

        decision.DenialReason.ShouldBe(DenialReason.DayNotAllowed);
    }

    [Fact]
    public void EntryContext_FromOrganization_TakesItsDays()
    {
        var org = new Organization("SSF", "Fund");
        org.SetEntryDays(SaturdayAndWednesday, false);

        var context = new EligibilityContext(org, Holder(), 720, NowOn(Sunday), Sunday);

        context.AllowedEntryDays.ShouldBe(SaturdayAndWednesday);
        _entryPolicy.Evaluate(context).DenialReason.ShouldBe(DenialReason.DayNotAllowed);
    }

    [Fact]
    public void EntryContext_WithoutDays_Throws() =>
        Should.Throw<ArgumentOutOfRangeException>(() =>
            new EligibilityContext(true, null, 720, BeneficiaryStatus.Active, null, null, NowOn(Sunday), Sunday, WeekDays.None));

    [Theory]
    [InlineData("2026-11-21", false, true)]  // Saturday: an entry day
    [InlineData("2026-11-22", false, false)] // Sunday: not an entry day
    [InlineData("2026-11-22", true, true)]   // Sunday, but gifts on any day
    public void Gift_FollowsEntryDaysUnlessGiftAnyDay(string date, bool giftAnyDay, bool expectedEligible)
    {
        var day = DateOnly.Parse(date);
        var org = GiftOrganization(giftAnyDay);

        var decision = _giftPolicy.Evaluate(new GiftEligibilityContext(org, Holder(), null, NowOn(day), hasEntryToday: true, businessDate: day));

        decision.IsEligible.ShouldBe(expectedEligible);
        if (!expectedEligible)
        {
            decision.Reason.ShouldBe(GiftDenialReason.DayNotAllowed);
            decision.CanBeOverridden.ShouldBeTrue();
        }
    }

    [Fact]
    public void Gift_NoEntryTodayOnAnotherDay_StaysNotOverridable()
    {
        var decision = _giftPolicy.Evaluate(new GiftEligibilityContext(GiftOrganization(false), Holder(), null, NowOn(Sunday), hasEntryToday: false, businessDate: Sunday));

        decision.Reason.ShouldBe(GiftDenialReason.NoEntryToday);
        decision.CanBeOverridden.ShouldBeFalse();
    }

    [Fact]
    public void Gift_OnWednesdayEntryDay_IsEligible() =>
        _giftPolicy.Evaluate(new GiftEligibilityContext(GiftOrganization(false), Holder(), null, NowOn(Wednesday), true, Wednesday))
            .IsEligible.ShouldBeTrue();

    private static EligibilityContext EntryContext(DateOnly day, BeneficiaryStatus status, WeekDays days) =>
        new(true, null, 720, status, null, null, NowOn(day), day, days);

    private static DateTime NowOn(DateOnly day) => day.ToDateTime(new TimeOnly(10, 0), DateTimeKind.Utc);

    private static Organization GiftOrganization(bool giftAnyDay)
    {
        var org = new Organization("SSF", "Fund");
        org.ConfigureGift(true, 10m, 30);
        org.SetEntryDays(SaturdayAndWednesday, giftAnyDay);
        return org;
    }

    private static Beneficiary Holder() =>
        new(Guid.NewGuid(), 1, CardNumber.Create("100200"), "Holder", PhoneNumber.Create("0912345678"), cardExpiryDate: null);
}
