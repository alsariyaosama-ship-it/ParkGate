using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.Organizations.Services;
using ParkGate.Domain.Common;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Dialog form for editing an existing card holder (SPEC §7.3 FR-BEN-03).
/// Supervisor/Admin only.
/// </summary>
public partial class EditBeneficiaryDialog : RtlFormBase
{
    private readonly IBeneficiaryService? _beneficiaryService;
    private readonly IOrganizationService? _orgService;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<EditBeneficiaryDialog>? _logger;

    private BeneficiaryDto? _originalBeneficiary;

    public BeneficiaryDto? UpdatedBeneficiary { get; private set; }

    public EditBeneficiaryDialog() : this(null, null, null, null)
    {
    }

    public EditBeneficiaryDialog(
        IBeneficiaryService? beneficiaryService,
        IOrganizationService? orgService,
        INotifyService? notifyService,
        ILogger<EditBeneficiaryDialog>? logger)
    {
        InitializeComponent();

        _beneficiaryService = beneficiaryService;
        _orgService = orgService;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_EditBeneficiary;
        _lblHeader.Text = Strings.Header_EditBeneficiary;
        _lblFullName.Text = Strings.Col_Ben_FullName + ":";
        _lblOrganization.Text = Strings.Col_Ben_Organization + ":";
        _lblCardNumber.Text = Strings.Col_Ben_CardNumber + ":";
        _lblPhone.Text = Strings.Col_Ben_Phone + ":";
        _lblNationalId.Text = Strings.Col_Ben_NationalId + ":";
        _lblExpiryDate.Text = Strings.Col_Ben_ExpiryDate + ":";
        _lblNotes.Text = Strings.Label_Notes;
        _btnSave.Text = Strings.Btn_Save;
        _btnCancel.Text = Strings.Btn_Cancel;
    }

    private void WireEvents()
    {
        _chkHasExpiry.CheckedChanged += (_, _) =>
        {
            _dtpExpiryDate.Enabled = _chkHasExpiry.Checked;
        };

        _btnSave.Click += async (_, _) => await SaveAsync();
    }

    public async Task LoadOrganizationsAsync(int selectedOrgId = 0)
    {
        if (_orgService == null)
        {
            return;
        }

        try
        {
            var orgs = await _orgService.GetAllAsync(activeOnly: false);
            _cboOrganization.Items.Clear();

            int selectedIndex = 0;
            for (int i = 0; i < orgs.Count; i++)
            {
                var org = orgs[i];
                _cboOrganization.Items.Add(new OrgItem(org.Id, org.NameAr));
                if (org.Id == selectedOrgId)
                {
                    selectedIndex = i;
                }
            }

            if (_cboOrganization.Items.Count > 0)
            {
                _cboOrganization.SelectedIndex = selectedIndex;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load organizations for edit dialog.");
        }
    }

    public async Task SetBeneficiaryAsync(BeneficiaryDto beneficiary)
    {
        _originalBeneficiary = beneficiary;

        await LoadOrganizationsAsync(beneficiary.OrganizationId);

        _txtFullName.Text = beneficiary.FullName;
        _txtCardNumber.Text = beneficiary.CardNumber;
        _txtPhone.Text = beneficiary.Phone;
        _txtNationalId.Text = beneficiary.NationalId ?? string.Empty;
        _txtNotes.Text = beneficiary.Notes ?? string.Empty;

        if (beneficiary.CardExpiryDate.HasValue)
        {
            _chkHasExpiry.Checked = true;
            _dtpExpiryDate.Enabled = true;
            _dtpExpiryDate.Value = beneficiary.CardExpiryDate.Value.ToDateTime(TimeOnly.MinValue);
        }
        else
        {
            _chkHasExpiry.Checked = false;
            _dtpExpiryDate.Enabled = false;
            _dtpExpiryDate.Value = DateTime.Today.AddYears(1);
        }
    }

    public async Task SaveAsync()
    {
        if (_originalBeneficiary == null || _beneficiaryService == null)
        {
            return;
        }

        var orgId = (_cboOrganization.SelectedItem as OrgItem)?.Id ?? _originalBeneficiary.OrganizationId;
        var fullName = _txtFullName.Text.Trim();
        var rawCard = DigitNormalizer.NormalizeDigits(_txtCardNumber.Text.Trim());
        var rawPhone = DigitNormalizer.NormalizeDigits(_txtPhone.Text.Trim());
        var rawNationalId = string.IsNullOrWhiteSpace(_txtNationalId.Text)
            ? null
            : DigitNormalizer.NormalizeDigits(_txtNationalId.Text.Trim());

        DateOnly? expiryDate = _chkHasExpiry.Checked
            ? DateOnly.FromDateTime(_dtpExpiryDate.Value)
            : null;

        var notes = string.IsNullOrWhiteSpace(_txtNotes.Text) ? null : _txtNotes.Text.Trim();

        var request = new UpdateBeneficiaryRequest(
            Id: _originalBeneficiary.Id,
            OrganizationId: orgId,
            CardNumber: rawCard,
            FullName: fullName,
            Phone: rawPhone,
            NationalId: rawNationalId,
            CardExpiryDate: expiryDate,
            Notes: notes);

        using (new BusyScope(this, _btnSave, _btnCancel))
        {
            try
            {
                var result = await _beneficiaryService.UpdateAsync(request);
                if (result.IsSuccess)
                {
                    UpdatedBeneficiary = result.Value;
                    if (_notifyService != null)
                    {
                        _notifyService.Info(Strings.Msg_EditBeneficiarySuccess);
                    }
                    else
                    {
                        MessageBox.Show(
                            Strings.Msg_EditBeneficiarySuccess,
                            Strings.Title_Info,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    var msg = ErrorMessages.GetMessage(result.Error);
                    if (_notifyService != null)
                    {
                        _notifyService.Warning(msg);
                    }
                    else
                    {
                        MessageBox.Show(
                            msg,
                            Strings.Title_Warning,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error updating beneficiary.");
                var msg = Strings.Msg_UnexpectedError + Environment.NewLine + ex.Message;
                if (_notifyService != null)
                {
                    _notifyService.Error(msg);
                }
                else
                {
                    MessageBox.Show(
                        msg,
                        Strings.Title_Error,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                }
            }
        }
    }

    private sealed record OrgItem(int Id, string Name)
    {
        public override string ToString() => Name;
    }
}
