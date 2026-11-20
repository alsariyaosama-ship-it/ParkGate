using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Audit;
using ParkGate.Application.Settings.Services;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Audit trail screen (R5.20): who did what, when, on which desk, for which visitor, with whose approval and why.
/// Read-only; supervisors and admins (<c>Permission.ViewAuditLog</c>).
/// </summary>
public partial class AuditLogForm : RtlFormBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ISettingsService _settingsService;
    private readonly INotifyService _notifyService;
    private readonly ILogger<AuditLogForm> _logger;
    private string? _timeZoneId;
    private List<AuditEntryDto> _rows = [];

    public AuditLogForm(IAuditLogService auditLogService, ISettingsService settingsService, INotifyService notifyService, ILogger<AuditLogForm> logger)
    {
        InitializeComponent();
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _notifyService = notifyService ?? throw new ArgumentNullException(nameof(notifyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeTexts();
        Theme.ApplyGridStyle(_gridAudit);
        Theme.FitColumnsToHeaders(_gridAudit);
        WireEvents();
    }

    internal IReadOnlyList<AuditEntryDto> ShownRows => _rows;

    internal string SummaryText => _lblSummary.Text;

    private void InitializeTexts()
    {
        Text = Strings.Title_AuditLog;
        _lblFrom.Text = Strings.Label_DateFrom;
        _lblTo.Text = Strings.Label_DateTo;
        _lblCategory.Text = Strings.Label_AuditCategory;
        _lblCard.Text = Strings.Label_AuditCard;
        _btnSearch.Text = Strings.Btn_Refresh;
        _btnClose.Text = Strings.Btn_Close;
        _grpDetails.Text = Strings.Title_AuditDetails;
        Theme.ApplyPrimaryButtonStyle(_btnSearch);
        Theme.ApplySecondaryButtonStyle(_btnClose);

        _colTime.HeaderText = Strings.Audit_Col_Time;
        _colAction.HeaderText = Strings.Audit_Col_Action;
        _colUser.HeaderText = Strings.Audit_Col_User;
        _colDesk.HeaderText = Strings.Audit_Col_Desk;
        _colCard.HeaderText = Strings.Audit_Col_Card;
        _colVisitor.HeaderText = Strings.Audit_Col_Visitor;
        _colReason.HeaderText = Strings.Audit_Col_Reason;
        _colOperator.HeaderText = Strings.Audit_Col_Operator;
        _colApprover.HeaderText = Strings.Audit_Col_Approver;

        _cmbCategory.Items.Clear();
        foreach (var category in Enum.GetValues<AuditCategory>())
        {
            _cmbCategory.Items.Add(new CategoryItem(category, Strings.GetString($"Audit_Category_{category}")));
        }

        _cmbCategory.SelectedIndex = 0;
        _dtpFrom.Value = DateTime.Today;
        _dtpTo.Value = DateTime.Today;
        _lblSummary.Text = string.Empty;
    }

    private void WireEvents()
    {
        Load += async (_, _) => await OnLoadAsync();
        _btnSearch.Click += async (_, _) => await SearchAsync();
        _btnClose.Click += (_, _) => Close();
        _gridAudit.SelectionChanged += (_, _) => ShowSelectedDetails();
    }

    private async Task OnLoadAsync()
    {
        Theme.FitColumnsToHeaders(_gridAudit); // now at the window's DPI
        FitTimeColumn();
        try
        {
            _timeZoneId = (await _settingsService.GetSettingsAsync()).TimeZoneId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read the park time zone for the audit screen.");
        }

        await SearchAsync();
    }

    internal async Task SearchAsync()
    {
        var category = (_cmbCategory.SelectedItem as CategoryItem)?.Value ?? AuditCategory.All;
        var request = new AuditLogSearchRequest(
            DateOnly.FromDateTime(_dtpFrom.Value), DateOnly.FromDateTime(_dtpTo.Value), category, _txtCard.Text);

        using (new BusyScope(this, _btnSearch))
        {
            try
            {
                var result = await _auditLogService.SearchAsync(request);
                if (result.IsFailure)
                {
                    _notifyService.Warning(ErrorMessages.GetMessage(result.Error));
                    return;
                }

                ShowPage(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search the audit trail.");
                _notifyService.Error(UserMessages.Describe(ex));
            }
        }
    }

    /// <summary>The time and operation columns always show their whole text.</summary>
    private void FitTimeColumn()
    {
        var sample = LocalTime(new DateTime(2026, 12, 28, 10, 58, 58, DateTimeKind.Utc), _timeZoneId);
        var font = _gridAudit.DefaultCellStyle.Font ?? _gridAudit.Font;
        var needed = TextRenderer.MeasureText(sample, font).Width + _gridAudit.DefaultCellStyle.Padding.Horizontal + LogicalToDeviceUnits(16);
        _colTime.MinimumWidth = Math.Max(_colTime.MinimumWidth, needed);

        // The longest operation name, so no name is cut.
        var longest = Enum.GetValues<AuditCategory>().SelectMany(c => AuditActions.Of(c) ?? []).Select(ActionName)
            .Max(name => TextRenderer.MeasureText(name, font).Width);
        _colAction.MinimumWidth = Math.Max(_colAction.MinimumWidth, longest + _gridAudit.DefaultCellStyle.Padding.Horizontal + LogicalToDeviceUnits(16));
    }

    private void ShowPage(AuditLogPage page)
    {
        _rows = page.Rows.ToList();
        _lblSummary.Text = page.IsTruncated
            ? string.Format(Strings.Audit_SummaryTruncated, _rows.Count)
            : string.Format(Strings.Audit_Summary, _rows.Count);

        _gridAudit.Rows.Clear();
        foreach (var row in _rows)
        {
            var index = _gridAudit.Rows.Add(
                LocalTime(row.AtUtc, _timeZoneId),
                ActionName(row.Action),
                row.ActorName,
                row.WorkstationName,
                row.CardNumber,
                row.HolderName,
                row.Reason,
                row.OperatorName,
                row.ApprovedByName);
            _gridAudit.Rows[index].Tag = row;
        }

        ShowSelectedDetails();
    }

    private void ShowSelectedDetails() =>
        _txtDetails.Text = _gridAudit.CurrentRow?.Tag is AuditEntryDto row ? FormatDetails(row, _timeZoneId) : string.Empty;

    /// <summary>Arabic name of an audit action; an action without one is shown as it is.</summary>
    internal static string ActionName(string action) =>
        Strings.GetOptionalString($"Audit_Action_{action.Replace('.', '_')}") ?? action;

    /// <summary>One line per detail, with Arabic labels, resolved user names and park-local times.</summary>
    internal static string FormatDetails(AuditEntryDto row, string? timeZoneId = null)
    {
        if (string.IsNullOrWhiteSpace(row.DetailsJson))
        {
            return string.Empty;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(row.DetailsJson);
        }
        catch (JsonException)
        {
            return row.DetailsJson;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return row.DetailsJson;
            }

            var text = new StringBuilder();
            foreach (var property in document.RootElement.EnumerateObject())
            {
                var label = Strings.GetOptionalString($"Audit_Field_{property.Name}") ?? property.Name;
                text.Append(label).Append(": ").AppendLine(FormatValue(property, row, timeZoneId));
            }

            return text.ToString().TrimEnd();
        }
    }

    private static string FormatValue(JsonProperty property, AuditEntryDto row, string? timeZoneId)
    {
        var value = property.Value;
        if (property.Name == "OperatorUserId" && row.OperatorName is { } operatorName)
        {
            return operatorName;
        }

        if (property.Name == "ApprovedByUserId" && row.ApprovedByName is { } approverName)
        {
            return approverName;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Null => "-",
            JsonValueKind.True => Strings.Audit_Yes,
            JsonValueKind.False => Strings.Audit_No,
            JsonValueKind.String when property.Name.EndsWith("BusinessDate", StringComparison.Ordinal) &&
                                      DateOnly.TryParseExact(value.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day) =>
                DateTimeDisplayHelper.FormatDate(day.ToDateTime(TimeOnly.MinValue)),
            JsonValueKind.String when property.Name.EndsWith("AtUtc", StringComparison.Ordinal) &&
                                      value.TryGetDateTime(out var at) => LocalTime(DateTime.SpecifyKind(at, DateTimeKind.Utc), timeZoneId),
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.Array => string.Join(Strings.Text_ListSeparator, value.EnumerateArray().Select(FormatItem)),
            _ => value.GetRawText(),
        };
    }

    private static string FormatItem(JsonElement item) =>
        item.ValueKind == JsonValueKind.Object && item.TryGetProperty("AgeCategoryId", out var category) && item.TryGetProperty("Count", out var count)
            ? $"{category.GetRawText()}: {count.GetRawText()}"
            : item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : item.GetRawText();

    private sealed record CategoryItem(AuditCategory Value, string Text)
    {
        public override string ToString() => Text;
    }

    private static string LocalTime(DateTime utc, string? timeZoneId) =>
        DateTimeDisplayHelper.FormatDateTime(DateTimeDisplayHelper.ToParkLocalTime(utc, timeZoneId));
}
