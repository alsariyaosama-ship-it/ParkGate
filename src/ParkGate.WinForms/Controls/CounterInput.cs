using System.ComponentModel;
using System.Windows.Forms;
using ParkGate.Domain.Common;

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

    public CounterInput()
    {
        InitializeComponent();
        WireEvents();
        UpdateButtonsState();
    }

    [Browsable(true)]
    [Category("Appearance")]
    [DefaultValue("الفئة:")]
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
        if (e.KeyCode == Keys.Up)
        {
            Increment();
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Down)
        {
            Decrement();
            e.Handled = true;
        }
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
