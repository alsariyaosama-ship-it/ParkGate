using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Validators;
using ParkGate.Application.Auth.DTOs;
using ParkGate.Application.Auth.Validators;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Validators;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.CheckIn.Validators;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Validators;
using ParkGate.Application.Settings.Models;
using ParkGate.Application.Settings.Validators;
using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Users.Validators;
using ParkGate.Application.Workstations.DTOs;
using ParkGate.Application.Workstations.Validators;
using ParkGate.Domain.Enums;
using Shouldly;
using Xunit;

namespace ParkGate.Application.Tests.Validation;

public sealed class ApplicationValidationTests
{
    [Theory]
    [InlineData("", "0912345678", "CARD1")] // empty name
    [InlineData("Ali", "12345", "CARD1")] // invalid phone (not Libyan 09XXXXXXXX)
    [InlineData("Ali", "0912345678", "")] // empty card
    public void RegisterBeneficiaryRequestValidator_WhenInvalid_ShouldFail(string fullName, string phone, string card)
    {
        var validator = new RegisterBeneficiaryRequestValidator();
        var request = new RegisterBeneficiaryRequest(
            OrganizationId: 1,
            CardNumber: card,
            FullName: fullName,
            Phone: phone,
            NationalId: null);

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void RegisterBeneficiaryRequestValidator_WhenNationalIdInvalid_ShouldFail()
    {
        var validator = new RegisterBeneficiaryRequestValidator();
        var request = new RegisterBeneficiaryRequest(
            OrganizationId: 1,
            CardNumber: "CARD1",
            FullName: "Ali Omar",
            Phone: "0912345678",
            NationalId: "12345"); // Not 12 digits

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RegisterBeneficiaryRequest.NationalId));
    }

    [Fact]
    public void ChangeBeneficiaryStatusRequestValidator_WhenReasonMissingOnSuspend_ShouldFail()
    {
        var validator = new ChangeBeneficiaryStatusRequestValidator();
        var request = new ChangeBeneficiaryStatusRequest(
            Id: Guid.NewGuid(),
            TargetStatus: BeneficiaryStatus.Suspended,
            Reason: "   "); // Empty reason

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ChangeBeneficiaryStatusRequest.Reason));
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("user", "")]
    public void LoginRequestValidator_WhenEmptyFields_ShouldFail(string userName, string password)
    {
        var validator = new LoginRequestValidator();
        var request = new LoginRequest(userName, password);

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void ChangePasswordRequestValidator_WhenMismatch_ShouldFail()
    {
        var validator = new ChangePasswordRequestValidator();
        var request = new ChangePasswordRequest(1, "currentPass1", "NewPass1234", "DifferentPass");

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ChangePasswordRequest.ConfirmNewPassword));
    }

    [Fact]
    public void ChangePasswordRequestValidator_WhenTooShort_ShouldFail()
    {
        var validator = new ChangePasswordRequestValidator();
        var request = new ChangePasswordRequest(1, "currentPass1", "short", "short");

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ChangePasswordRequest.NewPassword));
    }

    [Fact]
    public void RecordEntryRequestValidator_WhenBeneficiaryIdEmpty_ShouldFail()
    {
        var validator = new RecordEntryRequestValidator();
        var request = new RecordEntryRequest(Guid.Empty);

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void RecordEntryRequestValidator_WhenNegativeCompanions_ShouldFail()
    {
        var validator = new RecordEntryRequestValidator();
        var request = new RecordEntryRequest(
            Guid.NewGuid(),
            [new CompanionRequestDto(1, -2)]);

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void OverrideEntryRequestValidator_WhenReasonTooShort_ShouldFail()
    {
        var validator = new OverrideEntryRequestValidator();
        // BR-11: Reason must be at least 5 characters
        var request = new OverrideEntryRequest(
            BeneficiaryId: Guid.NewGuid(),
            Reason: "abc");

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(OverrideEntryRequest.Reason));
    }

    [Fact]
    public void VoidEntryRequestValidator_WhenReasonEmpty_ShouldFail()
    {
        var validator = new VoidEntryRequestValidator();
        var request = new VoidEntryRequest(Guid.NewGuid(), "   ");

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(VoidEntryRequest.Reason));
    }

    [Fact]
    public void CreateOrganizationRequestValidator_WhenInvalid_ShouldFail()
    {
        var validator = new CreateOrganizationRequestValidator();
        var request = new CreateOrganizationRequest("", "", -10);

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void CreateAgeCategoryRequestValidator_WhenSortOrderNegative_ShouldFail()
    {
        var validator = new CreateAgeCategoryRequestValidator();
        var request = new CreateAgeCategoryRequest("طفل", "وصف", -1);

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAgeCategoryRequest.SortOrder));
    }

    [Fact]
    public void CreateUserRequestValidator_WhenPasswordTooShort_ShouldFail()
    {
        var validator = new CreateUserRequestValidator();
        var request = new CreateUserRequest("newop", "New Operator", "123", UserRole.Operator);

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateUserRequest.Password));
    }

    [Fact]
    public void UpdateWorkstationRequestValidator_WhenDisplayNameEmpty_ShouldFail()
    {
        var validator = new UpdateWorkstationRequestValidator();
        var request = new UpdateWorkstationRequest(1, "", WorkstationType.Desk);

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateWorkstationRequest.DisplayName));
    }

    [Fact]
    public void UpdateSettingsRequestValidator_WhenValuesOutOfRange_ShouldFail()
    {
        var validator = new UpdateSettingsRequestValidator();
        var request = new UpdateSettingsRequest(
            DefaultCooldownMinutes: -1,
            MaxGroupSize: 0,
            DenialDedupMinutes: -1,
            PrefillCompanionsFromLastVisit: true,
            DenialSoundEnabled: true,
            MaxFailedLogins: 0,
            LockoutMinutes: 0,
            AutoLockMinutes: -1,
            ParkName: "",
            TimeZoneId: "");

        var result = validator.Validate(request);
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBeGreaterThanOrEqualTo(5);
    }
}
