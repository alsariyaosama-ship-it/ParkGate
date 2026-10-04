namespace ParkGate.Application.Ports.Audit;

public interface IAuditWriter
{
    Task WriteAsync(
        string action,
        int? userId = null,
        int? workstationId = null,
        string? entityName = null,
        string? entityId = null,
        string? detailsJson = null,
        CancellationToken cancellationToken = default);
}
