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

public sealed class CheckInServiceTests
{
    private readonly List<User> _users = [];
    private readonly List<Workstation> _workstations = [];
    private readonly List<Organization> _organizations = [];
    private readonly List<Beneficiary> _beneficiaries = [];
    private readonly List<EntryAttempt> _attempts = [];
    private readonly List<AgeCategory> _categories = [];
    private readonly FakeAppSettingRepository _settings = new();
    private readonly FakeAuditWriter _auditWriter = new();
    private readonly CurrentUserContext _currentUser = new();
    private readonly CurrentWorkstationContext _currentWorkstation = new("DESK-01");
    private readonly FakeUnitOfWorkFactory _uowFactory;
    private readonly IEntryEligibilityPolicy _policy = new RollingWindowEligibilityPolicy();

    private readonly CheckInService _sut;
    private readonly Organization _org;
    private readonly AgeCategory _childCategory;
    private readonly AgeCategory _adultCategory;

    public CheckInServiceTests()
    {
        _org = new Organization("SOC", "صندوق التضامن", cooldownMinutes: 720, isDefault: true, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(_org, 1);
        _organizations.Add(_org);

        _childCategory = new AgeCategory("أطفال", "من 3 إلى 12 سنة", sortOrder: 1, isActive: true);
        typeof(AgeCategory).GetProperty(nameof(AgeCategory.Id))!.SetValue(_childCategory, 1);
        _adultCategory = new AgeCategory("بالغون", "فوق 12 سنة", sortOrder: 2, isActive: true);
        typeof(AgeCategory).GetProperty(nameof(AgeCategory.Id))!.SetValue(_adultCategory, 2);
        _categories.AddRange([_childCategory, _adultCategory]);

        var workstation = new Workstation("DESK-01", "Desk 1", WorkstationType.Desk);
        typeof(Workstation).GetProperty(nameof(Workstation.Id))!.SetValue(workstation, 1);
        _workstations.Add(workstation);
        _currentWorkstation.SetWorkstation(workstation);

        var operatorUser = new User("operator1", "Desk Operator", "hash", UserRole.Operator);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(operatorUser, 1);
        var supervisorUser = new User("supervisor1", "Park Supervisor", "hash", UserRole.Supervisor);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(supervisorUser, 2);
        _users.AddRange([operatorUser, supervisorUser]);

        _currentUser.SetUser(operatorUser);

        _uowFactory = new FakeUnitOfWorkFactory(
            _users,
            _workstations,
            _settings,
            _organizations,
            _beneficiaries,
            _attempts,
            _categories);

        _sut = new CheckInService(
            _uowFactory,
            _policy,
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new RecordEntryRequestValidator(),
            new OverrideEntryRequestValidator(),
            new VoidEntryRequestValidator());
    }

    [Fact]
    public async Task RecordEntryAsync_WhenEligible_ShouldGrantEntryAndCalculateTotalPersons()
    {
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-1"), "Ali Salem", PhoneNumber.Create("0912345678"));
        _beneficiaries.Add(holder);

        var companions = new List<CompanionRequestDto>
        {
            new(_childCategory.Id, 2),
            new(_adultCategory.Id, 1)
        };

        var request = new RecordEntryRequest(holder.Id, companions);
        var result = await _sut.RecordEntryAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Result.ShouldBe(EntryResult.Allowed);
        result.Value.TotalPersons.ShouldBe(4); // 1 holder + 2 children + 1 adult (BR-08)

        holder.LastGrantedEntryUtc.ShouldBe(_uowFactory.CurrentUow.CurrentDbTime);
        _attempts.Count.ShouldBe(1);
        _attempts[0].Result.ShouldBe(EntryResult.Allowed);
        _auditWriter.Writes.ShouldContain(w => w.Action == "EntryGranted");
    }

    [Fact]
    public async Task RecordEntryAsync_WhenGroupSizeExceedsMax_ShouldReturnError()
    {
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-1"), "Ali Salem", PhoneNumber.Create("0912345678"));
        _beneficiaries.Add(holder);

        // 10 companions + 1 holder = 11 persons (exceeds default MaxGroupSize 10)
        var companions = new List<CompanionRequestDto>
        {
            new(_childCategory.Id, 10)
        };

        var request = new RecordEntryRequest(holder.Id, companions);
        var result = await _sut.RecordEntryAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.CheckIn.ExceedsMaxGroupSize.Code);
    }

    [Fact]
    public async Task RecordEntryAsync_WhenCooldownActive_ShouldDenyAndApplyDedupe()
    {
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-1"), "Ali Salem", PhoneNumber.Create("0912345678"));
        // Last granted 1 hour ago (within 12h cooldown)
        holder.RecordGrantedEntry(_uowFactory.CurrentUow.CurrentDbTime.AddHours(-1));
        _beneficiaries.Add(holder);

        var request = new RecordEntryRequest(holder.Id);

        // Attempt 1: Denied and logged
        var result1 = await _sut.RecordEntryAsync(request);
        result1.IsFailure.ShouldBeTrue();
        result1.Error.Code.ShouldBe(Errors.CheckIn.CooldownActive.Code);
        _attempts.Count.ShouldBe(1);
        _attempts[0].Result.ShouldBe(EntryResult.Denied);

        // Attempt 2 immediately after: Denied, but dedupe skips inserting duplicate row (BR-10)
        var result2 = await _sut.RecordEntryAsync(request);
        result2.IsFailure.ShouldBeTrue();
        result2.Error.Code.ShouldBe(Errors.CheckIn.CooldownActive.Code);
        _attempts.Count.ShouldBe(1); // Deduped! Still 1 attempt
    }

