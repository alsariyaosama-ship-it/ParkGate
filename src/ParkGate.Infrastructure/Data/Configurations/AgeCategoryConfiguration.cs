using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class AgeCategoryConfiguration : IEntityTypeConfiguration<AgeCategory>
{
    public void Configure(EntityTypeBuilder<AgeCategory> builder)
    {
        builder.ToTable("AgeCategories");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.NameAr)
            .HasMaxLength(AgeCategory.MaxNameLength)
            .IsRequired();

        builder.HasIndex(x => x.NameAr)
            .IsUnique()
            .HasDatabaseName("UX_AgeCategories_NameAr");

        builder.Property(x => x.DescriptionAr)
            .HasMaxLength(AgeCategory.MaxDescriptionLength);

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();
    }
}
