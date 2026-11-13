using ParkGate.Application.Common;

namespace ParkGate.Application.Sync;

/// <summary>Reports the health of cloud synchronization to administrators.</summary>
public interface ISyncStatusService
{
    /// <summary>Returns the agent's last reported counts. Requires <c>Permission.ManageSettings</c>.</summary>
    Task<Result<SyncStatusDto>> GetAsync(CancellationToken cancellationToken = default);
}
