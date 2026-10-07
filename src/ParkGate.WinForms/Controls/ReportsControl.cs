using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Services;
using ParkGate.Application.Ports.Reports;
using ParkGate.Application.Reports.DTOs;
using ParkGate.Application.Reports.Services;
using ParkGate.Application.Settings.Services;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Controls;

public partial class ReportsControl : UserControl
{
    private readonly IReportService? _reportService;
    private readonly IExcelExportService? _excelExportService;
    private readonly IOrganizationService? _organizationService;
    private readonly ISettingsService? _settingsService;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<ReportsControl>? _logger;

    private List<OrganizationDto> _organizations = [];
    private IReadOnlyList<AgeCategoryDto> _activeCategories = [];

    private DailySummaryReportDto? _currentDailyReport;
    private DateRangeSummaryReportDto? _currentDateRangeReport;
    private OrganizationStatisticsReportDto? _currentOrgStatsReport;
    private OperatorWorkstationReportDto? _currentOpWsReport;
    private DenialsAndOverridesReportDto? _currentDenialsReport;
    private HourlyDistributionReportDto? _currentHourlyReport;

    private string? _timeZoneId = "Libya Standard Time";

    public ReportsControl() : this(null, null, null, null, null, null)
    {
    }

    public ReportsControl(
        IReportService? reportService,
        IExcelExportService? excelExportService,
        IOrganizationService? organizationService,
        ISettingsService? settingsService,
        INotifyService? notifyService,
        ILogger<ReportsControl>? logger)
    {
        InitializeComponent();

        _reportService = reportService;
        _excelExportService = excelExportService;
        _organizationService = organizationService;
        _settingsService = settingsService;
        _notifyService = notifyService;
        _logger = logger;

        ApplyLocalizedTexts();
        UpdateFilterControlsVisibility();
        WireEvents();
    }

    private void ApplyLocalizedTexts()
    {
        _lblTitle.Text = Strings.Title_Reports;
        _lblFromDate.Text = Strings.Label_FromDate;
        _lblToDate.Text = Strings.Label_ToDate;
        _lblOrganization.Text = Strings.Label_Organization;
        _lblFilterType.Text = Strings.Label_FilterType;

        _btnToday.Text = Strings.Btn_Today;
        _btnYesterday.Text = Strings.Btn_Yesterday;
        _btnThisWeek.Text = Strings.Btn_ThisWeek;
        _btnThisMonth.Text = Strings.Btn_ThisMonth;

        _btnRunReport.Text = Strings.Btn_RunReport;
        _btnExportExcel.Text = Strings.Btn_ExportExcel;
        _btnPrintPreview.Text = Strings.Btn_PrintPreview;

        _tabDaily.Text = Strings.Tab_DailySummary;
        _tabDateRange.Text = Strings.Tab_DateRangeSummary;
        _tabOrgStats.Text = Strings.Tab_OrgStatistics;
        _tabOperatorWs.Text = Strings.Tab_OperatorWorkstation;
        _tabDenials.Text = Strings.Tab_DenialsAndOverrides;
        _tabHourly.Text = Strings.Tab_HourlyDistribution;

        _dtpFromDate.CustomFormat = "dd/MM/yyyy";
        _dtpFromDate.Format = DateTimePickerFormat.Custom;
        _dtpToDate.CustomFormat = "dd/MM/yyyy";
        _dtpToDate.Format = DateTimePickerFormat.Custom;

        // Initialize FilterType options for Tab 5
        _cboFilterType.Items.Clear();
        _cboFilterType.Items.Add(new ComboBoxItem<DenialOverrideFilter>(Strings.Filter_All, DenialOverrideFilter.All));
        _cboFilterType.Items.Add(new ComboBoxItem<DenialOverrideFilter>(Strings.Filter_DeniedOnly, DenialOverrideFilter.DeniedOnly));
        _cboFilterType.Items.Add(new ComboBoxItem<DenialOverrideFilter>(Strings.Filter_OverriddenOnly, DenialOverrideFilter.OverriddenOnly));
        _cboFilterType.SelectedIndex = 0;
    }

