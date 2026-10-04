using ParkGate.Domain.Common;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Common;

public sealed class DigitNormalizerTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("12345", "12345")]
    [InlineData("٠١٢٣٤٥٦٧٨٩", "0123456789")]
    [InlineData("۰۱۲۳۴۵۶۷۸۹", "0123456789")]
    [InlineData("Card-١٢٣-B", "Card-123-B")]
    [InlineData("Hello World", "Hello World")]
    public void NormalizeDigits_ReturnsExpectedNormalizedString(string? input, string expected)
    {
        var result = DigitNormalizer.NormalizeDigits(input);
        result.ShouldBe(expected);
    }
}
