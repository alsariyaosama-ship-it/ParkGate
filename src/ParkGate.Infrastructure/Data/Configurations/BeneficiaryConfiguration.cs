using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
{
    public void Configure(EntityTypeBuilder<Beneficiary> builder)
    {
        builder.ToTable("Beneficiaries", t =>
        {
            t.HasCheckConstraint("CK_Beneficiaries_StatusReason", "([Status] = 1) OR ([Status] <> 1 AND [StatusReason] IS NOT NULL)");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CardNumber)
            .HasConversion(c => c.Value, v => CardNumber.Create(v))
            .HasMaxLength(CardNumber.MaxLength)
            .IsRequired();

        builder.Property(x => x.FullName)
            .HasMaxLength(Beneficiary.MaxFullNameLength)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasConversion(p => p.Value, v => PhoneNumber.Create(v))
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(x => x.NationalId)
            .HasConversion(n => n != null ? n.Value : null, v => v != null ? NationalId.Create(v) : null)
            .HasMaxLength(NationalId.RequiredLength);

        builder.Property(x => x.CardExpiryDate)
            .HasColumnType("date");

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .HasDefaultValue(BeneficiaryStatus.Active)
            .IsRequired();

        builder.Property(x => x.StatusReason)
            .HasMaxLength(Beneficiary.MaxStatusReasonLength);

        builder.Property(x => x.Notes)
            .HasMaxLength(Beneficiary.MaxNotesLength);

        builder.Property(x => x.LastGrantedEntryUtc)
            .HasPrecision(3);

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

        // Indexes per DATABASE.md §3.2
        builder.HasIndex(x => new { x.OrganizationId, x.CardNumber })
            .IsUnique()
            .HasDatabaseName("UX_Beneficiaries_Org_Card");

        builder.HasIndex(x => x.CardNumber)
            .HasDatabaseName("IX_Beneficiaries_CardNumber");

        builder.HasIndex(x => x.Phone)
            .HasDatabaseName("IX_Beneficiaries_Phone");

        builder.HasIndex(x => x.NationalId)
            .IsUnique()
            .HasFilter("[NationalId] IS NOT NULL")
            .HasDatabaseName("UX_Beneficiaries_NationalId");

        builder.HasIndex(x => x.FullName)
            .HasDatabaseName("IX_Beneficiaries_FullName");
    }
}
