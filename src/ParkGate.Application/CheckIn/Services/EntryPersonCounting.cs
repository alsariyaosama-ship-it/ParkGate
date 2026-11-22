using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Domain.Entities;

namespace ParkGate.Application.CheckIn.Services;

/// <summary>
/// The one place that decides who counts in the person total printed on the entry receipt (BR-33): the holder and every
/// companion except those of a companion-limit-exempt category (infants). Infants stay in the breakdown.
/// </summary>
public static class EntryPersonCounting
{
    /// <summary>Holder plus companions whose category is not companion-limit-exempt. An unknown category counts.</summary>
    public static int CountedPersons(IEnumerable<CompanionRequestDto> companions, IEnumerable<AgeCategory> categories)
    {
        var exemptIds = categories.Where(c => c.IsCompanionLimitExempt).Select(c => c.Id).ToHashSet();
        return 1 + companions.Where(c => !exemptIds.Contains(c.AgeCategoryId)).Sum(c => c.Count);
    }

    /// <summary>Companions with their Arabic category name, in category sort order (unknown categories last).</summary>
    public static IReadOnlyList<CompanionCountDto> ToCompanionDtos(IEnumerable<CompanionRequestDto> companions, IEnumerable<AgeCategory> categories)
    {
        var byId = categories.ToDictionary(c => c.Id);
        return companions
            .OrderBy(c => byId.TryGetValue(c.AgeCategoryId, out var category) ? category.SortOrder : int.MaxValue)
            .ThenBy(c => c.AgeCategoryId)
            .Select(c => new CompanionCountDto(c.AgeCategoryId, byId.TryGetValue(c.AgeCategoryId, out var category) ? category.NameAr : string.Empty, c.Count))
            .ToList();
    }
}
