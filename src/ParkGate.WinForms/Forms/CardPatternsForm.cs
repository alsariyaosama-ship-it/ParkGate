using System.Globalization;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.CardPatterns;
using ParkGate.Application.Settings.Services;
using ParkGate.Domain.CardPatterns;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Admin screen for the card-number patterns (F8, BR-29): approve or reject proposed patterns, review alerts, and
/// open a holder to suspend or block. Nothing here is ever decided automatically.
/// </summary>
public partial class CardPatternsForm : RtlFormBase
{
    private readonly ICardPatternService _cardPatternService;
    private readonly IBeneficiaryService _beneficiaryService;
    private readonly ISettingsService _settingsService;
    private readonly INotifyService _notifyService;
    private readonly IFormFactory _formFactory;
    private readonly ILogger<CardPatternsForm> _logger;
    private string? _timeZoneId;

    public CardPatternsForm(
        ICardPatternService cardPatternService,
        IBeneficiaryService beneficiaryService,
        ISettingsService settingsService,
        INotifyService notifyService,
        IFormFactory formFactory,
        ILogger<CardPatternsForm> logger)
    {
        InitializeComponent();
        _cardPatternService = cardPatternService ?? throw new ArgumentNullException(nameof(cardPatternService));
        _beneficiaryService = beneficiaryService ?? throw new ArgumentNullException(nameof(beneficiaryService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _notifyService = notifyService ?? throw new ArgumentNullException(nameof(notifyService));
        _formFactory = formFactory ?? throw new ArgumentNullException(nameof(formFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeTexts();
        Theme.ApplyGridStyle(_gridPatterns);
        Theme.ApplyGridStyle(_gridAlerts);
        Theme.FitColumnsToHeaders(_gridPatterns);
        Theme.FitColumnsToHeaders(_gridAlerts);
        WireEvents();
    }

    internal IReadOnlyList<CardPatternDto> ShownPatterns { get; private set; } = [];

    internal IReadOnlyList<CardPatternAlertDto> ShownAlerts { get; private set; } = [];

    private void InitializeTexts()
    {
        Text = Strings.Title_CardPatterns;
        _tabPatterns.Text = Strings.Tab_CardPatterns;
        _tabAlerts.Text = Strings.Tab_CardPatternAlerts;
        _lblPatternHint.Text = Strings.CardPattern_Hint;
        _btnApprove.Text = Strings.Btn_ApprovePattern;
        _btnReject.Text = Strings.Btn_RejectPattern;
        _chkShowReviewed.Text = Strings.Label_ShowReviewedAlerts;
        _lblReviewNote.Text = Strings.Label_ReviewNote;
        _btnReview.Text = Strings.Btn_MarkReviewed;
        _btnChangeStatus.Text = Strings.Btn_SuspendOrBlockHolder;
        _btnClose.Text = Strings.Btn_Close;
        _lblSummary.Text = string.Empty;

        _colPatternOrg.HeaderText = Strings.CardPattern_Col_Organization;
        _colPatternDisplay.HeaderText = Strings.CardPattern_Col_Shape;
        _colPatternSource.HeaderText = Strings.CardPattern_Col_Source;
        _colPatternStatus.HeaderText = Strings.CardPattern_Col_Status;
        _colPatternSample.HeaderText = Strings.CardPattern_Col_Sample;
        _colPatternProposed.HeaderText = Strings.CardPattern_Col_Proposed;
        _colPatternDecision.HeaderText = Strings.CardPattern_Col_Decision;

        _colAlertTime.HeaderText = Strings.CardPattern_Col_Time;
        _colAlertOrg.HeaderText = Strings.CardPattern_Col_Organization;
        _colAlertCard.HeaderText = Strings.CardPattern_Col_Card;
        _colAlertHolder.HeaderText = Strings.CardPattern_Col_Holder;
        _colAlertShape.HeaderText = Strings.CardPattern_Col_Shape;
        _colAlertBy.HeaderText = Strings.CardPattern_Col_By;
        _colAlertDesk.HeaderText = Strings.CardPattern_Col_Desk;
        _colAlertStatus.HeaderText = Strings.CardPattern_Col_Status;
        _colAlertNote.HeaderText = Strings.CardPattern_Col_Note;

        Theme.ApplyPrimaryButtonStyle(_btnApprove);
        Theme.ApplySecondaryButtonStyle(_btnReject);
        Theme.ApplyPrimaryButtonStyle(_btnReview);
        Theme.ApplySecondaryButtonStyle(_btnChangeStatus);
        Theme.ApplySecondaryButtonStyle(_btnClose);
    }

    private void WireEvents()
    {
        // Shown, not Load: rows added during Load select a row in a grid on a tab page whose handle does not exist
        // yet, which made the tab control fail with "Error creating window handle" on a 125 % screen (T8.4b).
        Shown += async (_, _) => await OnShownAsync();
        _btnClose.Click += (_, _) => Close();
        _gridPatterns.SelectionChanged += (_, _) => UpdateButtons();
        _gridAlerts.SelectionChanged += (_, _) => UpdateButtons();
        _tabs.SelectedIndexChanged += (_, _) => UpdateSummary();
        _chkShowReviewed.CheckedChanged += async (_, _) => await LoadAlertsAsync();
        _btnApprove.Click += async (_, _) => await DecideAsync(approve: true);
        _btnReject.Click += async (_, _) => await DecideAsync(approve: false);
        _btnReview.Click += async (_, _) => await ReviewSelectedAlertAsync();
        _btnChangeStatus.Click += async (_, _) => await ChangeSelectedHolderStatusAsync();
    }

    private async Task OnShownAsync()
    {
        Theme.FitColumnsToHeaders(_gridPatterns); // now at the window's DPI
        Theme.FitColumnsToHeaders(_gridAlerts);
        FitTimeColumns();
        try
        {
            _timeZoneId = (await _settingsService.GetSettingsAsync()).TimeZoneId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read the park time zone for the card patterns screen.");
        }

        await ReloadAsync();
    }

    internal async Task ReloadAsync()
    {
        await LoadPatternsAsync();
        await LoadAlertsAsync();
    }

    private async Task LoadPatternsAsync()
    {
        try
        {
            var result = await _cardPatternService.GetPatternsAsync();
            if (result.IsFailure)
            {
                _notifyService.Warning(ErrorMessages.GetMessage(result.Error));
                return;
            }

            ShownPatterns = result.Value;
            _gridPatterns.Rows.Clear();
            foreach (var p in ShownPatterns)
            {
                var index = _gridPatterns.Rows.Add(
                    p.OrganizationName,
                    LeftToRight(p.Display),
                    SourceText(p.Source),
                    StatusText(p),
                    p.SampleCount.ToString(CultureInfo.InvariantCulture),
                    LocalTime(p.ProposedAtUtc),
                    p.DecidedAtUtc is { } at ? $"{p.DecidedByName} {LocalTime(at)}".Trim() : string.Empty);
                _gridPatterns.Rows[index].Tag = p;
                if (p.Status == CardPatternStatus.Proposed)
                {
                    _gridPatterns.Rows[index].DefaultCellStyle.ForeColor = Theme.Warning;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load card-number patterns.");
            _notifyService.Error(UserMessages.Describe(ex));
        }

        UpdateButtons();
        UpdateSummary();
    }

    private async Task LoadAlertsAsync()
    {
        try
        {
            var result = await _cardPatternService.GetAlertsAsync(openOnly: !_chkShowReviewed.Checked);
            if (result.IsFailure)
            {
                _notifyService.Warning(ErrorMessages.GetMessage(result.Error));
                return;
            }

            ShownAlerts = result.Value;
            _gridAlerts.Rows.Clear();
            foreach (var a in ShownAlerts)
            {
                var index = _gridAlerts.Rows.Add(
                    LocalTime(a.CreatedAtUtc),
                    a.OrganizationName,
                    LeftToRight(a.CardNumber),
                    a.HolderName,
                    LeftToRight(CardShape.Describe(a.Shape, null)),
                    a.CreatedByName ?? string.Empty,
                    a.WorkstationName ?? string.Empty,
                    a.Status == CardPatternAlertStatus.Open
                        ? Strings.CardPattern_AlertOpen
                        : string.Format(Strings.CardPattern_AlertReviewedBy, a.ReviewedByName),
                    a.ReviewNote ?? string.Empty);
                _gridAlerts.Rows[index].Tag = a;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load card-number alerts.");
            _notifyService.Error(UserMessages.Describe(ex));
        }

        UpdateButtons();
        UpdateSummary();
    }

    private CardPatternDto? SelectedPattern => _gridPatterns.CurrentRow?.Tag as CardPatternDto;

    private CardPatternAlertDto? SelectedAlert => _gridAlerts.CurrentRow?.Tag as CardPatternAlertDto;

    private void UpdateButtons()
    {
        var proposed = SelectedPattern is { Status: CardPatternStatus.Proposed };
        _btnApprove.Enabled = proposed;
        _btnReject.Enabled = proposed;
        _btnReview.Enabled = SelectedAlert is { Status: CardPatternAlertStatus.Open };
        _btnChangeStatus.Enabled = SelectedAlert is not null;
    }

    private void UpdateSummary()
    {
        var waiting = ShownPatterns.Count(p => p.Status == CardPatternStatus.Proposed);
        var open = ShownAlerts.Count(a => a.Status == CardPatternAlertStatus.Open);
        _lblSummary.Text = string.Format(Strings.CardPattern_Summary, waiting, open);
    }

    /// <summary>Test seam: the confirmation question.</summary>
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    internal Func<string, bool>? ConfirmOverride { get; set; }

    internal async Task DecideAsync(bool approve)
    {
        if (SelectedPattern is not { Status: CardPatternStatus.Proposed } pattern)
        {
            return;
        }

        var question = string.Format(approve ? Strings.Msg_ConfirmApprovePattern : Strings.Msg_ConfirmRejectPattern, pattern.Display, pattern.OrganizationName);
        if (!(ConfirmOverride?.Invoke(question) ?? _notifyService.Confirm(question, Strings.Title_Confirm)))
        {
            return;
        }

        using (new BusyScope(this, _btnApprove, _btnReject))
        {
            try
            {
                if (approve)
                {
                    var result = await _cardPatternService.ApprovePatternAsync(pattern.Id);
                    if (result.IsFailure)
                    {
                        _notifyService.Warning(ErrorMessages.GetMessage(result.Error));
                        return;
                    }

                    _notifyService.Info(string.Format(Strings.Msg_PatternApproved, result.Value));
                }
                else
                {
                    var result = await _cardPatternService.RejectPatternAsync(pattern.Id);
                    if (result.IsFailure)
                    {
                        _notifyService.Warning(ErrorMessages.GetMessage(result.Error));
                        return;
                    }

                    _notifyService.Info(Strings.Msg_PatternRejected);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decide card-number pattern {PatternId}", pattern.Id);
                _notifyService.Error(UserMessages.Describe(ex));
                return;
            }
        }

        await ReloadAsync();
    }

    internal async Task ReviewSelectedAlertAsync()
    {
        if (SelectedAlert is not { Status: CardPatternAlertStatus.Open } alert)
        {
            return;
        }

        using (new BusyScope(this, _btnReview))
        {
            try
            {
                var result = await _cardPatternService.ReviewAlertAsync(alert.Id, _txtReviewNote.Text);
                if (result.IsFailure)
                {
                    _notifyService.Warning(ErrorMessages.GetMessage(result.Error));
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to review card-number alert {AlertId}", alert.Id);
                _notifyService.Error(UserMessages.Describe(ex));
                return;
            }
        }

        _txtReviewNote.Clear();
        await LoadAlertsAsync();
    }

    private async Task ChangeSelectedHolderStatusAsync()
    {
        if (SelectedAlert is not { } alert)
        {
            return;
        }

        try
        {
            var holder = await _beneficiaryService.GetByIdAsync(alert.BeneficiaryId);
            if (holder.IsFailure)
            {
                _notifyService.Warning(ErrorMessages.GetMessage(holder.Error));
                return;
            }

            using var dialog = _formFactory.Create<ChangeBeneficiaryStatusDialog>();
            dialog.SetBeneficiary(holder.Value.Beneficiary);
            dialog.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open holder {BeneficiaryId} from a card-number alert", alert.BeneficiaryId);
            _notifyService.Error(UserMessages.Describe(ex));
        }
    }

    internal static string SourceText(CardPatternSource source) =>
        source == CardPatternSource.Dominant ? Strings.CardPattern_SourceDominant : Strings.CardPattern_SourceNewSeries;

    internal static string StatusText(CardPatternDto pattern) => pattern.Status switch
    {
        CardPatternStatus.Approved => Strings.CardPattern_StatusApproved,
        CardPatternStatus.Rejected => Strings.CardPattern_StatusRejected,
        _ when pattern.IsActive => Strings.CardPattern_StatusProposedActive,
        _ => Strings.CardPattern_StatusProposed,
    };

    /// <summary>
    /// Card numbers and shapes read left to right inside the right-to-left grid; without the embedding, "AA-##"
    /// showed as "##-AA" because "#" and "-" take the direction of the cell.
    /// </summary>
    internal static string LeftToRight(string text) => "\u202A" + text + "\u202C";

    /// <summary>Date-and-time columns always show their whole text.</summary>
    private void FitTimeColumns()
    {
        var sample = LocalTime(new DateTime(2026, 12, 28, 10, 58, 58, DateTimeKind.Utc));
        foreach (var (grid, column) in new[] { (_gridAlerts, _colAlertTime), (_gridPatterns, _colPatternProposed) })
        {
            var font = grid.DefaultCellStyle.Font ?? grid.Font;
            var needed = TextRenderer.MeasureText(sample, font).Width + grid.DefaultCellStyle.Padding.Horizontal + LogicalToDeviceUnits(16);
            column.MinimumWidth = Math.Max(column.MinimumWidth, needed);
        }
    }

    private string LocalTime(DateTime utc) =>
        DateTimeDisplayHelper.FormatDateTime(DateTimeDisplayHelper.ToParkLocalTime(utc, _timeZoneId));
}
