using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.UserName)
            .HasMaxLength(User.MaxUserNameLength)
            .IsRequired();

        builder.HasIndex(x => x.UserName)
            .IsUnique()
            .HasDatabaseName("UX_Users_UserName");

        builder.Property(x => x.FullName)
            .HasMaxLength(User.MaxFullNameLength)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(User.MaxPasswordHashLength)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<byte>()
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.MustChangePassword)
            .IsRequired();

        builder.Property(x => x.FailedLoginCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.LockoutUntilUtc)
            .HasPrecision(3);

        builder.Property(x => x.LastLoginUtc)
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
    }
}
