using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.WinForms.Printing;
using ParkGate.WinForms.Resources;

namespace ParkGate.ArchitectureTests;

/// <summary>
/// R7.12: the entry card and the gift voucher print on 80 mm cashier paper with the owner's wording.
/// </summary>
public sealed class ReceiptSlipsTests
{
    [Fact]
    public void EntryCard_PrintsHeaderQrOrganizationThenTheCardText()
    {
        var slip = ReceiptSlips.EntryCard("صندوق التضامن الاجتماعي", cooldownMinutes: 30 * 24 * 60);

        Texts(slip).ShouldBe(
        [
            Strings.Slip_CompanyName,
            Strings.Slip_ParkName,
            "[QR]",
            "صندوق التضامن الاجتماعي",
            Strings.Slip_Welcome,
            Strings.Slip_EntryTitle,
            string.Format(Strings.Slip_EntryRule, "كل 30 يوماً"),
        ]);
    }

    [Theory]
    [InlineData(24 * 60, "كل يوم")]
    [InlineData(2 * 24 * 60, "كل يومين")]
    [InlineData(3 * 24 * 60, "كل 3 أيام")]
    [InlineData(10 * 24 * 60, "كل 10 أيام")]
    [InlineData(30 * 24 * 60, "كل 30 يوماً")]
    [InlineData(60, "كل ساعة")]
    [InlineData(120, "كل ساعتين")]
    [InlineData(5 * 60, "كل 5 ساعات")]
    [InlineData(12 * 60, "كل 12 ساعة")]
    [InlineData(90, "كل ساعتين")]
    public void Period_IsWrittenInArabic(int minutes, string expected)
    {
        ReceiptSlips.Period(minutes).ShouldBe(expected);
    }

    [Fact]
    public void GiftVoucher_PrintsTheVoucherNumberButNoValue()
    {
        var slip = ReceiptSlips.GiftVoucher("صندوق التضامن الاجتماعي", receiptNo: 123);

        Texts(slip).ShouldBe(
        [
            Strings.Slip_CompanyName,
            Strings.Slip_ParkName,
            "[QR]",
            "صندوق التضامن الاجتماعي",
            Strings.Slip_Welcome,
            Strings.Slip_GiftCongrats,
            Strings.Slip_GiftValidToday,
            Strings.Slip_GiftPickup,
            string.Format(Strings.Slip_GiftNumber, 123),
        ]);
    }

    [Fact]
    public void Layout_Uses80mmPaperWithMarginsAndRoomForTheCutter()
    {
        var slip = ReceiptSlips.GiftVoucher("جهة", receiptNo: 1);

        // Every text line measures 20 hundredths of an inch here.
        var layout = ReceiptPrinter.Layout(slip, _ => 20);

        ReceiptPrinter.PaperWidth.ShouldBe(315);           // 80 mm
        ReceiptPrinter.PrintableWidth.ShouldBe(283);       // 72 mm between the 4 mm side margins
        layout.Items[0].Top.ShouldBe(ReceiptPrinter.TopMargin);
        layout.Items.Single(i => i.Item is SlipQr).Height.ShouldBe(ReceiptPrinter.QrSize);
        layout.PageHeight.ShouldBe(layout.Items[^1].Top + layout.Items[^1].Height + ReceiptPrinter.CutterFeed);
    }

    // ── T10.2, BR-33: the entry receipt ─────────────────────────────────────

    private static readonly DateTime ReceiptTime = new(2026, 11, 22, 15, 5, 0);

    private static CompanionCountDto[] Companions(int adults = 0, int children = 0, int infants = 0) =>
        [new(1, "بالغ", adults), new(2, "طفل", children), new(3, "رضيع", infants)];

    private static ReceiptSlip Receipt(int countedPersons, CompanionCountDto[] companions) =>
        ReceiptSlips.EntryReceipt("صندوق التضامن الاجتماعي", countedPersons, companions, ReceiptTime, "شباك 1", receiptNo: 57);

    [Fact]
    public void EntryReceipt_HolderWithAdultChildAndInfant_ListsEverythingInOrder()
    {
        var slip = Receipt(countedPersons: 3, Companions(adults: 1, children: 1, infants: 1));

        Texts(slip).ShouldBe(
        [
            Strings.Slip_CompanyName,
            Strings.Slip_ParkName,
            "صندوق التضامن الاجتماعي",
            "عدد الأشخاص: 3",
            "صاحب البطاقة + مرافقان",
            "المرافقون: بالغ 1 · طفل 1 · رضيع 1",
            "22/11/2026  03:05 م",
            "شباك 1   واصل رقم 57",
        ]);
    }

