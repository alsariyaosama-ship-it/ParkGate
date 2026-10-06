using ClosedXML.Excel;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.Ports.Reports;
using ParkGate.Application.Reports.DTOs;
using ParkGate.Domain.Enums;

namespace ParkGate.Infrastructure.Reports;

public sealed class ClosedXmlExcelExportService : IExcelExportService
{
    private static readonly XLColor PrimaryHeaderBg = XLColor.FromHtml("#1B873F"); // Park gate green
    private static readonly XLColor SecondaryHeaderBg = XLColor.FromHtml("#37474F"); // Slate gray
    private static readonly XLColor HeaderText = XLColor.White;
    private static readonly XLColor ZebraBg = XLColor.FromHtml("#F8FAFC");
    private static readonly XLColor TotalsBg = XLColor.FromHtml("#E8F5E9");

    public byte[] ExportDailySummary(DailySummaryReportDto report, IReadOnlyList<AgeCategoryDto> categories)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("الملخص اليومي");
        ws.RightToLeft = true;

        int row = 1;

        // Title
        ws.Cell(row, 1).Value = "تقرير ملخص حركة الدخول اليومي";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Range(row, 1, row, 6).Merge();
        row += 2;

        // Meta info
        ws.Cell(row, 1).Value = "تاريخ العمل:";
        ws.Cell(row, 2).Value = report.Date.ToString("dd/MM/yyyy");
        if (!string.IsNullOrWhiteSpace(report.OrganizationName))
        {
            ws.Cell(row, 3).Value = "الجهة:";
            ws.Cell(row, 4).Value = report.OrganizationName;
        }
        if (!string.IsNullOrWhiteSpace(report.WorkstationName))
        {
            ws.Cell(row, 5).Value = "نقطة الدخول:";
            ws.Cell(row, 6).Value = report.WorkstationName;
        }
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        // KPI Summary Table
        ws.Cell(row, 1).Value = "المؤشر";
        ws.Cell(row, 2).Value = "القيمة";
        StyleHeader(ws.Range(row, 1, row, 2), PrimaryHeaderBg);
        row++;

        AddKpiRow(ws, ref row, "إجمالي محاولات الدخول", report.TotalAttempts);
        AddKpiRow(ws, ref row, "الدخول المقبول (عادي)", report.AllowedEntries);
        AddKpiRow(ws, ref row, "الدخول المقبول (استثناء)", report.OverriddenEntries);
        AddKpiRow(ws, ref row, "إجمالي الزيارات المقبولة", report.TotalVisits, true);
        AddKpiRow(ws, ref row, "المحاولات المرفوضة", report.DeniedEntries);
        AddKpiRow(ws, ref row, "الزيارات الملغاة", report.VoidedEntries);
        AddKpiRow(ws, ref row, "إجمالي حاملي البطاقات", report.TotalHolders);
        AddKpiRow(ws, ref row, "إجمالي المرافقين", report.TotalCompanions);
        AddKpiRow(ws, ref row, "إجمالي الأفراد (الزوار الفعليين)", report.TotalPersons, true);

        // Age Category breakdown table
        if (report.AgeCategoryBreakdown.Count > 0)
        {
            row += 2;
            ws.Cell(row, 1).Value = "تفصيل المرافقين حسب الفئات العمرية";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 13;
            row++;

            ws.Cell(row, 1).Value = "الفئة العمرية";
            ws.Cell(row, 2).Value = "عدد المرافقين";
            StyleHeader(ws.Range(row, 1, row, 2), SecondaryHeaderBg);
            row++;

            foreach (var cat in report.AgeCategoryBreakdown)
            {
                ws.Cell(row, 1).Value = cat.CategoryName;
                ws.Cell(row, 2).Value = cat.Count;
                StyleDataRow(ws, row, 2, false);
                row++;
            }
        }

        // Organizations breakdown table
        if (report.OrganizationBreakdown.Count > 0)
        {
            row += 2;
            ws.Cell(row, 1).Value = "تفصيل الحركة حسب الجهات";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 13;
            row++;

            string[] orgHeaders = ["الجهة", "الكود", "مقبول", "استثناء", "إجمالي الزيارات", "المرافقون", "إجمالي الأفراد"];
            for (int c = 0; c < orgHeaders.Length; c++)
            {
                ws.Cell(row, c + 1).Value = orgHeaders[c];
            }
            StyleHeader(ws.Range(row, 1, row, orgHeaders.Length), PrimaryHeaderBg);
            row++;

            bool zebra = false;
            foreach (var org in report.OrganizationBreakdown)
            {
                ws.Cell(row, 1).Value = org.OrganizationName;
                ws.Cell(row, 2).Value = org.OrganizationCode;
                ws.Cell(row, 3).Value = org.AllowedVisits;
                ws.Cell(row, 4).Value = org.OverriddenVisits;
                ws.Cell(row, 5).Value = org.TotalVisits;
                ws.Cell(row, 6).Value = org.TotalCompanions;
                ws.Cell(row, 7).Value = org.TotalPersons;

                StyleDataRow(ws, row, 7, zebra);
                zebra = !zebra;
                row++;
            }
        }

