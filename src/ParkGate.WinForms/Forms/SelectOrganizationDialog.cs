using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Lets the operator choose the holder when one card number exists in several organizations (SPEC flow A2, R4.6).
/// Enter or double-click chooses, Esc cancels. Cancelling returns no holder: the desk never picks one for the operator.
/// </summary>
public partial class SelectOrganizationDialog : RtlFormBase
{
    private IReadOnlyList<BeneficiaryLookupResultDto> _matches = [];

    public SelectOrganizationDialog()
    {
        InitializeComponent();

        Text = Strings.Title_SelectOrganization;
        _lblHeader.Text = Strings.Header_SelectOrganization;
        _btnSelect.Text = Strings.Btn_SelectOrg;
        _btnCancel.Text = Strings.Btn_Cancel;

        _btnSelect.Click += (_, _) => Choose();
        _lstOrganizations.DoubleClick += (_, _) => Choose();
    }

    /// <summary>The holder chosen with Enter, the button or a double-click; null when cancelled.</summary>
    public BeneficiaryLookupResultDto? SelectedMatch { get; private set; }

    public void SetMatches(IReadOnlyList<BeneficiaryLookupResultDto> matches)
    {
        _matches = matches;
        _lstOrganizations.Items.Clear();
        foreach (var match in matches)
        {
            _lstOrganizations.Items.Add(string.Format(
                Strings.Format_OrganizationHolderItem, match.Beneficiary.OrganizationName, match.Beneficiary.FullName));
        }

        _lstOrganizations.SelectedIndex = matches.Count > 0 ? 0 : -1;
    }

    internal void Choose()
    {
        var index = _lstOrganizations.SelectedIndex;
        if (index < 0 || index >= _matches.Count)
        {
            return;
        }

        SelectedMatch = _matches[index];
        DialogResult = DialogResult.OK;
    }
}
