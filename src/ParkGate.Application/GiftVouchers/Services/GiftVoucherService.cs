using FluentValidation;
using ParkGate.Application.Common;
using ParkGate.Application.EntryCards;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Domain.Entities;
using ParkGate.Domain.EntryCards;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;

namespace ParkGate.Application.GiftVouchers.Services;

public sealed class GiftVoucherService : IGiftVoucherService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IGiftEligibilityPolicy _policy;
    private readonly QrTokenSigner _signer;
    private readonly QrTokenVerifier _verifier;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;
    private readonly SupervisorApprovalStore _approvals;
    private readonly IValidator<IssueGiftOverrideRequest> _overrideValidator;
    private readonly IValidator<VoidGiftVoucherRequest> _voidValidator;

    public GiftVoucherService(
        IUnitOfWorkFactory uowFactory,
        IGiftEligibilityPolicy policy,
        QrTokenSigner signer,
        QrTokenVerifier verifier,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        SupervisorApprovalStore approvals,
        IValidator<IssueGiftOverrideRequest> overrideValidator,
        IValidator<VoidGiftVoucherRequest> voidValidator)
    {
        _uowFactory = uowFactory;
        _policy = policy;
        _signer = signer;
        _verifier = verifier;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
        _approvals = approvals;
        _overrideValidator = overrideValidator;
        _voidValidator = voidValidator;
    }

    public async Task<Result<GiftEligibilityDto>> GetEligibilityAsync(
        Guid beneficiaryId,
        CancellationToken ct = default)
    {
        if (!_currentUser.Has(Permission.IssueGift))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();

        var holder = await uow.Beneficiaries.GetByIdAsync(beneficiaryId, ct);
        if (holder is null)
        {
            return Errors.Beneficiary.NotFound;
        }

        var org = await uow.Organizations.GetByIdAsync(holder.OrganizationId, ct);
        if (org is null)
        {
            return Errors.Organization.NotFound;
        }

        var lastIssuedUtc = await uow.GiftVouchers.GetLatestIssuedUtcAsync(beneficiaryId, ct);
        var nowUtc = await uow.GetDbUtcNowAsync(ct);
        var businessDate = TimeZoneHelper.GetLocalBusinessDate(nowUtc, await GetTimeZoneIdAsync(uow, ct));
        var enteredToday = await uow.EntryAttempts.HasGrantedEntryOnAsync(holder.Id, businessDate, ct);

        var context = new GiftEligibilityContext(org, holder, lastIssuedUtc, nowUtc, enteredToday, businessDate);
        var decision = _policy.Evaluate(context);

        return MapToEligibilityDto(decision, org.GiftValue, lastIssuedUtc);
    }

    public async Task<Result<IssuedGiftVoucherDto>> IssueAsync(
        IssueGiftVoucherRequest request,
        CancellationToken ct = default)
    {
        if (!_currentUser.Has(Permission.IssueGift))
        {
            return Errors.General.Forbidden;
        }

        var actor = Actor.Resolve(_currentUser, _currentWorkstation);
        if (actor.IsFailure)
        {
            return actor.Error;
        }

        return await ConcurrencyRetry.RunAsync(retryCt => IssueOnceAsync(request, actor.Value, retryCt), ct);
    }

    /// <summary>One attempt on a fresh unit of work; <see cref="ConcurrencyRetry"/> repeats it after a lost race (ADR-026).</summary>
    private async Task<Result<IssuedGiftVoucherDto>> IssueOnceAsync(IssueGiftVoucherRequest request, Actor actor, CancellationToken ct)
    {
        await using var uow = _uowFactory.Create();
        await uow.BeginTransactionAsync(ct);

        var holder = await uow.Beneficiaries.GetForUpdateAsync(request.BeneficiaryId, ct);
        if (holder is null)
        {
            return Errors.Beneficiary.NotFound;
        }

        var org = await uow.Organizations.GetByIdAsync(holder.OrganizationId, ct);
        if (org is null)
        {
            return Errors.Organization.NotFound;
        }

        var nowUtc = await uow.GetDbUtcNowAsync(ct);
        var timeZoneId = await GetTimeZoneIdAsync(uow, ct);
        var businessDate = TimeZoneHelper.GetLocalBusinessDate(nowUtc, timeZoneId);

        var lastIssuedUtc = await uow.GiftVouchers.GetLatestIssuedUtcAsync(holder.Id, ct);
        var enteredToday = await uow.EntryAttempts.HasGrantedEntryOnAsync(holder.Id, businessDate, ct);
        var context = new GiftEligibilityContext(org, holder, lastIssuedUtc, nowUtc, enteredToday, businessDate);
        var decision = _policy.Evaluate(context);

        if (!decision.IsEligible)
        {
            return MapDenialToError(decision.Reason!.Value);
        }

        var token = QrTokenSigner.NewCardId();
        var expiresOn = businessDate;
        var qrText = _signer.Encode(token, expiresOn, QrPurpose.GiftVoucher);

        var voucher = GiftVoucher.Issue(
            token,
            _signer.KeyId,
            holder.Id,
            org.Id,
            org.GiftValue,
            nowUtc,
            businessDate,
            actor.UserId,
            actor.WorkstationId);

        uow.GiftVouchers.Add(voucher);
        holder.RecordGiftIssued(nowUtc);
        await uow.SaveChangesAsync(ct);
        // Same transaction as the change it records: a crash cannot keep one without the other (R3.5).
        uow.AuditLogs.Add(new AuditLog(nowUtc, "Gift.Issued", actor.UserId, actor.WorkstationId, nameof(GiftVoucher), voucher.Id.ToString(), AuditDetails.ToJson(
            GiftTrail(holder, voucher, actor.UserId, null))));
        await uow.SaveChangesAsync(ct);
        await uow.CommitAsync(ct);

        return new IssuedGiftVoucherDto(
            ReceiptNo: voucher.Id,
            BeneficiaryId: holder.Id,
            HolderName: holder.FullName,
            CardNumber: holder.CardNumber.Value,
            OrganizationName: org.NameAr,
            Value: voucher.Value,
            IssuedAtUtc: voucher.IssuedAtUtc,
            ExpiresOn: voucher.ExpiresOn,
            IsOverride: false,
            QrText: qrText);
    }

    public async Task<Result<IssuedGiftVoucherDto>> IssueOverrideAsync(
        IssueGiftOverrideRequest request,
        CancellationToken ct = default)
    {
        var actor = Actor.Resolve(_currentUser, _currentWorkstation);
        if (actor.IsFailure)
        {
            return actor.Error;
        }

        var canApproveSelf = _currentUser.Has(Permission.OverrideGiftInterval);
        if (!canApproveSelf && request.SupervisorApprovalToken is null)
        {
            return Errors.Auth.SupervisorRequired;
        }

        var validation = await _overrideValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        // R3.2: the approver comes from a single-use token, never from an id the UI supplies.
        int approverUserId;
        if (canApproveSelf)
        {
            approverUserId = actor.Value.UserId;
        }
        else if (!_approvals.TryConsume(request.SupervisorApprovalToken, actor.Value.WorkstationId, out approverUserId))
        {
            return Errors.Auth.SupervisorRequired;
        }

        return await ConcurrencyRetry.RunAsync(retryCt => IssueOverrideOnceAsync(request, actor.Value, canApproveSelf, approverUserId, retryCt), ct);
    }

    /// <summary>One attempt on a fresh unit of work; <see cref="ConcurrencyRetry"/> repeats it after a lost race (ADR-026).</summary>
    private async Task<Result<IssuedGiftVoucherDto>> IssueOverrideOnceAsync(IssueGiftOverrideRequest request, Actor actor, bool canApproveSelf, int approverUserId, CancellationToken ct)
    {
        await using var uow = _uowFactory.Create();
        await uow.BeginTransactionAsync(ct);

        if (!canApproveSelf)
        {
            var approver = await uow.Users.GetByIdAsync(approverUserId, ct);
            if (approver is null || !approver.IsActive ||
                (approver.Role != UserRole.Supervisor && approver.Role != UserRole.Admin))
            {
                return Errors.Auth.SupervisorRequired;
            }
        }

        var holder = await uow.Beneficiaries.GetForUpdateAsync(request.BeneficiaryId, ct);
        if (holder is null)
        {
            return Errors.Beneficiary.NotFound;
        }

        var org = await uow.Organizations.GetByIdAsync(holder.OrganizationId, ct);
        if (org is null)
        {
            return Errors.Organization.NotFound;
        }

        var nowUtc = await uow.GetDbUtcNowAsync(ct);
        var timeZoneId = await GetTimeZoneIdAsync(uow, ct);
        var businessDate = TimeZoneHelper.GetLocalBusinessDate(nowUtc, timeZoneId);

        var lastIssuedUtc = await uow.GiftVouchers.GetLatestIssuedUtcAsync(holder.Id, ct);
        var enteredToday = await uow.EntryAttempts.HasGrantedEntryOnAsync(holder.Id, businessDate, ct);
        var context = new GiftEligibilityContext(org, holder, lastIssuedUtc, nowUtc, enteredToday, businessDate);
        var decision = _policy.Evaluate(context);

        if (!decision.IsEligible && !decision.CanBeOverridden)
        {
            return Errors.Gift.OverrideNotAllowed;
        }

        var token = QrTokenSigner.NewCardId();
        var expiresOn = businessDate;
        var qrText = _signer.Encode(token, expiresOn, QrPurpose.GiftVoucher);

        var isActualOverride = !decision.IsEligible;
        var overrideByUserId = isActualOverride ? (int?)approverUserId : null;
        var overrideReason = isActualOverride ? request.Reason.Trim() : null;

        var voucher = GiftVoucher.Issue(
            token,
            _signer.KeyId,
            holder.Id,
            org.Id,
            org.GiftValue,
            nowUtc,
            businessDate,
            actor.UserId,
            actor.WorkstationId,
            overrideByUserId,
            overrideReason);

        uow.GiftVouchers.Add(voucher);
        holder.RecordGiftIssued(nowUtc);
        await uow.SaveChangesAsync(ct);
        var auditAction = isActualOverride ? "Gift.IssuedOverride" : "Gift.Issued";
        // Same transaction as the change it records: a crash cannot keep one without the other (R3.5).
        uow.AuditLogs.Add(new AuditLog(nowUtc, auditAction, approverUserId, actor.WorkstationId, nameof(GiftVoucher), voucher.Id.ToString(), AuditDetails.ToJson(
            GiftTrail(holder, voucher, actor.UserId, approverUserId)
                .With("Reason", request.Reason.Trim())
                .With("IsOverride", isActualOverride))));
        await uow.SaveChangesAsync(ct);
        await uow.CommitAsync(ct);

        return new IssuedGiftVoucherDto(
            ReceiptNo: voucher.Id,
            BeneficiaryId: holder.Id,
            HolderName: holder.FullName,
            CardNumber: holder.CardNumber.Value,
            OrganizationName: org.NameAr,
            Value: voucher.Value,
            IssuedAtUtc: voucher.IssuedAtUtc,
            ExpiresOn: voucher.ExpiresOn,
            IsOverride: isActualOverride,
            QrText: qrText);
    }

    public async Task<Result<GiftRedemptionDto>> RedeemAsync(
        string? scannedText,
        CancellationToken ct = default)
    {
        if (!_currentUser.Has(Permission.RedeemGift))
        {
            return Errors.General.Forbidden;
        }

        var actor = Actor.Resolve(_currentUser, _currentWorkstation);
        if (actor.IsFailure)
        {
            return actor.Error;
        }

        var verifyResult = _verifier.Verify(scannedText, QrPurpose.GiftVoucher);

        if (verifyResult.Status == QrVerifyStatus.Unreadable || verifyResult.Status == QrVerifyStatus.UnknownKey)
        {
            return Errors.Gift.InvalidCode;
        }

        if (verifyResult.Status == QrVerifyStatus.InvalidSignature)
        {
            return Errors.Gift.Forged;
        }

        var token = verifyResult.Token!;

        return await ConcurrencyRetry.RunAsync(retryCt => RedeemOnceAsync(token, actor.Value, retryCt), ct);
    }

    /// <summary>One attempt on a fresh unit of work; <see cref="ConcurrencyRetry"/> repeats it after a lost race (ADR-026).</summary>
    private async Task<Result<GiftRedemptionDto>> RedeemOnceAsync(EntryCardToken token, Actor actor, CancellationToken ct)
    {
        await using var uow = _uowFactory.Create();
        await uow.BeginTransactionAsync(ct);

        var voucher = await uow.GiftVouchers.GetByTokenForUpdateAsync(token.CardId, ct);
        if (voucher is null)
        {
            return Errors.Gift.NotFound;
        }

        if (voucher.ExpiresOn != token.ExpiresOn)
        {
            return Errors.Gift.Forged;
        }

        var nowUtc = await uow.GetDbUtcNowAsync(ct);
        var timeZoneId = await GetTimeZoneIdAsync(uow, ct);
        var businessDate = TimeZoneHelper.GetLocalBusinessDate(nowUtc, timeZoneId);

        if (voucher.Status == GiftVoucherStatus.Voided)
        {
            return Errors.Gift.Voided;
        }

        if (voucher.Status == GiftVoucherStatus.Redeemed)
        {
            return Errors.Gift.AlreadyRedeemed;
        }

        if (voucher.IsExpired(businessDate))
        {
            return Errors.Gift.Expired;
        }

        var (userId, workstationId) = actor;

        voucher.Redeem(userId, workstationId, nowUtc, businessDate);
        var holder = await uow.Beneficiaries.GetByIdAsync(voucher.BeneficiaryId, ct);
        // Same transaction as the change it records: a crash cannot keep one without the other (R3.5).
        uow.AuditLogs.Add(new AuditLog(nowUtc, "Gift.Redeemed", userId, workstationId, nameof(GiftVoucher), voucher.Id.ToString(), AuditDetails.ToJson(
            (holder is null ? new Dictionary<string, object?> { ["BeneficiaryId"] = voucher.BeneficiaryId, ["OperatorUserId"] = userId } : GiftTrail(holder, voucher, userId, null))
                .With("RedeemedBusinessDate", businessDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)))));
        await uow.SaveChangesAsync(ct);
        await uow.CommitAsync(ct);
        var org = await uow.Organizations.GetByIdAsync(voucher.OrganizationId, ct);

        return new GiftRedemptionDto(
            ReceiptNo: voucher.Id,
            Value: voucher.Value,
            HolderName: holder?.FullName ?? string.Empty,
            CardNumber: holder?.CardNumber.Value ?? string.Empty,
            OrganizationName: org?.NameAr ?? string.Empty,
            IssuedAtUtc: voucher.IssuedAtUtc,
            RedeemedAtUtc: voucher.RedeemedAtUtc!.Value);
    }

    public async Task<Result> VoidAsync(
        VoidGiftVoucherRequest request,
        string? approvalToken = null,
        CancellationToken ct = default)
    {
        if (approvalToken is null && !_currentUser.Has(Permission.VoidGift))
        {
            return Errors.General.Forbidden;
        }

        var validation = await _voidValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        int voidedByUserId;
        if (approvalToken is null)
        {
            voidedByUserId = _currentUser.UserId!.Value;
        }
        else
        {
            // R3.2: the supervisor who signed in on this desk is the one who voids.
            if (!_approvals.TryConsume(approvalToken, _currentWorkstation.WorkstationId, out voidedByUserId))
            {
                return Errors.Auth.SupervisorRequired;
            }

            await using var uowCheck = _uowFactory.Create();
            var approver = await uowCheck.Users.GetByIdAsync(voidedByUserId, ct);
            if (approver is null || !approver.IsActive ||
                (approver.Role != UserRole.Supervisor && approver.Role != UserRole.Admin))
            {
                return Errors.Auth.SupervisorRequired;
            }
        }

        var operatorUserId = _currentUser.UserId ?? voidedByUserId;
        return await ConcurrencyRetry.RunAsync(retryCt => VoidOnceAsync(request, operatorUserId, voidedByUserId, retryCt), ct);
    }

    /// <summary>One attempt on a fresh unit of work; <see cref="ConcurrencyRetry"/> repeats it after a lost race (ADR-026).</summary>
    private async Task<Result> VoidOnceAsync(VoidGiftVoucherRequest request, int operatorUserId, int voidedByUserId, CancellationToken ct)
    {
        await using var uow = _uowFactory.Create();
        await uow.BeginTransactionAsync(ct);

        var voucher = await uow.GiftVouchers.GetByIdAsync(request.VoucherId, ct);
        if (voucher is null)
        {
            return Errors.Gift.NotFound;
        }

        var holder = await uow.Beneficiaries.GetForUpdateAsync(voucher.BeneficiaryId, ct);

        if (voucher.Status == GiftVoucherStatus.Redeemed)
        {
            return Errors.Gift.CannotVoidRedeemed;
        }

        if (voucher.Status == GiftVoucherStatus.Voided)
        {
            return Errors.Gift.Voided;
        }

        var nowUtc = await uow.GetDbUtcNowAsync(ct);

        voucher.Void(voidedByUserId, request.Reason, nowUtc);
        await uow.SaveChangesAsync(ct);
        holder?.RecalculateLastGiftIssued(await uow.GiftVouchers.GetLatestIssuedUtcAsync(voucher.BeneficiaryId, ct));
        // Same transaction as the change it records: a crash cannot keep one without the other (R3.5).
        var trail = holder is null
            ? new Dictionary<string, object?> { ["BeneficiaryId"] = voucher.BeneficiaryId, ["OperatorUserId"] = operatorUserId, ["ReceiptNo"] = voucher.Id }
            : GiftTrail(holder, voucher, operatorUserId, voidedByUserId != operatorUserId ? voidedByUserId : null);
        uow.AuditLogs.Add(new AuditLog(nowUtc, "Gift.Voided", voidedByUserId, _currentWorkstation.WorkstationId, nameof(GiftVoucher), voucher.Id.ToString(), AuditDetails.ToJson(
            trail.With("Reason", request.Reason))));
        await uow.SaveChangesAsync(ct);
        await uow.CommitAsync(ct);

        return Result.Success();
    }

    /// <summary>R5.18: visitor, day, operator and approver plus the voucher itself.</summary>
    private static Dictionary<string, object?> GiftTrail(Beneficiary holder, GiftVoucher voucher, int operatorUserId, int? approvedByUserId) =>
        AuditTrail.For(holder, voucher.BusinessDate, operatorUserId, approvedByUserId)
            .With("ReceiptNo", voucher.Id)
            .With("Value", voucher.Value)
            .With("IssuedAtUtc", voucher.IssuedAtUtc)
            .With("IssuedByUserId", voucher.IssuedByUserId);

    private static GiftEligibilityDto MapToEligibilityDto(
        GiftEligibilityDecision decision,
        decimal giftValue,
        DateTime? lastIssuedUtc)
    {
        string? reasonCode = decision.Reason switch
        {
            GiftDenialReason.OrganizationInactive => Errors.Gift.OrganizationInactive.Code,
            GiftDenialReason.GiftsNotEnabled => Errors.Gift.NotEnabled.Code,
            GiftDenialReason.HolderBlocked => Errors.Gift.HolderBlocked.Code,
            GiftDenialReason.HolderSuspended => Errors.Gift.HolderSuspended.Code,
            GiftDenialReason.IntervalNotElapsed => Errors.Gift.IntervalNotElapsed.Code,
            GiftDenialReason.NoEntryToday => Errors.Gift.NoEntryToday.Code,
            _ => null
        };

        return new GiftEligibilityDto(
            IsEligible: decision.IsEligible,
            Reason: decision.Reason,
            ReasonCode: reasonCode,
            NextEligibleAtUtc: decision.NextEligibleAtUtc,
            CanBeOverridden: decision.CanBeOverridden,
            GiftValue: giftValue,
            LastGiftIssuedUtc: lastIssuedUtc);
    }

    private static Error MapDenialToError(GiftDenialReason reason) => reason switch
    {
        GiftDenialReason.OrganizationInactive => Errors.Gift.OrganizationInactive,
        GiftDenialReason.GiftsNotEnabled => Errors.Gift.NotEnabled,
        GiftDenialReason.HolderBlocked => Errors.Gift.HolderBlocked,
        GiftDenialReason.HolderSuspended => Errors.Gift.HolderSuspended,
        GiftDenialReason.IntervalNotElapsed => Errors.Gift.IntervalNotElapsed,
        GiftDenialReason.NoEntryToday => Errors.Gift.NoEntryToday,
        _ => Errors.Gift.NotEnabled
    };

    private static async Task<string> GetTimeZoneIdAsync(IUnitOfWork uow, CancellationToken ct)
    {
        var tzSetting = await uow.AppSettings.GetByKeyAsync("Park.TimeZoneId", ct);
        return !string.IsNullOrWhiteSpace(tzSetting?.Value) ? tzSetting.Value : "Libya Standard Time";
    }
}
