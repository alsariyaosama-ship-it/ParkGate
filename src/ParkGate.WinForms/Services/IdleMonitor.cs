using System.Windows.Forms;

namespace ParkGate.WinForms.Services;

/// <summary>
/// Watches keyboard and mouse input for the whole application (an <see cref="IMessageFilter"/>) and says when the
/// desk has been idle for the auto-lock time (FR-AUTH-06, R5.10). A card scanner types keys, so a scan counts as input.
/// </summary>
public sealed class IdleMonitor : IMessageFilter
{
    private const int WmKeyDown = 0x0100;
    private const int WmSysKeyDown = 0x0104;
    private const int WmMouseMove = 0x0200;
    private const int WmLButtonDown = 0x0201;
    private const int WmRButtonDown = 0x0204;
    private const int WmMButtonDown = 0x0207;
    private const int WmMouseWheel = 0x020A;

    private readonly TimeProvider _timeProvider;
    private DateTimeOffset _lastInputUtc;
    private IntPtr? _lastPointer;

    public IdleMonitor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _lastInputUtc = _timeProvider.GetUtcNow();
    }

    /// <summary>True when auto-lock is on (<paramref name="autoLockMinutes"/> &gt; 0) and no input came for that long.</summary>
    public bool IsIdle(int autoLockMinutes) =>
        autoLockMinutes > 0 &&
        _timeProvider.GetUtcNow() - _lastInputUtc >= TimeSpan.FromMinutes(autoLockMinutes);

    /// <summary>Starts the idle time again, e.g. after the user signs back in.</summary>
    public void Reset() => _lastInputUtc = _timeProvider.GetUtcNow();

    /// <summary>Notes user input; never consumes the message.</summary>
    public bool PreFilterMessage(ref Message m)
    {
        if (IsUserInput(m))
        {
            Reset();
        }

        return false;
    }

    private bool IsUserInput(Message m)
    {
        switch (m.Msg)
        {
            case WmKeyDown:
            case WmSysKeyDown:
            case WmLButtonDown:
            case WmRButtonDown:
            case WmMButtonDown:
            case WmMouseWheel:
                return true;
            case WmMouseMove:
                // Windows repeats WM_MOUSEMOVE at the same point; only a real move is input. The point is per window,
                // so moving into another window can count once more — harmless.
                var moved = _lastPointer != m.LParam;
                _lastPointer = m.LParam;
                return moved;
            default:
                return false;
        }
    }
}
