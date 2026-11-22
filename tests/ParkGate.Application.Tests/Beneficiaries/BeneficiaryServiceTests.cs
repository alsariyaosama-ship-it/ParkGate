using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.Beneficiaries.Validators;
using ParkGate.Application.Common;
using ParkGate.Application.Security;
using ParkGate.Application.Tests.Common;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Application.Tests.Beneficiaries;

public sealed class BeneficiaryServiceTests
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
    private readonly IEntryEligibilityPolicy _policy = new RollingWindowEligibilityPolicy();

    private readonly BeneficiaryService _sut;
    private readonly Organization _org;

    public BeneficiaryServiceTests()
    {
        _org = new Organization("SOC", "صندوق التضامن", cooldownMinutes: 720, isDefault: true, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(_org, 1);
        _organizations.Add(_org);

        _uowFactory = new FakeUnitOfWorkFactory(
            _users,
            _workstations,
            _settings,
            _organizations,
            _beneficiaries,
            _attempts,
            _categories);

        _currentUser.SetUser(1, "operator1", "Desk Operator", UserRole.Operator);

        _sut = new BeneficiaryService(
            _uowFactory,
            _policy,
            _currentUser,
            _currentWorkstation,
            new RegisterBeneficiaryRequestValidator(),
            new UpdateBeneficiaryRequestValidator(),
            new ChangeBeneficiaryStatusRequestValidator());
    }

    [Theory]
    [InlineData(630, 90)]  // 10 h 30 min since the last entry of a 12 h cooldown: 1 h 30 min left, not "1 hour"
    [InlineData(719, 1)]   // seconds-level remainders round up, never down to 0
    public async Task GetByIdAsync_DuringCooldown_ReturnsExactRemainingMinutesAndNextAllowedTime(int minutesSinceEntry, int expectedRemaining)
    {
        // R3.9: the preview carries the exact next allowed instant and remaining minutes; the UI formats them.
        var now = _uowFactory.CurrentUow.CurrentDbTime;
        var lastEntry = now.AddMinutes(-minutesSinceEntry).AddSeconds(-20);
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-WAIT"), "Ali Omar", PhoneNumber.Create("0912345678"));
        holder.RecordGrantedEntry(lastEntry);
        _beneficiaries.Add(holder);

        var result = await _sut.GetByIdAsync(holder.Id);

        result.IsSuccess.ShouldBeTrue();
        var eligibility = result.Value.Eligibility;
        eligibility.IsEligible.ShouldBeFalse();
        eligibility.NextAllowedEntryUtc.ShouldBe(lastEntry.AddMinutes(720));
        eligibility.RemainingCooldownMinutes.ShouldBe(expectedRemaining);
    }

    [Fact]
    public async Task LookupByCardNumberAsync_WhenCardNotFound_ShouldReturnError()
    {
        var result = await _sut.LookupByCardNumberAsync("999999");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Beneficiary.CardNotFound.Code);
    }

    [Fact]
    public async Task LookupByCardNumberAsync_WhenCardExists_ShouldReturnHolderAndEligibility()
    {
        var card = CardNumber.Create("CARD-100");
        var phone = PhoneNumber.Create("0912345678");
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, card, "Ali Omar", phone);
        _beneficiaries.Add(holder);

        var result = await _sut.LookupByCardNumberAsync("card-100");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Matches.Count.ShouldBe(1);
        result.Value.RegisteredElsewhere.ShouldBeEmpty();
        var item = result.Value.Matches[0];
        item.Beneficiary.FullName.ShouldBe("Ali Omar");
        item.Beneficiary.CardNumber.ShouldBe("CARD-100");
        item.Eligibility.IsEligible.ShouldBeTrue();
        item.Eligibility.Result.ShouldBe(EntryResult.Allowed);
    }

    [Fact]
    public async Task LookupByCardNumberAsync_WithArabicIndicDigits_ShouldNormalizeAndFindMatch()
    {
        var card = CardNumber.Create("12345");
        var phone = PhoneNumber.Create("0912345678");
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, card, "Ali Omar", phone);
        _beneficiaries.Add(holder);

        // Searching with Arabic-Indic "١٢٣٤٥"
        var result = await _sut.LookupByCardNumberAsync("١٢٣٤٥");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Matches.Count.ShouldBe(1);
        result.Value.Matches[0].Beneficiary.CardNumber.ShouldBe("12345");
    }

    [Fact]
    public async Task LookupByCardNumberAsync_AcrossMultipleOrganizations_ShouldReturnAllMatches()
    {
        var org2 = new Organization("EDU", "التعليم", cooldownMinutes: 720, isDefault: false, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(org2, 2);
        _organizations.Add(org2);

        var card = CardNumber.Create("CARD-SAME");
        var phone = PhoneNumber.Create("0912345678");
        var holder1 = new Beneficiary(Guid.NewGuid(), _org.Id, card, "Holder Org 1", phone);
        var holder2 = new Beneficiary(Guid.NewGuid(), org2.Id, card, "Holder Org 2", phone);
        _beneficiaries.AddRange([holder1, holder2]);

        var result = await _sut.LookupByCardNumberAsync("CARD-SAME");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Matches.Count.ShouldBe(2);
        result.Value.Matches.ShouldContain(h => h.Beneficiary.OrganizationName == "صندوق التضامن");
        result.Value.Matches.ShouldContain(h => h.Beneficiary.OrganizationName == "التعليم");
    }

    // ── R3.8: lookup within the selected organization, and where else the card is registered ──

    [Fact]
    public async Task LookupByCardNumberAsync_CardOnlyInAnotherOrganization_ReportsWhereItIsRegistered()
    {
        var org2 = new Organization("EDU", "التعليم", cooldownMinutes: 720, isDefault: false, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(org2, 2);
        _organizations.Add(org2);
        _beneficiaries.Add(new Beneficiary(Guid.NewGuid(), org2.Id, CardNumber.Create("CARD-ELSE"), "Holder Org 2", PhoneNumber.Create("0912345678")));

        var result = await _sut.LookupByCardNumberAsync("CARD-ELSE", organizationId: _org.Id);

        // Not "card not found": the desk can say "registered under «التعليم»" instead of offering a new registration.
        result.IsSuccess.ShouldBeTrue();
        result.Value.Matches.ShouldBeEmpty();
        result.Value.RegisteredElsewhere.ShouldHaveSingleItem();
        result.Value.RegisteredElsewhere[0].Id.ShouldBe(org2.Id);
        result.Value.RegisteredElsewhere[0].NameAr.ShouldBe("التعليم");
    }

    [Fact]
    public async Task LookupByCardNumberAsync_CardInSelectedAndAnotherOrganization_MatchesOnlyTheSelectedOne()
    {
        var org2 = new Organization("EDU", "التعليم", cooldownMinutes: 720, isDefault: false, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(org2, 2);
        _organizations.Add(org2);
        var card = CardNumber.Create("CARD-BOTH");
        _beneficiaries.AddRange([
            new Beneficiary(Guid.NewGuid(), _org.Id, card, "Holder Org 1", PhoneNumber.Create("0912345678")),
            new Beneficiary(Guid.NewGuid(), org2.Id, card, "Holder Org 2", PhoneNumber.Create("0912345678"))]);

        var result = await _sut.LookupByCardNumberAsync("CARD-BOTH", organizationId: _org.Id);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Matches.ShouldHaveSingleItem().Beneficiary.FullName.ShouldBe("Holder Org 1");
        result.Value.RegisteredElsewhere.ShouldHaveSingleItem().Id.ShouldBe(org2.Id);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateHolderAndAudit()
    {
        var request = new RegisterBeneficiaryRequest(
            _org.Id,
            "CARD-NEW",
            "Fatima Salem",
            "0919876543",
            "119901234567",
            new DateOnly(2027, 12, 31),
            "New VIP holder");

        var result = await _sut.RegisterAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.FullName.ShouldBe("Fatima Salem");
        result.Value.CardNumber.ShouldBe("CARD-NEW");
        result.Value.Phone.ShouldBe("0919876543");
        result.Value.NationalId.ShouldBe("********4567"); // operator view (R6.16)
        _beneficiaries.Single(x => x.Id == result.Value.Id).NationalId!.Value.ShouldBe("119901234567");

        _beneficiaries.Count.ShouldBe(1);
        _uowFactory.CurrentUow.AuditRowsAtLastSave.ShouldContain(w => w.Action == "BeneficiaryRegistered");
    }

    [Fact]
    public async Task RegisterAsync_WhenCardAlreadyExistsInOrg_ShouldReturnError()
    {
        var card = CardNumber.Create("CARD-EXISTING");
        var phone = PhoneNumber.Create("0912345678");
        _beneficiaries.Add(new Beneficiary(Guid.NewGuid(), _org.Id, card, "Existing", phone));

        var request = new RegisterBeneficiaryRequest(
            _org.Id,
            "CARD-EXISTING",
            "Duplicate Card",
            "0919999999");

        var result = await _sut.RegisterAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Beneficiary.CardAlreadyExists.Code);
    }

    [Fact]
    public async Task RegisterAsync_WhenNationalIdAlreadyExists_ShouldReturnError()
    {
        var card1 = CardNumber.Create("CARD-1");
        var phone = PhoneNumber.Create("0912345678");
        var natId = NationalId.Create("119901234567");
        _beneficiaries.Add(new Beneficiary(Guid.NewGuid(), _org.Id, card1, "Existing", phone, natId));

        var request = new RegisterBeneficiaryRequest(
            _org.Id,
            "CARD-2",
            "Another Holder",
            "0919999999",
            "119901234567");

        var result = await _sut.RegisterAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Beneficiary.NationalIdAlreadyExists.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserIsOperator_ShouldBeForbidden()
    {
        // Operator doesn't have EditBeneficiary
        var request = new UpdateBeneficiaryRequest(
            Guid.NewGuid(),
            _org.Id,
            "CARD-1",
            "Name",
            "0912345678");

        var result = await _sut.UpdateAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserIsSupervisor_ShouldSucceedAndAudit()
    {
        _currentUser.SetUser(2, "supervisor1", "Supervisor", UserRole.Supervisor);

        var card = CardNumber.Create("CARD-ORIGINAL");
        var phone = PhoneNumber.Create("0912345678");
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, card, "Original Name", phone);
        _beneficiaries.Add(holder);

        var request = new UpdateBeneficiaryRequest(
            holder.Id,
            _org.Id,
            "CARD-NEW",
            "Updated Name",
            "0919998877");

        var result = await _sut.UpdateAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.FullName.ShouldBe("Updated Name");
        result.Value.CardNumber.ShouldBe("CARD-NEW");
        _uowFactory.CurrentUow.AuditRowsAtLastSave.ShouldContain(w => w.Action == "BeneficiaryUpdated");
    }

    [Fact]
    public async Task UpdateAsync_WhenCardNumberChanges_ShouldAuditOldAndNewCardNumber()
    {
        _currentUser.SetUser(2, "supervisor1", "Supervisor", UserRole.Supervisor);
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-OLD"), "Name", PhoneNumber.Create("0912345678"));
        _beneficiaries.Add(holder);

        var result = await _sut.UpdateAsync(new UpdateBeneficiaryRequest(holder.Id, _org.Id, "CARD-NEW", "Name", "0912345678"));

        result.IsSuccess.ShouldBeTrue();
        var row = _uowFactory.CurrentUow.AuditRowsAtLastSave.Single(w => w.Action == "BeneficiaryUpdated");
        row.DetailsJson.ShouldBe("{\"OldCardNumber\":\"CARD-OLD\",\"NewCardNumber\":\"CARD-NEW\"}");
    }

    [Fact]
    public async Task UpdateAsync_WhenCardNumberIsUnchanged_ShouldAuditWithoutCardDetails()
    {
        _currentUser.SetUser(2, "supervisor1", "Supervisor", UserRole.Supervisor);
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-SAME"), "Name", PhoneNumber.Create("0912345678"));
        _beneficiaries.Add(holder);

        var result = await _sut.UpdateAsync(new UpdateBeneficiaryRequest(holder.Id, _org.Id, "CARD-SAME", "New Name", "0912345678"));

        result.IsSuccess.ShouldBeTrue();
        _uowFactory.CurrentUow.AuditRowsAtLastSave.Single(w => w.Action == "BeneficiaryUpdated").DetailsJson.ShouldBeNull();
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenSuspended_ShouldUpdateStatusAndAudit()
    {
        _currentUser.SetUser(2, "supervisor1", "Supervisor", UserRole.Supervisor);

        var card = CardNumber.Create("CARD-1");
        var phone = PhoneNumber.Create("0912345678");
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, card, "Test Holder", phone);
        _beneficiaries.Add(holder);

        var request = new ChangeBeneficiaryStatusRequest(
            holder.Id,
            BeneficiaryStatus.Suspended,
            "Suspicious multiple entries reported");

        var result = await _sut.ChangeStatusAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe(BeneficiaryStatus.Suspended);
        result.Value.StatusReason.ShouldBe("Suspicious multiple entries reported");
        _uowFactory.CurrentUow.AuditRowsAtLastSave.ShouldContain(w => w.Action == "BeneficiaryStatusChanged");
    }

    [Fact]
    public async Task SearchAsync_ShouldFindHoldersMatchingTerm()
    {
        var card = CardNumber.Create("CARD-XYZ");
        var phone = PhoneNumber.Create("0912345678");
        var holder1 = new Beneficiary(Guid.NewGuid(), _org.Id, card, "Mohamed Salem", phone);
        var holder2 = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-ABC"), "Ibrahim Salem", phone);
        _beneficiaries.AddRange([holder1, holder2]);

        var result = await _sut.SearchAsync("Salem");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
    }

    [Fact]
    public async Task SearchAsync_WhenSearchTermIsEmpty_ShouldReturnAllHolders()
    {
        var card = CardNumber.Create("CARD-XYZ");
        var phone = PhoneNumber.Create("0912345678");
        var holder1 = new Beneficiary(Guid.NewGuid(), _org.Id, card, "Mohamed Salem", phone);
        var holder2 = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-ABC"), "Ibrahim Salem", phone);
        _beneficiaries.AddRange([holder1, holder2]);

        var result = await _sut.SearchAsync(string.Empty);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
    }

    // ── F8, BR-27: the lookup says the day is wrong and which days are allowed ──

    [Fact]
    public async Task LookupByCardNumberAsync_NotAnEntryDay_PreviewNamesTheAllowedDays()
    {
        _org.SetEntryDays(WeekDays.Saturday | WeekDays.Thursday, giftAnyDay: false);
        _beneficiaries.Add(new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-DAY"), "Ali Omar", PhoneNumber.Create("0912345678")));

        var result = await _sut.LookupByCardNumberAsync("CARD-DAY");

        var eligibility = result.Value.Matches.Single().Eligibility;
        eligibility.IsEligible.ShouldBeFalse();
        eligibility.DenialReason.ShouldBe(DenialReason.DayNotAllowed);
        eligibility.CanBeOverridden.ShouldBeTrue();
        eligibility.AllowedEntryDays.ShouldBe(WeekDays.Saturday | WeekDays.Thursday);
    }

    // ── F9, BR-15 and BR-32: optional phone and city ──

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegisterAsync_WithoutPhone_ShouldSucceedWithNoPhone(string? phone)
    {
        var result = await _sut.RegisterAsync(new RegisterBeneficiaryRequest(_org.Id, "CARD-NOPHONE", "Salem Ali", phone));

        result.IsSuccess.ShouldBeTrue();
        result.Value.Phone.ShouldBeNull();
        _beneficiaries.Single().Phone.ShouldBeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidPhone_ShouldReturnInvalidPhoneValidationError()
    {
        var result = await _sut.RegisterAsync(new RegisterBeneficiaryRequest(_org.Id, "CARD-BADPHONE", "Salem Ali", "12345"));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.General.ValidationFailed([]).Code);
        result.Error.ValidationErrors!.ShouldContain(e => e.Code == "InvalidPhone");
        _beneficiaries.ShouldBeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_WithPhone_ShouldStillNormalizeIt()
    {
        var result = await _sut.RegisterAsync(new RegisterBeneficiaryRequest(_org.Id, "CARD-PHONE", "Salem Ali", "+218912345678"));

        result.IsSuccess.ShouldBeTrue();
        result.Value.Phone.ShouldBe("0912345678");
    }

    [Fact]
    public async Task RegisterAsync_WithUnknownCity_ShouldReturnCityNotFound()
    {
        var result = await _sut.RegisterAsync(new RegisterBeneficiaryRequest(_org.Id, "CARD-CITY", "Salem Ali", null, CityId: 99));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.City.NotFound.Code);
        _beneficiaries.ShouldBeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_WithInactiveCity_ShouldReturnCityNotFound()
    {
        var inactive = _uowFactory.CurrentUow.CitiesFake.Add("زليتن", 2, isActive: false);

        var result = await _sut.RegisterAsync(new RegisterBeneficiaryRequest(_org.Id, "CARD-CITY", "Salem Ali", null, CityId: inactive.Id));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.City.NotFound.Code);
    }

    [Fact]
    public async Task RegisterAsync_WithNonPositiveCity_ShouldReturnInvalidCityValidationError()
    {
        var result = await _sut.RegisterAsync(new RegisterBeneficiaryRequest(_org.Id, "CARD-CITY", "Salem Ali", null, CityId: 0));

        result.IsFailure.ShouldBeTrue();
        result.Error.ValidationErrors!.ShouldContain(e => e.Code == "InvalidCity");
    }

    [Fact]
    public async Task RegisterAsync_WithValidCity_ShouldReturnCityIdAndName()
    {
        var city = _uowFactory.CurrentUow.CitiesFake.Add("الخمس", 1);

        var result = await _sut.RegisterAsync(new RegisterBeneficiaryRequest(_org.Id, "CARD-CITY", "Salem Ali", null, CityId: city.Id));

        result.IsSuccess.ShouldBeTrue();
        result.Value.CityId.ShouldBe(city.Id);
        result.Value.CityName.ShouldBe("الخمس");
        _beneficiaries.Single().CityId.ShouldBe(city.Id);
    }

    [Fact]
    public async Task UpdateAsync_WithNoPhoneAndNoCity_ShouldClearBoth()
    {
        _currentUser.SetUser(2, "supervisor1", "Supervisor", UserRole.Supervisor);
        var city = _uowFactory.CurrentUow.CitiesFake.Add("الخمس", 1);
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-CLR"), "Name", PhoneNumber.Create("0912345678"), cityId: city.Id);
        _beneficiaries.Add(holder);

        var result = await _sut.UpdateAsync(new UpdateBeneficiaryRequest(holder.Id, _org.Id, "CARD-CLR", "Name", null));

        result.IsSuccess.ShouldBeTrue();
        result.Value.Phone.ShouldBeNull();
        result.Value.CityId.ShouldBeNull();
        result.Value.CityName.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateAsync_KeepingAnInactiveCurrentCity_ShouldSucceed()
    {
        _currentUser.SetUser(2, "supervisor1", "Supervisor", UserRole.Supervisor);
        var inactive = _uowFactory.CurrentUow.CitiesFake.Add("زليتن", 2, isActive: false);
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-KEEP"), "Name", null, cityId: inactive.Id);
        _beneficiaries.Add(holder);

        var result = await _sut.UpdateAsync(new UpdateBeneficiaryRequest(holder.Id, _org.Id, "CARD-KEEP", "New Name", null, CityId: inactive.Id));

        result.IsSuccess.ShouldBeTrue();
        result.Value.CityId.ShouldBe(inactive.Id);
        result.Value.CityName.ShouldBe("زليتن");
    }

    [Fact]
    public async Task UpdateAsync_SwitchingToAnInactiveCity_ShouldReturnCityNotFound()
    {
        _currentUser.SetUser(2, "supervisor1", "Supervisor", UserRole.Supervisor);
        var active = _uowFactory.CurrentUow.CitiesFake.Add("الخمس", 1);
        var inactive = _uowFactory.CurrentUow.CitiesFake.Add("زليتن", 2, isActive: false);
        var holder = new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-SW"), "Name", null, cityId: active.Id);
        _beneficiaries.Add(holder);

        var result = await _sut.UpdateAsync(new UpdateBeneficiaryRequest(holder.Id, _org.Id, "CARD-SW", "Name", null, CityId: inactive.Id));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.City.NotFound.Code);
        holder.CityId.ShouldBe(active.Id);
    }

    [Fact]
    public async Task SearchAsync_HolderWithCity_ShouldCarryTheCityName()
    {
        var city = _uowFactory.CurrentUow.CitiesFake.Add("مصراتة", 3);
        _beneficiaries.Add(new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-S1"), "Mohamed Salem", null, cityId: city.Id));
        _beneficiaries.Add(new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-S2"), "Mohamed Omar", null));

        var result = await _sut.SearchAsync("Mohamed");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Single(b => b.CardNumber == "CARD-S1").CityName.ShouldBe("مصراتة");
        result.Value.Single(b => b.CardNumber == "CARD-S2").CityName.ShouldBeNull();
    }

    [Fact]
    public async Task LookupByCardNumberAsync_HolderWithCity_ShouldCarryTheCityName()
    {
        var city = _uowFactory.CurrentUow.CitiesFake.Add("مصراتة", 3);
        _beneficiaries.Add(new Beneficiary(Guid.NewGuid(), _org.Id, CardNumber.Create("CARD-L1"), "Mohamed Salem", null, cityId: city.Id));

        var result = await _sut.LookupByCardNumberAsync("CARD-L1");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Matches.Single().Beneficiary.CityName.ShouldBe("مصراتة");
        result.Value.Matches.Single().Beneficiary.Phone.ShouldBeNull();
    }
}
