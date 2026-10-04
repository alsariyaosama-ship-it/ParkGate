using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Policies;

public sealed class EligibilityDecisionTests
{
    [Theory]
    [InlineData(DenialReason.CooldownActive, true)]
    [InlineData(DenialReason.CardExpired, true)]
    [InlineData(DenialReason.HolderSuspended, true)]
    [InlineData(DenialReason.HolderBlocked, false)]
    [InlineData(DenialReason.OrganizationInactive, false)]
    [InlineData((DenialReason)99, false)]
    public void CanOverride_MatchesBR11Rules(DenialReason reason, bool expectedCanOverride)
    {
        EligibilityDecision.CanOverride(reason).ShouldBe(expectedCanOverride);
    }

    [Fact]
    public void Allowed_InitializesAllowedProperties()
    {
        var nowUtc = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);
        var nextEligible = nowUtc.AddMinutes(720);

        var decision = EligibilityDecision.Allowed(720, nextEligible);

        decision.IsEligible.ShouldBeTrue();
        decision.DenialReason.ShouldBeNull();
        decision.EffectiveCooldownMinutes.ShouldBe(720);
        decision.NextEligibleAtUtc.ShouldBe(nextEligible);
        decision.CanBeOverridden.ShouldBeFalse();
    }

    [Fact]
    public void Context_InvalidArguments_ThrowsAppropriateExceptions()
    {
        var nowUtc = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);
        var unspecTime = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Unspecified);
        var today = new DateOnly(2026, 10, 4);

        // Default cooldown <= 0
        Should.Throw<ArgumentOutOfRangeException>(() => new EligibilityContext(
            true, null, 0, BeneficiaryStatus.Active, null, null, nowUtc, today));

        // Org cooldown <= 0
        Should.Throw<ArgumentOutOfRangeException>(() => new EligibilityContext(
            true, -5, 720, BeneficiaryStatus.Active, null, null, nowUtc, today));

        // NowUtc unspecified kind
        Should.Throw<ArgumentException>(() => new EligibilityContext(
            true, null, 720, BeneficiaryStatus.Active, null, null, unspecTime, today));

        // LastGrantedEntryUtc unspecified kind
        Should.Throw<ArgumentException>(() => new EligibilityContext(
            true, null, 720, BeneficiaryStatus.Active, null, unspecTime, nowUtc, today));
    }
}
