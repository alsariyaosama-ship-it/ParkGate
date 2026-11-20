using System.Reflection;
using System.Windows.Forms;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Controls;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>R5.6: a visible void button above today's grid, enabled for a voidable row only.</summary>
public sealed class TodayEntriesPanelTests
{
    [Fact]
    public void VoidButton_NoSelection_IsDisabled()
    {
        using var panel = new TodayEntriesPanel();
        panel.ShowSummary(Summary(Attempt(EntryResult.Allowed)), timeZoneId: null, hostHandleCreated: true);
        Grid(panel).ClearSelection();

        VoidButton(panel).Enabled.ShouldBeFalse();
    }

    [Theory]
    [InlineData(EntryResult.Allowed, false, true)]
    [InlineData(EntryResult.Overridden, false, true)]
    [InlineData(EntryResult.Denied, false, false)]
    [InlineData(EntryResult.Allowed, true, false)]
    public void VoidButton_FollowsTheSelectedRow(EntryResult result, bool voided, bool enabled)
    {
        using var panel = new TodayEntriesPanel();
        panel.ShowSummary(Summary(Attempt(result, voided)), timeZoneId: null, hostHandleCreated: true);

        Grid(panel).ClearSelection();
        Grid(panel).Rows[0].Selected = true;

        VoidButton(panel).Enabled.ShouldBe(enabled);
    }

    [Fact]
    public void VoidButton_Click_RaisesVoidRequested()
    {
        using var panel = new TodayEntriesPanel();
        panel.ShowSummary(Summary(Attempt(EntryResult.Allowed)), timeZoneId: null, hostHandleCreated: true);
        Grid(panel).Rows[0].Selected = true;
        var raised = 0;
        panel.VoidRequested += (_, _) => raised++;

        VoidButton(panel).PerformClick();

        raised.ShouldBe(1);
    }

    [Fact]
    public void VoidButton_AfterTheListReloads_FollowsTheNewSelection()
    {
        using var panel = new TodayEntriesPanel();
        panel.ShowSummary(Summary(Attempt(EntryResult.Allowed)), timeZoneId: null, hostHandleCreated: true);
        Grid(panel).Rows[0].Selected = true;

        panel.ShowSummary(Summary(Attempt(EntryResult.Allowed, voided: true)), timeZoneId: null, hostHandleCreated: true);

        VoidButton(panel).Enabled.ShouldBe(panel.SelectedVoidableAttempt is not null);
        panel.SelectedVoidableAttempt.ShouldBeNull();
    }

    private static TodayDeskAttemptDto Attempt(EntryResult result, bool voided = false) =>
        new(Guid.NewGuid(), DateTime.UtcNow, "100200", "محمد أحمد", "صندوق التضامن", 2, result,
            result == EntryResult.Denied ? DenialReason.CooldownActive : null, voided);

    private static TodayDeskSummaryDto Summary(params TodayDeskAttemptDto[] attempts) =>
        new(DateOnly.FromDateTime(DateTime.UtcNow), 1, "Desk 1", 1, 0, 0, 2, [], attempts);

    private static DataGridView Grid(TodayEntriesPanel panel) => Field<DataGridView>(panel, "_gridToday");

    private static Button VoidButton(TodayEntriesPanel panel) => Field<Button>(panel, "_btnVoid");

    private static T Field<T>(object target, string name) where T : class =>
        (target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(target) as T)
        ?? throw new InvalidOperationException($"Field {name} not found.");
}
