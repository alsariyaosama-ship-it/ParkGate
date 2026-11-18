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

    private static string[] Texts(ReceiptSlip slip) =>
        slip.Items.Select(i => i is SlipText t ? t.Text : "[QR]").ToArray();
}
