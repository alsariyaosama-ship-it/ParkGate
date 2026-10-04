namespace ParkGate.Domain.Common;

/// <summary>
/// Contract for entities with audit metadata tracked automatically by SaveChangesInterceptor.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    int? CreatedByUserId { get; set; }
    DateTime? UpdatedAtUtc { get; set; }
    int? UpdatedByUserId { get; set; }
}
