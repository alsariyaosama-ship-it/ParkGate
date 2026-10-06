using System.Reflection;
using System.Windows.Forms;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Resources;
using Shouldly;

namespace ParkGate.ArchitectureTests;

public sealed class ReportsControlTests
{
    private static TControl GetField<TControl>(object target, string fieldName) where TControl : class
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.ShouldNotBeNull($"Field '{fieldName}' was not found on '{target.GetType().Name}'.");
        var val = field.GetValue(target) as TControl;
        val.ShouldNotBeNull($"Field '{fieldName}' could not be cast to '{typeof(TControl).Name}'.");
        return val;
    }

    [Fact]
    public void ReportsControl_ShouldHaveRtlAndSixLocalizedTabs()
    {
        using var control = new ReportsControl();

        control.RightToLeft.ShouldBe(RightToLeft.Yes);

        var tabControl = GetField<TabControl>(control, "_tabControl");
        tabControl.TabCount.ShouldBe(6);

        tabControl.TabPages[0].Text.ShouldBe(Strings.Tab_DailySummary);
        tabControl.TabPages[1].Text.ShouldBe(Strings.Tab_DateRangeSummary);
        tabControl.TabPages[2].Text.ShouldBe(Strings.Tab_OrgStatistics);
        tabControl.TabPages[3].Text.ShouldBe(Strings.Tab_OperatorWorkstation);
        tabControl.TabPages[4].Text.ShouldBe(Strings.Tab_DenialsAndOverrides);
        tabControl.TabPages[5].Text.ShouldBe(Strings.Tab_HourlyDistribution);
    }

    [Fact]
    public void ReportsControl_ActionButtons_ShouldHaveArabicLabels()
    {
        using var control = new ReportsControl();

        var btnRun = GetField<Button>(control, "_btnRunReport");
        var btnExport = GetField<Button>(control, "_btnExportExcel");
        var btnPrint = GetField<Button>(control, "_btnPrintPreview");

        btnRun.Text.ShouldBe(Strings.Btn_RunReport);
        btnExport.Text.ShouldBe(Strings.Btn_ExportExcel);
        btnPrint.Text.ShouldBe(Strings.Btn_PrintPreview);
    }

    [Fact]
    public void ReportsControl_QuickDateButtons_ShouldSetExpectedDates()
    {
        using var control = new ReportsControl();

        var btnToday = GetField<Button>(control, "_btnToday");
        var btnYesterday = GetField<Button>(control, "_btnYesterday");
        var dtpFrom = GetField<DateTimePicker>(control, "_dtpFromDate");
        var dtpTo = GetField<DateTimePicker>(control, "_dtpToDate");

        btnToday.PerformClick();
        dtpFrom.Value.Date.ShouldBe(DateTime.Today);
        dtpTo.Value.Date.ShouldBe(DateTime.Today);

        btnYesterday.PerformClick();
        dtpFrom.Value.Date.ShouldBe(DateTime.Today.AddDays(-1));
        dtpTo.Value.Date.ShouldBe(DateTime.Today.AddDays(-1));
    }

    [Fact]
    public void ReportsControl_SelectReportTab_ShouldSwitchActiveTab()
    {
        using var control = new ReportsControl();

        var tabControl = GetField<TabControl>(control, "_tabControl");
        control.SelectReportTab(2);

        tabControl.SelectedIndex.ShouldBe(2);
        tabControl.SelectedTab!.Text.ShouldBe(Strings.Tab_OrgStatistics);
    }

    [Fact]
    public void ReportsControl_FilterVisibility_ShouldAdaptToTab()
    {
        using var control = new ReportsControl();

        var lblFilterType = GetField<Label>(control, "_lblFilterType");
        var cboFilterType = GetField<ComboBox>(control, "_cboFilterType");
        var lblToDate = GetField<Label>(control, "_lblToDate");

        // Tab 0: Daily
        control.SelectReportTab(0);
        lblToDate.Visible.ShouldBeFalse();
        lblFilterType.Visible.ShouldBeFalse();

        // Tab 4: Denials & Overrides
        control.SelectReportTab(4);
        lblToDate.Visible.ShouldBeTrue();
        lblFilterType.Visible.ShouldBeTrue();
        cboFilterType.Visible.ShouldBeTrue();

        // Tab 2: Org Statistics
        control.SelectReportTab(2);
        lblFilterType.Visible.ShouldBeFalse();
        cboFilterType.Visible.ShouldBeFalse();
    }
}
