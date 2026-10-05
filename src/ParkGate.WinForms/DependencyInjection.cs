using FluentValidation;
using Microsoft.Extensions.Configuration;
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
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Services;
using ParkGate.Application.Organizations.Validators;
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
using ParkGate.Infrastructure;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms;

public static class DependencyInjection
{
    public static IServiceCollection AddParkGateServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Infrastructure layer
        services.AddInfrastructure(configuration);

        // 2. Domain policies
        services.AddSingleton<IEntryEligibilityPolicy, RollingWindowEligibilityPolicy>();

        // 3. Security contexts (Singletons)
        var userContext = new CurrentUserContext();
        services.AddSingleton<CurrentUserContext>(userContext);
        services.AddSingleton<ICurrentUser>(userContext);
        services.AddSingleton<ICurrentUserSession>(userContext);

        var workstationContext = new CurrentWorkstationContext();
        services.AddSingleton<CurrentWorkstationContext>(workstationContext);
        services.AddSingleton<ICurrentWorkstation>(workstationContext);
        services.AddSingleton<ICurrentWorkstationSession>(workstationContext);

        // 4. Application services (AGENTS.md §4.5)
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IBeneficiaryService, BeneficiaryService>();
        services.AddTransient<ICheckInService, CheckInService>();
        services.AddTransient<IOrganizationService, OrganizationService>();
        services.AddTransient<IAgeCategoryService, AgeCategoryService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IWorkstationService, WorkstationService>();

        // 5. FluentValidation validators
        services.AddTransient<IValidator<FirstRunAdminRequest>, FirstRunAdminRequestValidator>();
        services.AddTransient<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddTransient<IValidator<ChangePasswordRequest>, ChangePasswordRequestValidator>();
        services.AddTransient<IValidator<RegisterBeneficiaryRequest>, RegisterBeneficiaryRequestValidator>();
        services.AddTransient<IValidator<UpdateBeneficiaryRequest>, UpdateBeneficiaryRequestValidator>();
        services.AddTransient<IValidator<ChangeBeneficiaryStatusRequest>, ChangeBeneficiaryStatusRequestValidator>();
        services.AddTransient<IValidator<RecordEntryRequest>, RecordEntryRequestValidator>();
        services.AddTransient<IValidator<OverrideEntryRequest>, OverrideEntryRequestValidator>();
        services.AddTransient<IValidator<VoidEntryRequest>, VoidEntryRequestValidator>();
        services.AddTransient<IValidator<CreateOrganizationRequest>, CreateOrganizationRequestValidator>();
        services.AddTransient<IValidator<UpdateOrganizationRequest>, UpdateOrganizationRequestValidator>();
        services.AddTransient<IValidator<CreateAgeCategoryRequest>, CreateAgeCategoryRequestValidator>();
        services.AddTransient<IValidator<UpdateAgeCategoryRequest>, UpdateAgeCategoryRequestValidator>();
        services.AddTransient<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        services.AddTransient<IValidator<UpdateWorkstationRequest>, UpdateWorkstationRequestValidator>();
        services.AddTransient<IValidator<UpdateSettingsRequest>, UpdateSettingsRequestValidator>();

        // 6. WinForms presentation services & forms (Transient per AGENTS.md §4.5)
        services.AddSingleton<INotifyService, NotifyService>();
        services.AddSingleton<IFormFactory, FormFactory>();
        services.AddTransient<IStartupCoordinator, StartupCoordinator>();

        services.AddTransient<MainForm>();
        services.AddTransient<ConnectionSettingsForm>();
        services.AddTransient<FirstRunAdminForm>();
        services.AddTransient<LoginForm>();
        services.AddTransient<ChangePasswordForm>();
        services.AddTransient<SupervisorAuthDialog>();
        services.AddTransient<Controls.CheckInDeskControl>();

        return services;
    }
}
