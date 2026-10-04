using ParkGate.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.ValueObjects;

public sealed class CardNumberTests
{
    [Theory]
    [InlineData("12345", "12345")]
    [InlineData("  12345  ", "12345")]
    [InlineData("ssf-1234-a", "SSF-1234-A")]
    [InlineData("  ssf-١٢٣٤-a  ", "SSF-1234-A")]
    [InlineData("card  ۰۱۲۳  x", "CARD 0123 X")]
    [InlineData("abc   \t   def", "ABC DEF")]
    [InlineData("بطاقة-١٢٣", "بطاقة-123")]
    [InlineData("A-12-B", "A-12-B")]
    public void TryCreate_ValidInput_NormalizesCorrectly(string input, string expected)
    {
        var success = CardNumber.TryCreate(input, out var card, out var error);

        success.ShouldBeTrue();
        error.ShouldBeNull();
        card.ShouldNotBeNull();
        card.Value.ShouldBe(expected);
        card.ToString().ShouldBe(expected);
        string stringValue = card;
        stringValue.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n\r")]
    public void TryCreate_EmptyOrWhitespace_Fails(string? input)
    {
        var success = CardNumber.TryCreate(input, out var card, out var error);

        success.ShouldBeFalse();
        card.ShouldBeNull();
        error.ShouldNotBeNull();
    }

    [Fact]
    public void TryCreate_Exceeds50Characters_Fails()
    {
        var longCard = new string('A', 51);
        var success = CardNumber.TryCreate(longCard, out var card, out var error);

        success.ShouldBeFalse();
        card.ShouldBeNull();
        error.ShouldNotBeNull();
        error.ShouldContain("50");
    }

    [Fact]
    public void Create_ValidInput_ReturnsInstance()
    {
        var card = CardNumber.Create("ssf-٠٠١");
        card.Value.ShouldBe("SSF-001");
    }

    [Fact]
    public void Create_InvalidInput_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => CardNumber.Create("   "));
    }

    [Fact]
    public void Equality_SameNormalizedValue_AreEqual()
    {
        var card1 = CardNumber.Create("ssf-123");
        var card2 = CardNumber.Create("  SSF-١٢٣  ");

        card1.ShouldBe(card2);
        (card1 == card2).ShouldBeTrue();
    }
}
