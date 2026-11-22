using System.Globalization;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Printing;

public enum SlipTextStyle
{
    Company,
    Park,
    Emphasis,
    /// <summary>The person total of the entry receipt: the largest text on any slip.</summary>
    Total,
    Body,
    Small,
}

public abstract record SlipItem;

public sealed record SlipText(string Text, SlipTextStyle Style) : SlipItem;

/// <summary>The QR image; the printer draws the one it is given.</summary>
public sealed record SlipQr : SlipItem;

public sealed record ReceiptSlip(IReadOnlyList<SlipItem> Items);

/// <summary>
/// The owner's wording for the printed slips (R7.12, BR-33). Monochrome thermal paper: no logo, no colour.
/// The entry card carries no personal data; the gift voucher shows its number but not its value; the entry receipt
/// shows the person count, the time and the receipt number, never the holder's name or card.
/// </summary>
public static class ReceiptSlips
{
    private const int MinutesPerHour = 60;
    private const int MinutesPerDay = 24 * MinutesPerHour;
    private const string BreakdownSeparator = " · ";

    public static ReceiptSlip EntryCard(string organizationName, int cooldownMinutes) =>
        new([
            .. Header(organizationName),
            new SlipText(Strings.Slip_Welcome, SlipTextStyle.Emphasis),
            new SlipText(Strings.Slip_EntryTitle, SlipTextStyle.Body),
            new SlipText(string.Format(CultureInfo.InvariantCulture, Strings.Slip_EntryRule, Period(cooldownMinutes)), SlipTextStyle.Body),
        ]);

    public static ReceiptSlip GiftVoucher(string organizationName, int receiptNo) =>
        new([
            .. Header(organizationName),
            new SlipText(Strings.Slip_Welcome, SlipTextStyle.Emphasis),
            new SlipText(Strings.Slip_GiftCongrats, SlipTextStyle.Body),
            new SlipText(Strings.Slip_GiftValidToday, SlipTextStyle.Emphasis),
            new SlipText(Strings.Slip_GiftPickup, SlipTextStyle.Body),
            new SlipText(string.Format(CultureInfo.InvariantCulture, Strings.Slip_GiftNumber, receiptNo), SlipTextStyle.Small),
        ]);

    /// <summary>
    /// The visitor's entry receipt (T10.2, BR-33). <paramref name="countedPersons"/> is the holder plus the companions who
    /// count (infants do not); every companion with a count, infants included, appears in the breakdown.
    /// </summary>
    public static ReceiptSlip EntryReceipt(
        string organizationName,
        int countedPersons,
        IReadOnlyList<CompanionCountDto> companions,
        DateTime localTime,
        string deskName,
        int receiptNo)
    {
        var items = new List<SlipItem>(Header(organizationName, withQr: false))
        {
            new SlipText(string.Format(CultureInfo.InvariantCulture, Strings.Slip_EntryPersons, countedPersons), SlipTextStyle.Total),
            new SlipText(PersonsLine(countedPersons - 1), SlipTextStyle.Body),
        };

        var breakdown = companions.Where(c => c.Count > 0).Select(c => string.Create(CultureInfo.InvariantCulture, $"{c.CategoryName} {c.Count}")).ToList();
        if (breakdown.Count > 0)
        {
            items.Add(new SlipText(string.Format(CultureInfo.InvariantCulture, Strings.Slip_EntryBreakdown, string.Join(BreakdownSeparator, breakdown)), SlipTextStyle.Body));
        }

        items.Add(new SlipText($"{DateTimeDisplayHelper.FormatDate(localTime)}  {DateTimeDisplayHelper.FormatTime(localTime)}", SlipTextStyle.Body));
        items.Add(new SlipText(string.Format(CultureInfo.InvariantCulture, Strings.Slip_EntryDeskAndNumber, deskName, receiptNo), SlipTextStyle.Small));
        return new ReceiptSlip(items);
    }

    private static string PersonsLine(int countedCompanions) => countedCompanions <= 0
        ? Strings.Slip_EntryHolderOnly
        : Counted(countedCompanions, Strings.Slip_EntryWith_One, Strings.Slip_EntryWith_Two, Strings.Slip_EntryWith_Few, Strings.Slip_EntryWith_Many);

    /// <summary>
    /// «كل يوم», «كل يومين», «كل 3 أيام», «كل 30 يوماً»; hours when the period is not whole days (rounded up).
    /// </summary>
    public static string Period(int cooldownMinutes)
    {
        if (cooldownMinutes >= MinutesPerDay && cooldownMinutes % MinutesPerDay == 0)
        {
            return Counted(cooldownMinutes / MinutesPerDay,
                Strings.Slip_Every_Day, Strings.Slip_Every_TwoDays, Strings.Slip_Every_FewDays, Strings.Slip_Every_ManyDays);
        }

        var hours = Math.Max(1, (cooldownMinutes + MinutesPerHour - 1) / MinutesPerHour);
        return Counted(hours,
            Strings.Slip_Every_Hour, Strings.Slip_Every_TwoHours, Strings.Slip_Every_FewHours, Strings.Slip_Every_ManyHours);
    }

    private static IEnumerable<SlipItem> Header(string organizationName, bool withQr = true)
    {
        yield return new SlipText(Strings.Slip_CompanyName, SlipTextStyle.Company);
        yield return new SlipText(Strings.Slip_ParkName, SlipTextStyle.Park);
        if (withQr)
        {
            yield return new SlipQr();
        }

        yield return new SlipText(organizationName, SlipTextStyle.Emphasis);
    }

    // Arabic number agreement: 1 and 2 have their own words, 3-10 take the plural, 11 and up the singular.
    private static string Counted(int count, string one, string two, string few, string many) => count switch
    {
        1 => one,
        2 => two,
        <= 10 => string.Format(CultureInfo.InvariantCulture, few, count),
        _ => string.Format(CultureInfo.InvariantCulture, many, count),
    };
}