    [Fact]
    public void EntryReceipt_UsesTheLargestFontStyleForTheTotalAndHasNoQr()
    {
        var slip = Receipt(3, Companions(adults: 2));

        slip.Items.OfType<SlipText>().Single(t => t.Text.StartsWith("عدد الأشخاص")).Style.ShouldBe(SlipTextStyle.Total);
        slip.Items.OfType<SlipText>().Single(t => t.Text == "شباك 1   واصل رقم 57").Style.ShouldBe(SlipTextStyle.Small);
        slip.Items.ShouldNotContain(i => i is SlipQr);
    }

    [Theory]
    [InlineData(0, "صاحب البطاقة فقط")]
    [InlineData(1, "صاحب البطاقة + مرافق")]
    [InlineData(2, "صاحب البطاقة + مرافقان")]
    [InlineData(3, "صاحب البطاقة + 3 مرافقين")]
    [InlineData(10, "صاحب البطاقة + 10 مرافقين")]
    [InlineData(11, "صاحب البطاقة + 11 مرافقًا")]
    public void EntryReceipt_WritesTheCompanionCountWithArabicAgreement(int countedCompanions, string expected)
    {
        var slip = Receipt(1 + countedCompanions, Companions(adults: countedCompanions));

        Texts(slip)[3].ShouldBe($"عدد الأشخاص: {1 + countedCompanions}");
        Texts(slip)[4].ShouldBe(expected);
    }

    [Fact]
    public void EntryReceipt_WithoutCompanions_HasNoBreakdownLine()
    {
        var slip = Receipt(1, []);

        Texts(slip).ShouldBe(
        [
            Strings.Slip_CompanyName,
            Strings.Slip_ParkName,
            "صندوق التضامن الاجتماعي",
            "عدد الأشخاص: 1",
            "صاحب البطاقة فقط",
            "22/11/2026  03:05 م",
            "شباك 1   واصل رقم 57",
        ]);
    }

    [Fact]
    public void EntryReceipt_InfantsOnly_CountsTheHolderAloneButStillListsTheInfants()
    {
        var slip = Receipt(1, Companions(infants: 2));

        Texts(slip)[3].ShouldBe("عدد الأشخاص: 1");
        Texts(slip)[4].ShouldBe("صاحب البطاقة فقط");
        Texts(slip)[5].ShouldBe("المرافقون: رضيع 2");
    }

    [Fact]
    public void EntryReceipt_ShowsTheMorningTimeWithTheSabahDesignator()
    {
        var slip = ReceiptSlips.EntryReceipt("جهة", 1, [], new DateTime(2026, 1, 5, 9, 30, 0), "شباك 2", 1);

        Texts(slip).ShouldContain("05/01/2026  09:30 ص");
    }

    [Fact]
    public void EntryCardAndGiftVoucher_KeepTheirQrAndTheirHeader()
    {
        ReceiptSlips.EntryCard("جهة", 1440).Items.Count(i => i is SlipQr).ShouldBe(1);
        ReceiptSlips.GiftVoucher("جهة", 1).Items.Count(i => i is SlipQr).ShouldBe(1);
        ReceiptSlips.EntryCard("جهة", 1440).Items[2].ShouldBeOfType<SlipQr>();
    }

    [Fact]
    public void Layout_WithoutQr_StacksOnlyTheTexts()
    {
        var slip = Receipt(3, Companions(adults: 2));

        var layout = ReceiptPrinter.Layout(slip, _ => 20);

        layout.Items.Count.ShouldBe(slip.Items.Count);
        layout.Items.ShouldAllBe(i => i.Height == 20);
        layout.PageHeight.ShouldBe(layout.Items[^1].Top + 20 + ReceiptPrinter.CutterFeed);
    }

    [Fact]
    public void CreateDocument_WithoutQr_BuildsAnEntryReceiptDocument()
    {
        using var document = ReceiptPrinter.CreateDocument(Receipt(3, Companions(adults: 2)), qr: null);

        document.DefaultPageSettings.PaperSize.Width.ShouldBe(ReceiptPrinter.PaperWidth);
    }

    [Fact]
    public void CreateDocument_QrSlipWithoutImage_IsRejected()
    {
        Should.Throw<ArgumentException>(() => ReceiptPrinter.CreateDocument(ReceiptSlips.GiftVoucher("جهة", 1), qr: null));
    }

    private static string[] Texts(ReceiptSlip slip) =>
        slip.Items.Select(i => i is SlipText t ? t.Text : "[QR]").ToArray();
}
