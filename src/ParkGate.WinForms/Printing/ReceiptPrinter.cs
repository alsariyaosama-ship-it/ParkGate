using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using ParkGate.WinForms.Common;

namespace ParkGate.WinForms.Printing;

/// <summary>
/// Prints a <see cref="ReceiptSlip"/> on 80 mm cashier paper (R7.12). Units are hundredths of an inch, the
/// printer's page unit. The page is as long as its content plus blank paper for the automatic cutter.
/// </summary>
public static class ReceiptPrinter
{
    public const int PaperWidth = 315;      // 80 mm
    public const int SideMargin = 16;       // 4 mm
    public const int TopMargin = 16;        // 4 mm
    public const int PrintableWidth = PaperWidth - (2 * SideMargin);
    public const int QrSize = 200;          // about 51 mm, large enough for a desk scanner
    public const int ItemGap = 8;           // 2 mm
    public const int CutterFeed = 80;       // 20 mm of blank paper so the cut does not touch the text

    private const string PaperName = "Receipt 80 mm";

    public sealed record PlacedItem(SlipItem Item, int Top, int Height);

    public sealed record SlipLayout(IReadOnlyList<PlacedItem> Items, int PageHeight);

    /// <summary>Stacks the items top to bottom; <paramref name="measureText"/> returns a text's height.</summary>
    public static SlipLayout Layout(ReceiptSlip slip, Func<SlipText, int> measureText)
    {
        var placed = new List<PlacedItem>();
        var top = TopMargin;
        foreach (var item in slip.Items)
        {
            var height = item is SlipText text ? measureText(text) : QrSize;
            placed.Add(new PlacedItem(item, top, height));
            top += height + ItemGap;
        }

        var last = placed[^1];
        return new SlipLayout(placed, last.Top + last.Height + CutterFeed);
    }

    /// <summary>A document sized to the slip; the caller shows the print dialog and disposes it.</summary>
    public static PrintDocument CreateDocument(ReceiptSlip slip, Image qr)
    {
        var layout = Layout(slip, MeasureOnScreen);
        var document = new PrintDocument { DocumentName = PaperName, OriginAtMargins = false };
        document.DefaultPageSettings.PaperSize = new PaperSize(PaperName, PaperWidth, layout.PageHeight);
        document.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
        document.PrintPage += (_, e) =>
        {
            if (e.Graphics != null)
            {
                Draw(e.Graphics, layout, qr);
            }
        };
        return document;
    }

    private static void Draw(Graphics g, SlipLayout layout, Image qr)
    {
        g.InterpolationMode = InterpolationMode.NearestNeighbor; // crisp QR modules
        g.PixelOffsetMode = PixelOffsetMode.Half;
        using var format = CenteredRtl();
        foreach (var placed in layout.Items)
        {
            if (placed.Item is SlipText text)
            {
                using var font = FontFor(text.Style);
                var area = new RectangleF(SideMargin, placed.Top, PrintableWidth, placed.Height);
                g.DrawString(text.Text, font, Brushes.Black, area, format);
            }
            else
            {
                g.DrawImage(qr, (PaperWidth - QrSize) / 2, placed.Top, QrSize, QrSize);
            }
        }
    }

    private static int MeasureOnScreen(SlipText text)
    {
        using var bitmap = new Bitmap(1, 1);
        using var g = Graphics.FromImage(bitmap);
        g.PageUnit = GraphicsUnit.Inch;
        using var font = FontFor(text.Style);
        using var format = CenteredRtl();
        var size = g.MeasureString(text.Text, font, new SizeF(PrintableWidth / 100f, 100f), format);
        return (int)Math.Ceiling(size.Height * 100);
    }

    private static StringFormat CenteredRtl() => new(StringFormatFlags.DirectionRightToLeft)
    {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Near,
        Trimming = StringTrimming.Word,
    };

    private static Font FontFor(SlipTextStyle style) => style switch
    {
        SlipTextStyle.Company => AppFont.CreateFont(10F, FontStyle.Bold),
        SlipTextStyle.Park => AppFont.CreateFont(12F, FontStyle.Bold),
        SlipTextStyle.Emphasis => AppFont.CreateFont(10F, FontStyle.Bold),
        SlipTextStyle.Small => AppFont.CreateFont(9F, FontStyle.Regular),
        _ => AppFont.CreateFont(10F, FontStyle.Regular),
    };
}
