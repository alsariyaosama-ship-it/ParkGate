using ParkGate.Application.Common;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Services;
using ParkGate.Application.Organizations.Validators;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.AgeCategories.Validators;
using ParkGate.Application.Security;
using ParkGate.Application.Tests.Common;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Application.Tests.Management;

public sealed class OrganizationPolicyTests
{
    private readonly List<Organization> _organizations = [];
    private readonly List<AgeCategory> _categories = [];
    private readonly CurrentUserContext _currentUser = new();
    private readonly CurrentWorkstationContext _workstation = new("WS-1");
    private readonly FakeUnitOfWorkFactory _uowFactory;
    private readonly OrganizationService _orgSut;
    private readonly AgeCategoryService _catSut;
    private readonly Organization _org;

    public OrganizationPolicyTests()
    {
        _org = new Organization("SSF", "صندوق التضامن", isDefault: true, isActive: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(_org, 1);
        _organizations.Add(_org);

        _uowFactory = new FakeUnitOfWorkFactory(
            organizations: _organizations,
            categories: _categories);

        var adminUser = new User("admin1", "Admin User", "hash", UserRole.Admin);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(adminUser, 1);
        _currentUser.SetUser(adminUser);

        _orgSut = new OrganizationService(
            _uowFactory,
            _currentUser,
            _workstation,
            new CreateOrganizationRequestValidator(),
            new UpdateOrganizationRequestValidator(),
            new SetOrganizationPolicyRequestValidator());

        _catSut = new AgeCategoryService(
            _uowFactory,
            _currentUser,
            _workstation,
            new CreateAgeCategoryRequestValidator(),
            new UpdateAgeCategoryRequestValidator());
    }

    [Fact]
    public async Task SetPolicyAsync_WithValidPolicy_ShouldUpdateOrgAndAudit()
    {
        var request = new SetOrganizationPolicyRequest(
            OrganizationId: 1,
            MaxFreeCompanions: 3,
            GiftEnabled: true,
            GiftValue: 50m,
            GiftIntervalDays: 30,
            AllowedEntryDays: WeekDays.All,
            GiftAnyDay: false);

        var result = await _orgSut.SetPolicyAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.MaxFreeCompanions.ShouldBe(3);
        result.Value.GiftEnabled.ShouldBeTrue();
        result.Value.GiftValue.ShouldBe(50m);
        result.Value.GiftIntervalDays.ShouldBe(30);
        _uowFactory.CurrentUow.AuditRowsAtLastSave.ShouldContain(w => w.Action == "Organization.PolicyChanged");
    }

    [Fact]
    public async Task SetPolicyAsync_WhenOperator_ShouldReturnForbidden()
    {
        var op = new User("op1", "Op", "hash", UserRole.Operator);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(op, 2);
        _currentUser.SetUser(op);

        var request = new SetOrganizationPolicyRequest(1, null, false, 0m, 30, WeekDays.All, false);
        var result = await _orgSut.SetPolicyAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
    }

    [Fact]
    public async Task SetPolicyAsync_WhenOrgNotFound_ShouldReturnNotFound()
    {
        var request = new SetOrganizationPolicyRequest(999, null, false, 0m, 30, WeekDays.All, false);
        var result = await _orgSut.SetPolicyAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.Organization.NotFound.Code);
    }

    [Fact]
    public async Task SetPolicyAsync_WhenGiftEnabledWithZeroValue_ShouldReturnValidationError()
    {
        var request = new SetOrganizationPolicyRequest(1, null, true, 0m, 30, WeekDays.All, false);
        var result = await _orgSut.SetPolicyAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("General.ValidationFailed");
        result.Error.ValidationErrors.ShouldNotBeNull();
        result.Error.ValidationErrors!.Any(e => e.Code == "GiftValueRequired").ShouldBeTrue();
    }

    [Fact]
    public async Task SetPolicyAsync_WhenIntervalOutOfRange_ShouldReturnValidationError()
    {
        var request = new SetOrganizationPolicyRequest(1, null, false, 0m, 0, WeekDays.All, false);
        var result = await _orgSut.SetPolicyAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("General.ValidationFailed");
        result.Error.ValidationErrors.ShouldNotBeNull();
        result.Error.ValidationErrors!.Any(e => e.Code == "GiftIntervalOutOfRange").ShouldBeTrue();
    }

