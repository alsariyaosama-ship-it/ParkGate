using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.CheckIn.Services;
using ParkGate.Application.CheckIn.Validators;
using ParkGate.Application.Common;
using ParkGate.Application.Security;
using ParkGate.Application.Tests.Common;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Application.Tests.CheckIn;

/// <summary>
/// BR-31 (Osama, 2026-11-19): a gift depends on the entry (BR-30). Voiding the entry voids that day's unredeemed gift;
/// when the gift was already redeemed the entry cannot be voided at all.
/// </summary>
public sealed class VoidEntryGiftTests
{
    private const string Reason = "Wrong visitor recorded";

    private readonly List<Beneficiary> _beneficiaries = [];
    private readonly List<EntryAttempt> _attempts = [];
    private readonly FakeGiftVoucherRepository _gifts = new();
    private readonly CurrentUserContext _currentUser = new();
    private readonly FakeUnitOfWorkFactory _uowFactory;
    private readonly CheckInService _sut;
    private readonly Organization _org;
    private readonly Beneficiary _holder;
    private readonly DateTime _now;
    private readonly DateOnly _today;

    public VoidEntryGiftTests()
    {
        _org = new Organization("SOC", "صندوق التضامن", cooldownMinutes: 720, isDefault: true, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(_org, 1);
        var workstation = new Workstation("DESK-01", "Desk 1", WorkstationType.Desk);
        typeof(Workstation).GetProperty(nameof(Workstation.Id))!.SetValue(workstation, 1);
        var supervisor = new User("supervisor1", "Park Supervisor", "hash", UserRole.Supervisor);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(supervisor, 2);
        var currentWorkstation = new CurrentWorkstationContext("DESK-01");
        currentWorkstation.SetWorkstation(workstation);
        _currentUser.SetUser(supervisor);

        var settings = new FakeAppSettingRepository();
        _uowFactory = new FakeUnitOfWorkFactory(
            users: [supervisor], workstations: [workstation], appSettings: settings, organizations: [_org],
            beneficiaries: _beneficiaries, attempts: _attempts, categories: [], giftVouchers: _gifts);

        _sut = new CheckInService(
            _uowFactory, new RollingWindowEligibilityPolicy(), _currentUser, currentWorkstation,
            TestSettings.Over(settings), new SupervisorApprovalStore(),
            new RecordEntryRequestValidator(), new OverrideEntryRequestValidator(), new VoidEntryRequestValidator());

        _holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-1"), "Ali Salem", PhoneNumber.Create("0912345678"));
        _beneficiaries.Add(_holder);
        _now = _uowFactory.CurrentUow.CurrentDbTime;
        _today = DateOnly.FromDateTime(_now);
    }

    [Fact]
    public async Task VoidEntryAsync_UnredeemedGiftThatDay_VoidsTheGiftToo()
    {
        var entry = Entry(_now.AddHours(-2), _today);
        var gift = Gift(_now.AddHours(-1), _today);
        _holder.RecordGiftIssued(gift.IssuedAtUtc);

        var result = await _sut.VoidEntryAsync(new VoidEntryRequest(entry.Id, Reason));

        result.IsSuccess.ShouldBeTrue();
        entry.IsVoided.ShouldBeTrue();
        gift.Status.ShouldBe(GiftVoucherStatus.Voided);
        gift.VoidReason.ShouldBe(Reason);
        _holder.LastGiftIssuedUtc.ShouldBeNull("the gift interval starts again from the remaining gifts");
        _uowFactory.CurrentUow.AuditActionsAtCommit.ShouldContain("EntryVoided");
        _uowFactory.CurrentUow.AuditActionsAtCommit.ShouldContain("Gift.Voided");
    }

    [Fact]
    public async Task VoidEntryAsync_GiftAlreadyRedeemed_IsRefusedAndChangesNothing()
    {
        var entry = Entry(_now.AddHours(-2), _today);
        var gift = Gift(_now.AddHours(-1), _today);
        gift.Redeem(2, 1, _now.AddMinutes(-30), _today);

        var result = await _sut.VoidEntryAsync(new VoidEntryRequest(entry.Id, Reason));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.CheckIn.GiftAlreadyRedeemed.Code);
        entry.IsVoided.ShouldBeFalse();
        gift.Status.ShouldBe(GiftVoucherStatus.Redeemed);
    }

    [Fact]
    public async Task VoidEntryAsync_AnotherValidEntryThatDay_LeavesTheGift()
    {
        Entry(_now.AddHours(-3), _today);
        var second = Entry(_now.AddHours(-2), _today);
        var gift = Gift(_now.AddHours(-1), _today);
        gift.Redeem(2, 1, _now.AddMinutes(-30), _today);

        var result = await _sut.VoidEntryAsync(new VoidEntryRequest(second.Id, Reason));

        result.IsSuccess.ShouldBeTrue("the gift still rests on the other entry");
        second.IsVoided.ShouldBeTrue();
        gift.Status.ShouldBe(GiftVoucherStatus.Redeemed);
    }

    [Fact]
    public async Task VoidEntryAsync_GiftOnAnotherDay_IsNotTouched()
    {
        var yesterday = _today.AddDays(-1);
        Gift(_now.AddDays(-1), yesterday).Redeem(2, 1, _now.AddDays(-1).AddHours(1), yesterday);
        var entry = Entry(_now.AddHours(-2), _today);

        var result = await _sut.VoidEntryAsync(new VoidEntryRequest(entry.Id, Reason));

        result.IsSuccess.ShouldBeTrue();
        entry.IsVoided.ShouldBeTrue();
    }

    private EntryAttempt Entry(DateTime at, DateOnly day)
    {
        var attempt = EntryAttempt.CreateAllowed(Guid.NewGuid(), _holder.Id, _org.Id, at, day, 720, at.AddHours(12), 1, 1, EntryChannel.Desk);
        _attempts.Add(attempt);
        _holder.RecordGrantedEntry(at);
        return attempt;
    }

    private GiftVoucher Gift(DateTime at, DateOnly day)
    {
        var voucher = GiftVoucher.Issue(Guid.NewGuid(), 1, _holder.Id, _org.Id, 10m, at, day, 2, 1);
        _gifts.Add(voucher);
        return voucher;
    }
}
