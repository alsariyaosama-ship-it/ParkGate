using ParkGate.Application.Common;

namespace ParkGate.Application.Audit;

/// <summary>Reads the audit trail for the audit screen (R5.20). Needs <c>Permission.ViewAuditLog</c>.</summary>
public interface IAuditLogService
{
    Task<Result<AuditLogPage>> SearchAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default);
}
