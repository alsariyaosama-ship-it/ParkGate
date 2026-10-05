using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
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

    private readonly List<CounterInput> _counterInputs = [];
    private BeneficiaryDto? _activeHolder;
    private EligibilityPreviewDto? _activeEligibility;
    private int _maxGroupSize = 10;
    private bool _prefillFromLastVisit;
    private bool _denialSoundEnabled;
    private string? _timeZoneId = "Libya Standard Time";

    public CheckInDeskControl() : this(null, null, null, null, null, null, null, null, null, null)
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
        ILogger<CheckInDeskControl>? logger)
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
        _lblHolderNatIdTitle.Text = Strings.Label_HolderNationalId;
        _lblHolderExpiryTitle.Text = Strings.Label_HolderExpiry;
        _lblHolderLastEntryTitle.Text = Strings.Label_HolderLastEntry;
        _lblHolderStatusTitle.Text = Strings.Label_HolderStatus;

        // Inline Registration
        _grpRegistration.Text = Strings.Title_InlineRegistration;
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
            }
            else if (e.KeyCode == Keys.Escape)
            {
                ResetDesk();
            }
        };

        _btnSearch.Click += async (_, _) => await LookupCardAsync();
        _btnClear.Click += (_, _) => ResetDesk();
        _btnNewHolder.Click += (_, _) => StartManualRegistration();
        _btnCancelReg.Click += (_, _) => HideInlineRegistration();
        _btnSaveReg.Click += async (_, _) => await SaveInlineRegistrationAsync();
        _btnRecordEntry.Click += async (_, _) => await RecordEntryAsync();
        _btnOverride.Click += async (_, _) => await OverrideEntryAsync();
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

            case Keys.Escape:
                ResetDesk();
                return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
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

            _cmbOrganization.Items.Clear();
            _cmbOrganization.Items.Add(new OrgComboItem(null, Strings.Filter_AllOrganizations));

            foreach (var org in orgs)
            {
                _cmbOrganization.Items.Add(new OrgComboItem(org.Id, org.NameAr));
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
            new AgeCategoryDto(3, "رضيع", "أقل من 3 سنوات", 3, true)
        ]);
    }

    public void PopulateAgeCategories(IReadOnlyList<AgeCategoryDto> categories)
    {
        _flowCounters.SuspendLayout();
        _flowCounters.Controls.Clear();
        _counterInputs.Clear();

        foreach (var cat in categories.OrderBy(c => c.SortOrder))
        {
            var counter = new CounterInput
            {
                Title = $"{cat.NameAr}:",
                Tag = cat.Id,
                Width = _flowCounters.ClientSize.Width - 10,
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

        if (totalPersons > _maxGroupSize)
        {
            _lblTotalPersons.ForeColor = StatusBanner.ColorDenied;
            _btnRecordEntry.Enabled = false;
        }
        else
        {
            _lblTotalPersons.ForeColor = StatusBanner.ColorGranted;
            _btnRecordEntry.Enabled = _activeHolder != null && (_activeEligibility?.IsEligible ?? false);
        }
    }

    public void ResetDesk()
    {
        _activeHolder = null;
        _activeEligibility = null;

        _txtCardNumber.Clear();
        ClearHolderInfo();
        HideInlineRegistration();

        foreach (var counter in _counterInputs)
        {
            counter.Reset();
        }

        _statusBanner.Reset();
        _btnRecordEntry.Enabled = false;
        _btnOverride.Enabled = false;

        UpdateLiveTotalPersons();
        FocusCardNumberBox();
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
                var reasonText = !string.IsNullOrWhiteSpace(eligibility.DenialReasonText)
                    ? eligibility.DenialReasonText
                    : Strings.Banner_Denied;

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

        _statusBanner.SetWarning("البطاقة غير مسجلة — يرجى تسجيل بيانات المستفيد", "أدخل الاسم الرباعي ورقم الهاتف للمتابعة");
        _btnRecordEntry.Enabled = false;
        _btnOverride.Enabled = false;

        _txtRegFullName.Focus();
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
            _notifyService?.Warning("يرجى إدخال رقم البطاقة أولاً لتسجيل حامل جديد.", "تنبيه");
            FocusCardNumberBox();
            return;
        }

        ShowInlineRegistration(card);
    }

    public async Task LookupCardAsync()
    {
        var rawCard = _txtCardNumber.Text;
        var normalized = DigitNormalizer.NormalizeDigits(rawCard).Trim();
        _txtCardNumber.Text = normalized;

        if (string.IsNullOrWhiteSpace(normalized))
        {
            _statusBanner.SetWarning("يرجى إدخال أو مسح رقم البطاقة", null);
            FocusCardNumberBox();
            return;
        }

        if (_beneficiaryService == null)
        {
            return;
        }

        using (new BusyScope(this, _btnSearch, _btnRecordEntry, _btnOverride, _btnNewHolder))
        {
            try
            {
                var selectedOrgId = (_cmbOrganization.SelectedItem as OrgComboItem)?.Id;
                var lookupResult = await _beneficiaryService.LookupByCardNumberAsync(normalized);

                if (lookupResult.IsFailure)
                {
                    _statusBanner.SetDenied(ErrorMessages.GetMessage(lookupResult.Error));
                    return;
                }

                var matches = lookupResult.Value;
                if (selectedOrgId.HasValue)
                {
                    matches = matches.Where(m => m.Beneficiary.OrganizationId == selectedOrgId.Value).ToList();
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
                    // Flow A2: Card found in multiple organizations
                    matched = DisambiguateOrganization(matches) ?? matches[0];
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
                _statusBanner.SetDenied("تعذّر الاتصال بقاعدة البيانات", ex.Message);
            }
        }
    }

    private BeneficiaryLookupResultDto? DisambiguateOrganization(IReadOnlyList<BeneficiaryLookupResultDto> matches)
    {
        // Simple prompt or selection
        using var form = new RtlFormBase
        {
            Text = Strings.Title_SelectOrganization,
            Width = 400,
            Height = 250,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var lbl = new Label
        {
            Text = Strings.Header_SelectOrganization,
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(8)
        };

        var list = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10.5F)
        };

        foreach (var m in matches)
        {
            list.Items.Add($"{m.Beneficiary.OrganizationName} — {m.Beneficiary.FullName}");
        }
        list.SelectedIndex = 0;

        var btn = new Button
        {
            Text = Strings.Btn_SelectOrg,
            Dock = DockStyle.Bottom,
            Height = 40,
            BackColor = StatusBanner.ColorGranted,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btn.Click += (_, _) => form.DialogResult = DialogResult.OK;

        form.Controls.Add(list);
        form.Controls.Add(btn);
        form.Controls.Add(lbl);

        return form.ShowDialog(this) == DialogResult.OK && list.SelectedIndex >= 0
            ? matches[list.SelectedIndex]
            : matches[0];
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

    private async Task SaveInlineRegistrationAsync()
    {
        var fullName = _txtRegFullName.Text.Trim();
        var phone = _txtRegPhone.Text.Trim();
        var card = DigitNormalizer.NormalizeDigits(_txtCardNumber.Text.Trim());
        var selectedOrgId = (_cmbOrganization.SelectedItem as OrgComboItem)?.Id;

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
                int orgId;
                if (selectedOrgId.HasValue)
                {
                    orgId = selectedOrgId.Value;
                }
                else
                {
                    var orgs = await _orgService?.GetAllAsync(activeOnly: true)!;
                    orgId = orgs?.FirstOrDefault()?.Id ?? 1;
                }

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
                    _notifyService?.Info("تم تسجيل حامل البطاقة بنجاح.", "تم التسجيل");
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

                    await Task.Delay(1200);
                    ResetDesk();
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
                _statusBanner.SetDenied("فشل تسجيل الدخول", ex.Message);
            }
        }
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
                    SupervisorUserId: supervisorDialog.SupervisorUserId,
                    Companions: companions,
                    Channel: EntryChannel.Desk);

                var result = await _checkInService.OverrideEntryAsync(request);
                if (result.IsSuccess)
                {
                    _statusBanner.SetGranted("تم تسجيل الدخول بموافقة المشرف (تجاوز)", $"المشرف: {supervisorDialog.SupervisorFullName} · السبب: {supervisorDialog.Reason}");
                    await RefreshTodayDeskSummaryAsync();

                    await Task.Delay(1500);
                    ResetDesk();
                }
                else
                {
                    _statusBanner.SetDenied(ErrorMessages.GetMessage(result.Error));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing override.");
                _statusBanner.SetDenied("فشل التجاوز", ex.Message);
            }
        }
    }

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
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to refresh today's desk summary.");
        }
    }

    private sealed record OrgComboItem(int? Id, string Name)
    {
        public override string ToString() => Name;
    }
}
