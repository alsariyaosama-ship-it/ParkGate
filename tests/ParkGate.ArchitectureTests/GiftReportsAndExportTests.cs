using Microsoft.Extensions.Configuration;
using ParkGate.WinForms;
using System.Reflection;
using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using ParkGate.Application.Common;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.GiftVouchers.Services;
using ParkGate.Application.Ports.Reports;
using ParkGate.Application.Security;
using ParkGate.Domain.Enums;
using ParkGate.Infrastructure.Reports;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Forms;
using Shouldly;
using Xunit;

namespace ParkGate.ArchitectureTests;

public sealed class GiftReportsAndExportTests
{
    [Fact]
    public void ClosedXmlGiftExcelExportService_ExportsValidExcelWorkbook()
    {
        // Arrange
        var service = new ClosedXmlGiftExcelExportService();
        var rows = new List<GiftReceiptRowDto>
        {
            new(
                ReceiptNo: 1001,
                BeneficiaryId: Guid.NewGuid(),
                HolderName: "أحمد علي",
                CardNumber: "1001001",
                OrganizationId: 1,
                OrganizationName: "صندوق التضامن",
                Value: 50.000m,
                IssuedAtUtc: DateTime.UtcNow.AddHours(-3),
                IssuedByUserName: "operator1",
                RedeemedAtUtc: DateTime.UtcNow.AddHours(-1),
                RedeemedByUserName: "operator2",
                RedeemedWorkstationName: "شباك الألعاب 1",
                Status: GiftVoucherStatus.Redeemed,
                IsOverride: false,
                OverrideReason: null),
            new(
                ReceiptNo: 1002,
                BeneficiaryId: Guid.NewGuid(),
                HolderName: "سالم محمود",
                CardNumber: "1001002",
                OrganizationId: 1,
                OrganizationName: "صندوق التضامن",
                Value: 30.000m,
                IssuedAtUtc: DateTime.UtcNow.AddHours(-2),
                IssuedByUserName: "operator1",
                RedeemedAtUtc: null,
                RedeemedByUserName: null,
                RedeemedWorkstationName: null,
                Status: GiftVoucherStatus.Issued,
                IsOverride: true,
                OverrideReason: "موافقة مشرف")
        };

        var statusTotals = new List<GiftReceiptStatusTotalDto>
        {
            new(GiftVoucherStatus.Redeemed, 1, 50.000m),
            new(GiftVoucherStatus.Issued, 1, 30.000m)
        };

        var report = new GiftReceiptReportDto(
            FromDate: DateOnly.FromDateTime(DateTime.UtcNow),
            ToDate: DateOnly.FromDateTime(DateTime.UtcNow),
            OrganizationId: null,
            WorkstationId: null,
            Filter: GiftReceiptFilter.All,
            Rows: rows,
            StatusTotals: statusTotals,
            OrganizationTotals: [],
            TotalCount: 2,
            TotalValue: 80.000m);

        // Act
        var bytes = service.ExportGiftReceipts(report, "Libya Standard Time");

        // Assert
        bytes.ShouldNotBeNull();
        bytes.Length.ShouldBeGreaterThan(0);

        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        workbook.Worksheets.Count.ShouldBe(1);

        var ws = workbook.Worksheets.First();
        ws.RightToLeft.ShouldBeTrue();
        ws.Cell(1, 1).GetString().ShouldContain("قسائم ووصولات الهدايا");
        ws.Cell(5, 1).GetString().ShouldBe("المؤشر");
    }

