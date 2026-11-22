using System.ComponentModel;
using System.Windows.Forms;
using ParkGate.WinForms.Common;

namespace ParkGate.WinForms.Controls;

/// <summary>
/// A drop-down that filters its list while the operator types (BR-32 city box): the list shows the items whose
/// normalized text contains the normalized typed text (see <see cref="ArabicSearchText"/>), in their original order.
/// The full list is kept apart, so the filter never loses an item. When the box is left, the typed text is
/// resolved to one item or cleared, so it never holds text that is not a list entry.
/// </summary>
public class SearchableComboBox : ComboBox
{
    private List<object> _allItems = [];
    private bool _changingItems;

    public SearchableComboBox()
    {
        // Built-in auto-complete stays off (its default): it would replace the contains-search below, and setting it
        // needs an STA thread, which the UI tests do not have.
        DropDownStyle = ComboBoxStyle.DropDown;
    }

    /// <summary>True while the drop-down list is open; the desk lets Esc close it before it resets the desk (R4.5).</summary>
    [Browsable(false)]
    public bool IsListOpen => DroppedDown;

    /// <summary>Replaces the full list, clears the typed text and the selection.</summary>
    public void SetItems(IEnumerable<object> items)
    {
        _allItems = [.. items];
        ShowItems(_allItems);
        ClearSelection();
    }

    /// <summary>Selects <paramref name="item"/> (one of the full list) and shows the full list again.</summary>
    public void SelectItem(object? item)
    {
        ShowItems(_allItems);
        if (item is null)
        {
            ClearSelection();
            return;
        }

        SelectedItem = item;
    }

    /// <summary>
    /// Turns the typed text into a selection: an item whose text equals it, else the only item that contains it;
    /// otherwise the box is cleared. Empty text means no selection.
    /// </summary>
    public void CommitText()
    {
        var needle = ArabicSearchText.Normalize(Text);
        var match = needle.Length == 0 ? null : FindMatch(needle);

        ShowItems(_allItems);
        if (match is null)
        {
            ClearSelection();
            return;
        }

        SelectedItem = match;
    }

    /// <summary>Filters the list to the typed text and opens it. Internal so the tests can call it; WinForms calls it on typing.</summary>
    internal void ApplyFilter()
    {
        if (_changingItems)
        {
            return;
        }

        var typed = Text;
        // Reading the caret creates the native window, so a box that was never shown (tests) skips it.
        var caret = IsHandleCreated ? SelectionStart : Text.Length;
        var needle = ArabicSearchText.Normalize(typed);
        var matches = needle.Length == 0
            ? _allItems
            : _allItems.Where(i => Contains(i, needle)).ToList();

        ShowItems(matches);
        RestoreText(typed, caret);

        if (IsHandleCreated)
        {
            DroppedDown = matches.Count > 0;
            // WinForms hides the cursor while the list opens from typing.
            Cursor.Current = Cursors.Default;
        }
    }

    protected override void OnTextUpdate(EventArgs e)
    {
        base.OnTextUpdate(e);
        ApplyFilter();
    }

    protected override void OnDropDownClosed(EventArgs e)
    {
        base.OnDropDownClosed(e);

        // A pick leaves the filtered list in place; put the full list back and keep the pick.
        if (!_changingItems && SelectedItem is { } picked && IsSame(picked, ArabicSearchText.Normalize(Text)))
        {
            SelectItem(picked);
        }
    }

    protected override void OnLeave(EventArgs e)
    {
        CommitText();
        base.OnLeave(e);
    }

    protected override void OnValidating(CancelEventArgs e)
    {
        CommitText();
        base.OnValidating(e);
    }

    private object? FindMatch(string needle)
    {
        var equal = _allItems.FirstOrDefault(i => IsSame(i, needle));
        if (equal is not null)
        {
            return equal;
        }

        var containing = _allItems.Where(i => Contains(i, needle)).Take(2).ToList();
        return containing.Count == 1 ? containing[0] : null;
    }

    private static string Key(object item) => ArabicSearchText.Normalize(item.ToString());

    private static bool IsSame(object item, string needle) => Key(item) == needle;

    private static bool Contains(object item, string needle) => Key(item).Contains(needle, StringComparison.Ordinal);

    private void ShowItems(IReadOnlyCollection<object> items)
    {
        _changingItems = true;
        try
        {
            Items.Clear();
            Items.AddRange([.. items]);
        }
        finally
        {
            _changingItems = false;
        }
    }

    private void ClearSelection()
    {
        _changingItems = true;
        try
        {
            SelectedIndex = -1;
            Text = string.Empty;
        }
        finally
        {
            _changingItems = false;
        }
    }

    private void RestoreText(string typed, int caret)
    {
        _changingItems = true;
        try
        {
            Text = typed;
            if (IsHandleCreated)
            {
                SelectionStart = Math.Min(caret, typed.Length);
                SelectionLength = 0;
            }
        }
        finally
        {
            _changingItems = false;
        }
    }
}
