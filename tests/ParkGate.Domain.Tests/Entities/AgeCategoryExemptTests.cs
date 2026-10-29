using ParkGate.Domain.Entities;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Entities;

public sealed class AgeCategoryExemptTests
{
    [Fact]
    public void IsCompanionLimitExempt_DefaultFalse()
    {
        var category = new AgeCategory("بالغ", null, 1);
        category.IsCompanionLimitExempt.ShouldBeFalse();
    }

    [Fact]
    public void SetCompanionLimitExempt_True_Works()
    {
        var category = new AgeCategory("رضيع", null, 1);
        category.SetCompanionLimitExempt(true);
        category.IsCompanionLimitExempt.ShouldBeTrue();
    }

    [Fact]
    public void SetCompanionLimitExempt_False_Works()
    {
        var category = new AgeCategory("رضيع", null, 1);
        category.SetCompanionLimitExempt(true);
        category.SetCompanionLimitExempt(false);
        category.IsCompanionLimitExempt.ShouldBeFalse();
    }
}
