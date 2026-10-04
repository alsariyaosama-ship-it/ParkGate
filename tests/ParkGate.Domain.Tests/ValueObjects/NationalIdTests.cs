using ParkGate.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.ValueObjects;

public sealed class NationalIdTests
{
    [Theory]
    [InlineData("119850123456", "119850123456")]
    [InlineData("  119850123456  ", "119850123456")]
    [InlineData("1-1985-0123456", "119850123456")]
    [InlineData("1 1985 0123 456", "119850123456")]
    [InlineData("١١٩٨٥٠١٢٣٤٥٦", "119850123456")]
    [InlineData("۱۱۹۸۵۰۱۲۳۴۵۶", "119850123456")]
    [InlineData("  ١-١٩٨٥-٠١٢٣٤٥٦  ", "119850123456")]
    public void TryCreate_Valid12Digits_NormalizesCorrectly(string input, string expected)
    {
        var success = NationalId.TryCreate(input, out var nationalId, out var error);

        success.ShouldBeTrue();
        error.ShouldBeNull();
        nationalId.ShouldNotBeNull();
        nationalId.Value.ShouldBe(expected);
        nationalId.ToString().ShouldBe(expected);
        string stringValue = nationalId;
        stringValue.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("11985012345")] // 11 digits (too short)
    [InlineData("1198501234567")] // 13 digits (too long)
    [InlineData("11985012345A")] // Non-digit
    [InlineData("abcdefghijkl")] // Letters
    public void TryCreate_InvalidInput_Fails(string? input)
    {
        var success = NationalId.TryCreate(input, out var nationalId, out var error);

        success.ShouldBeFalse();
        nationalId.ShouldBeNull();
        error.ShouldNotBeNull();
    }

    [Fact]
    public void Create_ValidInput_ReturnsInstance()
    {
        var nationalId = NationalId.Create("119850123456");
        nationalId.Value.ShouldBe("119850123456");
    }

    [Fact]
    public void Create_InvalidInput_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => NationalId.Create("123"));
    }

    [Fact]
    public void Equality_SameNormalizedValue_AreEqual()
    {
        var id1 = NationalId.Create("119850123456");
        var id2 = NationalId.Create("١-١٩٨٥-٠١٢٣٤٥٦");

        id1.ShouldBe(id2);
        (id1 == id2).ShouldBeTrue();
    }
}
