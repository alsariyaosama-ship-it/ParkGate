namespace ParkGate.WinForms.Common;

/// <summary>
/// Decides when to tell an admin that Cairo is not installed for GDI+ (R4.10). Designer files create fonts with
/// <c>new Font("Cairo", …)</c>, which falls back to the default font when Cairo is only in the app's private
/// collection (an xcopy deployment). The installer installs it; the warning points admins to that fix.
/// </summary>
public sealed class FontInstallWarning
{
    /// <summary>One per process: an admin sees the warning once per run until the font is installed.</summary>
    public static FontInstallWarning ForThisProcess { get; } = new(() => AppFont.IsCairoInstalled);

    private readonly Func<bool> _isCairoInstalled;
    private bool _shown;

    public FontInstallWarning(Func<bool> isCairoInstalled)
    {
        _isCairoInstalled = isCairoInstalled;
    }

    public bool IsMissing => !_isCairoInstalled();

    public bool ShouldNotify(bool isAdmin)
    {
        if (_shown || !isAdmin || !IsMissing)
        {
            return false;
        }

        _shown = true;
        return true;
    }
}
