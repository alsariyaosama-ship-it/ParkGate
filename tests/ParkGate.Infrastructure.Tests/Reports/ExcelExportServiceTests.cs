using ClosedXML.Excel;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.Reports.DTOs;
using ParkGate.Domain.Enums;
using ParkGate.Infrastructure.Reports;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Reports;

public sealed class ExcelExportServiceTests
{
    private readonly ClosedXmlExcelExportService _sut = new();

    private static readonly List<AgeCategoryDto> SampleCategories =
    [
        new(1, "أطفال (0 - 3)", null, 1, true),
        new(2, "أطفال (4 - 12)", null, 2, true),
        new(3, "بالغون (13+)", null, 3, true)
    ];

    [Fact]
    public void ExportDailySummary_ShouldProduceValidRtlWorkbook()
    {
        var report = new DailySummaryReportDto(
            new DateOnly(2026, 10, 5),
            1, "صندوق التضامن الاجتماعي",
            1, "الشباك 1",
            50, 40, 5, 45, 5, 0, 45, 80, 125,
            [
                new(1, "أطفال (0 - 3)", 20),
                new(2, "أطفال (4 - 12)", 50),
                new(3, "بالغون (13+)", 10)
            ],
            [
                new(1, "SOLIDARITY", "صندوق التضامن", 40, 5, 45, 45, 80, 125)
            ]);

        var bytes = _sut.ExportDailySummary(report, SampleCategories);

        bytes.ShouldNotBeNull();
        bytes.Length.ShouldBeGreaterThan(1000);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        workbook.Worksheets.Count.ShouldBe(1);
        var ws = workbook.Worksheet(1);
        ws.Name.ShouldBe("الملخص اليومي");
        ws.RightToLeft.ShouldBeTrue();
    }

    [Fact]
    public void ExportDateRangeSummary_ShouldProduceValidWorkbookWithTotals()
    {
        var row1 = new DateRangeSummaryRowDto(
            new DateOnly(2026, 10, 5), "الاثنين",
            10, 8, 1, 9, 1, 0, 9, 15, 24,
            [new(1, "أطفال (0 - 3)", 5), new(2, "أطفال (4 - 12)", 10)]);

        var totals = new DateRangeTotalsDto(
            10, 8, 1, 9, 1, 0, 9, 15, 24,
            [new(1, "أطفال (0 - 3)", 5), new(2, "أطفال (4 - 12)", 10)]);

        var report = new DateRangeSummaryReportDto(
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 10, 5),
            null, null,
            [row1],
            totals);

        var bytes = _sut.ExportDateRangeSummary(report, SampleCategories);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        var ws = workbook.Worksheet("ملخص الفترة");
        ws.RightToLeft.ShouldBeTrue();
        ws.RowCount().ShouldBeGreaterThan(5);
    }

    [Fact]
    public void ExportOrganizationStatistics_ShouldProduceValidWorkbook()
    {
        var orgRow = new OrganizationStatisticsRowDto(
            1, "SOLIDARITY", "صندوق التضامن الاجتماعي",
            50, 42, 50, 90, 140, 75.5,
            [new(1, "أطفال (0 - 3)", 30)]);

        var totals = new OrganizationStatisticsTotalsDto(
            50, 42, 50, 90, 140,
            [new(1, "أطفال (0 - 3)", 30)]);

        var report = new OrganizationStatisticsReportDto(
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 10, 5),
            null, null,
            65, 185,
            [orgRow],
            totals);

        var bytes = _sut.ExportOrganizationStatistics(report, SampleCategories);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        var ws = workbook.Worksheet("إحصائيات الجهات");
        ws.RightToLeft.ShouldBeTrue();
    }

    [Fact]
    public void ExportOperatorWorkstationStatistics_ShouldProduceTwoSheets()
    {
        var op = new OperatorStatisticsRowDto(1, "admin", "مدير النظام", 20, 18, 1, 19, 1, 0, 45);
        var ws = new WorkstationStatisticsRowDto(1, "PC-01", "الشباك الرئيسي", 20, 18, 1, 19, 1, 0, 45);

        var report = new OperatorWorkstationReportDto(
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 10, 5),
            [op],
            [ws]);

        var bytes = _sut.ExportOperatorWorkstationStatistics(report);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        workbook.Worksheets.Count.ShouldBe(2);
        workbook.Worksheet("إحصائيات المشغلين").RightToLeft.ShouldBeTrue();
        workbook.Worksheet("إحصائيات نقاط الدخول").RightToLeft.ShouldBeTrue();
    }

    [Fact]
    public void ExportDenialsAndOverrides_ShouldProduceValidSheet()
    {
        var itm = new DenialOverrideLogItemDto(
            Guid.NewGuid(),
            new DateTime(2026, 10, 5, 14, 30, 0, DateTimeKind.Utc),
            new DateOnly(2026, 10, 5),
            "12345",
            "خالد عمر",
            1, "التضامن",
            EntryResult.Overridden,
            null,
            "استثناء لغرض الاختبار",
            "المشرف أحمد",
            "المشغل علي",
            "الشباك 1",
            4);

        var report = new DenialsAndOverridesReportDto(
            new DateOnly(2026, 10, 5),
            new DateOnly(2026, 10, 5),
            null, null,
            DenialOverrideFilter.All,
            [itm]);

        var bytes = _sut.ExportDenialsAndOverrides(report);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        var worksheet = workbook.Worksheet("المرفوضات والاستثناءات");
        worksheet.RightToLeft.ShouldBeTrue();
    }

    [Fact]
    public void ExportHourlyDistribution_ShouldProduce24RowsPlusTotals()
    {
        var rows = Enumerable.Range(0, 24).Select(h =>
            new HourlyDistributionRowDto(h, $"{h:D2}:00 - {h + 1:D2}:00", h == 14 ? 10 : 0, h == 14 ? 30 : 0, h == 14 ? 3.0 : 0.0, h == 14 ? 100.0 : 0.0)).ToList();

        var report = new HourlyDistributionReportDto(
            new DateOnly(2026, 10, 5),
            new DateOnly(2026, 10, 5),
            null, null,
            10, 30,
            rows);

        var bytes = _sut.ExportHourlyDistribution(report);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        var worksheet = workbook.Worksheet("التوزيع الساعي");
        worksheet.RightToLeft.ShouldBeTrue();
    }
}
