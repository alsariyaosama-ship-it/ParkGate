using ClosedXML.Excel;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.Ports.Reports;
using ParkGate.Domain.Enums;

namespace ParkGate.Infrastructure.Reports;

public sealed class ClosedXmlGiftExcelExportService : IGiftExcelExportService
{
    private static readonly XLColor PrimaryHeaderBg = XLColor.FromHtml("#1B873F");   // Park gate green
    private static readonly XLColor SecondaryHeaderBg = XLColor.FromHtml("#37474F"); // Slate gray
    private static readonly XLColor HeaderText = XLColor.White;
    private static readonly XLColor ZebraBg = XLColor.FromHtml("#F8FAFC");
    private static readonly XLColor TotalsBg = XLColor.FromHtml("#E8F5E9");

    public byte[] ExportGiftReceipts(GiftReceiptReportDto report, string? timeZoneId = null)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("قسائم الهدايا");
        ws.RightToLeft = true;

        var tz = ResolveTimeZone(timeZoneId);

        int row = 1;

        // Title
        ws.Cell(row, 1).Value = "تقرير قسائم ووصولات الهدايا";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Range(row, 1, row, 10).Merge();
        row += 2;

        // Meta info
        ws.Cell(row, 1).Value = "الفترة:";
        ws.Cell(row, 2).Value = $"من {report.FromDate:dd/MM/yyyy} إلى {report.ToDate:dd/MM/yyyy}";
        ws.Cell(row, 4).Value = "تصفية الحالة:";
        ws.Cell(row, 5).Value = GetFilterArabicName(report.Filter);
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        // KPI Summary block
        ws.Cell(row, 1).Value = "المؤشر";
        ws.Cell(row, 2).Value = "العدد";
        ws.Cell(row, 3).Value = "القيمة الإجمالية (د.ل)";
        StyleHeader(ws.Range(row, 1, row, 3), PrimaryHeaderBg);
        row++;

        foreach (var st in report.StatusTotals)
        {
            ws.Cell(row, 1).Value = GetStatusArabicName(st.Status);
            ws.Cell(row, 2).Value = st.Count;
            ws.Cell(row, 3).Value = st.TotalValue;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.000";
            StyleDataRow(ws, row, 3, false);
            row++;
        }

        // Total KPI Row
        ws.Cell(row, 1).Value = "الإجمالي العام";
        ws.Cell(row, 2).Value = report.TotalCount;
        ws.Cell(row, 3).Value = report.TotalValue;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.000";
        StyleTotalsRow(ws, row, 3);
        row += 2;

        // Receipts Table Header
        ws.Cell(row, 1).Value = "جدول وصولات وقسائم الهدايا";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 13;
        row++;

        var headers = new[]
        {
            "رقم الإيصال",
            "اسم المستفيد",
            "رقم البطاقة",
            "الجهة",
            "القيمة (د.ل)",
            "الحالة",
            "وقت الإصدار",
            "الموظف المُصدِر",
            "وقت الصرف",
            "موظف الصرف",
            "نقطة الصرف",
            "استثناء؟",
            "سبب الاستثناء / الإلغاء"
        };

        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cell(row, c + 1).Value = headers[c];
        }
        StyleHeader(ws.Range(row, 1, row, headers.Length), SecondaryHeaderBg);
        row++;

        bool zebra = false;
        foreach (var r in report.Rows)
        {
            ws.Cell(row, 1).Value = r.ReceiptNo;
            ws.Cell(row, 2).Value = r.HolderName;
            ws.Cell(row, 3).Value = r.CardNumber;
            ws.Cell(row, 4).Value = r.OrganizationName;
            ws.Cell(row, 5).Value = r.Value;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.000";
            ws.Cell(row, 6).Value = GetStatusArabicName(r.Status);
            ws.Cell(row, 7).Value = FormatLocalTime(r.IssuedAtUtc, tz);
            ws.Cell(row, 8).Value = r.IssuedByUserName;
            ws.Cell(row, 9).Value = r.RedeemedAtUtc.HasValue ? FormatLocalTime(r.RedeemedAtUtc.Value, tz) : "-";
            ws.Cell(row, 10).Value = r.RedeemedByUserName ?? "-";
            ws.Cell(row, 11).Value = r.RedeemedWorkstationName ?? "-";
            ws.Cell(row, 12).Value = r.IsOverride ? "نعم" : "لا";

            var reason = r.Status == GiftVoucherStatus.Voided
                ? r.VoidReason ?? "-"
                : (r.IsOverride ? r.OverrideReason ?? "-" : "-");
            ws.Cell(row, 13).Value = reason;

            StyleDataRow(ws, row, headers.Length, zebra);
            zebra = !zebra;
            row++;
        }

        // Totals row for main table
        ws.Cell(row, 1).Value = "الإجمالي";
        ws.Cell(row, 2).Value = $"{report.TotalCount} قسيمة";
        ws.Cell(row, 5).Value = report.TotalValue;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.000";
        StyleTotalsRow(ws, row, headers.Length);

        ws.Columns(1, headers.Length).AdjustToContents(10.0, 45.0);

        return SaveToBytes(workbook);
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }

    private static string FormatLocalTime(DateTime utcTime, TimeZoneInfo tz)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcTime, DateTimeKind.Utc), tz);
        return local.ToString("dd/MM/yyyy hh:mm tt");
    }

    private static string GetFilterArabicName(GiftReceiptFilter filter) => filter switch
    {
        GiftReceiptFilter.Redeemed => "المصروفة فقط",
        GiftReceiptFilter.Issued => "الصادرة (النشطة) فقط",
        GiftReceiptFilter.Voided => "الملغاة فقط",
        _ => "جميع القسائم"
    };

    private static string GetStatusArabicName(GiftVoucherStatus status) => status switch
    {
        GiftVoucherStatus.Issued => "صادرة (نشطة)",
        GiftVoucherStatus.Redeemed => "تم الصرف",
        GiftVoucherStatus.Voided => "ملغاة",
        _ => status.ToString()
    };

    private static void StyleHeader(IXLRange range, XLColor bgColor)
    {
        range.Style.Fill.BackgroundColor = bgColor;
        range.Style.Font.Bold = true;
        range.Style.Font.FontColor = HeaderText;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
    }

    private static void StyleDataRow(IXLWorksheet ws, int row, int colCount, bool zebra)
    {
        for (int c = 1; c <= colCount; c++)
        {
            var cell = ws.Cell(row, c);
            if (zebra)
            {
                cell.Style.Fill.BackgroundColor = ZebraBg;
            }
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.LightGray;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
    }

    private static void StyleTotalsRow(IXLWorksheet ws, int row, int colCount)
    {
        for (int c = 1; c <= colCount; c++)
        {
            var cell = ws.Cell(row, c);
            cell.Style.Fill.BackgroundColor = TotalsBg;
            cell.Style.Font.Bold = true;
            cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Double;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }
    }

    private static byte[] SaveToBytes(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
