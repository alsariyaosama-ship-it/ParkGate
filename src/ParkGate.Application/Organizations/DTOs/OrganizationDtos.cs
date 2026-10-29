using ParkGate.Domain.Entities;

namespace ParkGate.Application.Organizations.DTOs;

public sealed record OrganizationDto(
    int Id,
    string Code,
    string NameAr,
    int? CooldownMinutes,
    bool IsDefault,
    bool IsActive,
    int? MaxFreeCompanions = null,
    bool GiftEnabled = false,
    decimal GiftValue = 0m,
    int GiftIntervalDays = 30);

public sealed record CreateOrganizationRequest(
    string Code,
    string NameAr,
    int? CooldownMinutes = null,
    bool IsDefault = false);

public sealed record UpdateOrganizationRequest(
    int Id,
    string Code,
    string NameAr,
    int? CooldownMinutes,
    bool IsDefault);

public sealed record SetOrganizationPolicyRequest(
    int OrganizationId,
    int? MaxFreeCompanions,
    bool GiftEnabled,
    decimal GiftValue,
    int GiftIntervalDays);

public static class OrganizationMappingExtensions
{
    public static OrganizationDto ToDto(this Organization org) =>
        new(
            org.Id,
            org.Code,
            org.NameAr,
            org.CooldownMinutes,
            org.IsDefault,
            org.IsActive,
            org.MaxFreeCompanions,
            org.GiftEnabled,
            org.GiftValue,
            org.GiftIntervalDays);
}
