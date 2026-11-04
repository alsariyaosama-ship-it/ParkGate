using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Sync;

public enum PushFailureKind
{
    None = 0,
    NetworkOrTimeout = 1,
    Unauthorized = 2,
    ServerError = 3,
    PayloadRejected = 4
}

public sealed record PushBatchResult(
    bool IsSuccess,
    PushFailureKind FailureKind = PushFailureKind.None,
    string? ErrorMessage = null)
{
    public static PushBatchResult Success() => new(true, PushFailureKind.None);
    public static PushBatchResult Failure(PushFailureKind kind, string? message = null) => new(false, kind, message);
}

public interface ICloudPushService
{
    Task<PushBatchResult> PushBatchAsync(IReadOnlyList<SyncOutbox> batch, CancellationToken cancellationToken = default);
}
