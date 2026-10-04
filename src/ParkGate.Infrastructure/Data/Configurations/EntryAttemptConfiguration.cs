using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class EntryAttemptConfiguration : IEntityTypeConfiguration<EntryAttempt>
{
    public void Configure(EntityTypeBuilder<EntryAttempt> builder)
    {
        builder.ToTable("EntryAttempts", t =>
        {
            t.HasCheckConstraint("CK_EntryAttempts_DenialReason", "([Result] = 2 AND [DenialReason] IS NOT NULL) OR ([Result] <> 2 AND [DenialReason] IS NULL)");
            t.HasCheckConstraint("CK_EntryAttempts_Override", "([Result] = 3 AND [OverrideReason] IS NOT NULL AND [OverrideByUserId] IS NOT NULL) OR ([Result] <> 3 AND [OverrideReason] IS NULL AND [OverrideByUserId] IS NULL)");
            t.HasCheckConstraint("CK_EntryAttempts_Void", "([IsVoided] = 0 AND [VoidedAtUtc] IS NULL AND [VoidedByUserId] IS NULL AND [VoidReason] IS NULL) OR ([IsVoided] = 1 AND [VoidedAtUtc] IS NOT NULL AND [VoidedByUserId] IS NOT NULL AND [VoidReason] IS NOT NULL)");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWSEQUENTIALID()");

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

        builder.Property(x => x.AttemptedAtUtc)
            .HasPrecision(3)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(x => x.BusinessDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Result)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.DenialReason)
            .HasConversion<byte>();

        builder.Property(x => x.CooldownMinutesApplied)
            .IsRequired();

        builder.Property(x => x.NextEligibleAtUtc)
            .HasPrecision(3);

        builder.Property(x => x.TotalPersons)
            .IsRequired();

        builder.Property(x => x.Channel)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.WorkstationId)
            .IsRequired();

        builder.HasOne<Workstation>()
            .WithMany()
            .HasForeignKey(x => x.WorkstationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.OverrideByUserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.OverrideByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.OverrideReason)
            .HasMaxLength(EntryAttempt.MaxOverrideReasonLength);

        builder.Property(x => x.IsVoided)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.VoidedAtUtc)
            .HasPrecision(3);

        builder.Property(x => x.VoidedByUserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.VoidedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.VoidReason)
            .HasMaxLength(EntryAttempt.MaxVoidReasonLength);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Companions relationship
        builder.HasMany(x => x.Companions)
            .WithOne()
            .HasForeignKey(c => c.EntryAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Companions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Indexes per DATABASE.md §3.4
        builder.HasIndex(x => new { x.BeneficiaryId, x.AttemptedAtUtc })
            .IsDescending(false, true)
            .HasDatabaseName("IX_EntryAttempts_Beneficiary_Time");

        builder.HasIndex(x => x.BusinessDate)
            .IncludeProperties(x => new { x.Result, x.TotalPersons, x.OrganizationId, x.IsVoided })
            .HasDatabaseName("IX_EntryAttempts_BusinessDate");

        builder.HasIndex(x => new { x.WorkstationId, x.AttemptedAtUtc })
            .IsDescending(false, true)
            .HasDatabaseName("IX_EntryAttempts_Workstation_Time");
    }
}