    [Fact]
    public void GiftReceiptsForm_ConstructsParameterless_AndIsRtl()
    {
        using var form = new GiftReceiptsForm();
        form.RightToLeft.ShouldBe(System.Windows.Forms.RightToLeft.Yes);
        form.RightToLeftLayout.ShouldBeTrue();
        form.Text.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GiftReceiptsForm_LoadReportAsync_CallsServiceAndPopulatesGrid()
    {
        var fakeRows = new List<GiftReceiptRowDto>
        {
            new(
                ReceiptNo: 501,
                BeneficiaryId: Guid.NewGuid(),
                HolderName: "طارق عمر",
                CardNumber: "2002001",
                OrganizationId: 2,
                OrganizationName: "النفط",
                Value: 40.000m,
                IssuedAtUtc: DateTime.UtcNow,
                IssuedByUserName: "admin",
                RedeemedAtUtc: null,
                RedeemedByUserName: null,
                RedeemedWorkstationName: null,
                Status: GiftVoucherStatus.Issued,
                IsOverride: false,
                OverrideReason: null)
        };

        var fakeReport = new GiftReceiptReportDto(
            FromDate: DateOnly.FromDateTime(DateTime.Today),
            ToDate: DateOnly.FromDateTime(DateTime.Today),
            OrganizationId: null,
            WorkstationId: null,
            Filter: GiftReceiptFilter.All,
            Rows: fakeRows,
            StatusTotals: [new(GiftVoucherStatus.Issued, 1, 40.000m)],
            OrganizationTotals: [],
            TotalCount: 1,
            TotalValue: 40.000m);

        var fakeService = new FakeGiftReportService(fakeReport);
        using var form = new GiftReceiptsForm(fakeService, null, null, null, null, null);

        await form.LoadReportAsync();

        var grid = form.Controls.Find("_gridReceipts", true).OfType<System.Windows.Forms.DataGridView>().FirstOrDefault();
        grid.ShouldNotBeNull();
        grid.Rows.Count.ShouldBe(1);
    }

    [Fact]
    public void MainForm_GiftReceiptsReport_VisibleWhenHasViewReports()
    {
        var userWithReports = new FakeCurrentUser(UserRole.Operator, [Permission.ViewReports]);
        var userWithoutReports = new FakeCurrentUser(UserRole.Operator, [Permission.CheckIn]);

        using var formWithReports = new MainForm(userWithReports, null, null, null, null, null, null, null, null, null);
        formWithReports.ApplyPermissions();
        var menuWith = GetMenuItem(formWithReports, "_menuGiftReceiptsReport");
        menuWith.ShouldNotBeNull();
        menuWith.Available.ShouldBeTrue();

        using var formWithoutReports = new MainForm(userWithoutReports, null, null, null, null, null, null, null, null, null);
        formWithoutReports.ApplyPermissions();
        var menuWithout = GetMenuItem(formWithoutReports, "_menuGiftReceiptsReport");
        menuWithout.ShouldNotBeNull();
        menuWithout.Available.ShouldBeFalse();
    }

    [Fact]
    public void DependencyInjection_RegistersGiftReportAndExportServices()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();

        services.AddParkGateServices(config);

        services.Any(s => s.ServiceType == typeof(GiftReceiptsForm)).ShouldBeTrue();
        services.Any(s => s.ServiceType == typeof(IGiftExcelExportService) && s.ImplementationType == typeof(ClosedXmlGiftExcelExportService)).ShouldBeTrue();
    }

    private static System.Windows.Forms.ToolStripMenuItem? GetMenuItem(MainForm form, string fieldName)
    {
        var field = typeof(MainForm).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(form) as System.Windows.Forms.ToolStripMenuItem;
    }

    private sealed class FakeGiftReportService(GiftReceiptReportDto report) : IGiftReportService
    {
        public Task<Result<GiftReceiptReportDto>> GetReceiptsAsync(
            DateOnly fromDate,
            DateOnly toDate,
            int? organizationId = null,
            int? workstationId = null,
            GiftReceiptFilter filter = GiftReceiptFilter.All,
            CancellationToken ct = default)
        {
            return Task.FromResult(Result<GiftReceiptReportDto>.Success(report));
        }
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        private readonly HashSet<Permission> _permissions;

        public FakeCurrentUser(UserRole role, IEnumerable<Permission> permissions)
        {
            Role = role;
            _permissions = [.. permissions];
        }

        public int? UserId => 1;
        public string? UserName => "testuser";
        public string? FullName => "مستخدم تجريبي";
        public UserRole? Role { get; }
        public bool IsAuthenticated => true;

        public bool Has(Permission permission) => _permissions.Contains(permission);
        public bool HasAny(params Permission[] permissions) => permissions.Any(p => _permissions.Contains(p));
        public bool IsInRole(UserRole role) => Role == role;
    }
}
