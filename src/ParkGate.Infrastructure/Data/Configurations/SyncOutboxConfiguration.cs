using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class SyncOutboxConfiguration : IEntityTypeConfiguration<SyncOutbox>
{
    public void Configure(EntityTypeBuilder<SyncOutbox> builder)
    {
        builder.ToTable("SyncOutboxes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType)
            .HasMaxLength(SyncOutbox.MaxEntityTypeLength)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasMaxLength(SyncOutbox.MaxEntityIdLength)
            .IsRequired();

        builder.Property(x => x.OperationType)
            .HasMaxLength(SyncOutbox.MaxOperationTypeLength)
            .IsRequired();

        builder.Property(x => x.PayloadJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.SchemaVersion)
            .IsRequired();

        builder.Property(x => x.IsProcessed)
            .IsRequired();

        builder.Property(x => x.RetryCount)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(SyncOutbox.MaxLastErrorLength);

        builder.Property(x => x.CreatedAtUtc)
            .HasPrecision(3)
            .IsRequired();

        builder.Property(x => x.ProcessedAtUtc)
            .HasPrecision(3);

        builder.HasIndex(x => new { x.IsProcessed, x.RetryCount, x.CreatedAtUtc })
            .HasDatabaseName("IX_SyncOutboxes_Pending");
    }
}