        ws.Columns().AdjustToContents();
        return SaveToBytes(workbook);
    }

    public byte[] ExportDateRangeSummary(DateRangeSummaryReportDto report, IReadOnlyList<AgeCategoryDto> categories)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("ملخص الفترة");
        ws.RightToLeft = true;

        int row = 1;
        ws.Cell(row, 1).Value = "تقرير ملخص حركة الدخول حسب الأيام";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Range(row, 1, row, 8).Merge();
        row += 2;

        ws.Cell(row, 1).Value = $"الفترة من: {report.FromDate:dd/MM/yyyy} إلى: {report.ToDate:dd/MM/yyyy}";
        if (!string.IsNullOrWhiteSpace(report.OrganizationName))
        {
            ws.Cell(row, 4).Value = $"الجهة: {report.OrganizationName}";
        }
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        // Build headers
        var headers = new List<string>
        {
            "تاريخ العمل", "اليوم", "المحاولات", "مقبول", "استثناء", "إجمالي الزيارات",
            "مرفوض", "ملغى", "حاملو البطاقات", "المرافقون"
        };
        foreach (var cat in categories)
        {
            headers.Add(cat.NameAr);
        }
        headers.Add("إجمالي الأفراد");

        for (int c = 0; c < headers.Count; c++)
        {
            ws.Cell(row, c + 1).Value = headers[c];
        }
        StyleHeader(ws.Range(row, 1, row, headers.Count), PrimaryHeaderBg);
        row++;

        bool zebra = false;
        foreach (var r in report.Rows)
        {
            int col = 1;
            ws.Cell(row, col++).Value = r.BusinessDate.ToString("dd/MM/yyyy");
            ws.Cell(row, col++).Value = r.DayOfWeekArabic;
            ws.Cell(row, col++).Value = r.TotalAttempts;
            ws.Cell(row, col++).Value = r.AllowedVisits;
            ws.Cell(row, col++).Value = r.OverriddenVisits;
            ws.Cell(row, col++).Value = r.TotalVisits;
            ws.Cell(row, col++).Value = r.DeniedAttempts;
            ws.Cell(row, col++).Value = r.VoidedAttempts;
            ws.Cell(row, col++).Value = r.TotalHolders;
            ws.Cell(row, col++).Value = r.TotalCompanions;

            foreach (var cat in categories)
            {
                var count = r.CompanionsByAgeCategory.FirstOrDefault(x => x.CategoryId == cat.Id)?.Count ?? 0;
                ws.Cell(row, col++).Value = count;
            }

            ws.Cell(row, col++).Value = r.TotalPersons;

            StyleDataRow(ws, row, headers.Count, zebra);
            zebra = !zebra;
            row++;
        }

        // Totals Row
        int totCol = 1;
        ws.Cell(row, totCol++).Value = "الإجمالي العام";
        ws.Cell(row, totCol++).Value = "-";
        ws.Cell(row, totCol++).Value = report.Totals.TotalAttempts;
        ws.Cell(row, totCol++).Value = report.Totals.AllowedVisits;
        ws.Cell(row, totCol++).Value = report.Totals.OverriddenVisits;
        ws.Cell(row, totCol++).Value = report.Totals.TotalVisits;
        ws.Cell(row, totCol++).Value = report.Totals.DeniedAttempts;
        ws.Cell(row, totCol++).Value = report.Totals.VoidedAttempts;
        ws.Cell(row, totCol++).Value = report.Totals.TotalHolders;
        ws.Cell(row, totCol++).Value = report.Totals.TotalCompanions;

        foreach (var cat in categories)
        {
            var count = report.Totals.CompanionsByAgeCategory.FirstOrDefault(x => x.CategoryId == cat.Id)?.Count ?? 0;
            ws.Cell(row, totCol++).Value = count;
        }

        ws.Cell(row, totCol++).Value = report.Totals.TotalPersons;
        StyleTotalsRow(ws, row, headers.Count);

        ws.Columns().AdjustToContents();
        return SaveToBytes(workbook);
    }

    public byte[] ExportOrganizationStatistics(OrganizationStatisticsReportDto report, IReadOnlyList<AgeCategoryDto> categories)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("إحصائيات الجهات");
        ws.RightToLeft = true;

        int row = 1;
        ws.Cell(row, 1).Value = "تقرير إحصائيات الجهات التضامنية الشريكة";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Range(row, 1, row, 8).Merge();
        row += 2;

        ws.Cell(row, 1).Value = $"الفترة من: {report.FromDate:dd/MM/yyyy} إلى: {report.ToDate:dd/MM/yyyy}";
        ws.Cell(row, 4).Value = $"إجمالي زوار الحديقة في الفترة: {report.TotalParkPersons:N0}";
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        var headers = new List<string>
        {
            "الجهة", "الكود", "عدد الزيارات", "المستفيدون الفريدون", "حاملو البطاقات", "المرافقون"
        };
        foreach (var cat in categories)
        {
            headers.Add(cat.NameAr);
        }
        headers.Add("إجمالي الأفراد");
        headers.Add("النسبة %");

        for (int c = 0; c < headers.Count; c++)
        {
            ws.Cell(row, c + 1).Value = headers[c];
        }
        StyleHeader(ws.Range(row, 1, row, headers.Count), PrimaryHeaderBg);
        row++;

        bool zebra = false;
        foreach (var r in report.Rows)
        {
            int col = 1;
            ws.Cell(row, col++).Value = r.OrganizationName;
            ws.Cell(row, col++).Value = r.OrganizationCode;
            ws.Cell(row, col++).Value = r.TotalVisits;
            ws.Cell(row, col++).Value = r.UniqueBeneficiariesCount;
            ws.Cell(row, col++).Value = r.TotalHolders;
            ws.Cell(row, col++).Value = r.TotalCompanions;

            foreach (var cat in categories)
            {
                var count = r.CompanionsByAgeCategory.FirstOrDefault(x => x.CategoryId == cat.Id)?.Count ?? 0;
                ws.Cell(row, col++).Value = count;
            }

            ws.Cell(row, col++).Value = r.TotalPersons;
            ws.Cell(row, col++).Value = $"{r.PercentageShare:F1}%";

            StyleDataRow(ws, row, headers.Count, zebra);
            zebra = !zebra;
            row++;
        }

        // Totals Row
        int totCol = 1;
        ws.Cell(row, totCol++).Value = "الإجمالي العام";
        ws.Cell(row, totCol++).Value = "-";
        ws.Cell(row, totCol++).Value = report.Totals.TotalVisits;
        ws.Cell(row, totCol++).Value = report.Totals.UniqueBeneficiariesCount;
        ws.Cell(row, totCol++).Value = report.Totals.TotalHolders;
        ws.Cell(row, totCol++).Value = report.Totals.TotalCompanions;

        foreach (var cat in categories)
        {
            var count = report.Totals.CompanionsByAgeCategory.FirstOrDefault(x => x.CategoryId == cat.Id)?.Count ?? 0;
            ws.Cell(row, totCol++).Value = count;
        }

        ws.Cell(row, totCol++).Value = report.Totals.TotalPersons;
        ws.Cell(row, totCol++).Value = report.TotalParkPersons > 0 ? "100.0%" : "0.0%";
        StyleTotalsRow(ws, row, headers.Count);

        ws.Columns().AdjustToContents();
        return SaveToBytes(workbook);
    }

    public byte[] ExportOperatorWorkstationStatistics(OperatorWorkstationReportDto report)
    {
        using var workbook = new XLWorkbook();

        // 1. Operators sheet
        var wsOp = workbook.Worksheets.Add("إحصائيات المشغلين");
        wsOp.RightToLeft = true;

        int row = 1;
        wsOp.Cell(row, 1).Value = $"إحصائيات المشغلين للفترة ({report.FromDate:dd/MM/yyyy} - {report.ToDate:dd/MM/yyyy})";
        wsOp.Cell(row, 1).Style.Font.Bold = true;
        wsOp.Cell(row, 1).Style.Font.FontSize = 14;
        row += 2;

        string[] opHeaders = ["المشغل", "اسم المستخدم", "المحاولات", "مقبول", "استثناء", "إجمالي الزيارات", "مرفوض", "ملغى", "إجمالي الأفراد"];
        for (int c = 0; c < opHeaders.Length; c++)
        {
            wsOp.Cell(row, c + 1).Value = opHeaders[c];
        }
        StyleHeader(wsOp.Range(row, 1, row, opHeaders.Length), PrimaryHeaderBg);
        row++;

        bool zebra = false;
        foreach (var r in report.OperatorRows)
        {
            wsOp.Cell(row, 1).Value = r.FullName;
            wsOp.Cell(row, 2).Value = r.UserName;
            wsOp.Cell(row, 3).Value = r.TotalAttempts;
            wsOp.Cell(row, 4).Value = r.AllowedEntries;
            wsOp.Cell(row, 5).Value = r.OverriddenEntries;
            wsOp.Cell(row, 6).Value = r.TotalVisits;
            wsOp.Cell(row, 7).Value = r.DeniedEntries;
            wsOp.Cell(row, 8).Value = r.VoidedEntries;
            wsOp.Cell(row, 9).Value = r.TotalPersons;

            StyleDataRow(wsOp, row, opHeaders.Length, zebra);
            zebra = !zebra;
            row++;
        }
        wsOp.Columns().AdjustToContents();

        // 2. Workstations sheet
        var wsWs = workbook.Worksheets.Add("إحصائيات نقاط الدخول");
        wsWs.RightToLeft = true;

        row = 1;
        wsWs.Cell(row, 1).Value = $"إحصائيات نقاط الدخول للفترة ({report.FromDate:dd/MM/yyyy} - {report.ToDate:dd/MM/yyyy})";
        wsWs.Cell(row, 1).Style.Font.Bold = true;
        wsWs.Cell(row, 1).Style.Font.FontSize = 14;
        row += 2;

        string[] wsHeaders = ["نقطة الدخول", "اسم الجهاز", "المحاولات", "مقبول", "استثناء", "إجمالي الزيارات", "مرفوض", "ملغى", "إجمالي الأفراد"];
        for (int c = 0; c < wsHeaders.Length; c++)
        {
            wsWs.Cell(row, c + 1).Value = wsHeaders[c];
        }
        StyleHeader(wsWs.Range(row, 1, row, wsHeaders.Length), SecondaryHeaderBg);
        row++;

        zebra = false;
        foreach (var r in report.WorkstationRows)
        {
            wsWs.Cell(row, 1).Value = r.DisplayName;
            wsWs.Cell(row, 2).Value = r.MachineName;
            wsWs.Cell(row, 3).Value = r.TotalAttempts;
            wsWs.Cell(row, 4).Value = r.AllowedEntries;
            wsWs.Cell(row, 5).Value = r.OverriddenEntries;
            wsWs.Cell(row, 6).Value = r.TotalVisits;
            wsWs.Cell(row, 7).Value = r.DeniedEntries;
            wsWs.Cell(row, 8).Value = r.VoidedEntries;
            wsWs.Cell(row, 9).Value = r.TotalPersons;

            StyleDataRow(wsWs, row, wsHeaders.Length, zebra);
            zebra = !zebra;
            row++;
        }
        wsWs.Columns().AdjustToContents();

        return SaveToBytes(workbook);
    }

    public byte[] ExportDenialsAndOverrides(DenialsAndOverridesReportDto report, string? timeZoneId = null)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("المرفوضات والاستثناءات");
        ws.RightToLeft = true;

        int row = 1;
        ws.Cell(row, 1).Value = "سجل محاولات الدخول المرفوضة والاستثناءات المعتمدة";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Range(row, 1, row, 8).Merge();
        row += 2;

        ws.Cell(row, 1).Value = $"الفترة: {report.FromDate:dd/MM/yyyy} - {report.ToDate:dd/MM/yyyy}";
        ws.Cell(row, 4).Value = $"النوع: {(report.Filter == DenialOverrideFilter.DeniedOnly ? "مرفوض فقط" : report.Filter == DenialOverrideFilter.OverriddenOnly ? "استثناءات فقط" : "الكل")}";
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        string[] headers = [
            "وقت المحاولة", "تاريخ العمل", "رقم البطاقة", "اسم المستفيد", "الجهة",
            "النتيجة", "سبب الرفض", "سبب الاستثناء", "المشرف المعتمد", "المشغل", "نقطة الدخول", "الأفراد"
        ];
        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cell(row, c + 1).Value = headers[c];
        }
        StyleHeader(ws.Range(row, 1, row, headers.Length), PrimaryHeaderBg);
        row++;

        bool zebra = false;
        foreach (var itm in report.Items)
        {
            ws.Cell(row, 1).Value = itm.AttemptedAtUtc.ToString("dd/MM/yyyy hh:mm tt");
            ws.Cell(row, 2).Value = itm.BusinessDate.ToString("dd/MM/yyyy");
            ws.Cell(row, 3).Value = itm.CardNumber;
            ws.Cell(row, 4).Value = itm.BeneficiaryName;
            ws.Cell(row, 5).Value = itm.OrganizationName;
            ws.Cell(row, 6).Value = itm.Result == EntryResult.Denied ? "مرفوض" : "استثناء";
            ws.Cell(row, 7).Value = itm.DenialReason.HasValue ? GetDenialReasonArabic(itm.DenialReason.Value) : "-";
            ws.Cell(row, 8).Value = itm.OverrideReason ?? "-";
            ws.Cell(row, 9).Value = itm.SupervisorName ?? "-";
            ws.Cell(row, 10).Value = itm.OperatorName;
            ws.Cell(row, 11).Value = itm.WorkstationName;
            ws.Cell(row, 12).Value = itm.TotalPersons;

            StyleDataRow(ws, row, headers.Length, zebra);
            zebra = !zebra;
            row++;
        }

        ws.Columns().AdjustToContents();
        return SaveToBytes(workbook);
    }

    public byte[] ExportHourlyDistribution(HourlyDistributionReportDto report)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("التوزيع الساعي");
        ws.RightToLeft = true;

        int row = 1;
        ws.Cell(row, 1).Value = "تقرير التوزيع الساعي لحركة الدخول (ساعات الذروة)";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Range(row, 1, row, 6).Merge();
        row += 2;

        ws.Cell(row, 1).Value = $"الفترة: {report.FromDate:dd/MM/yyyy} - {report.ToDate:dd/MM/yyyy}";
        ws.Cell(row, 4).Value = $"إجمالي الزيارات: {report.TotalVisits:N0} | إجمالي الأفراد: {report.TotalPersons:N0}";
        ws.Row(row).Style.Font.Bold = true;
        row += 2;

        string[] headers = ["الساعة", "الفترة الزمنية", "عدد الزيارات", "إجمالي الأفراد", "متوسط حجم المجموعة", "النسبة %"];
        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cell(row, c + 1).Value = headers[c];
        }
        StyleHeader(ws.Range(row, 1, row, headers.Length), PrimaryHeaderBg);
        row++;

        bool zebra = false;
        foreach (var r in report.Rows)
        {
            ws.Cell(row, 1).Value = r.Hour;
            ws.Cell(row, 2).Value = r.HourLabel;
            ws.Cell(row, 3).Value = r.VisitCount;
            ws.Cell(row, 4).Value = r.TotalPersons;
            ws.Cell(row, 5).Value = r.AverageGroupSize;
            ws.Cell(row, 6).Value = $"{r.PercentageShare:F1}%";

            StyleDataRow(ws, row, headers.Length, zebra);
            zebra = !zebra;
            row++;
        }

        // Totals
        ws.Cell(row, 1).Value = "الإجمالي";
        ws.Cell(row, 2).Value = "-";
        ws.Cell(row, 3).Value = report.TotalVisits;
        ws.Cell(row, 4).Value = report.TotalPersons;
        ws.Cell(row, 5).Value = report.TotalVisits > 0 ? Math.Round((double)report.TotalPersons / report.TotalVisits, 1) : 0.0;
        ws.Cell(row, 6).Value = report.TotalVisits > 0 ? "100.0%" : "0.0%";
        StyleTotalsRow(ws, row, headers.Length);

        ws.Columns().AdjustToContents();
        return SaveToBytes(workbook);
    }

    private static void StyleHeader(IXLRange range, XLColor bgColor)
    {
        range.Style.Fill.BackgroundColor = bgColor;
        range.Style.Font.FontColor = HeaderText;
        range.Style.Font.Bold = true;
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

    private static void AddKpiRow(IXLWorksheet ws, ref int row, string label, int value, bool highlight = false)
    {
        ws.Cell(row, 1).Value = label;
        ws.Cell(row, 2).Value = value;
        ws.Cell(row, 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Cell(row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        if (highlight)
        {
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = TotalsBg;
            ws.Cell(row, 2).Style.Fill.BackgroundColor = TotalsBg;
        }
        row++;
    }

    private static string GetDenialReasonArabic(DenialReason reason) => reason switch
    {
        DenialReason.HolderBlocked => "المستفيد محظور",
        DenialReason.HolderSuspended => "المستفيد موقوف",
        DenialReason.CardExpired => "البطاقة منتهية الصلاحية",
        DenialReason.OrganizationInactive => "الجهة غير نشطة",
        DenialReason.CooldownActive => "فترة التهدئة نشطة (سبق الدخول)",
        _ => reason.ToString()
    };

    private static byte[] SaveToBytes(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
