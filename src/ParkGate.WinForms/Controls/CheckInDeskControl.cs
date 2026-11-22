using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.Cities.Services;
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
    private readonly IBeneficiaryService _beneficiaryService;
    private readonly ICheckInService _checkInService;
    private readonly IOrganizationService _orgService;
    private readonly IAgeCategoryService _ageCategoryService;
    private readonly ICityService _cityService;
    private readonly ISettingsService _settingsService;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;
    private readonly IFormFactory _formFactory;
    private readonly INotifyService _notifyService;
    private readonly ILogger<CheckInDeskControl> _logger;
    private readonly ParkGate.Application.EntryCards.ICardScanService _cardScanService;
    private readonly IGiftVoucherService _giftVoucherService;

    private readonly List<CounterInput> _counterInputs = [];
    private readonly List<OrganizationDto> _organizations = [];
    private readonly List<AgeCategoryDto> _ageCategories = [];
    private BeneficiaryDto? _activeHolder;
    private EligibilityPreviewDto? _activeEligibility;
    private bool _companionsBlockEntry;
    private bool _ageCategoriesUnavailable;

    // R4.4: the card last reported as registered under other organizations, so a second Enter can load it there.
    private string? _registeredElsewhereCard;
    private IReadOnlyList<int> _registeredElsewhereOrgIds = [];
    private int _maxGroupSize = 10;
    private bool _prefillFromLastVisit;
    private bool _companionsPrefilled; // R5.5: counts came from the last visit and nobody has touched them yet
    private bool _isPrefilling;
    private bool _denialSoundEnabled;
    private bool _successSoundEnabled;
    private readonly IDeskSounds _deskSounds;
    private string? _timeZoneId = "Libya Standard Time";
    private bool _isSearching;

    // R4.2: a lookup asked for while one runs is queued; the counter tells the success delay a lookup happened.
    private bool _lookupQueued;
    private int _lookupCount;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int AutoResetDelayMs { get; set; } = 1200;

    public CheckInDeskControl(
        IBeneficiaryService beneficiaryService,
        ICheckInService checkInService,
        IOrganizationService orgService,
        IAgeCategoryService ageCategoryService,
        ICityService cityService,
        ISettingsService settingsService,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        IFormFactory formFactory,
        INotifyService notifyService,
        ILogger<CheckInDeskControl> logger,
        ParkGate.Application.EntryCards.ICardScanService cardScanService,
        IGiftVoucherService giftVoucherService,
        ILogger<GiftOfferPanel> giftPanelLogger,
        IDeskSounds deskSounds)
    {
        InitializeComponent();
        ShowRegistrationModal = dialog => dialog.ShowDialog(FindForm());

        _beneficiaryService = beneficiaryService ?? throw new ArgumentNullException(nameof(beneficiaryService));
        _checkInService = checkInService ?? throw new ArgumentNullException(nameof(checkInService));
        _orgService = orgService ?? throw new ArgumentNullException(nameof(orgService));
        _ageCategoryService = ageCategoryService ?? throw new ArgumentNullException(nameof(ageCategoryService));
        _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _currentWorkstation = currentWorkstation ?? throw new ArgumentNullException(nameof(currentWorkstation));
        _formFactory = formFactory ?? throw new ArgumentNullException(nameof(formFactory));
        _notifyService = notifyService ?? throw new ArgumentNullException(nameof(notifyService));
        _deskSounds = deskSounds ?? throw new ArgumentNullException(nameof(deskSounds));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cardScanService = cardScanService ?? throw new ArgumentNullException(nameof(cardScanService));
        _giftVoucherService = giftVoucherService ?? throw new ArgumentNullException(nameof(giftVoucherService));

        _giftPanel.InitializeServices(
            _giftVoucherService,
            _currentUser,
            _formFactory,
            _notifyService,
            giftPanelLogger,
            _timeZoneId);

        InitializeControlText();
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
        _lblCardAlert.Text = Strings.Desk_CardPatternAlert;
        _lblHolderNatIdTitle.Text = Strings.Label_HolderNationalId;
        _lblHolderExpiryTitle.Text = Strings.Label_HolderExpiry;
        _lblHolderLastEntryTitle.Text = Strings.Label_HolderLastEntry;
        _lblHolderStatusTitle.Text = Strings.Label_HolderStatus;

        // Inline Registration

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
                if (!_registrationPanel.Visible)
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
            if (!_registrationPanel.Visible)
            {
                FocusCardNumberBox();
            }
        };
        _btnClear.Click += (_, _) => ResetDesk();
        _btnNewHolder.Click += (_, _) => StartManualRegistration();
        _flowCounters.ClientSizeChanged += (_, _) => ResizeCounters();
        _registrationPanel.CancelRequested += (_, _) => HideInlineRegistration();
        _registrationPanel.SaveRequested += async (_, _) => await SaveInlineRegistrationAsync();
        _btnRecordEntry.Click += async (_, _) => await RecordEntryAsync();
        _btnOverride.Click += async (_, _) => await OverrideEntryAsync();

        _todayPanel.VoidRequested += async (_, _) => await VoidSelectedTodayEntryAsync();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (FocusCounter(keyData))
        {
            return true;
        }

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

            case Keys.F8:
                // R5.6: void the entry selected in today's grid (same as its button and context menu).
                if (_todayPanel.SelectedVoidableAttempt != null)
                {
                    _ = VoidSelectedTodayEntryAsync();
                    return true;
                }
                break;

            case Keys.Escape:
                if (_cmbOrganization.DroppedDown || _registrationPanel.IsOrganizationListOpen || _registrationPanel.IsCityListOpen)
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
        var hasInput = _registrationPanel.Visible && _registrationPanel.HasInput;

        return !hasInput || (_notifyService.Confirm(Strings.Confirm_DiscardRegistration, Strings.Title_Warning));
    }

    public async Task OnDeskLoadedAsync()
    {
        await LoadSettingsAsync();
        await LoadOrganizationsAsync();
        await LoadCitiesAsync();
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
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            _maxGroupSize = settings.MaxGroupSize;
            _prefillFromLastVisit = settings.PrefillCompanionsFromLastVisit;
            _denialSoundEnabled = settings.DenialSoundEnabled;
            _successSoundEnabled = settings.SuccessSoundEnabled;
            _timeZoneId = settings.TimeZoneId;
            _giftPanel.SetTimeZone(_timeZoneId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load check-in settings.");
        }
    }

    private async Task LoadOrganizationsAsync()
    {
        try
        {
            var orgs = await _orgService.GetAllAsync(activeOnly: true);
            _organizations.Clear();
            _organizations.AddRange(orgs);

            _cmbOrganization.Items.Clear();
            _cmbOrganization.Items.Add(new OrgComboItem(null, Strings.Filter_AllOrganizations));

            // The registration panel offers real organizations only, never «الكل» (R4.3).
            _registrationPanel.SetOrganizations(orgs.Select(o => (o.Id, o.NameAr)));
            foreach (var org in orgs)
            {
                _cmbOrganization.Items.Add(new OrgComboItem(org.Id, org.NameAr));
            }

            _cmbOrganization.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load organizations for filter.");
        }
    }

    private async Task LoadCitiesAsync()
    {
        try
        {
            var result = await _cityService.GetActiveAsync();
            if (result.IsSuccess)
            {
                _registrationPanel.SetCities(result.Value.Select(c => (c.Id, c.NameAr)));
                return;
            }

            _logger.LogWarning("Failed to load cities {ErrorCode}.", result.Error.Code);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load cities for registration.");
        }

        // The city is optional (BR-32): registration still works with an empty list.
        _registrationPanel.SetCities([]);
    }

    public async Task LoadAgeCategoriesAsync()
    {
        try
        {
            // No categories configured is valid: the holder enters alone. Never invent categories (R4.9).
            PopulateAgeCategories(await _ageCategoryService.GetActiveAsync());
            _ageCategoriesUnavailable = false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load age categories. Recording is disabled until they load.");
            PopulateAgeCategories([]);
            _ageCategoriesUnavailable = true;
            _statusBanner.SetDenied(Strings.Banner_ConnectionFailed, Strings.Banner_AgeCategoriesUnavailable);
        }

        UpdateActionButtons();
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
            // R5.4: Alt+1…9 goes to counter n; the title shows its shortcut.
            var number = _counterInputs.Count + 1;
            var counter = new CounterInput
            {
                Title = number <= MaxCounterShortcut
                    ? string.Format(Strings.Label_CounterWithShortcut, cat.NameAr, number)
                    : $"{cat.NameAr}:",
                Tag = cat.Id,
                Width = CounterWidth(),
                Height = 56,
                Margin = new Padding(2, 4, 2, 4),
                MinValue = 0,
                MaxValue = _maxGroupSize - 1,
                Value = 0
            };

            counter.ValueChanged += (_, _) => OnCounterChanged();
            counter.ConfirmRequested += async (_, _) => await RecordEntryFromCounterAsync();
            _counterInputs.Add(counter);
            _flowCounters.Controls.Add(counter);
        }

        _flowCounters.ResumeLayout(true);
        UpdateLiveTotalPersons();
    }

    private const int MaxCounterShortcut = 9;
    private const int MinCounterWidth = 220;

    private void ResizeCounters()
    {
        var width = CounterWidth();
        foreach (var counter in _counterInputs)
        {
            counter.Width = width;
        }
    }

    /// <summary>R5.8: two counters per row, so four categories fit in the compact middle area.</summary>
    private int CounterWidth() =>
        Math.Max(LogicalToDeviceUnits(MinCounterWidth), (_flowCounters.ClientSize.Width - LogicalToDeviceUnits(16)) / 2);

    // R5.7: each cue has its own setting (Entry.SuccessSoundEnabled, Entry.DenialSoundEnabled).
    private void PlayGrantedSound()
    {
        if (_successSoundEnabled)
        {
            _deskSounds.PlayGranted();
        }
    }

    private void PlayDeniedSound()
    {
        if (_denialSoundEnabled)
        {
            _deskSounds.PlayDenied();
        }
    }

    /// <summary>Enter in a counter records the entry when it may be recorded (same rule as F5).</summary>
    private async Task RecordEntryFromCounterAsync()
    {
        if (_btnRecordEntry.Enabled)
        {
            await RecordEntryAsync();
        }
    }

    /// <summary>Alt+1…9: focus counter n (R5.4). False when there is no such counter.</summary>
    private bool FocusCounter(Keys keyData)
    {
        if ((keyData & Keys.Modifiers) != Keys.Alt)
        {
            return false;
        }

        var key = keyData & Keys.KeyCode;
        var number = key is >= Keys.D1 and <= Keys.D9 ? key - Keys.D1 + 1
            : key is >= Keys.NumPad1 and <= Keys.NumPad9 ? key - Keys.NumPad1 + 1
            : 0;
        if (number == 0 || number > _counterInputs.Count)
        {
            return false;
        }

        _counterInputs[number - 1].FocusValue();
        return true;
    }

    private void OnCounterChanged()
    {
        if (!_isPrefilling)
        {
            MarkCompanionsPrefilled(false);
        }

        UpdateLiveTotalPersons();
    }

    private void MarkCompanionsPrefilled(bool prefilled)
    {
        _companionsPrefilled = prefilled;
        foreach (var counter in _counterInputs)
        {
            counter.MarkAsPrefilled(prefilled);
        }
    }

    private void UpdateLiveTotalPersons()
    {
        var companionsCount = _counterInputs.Sum(c => c.Value);
        var totalPersons = 1 + companionsCount; // BR-08: Holder counts as one person

        _lblTotalPersons.Text = _companionsPrefilled
            ? $"{string.Format(Strings.Label_TotalPersons, totalPersons)} {Strings.Label_FromLastVisit}"
            : string.Format(Strings.Label_TotalPersons, totalPersons);

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
                    _pnlCompanionsTotal.BackColor = Theme.DangerTint;
                }
                else
                {
                    _lblFreeCompanions.Text = string.Format(Strings.Label_FreeCompanionsCount, nonExemptCompanions, maxFreeLimit);
                    _lblFreeCompanions.ForeColor = StatusBanner.ColorNeutral;
                    _pnlCompanionsTotal.BackColor = Theme.SuccessTint;
                }
            }
            else
            {
                _lblFreeCompanions.Visible = false;
                _pnlCompanionsTotal.BackColor = Theme.SuccessTint;
            }
        }
        else
        {
            _lblFreeCompanions.Visible = false;
            _pnlCompanionsTotal.BackColor = Theme.SuccessTint;
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
        var holderShown = _activeHolder != null && _activeEligibility != null && !_companionsBlockEntry && !_ageCategoriesUnavailable;
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
        _registrationPanel.ClearFields();

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
        _lblHolderStatus.ForeColor = Theme.TextPrimary;
        ShowCardAlert(false);
    }

    /// <summary>
    /// F8, BR-29: the number does not look like its organization's usual numbers. Amber and never blocking; the
    /// name gives up its last column only while the note shows, so the compact desk keeps its height.
    /// </summary>
    private void ShowCardAlert(bool show)
    {
        _lblCardAlert.Visible = show;
        _tblHolder.SetColumnSpan(_lblHolderName, show ? 2 : 3);
    }

    internal bool IsCardAlertShown => _lblCardAlert.Visible;

    public void ShowHolderInfo(BeneficiaryDto holder, EligibilityPreviewDto? eligibility)
    {
        _activeHolder = holder;
        _activeEligibility = eligibility;

        HideInlineRegistration();

        _lblHolderName.Text = holder.FullName;
        _lblHolderPhone.Text = holder.Phone ?? "-";
        ShowCardAlert(holder.HasOpenCardAlert);
        _lblHolderOrg.Text = holder.OrganizationName;

        // Operators receive the ID already masked from the Application layer (SPEC §8, R6.16).
        if (!string.IsNullOrWhiteSpace(holder.NationalId))
        {
            _lblHolderNatId.Text = holder.NationalId;
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
                        ? string.Format(Strings.Ui_CheckInDesk_01, DateTimeDisplayHelper.FormatDateTime(DateTimeDisplayHelper.ToParkLocalTime(holder.LastGrantedEntryUtc.Value, _timeZoneId)))
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

                if (eligibility.DenialReason == DenialReason.DayNotAllowed)
                {
                    details = string.Format(Strings.Desk_AllowedEntryDays, WeekDaysText.Format(eligibility.AllowedEntryDays));
                }
                else if (eligibility.NextAllowedEntryUtc.HasValue)
                {
                    var nextLocal = DateTimeDisplayHelper.ToParkLocalTime(eligibility.NextAllowedEntryUtc.Value, _timeZoneId);
                    details = CooldownText.Format(nextLocal, eligibility.RemainingCooldownMinutes);
                }

                _statusBanner.SetDenied(reasonText, details);
                _btnRecordEntry.Enabled = false;
                _btnOverride.Enabled = eligibility.CanBeOverridden;

                PlayDeniedSound();
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
        _registrationPanel.Visible = true;
        _registrationPanel.ClearFields();
        SelectRegistrationOrganization(orgId ?? (_cmbOrganization.SelectedItem as OrgComboItem)?.Id);

        _statusBanner.SetWarning(Strings.Ui_CheckInDesk_03, Strings.Ui_CheckInDesk_04);
        _btnRecordEntry.Enabled = false;
        _btnOverride.Enabled = false;

        _registrationPanel.FocusFullName();

        // R5.15: the compact desk (R5.8) has no room for the fields, so they open in a pop-up once this call returns
        // (the lookup that got here is still on the stack). A desk that is not on a shown window keeps them inline.
        if (UseRegistrationDialog)
        {
            BeginInvoke(OpenRegistrationDialog);
        }
    }

    private RegisterVisitorDialog? _registrationDialog;

    private bool UseRegistrationDialog => IsHandleCreated && FindForm() is { Visible: true };

    /// <summary>Test seam: shows the dialog modally over the desk's window.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal Func<RegisterVisitorDialog, DialogResult> ShowRegistrationModal { get; set; }

    /// <summary>Moves the registration panel into a pop-up until the visitor is saved or the form is closed.</summary>
    internal void OpenRegistrationDialog()
    {
        if (_registrationDialog != null || !_registrationPanel.Visible)
        {
            return;
        }

        var home = _registrationPanel.Parent;
        using var dialog = _formFactory.Create<RegisterVisitorDialog>();
        _registrationDialog = dialog;
        dialog.Text = string.Format(Strings.Title_RegisterVisitor, DigitNormalizer.NormalizeDigits(_txtCardNumber.Text.Trim()));
        dialog.ConfirmDiscard = ConfirmDiscardRegistration;
        dialog.Shown += (_, _) => _registrationPanel.FocusFullName();
        home?.Controls.Remove(_registrationPanel);
        dialog.Host(_registrationPanel);
        _grpHolder.Visible = true;

        DialogResult result;
        try
        {
            result = ShowRegistrationModal(dialog);
        }
        finally
        {
            dialog.Release(_registrationPanel);
            home?.Controls.Add(_registrationPanel);
            _registrationDialog = null;
        }

        if (result != DialogResult.OK)
        {
            // Closed or cancelled: back to the empty desk; the card number stays for another try.
            HideInlineRegistration();
            _registrationPanel.ClearFields();
            _statusBanner.Reset();
            FocusCardNumberBox();
        }
    }

    /// <summary>
    /// Preselects the organization for a new holder: the one asked for (the filter), else the default organization.
    /// Leaves it empty when neither exists, so the operator must choose (R4.3).
    /// </summary>
    private void SelectRegistrationOrganization(int? preferredOrgId)
    {
        var targetId = preferredOrgId ?? _organizations.FirstOrDefault(o => o.IsDefault)?.Id;
        _registrationPanel.SelectOrganization(targetId);
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
        if (_registrationDialog is { } dialog && dialog.DialogResult == DialogResult.None)
        {
            // The panel's cancel button: close the pop-up without asking again.
            dialog.ConfirmDiscard = null;
            dialog.DialogResult = DialogResult.Cancel;
        }

        _registrationPanel.Visible = false;
        _grpHolder.Visible = true;
    }

    private void StartManualRegistration()
    {
        var card = DigitNormalizer.NormalizeDigits(_txtCardNumber.Text.Trim());
        if (string.IsNullOrWhiteSpace(card))
        {
            _notifyService.Warning(Strings.Ui_CheckInDesk_05, Strings.Title_Warning);
            FocusCardNumberBox();
            return;
        }

        ShowInlineRegistration(card);
    }

    public async Task LookupCardAsync()
    {
        if (_isSearching)
        {
            // R4.2: a scan that arrives during a lookup is looked up right after it, not dropped.
            _lookupQueued = true;
            return;
        }

        do
        {
            _lookupQueued = false;
            await LookupCardOnceAsync();
        }
        while (_lookupQueued);
    }

    private async Task LookupCardOnceAsync()
    {
        _isSearching = true;
        _lookupCount++;
        try
        {
            var rawCard = _txtCardNumber.Text;
            var normalized = DigitNormalizer.NormalizeDigits(rawCard).Trim();
            _txtCardNumber.Text = normalized;

            if (string.IsNullOrWhiteSpace(normalized))
            {
                ClearPreviousHolderData();
                _statusBanner.SetWarning(Strings.Ui_CheckInDesk_06, null);
                FocusCardNumberBox();
                return;
            }

            // Always clear previous customer data and reset banner when initiating a new search
            ClearPreviousHolderData();
            _statusBanner.Reset();

            if (_ageCategoriesUnavailable)
            {
                // R4.9: retry the companion categories before showing a holder; on failure the banner says so.
                await LoadAgeCategoriesAsync();
                if (_ageCategoriesUnavailable)
                {
                    FocusCardNumberBox();
                    return;
                }
            }

        // 1. Check if scanned input is a signed QR token (203 digits per QrTokenFormat.DigitLength)
        if (normalized.Length == ParkGate.Domain.EntryCards.QrTokenFormat.DigitLength)
        {
            using (new BusyScope(this, _btnSearch, _btnNewHolder))
            {
                try
                {
                    var scanResult = await _cardScanService.ScanAsync(normalized);
                    if (!scanResult.IsAccepted)
                    {
                        _statusBanner.SetDenied(scanResult.MessageAr, Strings.Ui_CheckInDesk_07);
                        PlayDeniedSound();
                        FocusCardNumberBox();
                        return;
                    }

                    if (scanResult.BeneficiaryId.HasValue)
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
                    _logger.LogError(ex, "Error scanning QR card token.");
                    _statusBanner.SetDenied(Strings.Banner_QrScanFailed, UserMessages.Describe(ex));
                    FocusCardNumberBox();
                    return;
                }
            }
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
                _logger.LogError(ex, "Error occurred during card lookup.");
                _statusBanner.SetDenied(Strings.Banner_ConnectionFailed, Strings.Banner_ConnectionRetryHint);
                FocusCardNumberBox();
            }
        }

        if (!_registrationPanel.Visible)
        {
            FocusCardNumberBox();
        }
    }
    finally
    {
        _isSearching = false;
    }
}

    /// <summary>Test seam for the success delay; production waits <see cref="AutoResetDelayMs"/> milliseconds.</summary>
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    internal Func<int, Task> SuccessDelay { get; set; } = milliseconds => Task.Delay(milliseconds);

    /// <summary>
    /// After a granted entry: free the desk at once and keep the success banner for the delay. A card scanned during
    /// the delay is looked up as usual and is not wiped when the delay ends (R4.2). A negative delay keeps the desk.
    /// </summary>
    private async Task FinishSuccessfulEntryAsync(BeneficiaryDto entered)
    {
        if (AutoResetDelayMs < 0)
        {
            await _giftPanel.LoadAsync(entered.Id, entered.FullName);
            return;
        }

        var lookupsBefore = _lookupCount;
        ClearPreviousHolderData();
        _txtCardNumber.Clear();
        FocusCardNumberBox();

        // R5.14: the gift needs today's granted entry (BR-30), so offer it now for the visitor who just entered:
        // F6 issues it without a second scan. A new scan clears the panel and drops this load.
        await _giftPanel.LoadAsync(entered.Id, entered.FullName);

        if (AutoResetDelayMs > 0)
        {
            await SuccessDelay(AutoResetDelayMs);
        }

        if (_lookupCount == lookupsBefore && _statusBanner.State == BannerState.Granted)
        {
            _statusBanner.Reset();
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

        using var dialog = _formFactory.Create<SelectOrganizationDialog>();
        dialog.SetMatches(matches);
        return dialog.ShowDialog(this) == DialogResult.OK ? dialog.SelectedMatch : null;
    }

    private void PrefillCompanions(IReadOnlyList<CompanionCountDto> companions)
    {
        _isPrefilling = true;
        try
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
        finally
        {
            _isPrefilling = false;
        }

        MarkCompanionsPrefilled(_counterInputs.Any(c => c.Value > 0));
        UpdateLiveTotalPersons();
    }

    internal async Task SaveInlineRegistrationAsync()
    {
        var fullName = _registrationPanel.FullName;
        var phone = _registrationPanel.Phone;
        var card = DigitNormalizer.NormalizeDigits(_txtCardNumber.Text.Trim());
        var registrationOrgId = _registrationPanel.SelectedOrganizationId;

        if (registrationOrgId is null)
        {
            _notifyService.Warning(Strings.Val_RegOrganizationRequired, Strings.Title_Warning);
            _registrationPanel.FocusOrganization();
            return;
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            _notifyService.Warning(Strings.Ui_CheckInDesk_08, Strings.Title_Warning);
            _registrationPanel.FocusFullName();
            return;
        }

        using (new BusyScope(this, _registrationPanel.ActionButtons))
        {
            try
            {
                var orgId = registrationOrgId.Value;

                var expiry = _registrationPanel.ExpiryDate;
                var natId = _registrationPanel.NationalId;
                var notes = _registrationPanel.Notes;

                var regRequest = new RegisterBeneficiaryRequest(
                    OrganizationId: orgId,
                    CardNumber: card,
                    FullName: fullName,
                    Phone: string.IsNullOrWhiteSpace(phone) ? null : phone, // BR-15: optional
                    NationalId: natId,
                    CardExpiryDate: expiry,
                    Notes: notes,
                    CityId: _registrationPanel.SelectedCityId); // BR-32: optional

                var regResult = await _beneficiaryService.RegisterAsync(regRequest);
                if (regResult.IsSuccess)
                {
                    if (_registrationDialog != null)
                    {
                        _registrationDialog.DialogResult = DialogResult.OK;
                    }

                    // R5.2: no message box — the operator goes straight on to the holder panel.
                    await LookupCardAsync();
                    if (_activeHolder is not null)
                    {
                        _statusBanner.SetStatus(_statusBanner.State, _statusBanner.Title, Strings.Ui_CheckInDesk_10);
                    }
                }
                else
                {
                    _notifyService.Error(ErrorMessages.GetMessage(regResult.Error), Strings.Ui_CheckInDesk_12);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register beneficiary.");
                _notifyService.Error(UserMessages.Describe(ex), Strings.Title_Error);
            }
        }
    }

    public async Task RecordEntryAsync()
    {
        if (_activeHolder == null)
        {
            return;
        }

        var entered = _activeHolder;

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
                    _statusBanner.SetGranted(Strings.Ui_CheckInDesk_13, string.Format(Strings.Ui_CheckInDesk_14, companions.Sum(c => c.Count), result.Value.TotalPersons));
                    await RefreshTodayDeskSummaryAsync();

                    PlayGrantedSound();
                    await FinishSuccessfulEntryAsync(entered);
                }
                else
                {
                    _statusBanner.SetDenied(ErrorMessages.GetMessage(result.Error));
                    PlayDeniedSound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording entry.");
                _statusBanner.SetDenied(Strings.Ui_CheckInDesk_15, Strings.Ui_CheckInDesk_16);
            }
        }

        UpdateActionButtons();
    }

    public async Task OverrideEntryAsync()
    {
        if (_activeHolder == null)
        {
            return;
        }

        var entered = _activeHolder;

        var supervisorDialog = _formFactory.Create<SupervisorAuthDialog>();

        supervisorDialog.SetContext(defaultReason: Strings.Ui_CheckInDesk_17);

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
                    _statusBanner.SetGranted(Strings.Ui_CheckInDesk_18, string.Format(Strings.Ui_CheckInDesk_19, supervisorDialog.SupervisorFullName, supervisorDialog.Reason));
                    await RefreshTodayDeskSummaryAsync();

                    PlayGrantedSound();
                    await FinishSuccessfulEntryAsync(entered);
                }
                else
                {
                    _statusBanner.SetDenied(ErrorMessages.GetMessage(result.Error));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing override.");
                _statusBanner.SetDenied(Strings.Ui_CheckInDesk_20, Strings.Ui_CheckInDesk_21);
            }
        }

        UpdateActionButtons();
    }

    private static string GetDenialReasonArabic(DenialReason reason) => reason switch
    {
        DenialReason.CooldownActive => Strings.Denial_CooldownActive,
        DenialReason.CardExpired => Strings.Denial_CardExpired,
        DenialReason.HolderSuspended => Strings.Denial_HolderSuspended,
        DenialReason.HolderBlocked => Strings.Denial_HolderBlocked,
        DenialReason.OrganizationInactive => Strings.Denial_OrganizationInactive,
        DenialReason.DayNotAllowed => Strings.Denial_DayNotAllowed,
        _ => Strings.Banner_Denied
    };

    public async Task RefreshTodayDeskSummaryAsync()
    {
        try
        {
            var summaryResult = await _checkInService.GetTodayDeskEntriesAsync(_currentWorkstation.WorkstationId);
            if (summaryResult.IsSuccess)
            {
                _todayPanel.ShowSummary(summaryResult.Value, _timeZoneId, IsHandleCreated);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to refresh today's desk summary.");
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
        if (_todayPanel.SelectedVoidableAttempt is not { } attempt)
        {
            return;
        }

        string voidReason;
        string? approvalToken = null;

        var isSupervisorOrAdmin = (_currentUser.IsInRole(UserRole.Supervisor) || _currentUser.IsInRole(UserRole.Admin));

        if (!isSupervisorOrAdmin)
        {
            var authDialog = _formFactory.Create<SupervisorAuthDialog>();

            authDialog.SetContext(defaultReason: Strings.Ui_Beneficiaries_09);

            if (authDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            voidReason = authDialog.Reason;
            approvalToken = authDialog.ApprovalToken;
        }
        else
        {
            var reasonDialog = _formFactory.Create<VoidReasonDialog>();

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

        var confirmed = Notify.Confirm(Strings.Msg_ConfirmVoid, Strings.Title_Confirm);

        if (!confirmed)
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
                    _notifyService.Info(Strings.Msg_VoidSuccess);

                    await RefreshTodayDeskSummaryAsync();

                    if (_activeHolder != null && string.Equals(_activeHolder.CardNumber, attempt.CardNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        await LookupCardAsync();
                    }
                }
                else
                {
                    var msg = ErrorMessages.GetMessage(result.Error);
                    _notifyService.Warning(msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voiding today entry.");
                var msg = UserMessages.Describe(ex);
                _notifyService.Error(msg);
            }
        }
    }

    private sealed record OrgComboItem(int? Id, string Name)
    {
        public override string ToString() => Name;
    }
}
