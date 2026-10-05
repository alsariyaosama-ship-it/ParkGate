using Microsoft.EntityFrameworkCore;
using ParkGate.Domain.Entities;

namespace ParkGate.Infrastructure.Data;

public static class ModelBuilderExtensions
{
    public static void SeedInitialData(this ModelBuilder modelBuilder)
    {
        var seedTimeUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Seed default Organization (DATABASE.md §6)
        modelBuilder.Entity<Organization>().HasData(new
        {
            Id = 1,
            Code = "SSF",
            NameAr = "صندوق التضامن الاجتماعي",
            CooldownMinutes = (int?)null,
            IsDefault = true,
            IsActive = true,
            CreatedAtUtc = seedTimeUtc,
            CreatedByUserId = (int?)null,
            UpdatedAtUtc = (DateTime?)null,
            UpdatedByUserId = (int?)null
        });

        // Seed Age Categories (DATABASE.md §6)
        modelBuilder.Entity<AgeCategory>().HasData(
            new
            {
                Id = 1,
                NameAr = "بالغ",
                DescriptionAr = (string?)"13 سنة فأكثر",
                SortOrder = 1,
                IsActive = true
            },
            new
            {
                Id = 2,
                NameAr = "طفل",
                DescriptionAr = (string?)"من 3 إلى 12 سنة",
                SortOrder = 2,
                IsActive = true
            },
            new
            {
                Id = 3,
                NameAr = "رضيع",
                DescriptionAr = (string?)"أقل من 3 سنوات",
                SortOrder = 3,
                IsActive = true
            });

        // Seed AppSettings (DATABASE.md §5)
        modelBuilder.Entity<AppSetting>().HasData(
            new { Key = "Park.Name", Value = "منتزه المرح العائلي", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Park.TimeZoneId", Value = "Libya Standard Time", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Entry.DefaultCooldownMinutes", Value = "720", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Entry.MaxGroupSize", Value = "10", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Entry.DenialDedupMinutes", Value = "5", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Entry.PrefillCompanionsFromLastVisit", Value = "true", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Entry.PlaySoundOnDenial", Value = "true", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Security.MaxFailedLogins", Value = "5", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Security.LockoutMinutes", Value = "15", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Ui.MaskNationalIdForOperators", Value = "true", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null });
    }
}
