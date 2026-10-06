using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Settings.Services;
using ParkGate.Application.Workstations.DTOs;
using ParkGate.Application.Workstations.Services;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Management form for workstations (FR-AUTH-05, Phase 6).
/// Admin only.
/// </summary>
public partial class WorkstationsForm : RtlFormBase
{
    private readonly IWorkstationService? _workstationService;
    private readonly ISettingsService? _settingsService;
    private readonly IFormFactory? _formFactory;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<WorkstationsForm>? _logger;

    private readonly List<WorkstationDto> _workstations = [];
    private WorkstationDto? _selectedWorkstation;
    private string _timeZoneId = "Libya Standard Time";

    public WorkstationsForm() : this(null, null, null, null, null)
    {
    }

    public WorkstationsForm(
        IWorkstationService? workstationService,
        ISettingsService? settingsService,
        IFormFactory? formFactory,
        INotifyService? notifyService,
        ILogger<WorkstationsForm>? logger)
    {
        InitializeComponent();

        _workstationService = workstationService;
        _settingsService = settingsService;
        _formFactory = formFactory;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_Workstations;
        _lblHeader.Text = Strings.Title_Workstations;
        _btnEdit.Text = Strings.Btn_Edit;
        _btnToggleActive.Text = Strings.Btn_Deactivate;
        _btnRefresh.Text = Strings.Btn_Refresh;
        _btnClose.Text = Strings.Btn_Close;

        _colMachineName.HeaderText = Strings.Col_Ws_MachineName;
        _colDisplayName.HeaderText = Strings.Col_Ws_DisplayName;
        _colType.HeaderText = Strings.Col_Ws_Type;
        _colStatus.HeaderText = Strings.Col_Ws_Status;
        _colLastSeen.HeaderText = Strings.Col_Ws_LastSeen;
    }

    private void WireEvents()
    {
        Load += async (_, _) =>
        {
            if (_settingsService != null)
            {
                try
                {
                    var settings = await _settingsService.GetSettingsAsync();
                    _timeZoneId = settings.TimeZoneId;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to load time zone settings.");
                }
            }
            await LoadWorkstationsAsync();
        };

        _btnEdit.Click += async (_, _) => await EditWorkstationAsync();
        _btnToggleActive.Click += async (_, _) => await ToggleActiveAsync();
        _btnRefresh.Click += async (_, _) => await LoadWorkstationsAsync();
        _btnClose.Click += (_, _) => Close();

        _gridWorkstations.SelectionChanged += (_, _) => OnSelectionChanged();
        _gridWorkstations.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0)
            {
                await EditWorkstationAsync();
            }
        };
    }

    public async Task LoadWorkstationsAsync()
    {
        if (_workstationService == null)
        {
            return;
        }

        try
        {
            var list = await _workstationService.GetAllAsync();

            _workstations.Clear();
            _workstations.AddRange(list);

            PopulateGrid(_workstations);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading workstations.");
            _notifyService?.Error(Strings.Msg_UnexpectedError);
        }
    }

    private void PopulateGrid(IReadOnlyList<WorkstationDto> list)
    {
        _gridWorkstations.SuspendLayout();
        _gridWorkstations.Rows.Clear();

        foreach (var ws in list)
        {
            var typeText = ws.Type switch
            {
                WorkstationType.Desk => Strings.Ws_Type_Desk,
                WorkstationType.Gate => Strings.Ws_Type_Gate,
                _ => ws.Type.ToString()
            };

            var statusText = ws.IsActive ? Strings.Status_Active : Strings.Status_Inactive;

            var lastSeenText = ws.LastSeenUtc.HasValue
                ? DateTimeDisplayHelper.FormatDateTime(DateTimeDisplayHelper.ToParkLocalTime(ws.LastSeenUtc.Value, _timeZoneId))
                : "-";

            var rowIndex = _gridWorkstations.Rows.Add(
                ws.MachineName,
                ws.DisplayName,
                typeText,
                statusText,
                lastSeenText);

            var row = _gridWorkstations.Rows[rowIndex];
            row.Tag = ws;

            if (ws.IsActive)
            {
                row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorGranted;
            }
            else
            {
                row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorDenied;
            }
        }

        _gridWorkstations.ResumeLayout(true);
        _lblCount.Text = $"عدد المحطات: {list.Count}";

        if (_gridWorkstations.Rows.Count > 0)
        {
            _gridWorkstations.Rows[0].Selected = true;
        }
        else
        {
            OnSelectionChanged();
        }
    }

    private void OnSelectionChanged()
    {
        if (_gridWorkstations.SelectedRows.Count == 0)
        {
            _selectedWorkstation = null;
            _btnEdit.Enabled = false;
            _btnToggleActive.Enabled = false;
            return;
        }

        _selectedWorkstation = _gridWorkstations.SelectedRows[0].Tag as WorkstationDto;
        _btnEdit.Enabled = _selectedWorkstation != null;
        _btnToggleActive.Enabled = _selectedWorkstation != null;

        if (_selectedWorkstation != null)
        {
            _btnToggleActive.Text = _selectedWorkstation.IsActive
                ? Strings.Btn_Deactivate
                : Strings.Btn_Reactivate;
        }
    }

    public async Task EditWorkstationAsync()
    {
        if (_selectedWorkstation == null)
        {
            return;
        }

        var dialog = _formFactory?.Create<EditWorkstationDialog>() ??
                     new EditWorkstationDialog(_workstationService, _notifyService, null);

        using (dialog)
        {
            dialog.SetWorkstation(_selectedWorkstation);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await LoadWorkstationsAsync();
            }
        }
    }

    public async Task ToggleActiveAsync()
    {
        if (_selectedWorkstation == null || _workstationService == null)
        {
            return;
        }

        var ws = _selectedWorkstation;
        var confirmMsg = ws.IsActive
            ? Strings.Msg_ConfirmDeactivateWs
            : Strings.Msg_ConfirmReactivateWs;

        var confirmed = _notifyService?.Confirm(confirmMsg, Strings.Title_Confirm) ??
                        (MessageBox.Show(
                            confirmMsg,
                            Strings.Title_Confirm,
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign) == DialogResult.Yes);

        if (!confirmed)
        {
            return;
        }

        try
        {
            var result = ws.IsActive
                ? await _workstationService.DeactivateAsync(ws.Id)
                : await _workstationService.ReactivateAsync(ws.Id);

            if (result.IsSuccess)
            {
                await LoadWorkstationsAsync();
            }
            else
            {
                _notifyService?.Warning(ErrorMessages.GetMessage(result.Error));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error toggling workstation active state.");
            _notifyService?.Error(Strings.Msg_UnexpectedError);
        }
    }
}
