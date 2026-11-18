using System.Reflection;
using System.Windows.Forms;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Forms;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>R5.8: the desk fits a 1366×768 screen at 100 % (work area about 1366×728, desk area about 620 px).</summary>
public sealed class DeskLayoutTests
{
    private const int ScreenWorkAreaHeight = 728;
    private const int DeskAreaHeight = 620;

    [Fact]
    public void MainForm_MinimumSize_FitsA1366x768Screen()
    {
        using var form = TestUi.Create<MainForm>();

        form.MinimumSize.Width.ShouldBeLessThanOrEqualTo(1366);
        form.MinimumSize.Height.ShouldBeLessThanOrEqualTo(ScreenWorkAreaHeight);
    }

    [Fact]
    public void Desk_MinimumHeight_FitsTheDeskArea()
    {
        using var desk = TestUi.Create<CheckInDeskControl>();
        var layout = (TableLayoutPanel)typeof(CheckInDeskControl).GetField("_mainLayout", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(desk)!;

        layout.MinimumSize.Height.ShouldBeLessThanOrEqualTo(DeskAreaHeight);
    }

    [Fact]
    public void TodayGrid_UsesCompactRows()
    {
        using var panel = new TodayEntriesPanel();
        var grid = (DataGridView)typeof(TodayEntriesPanel).GetField("_gridToday", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(panel)!;

        grid.RowTemplate.Height.ShouldBe(panel.LogicalToDeviceUnits(28));
        grid.ColumnHeadersHeight.ShouldBe(panel.LogicalToDeviceUnits(30));
    }
}
