namespace ParkGate.Application.Audit;

/// <summary>Kinds of operations the audit screen can filter on (R5.20).</summary>
public enum AuditCategory
{
    All = 0,
    Entries = 1,
    Gifts = 2,
    Visitors = 3,
    Users = 4,
    Setup = 5,
}

/// <summary>Park-local days, both included; an optional card number.</summary>
public sealed record AuditLogSearchRequest(DateOnly From, DateOnly To, AuditCategory Category, string? CardNumber);

/// <summary>One audit row with the names resolved and the common details lifted out of the JSON.</summary>
public sealed record AuditEntryDto(
    long Id,
    DateTime AtUtc,
    string Action,
    string? ActorName,
    string? WorkstationName,
    string? EntityName,
    string? EntityId,
    string? CardNumber,
    string? HolderName,
    string? Reason,
    string? OperatorName,
    string? ApprovedByName,
    string? DetailsJson);

public sealed record AuditLogPage(IReadOnlyList<AuditEntryDto> Rows, bool IsTruncated);
