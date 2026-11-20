using ParkGate.Domain.Enums;

namespace ParkGate.Domain.Policies;

public sealed class RollingIntervalGiftEligibilityPolicy : IGiftEligibilityPolicy
{
    public GiftEligibilityDecision Evaluate(GiftEligibilityContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // 1. Organization inactive
        if (!context.IsOrganizationActive)
        {
            return GiftEligibilityDecision.Denied(GiftDenialReason.OrganizationInactive);
        }

        // 2. Gifts not enabled
        if (!context.GiftEnabled)
        {
            return GiftEligibilityDecision.Denied(GiftDenialReason.GiftsNotEnabled);
        }

        // 3. Holder blocked
        if (context.HolderStatus == BeneficiaryStatus.Blocked)
        {
            return GiftEligibilityDecision.Denied(GiftDenialReason.HolderBlocked);
        }

        // 4. Holder suspended
        if (context.HolderStatus == BeneficiaryStatus.Suspended)
        {
            return GiftEligibilityDecision.Denied(GiftDenialReason.HolderSuspended);
        }

        // 5. The gift goes with today's visit: no entry today, no gift, and a supervisor cannot override it.
        if (!context.HasEntryToday)
        {
            return GiftEligibilityDecision.Denied(GiftDenialReason.NoEntryToday);
        }

        // 6. Not a gift day (BR-28). After every reason that cannot be overridden, because an override skips only the first.
        if (!context.IsGiftDay)
        {
            return GiftEligibilityDecision.Denied(GiftDenialReason.DayNotAllowed);
        }

        // 7. Interval check
        if (context.LastGiftIssuedUtc.HasValue)
        {
            var nextEligibleAtUtc = context.LastGiftIssuedUtc.Value.AddDays(context.GiftIntervalDays);

            if (context.NowUtc < nextEligibleAtUtc)
            {
                return GiftEligibilityDecision.Denied(GiftDenialReason.IntervalNotElapsed, nextEligibleAtUtc);
            }
        }

        return GiftEligibilityDecision.Eligible();
    }
}
