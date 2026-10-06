using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Services;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Modal dialog for adding or editing an organization (FR-ADM-01).
/// Admin only.
/// </summary>
public partial class EditOrganizationDialog : RtlFormBase
{
    private readonly IOrganizationService? _organizationService;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<EditOrganizationDialog>? _logger;

    private OrganizationDto? _existingOrg;

    public OrganizationDto? SavedOrganization { get; private set; }

    public EditOrganizationDialog() : this(null, null, null)
    {
    }

    public EditOrganizationDialog(
        IOrganizationService? organizationService,
        INotifyService? notifyService,
        ILogger<EditOrganizationDialog>? logger)
    {
        InitializeComponent();

        _organizationService = organizationService;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_AddOrganization;
        _lblHeader.Text = Strings.Title_AddOrganization;
        _lblCode.Text = Strings.Label_Org_Code;
        _lblNameAr.Text = Strings.Label_Org_Name;
        _chkCustomCooldown.Text = Strings.Label_Org_CustomCooldown;
        _chkIsDefault.Text = Strings.Label_Org_IsDefault;
        _btnSave.Text = Strings.Btn_Save;
        _btnCancel.Text = Strings.Btn_Cancel;
    }

    private void WireEvents()
    {
        _chkCustomCooldown.CheckedChanged += (_, _) =>
        {
            _numCooldownHours.Enabled = _chkCustomCooldown.Checked;
        };

        _btnSave.Click += async (_, _) => await SaveAsync();
    }

    public void SetOrganization(OrganizationDto org)
    {
        _existingOrg = org;

        Text = Strings.Title_EditOrganization;
        _lblHeader.Text = Strings.Title_EditOrganization;

        _txtCode.Text = org.Code;
        _txtNameAr.Text = org.NameAr;

        if (org.CooldownMinutes.HasValue)
        {
            _chkCustomCooldown.Checked = true;
            var hours = Math.Max(1, Math.Min(168, (int)Math.Round((double)org.CooldownMinutes.Value / 60)));
            _numCooldownHours.Value = hours;
        }
        else
        {
            _chkCustomCooldown.Checked = false;
            _numCooldownHours.Value = 12;
        }

        _chkIsDefault.Checked = org.IsDefault;
    }

    public async Task SaveAsync()
    {
        if (_organizationService == null)
        {
            return;
        }

        var code = _txtCode.Text.Trim().ToUpperInvariant();
        var nameAr = _txtNameAr.Text.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            ShowWarning("رمز الجهة مطلوب.");
            _txtCode.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(nameAr))
        {
            ShowWarning("اسم الجهة بالعربية مطلوب.");
            _txtNameAr.Focus();
            return;
        }

        int? cooldownMinutes = _chkCustomCooldown.Checked
            ? (int)_numCooldownHours.Value * 60
            : null;

        var isDefault = _chkIsDefault.Checked;

        using (new BusyScope(this, _btnSave, _btnCancel))
        {
            try
            {
                if (_existingOrg == null)
                {
                    // Create
                    var request = new CreateOrganizationRequest(code, nameAr, cooldownMinutes, isDefault);
                    var result = await _organizationService.CreateAsync(request);
                    if (result.IsSuccess)
                    {
                        SavedOrganization = result.Value;
                        _notifyService?.Info(Strings.Msg_SaveOrgSuccess);
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        ShowWarning(ErrorMessages.GetMessage(result.Error));
                    }
                }
                else
                {
                    // Update
                    var request = new UpdateOrganizationRequest(_existingOrg.Id, code, nameAr, cooldownMinutes, isDefault);
                    var result = await _organizationService.UpdateAsync(request);
                    if (result.IsSuccess)
                    {
                        SavedOrganization = result.Value;
                        _notifyService?.Info(Strings.Msg_SaveOrgSuccess);
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        ShowWarning(ErrorMessages.GetMessage(result.Error));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving organization.");
                ShowError(Strings.Msg_UnexpectedError + Environment.NewLine + ex.Message);
            }
        }
    }

    private void ShowWarning(string message)
    {
        if (_notifyService != null)
        {
            _notifyService.Warning(message);
        }
        else
        {
            MessageBox.Show(
                message,
                Strings.Title_Warning,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void ShowError(string message)
    {
        if (_notifyService != null)
        {
            _notifyService.Error(message);
        }
        else
        {
            MessageBox.Show(
                message,
                Strings.Title_Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
}
