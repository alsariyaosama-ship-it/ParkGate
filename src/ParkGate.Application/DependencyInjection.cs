using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.Application.Auth.Services;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.CheckIn.Services;
using ParkGate.Application.EntryCards;
using ParkGate.Application.GiftVouchers.Services;
using ParkGate.Application.Organizations.Services;
using ParkGate.Application.Reports.Services;
using ParkGate.Application.Security;
using ParkGate.Application.Settings.Services;
using ParkGate.Application.Sync;
using ParkGate.Application.Users.Services;
using ParkGate.Application.Workstations.Services;
using ParkGate.Domain.Policies;

namespace ParkGate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 1. Domain policies
        services.AddSingleton<IEntryEligibilityPolicy, RollingWindowEligibilityPolicy>();
        services.AddSingleton<IGiftEligibilityPolicy, RollingIntervalGiftEligibilityPolicy>();

        // 2. Security contexts
        services.AddSingleton<CurrentUserContext>();
        services.AddSingleton<ICurrentUser>(sp => sp.GetRequiredService<CurrentUserContext>());
        services.AddSingleton<ICurrentUserSession>(sp => sp.GetRequiredService<CurrentUserContext>());

        services.AddSingleton<CurrentWorkstationContext>();
        services.AddSingleton<ICurrentWorkstation>(sp => sp.GetRequiredService<CurrentWorkstationContext>());
        services.AddSingleton<ICurrentWorkstationSession>(sp => sp.GetRequiredService<CurrentWorkstationContext>());

        // 3. Application services
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IBeneficiaryService, BeneficiaryService>();
        services.AddTransient<ICheckInService, CheckInService>();
        services.AddTransient<IOrganizationService, OrganizationService>();
        services.AddTransient<IAgeCategoryService, AgeCategoryService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IWorkstationService, WorkstationService>();
        services.AddTransient<IReportService, ReportService>();
        services.AddTransient<IGiftVoucherService, GiftVoucherService>();
        services.AddTransient<IGiftReportService, GiftReportService>();
        services.AddTransient<ISyncStatusService, SyncStatusService>();

        // 4. EntryCards services
        services.AddTransient<CardScanService>();
        services.AddTransient<EntryCardIssuer>();

        // 5. Automatic FluentValidation validators registration via assembly scanning
        var validatorInterface = typeof(IValidator<>);
        var validatorTypes = typeof(AssemblyReference).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var type in validatorTypes)
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == validatorInterface)
                {
                    services.AddTransient(iface, type);
                }
            }
        }

        return services;
    }
}
