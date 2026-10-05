using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("AppSettings");

        builder.HasKey(x => x.Key);

        builder.Property(x => x.Key)
            .HasMaxLength(AppSetting.MaxKeyLength)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(AppSetting.MaxValueLength)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasPrecision(3)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(x => x.UpdatedByUserId);
    }
}
