using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.GiftVouchers.Services;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.CheckIn.Services;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Services;
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
/// Check-in desk user control implementing SPEC §5 (Zones 1-7, flows 5.1 & A1-A8).
/// </summary>
public partial class CheckInDeskControl : UserControl
{
    private readonly IBeneficiaryService? _beneficiaryService;
    private readonly ICheckInService? _checkInService;
    private readonly IOrganizationService? _orgService;
    private readonly IAgeCategoryService? _ageCategoryService;
    private readonly ISettingsService? _settingsService;
    private readonly ICurrentUser? _currentUser;
    private readonly ICurrentWorkstation? _currentWorkstation;
    private readonly IFormFactory? _formFactory;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<CheckInDeskControl>? _logger;
    private readonly ParkGate.Application.EntryCards.CardScanService? _cardScanService;
    private readonly IGiftVoucherService? _giftVoucherService;

    private readonly List<CounterInput> _counterInputs = [];
    private readonly List<OrganizationDto> _organizations = [];
    private readonly List<AgeCategoryDto> _ageCategories = [];
    private readonly ContextMenuStrip _todayContextMenu = new();
    private readonly ToolStripMenuItem _menuVoidToday = new();
    private BeneficiaryDto? _activeHolder;
    private EligibilityPreviewDto? _activeEligibility;
    private bool _companionsBlockEntry;

    // R4.4: the card last reported as registered under other organizations, so a second Enter can load it there.
    private string? _registeredElsewhereCard;
    private IReadOnlyList<int> _registeredElsewhereOrgIds = [];
    private int _maxGroupSize = 10;
    private bool _prefillFromLastVisit;
    private bool _denialSoundEnabled;
    private string? _timeZoneId = "Libya Standard Time";
    private bool _isSearching;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int AutoResetDelayMs { get; set; } = 1200;

    public CheckInDeskControl() : this(null, null, null, null, null, null, null, null, null, null, null, null)
    {
    }

    public CheckInDeskControl(
        IBeneficiaryService? beneficiaryService,
        ICheckInService? checkInService,
        IOrganizationService? orgService,
        IAgeCategoryService? ageCategoryService,
        ISettingsService? settingsService,
        ICurrentUser? currentUser,
        ICurrentWorkstation? currentWorkstation,
        IFormFactory? formFactory,
        INotifyService? notifyService,
        ILogger<CheckInDeskControl>? logger,
        ParkGate.Application.EntryCards.CardScanService? cardScanService = null,
        IGiftVoucherService? giftVoucherService = null)
    {
        InitializeComponent();

        _beneficiaryService = beneficiaryService;
        _checkInService = checkInService;
        _orgService = orgService;
        _ageCategoryService = ageCategoryService;
        _settingsService = settingsService;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
        _formFactory = formFactory;
        _notifyService = notifyService;
        _logger = logger;
        _cardScanService = cardScanService;
        _giftVoucherService = giftVoucherService;

        _giftPanel.InitializeServices(
            _giftVoucherService,
            _currentUser,
            _formFactory,
            _notifyService,
            null,
            _timeZoneId);

        InitializeControlText();
        Theme.ApplyGridStyle(_gridToday);
        WireEvents();
    }

    private void InitializeControlText()
    {
        // Search Zone
        _lblCardNumber.Text = Strings.Label_CardNumber;
        _lblOrganization.Text = Strings.Label_OrganizationFilter;
        _btnSearch.Text = Strings.Btn_Search;

        // Holder Panel
        _grpHolder.Text = Strings.Title_HolderInfo;
        _lblHolderNameTitle.Text = Strings.Label_HolderName;
        _lblHolderPhoneTitle.Text = Strings.Label_HolderPhone;
        _lblHolderOrgTitle.Text = Strings.Label_HolderOrg;
        _lblHolderNatIdTitle.Text = Strings.Label_HolderNationalId;
        _lblHolderExpiryTitle.Text = Strings.Label_HolderExpiry;
        _lblHolderLastEntryTitle.Text = Strings.Label_HolderLastEntry;
        _lblHolderStatusTitle.Text = Strings.Label_HolderStatus;

        // Inline Registration
        _grpRegistration.Text = Strings.Title_InlineRegistration;
        _lblRegOrganization.Text = Strings.Label_RegOrganization;
        _lblRegFullName.Text = Strings.Label_RegFullName;
        _lblRegPhone.Text = Strings.Label_RegPhone;
        _lblRegNationalId.Text = Strings.Label_RegNationalId;
        _lblRegExpiry.Text = Strings.Label_RegExpiry;
        _lblRegNotes.Text = Strings.Label_RegNotes;
        _btnSaveReg.Text = Strings.Btn_SaveAndContinue;
        _btnCancelReg.Text = Strings.Btn_CancelReg;

        // Companions Panel
        _grpCompanions.Text = Strings.Title_Companions;
        _lblCompanionsNotice.Text = Strings.Notice_HolderCounted;
        _lblTotalPersons.Text = string.Format(Strings.Label_TotalPersons, 1);

        // Actions
        _btnRecordEntry.Text = Strings.Btn_RecordEntry;
        _btnOverride.Text = Strings.Btn_Override;
        _btnNewHolder.Text = Strings.Btn_NewHolder;
        _btnClear.Text = Strings.Btn_Clear;

        // Today Grid
        _grpToday.Text = Strings.Title_TodayDesk;
        _colTime.HeaderText = Strings.Col_Time;
        _colCard.HeaderText = Strings.Col_CardNumber;
        _colName.HeaderText = Strings.Col_HolderName;
        _colOrg.HeaderText = Strings.Col_Org;
        _colPersons.HeaderText = Strings.Col_Persons;
        _colResult.HeaderText = Strings.Col_Result;
        _lblTodaySummary.Text = string.Format(Strings.Summary_TodayStats, 0, 0, 0, 0);

        _statusBanner.Reset();
        _btnRecordEntry.Enabled = false;
        _btnOverride.Enabled = false;
    }

