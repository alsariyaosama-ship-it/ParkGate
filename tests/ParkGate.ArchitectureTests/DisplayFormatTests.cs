using System.Windows.Forms;
using ParkGate.WinForms.Common;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>R5.19: dates read dd/MM/yyyy everywhere (AGENTS.md §5.8), and grid headers are never cut.</summary>
public sealed class DisplayFormatTests
{
    public static TheoryData<Type> Screens()
    {
        var data = new TheoryData<Type>();
        foreach (var type in UiCompositionTests.DesignerTypes())
        {
            data.Add(type);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Screens))]
    public void EveryDateBox_ShowsDayMonthYear(Type screen)
    {
        using var control = (Control)TestUi.Create(screen);

        foreach (var picker in All(control).OfType<DateTimePicker>())
        {
            picker.Format.ShouldBe(DateTimePickerFormat.Custom, $"{screen.Name}.{picker.Name}");
            picker.CustomFormat.ShouldBe("dd/MM/yyyy", $"{screen.Name}.{picker.Name}");
        }
    }

    [Fact]
    public void GridStyle_HeaderGrowsWithItsTextAndRowsFollowDpi()
    {
        using var grid = new DataGridView();

        Theme.ApplyGridStyle(grid);

        grid.ColumnHeadersHeightSizeMode.ShouldBe(DataGridViewColumnHeadersHeightSizeMode.AutoSize);
        grid.ColumnHeadersDefaultCellStyle.WrapMode.ShouldBe(DataGridViewTriState.False);
        grid.RowTemplate.Height.ShouldBe(grid.LogicalToDeviceUnits(Theme.GridRowHeight));
    }

    [Fact]
    public void FitColumnsToHeaders_NoColumnNarrowerThanItsHeader()
    {
        using var grid = new DataGridView();
        Theme.ApplyGridStyle(grid);
        grid.Columns.Add("a", "رقم القسيمة");
        grid.Columns.Add("b", "وقت الإصدار");

        Theme.FitColumnsToHeaders(grid);

        foreach (DataGridViewColumn column in grid.Columns)
        {
            var needed = TextRenderer.MeasureText(column.HeaderText, grid.ColumnHeadersDefaultCellStyle.Font, new System.Drawing.Size(int.MaxValue, int.MaxValue), TextFormatFlags.SingleLine | TextFormatFlags.NoPadding).Width;
            column.MinimumWidth.ShouldBeGreaterThan(needed + grid.ColumnHeadersDefaultCellStyle.Padding.Horizontal);
        }
    }

    [Fact]
    public void GiftReceipts_FitsItsColumnsToTheHeaders()
    {
        using var form = TestUi.Create<ParkGate.WinForms.Forms.GiftReceiptsForm>();
        var grid = (DataGridView)typeof(ParkGate.WinForms.Forms.GiftReceiptsForm)
            .GetField("_gridReceipts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(form)!;

        grid.Columns.Cast<DataGridViewColumn>().ShouldAllBe(c => c.MinimumWidth > 20);
    }

    private static IEnumerable<Control> All(Control root) =>
        root.Controls.Cast<Control>().SelectMany(c => All(c).Prepend(c));
}
