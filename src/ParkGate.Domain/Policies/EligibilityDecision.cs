using ParkGate.Domain.Enums;

namespace ParkGate.Domain.Policies;

/// <summary>
/// Result of evaluating entry eligibility for a card holder (BR-02..BR-06, BR-11).
/// </summary>
public sealed record EligibilityDecision
{
    public bool IsEligible { get; }
    public DenialReason? DenialReason { get; }
    public int EffectiveCooldownMinutes { get; }
    public DateTime? NextEligibleAtUtc { get; }
    public bool CanBeOverridden { get; }

    private EligibilityDecision(
        bool isEligible,
        DenialReason? denialReason,
        int effectiveCooldownMinutes,
        DateTime? nextEligibleAtUtc,
        bool canBeOverridden)
    {
        IsEligible = isEligible;
        DenialReason = denialReason;
        EffectiveCooldownMinutes = effectiveCooldownMinutes;
        NextEligibleAtUtc = nextEligibleAtUtc;
        CanBeOverridden = canBeOverridden;
    }

    public static EligibilityDecision Allowed(int effectiveCooldownMinutes, DateTime nextEligibleAtUtc)
    {
        return new EligibilityDecision(
            isEligible: true,
            denialReason: null,
            effectiveCooldownMinutes: effectiveCooldownMinutes,
            nextEligibleAtUtc: nextEligibleAtUtc,
            canBeOverridden: false);
    }

    public static EligibilityDecision Denied(
        DenialReason reason,
        int effectiveCooldownMinutes,
        DateTime? nextEligibleAtUtc = null)
    {
        var canBeOverridden = CanOverride(reason);
        return new EligibilityDecision(
            isEligible: false,
            denialReason: reason,
            effectiveCooldownMinutes: effectiveCooldownMinutes,
            nextEligibleAtUtc: nextEligibleAtUtc,
            canBeOverridden: canBeOverridden);
    }

    /// <summary>
    /// Determines whether a denial reason can be overridden by a supervisor per BR-11.
    /// Allowed for: cooldown, expired card, suspended holder.
    /// Not allowed for: blocked holder, inactive organization.
    /// </summary>
    public static bool CanOverride(DenialReason reason)
    {
        return reason switch
        {
            Enums.DenialReason.CooldownActive => true,
            Enums.DenialReason.CardExpired => true,
            Enums.DenialReason.HolderSuspended => true,
            Enums.DenialReason.HolderBlocked => false,
            Enums.DenialReason.OrganizationInactive => false,
            _ => false
        };
    }
}
