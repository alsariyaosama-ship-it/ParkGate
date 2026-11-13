using FluentValidation;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Services;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;

namespace ParkGate.Application.CheckIn.Services;

public sealed class CheckInService : ICheckInService
{
    /// <summary>Rows the desk "today" grid shows; totals are not limited by it.</summary>
    private const int RecentAttemptsShown = 20;

    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IEntryEligibilityPolicy _eligibilityPolicy;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;
    private readonly ISettingsService _settings;
    private readonly IValidator<RecordEntryRequest> _recordValidator;
    private readonly IValidator<OverrideEntryRequest> _overrideValidator;
    private readonly IValidator<VoidEntryRequest> _voidValidator;

    public CheckInService(
        IUnitOfWorkFactory uowFactory,
        IEntryEligibilityPolicy eligibilityPolicy,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        ISettingsService settings,
        IValidator<RecordEntryRequest> recordValidator,
        IValidator<OverrideEntryRequest> overrideValidator,
        IValidator<VoidEntryRequest> voidValidator)
    {
        _uowFactory = uowFactory;
        _eligibilityPolicy = eligibilityPolicy;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
        _settings = settings;
        _recordValidator = recordValidator;
        _overrideValidator = overrideValidator;
        _voidValidator = voidValidator;
    }

    public async Task<Result<CheckInResultDto>> RecordEntryAsync(
        RecordEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.CheckIn))
        {
            return Errors.General.Forbidden;
        }

        var actor = Actor.Resolve(_currentUser, _currentWorkstation);
        if (actor.IsFailure)
        {
            return actor.Error;
        }

        var validation = await _recordValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        // Cached settings, read before the row lock is taken (R3.6).
        var settings = await _settings.GetSettingsAsync(cancellationToken);

        await using var uow = _uowFactory.Create();
        await uow.BeginTransactionAsync(cancellationToken);

        var holder = await uow.Beneficiaries.GetForUpdateAsync(request.BeneficiaryId, cancellationToken);
        if (holder is null)
        {
            return Errors.Beneficiary.NotFound;
        }

        var org = await uow.Organizations.GetByIdAsync(holder.OrganizationId, cancellationToken);
        if (org is null)
        {
            return Errors.Organization.NotFound;
        }

        var nowUtc = await uow.GetDbUtcNowAsync(cancellationToken);
        var (defaultCooldown, maxGroupSize, denialDedupMinutes, timeZoneId) =
            (settings.DefaultCooldownMinutes, settings.MaxGroupSize, settings.DenialDedupMinutes, settings.TimeZoneId);
        var businessDate = TimeZoneHelper.GetLocalBusinessDate(nowUtc, timeZoneId);

        var companionList = request.Companions ?? [];
        var companionSum = companionList.Sum(c => c.Count);
        var totalPersons = 1 + companionSum;

        if (totalPersons > maxGroupSize)
        {
            return Errors.CheckIn.ExceedsMaxGroupSize;
        }

        if (await ExceedsFreeCompanionLimitAsync(uow, org, companionList, cancellationToken))
        {
            return Errors.CheckIn.ExceedsFreeCompanionLimit;
        }

        var context = new EligibilityContext(org, holder, defaultCooldown, nowUtc, businessDate);
        var decision = _eligibilityPolicy.Evaluate(context);

        var (userId, workstationId) = actor.Value;

        if (!decision.IsEligible)
        {
            var latestDenial = await uow.EntryAttempts.GetLatestDenialAsync(holder.Id, workstationId, cancellationToken);
            var isDuplicate = latestDenial is not null &&
                              (nowUtc - latestDenial.AttemptedAtUtc).TotalMinutes < denialDedupMinutes;

            if (!isDuplicate)
            {
                var deniedAttempt = EntryAttempt.CreateDenied(
                    Guid.CreateVersion7(),
                    holder.Id,
                    org.Id,
                    nowUtc,
                    businessDate,
                    decision.DenialReason!.Value,
                    decision.EffectiveCooldownMinutes,
                    decision.NextEligibleAtUtc,
                    workstationId,
                    userId,
                    request.Channel);

                uow.EntryAttempts.Add(deniedAttempt);
                await uow.SaveChangesAsync(cancellationToken);
            }

            await uow.CommitAsync(cancellationToken);

            var error = MapDenialReasonToError(decision.DenialReason!.Value);
            return error;
        }

        var attemptId = Guid.CreateVersion7();
        var nextEligibleAtUtc = nowUtc.AddMinutes(decision.EffectiveCooldownMinutes);
        var domainCompanions = companionList
            .Where(c => c.Count > 0)
            .Select(c => new EntryAttemptCompanion(c.AgeCategoryId, c.Count))
            .ToList();

        var attempt = EntryAttempt.CreateAllowed(
            attemptId,
            holder.Id,
            org.Id,
            nowUtc,
            businessDate,
            decision.EffectiveCooldownMinutes,
            nextEligibleAtUtc,
            workstationId,
            userId,
            request.Channel,
            domainCompanions);

        holder.RecordGrantedEntry(nowUtc);
        uow.EntryAttempts.Add(attempt);

        // Same transaction as the change it records: a crash cannot keep one without the other (R3.5).
        uow.AuditLogs.Add(new AuditLog(nowUtc, "EntryGranted", userId, workstationId, nameof(EntryAttempt), attemptId.ToString(), AuditDetails.ToJson(new { attempt.TotalPersons })));
        await uow.SaveChangesAsync(cancellationToken);
        await uow.CommitAsync(cancellationToken);

        return new CheckInResultDto(
            AttemptId: attempt.Id,
            Result: EntryResult.Allowed,
            DenialReason: null,
            DenialReasonText: null,
            AttemptedAtUtc: attempt.AttemptedAtUtc,
            BusinessDate: attempt.BusinessDate,
            TotalPersons: attempt.TotalPersons,
            EffectiveCooldownMinutes: attempt.CooldownMinutesApplied,
            NextEligibleAtUtc: attempt.NextEligibleAtUtc,
            CanBeOverridden: false,
            Companions: companionList.Select(c => new CompanionCountDto(c.AgeCategoryId, string.Empty, c.Count)).ToList());
    }

    public async Task<Result<CheckInResultDto>> OverrideEntryAsync(
        OverrideEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var actor = Actor.Resolve(_currentUser, _currentWorkstation);
        if (actor.IsFailure)
        {
            return actor.Error;
        }

        int approverUserId;
        if (_currentUser.Has(Permission.OverrideDenial))
        {
            approverUserId = _currentUser.UserId!.Value;
        }
        else if (request.SupervisorUserId.HasValue)
        {
            approverUserId = request.SupervisorUserId.Value;
        }
        else
        {
            return Errors.Auth.SupervisorRequired;
        }

        var validation = await _overrideValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        // Cached settings, read before the row lock is taken (R3.6).
        var settings = await _settings.GetSettingsAsync(cancellationToken);

        await using var uow = _uowFactory.Create();
        await uow.BeginTransactionAsync(cancellationToken);

        var approver = await uow.Users.GetByIdAsync(approverUserId, cancellationToken);
        if (approver is null || !approver.IsActive || (approver.Role != UserRole.Supervisor && approver.Role != UserRole.Admin))
        {
            return Errors.Auth.SupervisorRequired;
        }

        var holder = await uow.Beneficiaries.GetForUpdateAsync(request.BeneficiaryId, cancellationToken);
        if (holder is null)
        {
            return Errors.Beneficiary.NotFound;
        }

        var org = await uow.Organizations.GetByIdAsync(holder.OrganizationId, cancellationToken);
        if (org is null)
        {
            return Errors.Organization.NotFound;
        }

        var nowUtc = await uow.GetDbUtcNowAsync(cancellationToken);
        var (defaultCooldown, maxGroupSize, timeZoneId) = (settings.DefaultCooldownMinutes, settings.MaxGroupSize, settings.TimeZoneId);
        var businessDate = TimeZoneHelper.GetLocalBusinessDate(nowUtc, timeZoneId);

        var companionList = request.Companions ?? [];
        var companionSum = companionList.Sum(c => c.Count);
        var totalPersons = 1 + companionSum;

        if (totalPersons > maxGroupSize)
        {
            return Errors.CheckIn.ExceedsMaxGroupSize;
        }

        if (await ExceedsFreeCompanionLimitAsync(uow, org, companionList, cancellationToken))
        {
            return Errors.CheckIn.ExceedsFreeCompanionLimit;
        }

        var context = new EligibilityContext(org, holder, defaultCooldown, nowUtc, businessDate);
        var decision = _eligibilityPolicy.Evaluate(context);

        if (!decision.CanBeOverridden && !decision.IsEligible)
        {
            return Errors.CheckIn.OverrideNotAllowed;
        }

        var attemptId = Guid.CreateVersion7();
        var nextEligibleAtUtc = nowUtc.AddMinutes(decision.EffectiveCooldownMinutes);
        var domainCompanions = companionList
            .Where(c => c.Count > 0)
            .Select(c => new EntryAttemptCompanion(c.AgeCategoryId, c.Count))
            .ToList();

        var (userId, workstationId) = actor.Value;

        var attempt = EntryAttempt.CreateOverridden(
            attemptId,
            holder.Id,
            org.Id,
            nowUtc,
            businessDate,
            decision.EffectiveCooldownMinutes,
            nextEligibleAtUtc,
            workstationId,
            userId,
            approverUserId,
            request.Reason,
            request.Channel,
            domainCompanions);

        holder.RecordGrantedEntry(nowUtc);
        uow.EntryAttempts.Add(attempt);

        // Same transaction as the change it records: a crash cannot keep one without the other (R3.5).
        uow.AuditLogs.Add(new AuditLog(nowUtc, "EntryOverridden", approverUserId, workstationId, nameof(EntryAttempt), attemptId.ToString(), AuditDetails.ToJson(new { request.Reason, attempt.TotalPersons })));
        await uow.SaveChangesAsync(cancellationToken);
        await uow.CommitAsync(cancellationToken);

        return new CheckInResultDto(
            AttemptId: attempt.Id,
            Result: EntryResult.Overridden,
            DenialReason: null,
            DenialReasonText: null,
            AttemptedAtUtc: attempt.AttemptedAtUtc,
            BusinessDate: attempt.BusinessDate,
            TotalPersons: attempt.TotalPersons,
            EffectiveCooldownMinutes: attempt.CooldownMinutesApplied,
            NextEligibleAtUtc: attempt.NextEligibleAtUtc,
            CanBeOverridden: false,
            Companions: companionList.Select(c => new CompanionCountDto(c.AgeCategoryId, string.Empty, c.Count)).ToList());
    }

    public async Task<Result> VoidEntryAsync(
        VoidEntryRequest request,
        int? supervisorUserId = null,
        CancellationToken cancellationToken = default)
    {
        var actor = Actor.Resolve(_currentUser, _currentWorkstation);
        if (actor.IsFailure)
        {
            return actor.Error;
        }

        var voidedByUserId = supervisorUserId ?? actor.Value.UserId;

        if (supervisorUserId.HasValue)
        {
            await using var uowCheck = _uowFactory.Create();
            var approver = await uowCheck.Users.GetByIdAsync(supervisorUserId.Value, cancellationToken);
            if (approver is null || !approver.IsActive || (approver.Role != UserRole.Supervisor && approver.Role != UserRole.Admin))
            {
                return Errors.Auth.SupervisorRequired;
            }
            voidedByUserId = supervisorUserId.Value;
        }
        else if (!_currentUser.Has(Permission.VoidEntry))
        {
            return Errors.General.Forbidden;
        }

        var validation = await _voidValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        await using var uow = _uowFactory.Create();
        await uow.BeginTransactionAsync(cancellationToken);

        var attempt = await uow.EntryAttempts.GetByIdAsync(request.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Errors.CheckIn.AttemptNotFound;
        }

        if (attempt.Result == EntryResult.Denied)
        {
            return Errors.CheckIn.CannotVoidDeniedAttempt;
        }

        if (attempt.IsVoided)
        {
            return Errors.CheckIn.AlreadyVoided;
        }

        var holder = await uow.Beneficiaries.GetForUpdateAsync(attempt.BeneficiaryId, cancellationToken);
        if (holder is null)
        {
            return Errors.Beneficiary.NotFound;
        }

        var nowUtc = await uow.GetDbUtcNowAsync(cancellationToken);

        attempt.Void(voidedByUserId, request.Reason, nowUtc);
        await uow.SaveChangesAsync(cancellationToken);

        var latestRemaining = await uow.Beneficiaries.GetLatestGrantedEntryUtcAsync(holder.Id, cancellationToken);
        holder.RecalculateLastGrantedEntry(latestRemaining);

        // Same transaction as the change it records: a crash cannot keep one without the other (R3.5).
        uow.AuditLogs.Add(new AuditLog(nowUtc, "EntryVoided", voidedByUserId, actor.Value.WorkstationId, nameof(EntryAttempt), attempt.Id.ToString(), AuditDetails.ToJson(new { request.Reason })));
        await uow.SaveChangesAsync(cancellationToken);
        await uow.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<TodayDeskSummaryDto>> GetTodayDeskEntriesAsync(
        int? workstationId = null,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ViewTodayDeskEntries))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var nowUtc = await uow.GetDbUtcNowAsync(cancellationToken);
        var timeZoneId = (await _settings.GetSettingsAsync(cancellationToken)).TimeZoneId;
        var businessDate = TimeZoneHelper.GetLocalBusinessDate(nowUtc, timeZoneId);

        if ((workstationId ?? _currentWorkstation.WorkstationId) is not { } targetWorkstationId)
        {
            return Errors.Auth.WorkstationNotFound;
        }

        var workstation = await uow.Workstations.GetByIdAsync(targetWorkstationId, cancellationToken);
        var workstationName = workstation?.DisplayName ?? _currentWorkstation.DisplayName ?? "Desk";

        // Totals come from an aggregate over the whole day; the list feeds the grid only (R3.1).
        var totals = await uow.EntryAttempts.GetTodayTotalsAsync(targetWorkstationId, businessDate, cancellationToken);
        var attempts = await uow.EntryAttempts.GetTodayByWorkstationIdAsync(targetWorkstationId, businessDate, RecentAttemptsShown, cancellationToken);
        var activeCategories = await uow.AgeCategories.GetAllActiveAsync(cancellationToken);

        var categoryTotals = activeCategories
            .Select(cat => new CompanionCountDto(cat.Id, cat.NameAr, totals.CompanionsByCategory.GetValueOrDefault(cat.Id)))
            .ToList();

        var attemptBeneficiaryIds = attempts.Select(a => a.BeneficiaryId).Distinct().ToList();
        var beneficiaries = await uow.Beneficiaries.GetByIdsAsync(attemptBeneficiaryIds, cancellationToken);
        var beneficiariesMap = beneficiaries.ToDictionary(b => b.Id, b => (CardNumber: b.CardNumber.Value, FullName: b.FullName));

        var allOrgs = await uow.Organizations.GetAllAsync(false, cancellationToken);
        var orgMap = allOrgs.ToDictionary(o => o.Id, o => o.NameAr);

        var recentAttemptDtos = attempts.Select(a =>
        {
            beneficiariesMap.TryGetValue(a.BeneficiaryId, out var bInfo);
            orgMap.TryGetValue(a.OrganizationId, out var orgName);

            return new TodayDeskAttemptDto(
                a.Id,
                a.AttemptedAtUtc,
                bInfo.CardNumber ?? string.Empty,
                bInfo.FullName ?? string.Empty,
                orgName ?? string.Empty,
                a.TotalPersons,
                a.Result,
                a.DenialReason,
                a.IsVoided);
        }).ToList();

        return new TodayDeskSummaryDto(
            BusinessDate: businessDate,
            WorkstationId: targetWorkstationId,
            WorkstationName: workstationName,
            TotalGranted: totals.Granted,
            TotalDenied: totals.Denied,
            TotalOverridden: totals.Overridden,
            TotalPersons: totals.Persons,
            PersonsPerCategory: categoryTotals,
            RecentAttempts: recentAttemptDtos);

    }

    private static Error MapDenialReasonToError(DenialReason reason) => reason switch
    {
        DenialReason.CooldownActive => Errors.CheckIn.CooldownActive,
        DenialReason.OrganizationInactive => Errors.CheckIn.OrganizationInactive,
        DenialReason.HolderBlocked => Errors.Beneficiary.Blocked,
        DenialReason.HolderSuspended => Errors.Beneficiary.Suspended,
        DenialReason.CardExpired => Errors.Beneficiary.CardExpired,
        _ => Errors.CheckIn.CooldownActive
    };

    private static async Task<bool> ExceedsFreeCompanionLimitAsync(
        IUnitOfWork uow,
        Organization org,
        IReadOnlyList<CompanionRequestDto> companions,
        CancellationToken ct)
    {
        if (org.MaxFreeCompanions is null)
        {
            return false;
        }

        var allCategories = await uow.AgeCategories.GetAllAsync(ct);
        var exemptIds = allCategories.Where(c => c.IsCompanionLimitExempt).Select(c => c.Id).ToHashSet();

        var counted = companions
            .Where(c => !exemptIds.Contains(c.AgeCategoryId))
            .Sum(c => c.Count);

        return counted > org.MaxFreeCompanions.Value;
    }

}