    private void WireEvents()
    {
        Load += async (_, _) => await OnDeskLoadedAsync();

        // Keyboard search on card box
        _txtCardNumber.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                await LookupCardAsync();
                if (!_grpRegistration.Visible)
                {
                    FocusCardNumberBox();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                ResetDesk();
            }
        };

        _txtCardNumber.Enter += (_, _) => _txtCardNumber.SelectAll();

        // Clear previous holder data if user changes the search card number
        _txtCardNumber.TextChanged += (_, _) =>
        {
            var currentCard = DigitNormalizer.NormalizeDigits(_txtCardNumber.Text).Trim();
            if (_activeHolder != null && !string.Equals(_activeHolder.CardNumber, currentCard, StringComparison.OrdinalIgnoreCase))
            {
                ClearPreviousHolderData();
                _statusBanner.Reset();
            }
        };

        _btnSearch.Click += async (_, _) =>
        {
            await LookupCardAsync();
            if (!_grpRegistration.Visible)
            {
                FocusCardNumberBox();
            }
        };
        _btnClear.Click += (_, _) => ResetDesk();
        _btnNewHolder.Click += (_, _) => StartManualRegistration();
        _btnCancelReg.Click += (_, _) => HideInlineRegistration();
        _btnSaveReg.Click += async (_, _) => await SaveInlineRegistrationAsync();
        _btnRecordEntry.Click += async (_, _) => await RecordEntryAsync();
        _btnOverride.Click += async (_, _) => await OverrideEntryAsync();

        // Today grid context menu for voiding (SPEC §7.2 FR-CHK-09)
        _menuVoidToday.Text = Strings.Menu_VoidTodayEntry;
        _menuVoidToday.Click += async (_, _) => await VoidSelectedTodayEntryAsync();
        _todayContextMenu.Items.Add(_menuVoidToday);
        _gridToday.ContextMenuStrip = _todayContextMenu;

