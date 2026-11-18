using ParkGate.Application.Ports.Persistence;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.Common;

/// <summary>
/// Adds audit rows to the unit of work that makes the change, so the row and the change are saved together
/// or not at all (R6.11). Writing the row afterwards through a separate context could lose it.
/// </summary>
internal static class UnitOfWorkAuditExtensions
{
    /// <summary>Adds an audit row stamped with the database clock (BR-05). The caller saves it.</summary>
    public static async Task AddAuditAsync(
        this IUnitOfWork uow,
        string action,
        int? userId,
        int? workstationId,
        string? entityName = null,
        string? entityId = null,
        string? detailsJson = null,
        CancellationToken cancellationToken = default)
    {
        var nowUtc = await uow.GetDbUtcNowAsync(cancellationToken);
        uow.AuditLogs.Add(new AuditLog(nowUtc, action, userId, workstationId, entityName, entityId, detailsJson));
    }
}
