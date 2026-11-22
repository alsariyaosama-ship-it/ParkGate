using ParkGate.Domain.Entities;

namespace ParkGate.Application.Cities.DTOs;

public sealed record CityDto(int Id, string NameAr);

public static class CityMappingExtensions
{
    public static CityDto ToDto(this City city) => new(city.Id, city.NameAr);
}
