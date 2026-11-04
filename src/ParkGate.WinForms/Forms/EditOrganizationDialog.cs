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

        _grpPolicy.Text = Strings.Grp_Org_Policy;
        _chkMaxFreeCompanions.Text = Strings.Label_Org_MaxFreeCompanions;
        _chkGiftEnabled.Text = Strings.Label_Org_GiftEnabled;
        _lblGiftValue.Text = Strings.Label_Org_GiftValue;
        _lblGiftInterval.Text = Strings.Label_Org_GiftInterval;

        _btnSave.Text = Strings.Btn_Save;
        _btnCancel.Text = Strings.Btn_Cancel;
    }

    private void WireEvents()
    {
        _chkCustomCooldown.CheckedChanged += (_, _) =>
        {
            _numCooldownHours.Enabled = _chkCustomCooldown.Checked;
        };

        _chkMaxFreeCompanions.CheckedChanged += (_, _) =>
        {
            _numMaxFreeCompanions.Enabled = _chkMaxFreeCompanions.Checked;
        };

        _chkGiftEnabled.CheckedChanged += (_, _) =>
        {
            var enabled = _chkGiftEnabled.Checked;
            _numGiftValue.Enabled = enabled;
            _numGiftIntervalDays.Enabled = enabled;
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

        // Policy
        if (org.MaxFreeCompanions.HasValue)
        {
            _chkMaxFreeCompanions.Checked = true;
            _numMaxFreeCompanions.Value = Math.Max(0, Math.Min(50, org.MaxFreeCompanions.Value));
            _numMaxFreeCompanions.Enabled = true;
        }
        else
        {
            _chkMaxFreeCompanions.Checked = false;
            _numMaxFreeCompanions.Value = 0;
            _numMaxFreeCompanions.Enabled = false;
        }

        _chkGiftEnabled.Checked = org.GiftEnabled;
        _numGiftValue.Value = org.GiftValue >= 0m ? org.GiftValue : 0m;
        _numGiftIntervalDays.Value = org.GiftIntervalDays >= 1 && org.GiftIntervalDays <= 365 ? org.GiftIntervalDays : 30;
        _numGiftValue.Enabled = org.GiftEnabled;
        _numGiftIntervalDays.Enabled = org.GiftEnabled;
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

        int? maxFreeCompanions = _chkMaxFreeCompanions.Checked
            ? (int)_numMaxFreeCompanions.Value
            : null;

        var giftEnabled = _chkGiftEnabled.Checked;
        var giftValue = _numGiftValue.Value;
        var giftIntervalDays = (int)_numGiftIntervalDays.Value;

        using (new BusyScope(this, _btnSave, _btnCancel))
        {
            try
            {
                if (_existingOrg == null)
                {
                    // Create
                    var createRequest = new CreateOrganizationRequest(code, nameAr, cooldownMinutes, isDefault);
                    var createResult = await _organizationService.CreateAsync(createRequest);
                    if (!createResult.IsSuccess)
                    {
                        ShowWarning(ErrorMessages.GetMessage(createResult.Error));
                        return;
                    }

                    var orgId = createResult.Value.Id;
                    var policyRequest = new SetOrganizationPolicyRequest(orgId, maxFreeCompanions, giftEnabled, giftValue, giftIntervalDays);
                    var policyResult = await _organizationService.SetPolicyAsync(policyRequest);
                    if (!policyResult.IsSuccess)
                    {
                        ShowWarning(ErrorMessages.GetMessage(policyResult.Error));
                        return;
                    }

                    SavedOrganization = policyResult.Value;
                    _notifyService?.Info(Strings.Msg_SaveOrgSuccess);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    // Update
                    var updateRequest = new UpdateOrganizationRequest(_existingOrg.Id, code, nameAr, cooldownMinutes, isDefault);
                    var updateResult = await _organizationService.UpdateAsync(updateRequest);
                    if (!updateResult.IsSuccess)
                    {
                        ShowWarning(ErrorMessages.GetMessage(updateResult.Error));
                        return;
                    }

                    var policyRequest = new SetOrganizationPolicyRequest(_existingOrg.Id, maxFreeCompanions, giftEnabled, giftValue, giftIntervalDays);
                    var policyResult = await _organizationService.SetPolicyAsync(policyRequest);
                    if (!policyResult.IsSuccess)
                    {
                        ShowWarning(ErrorMessages.GetMessage(policyResult.Error));
                        return;
                    }

                    SavedOrganization = policyResult.Value;
                    _notifyService?.Info(Strings.Msg_SaveOrgSuccess);
                    DialogResult = DialogResult.OK;
                    Close();
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
