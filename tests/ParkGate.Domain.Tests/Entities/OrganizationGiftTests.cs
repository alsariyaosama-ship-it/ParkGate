using ParkGate.Domain.Entities;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Entities;

public sealed class OrganizationGiftTests
{
    [Fact]
    public void ConfigureGift_ValidEnabled_SetsProperties()
    {
        var org = new Organization("TEST", "Test Org");
        org.ConfigureGift(true, 50.1234m, 15);

        org.GiftEnabled.ShouldBeTrue();
        org.GiftValue.ShouldBe(50.123m);
        org.GiftIntervalDays.ShouldBe(15);
    }

    [Fact]
    public void ConfigureGift_EnabledWithZeroValue_Throws()
    {
        var org = new Organization("TEST", "Test Org");
        Should.Throw<ArgumentOutOfRangeException>(() => org.ConfigureGift(true, 0m, 30));
    }

    [Fact]
    public void ConfigureGift_EnabledWithNegativeValue_Throws()
    {
        var org = new Organization("TEST", "Test Org");
        Should.Throw<ArgumentOutOfRangeException>(() => org.ConfigureGift(true, -5m, 30));
    }

    [Fact]
    public void ConfigureGift_IntervalTooLow_Throws()
    {
        var org = new Organization("TEST", "Test Org");
        Should.Throw<ArgumentOutOfRangeException>(() => org.ConfigureGift(true, 10m, 0));
    }

    [Fact]
    public void ConfigureGift_IntervalTooHigh_Throws()
    {
        var org = new Organization("TEST", "Test Org");
        Should.Throw<ArgumentOutOfRangeException>(() => org.ConfigureGift(true, 10m, 366));
    }

    [Fact]
    public void ConfigureGift_DisabledWithZeroValue_DoesNotThrow()
    {
        var org = new Organization("TEST", "Test Org");
        org.ConfigureGift(false, 0m, 30);
        org.GiftEnabled.ShouldBeFalse();
        org.GiftValue.ShouldBe(0m);
    }

    [Fact]
    public void ConfigureGift_DisabledWithNegativeValue_Throws()
    {
        var org = new Organization("TEST", "Test Org");
        Should.Throw<ArgumentOutOfRangeException>(() => org.ConfigureGift(false, -1m, 30));
    }

    [Fact]
    public void SetMaxFreeCompanions_Null_Allowed()
    {
        var org = new Organization("TEST", "Test Org");
        org.SetMaxFreeCompanions(null);
        org.MaxFreeCompanions.ShouldBeNull();
    }

    [Fact]
    public void SetMaxFreeCompanions_Zero_Allowed()
    {
        var org = new Organization("TEST", "Test Org");
        org.SetMaxFreeCompanions(0);
        org.MaxFreeCompanions.ShouldBe(0);
    }

    [Fact]
    public void SetMaxFreeCompanions_50_Allowed()
    {
        var org = new Organization("TEST", "Test Org");
        org.SetMaxFreeCompanions(50);
        org.MaxFreeCompanions.ShouldBe(50);
    }

    [Fact]
    public void SetMaxFreeCompanions_51_Throws()
    {
        var org = new Organization("TEST", "Test Org");
        Should.Throw<ArgumentOutOfRangeException>(() => org.SetMaxFreeCompanions(51));
    }

    [Fact]
    public void SetMaxFreeCompanions_Minus1_Throws()
    {
        var org = new Organization("TEST", "Test Org");
        Should.Throw<ArgumentOutOfRangeException>(() => org.SetMaxFreeCompanions(-1));
    }
}

