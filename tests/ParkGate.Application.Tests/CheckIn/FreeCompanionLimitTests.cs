using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.CheckIn.Services;
using ParkGate.Application.CheckIn.Validators;
using ParkGate.Application.Common;
using ParkGate.Application.Security;
using ParkGate.Application.Tests.Common;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Application.Tests.CheckIn;

public sealed class FreeCompanionLimitTests
{
    private readonly List<User> _users = [];
    private readonly List<Workstation> _workstations = [];
    private readonly List<Organization> _organizations = [];
    private readonly List<Beneficiary> _beneficiaries = [];
    private readonly List<EntryAttempt> _attempts = [];
    private readonly List<AgeCategory> _categories = [];
    private readonly FakeAppSettingRepository _settings = new();
    private readonly CurrentUserContext _currentUser = new();
    private readonly CurrentWorkstationContext _currentWorkstation = new("DESK-01");
    private readonly FakeUnitOfWorkFactory _uowFactory;
    private readonly CheckInService _sut;

    private readonly Organization _org;
    private readonly AgeCategory _adultCategory;
    private readonly AgeCategory _childCategory;
    private readonly AgeCategory _infantCategory;
    private readonly Beneficiary _holder;

    public FreeCompanionLimitTests()
    {
        _org = new Organization("SOC", "جهة التجربة", cooldownMinutes: null, isDefault: true, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(_org, 1);
        _org.SetMaxFreeCompanions(3);
        _organizations.Add(_org);

        _adultCategory = new AgeCategory("بالغ", null, 1);
        typeof(AgeCategory).GetProperty(nameof(AgeCategory.Id))!.SetValue(_adultCategory, 1);

        _childCategory = new AgeCategory("طفل", null, 2);
        typeof(AgeCategory).GetProperty(nameof(AgeCategory.Id))!.SetValue(_childCategory, 2);

        _infantCategory = new AgeCategory("رضيع", null, 3);
        typeof(AgeCategory).GetProperty(nameof(AgeCategory.Id))!.SetValue(_infantCategory, 3);
        _infantCategory.SetCompanionLimitExempt(true);

        _categories.AddRange([_adultCategory, _childCategory, _infantCategory]);

        var workstation = new Workstation("DESK-01", "Desk 1", Domain.Enums.WorkstationType.Desk);
        typeof(Workstation).GetProperty(nameof(Workstation.Id))!.SetValue(workstation, 1);
        _workstations.Add(workstation);
        _currentWorkstation.SetWorkstation(workstation);

        var operatorUser = new User("op1", "Operator", "hash", Domain.Enums.UserRole.Operator);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(operatorUser, 1);
        var supervisorUser = new User("sup1", "Supervisor", "hash", Domain.Enums.UserRole.Supervisor);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(supervisorUser, 2);
        _users.AddRange([operatorUser, supervisorUser]);
        _currentUser.SetUser(operatorUser);

        _holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-1"), "Ali Salem", PhoneNumber.Create("0912345678"));
        _beneficiaries.Add(_holder);

        _uowFactory = new FakeUnitOfWorkFactory(
            _users, _workstations, _settings, _organizations, _beneficiaries, _attempts, _categories);

        _settings.Add(new Domain.Entities.AppSetting("Entry.DefaultCooldownMinutes", "720", DateTime.UtcNow, null));
        _settings.Add(new Domain.Entities.AppSetting("Entry.MaxGroupSize", "20", DateTime.UtcNow, null));
        _settings.Add(new Domain.Entities.AppSetting("Entry.DenialDedupMinutes", "5", DateTime.UtcNow, null));
        _settings.Add(new Domain.Entities.AppSetting("Park.TimeZoneId", "Libya Standard Time", DateTime.UtcNow, null));

        _sut = new CheckInService(
            _uowFactory,
            new RollingWindowEligibilityPolicy(),
            _currentUser,
            _currentWorkstation,
            new RecordEntryRequestValidator(),
            new OverrideEntryRequestValidator(),
            new VoidEntryRequestValidator());
    }

    [Fact]
    public async Task RecordEntryAsync_WhenLimitNull_AnyCompanionCountAllowed()
    {
        _org.SetMaxFreeCompanions(null);
        var companions = new List<CompanionRequestDto> { new(_adultCategory.Id, 5) };
        var result = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id, companions));

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task RecordEntryAsync_WhenLimit3AndThreeAdults_ShouldAllow()
    {
        var companions = new List<CompanionRequestDto> { new(_adultCategory.Id, 3) };
        var result = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id, companions));

        result.IsSuccess.ShouldBeTrue();
        _attempts.Count.ShouldBe(1);
    }

    [Fact]
    public async Task RecordEntryAsync_WhenLimit3AndFourAdults_ShouldRejectWithCompanionLimitError()
    {
        var companions = new List<CompanionRequestDto> { new(_adultCategory.Id, 4) };
        var result = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id, companions));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.CheckIn.ExceedsFreeCompanionLimit.Code);
        _attempts.Count.ShouldBe(0);
    }

    [Fact]
    public async Task RecordEntryAsync_WhenLimit3ThreeAdultsFiveInfants_ShouldAllow()
    {
        var companions = new List<CompanionRequestDto>
        {
            new(_adultCategory.Id, 3),
            new(_infantCategory.Id, 5)
        };
        var result = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id, companions));

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task RecordEntryAsync_WhenLimit0AndOneChild_ShouldReject()
    {
        _org.SetMaxFreeCompanions(0);
        var companions = new List<CompanionRequestDto> { new(_childCategory.Id, 1) };
        var result = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id, companions));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.CheckIn.ExceedsFreeCompanionLimit.Code);
    }

    [Fact]
    public async Task RecordEntryAsync_WhenLimit0AndOnlyInfants_ShouldAllow()
    {
        _org.SetMaxFreeCompanions(0);
        var companions = new List<CompanionRequestDto> { new(_infantCategory.Id, 5) };
        var result = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id, companions));

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task RecordEntryAsync_WhenExactlyAtLimit_ShouldAllow()
    {
        // Limit 3 with exactly 3 companions — holder not counted
        var companions = new List<CompanionRequestDto> { new(_adultCategory.Id, 3) };
        var result = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id, companions));

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task OverrideEntryAsync_WhenCompanionLimitExceeded_SupervisorCannotBypass()
    {
        var supervisorUser = _users.First(u => u.UserName == "sup1");
        _currentUser.SetUser(supervisorUser);

        var companions = new List<CompanionRequestDto> { new(_adultCategory.Id, 4) };
        var request = new OverrideEntryRequest(_holder.Id, "طلب استثنائي برر جيد", Companions: companions);
        var result = await _sut.OverrideEntryAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.CheckIn.ExceedsFreeCompanionLimit.Code);
        _attempts.Count.ShouldBe(0);
    }

    [Fact]
    public async Task RecordEntryAsync_WhenCompanionLimitExceeded_NoAttemptWritten()
    {
        var companions = new List<CompanionRequestDto> { new(_adultCategory.Id, 4) };
        await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id, companions));

        _attempts.Count.ShouldBe(0);
    }
}
