using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class EntryCardConfiguration : IEntityTypeConfiguration<EntryCard>
{
    public void Configure(EntityTypeBuilder<EntryCard> builder)
    {
        builder.ToTable("EntryCards");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasIndex(x => x.CardId)
            .IsUnique()
            .HasDatabaseName("UX_EntryCards_CardId");

        builder.HasOne<Beneficiary>()
            .WithMany()
            .HasForeignKey(x => x.BeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<QrSigningKey>()
            .WithMany()
            .HasForeignKey(x => x.KeyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.IssuedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.RevokeReason)
            .HasMaxLength(EntryCard.MaxRevokeReasonLength);

        // At most one active card per holder; a reissue revokes the old one first.
        builder.HasIndex(x => x.BeneficiaryId)
            .IsUnique()
            .HasFilter("[RevokedAtUtc] IS NULL")
            .HasDatabaseName("UX_EntryCards_OneActivePerBeneficiary");

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.Ignore(x => x.IsRevoked);
    }
}
