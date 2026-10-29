using ParkGate.Domain.Enums;

namespace ParkGate.Domain.Policies;

public sealed record GiftEligibilityDecision
{
    public bool IsEligible { get; }
    public GiftDenialReason? Reason { get; }
    public DateTime? NextEligibleAtUtc { get; }
    public bool CanBeOverridden { get; }

    private GiftEligibilityDecision(bool isEligible, GiftDenialReason? reason, DateTime? nextEligibleAtUtc, bool canBeOverridden)
    {
        IsEligible = isEligible;
        Reason = reason;
        NextEligibleAtUtc = nextEligibleAtUtc;
        CanBeOverridden = canBeOverridden;
    }

    public static GiftEligibilityDecision Eligible() =>
        new(true, null, null, false);

    public static GiftEligibilityDecision Denied(GiftDenialReason reason, DateTime? nextEligibleAtUtc = null) =>
        new(false, reason, nextEligibleAtUtc, reason == GiftDenialReason.IntervalNotElapsed);
}
