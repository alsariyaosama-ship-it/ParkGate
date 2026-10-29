using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.AgeCategories.Validators;
using ParkGate.Application.Auth.DTOs;
using ParkGate.Application.Auth.Services;
using ParkGate.Application.Auth.Validators;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.Beneficiaries.Validators;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.CheckIn.Services;
using ParkGate.Application.CheckIn.Validators;
using ParkGate.Application.EntryCards;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Services;
using ParkGate.Application.Organizations.Validators;
using ParkGate.Application.Reports.Services;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Models;
using ParkGate.Application.Settings.Services;
using ParkGate.Application.Settings.Validators;
using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Users.Services;
using ParkGate.Application.Users.Validators;
using ParkGate.Application.Workstations.DTOs;
using ParkGate.Application.Workstations.Services;
using ParkGate.Application.Workstations.Validators;
using ParkGate.Domain.Policies;
using Shouldly;

namespace ParkGate.Application.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ShouldRegisterDomainPoliciesAndSecurityContexts()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        services.ShouldContain(sd => sd.ServiceType == typeof(IEntryEligibilityPolicy) &&
                                     sd.ImplementationType == typeof(RollingWindowEligibilityPolicy) &&
                                     sd.Lifetime == ServiceLifetime.Singleton);

        services.ShouldContain(sd => sd.ServiceType == typeof(CurrentUserContext) &&
                                     sd.Lifetime == ServiceLifetime.Singleton);
        services.ShouldContain(sd => sd.ServiceType == typeof(ICurrentUser) &&
                                     sd.Lifetime == ServiceLifetime.Singleton);
        services.ShouldContain(sd => sd.ServiceType == typeof(ICurrentUserSession) &&
                                     sd.Lifetime == ServiceLifetime.Singleton);

        services.ShouldContain(sd => sd.ServiceType == typeof(CurrentWorkstationContext) &&
                                     sd.Lifetime == ServiceLifetime.Singleton);
        services.ShouldContain(sd => sd.ServiceType == typeof(ICurrentWorkstation) &&
                                     sd.Lifetime == ServiceLifetime.Singleton);
        services.ShouldContain(sd => sd.ServiceType == typeof(ICurrentWorkstationSession) &&
                                     sd.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddApplication_ShouldRegisterApplicationServices()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        services.ShouldContain(sd => sd.ServiceType == typeof(ISettingsService) &&
                                     sd.ImplementationType == typeof(SettingsService) &&
                                     sd.Lifetime == ServiceLifetime.Singleton);

        services.ShouldContain(sd => sd.ServiceType == typeof(IAuthService) &&
                                     sd.ImplementationType == typeof(AuthService) &&
                                     sd.Lifetime == ServiceLifetime.Transient);

        services.ShouldContain(sd => sd.ServiceType == typeof(IBeneficiaryService) &&
                                     sd.ImplementationType == typeof(BeneficiaryService) &&
                                     sd.Lifetime == ServiceLifetime.Transient);

        services.ShouldContain(sd => sd.ServiceType == typeof(ICheckInService) &&
                                     sd.ImplementationType == typeof(CheckInService) &&
                                     sd.Lifetime == ServiceLifetime.Transient);

        services.ShouldContain(sd => sd.ServiceType == typeof(IOrganizationService) &&
                                     sd.ImplementationType == typeof(OrganizationService) &&
                                     sd.Lifetime == ServiceLifetime.Transient);

        services.ShouldContain(sd => sd.ServiceType == typeof(IAgeCategoryService) &&
                                     sd.ImplementationType == typeof(AgeCategoryService) &&
                                     sd.Lifetime == ServiceLifetime.Transient);

        services.ShouldContain(sd => sd.ServiceType == typeof(IUserService) &&
                                     sd.ImplementationType == typeof(UserService) &&
                                     sd.Lifetime == ServiceLifetime.Transient);

        services.ShouldContain(sd => sd.ServiceType == typeof(IWorkstationService) &&
                                     sd.ImplementationType == typeof(WorkstationService) &&
                                     sd.Lifetime == ServiceLifetime.Transient);

        services.ShouldContain(sd => sd.ServiceType == typeof(IReportService) &&
                                     sd.ImplementationType == typeof(ReportService) &&
                                     sd.Lifetime == ServiceLifetime.Transient);

        services.ShouldContain(sd => sd.ServiceType == typeof(CardScanService) &&
                                     sd.ImplementationType == typeof(CardScanService) &&
                                     sd.Lifetime == ServiceLifetime.Transient);

        services.ShouldContain(sd => sd.ServiceType == typeof(EntryCardIssuer) &&
                                     sd.ImplementationType == typeof(EntryCardIssuer) &&
                                     sd.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void AddApplication_ShouldRegisterAllFluentValidationValidatorsViaScanning()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<FirstRunAdminRequest>) &&
                                     sd.ImplementationType == typeof(FirstRunAdminRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<LoginRequest>) &&
                                     sd.ImplementationType == typeof(LoginRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<ChangePasswordRequest>) &&
                                     sd.ImplementationType == typeof(ChangePasswordRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<RegisterBeneficiaryRequest>) &&
                                     sd.ImplementationType == typeof(RegisterBeneficiaryRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<UpdateBeneficiaryRequest>) &&
                                     sd.ImplementationType == typeof(UpdateBeneficiaryRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<ChangeBeneficiaryStatusRequest>) &&
                                     sd.ImplementationType == typeof(ChangeBeneficiaryStatusRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<RecordEntryRequest>) &&
                                     sd.ImplementationType == typeof(RecordEntryRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<OverrideEntryRequest>) &&
                                     sd.ImplementationType == typeof(OverrideEntryRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<VoidEntryRequest>) &&
                                     sd.ImplementationType == typeof(VoidEntryRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<CreateOrganizationRequest>) &&
                                     sd.ImplementationType == typeof(CreateOrganizationRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<UpdateOrganizationRequest>) &&
                                     sd.ImplementationType == typeof(UpdateOrganizationRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<CreateAgeCategoryRequest>) &&
                                     sd.ImplementationType == typeof(CreateAgeCategoryRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<UpdateAgeCategoryRequest>) &&
                                     sd.ImplementationType == typeof(UpdateAgeCategoryRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<CreateUserRequest>) &&
                                     sd.ImplementationType == typeof(CreateUserRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<UpdateWorkstationRequest>) &&
                                     sd.ImplementationType == typeof(UpdateWorkstationRequestValidator));
        services.ShouldContain(sd => sd.ServiceType == typeof(IValidator<UpdateSettingsRequest>) &&
                                     sd.ImplementationType == typeof(UpdateSettingsRequestValidator));
    }

    [Fact]
    public void AddApplication_AllRegisteredValidators_ShouldBeAssignableToIValidator()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        var validatorDescriptors = services
            .Where(sd => sd.ServiceType.IsGenericType &&
                         sd.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>))
            .ToList();

        // Exactly 17 request validators in Application (16 original + SetOrganizationPolicyRequestValidator)
        validatorDescriptors.Count.ShouldBe(17);

        foreach (var desc in validatorDescriptors)
        {
            desc.ImplementationType.ShouldNotBeNull();
            desc.ServiceType.IsAssignableFrom(desc.ImplementationType).ShouldBeTrue();
            desc.Lifetime.ShouldBe(ServiceLifetime.Transient);
        }
    }
}
