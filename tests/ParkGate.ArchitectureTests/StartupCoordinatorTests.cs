using System.Windows.Forms;
using Microsoft.Extensions.Logging.Abstractions;
using ParkGate.Application.Auth.DTOs;
using ParkGate.Application.Auth.Services;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Workstations.DTOs;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Services;

namespace ParkGate.ArchitectureTests;

public sealed class StartupCoordinatorTests
{
    [Fact]
    public async Task RunStartupSequenceAsync_ShouldBlock_WhenPendingMigrationsExist()
    {
        var dbService = new FakeDatabaseStartupService
        {
            CanConnect = true,
            PendingMigrations = new List<string> { "20261005_AddInitialTables" }
        };
        var authService = new FakeAuthService();
        var formFactory = new FakeFormFactory();
        var notifyService = new FakeNotifyService();
        var coordinator = new StartupCoordinator(dbService, authService, formFactory, notifyService, NullLogger<StartupCoordinator>.Instance);

        var status = await coordinator.RunStartupSequenceAsync();

        status.ShouldBe(StartupStatus.BlockedByMigrations);
        notifyService.LastErrorMessage.ShouldNotBeNull();
    }

    [Fact]
    public async Task RunStartupSequenceAsync_ShouldFail_WhenWorkstationRegistrationFails()
    {
        var dbService = new FakeDatabaseStartupService
        {
            CanConnect = true,
            PendingMigrations = new List<string>()
        };
        var authService = new FakeAuthService
        {
            WorkstationResult = Errors.Auth.WorkstationInactive
        };
        var formFactory = new FakeFormFactory();
        var notifyService = new FakeNotifyService();
        var coordinator = new StartupCoordinator(dbService, authService, formFactory, notifyService, NullLogger<StartupCoordinator>.Instance);

        var status = await coordinator.RunStartupSequenceAsync();

        status.ShouldBe(StartupStatus.Failed);
        notifyService.LastErrorMessage.ShouldNotBeNull();
    }

    private sealed class FakeNotifyService : INotifyService
    {
        public string? LastErrorMessage { get; private set; }
        public string? LastWarningMessage { get; private set; }
        public string? LastInfoMessage { get; private set; }

        public void Info(string message, string? title = null) => LastInfoMessage = message;
        public void Warning(string message, string? title = null) => LastWarningMessage = message;
        public void Error(string message, string? title = null) => LastErrorMessage = message;
        public bool Confirm(string message, string? title = null) => true;
    }

    private sealed class FakeDatabaseStartupService : IDatabaseStartupService
    {
        public bool CanConnect { get; set; } = true;
        public IReadOnlyList<string> PendingMigrations { get; set; } = Array.Empty<string>();

        public Task<bool> CanConnectAsync(string? connectionString = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CanConnect);
        }

        public Task<IReadOnlyList<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PendingMigrations);
        }
    }

    private sealed class FakeAuthService : IAuthService
    {
        public bool IsFirstRun { get; set; }
        public Result<WorkstationDto> WorkstationResult { get; set; } =
            new WorkstationDto(1, "DESK-01", "DESK-01", WorkstationType.Desk, true, DateTime.UtcNow);

        public Task<bool> IsFirstRunAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(IsFirstRun);
        }

        public Task<Result<UserDto>> CompleteFirstRunAsync(FirstRunAdminRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<UserDto>.Success(new UserDto(1, "admin", "Admin User", UserRole.Admin, true, false, null)));
        }

        public Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = new UserDto(1, "admin", "Admin User", UserRole.Admin, true, false, null);
            var workstation = new WorkstationDto(1, "DESK-01", "DESK-01", WorkstationType.Desk, true, DateTime.UtcNow);
            return Task.FromResult(Result<LoginResponse>.Success(new LoginResponse(user, workstation, false)));
        }

        public Task<Result> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Success());
        }

        public Task<Result<UserDto>> ValidateSupervisorCredentialsAsync(string userName, string password, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<UserDto>.Success(new UserDto(1, "admin", "Admin User", UserRole.Admin, true, false, null)));
        }

        public Task<Result<WorkstationDto>> EnsureWorkstationRegisteredAsync(string? machineName = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(WorkstationResult);
        }

        public void Logout()
        {
        }
    }

    private sealed class FakeFormFactory : IFormFactory
    {
        public TForm Create<TForm>() where TForm : Form
        {
            return (TForm)Activator.CreateInstance(typeof(TForm))!;
        }

        public TControl CreateControl<TControl>() where TControl : Control
        {
            return (TControl)Activator.CreateInstance(typeof(TControl))!;
        }
    }
}
