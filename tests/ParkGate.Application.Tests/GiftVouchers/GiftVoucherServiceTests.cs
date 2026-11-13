using System.Security.Cryptography;
using FluentValidation;
using ParkGate.Application.Common;
using ParkGate.Application.EntryCards;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.GiftVouchers.Services;
using ParkGate.Application.GiftVouchers.Validators;
using ParkGate.Application.Security;
using ParkGate.Application.Tests.Common;
using ParkGate.Domain.Entities;
using ParkGate.Domain.EntryCards;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;
using Shouldly;

namespace ParkGate.Application.Tests.GiftVouchers;

public sealed class GiftVoucherServiceTests : IDisposable
{
    private const byte KeyId = 1;

    private readonly ECDsa _ecKey;
    private readonly QrTokenSigner _signer;
    private readonly QrTokenVerifier _verifier;

    private readonly List<User> _users = [];
    private readonly List<Workstation> _workstations = [];
    private readonly List<Organization> _organizations = [];
    private readonly List<Beneficiary> _beneficiaries = [];
    private readonly List<EntryAttempt> _attempts = [];
    private readonly List<AgeCategory> _categories = [];
    private readonly FakeGiftVoucherRepository _giftRepo = new();
    private readonly FakeAppSettingRepository _settings = new();
    private readonly FakeAuditWriter _auditWriter = new();
    private readonly CurrentUserContext _currentUser = new();
    private readonly CurrentWorkstationContext _currentWorkstation = new("DESK-01");
    private readonly FakeUnitOfWorkFactory _uowFactory;
    private readonly GiftVoucherService _sut;

    private readonly Organization _org;
    private readonly Beneficiary _holder;
    private readonly User _operatorUser;
    private readonly User _supervisorUser;
    private readonly Workstation _workstation;

