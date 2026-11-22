using ParkGate.WinForms.Common;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>F9, BR-32: the city box finds «الخمس» by «خمس» and «مصراتة» by «مصراته».</summary>
public sealed class ArabicSearchTextTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("  طرابلس  ", "طرابلس")]
    [InlineData("بني   وليد", "بني وليد")]
    [InlineData("سوق\tالخميس  امسيحل", "سوق الخميس امسيحل")]
    public void Normalize_TrimsAndCollapsesWhitespace(string? input, string expected)
    {
        ArabicSearchText.Normalize(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("أبو سليم", "ابو سليم")] // alef with hamza above
    [InlineData("إبراهيم", "ابراهيم")] // alef with hamza below
    [InlineData("آمنة", "امنه")] // alef with madda, and ta marbuta in the same word
    [InlineData("القره بوللي", "القره بوللي")]
    public void Normalize_MapsAlefVariantsToPlainAlef(string input, string expected)
    {
        ArabicSearchText.Normalize(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData("مصراتة", "مصراته")]
    [InlineData("مصراته", "مصراته")]
    [InlineData("الزاوية", "الزاويه")]
    public void Normalize_MapsTaMarbutaToHa(string input, string expected)
    {
        ArabicSearchText.Normalize(input).ShouldBe(expected);
    }

    [Fact]
    public void Normalize_MapsAlefMaqsuraToYa()
    {
        ArabicSearchText.Normalize("موسى").ShouldBe("موسي");
    }

    [Fact]
    public void Normalize_DropsTatweelAndHarakat()
    {
        ArabicSearchText.Normalize("مـصـراتَـة").ShouldBe("مصراته");
        ArabicSearchText.Normalize("طَرَابُلْس").ShouldBe("طرابلس");
    }

    [Theory]
    [InlineData("٠٩١٢٣", "09123")]
    [InlineData("۰۹۱۲۳", "09123")]
    [InlineData("المنطقة ١٢", "المنطقه 12")]
    public void Normalize_TurnsArabicIndicDigitsIntoWesternDigits(string input, string expected)
    {
        ArabicSearchText.Normalize(input).ShouldBe(expected);
    }

    [Fact]
    public void Normalize_TypedWithoutDefiniteArticle_IsContainedInTheCityName()
    {
        ArabicSearchText.Normalize("الخمس").ShouldContain(ArabicSearchText.Normalize("خمس"));
        ArabicSearchText.Normalize("مصراتة").ShouldContain(ArabicSearchText.Normalize("مصراته"));
    }
}
