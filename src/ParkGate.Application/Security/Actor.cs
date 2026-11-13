using ParkGate.Application.Common;

namespace ParkGate.Application.Security;

/// <summary>
/// The signed-in user and the registered workstation a use case acts as. Use cases that record who did
/// something resolve it first and fail instead of falling back to a made-up id (R3.3).
/// </summary>
internal readonly record struct Actor(int UserId, int WorkstationId)
{
    public static Result<Actor> Resolve(ICurrentUser currentUser, ICurrentWorkstation currentWorkstation)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Errors.General.Unauthorized;
        }

        if (currentWorkstation.WorkstationId is not { } workstationId)
        {
            return Errors.Auth.WorkstationNotFound;
        }

        return new Actor(userId, workstationId);
    }
}
