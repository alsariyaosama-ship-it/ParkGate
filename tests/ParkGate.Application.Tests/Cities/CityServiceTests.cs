using ParkGate.Application.Cities.Services;
using ParkGate.Application.Tests.Common;
using Shouldly;
using Xunit;

namespace ParkGate.Application.Tests.Cities;

public sealed class CityServiceTests
{
    private readonly FakeUnitOfWorkFactory _uowFactory = new();
    private readonly CityService _sut;

    public CityServiceTests()
    {
        _sut = new CityService(_uowFactory);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsActiveCitiesInTheFixedOrderNotAlphabetical()
    {
        var cities = _uowFactory.CurrentUow.CitiesFake;
        cities.Add("طرابلس", 4);
        cities.Add("الخمس", 1);
        cities.Add("زليتن", 2, isActive: false);
        cities.Add("مصراتة", 3);

        var result = await _sut.GetActiveAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.Select(c => c.NameAr).ShouldBe(["الخمس", "مصراتة", "طرابلس"]);
    }

    [Fact]
    public async Task GetActiveAsync_NoCities_ReturnsEmptyList()
    {
        var result = await _sut.GetActiveAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }
}
