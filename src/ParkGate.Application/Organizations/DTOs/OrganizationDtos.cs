using ParkGate.Domain.Entities;

namespace ParkGate.Application.Organizations.DTOs;

public sealed record OrganizationDto(
    int Id,
    string Code,
    string NameAr,
    int? CooldownMinutes,
    bool IsDefault,
    bool IsActive);

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

public static class OrganizationMappingExtensions
{
    public static OrganizationDto ToDto(this Organization org) =>
        new(
            org.Id,
            org.Code,
            org.NameAr,
            org.CooldownMinutes,
            org.IsDefault,
            org.IsActive);
}
