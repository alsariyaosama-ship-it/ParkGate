using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.CheckIn.Services;
using ParkGate.Application.CheckIn.Validators;
using ParkGate.Application.Security;
using ParkGate.Application.Tests.Common;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Application.Tests.CheckIn;

/// <summary>T10.1 / BR-33: what the entry receipt needs from a granted entry.</summary>
public sealed class EntryReceiptDataTests
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
    private readonly CheckInService _sut;

    private readonly AgeCategory _adult;
    private readonly AgeCategory _child;
    private readonly AgeCategory _infant;
    private readonly Beneficiary _holder;
    private readonly Beneficiary _otherHolder;

    public EntryReceiptDataTests()
    {
        var org = new Organization("SOC", "جهة التجربة", cooldownMinutes: null, isDefault: true, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(org, 1);
        _organizations.Add(org);

        // Added out of sort order on purpose: the result must come back in SortOrder.
        _infant = Category(3, "رضيع", 3, exempt: true);
        _adult = Category(1, "بالغ", 1);
        _child = Category(2, "طفل", 2);
        _categories.AddRange([_infant, _adult, _child]);

        var workstation = new Workstation("DESK-01", "Desk 1", Domain.Enums.WorkstationType.Desk);
        typeof(Workstation).GetProperty(nameof(Workstation.Id))!.SetValue(workstation, 1);
        _workstations.Add(workstation);
        _currentWorkstation.SetWorkstation(workstation);

        var op = new User("op1", "Operator", "hash", Domain.Enums.UserRole.Operator);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(op, 1);
        var sup = new User("sup1", "Supervisor", "hash", Domain.Enums.UserRole.Supervisor);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(sup, 2);
        _users.AddRange([op, sup]);
        _currentUser.SetUser(op);

        _holder = new Beneficiary(Guid.NewGuid(), org.Id, CardNumber.Create("CARD-1"), "Ali Salem", PhoneNumber.Create("0912345678"));
        _otherHolder = new Beneficiary(Guid.NewGuid(), org.Id, CardNumber.Create("CARD-2"), "Omar Salem", PhoneNumber.Create("0912345679"));
        _beneficiaries.AddRange([_holder, _otherHolder]);

        _settings.Add(new AppSetting("Entry.DefaultCooldownMinutes", "720", DateTime.UtcNow, null));
        _settings.Add(new AppSetting("Entry.MaxGroupSize", "20", DateTime.UtcNow, null));
        _settings.Add(new AppSetting("Entry.DenialDedupMinutes", "5", DateTime.UtcNow, null));
        _settings.Add(new AppSetting("Park.TimeZoneId", "Libya Standard Time", DateTime.UtcNow, null));

        _sut = new CheckInService(
            new FakeUnitOfWorkFactory(_users, _workstations, _settings, _organizations, _beneficiaries, _attempts, _categories),
            new RollingWindowEligibilityPolicy(),
            _currentUser,
            _currentWorkstation, TestSettings.Over(_settings), new SupervisorApprovalStore(),
            new RecordEntryRequestValidator(),
            new OverrideEntryRequestValidator(),
            new VoidEntryRequestValidator());
    }

    private static AgeCategory Category(int id, string name, int sortOrder, bool exempt = false)
    {
        var category = new AgeCategory(name, null, sortOrder);
        typeof(AgeCategory).GetProperty(nameof(AgeCategory.Id))!.SetValue(category, id);
        category.SetCompanionLimitExempt(exempt);
        return category;
    }

    [Fact]
    public async Task RecordEntryAsync_WithAdultChildAndInfant_CountsHolderAdultAndChildOnly()
    {
        var result = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id,
            [new(_infant.Id, 1), new(_child.Id, 1), new(_adult.Id, 1)]));

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalPersons.ShouldBe(4);
        result.Value.CountedPersons.ShouldBe(3);
    }

    [Fact]
    public async Task RecordEntryAsync_WithoutCompanions_CountsTheHolderOnly()
    {
        var result = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id));

        result.Value.CountedPersons.ShouldBe(1);
    }

    [Fact]
    public async Task RecordEntryAsync_WithInfantsOnly_CountsTheHolderOnly()
    {
        var result = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id, [new(_infant.Id, 3)]));

        result.Value.TotalPersons.ShouldBe(4);
        result.Value.CountedPersons.ShouldBe(1);
    }

    [Fact]
    public async Task RecordEntryAsync_Granted_FillsCategoryNamesInSortOrder()
    {
        var result = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id,
            [new(_infant.Id, 1), new(_child.Id, 2), new(_adult.Id, 1)]));

        result.Value.Companions.Select(c => c.CategoryName).ShouldBe(["بالغ", "طفل", "رضيع"]);
        result.Value.Companions.Select(c => c.Count).ShouldBe([1, 2, 1]);
    }

    [Fact]
    public async Task RecordEntryAsync_TwoGrantedEntries_ReturnIncreasingReceiptNumbers()
    {
        var first = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id));
        var second = await _sut.RecordEntryAsync(new RecordEntryRequest(_otherHolder.Id));

        first.Value.ReceiptNo.ShouldBeGreaterThan(0);
        second.Value.ReceiptNo.ShouldBe(first.Value.ReceiptNo + 1);
    }

    [Fact]
    public async Task OverrideEntryAsync_Granted_ReturnsReceiptNumberAndCounts()
    {
        var first = await _sut.RecordEntryAsync(new RecordEntryRequest(_holder.Id));
        _currentUser.SetUser(_users.First(u => u.UserName == "sup1"));

        var result = await _sut.OverrideEntryAsync(new OverrideEntryRequest(_holder.Id, "حالة خاصة جدا",
            Companions: [new(_adult.Id, 2), new(_infant.Id, 1)]));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ReceiptNo.ShouldBe(first.Value.ReceiptNo + 1);
        result.Value.CountedPersons.ShouldBe(3);
        result.Value.Companions.Select(c => c.CategoryName).ShouldBe(["بالغ", "رضيع"]);
    }
}

public sealed class EntryPersonCountingTests
{
    private static AgeCategory Category(int id, bool exempt)
    {
        var category = new AgeCategory($"cat{id}", null, id);
        typeof(AgeCategory).GetProperty(nameof(AgeCategory.Id))!.SetValue(category, id);
        category.SetCompanionLimitExempt(exempt);
        return category;
    }

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(2, 0, 3)]
    [InlineData(2, 5, 3)]
    [InlineData(0, 5, 1)]
    public void CountedPersons_ExcludesExemptCategories(int normal, int exempt, int expected)
    {
        var categories = new[] { Category(1, exempt: false), Category(2, exempt: true) };

        EntryPersonCounting.CountedPersons([new(1, normal), new(2, exempt)], categories).ShouldBe(expected);
    }

    [Fact]
    public void CountedPersons_UnknownCategory_Counts()
    {
        EntryPersonCounting.CountedPersons([new(99, 2)], [Category(1, exempt: true)]).ShouldBe(3);
    }

    [Fact]
    public void ToCompanionDtos_UnknownCategory_GoesLastWithEmptyName()
    {
        var dtos = EntryPersonCounting.ToCompanionDtos([new(99, 1), new(1, 1)], [Category(1, exempt: false)]);

        dtos.Select(d => d.AgeCategoryId).ShouldBe([1, 99]);
        dtos[1].CategoryName.ShouldBe(string.Empty);
    }
}
