using ParkGate.Domain.Entities;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Entities;

public sealed class OrganizationTests
{
    [Fact]
    public void Constructor_ValidInputs_InitializesCorrectly()
    {
        var org = new Organization("ssf", "صندوق التضامن", 720, isDefault: true);

        org.Code.ShouldBe("SSF");
        org.NameAr.ShouldBe("صندوق التضامن");
        org.CooldownMinutes.ShouldBe(720);
        org.IsDefault.ShouldBeTrue();
        org.IsActive.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("VERY_LONG_CODE_THAT_EXCEEDS_TWENTY_CHARS")]
    public void Constructor_InvalidCode_ThrowsArgumentException(string? invalidCode)
    {
        Should.Throw<ArgumentException>(() => new Organization(invalidCode!, "صندوق التضامن"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidName_ThrowsArgumentException(string? invalidName)
    {
        Should.Throw<ArgumentException>(() => new Organization("SSF", invalidName!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(10081)] // > 7 days (10080)
    public void Constructor_CooldownOutOfRange_ThrowsArgumentOutOfRangeException(int invalidCooldown)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new Organization("SSF", "صندوق التضامن", invalidCooldown));
    }

    [Fact]
    public void Update_ValidInputs_UpdatesFields()
    {
        var org = new Organization("SSF", "صندوق التضامن", 720);
        org.Update("ssf-new", "صندوق التضامن الاجتماعي", 1440);

        org.Code.ShouldBe("SSF-NEW");
        org.NameAr.ShouldBe("صندوق التضامن الاجتماعي");
        org.CooldownMinutes.ShouldBe(1440);
    }

    [Fact]
    public void ActivateAndDeactivate_TogglesIsActive()
    {
        var org = new Organization("SSF", "صندوق التضامن");
        org.Deactivate();
        org.IsActive.ShouldBeFalse();

        org.Activate();
        org.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void SetDefault_UpdatesIsDefault()
    {
        var org = new Organization("SSF", "صندوق التضامن", isDefault: false);
        org.SetDefault(true);
        org.IsDefault.ShouldBeTrue();
    }
}
