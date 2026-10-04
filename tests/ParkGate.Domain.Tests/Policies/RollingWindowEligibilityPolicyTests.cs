using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Policies;

public sealed class RollingWindowEligibilityPolicyTests
{
    private readonly RollingWindowEligibilityPolicy _policy = new();
    private readonly DateTime _baseTimeUtc = new(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);
    private readonly DateOnly _todayLocal = new(2026, 10, 4);

    [Fact]
    public void Evaluate_NullContext_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => _policy.Evaluate(null!));
    }

    [Fact]
    public void Evaluate_NoPreviousEntry_ReturnsAllowed()
    {
        var context = new EligibilityContext(
            isOrganizationActive: true,
            organizationCooldownMinutes: null,
            defaultCooldownMinutes: 720,
            beneficiaryStatus: BeneficiaryStatus.Active,
            cardExpiryDate: _todayLocal.AddMonths(6),
            lastGrantedEntryUtc: null,
            nowUtc: _baseTimeUtc,
            localTodayDate: _todayLocal);

        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBeTrue();
        decision.DenialReason.ShouldBeNull();
        decision.EffectiveCooldownMinutes.ShouldBe(720);
        decision.NextEligibleAtUtc.ShouldBe(_baseTimeUtc.AddMinutes(720));
        decision.CanBeOverridden.ShouldBeFalse();
    }

    [Theory]
    // Exactly on the boundary (cooldown = 12h = 720m) -> Eligible (BR-04)
    [InlineData(0, true, null)]
    // 1 second before the boundary -> Denied CooldownActive
    [InlineData(-1, false, DenialReason.CooldownActive)]
    // 1 second after the boundary -> Eligible
    [InlineData(1, true, null)]
    public void Evaluate_CooldownBoundaries_ReturnsExpectedDecision(
        int secondOffsetFromBoundary,
        bool expectedEligible,
        DenialReason? expectedDenialReason)
    {
        const int cooldownMinutes = 720;
        var lastGrantedUtc = _baseTimeUtc;
        var exactBoundaryUtc = lastGrantedUtc.AddMinutes(cooldownMinutes);
        var nowUtc = exactBoundaryUtc.AddSeconds(secondOffsetFromBoundary);

        var context = new EligibilityContext(
            isOrganizationActive: true,
            organizationCooldownMinutes: cooldownMinutes,
            defaultCooldownMinutes: 1440,
            beneficiaryStatus: BeneficiaryStatus.Active,
            cardExpiryDate: null,
            lastGrantedEntryUtc: lastGrantedUtc,
            nowUtc: nowUtc,
            localTodayDate: _todayLocal);

        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBe(expectedEligible);
        decision.DenialReason.ShouldBe(expectedDenialReason);
        decision.EffectiveCooldownMinutes.ShouldBe(cooldownMinutes);

        if (expectedEligible)
        {
            decision.NextEligibleAtUtc.ShouldBe(nowUtc.AddMinutes(cooldownMinutes));
            decision.CanBeOverridden.ShouldBeFalse();
        }
        else
        {
            decision.NextEligibleAtUtc.ShouldBe(exactBoundaryUtc);
            decision.CanBeOverridden.ShouldBeTrue(); // CooldownActive can be overridden (BR-11)
        }
    }

    [Theory]
    // Expired yesterday -> Denied
    [InlineData(-1, false, DenialReason.CardExpired, true)]
    // Expires today -> Eligible (valid until end of day today)
    [InlineData(0, true, null, false)]
    // Expires tomorrow -> Eligible
    [InlineData(1, true, null, false)]
    public void Evaluate_CardExpiryDates_ReturnsExpectedDecision(
        int dayOffsetFromToday,
        bool expectedEligible,
        DenialReason? expectedDenialReason,
        bool expectedCanOverride)
    {
        var expiryDate = _todayLocal.AddDays(dayOffsetFromToday);

        var context = new EligibilityContext(
            isOrganizationActive: true,
            organizationCooldownMinutes: null,
            defaultCooldownMinutes: 720,
            beneficiaryStatus: BeneficiaryStatus.Active,
            cardExpiryDate: expiryDate,
            lastGrantedEntryUtc: null,
            nowUtc: _baseTimeUtc,
            localTodayDate: _todayLocal);

        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBe(expectedEligible);
        decision.DenialReason.ShouldBe(expectedDenialReason);
        decision.CanBeOverridden.ShouldBe(expectedCanOverride);
    }

    [Fact]
    public void Evaluate_InactiveOrganization_ReturnsDenied_CannotBeOverridden()
    {
        var context = new EligibilityContext(
            isOrganizationActive: false,
            organizationCooldownMinutes: null,
            defaultCooldownMinutes: 720,
            beneficiaryStatus: BeneficiaryStatus.Active,
            cardExpiryDate: null,
            lastGrantedEntryUtc: null,
            nowUtc: _baseTimeUtc,
            localTodayDate: _todayLocal);

        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBeFalse();
        decision.DenialReason.ShouldBe(DenialReason.OrganizationInactive);
        decision.CanBeOverridden.ShouldBeFalse(); // Inactive org cannot be overridden (BR-11)
    }

    [Fact]
    public void Evaluate_BlockedBeneficiary_ReturnsDenied_CannotBeOverridden()
    {
        var context = new EligibilityContext(
            isOrganizationActive: true,
            organizationCooldownMinutes: null,
            defaultCooldownMinutes: 720,
            beneficiaryStatus: BeneficiaryStatus.Blocked,
            cardExpiryDate: null,
            lastGrantedEntryUtc: null,
            nowUtc: _baseTimeUtc,
            localTodayDate: _todayLocal);

        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBeFalse();
        decision.DenialReason.ShouldBe(DenialReason.HolderBlocked);
        decision.CanBeOverridden.ShouldBeFalse(); // Blocked holder cannot be overridden (BR-11)
    }

    [Fact]
    public void Evaluate_SuspendedBeneficiary_ReturnsDenied_CanBeOverridden()
    {
        var context = new EligibilityContext(
            isOrganizationActive: true,
            organizationCooldownMinutes: null,
            defaultCooldownMinutes: 720,
            beneficiaryStatus: BeneficiaryStatus.Suspended,
            cardExpiryDate: null,
            lastGrantedEntryUtc: null,
            nowUtc: _baseTimeUtc,
            localTodayDate: _todayLocal);

        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBeFalse();
        decision.DenialReason.ShouldBe(DenialReason.HolderSuspended);
        decision.CanBeOverridden.ShouldBeTrue(); // Suspended holder can be overridden (BR-11)
    }

    [Fact]
    public void Evaluate_CheckOrder_BR06_OrganizationInactiveTakesPrecedenceOverBlocked()
    {
        // Both organization inactive AND holder blocked
        var context = new EligibilityContext(
            isOrganizationActive: false,
            organizationCooldownMinutes: null,
            defaultCooldownMinutes: 720,
            beneficiaryStatus: BeneficiaryStatus.Blocked,
            cardExpiryDate: _todayLocal.AddDays(-10),
            lastGrantedEntryUtc: _baseTimeUtc.AddMinutes(-10),
            nowUtc: _baseTimeUtc,
            localTodayDate: _todayLocal);

        var decision = _policy.Evaluate(context);

        decision.DenialReason.ShouldBe(DenialReason.OrganizationInactive);
    }

    [Fact]
    public void Evaluate_CheckOrder_BR06_HolderBlockedTakesPrecedenceOverSuspendedAndExpired()
    {
        // Organization active, but holder blocked, card expired, cooldown active
        var context = new EligibilityContext(
            isOrganizationActive: true,
            organizationCooldownMinutes: null,
            defaultCooldownMinutes: 720,
            beneficiaryStatus: BeneficiaryStatus.Blocked,
            cardExpiryDate: _todayLocal.AddDays(-10),
            lastGrantedEntryUtc: _baseTimeUtc.AddMinutes(-10),
            nowUtc: _baseTimeUtc,
            localTodayDate: _todayLocal);

        var decision = _policy.Evaluate(context);

        decision.DenialReason.ShouldBe(DenialReason.HolderBlocked);
    }

    [Fact]
    public void Evaluate_CheckOrder_BR06_HolderSuspendedTakesPrecedenceOverExpiredAndCooldown()
    {
        // Organization active, holder suspended, card expired, cooldown active
        var context = new EligibilityContext(
            isOrganizationActive: true,
            organizationCooldownMinutes: null,
            defaultCooldownMinutes: 720,
            beneficiaryStatus: BeneficiaryStatus.Suspended,
            cardExpiryDate: _todayLocal.AddDays(-10),
            lastGrantedEntryUtc: _baseTimeUtc.AddMinutes(-10),
            nowUtc: _baseTimeUtc,
            localTodayDate: _todayLocal);

        var decision = _policy.Evaluate(context);

        decision.DenialReason.ShouldBe(DenialReason.HolderSuspended);
    }

    [Fact]
    public void Evaluate_CheckOrder_BR06_CardExpiredTakesPrecedenceOverCooldown()
    {
        // Organization active, holder active, but card expired and cooldown active
        var context = new EligibilityContext(
            isOrganizationActive: true,
            organizationCooldownMinutes: null,
            defaultCooldownMinutes: 720,
            beneficiaryStatus: BeneficiaryStatus.Active,
            cardExpiryDate: _todayLocal.AddDays(-10),
            lastGrantedEntryUtc: _baseTimeUtc.AddMinutes(-10),
            nowUtc: _baseTimeUtc,
            localTodayDate: _todayLocal);

        var decision = _policy.Evaluate(context);

        decision.DenialReason.ShouldBe(DenialReason.CardExpired);
    }

    [Theory]
    // When organization has specific cooldown, it overrides default (BR-02)
    [InlineData(360, 720, 360)]
    // When organization cooldown is null, default is used (BR-02)
    [InlineData(null, 720, 720)]
    public void Evaluate_EffectiveCooldownResolution_BR02(
        int? orgCooldown,
        int defaultCooldown,
        int expectedEffective)
    {
        var context = new EligibilityContext(
            isOrganizationActive: true,
            organizationCooldownMinutes: orgCooldown,
            defaultCooldownMinutes: defaultCooldown,
            beneficiaryStatus: BeneficiaryStatus.Active,
            cardExpiryDate: null,
            lastGrantedEntryUtc: null,
            nowUtc: _baseTimeUtc,
            localTodayDate: _todayLocal);

        var decision = _policy.Evaluate(context);

        decision.EffectiveCooldownMinutes.ShouldBe(expectedEffective);
    }

    [Fact]
    public void Evaluate_WithEntitiesConstructor_EvaluatesAccurately()
    {
        var org = new Organization("SSF", "صندوق التضامن", cooldownMinutes: 480);
        var beneficiary = new Beneficiary(
            Guid.NewGuid(),
            organizationId: 1,
            CardNumber.Create("SSF-001"),
            "سالم محمود",
            PhoneNumber.Create("0912345678"),
            cardExpiryDate: _todayLocal.AddMonths(1));

        beneficiary.RecordGrantedEntry(_baseTimeUtc.AddHours(-10)); // 10h ago > 480m (8h) cooldown

        var context = new EligibilityContext(org, beneficiary, defaultCooldownMinutes: 720, _baseTimeUtc, _todayLocal);
        var decision = _policy.Evaluate(context);

        decision.IsEligible.ShouldBeTrue();
        decision.EffectiveCooldownMinutes.ShouldBe(480);
    }
}
