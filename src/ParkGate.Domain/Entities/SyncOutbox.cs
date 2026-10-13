namespace ParkGate.Domain.Entities;

/// <summary>
/// Transactional outbox entry for asynchronous cloud synchronization (F3).
/// </summary>
public sealed class SyncOutbox
{
    public const int MaxEntityTypeLength = 100;
    public const int MaxEntityIdLength = 100;
    public const int MaxOperationTypeLength = 50;
    public const int MaxLastErrorLength = 500;

    public Guid Id { get; private set; }
    public string EntityType { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;
    public string OperationType { get; private set; } = null!;
    public string PayloadJson { get; private set; } = null!;
    public int SchemaVersion { get; private set; }
    public bool IsProcessed { get; private set; }
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }

    private SyncOutbox() { }

    public SyncOutbox(
        Guid id,
        string entityType,
        string entityId,
        string operationType,
        string payloadJson,
        int schemaVersion,
        DateTime createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);

        Id = id;
        EntityType = entityType.Trim();
        EntityId = entityId.Trim();
        OperationType = operationType.Trim();
        PayloadJson = payloadJson;
        SchemaVersion = schemaVersion;
        IsProcessed = false;
        RetryCount = 0;
        CreatedAtUtc = createdAtUtc;
    }

    public static SyncOutbox Create(
        string entityType,
        string entityId,
        string operationType,
        string payloadJson,
        DateTime createdAtUtc,
        int schemaVersion = 1)
    {
        return new SyncOutbox(
            Guid.NewGuid(),
            entityType,
            entityId,
            operationType,
            payloadJson,
            schemaVersion,
            createdAtUtc);
    }

    public void MarkProcessed(DateTime processedAtUtc)
    {
        IsProcessed = true;
        ProcessedAtUtc = processedAtUtc;
        LastError = null;
    }

    public void RecordFailure(string error)
    {
        RetryCount++;
        LastError = error.Length > MaxLastErrorLength
            ? error[..MaxLastErrorLength]
            : error;
    }

    public void UpdatePayload(string newPayloadJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPayloadJson);
        PayloadJson = newPayloadJson;
    }
}