    [Fact]
    public async Task SetPolicyAsync_WhenMaxCompanionsOutOfRange_ShouldReturnValidationError()
    {
        var request = new SetOrganizationPolicyRequest(1, 51, false, 0m, 30, WeekDays.All, false);
        var result = await _orgSut.SetPolicyAsync(request);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("General.ValidationFailed");
        result.Error.ValidationErrors.ShouldNotBeNull();
        result.Error.ValidationErrors!.Any(e => e.Code == "MaxFreeCompanionsOutOfRange").ShouldBeTrue();
    }

    [Fact]
    public async Task CreateAgeCategoryAsync_WithExemptFlag_ShouldPersistFlag()
    {
        var request = new CreateAgeCategoryRequest("رضيع", null, 1, IsCompanionLimitExempt: true);
        var result = await _catSut.CreateAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsCompanionLimitExempt.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateAgeCategoryAsync_WithExemptFlag_ShouldPersistFlag()
    {
        var cat = new AgeCategory("بالغ", null, 1);
        typeof(AgeCategory).GetProperty(nameof(AgeCategory.Id))!.SetValue(cat, 1);
        _categories.Add(cat);

        var request = new UpdateAgeCategoryRequest(1, "بالغ", null, 1, IsCompanionLimitExempt: true);
        var result = await _catSut.UpdateAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsCompanionLimitExempt.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateAgeCategoryAsync_ClearExemptFlag_ShouldPersistFalse()
    {
        var cat = new AgeCategory("بالغ", null, 1);
        typeof(AgeCategory).GetProperty(nameof(AgeCategory.Id))!.SetValue(cat, 1);
        cat.SetCompanionLimitExempt(true);
        _categories.Add(cat);

        var request = new UpdateAgeCategoryRequest(1, "بالغ", null, 1, IsCompanionLimitExempt: false);
        var result = await _catSut.UpdateAsync(request);

        result.IsSuccess.ShouldBeTrue();
        result.Value.IsCompanionLimitExempt.ShouldBeFalse();
    }

    // ── F8: entry days are saved and every change is traced ────────────────

    [Fact]
    public async Task SetPolicyAsync_EntryDays_SavesThemAndAuditsBeforeAndAfter()
    {
        _org.SetEntryDays(WeekDays.Saturday | WeekDays.Wednesday, giftAnyDay: false);

        var result = await _orgSut.SetPolicyAsync(new SetOrganizationPolicyRequest(1, null, false, 0m, 30, WeekDays.Sunday | WeekDays.Wednesday, true));

        result.IsSuccess.ShouldBeTrue();
        result.Value.AllowedEntryDays.ShouldBe(WeekDays.Sunday | WeekDays.Wednesday);
        result.Value.GiftAnyDay.ShouldBeTrue();
        _org.AllowedEntryDays.ShouldBe(WeekDays.Sunday | WeekDays.Wednesday);

        var audit = _uowFactory.CurrentUow.AuditRowsAtLastSave.Single(w => w.Action == "Organization.PolicyChanged");
        audit.UserId.ShouldBe(1);
        audit.DetailsJson.ShouldNotBeNull();
        audit.DetailsJson.ShouldContain("\"OldAllowedEntryDays\":72");   // Saturday + Wednesday
        audit.DetailsJson.ShouldContain("\"AllowedEntryDays\":9");       // Sunday + Wednesday
        audit.DetailsJson.ShouldContain("\"OldGiftAnyDay\":false");
        audit.DetailsJson.ShouldContain("\"GiftAnyDay\":true");
        audit.DetailsJson.ShouldContain("\"OrganizationCode\":\"SSF\"");
    }

    [Fact]
    public async Task SetPolicyAsync_NoEntryDay_ReturnsValidationErrorAndKeepsTheDays()
    {
        var result = await _orgSut.SetPolicyAsync(new SetOrganizationPolicyRequest(1, null, false, 0m, 30, WeekDays.None, false));

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("General.ValidationFailed");
        result.Error.ValidationErrors!.Any(e => e.Code == "EntryDaysRequired").ShouldBeTrue();
        _org.AllowedEntryDays.ShouldBe(WeekDays.All);
    }
}
