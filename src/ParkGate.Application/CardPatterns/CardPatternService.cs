using ParkGate.Application.Common;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Domain.CardPatterns;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.CardPatterns;

public sealed record CardPatternDto(
    int Id,
    int OrganizationId,
    string OrganizationName,
    string Shape,
    string? Prefix,
    string Display,
    CardPatternStatus Status,
    CardPatternSource Source,
    bool IsActive,
    int SampleCount,
    DateTime ProposedAtUtc,
    DateTime? DecidedAtUtc,
    string? DecidedByName);

public sealed record CardPatternAlertDto(
    long Id,
    int OrganizationId,
    string OrganizationName,
    Guid BeneficiaryId,
    string HolderName,
    string CardNumber,
    string Shape,
    DateTime CreatedAtUtc,
    string? CreatedByName,
    string? WorkstationName,
    CardPatternAlertStatus Status,
    DateTime? ReviewedAtUtc,
    string? ReviewedByName,
    string? ReviewNote);

/// <summary>
/// Admin use cases for the card-number patterns (F8, BR-29): approve or reject a proposed pattern and review alerts.
/// Only an Admin decides (<see cref="Permission.ManageCardPatterns"/>); nothing is ever approved automatically.
/// </summary>
public interface ICardPatternService
{
    Task<Result<IReadOnlyList<CardPatternDto>>> GetPatternsAsync(CancellationToken ct = default);

    /// <summary>Approves a proposal; the open alerts it now explains are closed as reviewed.</summary>
    Task<Result<int>> ApprovePatternAsync(int patternId, CancellationToken ct = default);

    /// <summary>Rejects a proposal; numbers like it keep raising alerts.</summary>
    Task<Result> RejectPatternAsync(int patternId, CancellationToken ct = default);

    Task<Result<IReadOnlyList<CardPatternAlertDto>>> GetAlertsAsync(bool openOnly, CancellationToken ct = default);

    Task<Result> ReviewAlertAsync(long alertId, string? note, CancellationToken ct = default);
}

public sealed class CardPatternService : ICardPatternService
{
    public const int MaxAlertRows = 1000;

    public const string PatternApproved = "CardPattern.Approved";
    public const string PatternRejected = "CardPattern.Rejected";
    public const string AlertReviewed = "CardPatternAlert.Reviewed";

    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;

    public CardPatternService(IUnitOfWorkFactory uowFactory, ICurrentUser currentUser, ICurrentWorkstation currentWorkstation)
    {
        _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _currentWorkstation = currentWorkstation ?? throw new ArgumentNullException(nameof(currentWorkstation));
    }

    public async Task<Result<IReadOnlyList<CardPatternDto>>> GetPatternsAsync(CancellationToken ct = default)
    {
        if (!_currentUser.Has(Permission.ManageCardPatterns))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var patterns = await uow.CardPatterns.ListAsync(ct);
        var names = await NamesAsync(uow, ct);

        IReadOnlyList<CardPatternDto> rows = patterns
            .Select(p => new CardPatternDto(
                p.Id,
                p.OrganizationId,
                names.Organizations.GetValueOrDefault(p.OrganizationId, string.Empty),
                p.Shape,
                p.Prefix,
                CardShape.Describe(p.Shape, p.Prefix),
                p.Status,
                p.Source,
                p.IsActive,
                p.SampleCount,
                p.ProposedAtUtc,
                p.DecidedAtUtc,
                p.DecidedByUserId is { } u ? names.Users.GetValueOrDefault(u) : null))
            .ToList();
        return Result.Success(rows);
    }

    public async Task<Result<int>> ApprovePatternAsync(int patternId, CancellationToken ct = default)
    {
        if (DecisionActor() is not { } admin)
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var pattern = await uow.CardPatterns.GetByIdAsync(patternId, ct);
        if (pattern is null)
        {
            return Errors.CardPattern.NotFound;
        }

        if (pattern.Status != CardPatternStatus.Proposed)
        {
            return Errors.CardPattern.AlreadyDecided;
        }

        var nowUtc = await uow.GetDbUtcNowAsync(ct);
        pattern.Approve(admin, nowUtc);

        var explained = (await uow.CardPatternAlerts.GetOpenForUpdateAsync(pattern.OrganizationId, pattern.Shape, ct))
            .Where(a => pattern.Matches(a.CardNumber))
            .ToList();
        foreach (var alert in explained)
        {
            alert.Review(admin, nowUtc, note: null);
        }

        await uow.AddAuditAsync(
            action: PatternApproved,
            userId: admin,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(OrganizationCardPattern),
            entityId: pattern.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            detailsJson: AuditDetails.ToJson(new
            {
                pattern.OrganizationId,
                pattern.Shape,
                pattern.Prefix,
                Source = pattern.Source.ToString(),
                pattern.SampleCount,
                ClosedAlerts = explained.Count,
            }),
            cancellationToken: ct);

        await uow.SaveChangesAsync(ct);
        return explained.Count;
    }

