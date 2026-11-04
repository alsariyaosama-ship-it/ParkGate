using ParkGate.Domain.Entities;

namespace ParkGate.Application.AgeCategories.DTOs;

public sealed record AgeCategoryDto(
    int Id,
    string NameAr,
    string? DescriptionAr,
    int SortOrder,
    bool IsActive,
    bool IsCompanionLimitExempt = false);

public sealed record CreateAgeCategoryRequest(
    string NameAr,
    string? DescriptionAr,
    int SortOrder,
    bool IsCompanionLimitExempt = false);

public sealed record UpdateAgeCategoryRequest(
    int Id,
    string NameAr,
    string? DescriptionAr,
    int SortOrder,
    bool IsCompanionLimitExempt = false);

public static class AgeCategoryMappingExtensions
{
    public static AgeCategoryDto ToDto(this AgeCategory category) =>
        new(
            category.Id,
            category.NameAr,
            category.DescriptionAr,
            category.SortOrder,
            category.IsActive,
            category.IsCompanionLimitExempt);
}
