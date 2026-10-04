using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ParkGate.Domain.Common;

namespace ParkGate.Infrastructure.Data.Interceptors;

/// <summary>
/// EF Core interceptor that automatically populates audit columns on IAuditableEntity instances.
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;
    private readonly Func<int?>? _currentUserIdProvider;

    public AuditSaveChangesInterceptor(
        TimeProvider? timeProvider = null,
        Func<int?>? currentUserIdProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
        _currentUserIdProvider = currentUserIdProvider;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var currentUserId = _currentUserIdProvider?.Invoke();

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAtUtc == default)
                {
                    entry.Entity.CreatedAtUtc = nowUtc;
                }

                entry.Entity.CreatedByUserId ??= currentUserId;
                entry.Entity.UpdatedAtUtc = null;
                entry.Entity.UpdatedByUserId = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = nowUtc;
                entry.Entity.UpdatedByUserId = currentUserId;

                entry.Property(nameof(IAuditableEntity.CreatedAtUtc)).IsModified = false;
                entry.Property(nameof(IAuditableEntity.CreatedByUserId)).IsModified = false;
            }
        }
    }
}
