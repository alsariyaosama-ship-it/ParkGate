namespace ParkGate.Application.Sync;

/// <summary>Sync health as last reported by the sync agent.</summary>
public sealed record SyncStatusDto(int PendingCount, int FailedCount, DateTime? LastHeartbeatUtc);
