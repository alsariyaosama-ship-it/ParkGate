using ParkGate.Domain.Entities;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Entities;

public sealed class CityTests
{
    [Fact]
    public void Constructor_ValidInputs_InitializesCorrectly()
    {
        var city = new City("الخمس", 1);

        city.NameAr.ShouldBe("الخمس");
        city.SortOrder.ShouldBe(1);
        city.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_NameWithSpaces_TrimsName()
    {
        var city = new City("  طرابلس  ", 4);

        city.NameAr.ShouldBe("طرابلس");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyName_ThrowsArgumentException(string? invalidName)
    {
        Should.Throw<ArgumentException>(() => new City(invalidName!, 1));
    }

    [Fact]
    public void Constructor_NameLongerThanMax_ThrowsArgumentException()
    {
        var tooLong = new string('م', City.MaxNameLength + 1);

        Should.Throw<ArgumentException>(() => new City(tooLong, 1));
    }

    [Fact]
    public void Constructor_NameOfMaxLength_Succeeds()
    {
        var city = new City(new string('م', City.MaxNameLength), 1);

        city.NameAr.Length.ShouldBe(City.MaxNameLength);
    }
}
