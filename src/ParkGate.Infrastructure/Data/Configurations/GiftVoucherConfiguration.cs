using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class GiftVoucherConfiguration : IEntityTypeConfiguration<GiftVoucher>
{
    public void Configure(EntityTypeBuilder<GiftVoucher> builder)
    {
        builder.ToTable("GiftVouchers", t =>
        {
            t.HasCheckConstraint("CK_GiftVouchers_Override", "([IsOverride] = 0 AND [OverrideByUserId] IS NULL AND [OverrideReason] IS NULL) OR ([IsOverride] = 1 AND [OverrideByUserId] IS NOT NULL AND [OverrideReason] IS NOT NULL)");
            t.HasCheckConstraint("CK_GiftVouchers_Status", "([Status] = 1 AND [RedeemedAtUtc] IS NULL AND [VoidedAtUtc] IS NULL) OR ([Status] = 2 AND [RedeemedAtUtc] IS NOT NULL AND [RedeemedBusinessDate] IS NOT NULL AND [RedeemedByUserId] IS NOT NULL AND [RedeemedWorkstationId] IS NOT NULL AND [VoidedAtUtc] IS NULL) OR ([Status] = 3 AND [VoidedAtUtc] IS NOT NULL AND [VoidedByUserId] IS NOT NULL AND [VoidReason] IS NOT NULL AND [RedeemedAtUtc] IS NULL)");
            t.HasCheckConstraint("CK_GiftVouchers_Value", "[Value] > 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Token)
            .IsRequired();

        builder.HasIndex(x => x.Token)
            .IsUnique()
            .HasDatabaseName("UX_GiftVouchers_Token");

        builder.Property(x => x.KeyId)
            .IsRequired();

        builder.Property(x => x.BeneficiaryId)
            .IsRequired();

        builder.HasOne<Beneficiary>()
            .WithMany()
            .HasForeignKey(x => x.BeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Value)
            .HasPrecision(10, 3)
            .IsRequired();

        builder.Property(x => x.IssuedAtUtc)
            .HasPrecision(3)
            .IsRequired();

        builder.Property(x => x.BusinessDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.ExpiresOn)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.IssuedByUserId)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.IssuedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.IssuedWorkstationId)
            .IsRequired();

        builder.HasOne<Workstation>()
            .WithMany()
            .HasForeignKey(x => x.IssuedWorkstationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.IsOverride)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.OverrideByUserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.OverrideByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.OverrideReason)
            .HasMaxLength(GiftVoucher.MaxOverrideReasonLength);

        builder.Property(x => x.RedeemedAtUtc)
            .HasPrecision(3);

        builder.Property(x => x.RedeemedBusinessDate)
            .HasColumnType("date");

        builder.Property(x => x.RedeemedByUserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.RedeemedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.RedeemedWorkstationId);

        builder.HasOne<Workstation>()
            .WithMany()
            .HasForeignKey(x => x.RedeemedWorkstationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.VoidedAtUtc)
            .HasPrecision(3);

        builder.Property(x => x.VoidedByUserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.VoidedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.VoidReason)
            .HasMaxLength(GiftVoucher.MaxVoidReasonLength);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Indexes per DATABASE.md §3.12
        builder.HasIndex(x => new { x.BeneficiaryId, x.IssuedAtUtc })
            .IsDescending(false, true)
            .HasDatabaseName("IX_GiftVouchers_Beneficiary_Time");

        builder.HasIndex(x => x.BusinessDate)
            .IncludeProperties(x => new { x.Status, x.Value, x.OrganizationId })
            .HasDatabaseName("IX_GiftVouchers_BusinessDate");

        builder.HasIndex(x => x.RedeemedBusinessDate)
            .IncludeProperties(x => new { x.Value, x.OrganizationId, x.RedeemedWorkstationId })
            .HasFilter("[RedeemedBusinessDate] IS NOT NULL")
            .HasDatabaseName("IX_GiftVouchers_RedeemedBusinessDate");
    }
}
