using FluentValidation;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Audit;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;

namespace ParkGate.Application.CheckIn.Services;

public sealed class CheckInService : ICheckInService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IEntryEligibilityPolicy _eligibilityPolicy;
    private readonly IAuditWriter _auditWriter;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;
    private readonly IValidator<RecordEntryRequest> _recordValidator;
    private readonly IValidator<OverrideEntryRequest> _overrideValidator;
    private readonly IValidator<VoidEntryRequest> _voidValidator;

    public CheckInService(
        IUnitOfWorkFactory uowFactory,
        IEntryEligibilityPolicy eligibilityPolicy,
        IAuditWriter auditWriter,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        IValidator<RecordEntryRequest> recordValidator,
        IValidator<OverrideEntryRequest> overrideValidator,
        IValidator<VoidEntryRequest> voidValidator)
    {
        _uowFactory = uowFactory;
        _eligibilityPolicy = eligibilityPolicy;
        _auditWriter = auditWriter;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
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

        var validation = await _recordValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

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
        var (defaultCooldown, maxGroupSize, denialDedupMinutes, timeZoneId) = await GetSettingsAsync(uow, cancellationToken);
        var businessDate = TimeZoneHelper.GetLocalBusinessDate(nowUtc, timeZoneId);

        var companionList = request.Companions ?? [];
        var companionSum = companionList.Sum(c => c.Count);
        var totalPersons = 1 + companionSum;

        if (totalPersons > maxGroupSize)
        {
            return Errors.CheckIn.ExceedsMaxGroupSize;
        }

        var context = new EligibilityContext(org, holder, defaultCooldown, nowUtc, businessDate);
        var decision = _eligibilityPolicy.Evaluate(context);

        var workstationId = _currentWorkstation.WorkstationId ?? 1;
        var userId = _currentUser.UserId ?? 1;

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

        await uow.SaveChangesAsync(cancellationToken);
        await uow.CommitAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "EntryGranted",
            userId: userId,
            workstationId: workstationId,
            entityName: nameof(EntryAttempt),
            entityId: attemptId.ToString(),
            detailsJson: $"{{\"TotalPersons\":{attempt.TotalPersons}}}",
            cancellationToken: cancellationToken);

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
        var (defaultCooldown, maxGroupSize, _, timeZoneId) = await GetSettingsAsync(uow, cancellationToken);
        var businessDate = TimeZoneHelper.GetLocalBusinessDate(nowUtc, timeZoneId);

        var companionList = request.Companions ?? [];
        var companionSum = companionList.Sum(c => c.Count);
        var totalPersons = 1 + companionSum;

        if (totalPersons > maxGroupSize)
        {
            return Errors.CheckIn.ExceedsMaxGroupSize;
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

        var workstationId = _currentWorkstation.WorkstationId ?? 1;
        var userId = _currentUser.UserId ?? approverUserId;

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

        await uow.SaveChangesAsync(cancellationToken);
        await uow.CommitAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "EntryOverridden",
            userId: approverUserId,
            workstationId: workstationId,
            entityName: nameof(EntryAttempt),
            entityId: attemptId.ToString(),
            detailsJson: $"{{\"Reason\":\"{request.Reason}\",\"TotalPersons\":{attempt.TotalPersons}}}",
            cancellationToken: cancellationToken);

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
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.VoidEntry))
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
        var voidedByUserId = _currentUser.UserId!.Value;

        attempt.Void(voidedByUserId, request.Reason, nowUtc);
        await uow.SaveChangesAsync(cancellationToken);

        var latestRemaining = await uow.Beneficiaries.GetLatestGrantedEntryUtcAsync(holder.Id, cancellationToken);
        holder.RecalculateLastGrantedEntry(latestRemaining);

        await uow.SaveChangesAsync(cancellationToken);
        await uow.CommitAsync(cancellationToken);

        await _auditWriter.WriteAsync(
            action: "EntryVoided",
            userId: voidedByUserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(EntryAttempt),
            entityId: attempt.Id.ToString(),
            detailsJson: $"{{\"Reason\":\"{request.Reason}\"}}",
            cancellationToken: cancellationToken);

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
        var (_, _, _, timeZoneId) = await GetSettingsAsync(uow, cancellationToken);
        var businessDate = TimeZoneHelper.GetLocalBusinessDate(nowUtc, timeZoneId);

        var targetWorkstationId = workstationId ?? _currentWorkstation.WorkstationId ?? 1;
        var workstation = await uow.Workstations.GetByIdAsync(targetWorkstationId, cancellationToken);
        var workstationName = workstation?.DisplayName ?? _currentWorkstation.DisplayName ?? "Desk";

        var attempts = await uow.EntryAttempts.GetTodayByWorkstationIdAsync(targetWorkstationId, businessDate, 50, cancellationToken);
        var activeCategories = await uow.AgeCategories.GetAllActiveAsync(cancellationToken);

        var totalGranted = attempts.Count(a => a.Result == EntryResult.Allowed && !a.IsVoided);
        var totalDenied = attempts.Count(a => a.Result == EntryResult.Denied);
        var totalOverridden = attempts.Count(a => a.Result == EntryResult.Overridden && !a.IsVoided);
        var totalPersons = attempts.Where(a => !a.IsVoided && (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden)).Sum(a => a.TotalPersons);

        var categoryTotals = new List<CompanionCountDto>();
        foreach (var cat in activeCategories)
        {
            var countForCat = attempts
                .Where(a => !a.IsVoided && (a.Result == EntryResult.Allowed || a.Result == EntryResult.Overridden))
                .SelectMany(a => a.Companions)
                .Where(c => c.AgeCategoryId == cat.Id)
                .Sum(c => c.Count);

            categoryTotals.Add(new CompanionCountDto(cat.Id, cat.NameAr, countForCat));
        }

        var attemptBeneficiaryIds = attempts.Take(20).Select(a => a.BeneficiaryId).Distinct().ToList();
        var beneficiariesMap = new Dictionary<Guid, (string CardNumber, string FullName)>();
        foreach (var bId in attemptBeneficiaryIds)
        {
            var b = await uow.Beneficiaries.GetByIdAsync(bId, cancellationToken);
            if (b != null)
            {
                beneficiariesMap[bId] = (b.CardNumber.Value, b.FullName);
            }
        }

        var allOrgs = await uow.Organizations.GetAllAsync(false, cancellationToken);
        var orgMap = allOrgs.ToDictionary(o => o.Id, o => o.NameAr);

        var recentAttemptDtos = attempts.Take(20).Select(a =>
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
            TotalGranted: totalGranted,
            TotalDenied: totalDenied,
            TotalOverridden: totalOverridden,
            TotalPersons: totalPersons,
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

    private static async Task<(int DefaultCooldownMinutes, int MaxGroupSize, int DenialDedupMinutes, string TimeZoneId)> GetSettingsAsync(
        IUnitOfWork uow,
        CancellationToken cancellationToken)
    {
        int defaultCooldown = 720;
        var cdSetting = await uow.AppSettings.GetByKeyAsync("Entry.DefaultCooldownMinutes", cancellationToken);
        if (cdSetting is not null && int.TryParse(cdSetting.Value, out var parsedCd))
        {
            defaultCooldown = parsedCd;
        }

        int maxGroupSize = 10;
        var groupSetting = await uow.AppSettings.GetByKeyAsync("Entry.MaxGroupSize", cancellationToken);
        if (groupSetting is not null && int.TryParse(groupSetting.Value, out var parsedGroup))
        {
            maxGroupSize = parsedGroup;
        }

        int denialDedup = 5;
        var dedupSetting = await uow.AppSettings.GetByKeyAsync("Entry.DenialDedupMinutes", cancellationToken);
        if (dedupSetting is not null && int.TryParse(dedupSetting.Value, out var parsedDedup))
        {
            denialDedup = parsedDedup;
        }

        string timeZoneId = "Libya Standard Time";
        var tzSetting = await uow.AppSettings.GetByKeyAsync("Park.TimeZoneId", cancellationToken);
        if (tzSetting is not null && !string.IsNullOrWhiteSpace(tzSetting.Value))
        {
            timeZoneId = tzSetting.Value;
        }

        return (defaultCooldown, maxGroupSize, denialDedup, timeZoneId);
    }
}
