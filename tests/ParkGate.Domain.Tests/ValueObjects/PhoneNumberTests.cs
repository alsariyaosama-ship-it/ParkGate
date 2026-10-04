using ParkGate.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.ValueObjects;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData("0912345678", "0912345678")]
    [InlineData("0921234567", "0921234567")]
    [InlineData("+218912345678", "0912345678")]
    [InlineData("00218912345678", "0912345678")]
    [InlineData("218912345678", "0912345678")]
    [InlineData("912345678", "0912345678")]
    [InlineData("+218 (91) 234-5678", "0912345678")]
    [InlineData("091-234-5678", "0912345678")]
    [InlineData("  091 234 56 78  ", "0912345678")]
    [InlineData("٠٩١٢٣٤٥٦٧٨", "0912345678")]
    [InlineData("+٢١٨٩١٢٣٤٥٦٧٨", "0912345678")]
    [InlineData("۰۹۱۲۳۴۵۶۷۸", "0912345678")]
    public void TryCreate_ValidLibyanMobile_NormalizesTo09Format(string input, string expected)
    {
        var success = PhoneNumber.TryCreate(input, out var phone, out var error);

        success.ShouldBeTrue();
        error.ShouldBeNull();
        phone.ShouldNotBeNull();
        phone.Value.ShouldBe(expected);
        phone.ToString().ShouldBe(expected);
        string stringValue = phone;
        stringValue.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("0211234567")] // Landline (starts with 021, not 09)
    [InlineData("091234567")]   // 9 digits starting with 09 (too short)
    [InlineData("09123456789")] // 11 digits (too long)
    [InlineData("091234567A")] // Non-digit
    [InlineData("+12345678901")] // Non-Libyan country code
    public void TryCreate_InvalidInput_Fails(string? input)
    {
        var success = PhoneNumber.TryCreate(input, out var phone, out var error);

        success.ShouldBeFalse();
        phone.ShouldBeNull();
        error.ShouldNotBeNull();
    }

    [Fact]
    public void Create_ValidInput_ReturnsInstance()
    {
        var phone = PhoneNumber.Create("0912345678");
        phone.Value.ShouldBe("0912345678");
    }

    [Fact]
    public void Create_InvalidInput_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => PhoneNumber.Create("1234"));
    }

    [Fact]
    public void Equality_SameNormalizedValue_AreEqual()
    {
        var phone1 = PhoneNumber.Create("0912345678");
        var phone2 = PhoneNumber.Create("+218 (91) 234-5678");

        phone1.ShouldBe(phone2);
        (phone1 == phone2).ShouldBeTrue();
    }
}
