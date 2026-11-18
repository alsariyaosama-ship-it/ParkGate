using System.Windows.Forms;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Controls;

/// <summary>
/// The desk's inline "new visitor" form (R7.10, split from <see cref="CheckInDeskControl"/>). View only: the desk
/// validates the fields and saves through the Application services.
/// </summary>
public partial class InlineRegistrationPanel : UserControl
{
    public InlineRegistrationPanel()
    {
        InitializeComponent();

        _grpRegistration.Text = Strings.Title_InlineRegistration;
        _lblRegOrganization.Text = Strings.Label_RegOrganization;
        _lblRegFullName.Text = Strings.Label_RegFullName;
        _lblRegPhone.Text = Strings.Label_RegPhone;
        _lblRegNationalId.Text = Strings.Label_RegNationalId;
        _lblRegExpiry.Text = Strings.Label_RegExpiry;
        _lblRegNotes.Text = Strings.Label_RegNotes;
        _btnSaveReg.Text = Strings.Btn_SaveAndContinue;
        _btnCancelReg.Text = Strings.Btn_CancelReg;

        _btnSaveReg.Click += (_, _) => SaveRequested?.Invoke(this, EventArgs.Empty);
        _btnCancelReg.Click += (_, _) => CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? SaveRequested;

    public event EventHandler? CancelRequested;

    public string FullName => _txtRegFullName.Text.Trim();

    public string Phone => _txtRegPhone.Text.Trim();

    public string? NationalId => string.IsNullOrWhiteSpace(_txtRegNationalId.Text) ? null : _txtRegNationalId.Text.Trim();

    public string? Notes => string.IsNullOrWhiteSpace(_txtRegNotes.Text) ? null : _txtRegNotes.Text.Trim();

    public DateOnly? ExpiryDate => _dtpRegExpiry.Checked ? DateOnly.FromDateTime(_dtpRegExpiry.Value) : null;

    public int? SelectedOrganizationId => (_cmbRegOrganization.SelectedItem as OrgItem)?.Id;

    public bool IsOrganizationListOpen => _cmbRegOrganization.DroppedDown;

    /// <summary>True when the operator typed or picked anything that a discard would lose.</summary>
    public bool HasInput =>
        !string.IsNullOrWhiteSpace(_txtRegFullName.Text) ||
        !string.IsNullOrWhiteSpace(_txtRegPhone.Text) ||
        !string.IsNullOrWhiteSpace(_txtRegNationalId.Text) ||
        !string.IsNullOrWhiteSpace(_txtRegNotes.Text) ||
        _dtpRegExpiry.Checked;

    /// <summary>The save and cancel buttons, for the desk's busy scope while saving.</summary>
    public Control[] ActionButtons => [_btnSaveReg, _btnCancelReg];

    public void SetOrganizations(IEnumerable<(int Id, string Name)> organizations)
    {
        _cmbRegOrganization.Items.Clear();
        foreach (var (id, name) in organizations)
        {
            _cmbRegOrganization.Items.Add(new OrgItem(id, name));
        }
    }

    public void SelectOrganization(int? organizationId)
    {
        _cmbRegOrganization.SelectedItem = _cmbRegOrganization.Items.OfType<OrgItem>()
            .FirstOrDefault(i => i.Id == organizationId);
    }

    public void ClearFields()
    {
        _txtRegFullName.Clear();
        _txtRegPhone.Clear();
        _txtRegNationalId.Clear();
        _dtpRegExpiry.Checked = false;
        _txtRegNotes.Clear();
    }

    public void FocusFullName() => _txtRegFullName.Focus();

    public void FocusPhone() => _txtRegPhone.Focus();

    public void FocusOrganization() => _cmbRegOrganization.Focus();

    private sealed record OrgItem(int Id, string Name)
    {
        public override string ToString() => Name;
    }
}