    [Fact]
    public async Task OverrideEntryAsync_WhenApprovedBySupervisor_ShouldGrantOverriddenEntry()
    {
        _currentUser.SetUser(2, "supervisor1", "Park Supervisor", UserRole.Supervisor);

        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-1"), "Ali Salem", PhoneNumber.Create("0912345678"));
        holder.RecordGrantedEntry(_uowFactory.CurrentUow.CurrentDbTime.AddHours(-1));
        _beneficiaries.Add(holder);

        var request = new OverrideEntryRequest(holder.Id, "Special visit authorized by management");
        var result = await _sut.OverrideEntryAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Result.ShouldBe(EntryResult.Overridden);
        holder.LastGrantedEntryUtc.ShouldBe(_uowFactory.CurrentUow.CurrentDbTime);
        _auditWriter.Writes.ShouldContain(w => w.Action == "EntryOverridden");
    }

    [Fact]
    public async Task OverrideEntryAsync_WhenHolderBlocked_ShouldRejectOverride()
    {
        _currentUser.SetUser(2, "supervisor1", "Park Supervisor", UserRole.Supervisor);

        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-1"), "Ali Salem", PhoneNumber.Create("0912345678"));
        holder.Block("Card stolen reported");
        _beneficiaries.Add(holder);

        var request = new OverrideEntryRequest(holder.Id, "Attempting override on blocked card");
        var result = await _sut.OverrideEntryAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.CheckIn.OverrideNotAllowed.Code);
    }

    [Fact]
    public async Task VoidEntryAsync_ShouldMarkVoidedAndRecalculateLastGrantedUtc()
    {
        _currentUser.SetUser(2, "supervisor1", "Park Supervisor", UserRole.Supervisor);

        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-1"), "Ali Salem", PhoneNumber.Create("0912345678"));
        _beneficiaries.Add(holder);

        var entry1Time = _uowFactory.CurrentUow.CurrentDbTime.AddDays(-2);
        var entry2Time = _uowFactory.CurrentUow.CurrentDbTime.AddHours(-1);

        var attempt1 = EntryAttempt.CreateAllowed(
            Guid.NewGuid(), holder.Id, _org.Id, entry1Time, DateOnly.FromDateTime(entry1Time),
            720, entry1Time.AddHours(12), 1, 1, EntryChannel.Desk);

        var attempt2 = EntryAttempt.CreateAllowed(
            Guid.NewGuid(), holder.Id, _org.Id, entry2Time, DateOnly.FromDateTime(entry2Time),
            720, entry2Time.AddHours(12), 1, 1, EntryChannel.Desk);

        holder.RecordGrantedEntry(entry2Time);
        _attempts.AddRange([attempt1, attempt2]);

        // Void the latest entry
        var voidRequest = new VoidEntryRequest(attempt2.Id, "Operator recorded wrong beneficiary");
        var result = await _sut.VoidEntryAsync(voidRequest);

        result.IsSuccess.ShouldBeTrue();
        attempt2.IsVoided.ShouldBeTrue();
        attempt2.VoidReason.ShouldBe("Operator recorded wrong beneficiary");

        // Recalculates to entry1Time (BR-13)
        holder.LastGrantedEntryUtc.ShouldBe(entry1Time);
        _auditWriter.Writes.ShouldContain(w => w.Action == "EntryVoided");
    }

    [Fact]
    public async Task GetTodayDeskEntriesAsync_ShouldReturnSummaryAndCounts()
    {
        var today = DateOnly.FromDateTime(_uowFactory.CurrentUow.CurrentDbTime);
        var attemptAllowed = EntryAttempt.CreateAllowed(
            Guid.NewGuid(), Guid.NewGuid(), _org.Id, _uowFactory.CurrentUow.CurrentDbTime, today,
            720, _uowFactory.CurrentUow.CurrentDbTime.AddHours(12), 1, 1, EntryChannel.Desk,
            [new EntryAttemptCompanion(_childCategory.Id, 2)]);

        var attemptDenied = EntryAttempt.CreateDenied(
            Guid.NewGuid(), Guid.NewGuid(), _org.Id, _uowFactory.CurrentUow.CurrentDbTime, today,
            DenialReason.CooldownActive, 720, null, 1, 1, EntryChannel.Desk);

        _attempts.AddRange([attemptAllowed, attemptDenied]);

        var result = await _sut.GetTodayDeskEntriesAsync(1);

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalGranted.ShouldBe(1);
        result.Value.TotalDenied.ShouldBe(1);
        result.Value.TotalPersons.ShouldBe(3); // 1 holder + 2 children
        result.Value.PersonsPerCategory.ShouldContain(c => c.AgeCategoryId == _childCategory.Id && c.Count == 2);
    }
}
