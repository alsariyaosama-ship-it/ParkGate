using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.CheckIn.Services;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Services;
using ParkGate.Domain.Common;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Controls;

/// <summary>
/// Holders management user control (SPEC §7.3 FR-BEN-01..04).
/// Search, details, full history, edit, status changes, and void entry.
/// </summary>
public partial class BeneficiariesControl : UserControl
{
    private readonly IBeneficiaryService? _beneficiaryService;
    private readonly ICheckInService? _checkInService;
    private readonly ISettingsService? _settingsService;
    private readonly ICurrentUser? _currentUser;
    private readonly ICurrentWorkstation? _currentWorkstation;
    private readonly IFormFactory? _formFactory;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<BeneficiariesControl>? _logger;

    private readonly List<BeneficiaryDto> _loadedBeneficiaries = [];
    private BeneficiaryDto? _selectedBeneficiary;
    private string? _timeZoneId = "Libya Standard Time";
    private readonly ContextMenuStrip _historyContextMenu = new();

    public event EventHandler<string>? RequestCheckIn;

    public BeneficiariesControl() : this(null, null, null, null, null, null, null, null)
    {
    }

    public BeneficiariesControl(
        IBeneficiaryService? beneficiaryService,
        ICheckInService? checkInService,
        ISettingsService? settingsService,
        ICurrentUser? currentUser,
        ICurrentWorkstation? currentWorkstation,
        IFormFactory? formFactory,
        INotifyService? notifyService,
        ILogger<BeneficiariesControl>? logger)
    {
        InitializeComponent();

        _beneficiaryService = beneficiaryService;
        _checkInService = checkInService;
        _settingsService = settingsService;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
        _formFactory = formFactory;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        ApplyStyles();
        ClearBeneficiaryDetails();
        ClearHistoryGrid();
        ApplyPermissions();
        WireEvents();
    }

    private void ApplyStyles()
    {
        Theme.ApplyGridStyle(_gridBeneficiaries);
        Theme.ApplyGridStyle(_gridHistory);
        Theme.ApplyPrimaryButtonStyle(_btnCheckInDirect);
        Theme.ApplySecondaryButtonStyle(_btnEdit);
        Theme.ApplySecondaryButtonStyle(_btnChangeStatus);
        Theme.ApplySecondaryButtonStyle(_btnVoidHistoryEntry);
        Theme.ApplyPrimaryButtonStyle(_btnSearch);
        Theme.ApplySecondaryButtonStyle(_btnClearSearch);
        Theme.ApplyPrimaryButtonStyle(_btnIssueQr);
    }

    private void InitializeFormText()
    {
        _lblSearch.Text = Strings.Header_BeneficiariesSearch + ":";
        _txtSearch.PlaceholderText = Strings.Placeholder_SearchBeneficiary;
        _btnSearch.Text = Strings.Btn_Search;
        _btnClearSearch.Text = Strings.Btn_ClearSearch;

        _grpResults.Text = "قائمة المستفيدين";
        _colCardNumber.HeaderText = Strings.Col_Ben_CardNumber;
        _colOrganization.HeaderText = Strings.Col_Ben_Organization;
        _colFullName.HeaderText = Strings.Col_Ben_FullName;
        _colPhone.HeaderText = Strings.Col_Ben_Phone;
        _colNationalId.HeaderText = Strings.Col_Ben_NationalId;
        _colStatus.HeaderText = Strings.Col_Ben_Status;
        _colLastEntry.HeaderText = Strings.Col_Ben_LastEntry;
        _colExpiryDate.HeaderText = Strings.Col_Ben_ExpiryDate;

        _grpHolderDetails.Text = Strings.Group_Ben_Details;
        _lblDetailNameTitle.Text = Strings.Col_Ben_FullName + ":";
        _lblDetailCardTitle.Text = Strings.Col_Ben_CardNumber + ":";
        _lblDetailOrgTitle.Text = Strings.Col_Ben_Organization + ":";
        _lblDetailPhoneTitle.Text = Strings.Col_Ben_Phone + ":";
        _lblDetailNidTitle.Text = Strings.Col_Ben_NationalId + ":";
        _lblDetailExpiryTitle.Text = Strings.Col_Ben_ExpiryDate + ":";
        _lblDetailStatusTitle.Text = Strings.Col_Ben_Status + ":";
        _lblDetailReasonTitle.Text = "سبب الحالة:";
        _lblDetailNotesTitle.Text = Strings.Label_Notes;

        _btnEdit.Text = Strings.Btn_EditBeneficiary;
        _btnChangeStatus.Text = Strings.Btn_ChangeStatus;
        _btnCheckInDirect.Text = Strings.Btn_CheckInDirect;

        _grpHistory.Text = Strings.Group_Ben_History;
        _colHistTime.HeaderText = Strings.Col_Hist_Time;
        _colHistBusinessDate.HeaderText = Strings.Col_Hist_BusinessDate;
        _colHistWorkstation.HeaderText = Strings.Col_Hist_Workstation;
        _colHistResult.HeaderText = Strings.Col_Hist_Result;
        _colHistPersons.HeaderText = Strings.Col_Hist_Persons;
        _colHistDetails.HeaderText = Strings.Col_Hist_Details;
        _colHistStatus.HeaderText = Strings.Col_Hist_Status;
        _colHistVoidReason.HeaderText = Strings.Col_Hist_VoidReason;

        _btnVoidHistoryEntry.Text = Strings.Menu_VoidTodayEntry;
    }

