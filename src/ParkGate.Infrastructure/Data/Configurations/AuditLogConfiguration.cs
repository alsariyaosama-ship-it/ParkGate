using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.AtUtc)
            .HasPrecision(3)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(x => x.UserId);

        builder.Property(x => x.WorkstationId);

        builder.Property(x => x.Action)
            .HasMaxLength(AuditLog.MaxActionLength)
            .IsRequired();

        builder.Property(x => x.EntityName)
            .HasMaxLength(AuditLog.MaxEntityNameLength);

        builder.Property(x => x.EntityId)
            .HasMaxLength(AuditLog.MaxEntityIdLength);

        builder.Property(x => x.DetailsJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.AtUtc)
            .HasDatabaseName("IX_AuditLogs_AtUtc");

        builder.HasIndex(x => new { x.EntityName, x.EntityId })
            .HasDatabaseName("IX_AuditLogs_Entity");
    }
}
