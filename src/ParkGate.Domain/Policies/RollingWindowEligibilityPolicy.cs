using ParkGate.Domain.Enums;

namespace ParkGate.Domain.Policies;

/// <summary>
/// MVP implementation of entry eligibility based on a rolling time window (BR-02..BR-06).
/// </summary>
public sealed class RollingWindowEligibilityPolicy : IEntryEligibilityPolicy
{
    public EligibilityDecision Evaluate(EligibilityContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // BR-02: Effective cooldown
        var effectiveCooldownMinutes = context.OrganizationCooldownMinutes ?? context.DefaultCooldownMinutes;

        // BR-06 Check order:

        // 1. Organization inactive
        if (!context.IsOrganizationActive)
        {
            return EligibilityDecision.Denied(DenialReason.OrganizationInactive, effectiveCooldownMinutes);
        }

        // 2. Holder Blocked
        if (context.BeneficiaryStatus == BeneficiaryStatus.Blocked)
        {
            return EligibilityDecision.Denied(DenialReason.HolderBlocked, effectiveCooldownMinutes);
        }

        // 3. Not an entry day of the organization (BR-27). After "blocked" on purpose: an override skips only the
        // first reason, so every reason that cannot be overridden must come before this one.
        if (!context.AllowedEntryDays.Includes(context.LocalTodayDate.DayOfWeek))
        {
            return EligibilityDecision.Denied(DenialReason.DayNotAllowed, effectiveCooldownMinutes);
        }

        // 4. Holder Suspended
        if (context.BeneficiaryStatus == BeneficiaryStatus.Suspended)
        {
            return EligibilityDecision.Denied(DenialReason.HolderSuspended, effectiveCooldownMinutes);
        }

        // 5. Card expired (CardExpiryDate < today local)
        if (context.CardExpiryDate.HasValue && context.CardExpiryDate.Value < context.LocalTodayDate)
        {
            return EligibilityDecision.Denied(DenialReason.CardExpired, effectiveCooldownMinutes);
        }

        // 6. Cooldown check (BR-03, BR-04)
        if (context.LastGrantedEntryUtc.HasValue)
        {
            var nextEligibleAtUtc = context.LastGrantedEntryUtc.Value.AddMinutes(effectiveCooldownMinutes);
            if (context.NowUtc < nextEligibleAtUtc)
            {
                return EligibilityDecision.Denied(DenialReason.CooldownActive, effectiveCooldownMinutes, nextEligibleAtUtc);
            }
        }

        // Eligible
        var nextAllowedAtUtc = context.NowUtc.AddMinutes(effectiveCooldownMinutes);
        return EligibilityDecision.Allowed(effectiveCooldownMinutes, nextAllowedAtUtc);
    }
}
