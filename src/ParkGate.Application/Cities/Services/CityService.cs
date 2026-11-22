using ParkGate.Application.Cities.DTOs;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Persistence;

namespace ParkGate.Application.Cities.Services;

public sealed class CityService : ICityService
{
    private readonly IUnitOfWorkFactory _uowFactory;

    public CityService(IUnitOfWorkFactory uowFactory)
    {
        _uowFactory = uowFactory;
    }

    public async Task<Result<IReadOnlyList<CityDto>>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await using var uow = _uowFactory.Create();
        var list = await uow.Cities.GetAllActiveAsync(cancellationToken);
        IReadOnlyList<CityDto> dtos = list.Select(c => c.ToDto()).ToList();
        return Result<IReadOnlyList<CityDto>>.Success(dtos);
    }
}
