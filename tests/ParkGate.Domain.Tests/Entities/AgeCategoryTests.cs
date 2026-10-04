using ParkGate.Domain.Entities;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Entities;

public sealed class AgeCategoryTests
{
    [Fact]
    public void Constructor_ValidInputs_InitializesCorrectly()
    {
        var category = new AgeCategory("طفل", "من 3 إلى 12 سنة", 2);

        category.NameAr.ShouldBe("طفل");
        category.DescriptionAr.ShouldBe("من 3 إلى 12 سنة");
        category.SortOrder.ShouldBe(2);
        category.IsActive.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidName_ThrowsArgumentException(string? invalidName)
    {
        Should.Throw<ArgumentException>(() => new AgeCategory(invalidName!, "وصف", 1));
    }

    [Fact]
    public void Update_ValidInputs_UpdatesFields()
    {
        var category = new AgeCategory("طفل", null, 1);
        category.Update("طفل مميز", "وصف جديد", 5);

        category.NameAr.ShouldBe("طفل مميز");
        category.DescriptionAr.ShouldBe("وصف جديد");
        category.SortOrder.ShouldBe(5);
    }

    [Fact]
    public void ActivateAndDeactivate_TogglesIsActive()
    {
        var category = new AgeCategory("طفل", null, 1);
        category.Deactivate();
        category.IsActive.ShouldBeFalse();

        category.Activate();
        category.IsActive.ShouldBeTrue();
    }
}
