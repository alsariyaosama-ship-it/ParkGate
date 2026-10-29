using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Policies;

public sealed class RollingIntervalGiftEligibilityPolicyTests
{
    private readonly RollingIntervalGiftEligibilityPolicy _policy = new();
    private readonly DateTime _baseTimeUtc = new(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);

    // --- Helpers ---

    private static Organization ActiveOrg(bool giftEnabled = true, int intervalDays = 30)
    {
        var org = new Organization("TEST", "Test Org");
        org.ConfigureGift(giftEnabled, giftEnabled ? 50m : 0m, intervalDays);
        return org;
    }

    private static Organization InactiveOrg()
    {
        var org = new Organization("TEST", "Test Org");
        org.Deactivate();
        org.ConfigureGift(true, 50m, 30);
        return org;
    }

    private static Beneficiary ActiveHolder()
    {
        return new Beneficiary(
            Guid.NewGuid(),
            organizationId: 1,
            CardNumber.Create("TEST-001"),
            "Test Holder",
            PhoneNumber.Create("0912345678"),
            cardExpiryDate: new DateOnly(2030, 1, 1));
    }

    private static Beneficiary BlockedHolder()
    {
        var b = ActiveHolder();
        b.Block("blocked for test purpose");
        return b;
    }

    private static Beneficiary SuspendedHolder()
    {
        var b = ActiveHolder();
        b.Suspend("suspended for test purpose");
        return b;
    }

    // --- Tests ---

    [Fact]
    public void Evaluate_NullContext_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => _policy.Evaluate(null!));
    }

    [Fact]
    public void Evaluate_NoPreviousGift_ReturnsEligible()
    {
        var context = new GiftEligibilityContext(ActiveOrg(), ActiveHolder(), null, _baseTimeUtc);

        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBeTrue();
        decision.Reason.ShouldBeNull();
        decision.CanBeOverridden.ShouldBeFalse();
    }

    [Theory]
    // Exactly at the boundary (now == last + days) -> Eligible (>=)
    [InlineData(0, true)]
    // 1 second before boundary -> Denied
    [InlineData(-1, false)]
    // 1 second after boundary -> Eligible
    [InlineData(1, true)]
    public void Evaluate_IntervalBoundaries_ReturnsExpectedDecision(int secondOffset, bool expectedEligible)
    {
        const int intervalDays = 30;
        var lastGiftUtc = _baseTimeUtc;
        var boundary = lastGiftUtc.AddDays(intervalDays);
        var nowUtc = boundary.AddSeconds(secondOffset);

        var context = new GiftEligibilityContext(ActiveOrg(intervalDays: intervalDays), ActiveHolder(), lastGiftUtc, nowUtc);

        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBe(expectedEligible);

        if (!expectedEligible)
        {
            decision.Reason.ShouldBe(GiftDenialReason.IntervalNotElapsed);
            decision.CanBeOverridden.ShouldBeTrue();
            decision.NextEligibleAtUtc.ShouldBe(boundary);
        }
    }

    [Fact]
    public void Evaluate_OrganizationInactive_ReturnsDenied_CannotBeOverridden()
    {
        var context = new GiftEligibilityContext(InactiveOrg(), ActiveHolder(), null, _baseTimeUtc);

        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBeFalse();
        decision.Reason.ShouldBe(GiftDenialReason.OrganizationInactive);
        decision.CanBeOverridden.ShouldBeFalse();
    }

    [Fact]
    public void Evaluate_GiftsNotEnabled_ReturnsDenied_CannotBeOverridden()
    {
        var context = new GiftEligibilityContext(ActiveOrg(giftEnabled: false), ActiveHolder(), null, _baseTimeUtc);

        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBeFalse();
        decision.Reason.ShouldBe(GiftDenialReason.GiftsNotEnabled);
        decision.CanBeOverridden.ShouldBeFalse();
    }

    [Fact]
    public void Evaluate_HolderBlocked_ReturnsDenied_CannotBeOverridden()
    {
        var context = new GiftEligibilityContext(ActiveOrg(), BlockedHolder(), null, _baseTimeUtc);

        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBeFalse();
        decision.Reason.ShouldBe(GiftDenialReason.HolderBlocked);
        decision.CanBeOverridden.ShouldBeFalse();
    }

    [Fact]
    public void Evaluate_HolderSuspended_ReturnsDenied_CannotBeOverridden()
    {
        var context = new GiftEligibilityContext(ActiveOrg(), SuspendedHolder(), null, _baseTimeUtc);

        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBeFalse();
        decision.Reason.ShouldBe(GiftDenialReason.HolderSuspended);
        decision.CanBeOverridden.ShouldBeFalse();
    }

    [Fact]
    public void Evaluate_CheckOrder_BlockedAndIntervalNotElapsed_ReturnsHolderBlocked()
    {
        // Both Blocked and interval not elapsed — Blocked wins (check order 3 before 5)
        var lastGiftUtc = _baseTimeUtc.AddDays(-1); // only 1 day passed, interval is 30
        var context = new GiftEligibilityContext(ActiveOrg(), BlockedHolder(), lastGiftUtc, _baseTimeUtc);

        var decision = _policy.Evaluate(context);

        decision.Reason.ShouldBe(GiftDenialReason.HolderBlocked);
    }

    [Fact]
    public void Evaluate_CheckOrder_OrgInactiveAndGiftsDisabledAndBlocked_ReturnsOrganizationInactive()
    {
        // All failures present — OrganizationInactive wins (check order 1 first)
        // Org inactive, gifts disabled on inactive org — we configure it inactive so gifts check irrelevant
        var org = new Organization("TEST", "Test Org");
        org.Deactivate();
        // GiftEnabled defaults to false, intervalDays defaults to DefaultGiftIntervalDays
        // Don't call ConfigureGift to keep gifts disabled
        var context = new GiftEligibilityContext(org, BlockedHolder(), null, _baseTimeUtc);

        var decision = _policy.Evaluate(context);

        decision.Reason.ShouldBe(GiftDenialReason.OrganizationInactive);
    }
}
