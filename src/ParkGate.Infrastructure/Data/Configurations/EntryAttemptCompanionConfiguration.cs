using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class EntryAttemptCompanionConfiguration : IEntityTypeConfiguration<EntryAttemptCompanion>
{
    public void Configure(EntityTypeBuilder<EntryAttemptCompanion> builder)
    {
        builder.ToTable("EntryAttemptCompanions", t =>
        {
            t.HasCheckConstraint("CK_EntryAttemptCompanions_Count", "[Count] >= 0");
        });

        builder.HasKey(x => new { x.EntryAttemptId, x.AgeCategoryId });

        builder.Property(x => x.EntryAttemptId)
            .IsRequired();

        builder.Property(x => x.AgeCategoryId)
            .IsRequired();

        builder.HasOne<AgeCategory>()
            .WithMany()
            .HasForeignKey(x => x.AgeCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Count)
            .IsRequired();
    }
}
