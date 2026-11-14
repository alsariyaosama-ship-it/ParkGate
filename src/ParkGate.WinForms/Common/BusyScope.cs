using System.Windows.Forms;

namespace ParkGate.WinForms.Common;

/// <summary>
/// Scoped helper that sets the wait cursor and disables specified controls during async operations,
/// restoring them when disposed.
/// </summary>
public sealed class BusyScope : IDisposable
{
    private readonly Control? _parent;
    private readonly Control[] _controlsToDisable;
    private readonly bool[] _previousEnabled;
    private readonly Cursor _previousCursor;

    public BusyScope(Control? parent, params Control[] controlsToDisable)
    {
        _parent = parent;
        _controlsToDisable = controlsToDisable ?? [];
        _previousEnabled = _controlsToDisable.Select(c => c.Enabled).ToArray();
        _previousCursor = parent?.Cursor ?? Cursors.Default;

        if (_parent != null)
        {
            _parent.Cursor = Cursors.WaitCursor;
        }

        foreach (var control in _controlsToDisable)
        {
            control.Enabled = false;
        }
    }

    public void Dispose()
    {
        // Restore what each control was, not "enabled": a control the caller disabled stays disabled (R4.1).
        for (var i = 0; i < _controlsToDisable.Length; i++)
        {
            _controlsToDisable[i].Enabled = _previousEnabled[i];
        }

        if (_parent != null)
        {
            _parent.Cursor = _previousCursor;
        }
    }
}