    public async Task<Result> RejectPatternAsync(int patternId, CancellationToken ct = default)
    {
        if (DecisionActor() is not { } admin)
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var pattern = await uow.CardPatterns.GetByIdAsync(patternId, ct);
        if (pattern is null)
        {
            return Errors.CardPattern.NotFound;
        }

        if (pattern.Status != CardPatternStatus.Proposed)
        {
            return Errors.CardPattern.AlreadyDecided;
        }

        pattern.Reject(admin, await uow.GetDbUtcNowAsync(ct));
        await uow.AddAuditAsync(
            action: PatternRejected,
            userId: admin,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(OrganizationCardPattern),
            entityId: pattern.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            detailsJson: AuditDetails.ToJson(new
            {
                pattern.OrganizationId,
                pattern.Shape,
                pattern.Prefix,
                Source = pattern.Source.ToString(),
                pattern.SampleCount,
            }),
            cancellationToken: ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<CardPatternAlertDto>>> GetAlertsAsync(bool openOnly, CancellationToken ct = default)
    {
        if (!_currentUser.Has(Permission.ManageCardPatterns))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var alerts = await uow.CardPatternAlerts.ListAsync(openOnly, MaxAlertRows, ct);
        var names = await NamesAsync(uow, ct);
        var holders = (await uow.Beneficiaries.GetByIdsAsync(alerts.Select(a => a.BeneficiaryId).Distinct(), ct))
            .ToDictionary(b => b.Id, b => b.FullName);

        IReadOnlyList<CardPatternAlertDto> rows = alerts
            .Select(a => new CardPatternAlertDto(
                a.Id,
                a.OrganizationId,
                names.Organizations.GetValueOrDefault(a.OrganizationId, string.Empty),
                a.BeneficiaryId,
                holders.GetValueOrDefault(a.BeneficiaryId, string.Empty),
                a.CardNumber,
                a.Shape,
                a.CreatedAtUtc,
                names.Users.GetValueOrDefault(a.CreatedByUserId),
                names.Workstations.GetValueOrDefault(a.WorkstationId),
                a.Status,
                a.ReviewedAtUtc,
                a.ReviewedByUserId is { } r ? names.Users.GetValueOrDefault(r) : null,
                a.ReviewNote))
            .ToList();
        return Result.Success(rows);
    }

    public async Task<Result> ReviewAlertAsync(long alertId, string? note, CancellationToken ct = default)
    {
        if (DecisionActor() is not { } admin)
        {
            return Errors.General.Forbidden;
        }

        if (note is not null && note.Trim().Length > CardPatternAlert.MaxReviewNoteLength)
        {
            return Errors.CardPattern.NoteTooLong;
        }

        await using var uow = _uowFactory.Create();
        var alert = await uow.CardPatternAlerts.GetByIdAsync(alertId, ct);
        if (alert is null)
        {
            return Errors.CardPattern.AlertNotFound;
        }

        if (alert.Status != CardPatternAlertStatus.Open)
        {
            return Errors.CardPattern.AlertAlreadyReviewed;
        }

        alert.Review(admin, await uow.GetDbUtcNowAsync(ct), note);
        await uow.AddAuditAsync(
            action: AlertReviewed,
            userId: admin,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(Beneficiary),
            entityId: alert.BeneficiaryId.ToString(),
            detailsJson: AuditDetails.ToJson(new
            {
                alert.BeneficiaryId,
                alert.CardNumber,
                alert.OrganizationId,
                alert.Shape,
                Reason = alert.ReviewNote,
            }),
            cancellationToken: ct);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    private int? DecisionActor() =>
        _currentUser.Has(Permission.ManageCardPatterns) ? _currentUser.UserId : null;

    private static async Task<(Dictionary<int, string> Organizations, Dictionary<int, string> Users, Dictionary<int, string> Workstations)> NamesAsync(IUnitOfWork uow, CancellationToken ct) =>
        ((await uow.Organizations.GetAllAsync(false, ct)).ToDictionary(o => o.Id, o => o.NameAr),
         (await uow.Users.GetAllAsync(ct)).ToDictionary(u => u.Id, u => u.FullName),
         (await uow.Workstations.GetAllAsync(ct)).ToDictionary(w => w.Id, w => w.DisplayName));
}