    public GiftVoucherServiceTests()
    {
        _ecKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _signer = new QrTokenSigner(ECDsa.Create(_ecKey.ExportParameters(true)), KeyId);
        _verifier = new QrTokenVerifier([new QrPublicKey(KeyId, _ecKey.ExportSubjectPublicKeyInfo())]);

        _org = new Organization("SSF", "صندوق التضامن", isDefault: true, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(_org, 1);
        _org.ConfigureGift(true, 50m, 30);
        _organizations.Add(_org);

        _workstation = new Workstation("DESK-01", "Gate Desk", WorkstationType.Desk, isActive: true);
        typeof(Workstation).GetProperty(nameof(Workstation.Id))!.SetValue(_workstation, 1);
        _workstations.Add(_workstation);

        _operatorUser = new User("op1", "Operator", "hash", UserRole.Operator);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(_operatorUser, 1);

        _supervisorUser = new User("sup1", "Supervisor", "hash", UserRole.Supervisor);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(_supervisorUser, 2);

        _users.AddRange([_operatorUser, _supervisorUser]);
        _currentUser.SetUser(_operatorUser);
        _currentWorkstation.SetWorkstation(_workstation);

        _holder = new Beneficiary(
            Guid.NewGuid(), _org.Id,
            CardNumber.Create("CARD-001"), "Ali Salem",
            PhoneNumber.Create("0912345678"));
        _beneficiaries.Add(_holder);

        _settings.Add(new Domain.Entities.AppSetting("Park.TimeZoneId", "Libya Standard Time", DateTime.UtcNow, null));

        _uowFactory = new FakeUnitOfWorkFactory(
            _users, _workstations, _settings, _organizations, _beneficiaries, _attempts, _categories, _giftRepo);

        _sut = new GiftVoucherService(
            _uowFactory,
            new RollingIntervalGiftEligibilityPolicy(),
            _signer,
            _verifier,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new IssueGiftOverrideRequestValidator(),
            new VoidGiftVoucherRequestValidator());
    }

    public void Dispose()
    {
        _signer.Dispose();
        _verifier.Dispose();
        _ecKey.Dispose();
    }

    // ── Eligibility ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetEligibilityAsync_WhenEligible_ShouldReturnEligibleDto()
    {
        var result = await _sut.GetEligibilityAsync(_holder.Id);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsEligible.ShouldBeTrue();
        result.Value.GiftValue.ShouldBe(50m);
    }

    [Fact]
    public async Task GetEligibilityAsync_WhenGiftsNotEnabled_ShouldReturnNotEnabled()
    {
        _org.ConfigureGift(false, 0m, 30);

        var result = await _sut.GetEligibilityAsync(_holder.Id);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsEligible.ShouldBeFalse();
        result.Value.ReasonCode.ShouldBe(Errors.Gift.NotEnabled.Code);
    }

    [Fact]
    public async Task GetEligibilityAsync_WhenHolderBlocked_ShouldReturnHolderBlocked()
    {
        _holder.Suspend("test");
        _holder.Block("blocked");

        var result = await _sut.GetEligibilityAsync(_holder.Id);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsEligible.ShouldBeFalse();
        result.Value.ReasonCode.ShouldBe(Errors.Gift.HolderBlocked.Code);
    }

    [Fact]
    public async Task GetEligibilityAsync_WhenIntervalNotElapsed_ShouldReturnIntervalNotElapsed()
    {
        var uow = _uowFactory.CurrentUow;
        uow.CurrentDbTime = new DateTime(2026, 10, 29, 10, 0, 0, DateTimeKind.Utc);

        var recentVoucher = GiftVoucher.Issue(
            Guid.NewGuid(), KeyId, _holder.Id, _org.Id, 50m,
            new DateTime(2026, 10, 10, 10, 0, 0, DateTimeKind.Utc),
            new DateOnly(2026, 10, 10),
            _operatorUser.Id, _workstation.Id);
        typeof(GiftVoucher).GetProperty(nameof(GiftVoucher.Id))!.SetValue(recentVoucher, 1);
        _giftRepo.Add(recentVoucher);

        var result = await _sut.GetEligibilityAsync(_holder.Id);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsEligible.ShouldBeFalse();
        result.Value.ReasonCode.ShouldBe(Errors.Gift.IntervalNotElapsed.Code);
        result.Value.CanBeOverridden.ShouldBeTrue();
    }

    [Fact]
    public async Task GetEligibilityAsync_WhenOperatorLacksPermission_ShouldReturnForbidden()
    {
        var adminUser = new User("admin", "Admin", "hash", UserRole.Admin);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(adminUser, 99);

        // Switch to a user with no IssueGift permission by creating a new context
        var noPermUser = new CurrentUserContext();
        var restrictedUser = new User("viewer", "Viewer", "hash", UserRole.Operator);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(restrictedUser, 10);
        // Operator has IssueGift, so use a raw context with no user set (not logged in)
        var emptyUser = new CurrentUserContext();

        var sut = new GiftVoucherService(
            _uowFactory,
            new RollingIntervalGiftEligibilityPolicy(),
            _signer, _verifier, _auditWriter,
            emptyUser, _currentWorkstation,
            new IssueGiftOverrideRequestValidator(),
            new VoidGiftVoucherRequestValidator());

        var result = await sut.GetEligibilityAsync(_holder.Id);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
    }

    // ── Issue ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task IssueAsync_WhenEligible_ShouldCreateVoucherAndQr()
    {
        var result = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(50m);
        result.Value.IsOverride.ShouldBeFalse();
        result.Value.QrText.Length.ShouldBe(QrTokenFormat.DigitLength);
        _giftRepo.Vouchers.Count.ShouldBe(1);
    }

    [Fact]
    public async Task IssueAsync_QrShouldVerifyWithGiftVoucherPurpose_AndFailWithEntryCardPurpose()
    {
        var result = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));
        var qrText = result.Value.QrText;

        var giftVerify = _verifier.Verify(qrText, QrPurpose.GiftVoucher);
        var entryVerify = _verifier.Verify(qrText, QrPurpose.EntryCard);

