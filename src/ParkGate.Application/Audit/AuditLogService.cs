using System.Text.Json;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Services;
using ParkGate.Domain.Common;

namespace ParkGate.Application.Audit;

/// <summary>
/// Searches the audit trail (R5.20): park-local days, a kind of operation and an optional card number. Names of the
/// acting user, the desk, the operator and the approver are resolved; the full details JSON is kept for the screen.
/// </summary>
public sealed class AuditLogService : IAuditLogService
{
    /// <summary>Rows returned at most; the screen says when the list was cut.</summary>
    public const int MaxRows = 1000;

    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly ICurrentUser _currentUser;
    private readonly ISettingsService _settings;

    public AuditLogService(IUnitOfWorkFactory uowFactory, ICurrentUser currentUser, ISettingsService settings)
    {
        _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<Result<AuditLogPage>> SearchAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ViewAuditLog))
        {
            return Errors.General.Forbidden;
        }

        var timeZone = TimeZoneHelper.ResolveTimeZone((await _settings.GetSettingsAsync(cancellationToken)).TimeZoneId);
        var (from, to) = request.From <= request.To ? (request.From, request.To) : (request.To, request.From);
        var fromUtc = LocalMidnightUtc(from, timeZone);
        var toUtc = LocalMidnightUtc(to.AddDays(1), timeZone);

        var card = DigitNormalizer.NormalizeDigits(request.CardNumber).Trim();
        var fragment = card.Length == 0 ? null : $"\"CardNumber\":{JsonSerializer.Serialize(card)}";

        await using var uow = _uowFactory.Create();
        var logs = await uow.AuditLogs.SearchAsync(fromUtc, toUtc, AuditActions.Of(request.Category), fragment, MaxRows + 1, cancellationToken);
        var users = (await uow.Users.GetAllAsync(cancellationToken)).ToDictionary(u => u.Id, u => u.FullName);
        var desks = (await uow.Workstations.GetAllAsync(cancellationToken)).ToDictionary(w => w.Id, w => w.DisplayName);

        var rows = logs.Take(MaxRows).Select(log => ToDto(log, users, desks)).ToList();
        return new AuditLogPage(rows, logs.Count > MaxRows);
    }

    private static DateTime LocalMidnightUtc(DateOnly day, TimeZoneInfo timeZone) =>
        TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), timeZone);

    private static AuditEntryDto ToDto(Domain.Entities.AuditLog log, Dictionary<int, string> users, Dictionary<int, string> desks)
    {
        var details = ParseDetails(log.DetailsJson);
        return new AuditEntryDto(
            log.Id,
            log.AtUtc,
            log.Action,
            Name(users, log.UserId),
            log.WorkstationId is { } desk && desks.TryGetValue(desk, out var deskName) ? deskName : null,
            log.EntityName,
            log.EntityId,
            Text(details, "CardNumber"),
            Text(details, "HolderName"),
            Text(details, "Reason"),
            Name(users, Number(details, "OperatorUserId")),
            Name(users, Number(details, "ApprovedByUserId")),
            log.DetailsJson);
    }

    private static string? Name(Dictionary<int, string> users, int? id) =>
        id is { } value && users.TryGetValue(value, out var name) ? name : null;

    private static JsonElement? ParseDetails(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object ? document.RootElement.Clone() : null;
        }
        catch (JsonException)
        {
            // Older rows may hold hand-built text; the screen still shows it raw.
            return null;
        }
    }

    private static string? Text(JsonElement? details, string name) =>
        details is { } d && d.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static int? Number(JsonElement? details, string name) =>
        details is { } d && d.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) ? number : null;
}
