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
            MaxFreeCompanions = (int?)null,
            GiftEnabled = false,
            GiftValue = 0m,
            GiftIntervalDays = 30,
            AllowedEntryDays = ParkGate.Domain.Enums.WeekDays.All,
            GiftAnyDay = false,
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
                IsActive = true,
                IsCompanionLimitExempt = false
            },
            new
            {
                Id = 2,
                NameAr = "طفل",
                DescriptionAr = (string?)"من 3 إلى 12 سنة",
                SortOrder = 2,
                IsActive = true,
                IsCompanionLimitExempt = false
            },
            new
            {
                Id = 3,
                NameAr = "رضيع",
                DescriptionAr = (string?)"أقل من 3 سنوات",
                SortOrder = 3,
                IsActive = true,
                IsCompanionLimitExempt = true
            });

        // Seed Cities (DATABASE.md §3.18, §6): central and western Libya plus Sabha, fixed display order (BR-32)
        modelBuilder.Entity<City>().HasData(
            new { Id = 1, NameAr = "الخمس", SortOrder = 1, IsActive = true },
            new { Id = 2, NameAr = "زليتن", SortOrder = 2, IsActive = true },
            new { Id = 3, NameAr = "مصراتة", SortOrder = 3, IsActive = true },
            new { Id = 4, NameAr = "طرابلس", SortOrder = 4, IsActive = true },
            new { Id = 5, NameAr = "مسلاتة", SortOrder = 5, IsActive = true },
            new { Id = 6, NameAr = "ترهونة", SortOrder = 6, IsActive = true },
            new { Id = 7, NameAr = "القره بوللي", SortOrder = 7, IsActive = true },
            new { Id = 8, NameAr = "قصر الأخيار", SortOrder = 8, IsActive = true },
            new { Id = 9, NameAr = "العلوص", SortOrder = 9, IsActive = true },
            new { Id = 10, NameAr = "الثمانين", SortOrder = 10, IsActive = true },
            new { Id = 11, NameAr = "قماطة", SortOrder = 11, IsActive = true },
            new { Id = 12, NameAr = "بني وليد", SortOrder = 12, IsActive = true },
            new { Id = 13, NameAr = "سرت", SortOrder = 13, IsActive = true },
            new { Id = 14, NameAr = "العجيلات", SortOrder = 14, IsActive = true },
            new { Id = 15, NameAr = "صبراتة", SortOrder = 15, IsActive = true },
            new { Id = 16, NameAr = "جنزور", SortOrder = 16, IsActive = true },
            new { Id = 17, NameAr = "أبو سليم", SortOrder = 17, IsActive = true },
            new { Id = 18, NameAr = "أبو كماش", SortOrder = 18, IsActive = true },
            new { Id = 19, NameAr = "الأصابعة", SortOrder = 19, IsActive = true },
            new { Id = 20, NameAr = "الجميل", SortOrder = 20, IsActive = true },
            new { Id = 21, NameAr = "الرجبان", SortOrder = 21, IsActive = true },
            new { Id = 22, NameAr = "الرحيبات", SortOrder = 22, IsActive = true },
            new { Id = 23, NameAr = "الزاوية", SortOrder = 23, IsActive = true },
            new { Id = 24, NameAr = "الزنتان", SortOrder = 24, IsActive = true },
            new { Id = 25, NameAr = "الزهراء", SortOrder = 25, IsActive = true },
            new { Id = 26, NameAr = "السبيعة", SortOrder = 26, IsActive = true },
            new { Id = 27, NameAr = "السواني", SortOrder = 27, IsActive = true },
            new { Id = 28, NameAr = "العزيزية", SortOrder = 28, IsActive = true },
            new { Id = 29, NameAr = "القلعة", SortOrder = 29, IsActive = true },
            new { Id = 30, NameAr = "الماية", SortOrder = 30, IsActive = true },
            new { Id = 31, NameAr = "بئر الغنم", SortOrder = 31, IsActive = true },
            new { Id = 32, NameAr = "تاجوراء", SortOrder = 32, IsActive = true },
            new { Id = 33, NameAr = "تاورغاء", SortOrder = 33, IsActive = true },
            new { Id = 34, NameAr = "جادو", SortOrder = 34, IsActive = true },
            new { Id = 35, NameAr = "حي الأندلس", SortOrder = 35, IsActive = true },
            new { Id = 36, NameAr = "درج", SortOrder = 36, IsActive = true },
            new { Id = 37, NameAr = "رقدالين", SortOrder = 37, IsActive = true },
            new { Id = 38, NameAr = "زلطن", SortOrder = 38, IsActive = true },
            new { Id = 39, NameAr = "زوارة", SortOrder = 39, IsActive = true },
            new { Id = 40, NameAr = "سبها", SortOrder = 40, IsActive = true },
            new { Id = 41, NameAr = "سوق الجمعة", SortOrder = 41, IsActive = true },
            new { Id = 42, NameAr = "سوق الخميس امسيحل", SortOrder = 42, IsActive = true },
            new { Id = 43, NameAr = "سوكنة", SortOrder = 43, IsActive = true },
            new { Id = 44, NameAr = "صرمان", SortOrder = 44, IsActive = true },
            new { Id = 45, NameAr = "عين زارة", SortOrder = 45, IsActive = true },
            new { Id = 46, NameAr = "غدامس", SortOrder = 46, IsActive = true },
            new { Id = 47, NameAr = "غريان", SortOrder = 47, IsActive = true },
            new { Id = 48, NameAr = "قصر بن غشير", SortOrder = 48, IsActive = true },
            new { Id = 49, NameAr = "كاباو", SortOrder = 49, IsActive = true },
            new { Id = 50, NameAr = "كعام", SortOrder = 50, IsActive = true },
            new { Id = 51, NameAr = "ككلة", SortOrder = 51, IsActive = true },
            new { Id = 52, NameAr = "ماجر", SortOrder = 52, IsActive = true },
            new { Id = 53, NameAr = "مزدة", SortOrder = 53, IsActive = true },
            new { Id = 54, NameAr = "نالوت", SortOrder = 54, IsActive = true },
            new { Id = 55, NameAr = "هون", SortOrder = 55, IsActive = true },
            new { Id = 56, NameAr = "ودان", SortOrder = 56, IsActive = true },
            new { Id = 57, NameAr = "يفرن", SortOrder = 57, IsActive = true });

        // Seed AppSettings (DATABASE.md §5)
        modelBuilder.Entity<AppSetting>().HasData(
            new { Key = "Park.Name", Value = "منتزه المرح العائلي", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Park.TimeZoneId", Value = "Libya Standard Time", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Entry.DefaultCooldownMinutes", Value = "720", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Entry.MaxGroupSize", Value = "10", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Entry.DenialDedupMinutes", Value = "5", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Entry.PrefillCompanionsFromLastVisit", Value = "true", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Security.MaxFailedLogins", Value = "5", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null },
            new { Key = "Security.LockoutMinutes", Value = "15", UpdatedAtUtc = seedTimeUtc, UpdatedByUserId = (int?)null });
    }
}