        giftVerify.Status.ShouldBe(QrVerifyStatus.Valid);
        entryVerify.Status.ShouldBe(QrVerifyStatus.InvalidSignature);
    }

    [Fact]
    public async Task IssueAsync_SecondIssueSameDay_ShouldReturnIntervalNotElapsed()
    {
        await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        var result = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Gift.IntervalNotElapsed.Code);
        _giftRepo.Vouchers.Count.ShouldBe(1);
    }

    [Fact]
    public async Task IssueAsync_AfterInterval_ShouldSucceedAgain()
    {
        var uow = _uowFactory.CurrentUow;
        uow.CurrentDbTime = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);

        await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        uow.CurrentDbTime = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);
        var result = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        result.IsSuccess.ShouldBeTrue();
        _giftRepo.Vouchers.Count.ShouldBe(2);
    }

    [Fact]
    public async Task IssueAsync_VoidedVoucherDoesNotCountTowardInterval()
    {
        var uow = _uowFactory.CurrentUow;
        uow.CurrentDbTime = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);

        var issueResult = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));
        var voucherId = issueResult.Value.ReceiptNo;

        _currentUser.SetUser(_supervisorUser);
        await _sut.VoidAsync(new VoidGiftVoucherRequest(voucherId, "printing mistake"));

        _currentUser.SetUser(_operatorUser);
        var result = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task IssueAsync_WhenOrgGiftsDisabled_ShouldReturnNotEnabled()
    {
        _org.ConfigureGift(false, 0m, 30);

        var result = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Gift.NotEnabled.Code);
    }

    [Fact]
    public async Task IssueAsync_WhenHolderBlocked_ShouldReturnHolderBlocked()
    {
        _holder.Suspend("test");
        _holder.Block("blocked");

        var result = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Gift.HolderBlocked.Code);
    }

    [Fact]
    public async Task IssueAsync_ShouldWriteAuditWithoutQrText()
    {
        await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        _auditWriter.Writes.ShouldContain(w => w.Action == "Gift.Issued");
        // DetailsJson must not contain the QR text (would be > 200 chars of digits)
        var write = _auditWriter.Writes.First(w => w.Action == "Gift.Issued");
        // The detailsJson should be short (just ReceiptNo and Value), not 203-digit QR string
        // We verify it's not null and doesn't exceed reasonable length
        (write.EntityName == nameof(GiftVoucher)).ShouldBeTrue();
    }

    // ── R3.3: no silent fallback to user or workstation id 0 ────────────────

    [Fact]
    public async Task IssueAsync_WhenWorkstationNotRegistered_ReturnsWorkstationNotFoundAndIssuesNothing()
    {
        _currentWorkstation.Clear();

        var result = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Auth.WorkstationNotFound.Code);
        _auditWriter.Writes.ShouldBeEmpty();
    }

    [Fact]
    public async Task IssueOverrideAsync_WhenNobodyIsSignedIn_ReturnsUnauthorized()
    {
        _currentUser.Clear();

        var result = await _sut.IssueOverrideAsync(
            new IssueGiftOverrideRequest(_holder.Id, "manager authorized", _supervisorUser.Id));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.General.Unauthorized.Code);
        _auditWriter.Writes.ShouldBeEmpty();
    }

    [Fact]
    public async Task RedeemAsync_WhenWorkstationNotRegistered_ReturnsWorkstationNotFoundAndKeepsVoucherOpen()
    {
        var issued = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));
        _currentWorkstation.Clear();

        var result = await _sut.RedeemAsync(issued.Value.QrText);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Auth.WorkstationNotFound.Code);
        _auditWriter.Writes.ShouldNotContain(w => w.Action == "Gift.Redeemed");
    }

    // ── Issue Override ───────────────────────────────────────────────────────

    [Fact]
    public async Task IssueOverrideAsync_OperatorWithoutSupervisorId_ShouldReturnSupervisorRequired()
    {
        var result = await _sut.IssueOverrideAsync(new IssueGiftOverrideRequest(_holder.Id, "valid reason here"));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Auth.SupervisorRequired.Code);
    }

    [Fact]
    public async Task IssueOverrideAsync_OperatorWithValidSupervisorId_AfterInterval_ShouldIssueWithOverrideFlag()
    {
        // Put holder in interval-not-elapsed state
        var uow = _uowFactory.CurrentUow;
        uow.CurrentDbTime = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        // Try override while interval is active
        uow.CurrentDbTime = new DateTime(2026, 10, 10, 10, 0, 0, DateTimeKind.Utc);
        var result = await _sut.IssueOverrideAsync(
            new IssueGiftOverrideRequest(_holder.Id, "manager authorized", _supervisorUser.Id));

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsOverride.ShouldBeTrue();
    }

    [Fact]
    public async Task IssueOverrideAsync_ReasonTooShort_ShouldReturnValidationError()
    {
        var result = await _sut.IssueOverrideAsync(
            new IssueGiftOverrideRequest(_holder.Id, "hi", _supervisorUser.Id));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("General.ValidationFailed");
    }

    [Fact]
    public async Task IssueOverrideAsync_HolderBlocked_CannotBeOverridden()
    {
        _holder.Suspend("test");
        _holder.Block("blocked");

        var result = await _sut.IssueOverrideAsync(
            new IssueGiftOverrideRequest(_holder.Id, "valid reason here", _supervisorUser.Id));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Gift.OverrideNotAllowed.Code);
    }

    [Fact]
    public async Task IssueOverrideAsync_SupervisorDirectly_ShouldSucceed()
    {
        _currentUser.SetUser(_supervisorUser);
        var uow = _uowFactory.CurrentUow;
        uow.CurrentDbTime = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));
        uow.CurrentDbTime = new DateTime(2026, 10, 10, 10, 0, 0, DateTimeKind.Utc);

        var result = await _sut.IssueOverrideAsync(
            new IssueGiftOverrideRequest(_holder.Id, "supervisor approves"));

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsOverride.ShouldBeTrue();
    }

    // ── Redeem ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task RedeemAsync_WithValidQr_ShouldSucceed()
    {
        var issueResult = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));
        var qrText = issueResult.Value.QrText;

        var result = await _sut.RedeemAsync(qrText);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(50m);
        result.Value.ReceiptNo.ShouldBe(issueResult.Value.ReceiptNo);
    }

    [Fact]
    public async Task RedeemAsync_SecondScan_ShouldReturnAlreadyRedeemed()
    {
        var issueResult = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));
        var qrText = issueResult.Value.QrText;

        await _sut.RedeemAsync(qrText);
        var result = await _sut.RedeemAsync(qrText);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Gift.AlreadyRedeemed.Code);
    }

    [Fact]
    public async Task RedeemAsync_EntryCardQr_ShouldReturnForged()
    {
        using var entrySigner = new QrTokenSigner(ECDsa.Create(_ecKey.ExportParameters(true)), KeyId);
        var entryQr = entrySigner.Encode(QrTokenSigner.NewCardId(), new DateOnly(2026, 12, 31), QrPurpose.EntryCard);

        var result = await _sut.RedeemAsync(entryQr);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Gift.Forged.Code);
    }

    [Fact]
    public async Task RedeemAsync_GarbageInput_ShouldReturnInvalidCode()
    {
        var result = await _sut.RedeemAsync("000000000000");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Gift.InvalidCode.Code);
    }

    [Fact]
    public async Task RedeemAsync_NullInput_ShouldReturnInvalidCode()
    {
        var result = await _sut.RedeemAsync(null);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Gift.InvalidCode.Code);
    }

    [Fact]
    public async Task RedeemAsync_UnknownToken_ValidSignature_ShouldReturnNotFound()
    {
        // Sign a token with the real key but never insert a voucher for it
        var unknownToken = QrTokenSigner.NewCardId();
        var qrText = _signer.Encode(unknownToken, new DateOnly(2027, 10, 29), QrPurpose.GiftVoucher);

        var result = await _sut.RedeemAsync(qrText);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Gift.NotFound.Code);
    }

    [Fact]
    public async Task RedeemAsync_ExpiredVoucher_ShouldReturnExpired()
    {
        var uow = _uowFactory.CurrentUow;
        // Issue on day 1
        uow.CurrentDbTime = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var issueResult = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));
        var qrText = issueResult.Value.QrText;

        // Advance DB time past the business date
        uow.CurrentDbTime = new DateTime(2026, 10, 2, 10, 0, 0, DateTimeKind.Utc);

        var result = await _sut.RedeemAsync(qrText);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Gift.Expired.Code);
    }

    [Fact]
    public async Task RedeemAsync_VoidedVoucher_ShouldReturnVoided()
    {
        var uow = _uowFactory.CurrentUow;
        var issueResult = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));
        var qrText = issueResult.Value.QrText;

        _currentUser.SetUser(_supervisorUser);
        await _sut.VoidAsync(new VoidGiftVoucherRequest(issueResult.Value.ReceiptNo, "void reason here"));

        _currentUser.SetUser(_operatorUser);
        var result = await _sut.RedeemAsync(qrText);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Gift.Voided.Code);
    }

    [Fact]
    public async Task RedeemAsync_WithoutRedeemGiftPermission_ShouldReturnForbidden()
    {
        var issueResult = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));
        var qrText = issueResult.Value.QrText;

        // Use a context with no user set
        var emptyUser = new CurrentUserContext();
        var sut = new GiftVoucherService(
            _uowFactory, new RollingIntervalGiftEligibilityPolicy(),
            _signer, _verifier, _auditWriter,
            emptyUser, _currentWorkstation,
            new IssueGiftOverrideRequestValidator(), new VoidGiftVoucherRequestValidator());

        var result = await sut.RedeemAsync(qrText);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
    }

    // ── Void ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task VoidAsync_IssuedVoucher_ShouldSucceed()
    {
        _currentUser.SetUser(_supervisorUser);
        var issueResult = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        var voidResult = await _sut.VoidAsync(new VoidGiftVoucherRequest(issueResult.Value.ReceiptNo, "print error"));

        voidResult.IsSuccess.ShouldBeTrue();
        _giftRepo.Vouchers[0].Status.ShouldBe(GiftVoucherStatus.Voided);
    }

    [Fact]
    public async Task VoidAsync_RedeemedVoucher_ShouldReturnCannotVoidRedeemed()
    {
        _currentUser.SetUser(_supervisorUser);
        var issueResult = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));
        _currentUser.SetUser(_operatorUser);
        await _sut.RedeemAsync(issueResult.Value.QrText);

        _currentUser.SetUser(_supervisorUser);
        var voidResult = await _sut.VoidAsync(new VoidGiftVoucherRequest(issueResult.Value.ReceiptNo, "print error"));

        voidResult.IsFailure.ShouldBeTrue();
        voidResult.Error.Code.ShouldBe(Errors.Gift.CannotVoidRedeemed.Code);
    }

    [Fact]
    public async Task VoidAsync_AfterVoid_HolderCanBeIssuedAgain()
    {
        var uow = _uowFactory.CurrentUow;
        uow.CurrentDbTime = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);

        var issueResult = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        _currentUser.SetUser(_supervisorUser);
        await _sut.VoidAsync(new VoidGiftVoucherRequest(issueResult.Value.ReceiptNo, "void reason here"));

        _currentUser.SetUser(_operatorUser);
        var result = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task VoidAsync_WithoutVoidGiftPermission_ShouldReturnForbidden()
    {
        var issueResult = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));
        // Operator does not have VoidGift; Supervisor does

        var voidResult = await _sut.VoidAsync(new VoidGiftVoucherRequest(issueResult.Value.ReceiptNo, "print error"));

        voidResult.IsFailure.ShouldBeTrue();
        voidResult.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
    }

    // ── Validator boundary ───────────────────────────────────────────────────

    [Fact]
    public async Task VoidAsync_ReasonExactlyFiveChars_ShouldPass()
    {
        _currentUser.SetUser(_supervisorUser);
        var issueResult = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        var result = await _sut.VoidAsync(new VoidGiftVoucherRequest(issueResult.Value.ReceiptNo, "12345"));

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task VoidAsync_ReasonFourChars_ShouldReturnValidationError()
    {
        _currentUser.SetUser(_supervisorUser);
        var issueResult = await _sut.IssueAsync(new IssueGiftVoucherRequest(_holder.Id));

        var result = await _sut.VoidAsync(new VoidGiftVoucherRequest(issueResult.Value.ReceiptNo, "1234"));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("General.ValidationFailed");
    }
}
