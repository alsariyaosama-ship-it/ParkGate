using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.Cities.Services;
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
    private readonly IBeneficiaryService _beneficiaryService;
    private readonly IOrganizationService _orgService;
    private readonly ICityService _cityService;
    private readonly INotifyService _notifyService;
    private readonly ILogger<EditBeneficiaryDialog> _logger;

    private BeneficiaryDto? _originalBeneficiary;

    public BeneficiaryDto? UpdatedBeneficiary { get; private set; }

    public EditBeneficiaryDialog(
        IBeneficiaryService beneficiaryService,
        IOrganizationService orgService,
        ICityService cityService,
        INotifyService notifyService,
        ILogger<EditBeneficiaryDialog> logger)
    {
        InitializeComponent();

        _beneficiaryService = beneficiaryService ?? throw new ArgumentNullException(nameof(beneficiaryService));
        _orgService = orgService ?? throw new ArgumentNullException(nameof(orgService));
        _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
        _notifyService = notifyService ?? throw new ArgumentNullException(nameof(notifyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
        _lblCity.Text = Strings.Col_Ben_City + ":";
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
            _logger.LogError(ex, "Failed to load organizations for edit dialog.");
        }
    }

    private async Task LoadCitiesAsync(BeneficiaryDto beneficiary)
    {
        var cities = new List<CityItem>();
        try
        {
            var result = await _cityService.GetActiveAsync();
            if (result.IsSuccess)
            {
                cities.AddRange(result.Value.Select(c => new CityItem(c.Id, c.NameAr)));
            }
            else
            {
                _logger.LogWarning("Failed to load cities {ErrorCode}.", result.Error.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load cities for edit dialog.");
        }

        // A city that was stopped after it was chosen stays on the holder, so it must stay in the list (BR-32).
        if (beneficiary.CityId is { } cityId && beneficiary.CityName is { } cityName && cities.All(c => c.Id != cityId))
        {
            cities.Add(new CityItem(cityId, cityName));
        }

        _cboCity.SetItems(cities);
        _cboCity.SelectItem(cities.FirstOrDefault(c => c.Id == beneficiary.CityId));
    }

    public async Task SetBeneficiaryAsync(BeneficiaryDto beneficiary)
    {
        _originalBeneficiary = beneficiary;

        await LoadOrganizationsAsync(beneficiary.OrganizationId);
        await LoadCitiesAsync(beneficiary);

        _txtFullName.Text = beneficiary.FullName;
        _txtCardNumber.Text = beneficiary.CardNumber;
        _txtPhone.Text = beneficiary.Phone ?? string.Empty;
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
        if (_originalBeneficiary == null)
        {
            return;
        }

        var orgId = (_cboOrganization.SelectedItem as OrgItem)?.Id ?? _originalBeneficiary.OrganizationId;
        var fullName = _txtFullName.Text.Trim();
        var rawCard = DigitNormalizer.NormalizeDigits(_txtCardNumber.Text.Trim());
        var rawPhone = string.IsNullOrWhiteSpace(_txtPhone.Text)
            ? null
            : DigitNormalizer.NormalizeDigits(_txtPhone.Text.Trim()); // BR-15: optional
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
            Notes: notes,
            CityId: SelectedCityId());

        using (new BusyScope(this, _btnSave, _btnCancel))
        {
            try
            {
                var result = await _beneficiaryService.UpdateAsync(request);
                if (result.IsSuccess)
                {
                    UpdatedBeneficiary = result.Value;
                    if (result.Value.HasOpenCardAlert)
                    {
                        _notifyService.Warning(Strings.Msg_EditBeneficiaryCardAlert); // F8, BR-29: saved anyway
                    }
                    else
                    {
                        _notifyService.Info(Strings.Msg_EditBeneficiarySuccess);
                    }

                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    var msg = ErrorMessages.GetMessage(result.Error);
                    _notifyService.Warning(msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating beneficiary.");
                var msg = UserMessages.Describe(ex);
                _notifyService.Error(msg);
            }
        }
    }

    /// <summary>The chosen city (BR-32), or null; the typed text is resolved first so Enter without leaving the box counts.</summary>
    private int? SelectedCityId()
    {
        _cboCity.CommitText();
        return (_cboCity.SelectedItem as CityItem)?.Id;
    }

    private sealed record OrgItem(int Id, string Name)
    {
        public override string ToString() => Name;
    }

    private sealed record CityItem(int Id, string Name)
    {
        public override string ToString() => Name;
    }
}
