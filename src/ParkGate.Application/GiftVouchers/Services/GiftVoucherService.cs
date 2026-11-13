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
    private readonly IValidator<IssueGiftOverrideRequest> _overrideValidator;
    private readonly IValidator<VoidGiftVoucherRequest> _voidValidator;

    public GiftVoucherService(
        IUnitOfWorkFactory uowFactory,
        IGiftEligibilityPolicy policy,
        QrTokenSigner signer,
        QrTokenVerifier verifier,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        IValidator<IssueGiftOverrideRequest> overrideValidator,
        IValidator<VoidGiftVoucherRequest> voidValidator)
    {
        _uowFactory = uowFactory;
        _policy = policy;
        _signer = signer;
        _verifier = verifier;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
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

        var context = new GiftEligibilityContext(org, holder, lastIssuedUtc, nowUtc);
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
        var context = new GiftEligibilityContext(org, holder, lastIssuedUtc, nowUtc);
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
            actor.Value.UserId,
            actor.Value.WorkstationId);

        uow.GiftVouchers.Add(voucher);
        await uow.SaveChangesAsync(ct);
        // Same transaction as the change it records: a crash cannot keep one without the other (R3.5).
        uow.AuditLogs.Add(new AuditLog(nowUtc, "Gift.Issued", actor.Value.UserId, actor.Value.WorkstationId, nameof(GiftVoucher), voucher.Id.ToString(), AuditDetails.ToJson(new { ReceiptNo = voucher.Id, voucher.Value })));
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

        int approverUserId;
        if (_currentUser.Has(Permission.OverrideGiftInterval))
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

        var validation = await _overrideValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        await using var uow = _uowFactory.Create();
        await uow.BeginTransactionAsync(ct);

        if (!_currentUser.Has(Permission.OverrideGiftInterval))
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
        var context = new GiftEligibilityContext(org, holder, lastIssuedUtc, nowUtc);
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
            actor.Value.UserId,
            actor.Value.WorkstationId,
            overrideByUserId,
            overrideReason);

        uow.GiftVouchers.Add(voucher);
        await uow.SaveChangesAsync(ct);
        var auditAction = isActualOverride ? "Gift.IssuedOverride" : "Gift.Issued";
        // Same transaction as the change it records: a crash cannot keep one without the other (R3.5).
        uow.AuditLogs.Add(new AuditLog(nowUtc, auditAction, approverUserId, actor.Value.WorkstationId, nameof(GiftVoucher), voucher.Id.ToString(), AuditDetails.ToJson(new { ReceiptNo = voucher.Id, voucher.Value })));
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

        var (userId, workstationId) = actor.Value;

        voucher.Redeem(userId, workstationId, nowUtc, businessDate);
        // Same transaction as the change it records: a crash cannot keep one without the other (R3.5).
        uow.AuditLogs.Add(new AuditLog(nowUtc, "Gift.Redeemed", userId, workstationId, nameof(GiftVoucher), voucher.Id.ToString(), AuditDetails.ToJson(new { ReceiptNo = voucher.Id, voucher.Value })));
        await uow.SaveChangesAsync(ct);
        await uow.CommitAsync(ct);

        var holder = await uow.Beneficiaries.GetByIdAsync(voucher.BeneficiaryId, ct);
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
        int? supervisorUserId = null,
        CancellationToken ct = default)
    {
        int voidedByUserId;

        if (supervisorUserId.HasValue)
        {
            await using var uowCheck = _uowFactory.Create();
            var approver = await uowCheck.Users.GetByIdAsync(supervisorUserId.Value, ct);
            if (approver is null || !approver.IsActive ||
                (approver.Role != UserRole.Supervisor && approver.Role != UserRole.Admin))
            {
                return Errors.Auth.SupervisorRequired;
            }
            voidedByUserId = supervisorUserId.Value;
        }
        else if (_currentUser.Has(Permission.VoidGift))
        {
            voidedByUserId = _currentUser.UserId!.Value;
        }
        else
        {
            return Errors.General.Forbidden;
        }

        var validation = await _voidValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        await using var uow = _uowFactory.Create();
        await uow.BeginTransactionAsync(ct);

        var voucher = await uow.GiftVouchers.GetByIdAsync(request.VoucherId, ct);
        if (voucher is null)
        {
            return Errors.Gift.NotFound;
        }

        await uow.Beneficiaries.GetForUpdateAsync(voucher.BeneficiaryId, ct);

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
        // Same transaction as the change it records: a crash cannot keep one without the other (R3.5).
        uow.AuditLogs.Add(new AuditLog(nowUtc, "Gift.Voided", voidedByUserId, _currentWorkstation.WorkstationId, nameof(GiftVoucher), voucher.Id.ToString(), AuditDetails.ToJson(new { ReceiptNo = voucher.Id, request.Reason })));
        await uow.SaveChangesAsync(ct);
        await uow.CommitAsync(ct);

        return Result.Success();
    }

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
        _ => Errors.Gift.NotEnabled
    };

    private static async Task<string> GetTimeZoneIdAsync(IUnitOfWork uow, CancellationToken ct)
    {
        var tzSetting = await uow.AppSettings.GetByKeyAsync("Park.TimeZoneId", ct);
        return !string.IsNullOrWhiteSpace(tzSetting?.Value) ? tzSetting.Value : "Libya Standard Time";
    }
}
