using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Workstations.DTOs;
using ParkGate.Application.Workstations.Services;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Modal dialog for editing a workstation's display name and type (FR-AUTH-05, Phase 6).
/// Admin only.
/// </summary>
public partial class EditWorkstationDialog : RtlFormBase
{
    private readonly IWorkstationService _workstationService;
    private readonly INotifyService _notifyService;
    private readonly ILogger<EditWorkstationDialog> _logger;

    private WorkstationDto? _existingWorkstation;

    public WorkstationDto? SavedWorkstation { get; private set; }

    public EditWorkstationDialog(
        IWorkstationService workstationService,
        INotifyService notifyService,
        ILogger<EditWorkstationDialog> logger)
    {
        InitializeComponent();

        _workstationService = workstationService ?? throw new ArgumentNullException(nameof(workstationService));
        _notifyService = notifyService ?? throw new ArgumentNullException(nameof(notifyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeFormText();
        PopulateTypeCombo();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_EditWorkstation;
        _lblHeader.Text = Strings.Title_EditWorkstation;
        _lblMachineNameTitle.Text = Strings.Label_Ws_MachineName;
        _lblDisplayName.Text = Strings.Label_Ws_DisplayName;
        _lblType.Text = Strings.Label_Ws_Type;
        _btnSave.Text = Strings.Btn_Save;
        _btnCancel.Text = Strings.Btn_Cancel;
    }

    private void PopulateTypeCombo()
    {
        _cmbType.Items.Clear();
        _cmbType.Items.Add(new ComboBoxItem<WorkstationType>(WorkstationType.Desk, Strings.Ws_Type_Desk));
        _cmbType.Items.Add(new ComboBoxItem<WorkstationType>(WorkstationType.Gate, Strings.Ws_Type_Gate));
        _cmbType.SelectedIndex = 0;
    }

    private void WireEvents()
    {
        _btnSave.Click += async (_, _) => await SaveAsync();
    }

    public void SetWorkstation(WorkstationDto ws)
    {
        _existingWorkstation = ws;

        _lblMachineNameValue.Text = ws.MachineName;
        _txtDisplayName.Text = ws.DisplayName;

        for (var i = 0; i < _cmbType.Items.Count; i++)
        {
            if (_cmbType.Items[i] is ComboBoxItem<WorkstationType> item && item.Value == ws.Type)
            {
                _cmbType.SelectedIndex = i;
                break;
            }
        }
    }

    public async Task SaveAsync()
    {
        if (_existingWorkstation == null)
        {
            return;
        }

        var displayName = _txtDisplayName.Text.Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            ShowWarning(Strings.Ui_EditWorkstation_01);
            _txtDisplayName.Focus();
            return;
        }

        var selectedType = (_cmbType.SelectedItem as ComboBoxItem<WorkstationType>)?.Value ?? WorkstationType.Desk;

        using (new BusyScope(this, _btnSave, _btnCancel))
        {
            try
            {
                var request = new UpdateWorkstationRequest(_existingWorkstation.Id, displayName, selectedType);
                var result = await _workstationService.UpdateAsync(request);
                if (result.IsSuccess)
                {
                    SavedWorkstation = result.Value;
                    _notifyService.Info(Strings.Msg_SaveWsSuccess);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    ShowWarning(ErrorMessages.GetMessage(result.Error));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workstation.");
                ShowError(UserMessages.Describe(ex));
            }
        }
    }

    private void ShowWarning(string message)
    {
        _notifyService.Warning(message);
    }

    private void ShowError(string message)
    {
        _notifyService.Error(message);
    }

    private sealed class ComboBoxItem<T>
    {
        public T Value { get; }
        public string Text { get; }

        public ComboBoxItem(T value, string text)
        {
            Value = value;
            Text = text;
        }

        public override string ToString() => Text;
    }
}
