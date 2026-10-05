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
    private readonly Cursor _previousCursor;

    public BusyScope(Control? parent, params Control[] controlsToDisable)
    {
        _parent = parent;
        _controlsToDisable = controlsToDisable ?? [];
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
        foreach (var control in _controlsToDisable)
        {
            control.Enabled = true;
        }

        if (_parent != null)
        {
            _parent.Cursor = _previousCursor;
        }
    }
}
