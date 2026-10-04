using Microsoft.EntityFrameworkCore;
using ParkGate.Application.Ports.Audit;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data;

public sealed class AuditWriter : IAuditWriter
{
    private readonly IDbContextFactory<ParkGateDbContext> _contextFactory;
    private readonly TimeProvider _timeProvider;

    public AuditWriter(
        IDbContextFactory<ParkGateDbContext> contextFactory,
        TimeProvider? timeProvider = null)
    {
        _contextFactory = contextFactory;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task WriteAsync(
        string action,
        int? userId = null,
        int? workstationId = null,
        string? entityName = null,
        string? entityId = null,
        string? detailsJson = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

        var log = new AuditLog(nowUtc, action, userId, workstationId, entityName, entityId, detailsJson);
        context.AuditLogs.Add(log);
        await context.SaveChangesAsync(cancellationToken);
    }
}
