using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ParkGate.Domain.Common;

namespace ParkGate.Infrastructure.Data.Interceptors;

/// <summary>
/// Fills the audit columns of <see cref="IAuditableEntity"/> rows. Times come from the database clock (BR-05,
/// R6.3), never from the desk: a new row keeps the <c>SYSUTCDATETIME()</c> column default, and an update reads
/// the clock once per save, only when a tracked auditable row changed.
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly Func<int?> _currentUserId;
    private readonly Func<ParkGateDbContext, CancellationToken, Task<DateTime>> _databaseUtcNow;

    public AuditSaveChangesInterceptor(
        Func<int?>? currentUserIdProvider = null,
        Func<ParkGateDbContext, CancellationToken, Task<DateTime>>? databaseUtcNow = null)
    {
        _currentUserId = currentUserIdProvider ?? (() => null);
        _databaseUtcNow = databaseUtcNow ?? DatabaseClock.GetUtcNowAsync;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        // ParkGateDbContext routes SaveChanges through SaveChangesAsync; this path serves other callers only.
        UpdateAuditableEntitiesAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await UpdateAuditableEntitiesAsync(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task UpdateAuditableEntitiesAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is not ParkGateDbContext parkGate)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries<IAuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();
        if (entries.Count == 0)
        {
            return;
        }

        var currentUserId = _currentUserId();
        DateTime? updatedAtUtc = null;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                // CreatedAtUtc stays default so the column default SYSUTCDATETIME() sets it.
                entry.Entity.CreatedByUserId ??= currentUserId;
                entry.Entity.UpdatedAtUtc = null;
                entry.Entity.UpdatedByUserId = null;
                continue;
            }

            updatedAtUtc ??= await _databaseUtcNow(parkGate, cancellationToken);
            entry.Entity.UpdatedAtUtc = updatedAtUtc;
            entry.Entity.UpdatedByUserId = currentUserId;
            entry.Property(nameof(IAuditableEntity.CreatedAtUtc)).IsModified = false;
            entry.Property(nameof(IAuditableEntity.CreatedByUserId)).IsModified = false;
        }
    }
}
