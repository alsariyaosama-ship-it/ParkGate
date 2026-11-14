using ParkGate.Application.Users.DTOs;
using ParkGate.Application.Workstations.DTOs;

namespace ParkGate.Application.Auth.DTOs;

public sealed record FirstRunAdminRequest(
    string UserName,
    string FullName,
    string Password,
    string ConfirmPassword);

public sealed record LoginRequest(
    string UserName,
    string Password,
    string? MachineName = null);

public sealed record LoginResponse(
    UserDto User,
    WorkstationDto Workstation,
    bool MustChangePassword);

public sealed record ChangePasswordRequest(
    int UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);

/// <summary>
/// A supervisor approval (R3.2): <see cref="Token"/> is single-use, expires after two minutes and is bound to this
/// workstation. Pass it to the use case; the user id and name are for display only.
/// </summary>
public sealed record SupervisorApprovalDto(string Token, int SupervisorUserId, string SupervisorFullName);