        _gridToday.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit = _gridToday.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0)
                {
                    _gridToday.ClearSelection();
                    _gridToday.Rows[hit.RowIndex].Selected = true;
                }
            }
        };

        _todayContextMenu.Opening += (_, e) =>
        {
            if (_gridToday.SelectedRows.Count == 0 ||
                _gridToday.SelectedRows[0].Tag is not TodayDeskAttemptDto attempt ||
                attempt.IsVoided ||
                attempt.Result == EntryResult.Denied)
            {
                e.Cancel = true;
            }
        };
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.F5:
                if (_btnRecordEntry.Enabled)
                {
                    _ = RecordEntryAsync();
                    return true;
                }
                break;

            case Keys.F9:
                if (_btnOverride.Enabled)
                {
                    _ = OverrideEntryAsync();
                    return true;
                }
                break;

            case Keys.F2:
                StartManualRegistration();
                return true;

            case Keys.F6:
                if (_giftPanel.CanIssue)
                {
                    _ = _giftPanel.IssueAsync();
                    return true;
                }
                break;

            case Keys.F7:
                if (_giftPanel.CanIssueOverride)
                {
                    _ = _giftPanel.IssueOverrideAsync();
                    return true;
                }
                break;

            case Keys.Escape:
                if (_cmbOrganization.DroppedDown || _cmbRegOrganization.DroppedDown)
                {
                    // Esc closes the open list; it must not also reset the desk (R4.5).
                    break;
                }

                if (!ConfirmDiscardRegistration())
                {
                    return true;
                }

                ResetDesk();
                return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    /// <summary>
    /// True when nothing would be lost, or the operator agreed to discard a half-filled registration (R4.5).
    /// Without a notify service nothing is discarded.
    /// </summary>
    private bool ConfirmDiscardRegistration()
    {
        var hasInput = _grpRegistration.Visible &&
            (!string.IsNullOrWhiteSpace(_txtRegFullName.Text) ||
             !string.IsNullOrWhiteSpace(_txtRegPhone.Text) ||
             !string.IsNullOrWhiteSpace(_txtRegNationalId.Text) ||
             !string.IsNullOrWhiteSpace(_txtRegNotes.Text) ||
             _dtpRegExpiry.Checked);

        return !hasInput || (_notifyService?.Confirm(Strings.Confirm_DiscardRegistration, Strings.Title_Warning) ?? false);
    }

    public async Task OnDeskLoadedAsync()
    {
        await LoadSettingsAsync();
        await LoadOrganizationsAsync();
        await LoadAgeCategoriesAsync();
        await RefreshTodayDeskSummaryAsync();

        FocusCardNumberBox();
    }

    public void FocusCardNumberBox()
    {
        _txtCardNumber.Focus();
        _txtCardNumber.SelectAll();
    }

    private async Task LoadSettingsAsync()
    {
        if (_settingsService == null)
        {
            return;
        }

        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            _maxGroupSize = settings.MaxGroupSize;
            _prefillFromLastVisit = settings.PrefillCompanionsFromLastVisit;
            _denialSoundEnabled = settings.DenialSoundEnabled;
            _timeZoneId = settings.TimeZoneId;
            _giftPanel.SetTimeZone(_timeZoneId);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load check-in settings.");
        }
    }

    private async Task LoadOrganizationsAsync()
    {
        if (_orgService == null)
        {
            return;
        }

        try
        {
            var orgs = await _orgService.GetAllAsync(activeOnly: true);
            _organizations.Clear();
            _organizations.AddRange(orgs);

            _cmbOrganization.Items.Clear();
            _cmbOrganization.Items.Add(new OrgComboItem(null, Strings.Filter_AllOrganizations));

            // The registration panel offers real organizations only, never «الكل» (R4.3).
            _cmbRegOrganization.Items.Clear();
            foreach (var org in orgs)
            {
                _cmbOrganization.Items.Add(new OrgComboItem(org.Id, org.NameAr));
                _cmbRegOrganization.Items.Add(new OrgComboItem(org.Id, org.NameAr));
            }

            _cmbOrganization.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load organizations for filter.");
        }
    }

    public async Task LoadAgeCategoriesAsync()
    {
        if (_ageCategoryService == null)
        {
            CreateDefaultAgeCategories();
            return;
        }

        try
        {
            var categories = await _ageCategoryService.GetActiveAsync();
            if (categories.Count == 0)
            {
                CreateDefaultAgeCategories();
            }
            else
            {
                PopulateAgeCategories(categories);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load age categories. Using defaults.");
            CreateDefaultAgeCategories();
        }
    }

    private void CreateDefaultAgeCategories()
    {
        PopulateAgeCategories(
        [
            new AgeCategoryDto(1, "بالغ", "12 سنة فما فوق", 1, true),
            new AgeCategoryDto(2, "طفل", "من 3 إلى 12 سنة", 2, true),
            new AgeCategoryDto(3, "رضيع", "أقل من 3 سنوات", 3, true, IsCompanionLimitExempt: true)
        ]);
    }

    public void PopulateAgeCategories(IReadOnlyList<AgeCategoryDto> categories)
    {
        _ageCategories.Clear();
        _ageCategories.AddRange(categories);

        _flowCounters.SuspendLayout();
        _flowCounters.Controls.Clear();
        _counterInputs.Clear();

        foreach (var cat in categories.OrderBy(c => c.SortOrder))
        {
            var counter = new CounterInput
            {
                Title = $"{cat.NameAr}:",
                Tag = cat.Id,
                Width = Math.Max(260, _flowCounters.ClientSize.Width - 12),
                Height = 56,
                Margin = new Padding(2, 4, 2, 4),
                MinValue = 0,
                MaxValue = _maxGroupSize - 1,
                Value = 0
            };

            counter.ValueChanged += (_, _) => UpdateLiveTotalPersons();
            _counterInputs.Add(counter);
            _flowCounters.Controls.Add(counter);
        }

        _flowCounters.ResumeLayout(true);
        UpdateLiveTotalPersons();
    }

    private void UpdateLiveTotalPersons()
    {
        var companionsCount = _counterInputs.Sum(c => c.Value);
        var totalPersons = 1 + companionsCount; // BR-08: Holder counts as one person

        _lblTotalPersons.Text = string.Format(Strings.Label_TotalPersons, totalPersons);

        var limitExceeded = false;
        var maxFreeLimit = 0;
        if (_activeHolder != null)
        {
            var org = _organizations.FirstOrDefault(o => o.Id == _activeHolder.OrganizationId);
            if (org?.MaxFreeCompanions.HasValue == true)
            {
                maxFreeLimit = org.MaxFreeCompanions.Value;
                var nonExemptCompanions = 0;
                foreach (var counter in _counterInputs)
                {
                    if (counter.Tag is int catId)
                    {
                        var cat = _ageCategories.FirstOrDefault(c => c.Id == catId);
                        if (cat == null || !cat.IsCompanionLimitExempt)
                        {
                            nonExemptCompanions += counter.Value;
                        }
                    }
                    else
                    {
                        nonExemptCompanions += counter.Value;
                    }
                }

                _lblFreeCompanions.Visible = true;
                if (nonExemptCompanions > maxFreeLimit)
                {
                    limitExceeded = true;
                    _lblFreeCompanions.Text = string.Format(Strings.Label_FreeCompanionsExceeded, nonExemptCompanions, maxFreeLimit);
                    _lblFreeCompanions.ForeColor = StatusBanner.ColorDenied;
                    _pnlCompanionsTotal.BackColor = Color.FromArgb(254, 242, 242);
                }
                else
                {
                    _lblFreeCompanions.Text = string.Format(Strings.Label_FreeCompanionsCount, nonExemptCompanions, maxFreeLimit);
                    _lblFreeCompanions.ForeColor = StatusBanner.ColorNeutral;
                    _pnlCompanionsTotal.BackColor = Color.FromArgb(240, 253, 244);
                }
            }
            else
            {
                _lblFreeCompanions.Visible = false;
                _pnlCompanionsTotal.BackColor = Color.FromArgb(240, 253, 244);
            }
        }
        else
        {
            _lblFreeCompanions.Visible = false;
            _pnlCompanionsTotal.BackColor = Color.FromArgb(240, 253, 244);
        }

        _companionsBlockEntry = totalPersons > _maxGroupSize || limitExceeded;
        UpdateActionButtons();

        if (totalPersons > _maxGroupSize)
        {
            _lblTotalPersons.ForeColor = StatusBanner.ColorDenied;
        }
        else if (limitExceeded)
        {
            _lblTotalPersons.ForeColor = StatusBanner.ColorDenied;
            _statusBanner.SetWarning(
                string.Format(Strings.Error_FreeCompanionsExceededBanner, maxFreeLimit),
                Strings.Error_CheckIn_ExceedsFreeCompanionLimit);
        }
        else
        {
            _lblTotalPersons.ForeColor = StatusBanner.ColorGranted;
        }
    }

    /// <summary>
    /// The one place that decides whether «تسجيل الدخول» and «تجاوز المشرف» are available: a holder is shown, the
    /// companions fit the limits, and the eligibility allows it. Called after every operation (R4.1).
    /// </summary>
    private void UpdateActionButtons()
    {
        var holderShown = _activeHolder != null && _activeEligibility != null && !_companionsBlockEntry;
        _btnRecordEntry.Enabled = holderShown && _activeEligibility!.IsEligible;
        _btnOverride.Enabled = holderShown && !_activeEligibility!.IsEligible && _activeEligibility.CanBeOverridden;
    }

    public void ResetDesk()
    {
        ClearPreviousHolderData();
        _txtCardNumber.Clear();
        _statusBanner.Reset();
        FocusCardNumberBox();
    }

    public void ClearPreviousHolderData()
    {
        _activeHolder = null;
        _activeEligibility = null;

        ClearHolderInfo();
        HideInlineRegistration();
        _giftPanel.Clear();

        _txtRegFullName.Clear();
        _txtRegPhone.Clear();
        _txtRegNationalId.Clear();
        _dtpRegExpiry.Checked = false;
        _txtRegNotes.Clear();

        foreach (var counter in _counterInputs)
        {
            counter.Reset();
        }

        _btnRecordEntry.Enabled = false;
        _btnOverride.Enabled = false;

        UpdateLiveTotalPersons();
    }

    private void ClearHolderInfo()
    {
        _lblHolderName.Text = "-";
        _lblHolderPhone.Text = "-";
        _lblHolderOrg.Text = "-";
        _lblHolderNatId.Text = "-";
        _lblHolderExpiry.Text = "-";
        _lblHolderLastEntry.Text = "-";
        _lblHolderStatus.Text = "-";
        _lblHolderStatus.ForeColor = Color.FromArgb(45, 55, 72);
    }

    public void ShowHolderInfo(BeneficiaryDto holder, EligibilityPreviewDto? eligibility)
    {
        _activeHolder = holder;
        _activeEligibility = eligibility;

        HideInlineRegistration();

        _lblHolderName.Text = holder.FullName;
        _lblHolderPhone.Text = holder.Phone;
        _lblHolderOrg.Text = holder.OrganizationName;

        // Mask National ID for Operator role per SPEC §5.3 zone 4 and BR-16
        if (!string.IsNullOrWhiteSpace(holder.NationalId))
        {
            if (_currentUser != null && _currentUser.Role == UserRole.Operator && holder.NationalId.Length > 4)
            {
                _lblHolderNatId.Text = new string('*', holder.NationalId.Length - 4) + holder.NationalId[^4..];
            }
            else
            {
                _lblHolderNatId.Text = holder.NationalId;
            }
        }
        else
        {
            _lblHolderNatId.Text = Strings.Value_None;
        }

        _lblHolderExpiry.Text = holder.CardExpiryDate.HasValue
            ? DateTimeDisplayHelper.FormatDate(holder.CardExpiryDate.Value.ToDateTime(TimeOnly.MinValue))
            : Strings.Value_None;

        _lblHolderLastEntry.Text = holder.LastGrantedEntryUtc.HasValue
            ? DateTimeDisplayHelper.FormatDateTime(DateTimeDisplayHelper.ToParkLocalTime(holder.LastGrantedEntryUtc.Value, _timeZoneId))
            : Strings.Value_None;

        _lblHolderStatus.Text = holder.Status switch
        {
            BeneficiaryStatus.Active => Strings.Status_Active,
            BeneficiaryStatus.Suspended => Strings.Status_Suspended,
            BeneficiaryStatus.Blocked => Strings.Status_Blocked,
            _ => holder.Status.ToString()
        };

        _lblHolderStatus.ForeColor = holder.Status == BeneficiaryStatus.Active
            ? StatusBanner.ColorGranted
            : StatusBanner.ColorDenied;

        // Update StatusBanner based on Eligibility decision
        if (eligibility != null)
        {
            if (eligibility.IsEligible)
            {
                _statusBanner.SetGranted(
                    message: Strings.Banner_Granted,
                    details: holder.LastGrantedEntryUtc.HasValue
                        ? $"آخر دخول: {DateTimeDisplayHelper.FormatDateTime(DateTimeDisplayHelper.ToParkLocalTime(holder.LastGrantedEntryUtc.Value, _timeZoneId))}"
                        : null);

                _btnRecordEntry.Enabled = true;
                _btnOverride.Enabled = false;
            }
            else
            {
                var reasonText = eligibility.DenialReason.HasValue
                    ? GetDenialReasonArabic(eligibility.DenialReason.Value)
                    : (!string.IsNullOrWhiteSpace(eligibility.DenialReasonText)
                        ? eligibility.DenialReasonText
                        : Strings.Banner_Denied);

                string? details = null;

                if (eligibility.NextAllowedEntryUtc.HasValue)
                {
                    var nextLocal = DateTimeDisplayHelper.ToParkLocalTime(eligibility.NextAllowedEntryUtc.Value, _timeZoneId);
                    details = $"يسمح بالدخول بعد: {DateTimeDisplayHelper.FormatDateTime(nextLocal)} (فترة الانتظار: {eligibility.EffectiveCooldownMinutes / 60} ساعة)";
                }

                _statusBanner.SetDenied(reasonText, details);
                _btnRecordEntry.Enabled = false;
                _btnOverride.Enabled = eligibility.CanBeOverridden;

                if (_denialSoundEnabled)
                {
                    System.Media.SystemSounds.Exclamation.Play();
                }
            }
        }
        else
        {
            _statusBanner.Reset();
            _btnRecordEntry.Enabled = false;
            _btnOverride.Enabled = false;
        }

        UpdateLiveTotalPersons();
        _ = _giftPanel.LoadAsync(holder.Id);
    }

    public void ShowInlineRegistration(string cardNumber, int? orgId = null)
    {
        _grpHolder.Visible = false;
        _grpRegistration.Visible = true;

        _txtRegFullName.Clear();
        _txtRegPhone.Clear();
        _txtRegNationalId.Clear();
        _dtpRegExpiry.Checked = false;
        _txtRegNotes.Clear();
        SelectRegistrationOrganization(orgId ?? (_cmbOrganization.SelectedItem as OrgComboItem)?.Id);

        _statusBanner.SetWarning("البطاقة غير مسجلة — يرجى تسجيل بيانات المستفيد", "أدخل الاسم الرباعي ورقم الهاتف للمتابعة");
        _btnRecordEntry.Enabled = false;
        _btnOverride.Enabled = false;

        _txtRegFullName.Focus();
    }

    /// <summary>
    /// Preselects the organization for a new holder: the one asked for (the filter), else the default organization.
    /// Leaves it empty when neither exists, so the operator must choose (R4.3).
    /// </summary>
    private void SelectRegistrationOrganization(int? preferredOrgId)
    {
        var targetId = preferredOrgId ?? _organizations.FirstOrDefault(o => o.IsDefault)?.Id;
        var match = _cmbRegOrganization.Items.OfType<OrgComboItem>().FirstOrDefault(i => i.Id == targetId);
        _cmbRegOrganization.SelectedItem = match;
    }

    private void SelectFilterOrganization(int? orgId)
    {
        var item = _cmbOrganization.Items.OfType<OrgComboItem>().FirstOrDefault(i => i.Id == orgId);
        if (item != null)
        {
            _cmbOrganization.SelectedItem = item;
        }
    }

    public void HideInlineRegistration()
    {
        _grpRegistration.Visible = false;
        _grpHolder.Visible = true;
    }

    private void StartManualRegistration()
    {
        var card = DigitNormalizer.NormalizeDigits(_txtCardNumber.Text.Trim());
        if (string.IsNullOrWhiteSpace(card))
        {
            _notifyService?.Warning("يرجى إدخال رقم البطاقة أولاً لتسجيل زبون جديد.", "تنبيه");
            FocusCardNumberBox();
            return;
        }

        ShowInlineRegistration(card);
    }

    public async Task LookupCardAsync()
    {
        if (_isSearching)
        {
            return;
        }

        _isSearching = true;
        try
        {
            var rawCard = _txtCardNumber.Text;
            var normalized = DigitNormalizer.NormalizeDigits(rawCard).Trim();
            _txtCardNumber.Text = normalized;

            if (string.IsNullOrWhiteSpace(normalized))
            {
                ClearPreviousHolderData();
                _statusBanner.SetWarning("يرجى إدخال أو مسح رقم البطاقة", null);
                FocusCardNumberBox();
                return;
            }

            // Always clear previous customer data and reset banner when initiating a new search
            ClearPreviousHolderData();
            _statusBanner.Reset();

        // 1. Check if scanned input is a signed QR token (203 digits per QrTokenFormat.DigitLength)
        if (normalized.Length == ParkGate.Domain.EntryCards.QrTokenFormat.DigitLength && _cardScanService != null)
        {
            using (new BusyScope(this, _btnSearch, _btnNewHolder))
            {
                try
                {
                    var scanResult = await _cardScanService.ScanAsync(normalized);
                    if (!scanResult.IsAccepted)
                    {
                        _statusBanner.SetDenied(scanResult.MessageAr, "رمز QR غير صالح أو ملغى");
                        if (_denialSoundEnabled)
                        {
                            System.Media.SystemSounds.Exclamation.Play();
                        }
                        FocusCardNumberBox();
                        return;
                    }

                    if (scanResult.BeneficiaryId.HasValue && _beneficiaryService != null)
                    {
                        var lookupById = await _beneficiaryService.GetByIdAsync(scanResult.BeneficiaryId.Value);
                        if (lookupById.IsSuccess)
                        {
                            var matched = lookupById.Value;
                            _txtCardNumber.Text = matched.Beneficiary.CardNumber;
                            ShowHolderInfo(matched.Beneficiary, matched.Eligibility);

                            if (_prefillFromLastVisit && matched.LastCompanions?.Count > 0)
                            {
                                PrefillCompanions(matched.LastCompanions);
                            }
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error scanning QR card token.");
                    _statusBanner.SetDenied("حدث خطأ أثناء فحص رمز QR: " + ex.Message);
                    FocusCardNumberBox();
                    return;
                }
            }
        }

        if (_beneficiaryService == null)
        {
            return;
        }

        using (new BusyScope(this, _btnSearch, _btnNewHolder))
        {
            try
            {
                if (_registeredElsewhereCard == normalized && _registeredElsewhereOrgIds.Count > 0)
                {
                    // Second Enter on the same card: follow it to its organization (several: «الكل», then choose).
                    SelectFilterOrganization(_registeredElsewhereOrgIds.Count == 1 ? _registeredElsewhereOrgIds[0] : null);
                }

                _registeredElsewhereCard = null;
                _registeredElsewhereOrgIds = [];

                var selectedOrgId = (_cmbOrganization.SelectedItem as OrgComboItem)?.Id;
                var lookupResult = await _beneficiaryService.LookupByCardNumberAsync(normalized, selectedOrgId);

                if (lookupResult.IsFailure)
                {
                    _statusBanner.SetDenied(ErrorMessages.GetMessage(lookupResult.Error));
                    FocusCardNumberBox();
                    return;
                }

                // The organization filter is applied by the service (R3.8). RegisteredElsewhere is shown by R4.4.
                var matches = lookupResult.Value.Matches;

                if (matches.Count == 0 && lookupResult.Value.RegisteredElsewhere.Count > 0)
                {
                    // R4.4: not "unregistered" — it belongs to another organization. Registration stays on F2.
                    var elsewhere = lookupResult.Value.RegisteredElsewhere;
                    _registeredElsewhereCard = normalized;
                    _registeredElsewhereOrgIds = elsewhere.Select(o => o.Id).ToList();
                    _statusBanner.SetWarning(
                        string.Format(Strings.Banner_CardRegisteredElsewhere, string.Join(Strings.Text_ListSeparator, elsewhere.Select(o => o.NameAr))),
                        Strings.Banner_CardRegisteredElsewhereHint);
                    FocusCardNumberBox();
                    return;
                }

                if (matches.Count == 0)
                {
                    // Flow A1: Card not found -> Inline registration
                    ShowInlineRegistration(normalized, selectedOrgId);
                    return;
                }

                BeneficiaryLookupResultDto matched;
                if (matches.Count == 1)
                {
                    matched = matches[0];
                }
                else
                {
                    // Flow A2: Card found in multiple organizations. Cancelling picks nobody (R4.6).
                    var chosen = DisambiguateOrganization(matches);
                    if (chosen is null)
                    {
                        _statusBanner.Reset(Strings.Banner_OrganizationNotChosen);
                        FocusCardNumberBox();
                        return;
                    }

                    matched = chosen;
                }

                ShowHolderInfo(matched.Beneficiary, matched.Eligibility);

                if (_prefillFromLastVisit && matched.LastCompanions?.Count > 0)
                {
                    PrefillCompanions(matched.LastCompanions);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred during card lookup.");
                _statusBanner.SetDenied("تعذّر الاتصال بقاعدة البيانات", "يرجى التحقق من الاتصال بالخادم والمحاولة مرة أخرى.");
                FocusCardNumberBox();
            }
        }

        if (!_grpRegistration.Visible)
        {
            FocusCardNumberBox();
        }
    }
    finally
    {
        _isSearching = false;
    }
}

    /// <summary>Test seam: replaces the chooser dialog. Returns null for "cancelled".</summary>
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    internal Func<IReadOnlyList<BeneficiaryLookupResultDto>, BeneficiaryLookupResultDto?>? ChooseOrganizationOverride { get; set; }

    private BeneficiaryLookupResultDto? DisambiguateOrganization(IReadOnlyList<BeneficiaryLookupResultDto> matches)
    {
        if (ChooseOrganizationOverride != null)
        {
            return ChooseOrganizationOverride(matches);
        }

        using var dialog = _formFactory?.Create<SelectOrganizationDialog>() ?? new SelectOrganizationDialog();
        dialog.SetMatches(matches);
        return dialog.ShowDialog(this) == DialogResult.OK ? dialog.SelectedMatch : null;
    }

    private void PrefillCompanions(IReadOnlyList<CompanionCountDto> companions)
    {
        foreach (var comp in companions)
        {
            var counter = _counterInputs.FirstOrDefault(c => c.Tag is int id && id == comp.AgeCategoryId);
            if (counter != null)
            {
                counter.Value = comp.Count;
            }
        }
    }

    internal async Task SaveInlineRegistrationAsync()
    {
        var fullName = _txtRegFullName.Text.Trim();
        var phone = _txtRegPhone.Text.Trim();
        var card = DigitNormalizer.NormalizeDigits(_txtCardNumber.Text.Trim());
        var registrationOrgId = (_cmbRegOrganization.SelectedItem as OrgComboItem)?.Id;

        if (registrationOrgId is null)
        {
            _notifyService?.Warning(Strings.Val_RegOrganizationRequired, Strings.Title_Warning);
            _cmbRegOrganization.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            _notifyService?.Warning("الاسم الرباعي مطلوب.", "تنبيه");
            _txtRegFullName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            _notifyService?.Warning("رقم الهاتف مطلوب.", "تنبيه");
            _txtRegPhone.Focus();
            return;
        }

        if (_beneficiaryService == null)
        {
            return;
        }

        using (new BusyScope(this, _btnSaveReg, _btnCancelReg))
        {
            try
            {
                var orgId = registrationOrgId.Value;

                DateOnly? expiry = _dtpRegExpiry.Checked ? DateOnly.FromDateTime(_dtpRegExpiry.Value) : null;
                var natId = string.IsNullOrWhiteSpace(_txtRegNationalId.Text) ? null : _txtRegNationalId.Text.Trim();
                var notes = string.IsNullOrWhiteSpace(_txtRegNotes.Text) ? null : _txtRegNotes.Text.Trim();

                var regRequest = new RegisterBeneficiaryRequest(
                    OrganizationId: orgId,
                    CardNumber: card,
                    FullName: fullName,
                    Phone: phone,
                    NationalId: natId,
                    CardExpiryDate: expiry,
                    Notes: notes);

                var regResult = await _beneficiaryService.RegisterAsync(regRequest);
                if (regResult.IsSuccess)
                {
                    _notifyService?.Info("تم تسجيل الزبون بنجاح.", "تم التسجيل");
                    await LookupCardAsync();
                }
                else
                {
                    _notifyService?.Error(ErrorMessages.GetMessage(regResult.Error), "فشل التسجيل");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to register beneficiary.");
                _notifyService?.Error(ex.Message, "خطأ في التسجيل");
            }
        }
    }

    public async Task RecordEntryAsync()
    {
        if (_activeHolder == null || _checkInService == null)
        {
            return;
        }

        var companions = _counterInputs
            .Where(c => c.Value > 0)
            .Select(c => new CompanionRequestDto((int)c.Tag!, c.Value))
            .ToList();

        using (new BusyScope(this, _btnRecordEntry, _btnOverride, _btnClear))
        {
            try
            {
                var request = new RecordEntryRequest(
                    BeneficiaryId: _activeHolder.Id,
                    Companions: companions,
                    Channel: EntryChannel.Desk);

                var result = await _checkInService.RecordEntryAsync(request);
                if (result.IsSuccess)
                {
                    _statusBanner.SetGranted("تم تسجيل الدخول بنجاح!", $"المرافقون: {companions.Sum(c => c.Count)} · الإجمالي: {result.Value.TotalPersons} فرد");
                    await RefreshTodayDeskSummaryAsync();

                    if (AutoResetDelayMs > 0)
                    {
                        await Task.Delay(AutoResetDelayMs);
                    }

                    if (AutoResetDelayMs >= 0)
                    {
                        ResetDesk();
                    }
                }
                else
                {
                    _statusBanner.SetDenied(ErrorMessages.GetMessage(result.Error));
                    if (_denialSoundEnabled)
                    {
                        System.Media.SystemSounds.Hand.Play();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording entry.");
                _statusBanner.SetDenied("فشل تسجيل الدخول", "حدث خطأ غير متوقع أثناء معالجة العملية.");
            }
        }

        UpdateActionButtons();
    }

    public async Task OverrideEntryAsync()
    {
        if (_activeHolder == null || _checkInService == null)
        {
            return;
        }

        var supervisorDialog = _formFactory?.Create<SupervisorAuthDialog>() ??
                               new SupervisorAuthDialog(null, _currentUser);

        supervisorDialog.SetContext(defaultReason: "تجاوز فترة الانتظار استثنائياً");

        if (supervisorDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var companions = _counterInputs
            .Where(c => c.Value > 0)
            .Select(c => new CompanionRequestDto((int)c.Tag!, c.Value))
            .ToList();

        using (new BusyScope(this, _btnRecordEntry, _btnOverride, _btnClear))
        {
            try
            {
                var request = new OverrideEntryRequest(
                    BeneficiaryId: _activeHolder.Id,
                    Reason: supervisorDialog.Reason,
                    SupervisorApprovalToken: supervisorDialog.ApprovalToken,
                    Companions: companions,
                    Channel: EntryChannel.Desk);

                var result = await _checkInService.OverrideEntryAsync(request);
                if (result.IsSuccess)
                {
                    _statusBanner.SetGranted("تم تسجيل الدخول بموافقة المشرف (تجاوز)", $"المشرف: {supervisorDialog.SupervisorFullName} · السبب: {supervisorDialog.Reason}");
                    await RefreshTodayDeskSummaryAsync();

                    if (AutoResetDelayMs > 0)
                    {
                        await Task.Delay(AutoResetDelayMs);
                    }

                    if (AutoResetDelayMs >= 0)
                    {
                        ResetDesk();
                    }
                }
                else
                {
                    _statusBanner.SetDenied(ErrorMessages.GetMessage(result.Error));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing override.");
                _statusBanner.SetDenied("فشل التجاوز", "حدث خطأ غير متوقع أثناء معالجة التجاوز.");
            }
        }

        UpdateActionButtons();
    }

    private static string GetDenialReasonArabic(DenialReason reason) => reason switch
    {
        DenialReason.CooldownActive => "فترة التهدئة نشطة (سبق الدخول)",
        DenialReason.CardExpired => "صلاحية البطاقة منتهية",
        DenialReason.HolderSuspended => "حساب الزبون موقوف مؤقتاً",
        DenialReason.HolderBlocked => "حساب الزبون محظور",
        DenialReason.OrganizationInactive => "الجهة الراعية غير نشطة",
        _ => Strings.Banner_Denied
    };

    public async Task RefreshTodayDeskSummaryAsync()
    {
        if (_checkInService == null)
        {
            return;
        }

        try
        {
            var summaryResult = await _checkInService.GetTodayDeskEntriesAsync(_currentWorkstation?.WorkstationId);
            if (summaryResult.IsSuccess)
            {
                var summary = summaryResult.Value;

                var baseStats = string.Format(
                    Strings.Summary_TodayStats,
                    summary.TotalGranted,
                    summary.TotalOverridden,
                    summary.TotalDenied,
                    summary.TotalPersons);

                if (summary.PersonsPerCategory.Count > 0)
                {
                    var categoryBreakdown = string.Join(" · ", summary.PersonsPerCategory.Select(c => $"{c.CategoryName}: {c.Count}"));
                    _lblTodaySummary.Text = $"{baseStats}   |   {categoryBreakdown}";
                }
                else
                {
                    _lblTodaySummary.Text = baseStats;
                }

                if (IsHandleCreated || _gridToday.IsHandleCreated)
                {
                    _gridToday.SuspendLayout();
                    _gridToday.Rows.Clear();

                foreach (var item in summary.RecentAttempts)
                {
                    var localTime = DateTimeDisplayHelper.ToParkLocalTime(item.AttemptedAtUtc, _timeZoneId);
                    var timeStr = DateTimeDisplayHelper.FormatTime(localTime);

                    string resultText;
                    if (item.IsVoided)
                    {
                        resultText = Strings.Result_Voided;
                    }
                    else
                    {
                        resultText = item.Result switch
                        {
                            EntryResult.Allowed => Strings.Result_Allowed,
                            EntryResult.Denied => Strings.Result_Denied,
                            EntryResult.Overridden => Strings.Result_Overridden,
                            _ => item.Result.ToString()
                        };
                    }

                    var rowIndex = _gridToday.Rows.Add(
                        timeStr,
                        item.CardNumber,
                        item.BeneficiaryFullName,
                        item.OrganizationName,
                        item.TotalPersons,
                        resultText);

                    var row = _gridToday.Rows[rowIndex];
                    row.Tag = item;
                    if (item.IsVoided)
                    {
                        row.DefaultCellStyle.ForeColor = Color.Gray;
                    }
                    else if (item.Result == EntryResult.Allowed)
                    {
                        row.DefaultCellStyle.ForeColor = StatusBanner.ColorGranted;
                    }
                    else if (item.Result == EntryResult.Denied)
                    {
                        row.DefaultCellStyle.ForeColor = StatusBanner.ColorDenied;
                    }
                    else if (item.Result == EntryResult.Overridden)
                    {
                        row.DefaultCellStyle.ForeColor = StatusBanner.ColorWarning;
                    }
                }

                _gridToday.ResumeLayout(true);
            }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to refresh today's desk summary.");
        }
    }

    public void SetCardNumber(string cardNumber)
    {
        _txtCardNumber.Text = cardNumber;
    }

    public async Task SearchCardAsync()
    {
        await LookupCardAsync();
    }

    public async Task VoidSelectedTodayEntryAsync()
    {
        if (_gridToday.SelectedRows.Count == 0 || _checkInService == null)
        {
            return;
        }

        if (_gridToday.SelectedRows[0].Tag is not TodayDeskAttemptDto attempt ||
            attempt.IsVoided ||
            attempt.Result == EntryResult.Denied)
        {
            return;
        }

        string voidReason;
        string? approvalToken = null;

        var isSupervisorOrAdmin = _currentUser != null &&
            (_currentUser.IsInRole(UserRole.Supervisor) || _currentUser.IsInRole(UserRole.Admin));

        if (!isSupervisorOrAdmin)
        {
            var authDialog = _formFactory?.Create<SupervisorAuthDialog>() ??
                             new SupervisorAuthDialog(null, _currentUser);

            authDialog.SetContext(defaultReason: "إلغاء عملية دخول مسجلة خطأً");

            if (authDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            voidReason = authDialog.Reason;
            approvalToken = authDialog.ApprovalToken;
        }
        else
        {
            var reasonDialog = _formFactory?.Create<VoidReasonDialog>() ??
                               new VoidReasonDialog(_notifyService);

            var localTimeStr = DateTimeDisplayHelper.FormatDateTime(
                DateTimeDisplayHelper.ToParkLocalTime(attempt.AttemptedAtUtc, _timeZoneId));

            reasonDialog.SetContext(
                attempt.CardNumber,
                attempt.BeneficiaryFullName,
                localTimeStr,
                attempt.TotalPersons);

            if (reasonDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            voidReason = reasonDialog.Reason;
        }

        var confirmResult = MessageBox.Show(
            Strings.Msg_ConfirmVoid,
            Strings.Title_Confirm,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);

        if (confirmResult != DialogResult.Yes)
        {
            return;
        }

        using (new BusyScope(this))
        {
            try
            {
                var request = new VoidEntryRequest(attempt.Id, voidReason);
                var result = await _checkInService.VoidEntryAsync(request, approvalToken);

                if (result.IsSuccess)
                {
                    if (_notifyService != null)
                    {
                        _notifyService.Info(Strings.Msg_VoidSuccess);
                    }
                    else
                    {
                        MessageBox.Show(
                            Strings.Msg_VoidSuccess,
                            Strings.Title_Info,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
                    }

                    await RefreshTodayDeskSummaryAsync();

                    if (_activeHolder != null && string.Equals(_activeHolder.CardNumber, attempt.CardNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        await LookupCardAsync();
                    }
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
                _logger?.LogError(ex, "Error voiding today entry.");
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

    private sealed record OrgComboItem(int? Id, string Name)
    {
        public override string ToString() => Name;
    }
}
