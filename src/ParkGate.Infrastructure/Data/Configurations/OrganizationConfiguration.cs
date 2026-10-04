using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations", t =>
        {
            t.HasCheckConstraint("CK_Organizations_CooldownMinutes", "[CooldownMinutes] IS NULL OR ([CooldownMinutes] >= 1 AND [CooldownMinutes] <= 10080)");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Code)
            .HasMaxLength(Organization.MaxCodeLength)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("UX_Organizations_Code");

        builder.Property(x => x.NameAr)
            .HasMaxLength(Organization.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.CooldownMinutes);

        builder.Property(x => x.IsDefault)
            .IsRequired();

        builder.HasIndex(x => x.IsDefault)
            .IsUnique()
            .HasFilter("[IsDefault] = 1")
            .HasDatabaseName("UX_Organizations_IsDefault");

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasPrecision(3)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(x => x.CreatedByUserId);

        builder.Property(x => x.UpdatedAtUtc)
            .HasPrecision(3);

        builder.Property(x => x.UpdatedByUserId);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}
