using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Domain.Policies;

/// <summary>
/// Inputs required to evaluate entry eligibility for a card holder (BR-02..BR-06).
/// </summary>
public sealed record EligibilityContext
{
    public bool IsOrganizationActive { get; }
    public int? OrganizationCooldownMinutes { get; }
    public int DefaultCooldownMinutes { get; }
    public BeneficiaryStatus BeneficiaryStatus { get; }
    public DateOnly? CardExpiryDate { get; }
    public DateTime? LastGrantedEntryUtc { get; }
    public DateTime NowUtc { get; }
    public DateOnly LocalTodayDate { get; }

    public EligibilityContext(
        bool isOrganizationActive,
        int? organizationCooldownMinutes,
        int defaultCooldownMinutes,
        BeneficiaryStatus beneficiaryStatus,
        DateOnly? cardExpiryDate,
        DateTime? lastGrantedEntryUtc,
        DateTime nowUtc,
        DateOnly localTodayDate)
    {
        if (defaultCooldownMinutes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultCooldownMinutes), "Default cooldown must be positive.");
        }

        if (organizationCooldownMinutes.HasValue && organizationCooldownMinutes.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(organizationCooldownMinutes), "Organization cooldown must be positive.");
        }

        if (nowUtc.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Current timestamp must have UTC kind.", nameof(nowUtc));
        }

        if (lastGrantedEntryUtc.HasValue && lastGrantedEntryUtc.Value.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Last granted timestamp must have UTC kind.", nameof(lastGrantedEntryUtc));
        }

        IsOrganizationActive = isOrganizationActive;
        OrganizationCooldownMinutes = organizationCooldownMinutes;
        DefaultCooldownMinutes = defaultCooldownMinutes;
        BeneficiaryStatus = beneficiaryStatus;
        CardExpiryDate = cardExpiryDate;
        LastGrantedEntryUtc = lastGrantedEntryUtc;
        NowUtc = nowUtc;
        LocalTodayDate = localTodayDate;
    }

    public EligibilityContext(
        Organization organization,
        Beneficiary beneficiary,
        int defaultCooldownMinutes,
        DateTime nowUtc,
        DateOnly localTodayDate)
        : this(
            organization?.IsActive ?? throw new ArgumentNullException(nameof(organization)),
            organization.CooldownMinutes,
            defaultCooldownMinutes,
            beneficiary?.Status ?? throw new ArgumentNullException(nameof(beneficiary)),
            beneficiary.CardExpiryDate,
            beneficiary.LastGrantedEntryUtc,
            nowUtc,
            localTodayDate)
    {
    }
}
