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
    private readonly FakeAuditWriter _auditWriter = new();
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
            _auditWriter,
            _currentUser,
            _currentWorkstation,
            new RegisterBeneficiaryRequestValidator(),
            new UpdateBeneficiaryRequestValidator(),
            new ChangeBeneficiaryStatusRequestValidator());
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
        result.Value.Count.ShouldBe(1);
        var item = result.Value[0];
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
        result.Value.Count.ShouldBe(1);
        result.Value[0].Beneficiary.CardNumber.ShouldBe("12345");
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
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldContain(h => h.Beneficiary.OrganizationName == "صندوق التضامن");
        result.Value.ShouldContain(h => h.Beneficiary.OrganizationName == "التعليم");
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
        result.Value.NationalId.ShouldBe("119901234567");

        _beneficiaries.Count.ShouldBe(1);
        _auditWriter.Writes.ShouldContain(w => w.Action == "BeneficiaryRegistered");
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
        _auditWriter.Writes.ShouldContain(w => w.Action == "BeneficiaryUpdated");
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
        _auditWriter.Writes.ShouldContain(w => w.Action == "BeneficiaryStatusChanged");
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
}
