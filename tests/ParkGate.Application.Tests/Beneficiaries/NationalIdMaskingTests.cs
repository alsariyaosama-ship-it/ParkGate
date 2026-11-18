using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.Beneficiaries.Validators;
using ParkGate.Application.Security;
using ParkGate.Application.Tests.Common;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace ParkGate.Application.Tests.Beneficiaries;

/// <summary>
/// SPEC §8 / R6.16: operators see only the last four digits of a national ID. The masking happens in
/// Application, so the full number never reaches an operator's screen.
/// </summary>
public sealed class NationalIdMaskingTests
{
    private const string FullId = "119900123456";
    private const string MaskedId = "********3456";

    private readonly List<Beneficiary> _beneficiaries = [];
    private readonly CurrentUserContext _user = new();
    private readonly FakeUnitOfWorkFactory _uowFactory;
    private readonly BeneficiaryService _sut;
    private readonly Beneficiary _holder;

    public NationalIdMaskingTests()
    {
        var org = new Organization("SOC", "صندوق التضامن", cooldownMinutes: 720, isDefault: true, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(org, 1);

        _holder = new Beneficiary(Guid.NewGuid(), 1, CardNumber.Create("CARD-NID"), "Ali Omar",
            PhoneNumber.Create("0912345678"), NationalId.Create(FullId));
        _beneficiaries.Add(_holder);

        _uowFactory = new FakeUnitOfWorkFactory(organizations: [org], beneficiaries: _beneficiaries);
        _sut = new BeneficiaryService(
            _uowFactory,
            new RollingWindowEligibilityPolicy(),
            _user,
            new CurrentWorkstationContext("DESK-01"),
            new RegisterBeneficiaryRequestValidator(),
            new UpdateBeneficiaryRequestValidator(),
            new ChangeBeneficiaryStatusRequestValidator());
    }

    [Theory]
    [InlineData(UserRole.Operator, MaskedId)]
    [InlineData(UserRole.Supervisor, FullId)]
    [InlineData(UserRole.Admin, FullId)]
    public async Task LookupByCardNumberAsync_ShowsTheFullIdOnlyToSupervisorsAndAdmins(UserRole role, string expected)
    {
        _user.SetUser(1, "user", "User", role);

        var result = await _sut.LookupByCardNumberAsync("CARD-NID");

        result.Value.Matches.Single().Beneficiary.NationalId.ShouldBe(expected);
    }

    [Fact]
    public async Task GetByIdAndSearch_ForOperator_ReturnMaskedId()
    {
        _user.SetUser(1, "operator", "Operator", UserRole.Operator);

        (await _sut.GetByIdAsync(_holder.Id)).Value.Beneficiary.NationalId.ShouldBe(MaskedId);
        (await _sut.SearchAsync("Ali")).Value.Single().NationalId.ShouldBe(MaskedId);
    }

    [Fact]
    public async Task RegisterAsync_ForOperator_ReturnsMaskedIdOfTheNewHolder()
    {
        _user.SetUser(1, "operator", "Operator", UserRole.Operator);

        var result = await _sut.RegisterAsync(new RegisterBeneficiaryRequest(1, "CARD-NEW", "Sara Ali", "0923456789", "119900654321"));

        result.Value.NationalId.ShouldBe("********4321");
    }

    [Fact]
    public async Task LookupByCardNumberAsync_ForOperator_KeepsAHolderWithoutIdNull()
    {
        _user.SetUser(1, "operator", "Operator", UserRole.Operator);
        _beneficiaries.Add(new Beneficiary(Guid.NewGuid(), 1, CardNumber.Create("CARD-NOID"), "No Id", PhoneNumber.Create("0912345679")));

        var result = await _sut.LookupByCardNumberAsync("CARD-NOID");

        result.Value.Matches.Single().Beneficiary.NationalId.ShouldBeNull();
    }
}
