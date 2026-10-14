namespace ParkGate.Application.Sync;

public sealed class SyncOptions
{
    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 30;
    public int BatchSize { get; set; } = 100;
    public int MaxRetries { get; set; } = 10;
}

public sealed class CloudApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string? AesKey { get; set; }
}
