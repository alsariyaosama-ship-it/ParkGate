using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Auth.Services;
using ParkGate.Application.Ports.Persistence;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Services;

/// <summary>
/// Orchestrates application startup sequence (T4.1):
/// DB connectivity check -> Connection dialog (DPAPI) -> Pending migrations block -> First-run wizard -> Login -> Shell.
/// </summary>
public sealed class StartupCoordinator : IStartupCoordinator
{
    private readonly IDatabaseStartupService _startupService;
    private readonly IAuthService _authService;
    private readonly IFormFactory _formFactory;
    private readonly INotifyService _notifyService;
    private readonly ILogger<StartupCoordinator> _logger;

    public StartupCoordinator(
        IDatabaseStartupService startupService,
        IAuthService authService,
        IFormFactory formFactory,
        INotifyService notifyService,
        ILogger<StartupCoordinator> logger)
    {
        _startupService = startupService;
        _authService = authService;
        _formFactory = formFactory;
        _notifyService = notifyService;
        _logger = logger;
    }

    public async Task<StartupStatus> RunStartupSequenceAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Beginning application startup verification sequence.");

        // Step 1: Database connectivity check
        var connected = await _startupService.CanConnectAsync(cancellationToken: cancellationToken);
        while (!connected)
        {
            _logger.LogWarning("Unable to connect to database. Displaying connection settings dialog.");
            using var connForm = _formFactory.Create<ConnectionSettingsForm>();
            var dialogResult = connForm.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                _logger.LogInformation("Startup cancelled by user on connection settings dialog.");
                return StartupStatus.Aborted;
            }

            connected = await _startupService.CanConnectAsync(cancellationToken: cancellationToken);
        }

        _logger.LogInformation("Database connectivity verified successfully.");

        // Step 2: Pending migrations check (FR-AUTH-02, ADR-011)
        var pendingMigrations = await _startupService.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Count > 0)
        {
            _logger.LogError("Database has {Count} pending migrations. App cannot start until migrations are applied.", pendingMigrations.Count);
            var message = string.Format(Strings.Msg_PendingMigrationsBlock, pendingMigrations.Count);
            _notifyService.Error(message, Strings.Title_PendingMigrations);
            return StartupStatus.BlockedByMigrations;
        }

        _logger.LogInformation("Database schema verified up-to-date (no pending migrations).");

        // Step 2b: Ensure EntryCards schema exists
        await _startupService.EnsureEntryCardsSchemaAsync(cancellationToken);

        // Step 3: Workstation auto-registration (FR-AUTH-05)
        var wsResult = await _authService.EnsureWorkstationRegisteredAsync(Environment.MachineName, cancellationToken);
        if (wsResult.IsFailure)
        {
            _logger.LogError("Workstation registration failed: {Error}", wsResult.Error.Message);
            _notifyService.Error(ErrorMessages.GetMessage(wsResult.Error));
            return StartupStatus.Failed;
        }

        _logger.LogInformation("Workstation '{MachineName}' verified/registered.", Environment.MachineName);

        // Step 4: First-run admin wizard check (FR-AUTH-03, ADR-012)
        var isFirstRun = await _authService.IsFirstRunAsync(cancellationToken);
        if (isFirstRun)
        {
            _logger.LogInformation("First run detected: no users exist. Displaying first-run admin wizard.");
            using var firstRunForm = _formFactory.Create<FirstRunAdminForm>();
            var firstRunResult = firstRunForm.ShowDialog();

            if (firstRunResult != DialogResult.OK)
            {
                _logger.LogInformation("Startup cancelled by user on first-run wizard.");
                return StartupStatus.Aborted;
            }
        }

        // Step 5: User login dialog (FR-AUTH-04, BR-18)
        using var loginForm = _formFactory.Create<LoginForm>();
        var loginResult = loginForm.ShowDialog();

        if (loginResult != DialogResult.OK)
        {
            _logger.LogInformation("Startup cancelled by user on login form.");
            return StartupStatus.Aborted;
        }

        _logger.LogInformation("User login successful. Proceeding to main shell.");
        return StartupStatus.Success;
    }
}
