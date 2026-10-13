using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Sync;

public interface ICloudPushService
{
    Task<bool> PushBatchAsync(IReadOnlyList<SyncOutbox> batch, CancellationToken cancellationToken = default);
}
