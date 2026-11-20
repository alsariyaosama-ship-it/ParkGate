using System.Drawing;
using System.Windows.Forms;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Controls;

/// <summary>
/// Today's attempts at this desk with the totals line and the void menu (R7.10, split from
/// <see cref="CheckInDeskControl"/>). The desk loads the data and performs the void.
/// </summary>
public partial class TodayEntriesPanel : UserControl
{
    private readonly ContextMenuStrip _todayContextMenu = new();
    private readonly ToolStripMenuItem _menuVoidToday = new();

    public TodayEntriesPanel()
    {
        InitializeComponent();
        Theme.ApplyGridStyle(_gridToday);

        // R5.8: compact rows so five fit on a 1366x768 desk at 100 %; the title moved into the totals line.
        _gridToday.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; // one-word headers
        _gridToday.ColumnHeadersHeight = LogicalToDeviceUnits(CompactHeaderHeight);
        _gridToday.RowTemplate.Height = LogicalToDeviceUnits(CompactRowHeight);
        _gridToday.DefaultCellStyle.Padding = new Padding(3, 0, 3, 0);
        _lblTodayTitle.Text = Strings.Title_TodayDesk;
        _colTime.HeaderText = Strings.Col_Time;
        _colCard.HeaderText = Strings.Col_CardNumber;
        _colName.HeaderText = Strings.Col_HolderName;
        _colOrg.HeaderText = Strings.Col_Org;
        _colPersons.HeaderText = Strings.Col_Persons;
        _colResult.HeaderText = Strings.Col_Result;
        _lblTodaySummary.Text = string.Format(Strings.Summary_TodayStats, 0, 0, 0, 0);

        // SPEC §7.2 FR-CHK-09: void from the grid's context menu.
        _menuVoidToday.Text = Strings.Menu_VoidTodayEntry;
        _menuVoidToday.Click += (_, _) => VoidRequested?.Invoke(this, EventArgs.Empty);
        _todayContextMenu.Items.Add(_menuVoidToday);
        _gridToday.ContextMenuStrip = _todayContextMenu;

        _gridToday.MouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            var hit = _gridToday.HitTest(e.X, e.Y);
            if (hit.RowIndex >= 0)
            {
                _gridToday.ClearSelection();
                _gridToday.Rows[hit.RowIndex].Selected = true;
            }
        };

        _todayContextMenu.Opening += (_, e) => e.Cancel = SelectedVoidableAttempt == null;

        // R5.6: the same action as a visible button (F8 on the desk); the context menu stays.
        Theme.ApplySecondaryButtonStyle(_btnVoid);
        _btnVoid.Text = Strings.Btn_VoidTodayEntry;
        _btnVoid.Click += (_, _) =>
        {
            if (SelectedVoidableAttempt != null)
            {
                VoidRequested?.Invoke(this, EventArgs.Empty);
            }
        };
        _gridToday.SelectionChanged += (_, _) => UpdateVoidButton();
    }

    private const int CompactHeaderHeight = 30;
    private const int CompactRowHeight = 28;

    public event EventHandler? VoidRequested;

    /// <summary>The selected attempt when it can be voided (granted or overridden, not voided yet).</summary>
    public TodayDeskAttemptDto? SelectedVoidableAttempt =>
        _gridToday.SelectedRows.Count > 0 &&
        _gridToday.SelectedRows[0].Tag is TodayDeskAttemptDto attempt &&
        !attempt.IsVoided &&
        attempt.Result != EntryResult.Denied
            ? attempt
            : null;

    /// <summary>
    /// Shows the totals, and the rows when a window handle exists (rows on a never-shown grid are not needed and
    /// touching them from a test thread hangs the host).
    /// </summary>
    public void ShowSummary(TodayDeskSummaryDto summary, string? timeZoneId, bool hostHandleCreated)
    {
        var baseStats = string.Format(
            Strings.Summary_TodayStats,
            summary.TotalGranted,
            summary.TotalOverridden,
            summary.TotalDenied,
            summary.TotalPersons);

        _lblTodaySummary.Text = summary.PersonsPerCategory.Count > 0
            ? $"{baseStats}   |   {string.Join(" · ", summary.PersonsPerCategory.Select(c => $"{c.CategoryName}: {c.Count}"))}"
            : baseStats;

        if (!hostHandleCreated && !_gridToday.IsHandleCreated)
        {
            return;
        }

        _gridToday.SuspendLayout();
        _gridToday.Rows.Clear();
        foreach (var item in summary.RecentAttempts)
        {
            AddRow(item, timeZoneId);
        }

        _gridToday.ResumeLayout(true);
        UpdateVoidButton();
    }

    private void UpdateVoidButton() => _btnVoid.Enabled = SelectedVoidableAttempt != null;

    private void AddRow(TodayDeskAttemptDto item, string? timeZoneId)
    {
        var localTime = DateTimeDisplayHelper.ToParkLocalTime(item.AttemptedAtUtc, timeZoneId);
        var resultText = item.IsVoided
            ? Strings.Result_Voided
            : item.Result switch
            {
                EntryResult.Allowed => Strings.Result_Allowed,
                EntryResult.Denied => Strings.Result_Denied,
                EntryResult.Overridden => Strings.Result_Overridden,
                _ => item.Result.ToString()
            };

        var rowIndex = _gridToday.Rows.Add(
            DateTimeDisplayHelper.FormatTime(localTime),
            item.CardNumber,
            item.BeneficiaryFullName,
            item.OrganizationName,
            item.TotalPersons,
            resultText);

        var row = _gridToday.Rows[rowIndex];
        row.Tag = item;
        row.DefaultCellStyle.ForeColor = item.IsVoided
            ? Color.Gray
            : item.Result switch
            {
                EntryResult.Allowed => StatusBanner.ColorGranted,
                EntryResult.Denied => StatusBanner.ColorDenied,
                EntryResult.Overridden => StatusBanner.ColorWarning,
                _ => row.DefaultCellStyle.ForeColor
            };
    }
}
