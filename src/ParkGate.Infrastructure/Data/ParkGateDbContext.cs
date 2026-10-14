using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data;

public class ParkGateDbContext : DbContext
{
    public ParkGateDbContext(DbContextOptions<ParkGateDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<AgeCategory> AgeCategories => Set<AgeCategory>();
    public DbSet<EntryAttempt> EntryAttempts => Set<EntryAttempt>();
    public DbSet<EntryAttemptCompanion> EntryAttemptCompanions => Set<EntryAttemptCompanion>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Workstation> Workstations => Set<Workstation>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SyncOutbox> SyncOutboxes => Set<SyncOutbox>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ParkGateDbContext).Assembly);
        modelBuilder.SeedInitialData();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<DateTime>()
            .HavePrecision(3)
            .HaveConversion<DateTimeUtcConverter>();

        configurationBuilder.Properties<DateTime?>()
            .HavePrecision(3)
            .HaveConversion<NullableDateTimeUtcConverter>();

        configurationBuilder.Properties<DateOnly>()
            .HaveColumnType("date");
    }

    private sealed record PendingOutboxEntry(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry,
        EntityState InitialState,
        string EntityType,
        string? PreExtractedId,
        string? PreSerializedPayload);

    public override int SaveChanges()
    {
        return SaveChanges(true);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        return SaveChangesAsync(acceptAllChangesOnSuccess).GetAwaiter().GetResult();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(true, cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction == null && Database.IsRelational())
        {
            await using var tx = await Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await SaveChangesWithOutboxAsync(acceptAllChangesOnSuccess, cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        }

        return await SaveChangesWithOutboxAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private async Task<int> SaveChangesWithOutboxAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken)
    {
        var pending = new List<PendingOutboxEntry>();
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            var typeName = entry.Metadata.ClrType.Name;
            if (!OutboxEntityExtractor.IsSyncable(typeName))
            {
                continue;
            }

            string? id = null;
            string? payload = null;

            if (entry.State == EntityState.Deleted)
            {
                id = OutboxEntityExtractor.ExtractEntityId(entry);
                payload = OutboxEntityExtractor.SerializePayload(entry);
            }

            pending.Add(new PendingOutboxEntry(entry, entry.State, typeName, id, payload));
        }

        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        if (pending.Count > 0)
        {
            var nowUtc = DateTime.UtcNow;
            var outboxes = new List<SyncOutbox>();

            foreach (var p in pending)
            {
                var id = p.PreExtractedId ?? OutboxEntityExtractor.ExtractEntityId(p.Entry);
                var payload = p.PreSerializedPayload ?? OutboxEntityExtractor.SerializePayload(p.Entry);
                var op = p.InitialState switch
                {
                    EntityState.Added => "Insert",
                    EntityState.Deleted => "Delete",
                    _ => "Update"
                };

                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(payload))
                {
                    outboxes.Add(SyncOutbox.Create(p.EntityType, id, op, payload, nowUtc, schemaVersion: 1));
                }
            }

            if (outboxes.Count > 0)
            {
                await SyncOutboxes.AddRangeAsync(outboxes, cancellationToken);
                await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
        }

        return result;
    }
}

