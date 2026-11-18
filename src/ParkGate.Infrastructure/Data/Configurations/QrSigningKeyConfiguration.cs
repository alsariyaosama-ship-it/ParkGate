using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class QrSigningKeyConfiguration : IEntityTypeConfiguration<QrSigningKey>
{
    public void Configure(EntityTypeBuilder<QrSigningKey> builder)
    {
        builder.ToTable("QrSigningKeys");

        builder.HasKey(x => x.KeyId);
        builder.Property(x => x.KeyId).ValueGeneratedNever();

        builder.Property(x => x.PublicKeySpki)
            .HasMaxLength(QrSigningKey.MaxPublicKeyLength)
            .IsRequired();

        builder.HasIndex(x => x.PublicKeySpki)
            .IsUnique()
            .HasDatabaseName("UX_QrSigningKeys_PublicKeySpki");

        builder.Property(x => x.StationName)
            .HasMaxLength(QrSigningKey.MaxStationNameLength)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.Ignore(x => x.IsRevoked);
    }
}