    public void ApplyPermissions()
    {
        var canEdit = _currentUser?.Has(Permission.EditBeneficiary) ?? false;
        var canChangeStatus = _currentUser?.Has(Permission.ChangeBeneficiaryStatus) ?? false;
        var canCheckIn = _currentUser?.Has(Permission.CheckIn) ?? false;

        _btnEdit.Visible = canEdit;
        _btnChangeStatus.Visible = canChangeStatus;
        _btnCheckInDirect.Visible = canCheckIn;
        _btnIssueQr.Visible = canEdit; // Assume same permission as editing beneficiary
    }

    private void WireEvents()
    {
        Load += async (_, _) => await OnControlLoadedAsync();

        _btnSearch.Click += async (_, _) => await PerformSearchAsync();
        _btnClearSearch.Click += async (_, _) => await ClearSearchAsync();

        _txtSearch.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await PerformSearchAsync();
            }
        };

        _gridBeneficiaries.SelectionChanged += async (_, _) => await OnBeneficiarySelectionChangedAsync();

        _btnEdit.Click += async (_, _) => await OpenEditDialogAsync();
        _btnChangeStatus.Click += async (_, _) => await OpenChangeStatusDialogAsync();
        _btnCheckInDirect.Click += (_, _) => OnCheckInDirectClicked();
        _btnIssueQr.Click += async (_, _) => await OpenIssueQrDialogAsync();

        _gridHistory.SelectionChanged += (_, _) => UpdateVoidButtonState();
        _btnVoidHistoryEntry.Click += async (_, _) => await VoidSelectedHistoryEntryAsync();

        // History grid context menu
        var voidMenuItem = new ToolStripMenuItem(Strings.Menu_VoidTodayEntry);
        voidMenuItem.Click += async (_, _) => await VoidSelectedHistoryEntryAsync();
        _historyContextMenu.Items.Add(voidMenuItem);
        _gridHistory.ContextMenuStrip = _historyContextMenu;
        _historyContextMenu.Opening += (_, e) =>
        {
            var canVoid = CanVoidSelectedHistory();
            voidMenuItem.Enabled = canVoid;
            if (!canVoid && _gridHistory.SelectedRows.Count == 0)
            {
                e.Cancel = true;
            }
        };
    }

    private async Task OnControlLoadedAsync()
    {
        try
        {
            if (_settingsService != null)
            {
                var settings = await _settingsService.GetSettingsAsync();
                _timeZoneId = settings.TimeZoneId;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load time zone in BeneficiariesControl.");
        }
    }

    public async Task EnsureLoadedAsync()
    {
        if (_loadedBeneficiaries.Count == 0)
        {
            await PerformSearchAsync();
        }
    }

    public void FocusSearchBox()
    {
        _txtSearch.Focus();
        _txtSearch.SelectAll();
    }

    public void SetSearchQuery(string query)
    {
        _txtSearch.Text = query;
    }

    public async Task PerformSearchAsync()
    {
        if (_beneficiaryService == null)
        {
            return;
        }

        var rawTerm = _txtSearch.Text.Trim();
        var term = DigitNormalizer.NormalizeDigits(rawTerm);

        using (new BusyScope(this, _btnSearch, _btnClearSearch))
        {
            try
            {
                var result = await _beneficiaryService.SearchAsync(term, maxResults: 100);
                if (result.IsSuccess)
                {
                    _loadedBeneficiaries.Clear();
                    _loadedBeneficiaries.AddRange(result.Value);

                    PopulateBeneficiariesGrid(_loadedBeneficiaries);
                    _lblSearchResultCount.Text = string.IsNullOrWhiteSpace(term)
                        ? $"إجمالي المستفيدين: {_loadedBeneficiaries.Count}"
                        : string.Format(Strings.Summary_SearchResults, _loadedBeneficiaries.Count);
                }
                else
                {
                    var msg = ErrorMessages.GetMessage(result.Error);
                    _notifyService?.Warning(msg);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error searching beneficiaries.");
                _notifyService?.Error(Strings.Msg_UnexpectedError);
            }
        }
    }

    public async Task ClearSearchAsync()
    {
        _txtSearch.Clear();
        await PerformSearchAsync();
        _txtSearch.Focus();
    }

    private void ClearSearch()
    {
        _ = ClearSearchAsync();
    }

    private void PopulateBeneficiariesGrid(IReadOnlyList<BeneficiaryDto> beneficiaries)
    {
        _gridBeneficiaries.SuspendLayout();
        _gridBeneficiaries.Rows.Clear();

        var isOperator = _currentUser?.IsInRole(UserRole.Operator) ?? false;

        foreach (var b in beneficiaries)
        {
            var displayNid = FormatNationalId(b.NationalId, isOperator);
            var statusText = GetStatusArabic(b.Status);
            var expiryText = b.CardExpiryDate?.ToString("dd/MM/yyyy") ?? "-";
            var lastEntryText = b.LastGrantedEntryUtc.HasValue
                ? DateTimeDisplayHelper.FormatDateTime(DateTimeDisplayHelper.ToParkLocalTime(b.LastGrantedEntryUtc.Value, _timeZoneId))
                : "-";

            var rowIndex = _gridBeneficiaries.Rows.Add(
                b.CardNumber,
                b.OrganizationName,
                b.FullName,
                b.Phone,
                displayNid,
                statusText,
                lastEntryText,
                expiryText);

            var row = _gridBeneficiaries.Rows[rowIndex];
            row.Tag = b;

            // Color status
            if (b.Status == BeneficiaryStatus.Active)
            {
                row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorGranted;
            }
            else if (b.Status == BeneficiaryStatus.Suspended)
            {
                row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorWarning;
            }
            else if (b.Status == BeneficiaryStatus.Blocked)
            {
                row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorDenied;
            }
        }

        _gridBeneficiaries.ResumeLayout(true);

        if (_gridBeneficiaries.Rows.Count > 0)
        {
            _gridBeneficiaries.Rows[0].Selected = true;
        }
        else
        {
            ClearBeneficiaryDetails();
            ClearHistoryGrid();
        }
    }

    private static string FormatNationalId(string? nationalId, bool isOperator)
    {
        if (string.IsNullOrWhiteSpace(nationalId))
        {
            return "-";
        }

        if (isOperator && nationalId.Length > 4)
        {
            return new string('*', nationalId.Length - 4) + nationalId[^4..];
        }

        return nationalId;
    }

    private async Task OnBeneficiarySelectionChangedAsync()
    {
        if (_gridBeneficiaries.SelectedRows.Count == 0)
        {
            ClearBeneficiaryDetails();
            ClearHistoryGrid();
            return;
        }

        var b = _gridBeneficiaries.SelectedRows[0].Tag as BeneficiaryDto;
        if (b == null)
        {
            return;
        }

        _selectedBeneficiary = b;
        DisplayBeneficiaryDetails(b);
        await LoadBeneficiaryHistoryAsync(b.Id);
    }

    private void DisplayBeneficiaryDetails(BeneficiaryDto b)
    {
        var isOperator = _currentUser?.IsInRole(UserRole.Operator) ?? false;

        _lblDetailName.Text = b.FullName;
        _lblDetailCard.Text = b.CardNumber;
        _lblDetailOrg.Text = b.OrganizationName;
        _lblDetailPhone.Text = b.Phone;
        _lblDetailNid.Text = FormatNationalId(b.NationalId, isOperator);
        _lblDetailExpiry.Text = b.CardExpiryDate?.ToString("dd/MM/yyyy") ?? "غير محدد";

        _lblDetailStatus.Text = GetStatusArabic(b.Status);
        _lblDetailStatus.ForeColor = b.Status switch
        {
            BeneficiaryStatus.Active => StatusBanner.ColorGranted,
            BeneficiaryStatus.Suspended => StatusBanner.ColorWarning,
            BeneficiaryStatus.Blocked => StatusBanner.ColorDenied,
            _ => Color.Black
        };

        _lblDetailReason.Text = !string.IsNullOrWhiteSpace(b.StatusReason) ? b.StatusReason : "-";
        _lblDetailNotes.Text = !string.IsNullOrWhiteSpace(b.Notes) ? b.Notes : "-";

        _btnEdit.Enabled = true;
        _btnChangeStatus.Enabled = true;
        _btnCheckInDirect.Enabled = true;
        _btnIssueQr.Enabled = b.Status == BeneficiaryStatus.Active;
    }

    private void ClearBeneficiaryDetails()
    {
        _selectedBeneficiary = null;
        _lblDetailName.Text = "-";
        _lblDetailCard.Text = "-";
        _lblDetailOrg.Text = "-";
        _lblDetailPhone.Text = "-";
        _lblDetailNid.Text = "-";
        _lblDetailExpiry.Text = "-";
        _lblDetailStatus.Text = "-";
        _lblDetailStatus.ForeColor = Color.Black;
        _lblDetailReason.Text = "-";
        _lblDetailNotes.Text = "-";

        _btnEdit.Enabled = false;
        _btnChangeStatus.Enabled = false;
        _btnCheckInDirect.Enabled = false;
        _btnIssueQr.Enabled = false;
    }

    private async Task LoadBeneficiaryHistoryAsync(Guid beneficiaryId)
    {
        if (_beneficiaryService == null)
        {
            return;
        }

        try
        {
            var historyResult = await _beneficiaryService.GetHistoryAsync(beneficiaryId, maxResults: 100);
            if (historyResult.IsSuccess)
            {
                PopulateHistoryGrid(historyResult.Value);
            }
            else
            {
                ClearHistoryGrid();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load beneficiary history.");
            ClearHistoryGrid();
        }
    }

    private void PopulateHistoryGrid(IReadOnlyList<EntryAttemptDto> attempts)
    {
        _gridHistory.SuspendLayout();
        _gridHistory.Rows.Clear();

        foreach (var item in attempts)
        {
            var localTime = DateTimeDisplayHelper.ToParkLocalTime(item.AttemptedAtUtc, _timeZoneId);
            var timeStr = DateTimeDisplayHelper.FormatDateTime(localTime);
            var dateStr = item.BusinessDate.ToString("dd/MM/yyyy");

            string resultText = item.Result switch
            {
                EntryResult.Allowed => Strings.Result_Allowed,
                EntryResult.Denied => Strings.Result_Denied,
                EntryResult.Overridden => Strings.Result_Overridden,
                _ => item.Result.ToString()
            };

            string statusText = item.IsVoided ? Strings.Status_Attempt_Voided : Strings.Status_Attempt_Active;

            // Details / companions breakdown
            string detailsText;
            if (item.Companions.Count > 0)
            {
                detailsText = string.Join(" · ", item.Companions.Select(c => $"{c.Count} فرد"));
            }
            else if (!string.IsNullOrWhiteSpace(item.OverrideReason))
            {
                detailsText = $"تجاوز: {item.OverrideReason}";
            }
            else if (item.DenialReason.HasValue)
            {
                detailsText = $"مرفوض: {item.DenialReason.Value}";
            }
            else
            {
                detailsText = "دخول فردي";
            }

            var rowIndex = _gridHistory.Rows.Add(
                timeStr,
                dateStr,
                $"#{item.WorkstationId}",
                resultText,
                item.TotalPersons,
                detailsText,
                statusText,
                item.VoidReason ?? "-");

            var row = _gridHistory.Rows[rowIndex];
            row.Tag = item;

            if (item.IsVoided)
            {
                row.DefaultCellStyle.ForeColor = Color.Gray;
            }
            else if (item.Result == EntryResult.Allowed)
            {
                row.Cells[_colHistResult.Index].Style.ForeColor = StatusBanner.ColorGranted;
            }
            else if (item.Result == EntryResult.Denied)
            {
                row.Cells[_colHistResult.Index].Style.ForeColor = StatusBanner.ColorDenied;
            }
            else if (item.Result == EntryResult.Overridden)
            {
                row.Cells[_colHistResult.Index].Style.ForeColor = StatusBanner.ColorWarning;
            }
        }

        _gridHistory.ResumeLayout(true);
        UpdateVoidButtonState();
    }

    private void ClearHistoryGrid()
    {
        _gridHistory.Rows.Clear();
        UpdateVoidButtonState();
    }

    private bool CanVoidSelectedHistory()
    {
        if (_gridHistory.SelectedRows.Count == 0)
        {
            return false;
        }

        if (_gridHistory.SelectedRows[0].Tag is not EntryAttemptDto attempt)
        {
            return false;
        }

        return !attempt.IsVoided && attempt.Result != EntryResult.Denied;
    }

    private void UpdateVoidButtonState()
    {
        _btnVoidHistoryEntry.Enabled = CanVoidSelectedHistory();
    }

    private async Task OpenEditDialogAsync()
    {
        if (_selectedBeneficiary == null)
        {
            return;
        }

        var dialog = _formFactory?.Create<EditBeneficiaryDialog>() ??
                     new EditBeneficiaryDialog(_beneficiaryService, null, _notifyService, null);

        await dialog.SetBeneficiaryAsync(_selectedBeneficiary);

        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.UpdatedBeneficiary != null)
        {
            var updated = dialog.UpdatedBeneficiary;
            _selectedBeneficiary = updated;
            DisplayBeneficiaryDetails(updated);

            // Update row in grid
            if (_gridBeneficiaries.SelectedRows.Count > 0)
            {
                var row = _gridBeneficiaries.SelectedRows[0];
                row.Tag = updated;
                row.Cells[_colCardNumber.Index].Value = updated.CardNumber;
                row.Cells[_colOrganization.Index].Value = updated.OrganizationName;
                row.Cells[_colFullName.Index].Value = updated.FullName;
                row.Cells[_colPhone.Index].Value = updated.Phone;
                row.Cells[_colNationalId.Index].Value = FormatNationalId(updated.NationalId, _currentUser?.IsInRole(UserRole.Operator) ?? false);
                row.Cells[_colExpiryDate.Index].Value = updated.CardExpiryDate?.ToString("dd/MM/yyyy") ?? "-";
            }
        }
    }

    private async Task OpenChangeStatusDialogAsync()
    {
        if (_selectedBeneficiary == null)
        {
            return;
        }

        var dialog = _formFactory?.Create<ChangeBeneficiaryStatusDialog>() ??
                     new ChangeBeneficiaryStatusDialog(_beneficiaryService, _notifyService, null);

        dialog.SetBeneficiary(_selectedBeneficiary);

        if (dialog.ShowDialog(this) == DialogResult.OK && dialog.UpdatedBeneficiary != null)
        {
            var updated = dialog.UpdatedBeneficiary;
            _selectedBeneficiary = updated;
            DisplayBeneficiaryDetails(updated);

            // Update row in grid
            if (_gridBeneficiaries.SelectedRows.Count > 0)
            {
                var row = _gridBeneficiaries.SelectedRows[0];
                row.Tag = updated;
                row.Cells[_colStatus.Index].Value = GetStatusArabic(updated.Status);

                if (updated.Status == BeneficiaryStatus.Active)
                {
                    row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorGranted;
                }
                else if (updated.Status == BeneficiaryStatus.Suspended)
                {
                    row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorWarning;
                }
                else if (updated.Status == BeneficiaryStatus.Blocked)
                {
                    row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorDenied;
                }
            }
        }
    }

    private void OnCheckInDirectClicked()
    {
        if (_selectedBeneficiary != null)
        {
            RequestCheckIn?.Invoke(this, _selectedBeneficiary.CardNumber);
        }
    }

    private async Task OpenIssueQrDialogAsync()
    {
        if (_selectedBeneficiary == null || _selectedBeneficiary.Status != BeneficiaryStatus.Active)
        {
            return;
        }

        var dialog = _formFactory?.Create<IssueQrDialog>() ?? new IssueQrDialog();
        using (dialog)
        {
            await dialog.LoadAndShowAsync(_selectedBeneficiary);
            dialog.ShowDialog(this);
        }
    }

    public async Task VoidSelectedHistoryEntryAsync()
    {
        if (!CanVoidSelectedHistory() || _checkInService == null)
        {
            return;
        }

        var attempt = (EntryAttemptDto)_gridHistory.SelectedRows[0].Tag!;

        string voidReason;
        int? supervisorUserId = null;

        var isSupervisorOrAdmin = _currentUser != null &&
            (_currentUser.IsInRole(UserRole.Supervisor) || _currentUser.IsInRole(UserRole.Admin));

        if (!isSupervisorOrAdmin)
        {
            // Prompt supervisor re-auth
            var authDialog = _formFactory?.Create<SupervisorAuthDialog>() ??
                             new SupervisorAuthDialog(null, _currentUser);

            authDialog.SetContext(defaultReason: "إلغاء عملية دخول مسجلة خطأً");

            if (authDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            voidReason = authDialog.Reason;
            supervisorUserId = authDialog.SupervisorUserId;
        }
        else
        {
            // Prompt reason dialog
            var reasonDialog = _formFactory?.Create<VoidReasonDialog>() ??
                               new VoidReasonDialog(_notifyService);

            var localTimeStr = DateTimeDisplayHelper.FormatDateTime(
                DateTimeDisplayHelper.ToParkLocalTime(attempt.AttemptedAtUtc, _timeZoneId));

            reasonDialog.SetContext(
                _selectedBeneficiary?.CardNumber ?? "-",
                _selectedBeneficiary?.FullName ?? "-",
                localTimeStr,
                attempt.TotalPersons);

            if (reasonDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            voidReason = reasonDialog.Reason;
        }

        // Confirm prompt
        var confirmMsg = Strings.Msg_ConfirmVoid;
        var confirmResult = MessageBox.Show(
            confirmMsg,
            Strings.Title_Confirm,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

        if (confirmResult != DialogResult.Yes)
        {
            return;
        }

        using (new BusyScope(this, _btnVoidHistoryEntry))
        {
            try
            {
                var request = new VoidEntryRequest(attempt.Id, voidReason);
                var result = await _checkInService.VoidEntryAsync(request, supervisorUserId);

                if (result.IsSuccess)
                {
                    _notifyService?.Info(Strings.Msg_VoidSuccess);

                    // Refresh history and reload beneficiary to show updated last granted entry
                    if (_selectedBeneficiary != null)
                    {
                        await LoadBeneficiaryHistoryAsync(_selectedBeneficiary.Id);

                        var freshBeneficiaryResult = await _beneficiaryService!.GetByIdAsync(_selectedBeneficiary.Id);
                        if (freshBeneficiaryResult.IsSuccess)
                        {
                            _selectedBeneficiary = freshBeneficiaryResult.Value.Beneficiary;
                            DisplayBeneficiaryDetails(_selectedBeneficiary);

                            // Update last entry column in beneficiaries grid
                            if (_gridBeneficiaries.SelectedRows.Count > 0)
                            {
                                var row = _gridBeneficiaries.SelectedRows[0];
                                row.Tag = _selectedBeneficiary;
                                row.Cells[_colLastEntry.Index].Value = _selectedBeneficiary.LastGrantedEntryUtc.HasValue
                                    ? DateTimeDisplayHelper.FormatDateTime(DateTimeDisplayHelper.ToParkLocalTime(_selectedBeneficiary.LastGrantedEntryUtc.Value, _timeZoneId))
                                    : "-";
                            }
                        }
                    }
                }
                else
                {
                    var msg = ErrorMessages.GetMessage(result.Error);
                    _notifyService?.Warning(msg);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error voiding history entry.");
                _notifyService?.Error(Strings.Msg_UnexpectedError);
            }
        }
    }

    private static string GetStatusArabic(BeneficiaryStatus status) => status switch
    {
        BeneficiaryStatus.Active => Strings.Status_Active,
        BeneficiaryStatus.Suspended => Strings.Status_Suspended,
        BeneficiaryStatus.Blocked => Strings.Status_Blocked,
        _ => status.ToString()
    };
}
