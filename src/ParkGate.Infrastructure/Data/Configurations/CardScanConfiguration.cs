using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class CardScanConfiguration : IEntityTypeConfiguration<CardScan>
{
    public void Configure(EntityTypeBuilder<CardScan> builder)
    {
        builder.ToTable("CardScanLog");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.ScannedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.StationName)
            .HasMaxLength(CardScan.MaxStationNameLength)
            .IsRequired();

        builder.HasOne<Workstation>()
            .WithMany()
            .HasForeignKey(x => x.WorkstationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.RawPrefix)
            .HasMaxLength(CardScan.MaxRawPrefixLength);

        builder.HasIndex(x => x.ScannedAtUtc)
            .IncludeProperties(x => new { x.Status, x.StationName })
            .HasDatabaseName("IX_CardScanLog_ScannedAtUtc");

        builder.HasIndex(x => x.CardId)
            .HasDatabaseName("IX_CardScanLog_CardId");
    }
}
