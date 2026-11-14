using ParkGate.Application.Auth.DTOs;
using ParkGate.Application.Common;
using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Workstations.DTOs;

namespace ParkGate.Application.Auth.Services;

public interface IAuthService
{
    Task<bool> IsFirstRunAsync(CancellationToken cancellationToken = default);
    Task<Result<UserDto>> CompleteFirstRunAsync(FirstRunAdminRequest request, CancellationToken cancellationToken = default);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Checks a supervisor's or admin's credentials and issues a single-use approval token for this workstation (R3.2).
    /// Use cases accept only the token, so the approver can never be an id chosen by the UI.
    /// </summary>
    Task<Result<SupervisorApprovalDto>> AuthorizeSupervisorAsync(string userName, string password, CancellationToken cancellationToken = default);
    Task<Result<WorkstationDto>> EnsureWorkstationRegisteredAsync(string? machineName = null, CancellationToken cancellationToken = default);
    void Logout();
}
