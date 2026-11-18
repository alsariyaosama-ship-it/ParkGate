using System.ComponentModel;
using System.Windows.Forms;
using ParkGate.Domain.Common;
using ParkGate.WinForms.Common;

namespace ParkGate.WinForms.Controls;

/// <summary>
/// Numeric companion counter control per SPEC §5.3 zone 5 and AGENTS.md §5.9.
/// Provides [-] [Value] [+] buttons with keyboard arrows, direct typing,
/// and automatic normalization of Arabic-Indic / Persian digits.
/// </summary>
public partial class CounterInput : UserControl
{
    private int _value = 0;
    private int _minValue = 0;
    private int _maxValue = 99;
    private bool _isUpdatingText;

    public event EventHandler? ValueChanged;

    /// <summary>Enter in the value box (R5.4): the desk records the entry.</summary>
    public event EventHandler? ConfirmRequested;

    public CounterInput()
    {
        InitializeComponent();
        WireEvents();
        UpdateButtonsState();
    }

    [Browsable(true)]
    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string Title
    {
        get => _lblTitle.Text;
        set => _lblTitle.Text = value;
    }

    [Browsable(true)]
    [Category("Behavior")]
    [DefaultValue(0)]
    public int Value
    {
        get => _value;
        set
        {
            var clamped = Math.Clamp(value, _minValue, _maxValue);
            if (_value != clamped)
            {
                _value = clamped;
                UpdateTextBox();
                UpdateButtonsState();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (_txtValue.Text != clamped.ToString())
            {
                UpdateTextBox();
            }
        }
    }

    [Browsable(true)]
    [Category("Behavior")]
    [DefaultValue(0)]
    public int MinValue
    {
        get => _minValue;
        set
        {
            _minValue = value;
            if (_value < _minValue)
            {
                Value = _minValue;
            }
            UpdateButtonsState();
        }
    }

    [Browsable(true)]
    [Category("Behavior")]
    [DefaultValue(99)]
    public int MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            if (_value > _maxValue)
            {
                Value = _maxValue;
            }
            UpdateButtonsState();
        }
    }

    public void Reset()
    {
        Value = _minValue;
    }

    /// <summary>Puts the cursor in the value box with the number selected, ready to type over (R5.4).</summary>
    public void FocusValue()
    {
        _txtValue.Focus();
        _txtValue.SelectAll();
    }

    /// <summary>True while the value still comes from the visitor's last visit (R5.5).</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsMarkedAsPrefilled { get; private set; }

    /// <summary>Tints the value box while it shows the last visit's count, so it does not pass unnoticed.</summary>
    public void MarkAsPrefilled(bool marked)
    {
        IsMarkedAsPrefilled = marked;
        _txtValue.BackColor = marked ? Theme.WarningTint : Theme.Surface;
    }

    private void WireEvents()
    {
        _btnDecrement.Click += (_, _) => Decrement();
        _btnIncrement.Click += (_, _) => Increment();

        _txtValue.TextChanged += OnTextBoxTextChanged;
        _txtValue.KeyDown += OnTextBoxKeyDown;
    }

    public void Increment()
    {
        if (_value < _maxValue)
        {
            Value++;
        }
    }

    public void Decrement()
    {
        if (_value > _minValue)
        {
            Value--;
        }
    }

    private void OnTextBoxTextChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingText)
        {
            return;
        }

        var rawText = _txtValue.Text;
        var normalized = DigitNormalizer.NormalizeDigits(rawText);

        if (normalized != rawText)
        {
            _isUpdatingText = true;
            var selStart = _txtValue.SelectionStart;
            _txtValue.Text = normalized;
            _txtValue.SelectionStart = Math.Min(selStart, _txtValue.Text.Length);
            _isUpdatingText = false;
        }

        if (int.TryParse(normalized, out var parsed))
        {
            var clamped = Math.Clamp(parsed, _minValue, _maxValue);
            if (_value != clamped)
            {
                _value = clamped;
                UpdateButtonsState();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (string.IsNullOrWhiteSpace(normalized))
        {
            // Allowed while typing, keep current _value or 0 until blur
            UpdateButtonsState();
        }
    }

    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Up:
            case Keys.Add:
            case Keys.Oemplus: // «+» is Shift with this key, «=» without; both count up
                Increment();
                break;
            case Keys.Down:
            case Keys.Subtract:
            case Keys.OemMinus:
                Decrement();
                break;
            case Keys.Enter:
                ConfirmRequested?.Invoke(this, EventArgs.Empty);
                break;
            default:
                return;
        }

        // The key is a command, not text: «+» must not end up in the value box.
        e.Handled = true;
        e.SuppressKeyPress = true;
    }

    private void OnControlLeave(object? sender, EventArgs e)
    {
        // On blur, normalize display to current numeric value
        UpdateTextBox();
    }

    private void UpdateTextBox()
    {
        _isUpdatingText = true;
        _txtValue.Text = _value.ToString();
        _isUpdatingText = false;
    }

    private void UpdateButtonsState()
    {
        _btnDecrement.Enabled = _value > _minValue;
        _btnIncrement.Enabled = _value < _maxValue;
    }
}
