using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParkGate.Application;
using ParkGate.Application.EntryCards;
using ParkGate.Application.Ports.EntryCards;
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

        // 2. Application layer (Domain policies, security contexts, use cases, validators)
        services.AddApplication();

        // 3. EntryCards runtime infrastructure & asynchronously initialized QR crypto provider
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(new GateStation(Environment.MachineName));
        services.AddSingleton(new EntryCardOptions { ValidityDays = 365 });
        services.AddSingleton(sp => sp.GetRequiredService<IQrCryptoProvider>().Verifier
            ?? throw new InvalidOperationException("QrTokenVerifier is not initialized. Ensure StartupCoordinator has executed."));
        services.AddSingleton(sp => sp.GetRequiredService<IQrCryptoProvider>().Signer
            ?? throw new InvalidOperationException("QrTokenSigner is not initialized. Ensure StartupCoordinator has executed."));

        // 4. WinForms presentation services & forms (Transient per AGENTS.md §4.5)
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
        services.AddTransient<Controls.BeneficiariesControl>();
        services.AddTransient<EditBeneficiaryDialog>();
        services.AddTransient<ChangeBeneficiaryStatusDialog>();
        services.AddTransient<VoidReasonDialog>();
        services.AddTransient<OrganizationsForm>();
        services.AddTransient<EditOrganizationDialog>();
        services.AddTransient<AgeCategoriesForm>();
        services.AddTransient<EditAgeCategoryDialog>();
        services.AddTransient<WorkstationsForm>();
        services.AddTransient<EditWorkstationDialog>();
        services.AddTransient<UsersForm>();
        services.AddTransient<CreateUserDialog>();
        services.AddTransient<ChangeUserRoleDialog>();
        services.AddTransient<ResetUserPasswordDialog>();
        services.AddTransient<SettingsForm>();
        services.AddTransient<Controls.ReportsControl>();
        services.AddTransient<Forms.IssueQrDialog>();
        services.AddTransient<Controls.GiftOfferPanel>();
        services.AddTransient<GiftQrDialog>();
        services.AddTransient<Controls.GiftRedeemControl>();
        services.AddTransient<GiftReceiptsForm>();

        return services;
    }
}
