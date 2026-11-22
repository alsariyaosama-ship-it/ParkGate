using ParkGate.Application.Cities.DTOs;
using ParkGate.Application.Common;

namespace ParkGate.Application.Cities.Services;

/// <summary>Reads the fixed list of cities a visitor can come from (BR-32).</summary>
public interface ICityService
{
    /// <summary>Active cities in their fixed display order.</summary>
    Task<Result<IReadOnlyList<CityDto>>> GetActiveAsync(CancellationToken cancellationToken = default);
}
