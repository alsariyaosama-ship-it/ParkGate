namespace ParkGate.Domain.Entities;

/// <summary>
/// Audit log entry capturing business actions, security events and state changes (BR-17).
/// </summary>
public sealed class AuditLog
{
    public const int MaxActionLength = 50;
    public const int MaxEntityNameLength = 50;
    public const int MaxEntityIdLength = 50;

    public long Id { get; private set; }
    public DateTime AtUtc { get; private set; }
    public int? UserId { get; private set; }
    public int? WorkstationId { get; private set; }
    public string Action { get; private set; } = null!;
    public string? EntityName { get; private set; }
    public string? EntityId { get; private set; }
    public string? DetailsJson { get; private set; }

    private AuditLog() { }

    public AuditLog(
        DateTime atUtc,
        string action,
        int? userId = null,
        int? workstationId = null,
        string? entityName = null,
        string? entityId = null,
        string? detailsJson = null)
    {
        if (atUtc.Kind == DateTimeKind.Unspecified)
        {
            throw new ArgumentException("Timestamp must have UTC kind.", nameof(atUtc));
        }

        if (string.IsNullOrWhiteSpace(action) || action.Trim().Length > MaxActionLength)
        {
            throw new ArgumentException($"Action must be between 1 and {MaxActionLength} characters.", nameof(action));
        }

        if (entityName != null && entityName.Trim().Length > MaxEntityNameLength)
        {
            throw new ArgumentException($"Entity name cannot exceed {MaxEntityNameLength} characters.", nameof(entityName));
        }

        if (entityId != null && entityId.Trim().Length > MaxEntityIdLength)
        {
            throw new ArgumentException($"Entity ID cannot exceed {MaxEntityIdLength} characters.", nameof(entityId));
        }

        AtUtc = atUtc;
        Action = action.Trim();
        UserId = userId;
        WorkstationId = workstationId;
        EntityName = string.IsNullOrWhiteSpace(entityName) ? null : entityName.Trim();
        EntityId = string.IsNullOrWhiteSpace(entityId) ? null : entityId.Trim();
        DetailsJson = detailsJson;
    }
}
