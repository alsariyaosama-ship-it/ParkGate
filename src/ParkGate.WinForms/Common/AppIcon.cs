using System.Drawing;
using System.Reflection;

namespace ParkGate.WinForms.Common;

/// <summary>
/// Provides application-wide icon and image assets for ParkGate windows and controls.
/// </summary>
public static class AppIcon
{
    private static readonly Lazy<Icon?> _appIcon = new(() =>
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ParkGate.WinForms.Resources.Icons.app.ico");
            if (stream != null)
            {
                return new Icon(stream);
            }

            var iconPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Icons", "app.ico");
            if (File.Exists(iconPath))
            {
                return new Icon(iconPath);
            }
        }
        catch
        {
            // Fallback gracefully
        }

        return null;
    });

    private static readonly Lazy<Image?> _appImage = new(() =>
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ParkGate.WinForms.Resources.Icons.app.png");
            if (stream != null)
            {
                using var memStream = new MemoryStream();
                stream.CopyTo(memStream);
                memStream.Position = 0;
                return Image.FromStream(memStream);
            }

            var pngPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Icons", "app.png");
            if (File.Exists(pngPath))
            {
                using var fs = File.OpenRead(pngPath);
                using var memStream = new MemoryStream();
                fs.CopyTo(memStream);
                memStream.Position = 0;
                return Image.FromStream(memStream);
            }
        }
        catch
        {
            // Fallback gracefully
        }

        return null;
    });

    public static Icon? Current => _appIcon.Value;

    public static Image? Image => _appImage.Value;
}
