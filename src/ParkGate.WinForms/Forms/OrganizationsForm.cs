using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Services;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Management form for organizations (FR-ADM-01).
/// Admin only.
/// </summary>
public partial class OrganizationsForm : RtlFormBase
{
    private readonly IOrganizationService? _organizationService;
    private readonly IFormFactory? _formFactory;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<OrganizationsForm>? _logger;

    private readonly List<OrganizationDto> _organizations = [];
    private OrganizationDto? _selectedOrg;

    public OrganizationsForm() : this(null, null, null, null)
    {
    }

    public OrganizationsForm(
        IOrganizationService? organizationService,
        IFormFactory? formFactory,
        INotifyService? notifyService,
        ILogger<OrganizationsForm>? logger)
    {
        InitializeComponent();

        _organizationService = organizationService;
        _formFactory = formFactory;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_Organizations;
        _lblHeader.Text = Strings.Title_Organizations;
        _btnAdd.Text = Strings.Btn_Add;
        _btnEdit.Text = Strings.Btn_Edit;
        _btnToggleActive.Text = Strings.Btn_Deactivate;
        _btnRefresh.Text = Strings.Btn_Refresh;
        _btnClose.Text = Strings.Btn_Close;

        _colCode.HeaderText = Strings.Col_Org_Code;
        _colName.HeaderText = Strings.Col_Org_Name;
        _colCooldown.HeaderText = Strings.Col_Org_Cooldown;
        _colDefault.HeaderText = Strings.Col_Org_Default;
        _colStatus.HeaderText = Strings.Col_Org_Status;
    }

    private void WireEvents()
    {
        Load += async (_, _) => await LoadOrganizationsAsync();
        _btnAdd.Click += async (_, _) => await AddOrganizationAsync();
        _btnEdit.Click += async (_, _) => await EditOrganizationAsync();
        _btnToggleActive.Click += async (_, _) => await ToggleActiveAsync();
        _btnRefresh.Click += async (_, _) => await LoadOrganizationsAsync();
        _chkActiveOnly.CheckedChanged += async (_, _) => await LoadOrganizationsAsync();
        _btnClose.Click += (_, _) => Close();

        _gridOrganizations.SelectionChanged += (_, _) => OnSelectionChanged();
        _gridOrganizations.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0)
            {
                await EditOrganizationAsync();
            }
        };
    }

    public async Task LoadOrganizationsAsync()
    {
        if (_organizationService == null)
        {
            return;
        }

        try
        {
            var activeOnly = _chkActiveOnly.Checked;
            var list = await _organizationService.GetAllAsync(activeOnly);

            _organizations.Clear();
            _organizations.AddRange(list);

            PopulateGrid(_organizations);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading organizations.");
            _notifyService?.Error(Strings.Msg_UnexpectedError);
        }
    }

    private void PopulateGrid(IReadOnlyList<OrganizationDto> list)
    {
        _gridOrganizations.SuspendLayout();
        _gridOrganizations.Rows.Clear();

        foreach (var org in list)
        {
            var cooldownText = org.CooldownMinutes.HasValue
                ? $"{org.CooldownMinutes.Value / 60} ساعة"
                : "افتراضي";

            var defaultText = org.IsDefault ? "نعم" : "لا";
            var statusText = org.IsActive ? Strings.Status_Active : Strings.Status_Inactive;

            var rowIndex = _gridOrganizations.Rows.Add(
                org.Code,
                org.NameAr,
                cooldownText,
                defaultText,
                statusText);

            var row = _gridOrganizations.Rows[rowIndex];
            row.Tag = org;

            if (org.IsActive)
            {
                row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorGranted;
            }
            else
            {
                row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorDenied;
            }
        }

        _gridOrganizations.ResumeLayout(true);
        _lblCount.Text = $"عدد الجهات: {list.Count}";

        if (_gridOrganizations.Rows.Count > 0)
        {
            _gridOrganizations.Rows[0].Selected = true;
        }
        else
        {
            OnSelectionChanged();
        }
    }

    private void OnSelectionChanged()
    {
        if (_gridOrganizations.SelectedRows.Count == 0)
        {
            _selectedOrg = null;
            _btnEdit.Enabled = false;
            _btnToggleActive.Enabled = false;
            return;
        }

        _selectedOrg = _gridOrganizations.SelectedRows[0].Tag as OrganizationDto;
        _btnEdit.Enabled = _selectedOrg != null;
        _btnToggleActive.Enabled = _selectedOrg != null;

        if (_selectedOrg != null)
        {
            _btnToggleActive.Text = _selectedOrg.IsActive
                ? Strings.Btn_Deactivate
                : Strings.Btn_Reactivate;
        }
    }

    public async Task AddOrganizationAsync()
    {
        var dialog = _formFactory?.Create<EditOrganizationDialog>() ??
                     new EditOrganizationDialog(_organizationService, _notifyService, null);

        using (dialog)
        {
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await LoadOrganizationsAsync();
            }
        }
    }

    public async Task EditOrganizationAsync()
    {
        if (_selectedOrg == null)
        {
            return;
        }

        var dialog = _formFactory?.Create<EditOrganizationDialog>() ??
                     new EditOrganizationDialog(_organizationService, _notifyService, null);

        using (dialog)
        {
            dialog.SetOrganization(_selectedOrg);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await LoadOrganizationsAsync();
            }
        }
    }

    public async Task ToggleActiveAsync()
    {
        if (_selectedOrg == null || _organizationService == null)
        {
            return;
        }

        var org = _selectedOrg;
        var confirmMsg = org.IsActive
            ? Strings.Msg_ConfirmDeactivateOrg
            : Strings.Msg_ConfirmReactivateOrg;

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
            var result = org.IsActive
                ? await _organizationService.DeactivateAsync(org.Id)
                : await _organizationService.ReactivateAsync(org.Id);

            if (result.IsSuccess)
            {
                await LoadOrganizationsAsync();
            }
            else
            {
                _notifyService?.Warning(ErrorMessages.GetMessage(result.Error));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error toggling organization active state.");
            _notifyService?.Error(Strings.Msg_UnexpectedError);
        }
    }
}
