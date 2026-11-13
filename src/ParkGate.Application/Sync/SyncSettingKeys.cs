namespace ParkGate.Application.Sync;

/// <summary>
/// AppSettings keys the sync agent writes so other processes (the desk UI) can show sync health
/// without knowing the agent's configuration.
/// </summary>
public static class SyncSettingKeys
{
    public const string LastHeartbeatUtc = "SyncAgent.LastHeartbeatUtc";

    /// <summary>Outbox rows still waiting to be pushed and under the retry limit.</summary>
    public const string PendingCount = "SyncAgent.PendingCount";

    /// <summary>Outbox rows that reached the retry limit. They are never retried again and need attention (R2.5).</summary>
    public const string FailedCount = "SyncAgent.FailedCount";
}
