using ParkGate.WinForms.Controls;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>F9, BR-32: the type-to-search city list. Synchronous tests; no window handle is created.</summary>
public sealed class SearchableComboBoxTests
{
    private static readonly string[] Cities = ["الخمس", "زليتن", "مصراتة", "طرابلس", "سوق الجمعة", "سوق الخميس امسيحل"];

    private static SearchableComboBox CreateCombo()
    {
        var combo = new SearchableComboBox();
        combo.SetItems(Cities);
        return combo;
    }

    private static string[] Shown(SearchableComboBox combo) => combo.Items.Cast<object>().Select(i => i.ToString()!).ToArray();

    private static void Type(SearchableComboBox combo, string text)
    {
        combo.Text = text;
        combo.ApplyFilter();
    }

    [Fact]
    public void Constructor_UsesAnEditableListWithoutBuiltInAutoComplete()
    {
        using var combo = new SearchableComboBox();

        combo.DropDownStyle.ShouldBe(System.Windows.Forms.ComboBoxStyle.DropDown);
        combo.AutoCompleteMode.ShouldBe(System.Windows.Forms.AutoCompleteMode.None);
        combo.IsListOpen.ShouldBeFalse();
    }

    [Fact]
    public void SetItems_KeepsTheOriginalOrderNotAlphabetical()
    {
        using var combo = CreateCombo();

        Shown(combo).ShouldBe(Cities);
        combo.SelectedItem.ShouldBeNull();
    }

    [Fact]
    public void Filter_PartOfTheNameWithoutTheArticle_FindsOnlyThatCity()
    {
        using var combo = CreateCombo();

        Type(combo, "خمس");

        Shown(combo).ShouldBe(["الخمس"]);
    }

    [Fact]
    public void Filter_TaMarbutaTypedAsHa_FindsTheCity()
    {
        using var combo = CreateCombo();

        Type(combo, "مصراته");

        Shown(combo).ShouldBe(["مصراتة"]);
    }

    [Fact]
    public void Filter_SeveralMatches_KeepTheirOriginalOrder()
    {
        using var combo = CreateCombo();

        Type(combo, "سوق");

        Shown(combo).ShouldBe(["سوق الجمعة", "سوق الخميس امسيحل"]);
    }

    [Fact]
    public void Filter_EmptyText_ShowsAllItemsInTheOriginalOrder()
    {
        using var combo = CreateCombo();
        Type(combo, "خمس");

        Type(combo, string.Empty);

        Shown(combo).ShouldBe(Cities);
    }

    [Fact]
    public void Filter_KeepsTheTypedText()
    {
        // The caret is restored too, but reading it creates a native window, which these tests avoid.
        using var combo = CreateCombo();
        combo.Text = "مصراته";

        combo.ApplyFilter();

        combo.Text.ShouldBe("مصراته");
        combo.SelectedItem.ShouldBeNull();
    }

    [Fact]
    public void Filter_NoMatch_ShowsAnEmptyListButKeepsTheText()
    {
        using var combo = CreateCombo();

        Type(combo, "xyz");

        Shown(combo).ShouldBeEmpty();
        combo.Text.ShouldBe("xyz");
    }

    [Fact]
    public void CommitText_ExactText_SelectsThatItemAndRestoresTheFullList()
    {
        using var combo = CreateCombo();
        Type(combo, "طرابلس");

        combo.CommitText();

        combo.SelectedItem.ShouldBe("طرابلس");
        combo.Text.ShouldBe("طرابلس");
        Shown(combo).ShouldBe(Cities);
    }

    [Fact]
    public void CommitText_ExactTextWithHamzaAndTaMarbutaVariants_SelectsTheItem()
    {
        using var combo = CreateCombo();
        Type(combo, "مصراته");

        combo.CommitText();

        combo.SelectedItem.ShouldBe("مصراتة");
    }

    [Fact]
    public void CommitText_PartOfOneName_SelectsTheOnlyItemThatContainsIt()
    {
        using var combo = CreateCombo();
        Type(combo, "خمس");

        combo.CommitText();

        combo.SelectedItem.ShouldBe("الخمس");
    }

    [Fact]
    public void CommitText_PartOfSeveralNames_ClearsTheBoxInsteadOfGuessing()
    {
        using var combo = CreateCombo();
        Type(combo, "سوق");

        combo.CommitText();

        combo.SelectedItem.ShouldBeNull();
        combo.Text.ShouldBeEmpty();
    }

    [Fact]
    public void CommitText_UnknownText_ClearsTextAndSelection()
    {
        using var combo = CreateCombo();
        combo.SelectedItem = "زليتن";
        Type(combo, "بلدة غير موجودة");

        combo.CommitText();

        combo.SelectedItem.ShouldBeNull();
        combo.Text.ShouldBeEmpty();
        Shown(combo).ShouldBe(Cities);
    }

    [Fact]
    public void CommitText_EmptyText_MeansNoSelection()
    {
        using var combo = CreateCombo();
        combo.SelectedItem = "زليتن";
        combo.Text = string.Empty;

        combo.CommitText();

        combo.SelectedItem.ShouldBeNull();
    }

    [Fact]
    public void SelectItem_ShowsTheFullListAndSelectsIt()
    {
        using var combo = CreateCombo();
        Type(combo, "خمس");

        combo.SelectItem("زليتن");

        combo.SelectedItem.ShouldBe("زليتن");
        Shown(combo).ShouldBe(Cities);
    }

    [Fact]
    public void SelectItem_Null_ClearsTheBox()
    {
        using var combo = CreateCombo();
        combo.SelectedItem = "زليتن";

        combo.SelectItem(null);

        combo.SelectedItem.ShouldBeNull();
        combo.Text.ShouldBeEmpty();
    }
}