    private void WireEvents()
    {
        Load += async (_, _) => await OnControlLoadedAsync();

        _btnRunReport.Click += async (_, _) => await RunCurrentReportAsync();
        _btnExportExcel.Click += async (_, _) => await ExportToExcelAsync();
        _btnPrintPreview.Click += (_, _) => ShowPrintPreview();

        _btnToday.Click += (_, _) => SetDateRange(DateTime.Today, DateTime.Today);
        _btnYesterday.Click += (_, _) => SetDateRange(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1));
        _btnThisWeek.Click += (_, _) =>
        {
            var diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Saturday)) % 7;
            var start = DateTime.Today.AddDays(-diff);
            SetDateRange(start, DateTime.Today);
        };
        _btnThisMonth.Click += (_, _) =>
        {
            var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            SetDateRange(start, DateTime.Today);
        };

        _tabControl.SelectedIndexChanged += async (_, _) =>
        {
            UpdateFilterControlsVisibility();
            await RunCurrentReportAsync();
        };
    }

    public async Task OnControlLoadedAsync()
    {
        try
        {
            if (_settingsService != null)
            {
                var settings = await _settingsService.GetSettingsAsync();
                _timeZoneId = settings.TimeZoneId;
            }

            await LoadOrganizationsAsync();
            await LoadActiveCategoriesAsync();

            SetDateRange(DateTime.Today, DateTime.Today);
            UpdateFilterControlsVisibility();
            await RunCurrentReportAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize reports control.");
        }
    }

    public void SelectReportTab(int tabIndex)
    {
        if (tabIndex >= 0 && tabIndex < _tabControl.TabCount)
        {
            _tabControl.SelectedIndex = tabIndex;
            UpdateFilterControlsVisibility();
        }
    }

    private void UpdateFilterControlsVisibility()
    {
        bool isDenialsTab = _tabControl.SelectedIndex == 4;
        _lblFilterType.Visible = isDenialsTab;
        _cboFilterType.Visible = isDenialsTab;

        bool isDailyTab = _tabControl.SelectedIndex == 0;
        _lblToDate.Visible = !isDailyTab;
        _dtpToDate.Visible = !isDailyTab;
        _lblFromDate.Text = isDailyTab ? Strings.Label_Date : Strings.Label_FromDate;
    }

    private void SetDateRange(DateTime from, DateTime to)
    {
        _dtpFromDate.Value = from;
        _dtpToDate.Value = to;
    }

    private async Task LoadOrganizationsAsync()
    {
        if (_organizationService == null)
        {
            return;
        }

        try
        {
            var orgs = await _organizationService.GetAllAsync(activeOnly: true);
            _organizations = orgs.ToList();

            _cboOrganization.Items.Clear();
            _cboOrganization.Items.Add(new ComboBoxItem<int?>(Strings.Label_AllOrganizations, null));
            foreach (var org in _organizations)
            {
                _cboOrganization.Items.Add(new ComboBoxItem<int?>(org.NameAr, org.Id));
            }
            _cboOrganization.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading organizations for report filter.");
        }
    }

    private async Task LoadActiveCategoriesAsync()
    {
        if (_reportService == null)
        {
            return;
        }

        try
        {
            var res = await _reportService.GetActiveAgeCategoriesAsync();
            if (res.IsSuccess)
            {
                _activeCategories = res.Value;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading active age categories for reports.");
        }
    }

    private int? SelectedOrganizationId
    {
        get
        {
            if (_cboOrganization.SelectedItem is ComboBoxItem<int?> item)
            {
                return item.Value;
            }
            return null;
        }
    }

    private DenialOverrideFilter SelectedDenialFilter
    {
        get
        {
            if (_cboFilterType.SelectedItem is ComboBoxItem<DenialOverrideFilter> item)
            {
                return item.Value;
            }
            return DenialOverrideFilter.All;
        }
    }

    public async Task RunCurrentReportAsync()
    {
        if (_reportService == null)
        {
            return;
        }

        var fromDate = DateOnly.FromDateTime(_dtpFromDate.Value);
        var toDate = DateOnly.FromDateTime(_dtpToDate.Value);
        var isDailyTab = _tabControl.SelectedIndex == 0;

        if (!isDailyTab && fromDate > toDate)
        {
            _notifyService?.Warning(Strings.Msg_InvalidDateRange);
            return;
        }

        SetBusy(true);

        try
        {
            switch (_tabControl.SelectedIndex)
            {
                case 0:
                    await LoadDailySummaryAsync(fromDate);
                    break;
                case 1:
                    await LoadDateRangeSummaryAsync(fromDate, toDate);
                    break;
                case 2:
                    await LoadOrgStatisticsAsync(fromDate, toDate);
                    break;
                case 3:
                    await LoadOperatorWorkstationStatsAsync(fromDate, toDate);
                    break;
                case 4:
                    await LoadDenialsAndOverridesAsync(fromDate, toDate);
                    break;
                case 5:
                    await LoadHourlyDistributionAsync(fromDate, toDate);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error generating report.");
            _notifyService?.Error(Strings.Msg_UnexpectedError);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool isBusy)
    {
        _btnRunReport.Enabled = !isBusy;
        _btnExportExcel.Enabled = !isBusy;
        _btnPrintPreview.Enabled = !isBusy;
        _lblStatus.Text = isBusy ? "جاري استخراج بيانات التقرير..." : string.Empty;
    }

    // 1. Daily Summary
    private async Task LoadDailySummaryAsync(DateOnly date)
    {
        var result = await _reportService!.GetDailySummaryAsync(date, SelectedOrganizationId);
        if (!result.IsSuccess)
        {
            _notifyService?.Error(ErrorMessages.GetMessage(result.Error));
            return;
        }

        _currentDailyReport = result.Value;
        RenderDailySummary(result.Value);
    }

    private void RenderDailySummary(DailySummaryReportDto report)
    {
        _pnlDailyKpi.SuspendLayout();
        _pnlDailyKpi.Controls.Clear();

        AddKpiCard(_pnlDailyKpi, Strings.Kpi_TotalAttempts, report.TotalAttempts.ToString("N0"), Color.FromArgb(69, 90, 100));
        AddKpiCard(_pnlDailyKpi, Strings.Kpi_AllowedEntries, report.AllowedEntries.ToString("N0"), Color.FromArgb(27, 135, 63));
        AddKpiCard(_pnlDailyKpi, Strings.Kpi_OverriddenEntries, report.OverriddenEntries.ToString("N0"), Color.FromArgb(178, 106, 0));
        AddKpiCard(_pnlDailyKpi, Strings.Kpi_TotalVisits, report.TotalVisits.ToString("N0"), Color.FromArgb(27, 135, 63), true);
        AddKpiCard(_pnlDailyKpi, Strings.Kpi_DeniedEntries, report.DeniedEntries.ToString("N0"), Color.FromArgb(198, 40, 40));
        AddKpiCard(_pnlDailyKpi, Strings.Kpi_VoidedEntries, report.VoidedEntries.ToString("N0"), Color.FromArgb(120, 144, 156));
        AddKpiCard(_pnlDailyKpi, Strings.Kpi_Holders, report.TotalHolders.ToString("N0"), Color.FromArgb(21, 101, 192));
        AddKpiCard(_pnlDailyKpi, Strings.Kpi_Companions, report.TotalCompanions.ToString("N0"), Color.FromArgb(0, 137, 123));
        AddKpiCard(_pnlDailyKpi, Strings.Kpi_TotalPersons, report.TotalPersons.ToString("N0"), Color.FromArgb(46, 125, 50), true);

        _pnlDailyKpi.ResumeLayout();

        // Populate Categories grid
        _gridDailyCategories.Columns.Clear();
        _gridDailyCategories.Columns.Add("CategoryName", "الفئة العمرية");
        _gridDailyCategories.Columns.Add("Count", "عدد المرافقين");
        _gridDailyCategories.Rows.Clear();

        foreach (var cat in report.AgeCategoryBreakdown)
        {
            _gridDailyCategories.Rows.Add(cat.CategoryName, cat.Count.ToString("N0"));
        }

        // Populate Orgs grid
        _gridDailyOrgs.Columns.Clear();
        _gridDailyOrgs.Columns.Add("OrgName", Strings.Col_Organization);
        _gridDailyOrgs.Columns.Add("OrgCode", Strings.Col_OrgCode);
        _gridDailyOrgs.Columns.Add("Allowed", Strings.Col_Allowed);
        _gridDailyOrgs.Columns.Add("Overridden", Strings.Col_Overridden);
        _gridDailyOrgs.Columns.Add("Visits", Strings.Col_Visits);
        _gridDailyOrgs.Columns.Add("Companions", Strings.Col_Companions);
        _gridDailyOrgs.Columns.Add("TotalPersons", Strings.Col_TotalPersons);
        _gridDailyOrgs.Rows.Clear();

        foreach (var org in report.OrganizationBreakdown)
        {
            _gridDailyOrgs.Rows.Add(
                org.OrganizationName,
                org.OrganizationCode,
                org.AllowedVisits.ToString("N0"),
                org.OverriddenVisits.ToString("N0"),
                org.TotalVisits.ToString("N0"),
                org.TotalCompanions.ToString("N0"),
                org.TotalPersons.ToString("N0"));
        }
    }

    private static void AddKpiCard(FlowLayoutPanel pnl, string title, string value, Color accentColor, bool highlight = false)
    {
        var card = new Panel
        {
            Size = new Size(112, 75),
            Margin = new Padding(3),
            BackColor = highlight ? Color.FromArgb(232, 245, 233) : Color.FromArgb(248, 249, 250),
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblTitle = new Label
        {
            Text = title,
            Font = new Font("Cairo", 8.5F, FontStyle.Regular),
            ForeColor = Color.FromArgb(100, 116, 139),
            Dock = DockStyle.Top,
            Height = 28,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblVal = new Label
        {
            Text = value,
            Font = new Font("Cairo", 13F, FontStyle.Bold),
            ForeColor = accentColor,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };

        card.Controls.Add(lblVal);
        card.Controls.Add(lblTitle);
        pnl.Controls.Add(card);
    }

    // 2. Date Range Summary
    private async Task LoadDateRangeSummaryAsync(DateOnly fromDate, DateOnly toDate)
    {
        var result = await _reportService!.GetDateRangeSummaryAsync(fromDate, toDate, SelectedOrganizationId);
        if (!result.IsSuccess)
        {
            _notifyService?.Error(ErrorMessages.GetMessage(result.Error));
            return;
        }

        _currentDateRangeReport = result.Value;
        RenderDateRangeSummary(result.Value);
    }

    private void RenderDateRangeSummary(DateRangeSummaryReportDto report)
    {
        _gridDateRange.Columns.Clear();
        _gridDateRange.Columns.Add("Date", Strings.Col_Date);
        _gridDateRange.Columns.Add("Day", Strings.Col_Day);
        _gridDateRange.Columns.Add("Attempts", Strings.Col_Attempts);
        _gridDateRange.Columns.Add("Allowed", Strings.Col_Allowed);
        _gridDateRange.Columns.Add("Overridden", Strings.Col_Overridden);
        _gridDateRange.Columns.Add("Visits", Strings.Col_Visits);
        _gridDateRange.Columns.Add("Denied", Strings.Col_Denied);
        _gridDateRange.Columns.Add("Voided", Strings.Col_Voided);
        _gridDateRange.Columns.Add("Holders", Strings.Col_Holders);
        _gridDateRange.Columns.Add("Companions", Strings.Col_Companions);

        foreach (var cat in _activeCategories)
        {
            _gridDateRange.Columns.Add($"Cat_{cat.Id}", cat.NameAr);
        }

        _gridDateRange.Columns.Add("TotalPersons", Strings.Col_TotalPersons);
        _gridDateRange.Rows.Clear();

        foreach (var r in report.Rows)
        {
            var rowVals = new List<object>
            {
                r.BusinessDate.ToString("dd/MM/yyyy"),
                r.DayOfWeekArabic,
                r.TotalAttempts.ToString("N0"),
                r.AllowedVisits.ToString("N0"),
                r.OverriddenVisits.ToString("N0"),
                r.TotalVisits.ToString("N0"),
                r.DeniedAttempts.ToString("N0"),
                r.VoidedAttempts.ToString("N0"),
                r.TotalHolders.ToString("N0"),
                r.TotalCompanions.ToString("N0")
            };

            foreach (var cat in _activeCategories)
            {
                var count = r.CompanionsByAgeCategory.FirstOrDefault(c => c.CategoryId == cat.Id)?.Count ?? 0;
                rowVals.Add(count.ToString("N0"));
            }

            rowVals.Add(r.TotalPersons.ToString("N0"));
            _gridDateRange.Rows.Add(rowVals.ToArray());
        }

        _lblDateRangeTotals.Text = string.Format(
            "الإجمالي العام: {0:N0} زيارة مقبولة | {1:N0} زبون | {2:N0} مرافق | {3:N0} إجمالي الأفراد | {4:N0} محاولة مرفوضة | {5:N0} ملغاة",
            report.Totals.TotalVisits,
            report.Totals.TotalHolders,
            report.Totals.TotalCompanions,
            report.Totals.TotalPersons,
            report.Totals.DeniedAttempts,
            report.Totals.VoidedAttempts);
    }

    // 3. Organization Statistics (Q8)
    private async Task LoadOrgStatisticsAsync(DateOnly fromDate, DateOnly toDate)
    {
        var result = await _reportService!.GetOrganizationStatisticsAsync(fromDate, toDate, SelectedOrganizationId);
        if (!result.IsSuccess)
        {
            _notifyService?.Error(ErrorMessages.GetMessage(result.Error));
            return;
        }

        _currentOrgStatsReport = result.Value;
        RenderOrgStatistics(result.Value);
    }

    private void RenderOrgStatistics(OrganizationStatisticsReportDto report)
    {
        _gridOrgStats.Columns.Clear();
        _gridOrgStats.Columns.Add("OrgName", Strings.Col_Organization);
        _gridOrgStats.Columns.Add("OrgCode", Strings.Col_OrgCode);
        _gridOrgStats.Columns.Add("Visits", Strings.Col_Visits);
        _gridOrgStats.Columns.Add("UniqueHolders", Strings.Col_UniqueHolders);
        _gridOrgStats.Columns.Add("Holders", Strings.Col_Holders);
        _gridOrgStats.Columns.Add("Companions", Strings.Col_Companions);

        foreach (var cat in _activeCategories)
        {
            _gridOrgStats.Columns.Add($"Cat_{cat.Id}", cat.NameAr);
        }

        _gridOrgStats.Columns.Add("TotalPersons", Strings.Col_TotalPersons);
        _gridOrgStats.Columns.Add("Percentage", Strings.Col_Percentage);
        _gridOrgStats.Rows.Clear();

        foreach (var r in report.Rows)
        {
            var rowVals = new List<object>
            {
                r.OrganizationName,
                r.OrganizationCode,
                r.TotalVisits.ToString("N0"),
                r.UniqueBeneficiariesCount.ToString("N0"),
                r.TotalHolders.ToString("N0"),
                r.TotalCompanions.ToString("N0")
            };

            foreach (var cat in _activeCategories)
            {
                var count = r.CompanionsByAgeCategory.FirstOrDefault(c => c.CategoryId == cat.Id)?.Count ?? 0;
                rowVals.Add(count.ToString("N0"));
            }

            rowVals.Add(r.TotalPersons.ToString("N0"));
            rowVals.Add($"{r.PercentageShare:F1}%");

            _gridOrgStats.Rows.Add(rowVals.ToArray());
        }

        _lblOrgStatsTotals.Text = string.Format(
            "الإجمالي: {0:N0} زيارة مقبولة | {1:N0} مستفيد فريد | {2:N0} مرافق | {3:N0} إجمالي الأفراد المستفيدين (تمثل {4} من زوار الحديقة)",
            report.Totals.TotalVisits,
            report.Totals.UniqueBeneficiariesCount,
            report.Totals.TotalCompanions,
            report.Totals.TotalPersons,
            report.TotalParkPersons > 0 ? "100%" : "0%");
    }

    // 4. Operator & Workstation
    private async Task LoadOperatorWorkstationStatsAsync(DateOnly fromDate, DateOnly toDate)
    {
        var result = await _reportService!.GetOperatorWorkstationStatisticsAsync(fromDate, toDate);
        if (!result.IsSuccess)
        {
            _notifyService?.Error(ErrorMessages.GetMessage(result.Error));
            return;
        }

        _currentOpWsReport = result.Value;
        RenderOperatorWorkstationStats(result.Value);
    }

    private void RenderOperatorWorkstationStats(OperatorWorkstationReportDto report)
    {
        // Operators
        _gridOperators.Columns.Clear();
        _gridOperators.Columns.Add("FullName", Strings.Col_Operator);
        _gridOperators.Columns.Add("UserName", Strings.Col_UserName);
        _gridOperators.Columns.Add("Attempts", Strings.Col_Attempts);
        _gridOperators.Columns.Add("Allowed", Strings.Col_Allowed);
        _gridOperators.Columns.Add("Overridden", Strings.Col_Overridden);
        _gridOperators.Columns.Add("Visits", Strings.Col_Visits);
        _gridOperators.Columns.Add("Denied", Strings.Col_Denied);
        _gridOperators.Columns.Add("Voided", Strings.Col_Voided);
        _gridOperators.Columns.Add("TotalPersons", Strings.Col_TotalPersons);
        _gridOperators.Rows.Clear();

        foreach (var op in report.OperatorRows)
        {
            _gridOperators.Rows.Add(
                op.FullName,
                op.UserName,
                op.TotalAttempts.ToString("N0"),
                op.AllowedEntries.ToString("N0"),
                op.OverriddenEntries.ToString("N0"),
                op.TotalVisits.ToString("N0"),
                op.DeniedEntries.ToString("N0"),
                op.VoidedEntries.ToString("N0"),
                op.TotalPersons.ToString("N0"));
        }

        // Workstations
        _gridWorkstations.Columns.Clear();
        _gridWorkstations.Columns.Add("DisplayName", Strings.Col_Workstation);
        _gridWorkstations.Columns.Add("MachineName", Strings.Col_MachineName);
        _gridWorkstations.Columns.Add("Attempts", Strings.Col_Attempts);
        _gridWorkstations.Columns.Add("Allowed", Strings.Col_Allowed);
        _gridWorkstations.Columns.Add("Overridden", Strings.Col_Overridden);
        _gridWorkstations.Columns.Add("Visits", Strings.Col_Visits);
        _gridWorkstations.Columns.Add("Denied", Strings.Col_Denied);
        _gridWorkstations.Columns.Add("Voided", Strings.Col_Voided);
        _gridWorkstations.Columns.Add("TotalPersons", Strings.Col_TotalPersons);
        _gridWorkstations.Rows.Clear();

        foreach (var ws in report.WorkstationRows)
        {
            _gridWorkstations.Rows.Add(
                ws.DisplayName,
                ws.MachineName,
                ws.TotalAttempts.ToString("N0"),
                ws.AllowedEntries.ToString("N0"),
                ws.OverriddenEntries.ToString("N0"),
                ws.TotalVisits.ToString("N0"),
                ws.DeniedEntries.ToString("N0"),
                ws.VoidedEntries.ToString("N0"),
                ws.TotalPersons.ToString("N0"));
        }
    }

    // 5. Denials & Overrides Log
    private async Task LoadDenialsAndOverridesAsync(DateOnly fromDate, DateOnly toDate)
    {
        var result = await _reportService!.GetDenialsAndOverridesLogAsync(fromDate, toDate, SelectedOrganizationId, SelectedDenialFilter);
        if (!result.IsSuccess)
        {
            _notifyService?.Error(ErrorMessages.GetMessage(result.Error));
            return;
        }

        _currentDenialsReport = result.Value;
        RenderDenialsAndOverrides(result.Value);
    }

    private void RenderDenialsAndOverrides(DenialsAndOverridesReportDto report)
    {
        _gridDenials.Columns.Clear();
        _gridDenials.Columns.Add("Time", Strings.Col_AttemptTime);
        _gridDenials.Columns.Add("CardNumber", Strings.Col_CardNumber);
        _gridDenials.Columns.Add("BeneficiaryName", Strings.Col_BeneficiaryName);
        _gridDenials.Columns.Add("Organization", Strings.Col_Organization);
        _gridDenials.Columns.Add("Result", Strings.Col_Result);
        _gridDenials.Columns.Add("DenialReason", Strings.Col_DenialReason);
        _gridDenials.Columns.Add("OverrideReason", Strings.Col_OverrideReason);
        _gridDenials.Columns.Add("Supervisor", Strings.Col_Supervisor);
        _gridDenials.Columns.Add("Operator", Strings.Col_Operator);
        _gridDenials.Columns.Add("Workstation", Strings.Col_Workstation);
        _gridDenials.Columns.Add("TotalPersons", Strings.Col_TotalPersons);
        _gridDenials.Rows.Clear();

        foreach (var item in report.Items)
        {
            var localTime = DateTimeDisplayHelper.ToParkLocalTime(item.AttemptedAtUtc, _timeZoneId);
            var resultText = item.Result == EntryResult.Denied ? "مرفوض" : "استثناء";
            var reasonText = item.DenialReason.HasValue ? GetDenialReasonArabic(item.DenialReason.Value) : "-";

            var rowIdx = _gridDenials.Rows.Add(
                DateTimeDisplayHelper.FormatDateTime(localTime),
                item.CardNumber,
                item.BeneficiaryName,
                item.OrganizationName,
                resultText,
                reasonText,
                item.OverrideReason ?? "-",
                item.SupervisorName ?? "-",
                item.OperatorName,
                item.WorkstationName,
                item.TotalPersons.ToString("N0"));

            // Color status
            if (item.Result == EntryResult.Denied)
            {
                _gridDenials.Rows[rowIdx].Cells["Result"].Style.ForeColor = Color.FromArgb(198, 40, 40);
            }
            else
            {
                _gridDenials.Rows[rowIdx].Cells["Result"].Style.ForeColor = Color.FromArgb(178, 106, 0);
            }
        }

        _lblDenialsCount.Text = $"إجمالي السجلات المعروضة: {report.Items.Count:N0}";
    }

    // 6. Hourly Distribution
    private async Task LoadHourlyDistributionAsync(DateOnly fromDate, DateOnly toDate)
    {
        var result = await _reportService!.GetHourlyDistributionAsync(fromDate, toDate, SelectedOrganizationId, _timeZoneId);
        if (!result.IsSuccess)
        {
            _notifyService?.Error(ErrorMessages.GetMessage(result.Error));
            return;
        }

        _currentHourlyReport = result.Value;
        RenderHourlyDistribution(result.Value);
    }

    private void RenderHourlyDistribution(HourlyDistributionReportDto report)
    {
        _gridHourly.Columns.Clear();
        _gridHourly.Columns.Add("Hour", Strings.Col_Hour);
        _gridHourly.Columns.Add("TimeRange", Strings.Col_TimeRange);
        _gridHourly.Columns.Add("Visits", Strings.Col_Visits);
        _gridHourly.Columns.Add("TotalPersons", Strings.Col_TotalPersons);
        _gridHourly.Columns.Add("AvgGroupSize", Strings.Col_AvgGroupSize);
        _gridHourly.Columns.Add("Percentage", Strings.Col_Percentage);
        _gridHourly.Rows.Clear();

        foreach (var r in report.Rows)
        {
            _gridHourly.Rows.Add(
                r.Hour.ToString("D2"),
                r.HourLabel,
                r.VisitCount.ToString("N0"),
                r.TotalPersons.ToString("N0"),
                r.AverageGroupSize.ToString("F1"),
                $"{r.PercentageShare:F1}%");
        }

        _lblHourlyTotals.Text = string.Format(
            "الإجمالي: {0:N0} زيارة | {1:N0} فرد | متوسط حجم المجموعة: {2:F1}",
            report.TotalVisits,
            report.TotalPersons,
            report.TotalVisits > 0 ? (double)report.TotalPersons / report.TotalVisits : 0.0);
    }

    // Excel Export
    public async Task ExportToExcelAsync()
    {
        if (_excelExportService == null)
        {
            return;
        }

        byte[]? bytes = null;
        string defaultFileName = string.Empty;

        switch (_tabControl.SelectedIndex)
        {
            case 0:
                if (_currentDailyReport == null)
                {
                    break;
                }
                bytes = _excelExportService.ExportDailySummary(_currentDailyReport, _activeCategories);
                defaultFileName = $"Daily_Summary_{_currentDailyReport.Date:yyyyMMdd}.xlsx";
                break;
            case 1:
                if (_currentDateRangeReport == null)
                {
                    break;
                }
                bytes = _excelExportService.ExportDateRangeSummary(_currentDateRangeReport, _activeCategories);
                defaultFileName = $"Date_Range_{_currentDateRangeReport.FromDate:yyyyMMdd}_{_currentDateRangeReport.ToDate:yyyyMMdd}.xlsx";
                break;
            case 2:
                if (_currentOrgStatsReport == null)
                {
                    break;
                }
                bytes = _excelExportService.ExportOrganizationStatistics(_currentOrgStatsReport, _activeCategories);
                defaultFileName = $"Org_Statistics_{_currentOrgStatsReport.FromDate:yyyyMMdd}_{_currentOrgStatsReport.ToDate:yyyyMMdd}.xlsx";
                break;
            case 3:
                if (_currentOpWsReport == null)
                {
                    break;
                }
                bytes = _excelExportService.ExportOperatorWorkstationStatistics(_currentOpWsReport);
                defaultFileName = $"Operators_Workstations_{_currentOpWsReport.FromDate:yyyyMMdd}_{_currentOpWsReport.ToDate:yyyyMMdd}.xlsx";
                break;
            case 4:
                if (_currentDenialsReport == null)
                {
                    break;
                }
                bytes = _excelExportService.ExportDenialsAndOverrides(_currentDenialsReport, _timeZoneId);
                defaultFileName = $"Denials_Overrides_{_currentDenialsReport.FromDate:yyyyMMdd}_{_currentDenialsReport.ToDate:yyyyMMdd}.xlsx";
                break;
            case 5:
                if (_currentHourlyReport == null)
                {
                    break;
                }
                bytes = _excelExportService.ExportHourlyDistribution(_currentHourlyReport);
                defaultFileName = $"Hourly_Distribution_{_currentHourlyReport.FromDate:yyyyMMdd}_{_currentHourlyReport.ToDate:yyyyMMdd}.xlsx";
                break;
        }

        if (bytes == null || bytes.Length == 0)
        {
            _notifyService?.Warning(Strings.Msg_NoDataToExport);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "Excel Workbooks (*.xlsx)|*.xlsx",
            FileName = defaultFileName,
            Title = "تصدير التقرير إلى Excel"
        };

        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                await File.WriteAllBytesAsync(sfd.FileName, bytes);
                _notifyService?.Info(Strings.Msg_ExportSuccess);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save exported excel file.");
                _notifyService?.Error("تعذر حفظ ملف Excel. تحقق من صلاحيات المجلد أو عدم فتح الملف في برنامج آخر.");
            }
        }
    }

    // Print Preview
    public void ShowPrintPreview()
    {
        try
        {
            using var doc = new PrintDocument();
            doc.DefaultPageSettings.Landscape = true;
            doc.PrintPage += OnPrintPage;

            using var preview = new PrintPreviewDialog
            {
                Document = doc,
                Width = 950,
                Height = 700,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true
            };

            preview.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error opening print preview.");
            _notifyService?.Error("تعذر فتح معاينة الطباعة.");
        }
    }

    private void OnPrintPage(object sender, PrintPageEventArgs e)
    {
        var g = e.Graphics;
        if (g == null)
        {
            return;
        }

        var fontTitle = new Font("Cairo", 16F, FontStyle.Bold);
        var fontSubtitle = new Font("Cairo", 10F, FontStyle.Regular);
        var fontHeader = new Font("Cairo", 9.5F, FontStyle.Bold);
        var fontCell = new Font("Cairo", 9F, FontStyle.Regular);

        float y = 50;
        float left = e.MarginBounds.Left;
        float right = e.MarginBounds.Right;
        float width = e.MarginBounds.Width;

        var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        // Title
        string reportTitle = _tabControl.SelectedTab?.Text ?? Strings.Title_Reports;
        g.DrawString(reportTitle, fontTitle, Brushes.DarkGreen, new RectangleF(left, y, width, 30), sfCenter);
        y += 35;

        // Subtitle / Date range
        string sub = $"الفترة: {_dtpFromDate.Value:dd/MM/yyyy} إلى {_dtpToDate.Value:dd/MM/yyyy}";
        if (_cboOrganization.SelectedItem is ComboBoxItem<int?> orgItem && orgItem.Value.HasValue)
        {
            sub += $" | الجهة: {orgItem.Text}";
        }
        g.DrawString(sub, fontSubtitle, Brushes.DarkSlateGray, new RectangleF(left, y, width, 22), sfCenter);
        y += 30;

        // Draw separator
        g.DrawLine(Pens.DarkSeaGreen, left, y, right, y);
        y += 10;

        // Get currently active DataGridView to render
        DataGridView? gridToPrint = _tabControl.SelectedIndex switch
        {
            0 => _gridDailyOrgs,
            1 => _gridDateRange,
            2 => _gridOrgStats,
            3 => _gridOperators,
            4 => _gridDenials,
            5 => _gridHourly,
            _ => null
        };

        if (gridToPrint == null || gridToPrint.Rows.Count == 0)
        {
            g.DrawString("لا توجد بيانات متاحة في هذا التقرير.", fontSubtitle, Brushes.Gray, new RectangleF(left, y, width, 30), sfCenter);
            return;
        }

        // Draw Table Headers
        int colCount = Math.Min(gridToPrint.Columns.Count, 12);
        float colWidth = width / colCount;

        g.FillRectangle(Brushes.DarkGreen, left, y, width, 26);
        for (int c = 0; c < colCount; c++)
        {
            float colX = right - ((c + 1) * colWidth);
            var rect = new RectangleF(colX, y, colWidth, 26);
            g.DrawString(gridToPrint.Columns[c].HeaderText, fontHeader, Brushes.White, rect, sfCenter);
        }
        y += 26;

        // Draw Rows
        bool zebra = false;
        int maxRows = Math.Min(gridToPrint.Rows.Count, 35);
        for (int r = 0; r < maxRows; r++)
        {
            var rowRect = new RectangleF(left, y, width, 22);
            if (zebra)
            {
                g.FillRectangle(Brushes.WhiteSmoke, rowRect);
            }
            g.DrawRectangle(Pens.LightGray, rowRect.X, rowRect.Y, rowRect.Width, rowRect.Height);

            for (int c = 0; c < colCount; c++)
            {
                float colX = right - ((c + 1) * colWidth);
                var cellRect = new RectangleF(colX, y, colWidth, 22);
                var val = gridToPrint.Rows[r].Cells[c].Value?.ToString() ?? string.Empty;
                g.DrawString(val, fontCell, Brushes.Black, cellRect, sfCenter);
            }

            y += 22;
            zebra = !zebra;
        }

        // Footer note
        y += 15;
        g.DrawString($"طُبع من منظومة ParkGate بتاريخ {DateTime.Now:dd/MM/yyyy hh:mm tt}", fontSubtitle, Brushes.Gray, new RectangleF(left, y, width, 20), sfCenter);
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

    private sealed class ComboBoxItem<T>(string text, T value)
    {
        public string Text { get; } = text;
        public T Value { get; } = value;
        public override string ToString() => Text;
    }
}
