using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.CardPatterns;
using ParkGate.Domain.Entities;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Infrastructure.Data.Configurations;

/// <summary>DATABASE.md §3.13 (F8).</summary>
public sealed class OrganizationCardPatternConfiguration : IEntityTypeConfiguration<OrganizationCardPattern>
{
    public void Configure(EntityTypeBuilder<OrganizationCardPattern> builder)
    {
        builder.ToTable("OrganizationCardPatterns");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.OrganizationId).IsRequired();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Shape).HasMaxLength(CardShape.MaxLength).IsRequired();
        builder.Property(x => x.Prefix).HasMaxLength(OrganizationCardPattern.MaxPrefixLength);
        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.Source).HasConversion<byte>().IsRequired();
        builder.Property(x => x.SampleCount).IsRequired();
        builder.Property(x => x.ProposedAtUtc).HasPrecision(3).IsRequired();
        builder.Property(x => x.DecidedAtUtc).HasPrecision(3);

        builder.Property(x => x.DecidedByUserId);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.DecidedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.OrganizationId, x.Shape, x.Prefix })
            .IsUnique()
            .HasFilter(null) // one pattern per shape without a prefix too: SQL Server treats NULLs as equal here
            .HasDatabaseName("UX_OrganizationCardPatterns_Org_Shape_Prefix");

        builder.Property(x => x.CreatedAtUtc).HasPrecision(3).IsRequired().HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.Property(x => x.UpdatedByUserId);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

/// <summary>DATABASE.md §3.14 (F8).</summary>
public sealed class CardPatternAlertConfiguration : IEntityTypeConfiguration<CardPatternAlert>
{
    public void Configure(EntityTypeBuilder<CardPatternAlert> builder)
    {
        builder.ToTable("CardPatternAlerts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.OrganizationId).IsRequired();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.BeneficiaryId).IsRequired();
        builder.HasOne<Beneficiary>()
            .WithMany()
            .HasForeignKey(x => x.BeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CardNumber).HasMaxLength(CardNumber.MaxLength).IsRequired();
        builder.Property(x => x.Shape).HasMaxLength(CardShape.MaxLength).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3).IsRequired();

        builder.Property(x => x.CreatedByUserId).IsRequired();
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.WorkstationId).IsRequired();
        builder.HasOne<Workstation>()
            .WithMany()
            .HasForeignKey(x => x.WorkstationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Status).HasConversion<byte>().IsRequired();
        builder.Property(x => x.ReviewedAtUtc).HasPrecision(3);

        builder.Property(x => x.ReviewedByUserId);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ReviewNote).HasMaxLength(CardPatternAlert.MaxReviewNoteLength);

        builder.HasIndex(x => new { x.OrganizationId, x.Status, x.Shape })
            .HasDatabaseName("IX_CardPatternAlerts_Org_Status_Shape");
    }
}
