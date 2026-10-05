using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class WorkstationConfiguration : IEntityTypeConfiguration<Workstation>
{
    public void Configure(EntityTypeBuilder<Workstation> builder)
    {
        builder.ToTable("Workstations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.MachineName)
            .HasMaxLength(Workstation.MaxMachineNameLength)
            .IsRequired();

        builder.HasIndex(x => x.MachineName)
            .IsUnique()
            .HasDatabaseName("UX_Workstations_MachineName");

        builder.Property(x => x.DisplayName)
            .HasMaxLength(Workstation.MaxDisplayNameLength)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.LastSeenUtc)
            .HasPrecision(3);
    }
}
