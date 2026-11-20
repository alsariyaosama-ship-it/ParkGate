using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Domain.Policies;

public sealed record GiftEligibilityContext
{
    public bool IsOrganizationActive { get; }
    public bool GiftEnabled { get; }
    public int GiftIntervalDays { get; }
    public BeneficiaryStatus HolderStatus { get; }
    public DateTime? LastGiftIssuedUtc { get; }
    public bool HasEntryToday { get; }

    /// <summary>F8, BR-28: today is an entry day of the organization, or its gifts are given on any day.</summary>
    public bool IsGiftDay { get; }
    public DateTime NowUtc { get; }

    public GiftEligibilityContext(
        Organization organization,
        Beneficiary holder,
        DateTime? lastGiftIssuedUtc,
        DateTime nowUtc,
        bool hasEntryToday,
        DateOnly businessDate)
    {
        ArgumentNullException.ThrowIfNull(organization);
        ArgumentNullException.ThrowIfNull(holder);

        if (nowUtc.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("NowUtc must have UTC kind.", nameof(nowUtc));
        }

        IsOrganizationActive = organization.IsActive;
        GiftEnabled = organization.GiftEnabled;
        GiftIntervalDays = organization.GiftIntervalDays;
        HolderStatus = holder.Status;
        LastGiftIssuedUtc = lastGiftIssuedUtc;
        NowUtc = nowUtc;
        HasEntryToday = hasEntryToday;
        IsGiftDay = organization.GiftAnyDay || organization.IsEntryDay(businessDate);
    }
}
