using System.Drawing;
using System.Drawing.Text;
using System.Reflection;

namespace ParkGate.WinForms.Common;

/// <summary>
/// Provides application-wide Cairo typography with fallback to system fonts.
/// </summary>
public static class AppFont
{
    private static readonly PrivateFontCollection FontCollection = new();
    private static FontFamily? _cairoFamily;
    private static bool _initialized;
    private static readonly object SyncLock = new();

    public static string FamilyName
    {
        get
        {
            Initialize();
            return _cairoFamily?.Name ?? "Cairo";
        }
    }

    public static FontFamily FontFamily
    {
        get
        {
            Initialize();
            return _cairoFamily ?? FontFamily.GenericSansSerif;
        }
    }

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        lock (SyncLock)
        {
            if (_initialized)
            {
                return;
            }

            try
            {
                // 1. Check if Cairo font is already installed in the OS
                using var installed = new InstalledFontCollection();
                var osFont = installed.Families.FirstOrDefault(f => f.Name.Equals("Cairo", StringComparison.OrdinalIgnoreCase));
                if (osFont != null)
                {
                    _cairoFamily = new FontFamily(osFont.Name);
                    _initialized = true;
                    return;
                }

                // 2. Try loading from application folder
                var localFontPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Fonts", "Cairo.ttf");
                if (File.Exists(localFontPath))
                {
                    FontCollection.AddFontFile(localFontPath);
                }
                else
                {
                    // 3. Extract embedded resource to %ProgramData%\ParkGate\fonts\Cairo.ttf
                    var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    var fontDir = Path.Combine(programData, "ParkGate", "fonts");
                    Directory.CreateDirectory(fontDir);
                    var targetFile = Path.Combine(fontDir, "Cairo.ttf");

                    if (!File.Exists(targetFile) || new FileInfo(targetFile).Length == 0)
                    {
                        var assembly = Assembly.GetExecutingAssembly();
                        using var stream = assembly.GetManifestResourceStream("ParkGate.WinForms.Resources.Fonts.Cairo.ttf");
                        if (stream != null)
                        {
                            using var fs = new FileStream(targetFile, FileMode.Create, FileAccess.Write);
                            stream.CopyTo(fs);
                        }
                    }

                    if (File.Exists(targetFile))
                    {
                        FontCollection.AddFontFile(targetFile);
                    }
                }

                if (FontCollection.Families.Length > 0)
                {
                    _cairoFamily = FontCollection.Families.FirstOrDefault(f => f.Name.Equals("Cairo", StringComparison.OrdinalIgnoreCase))
                                   ?? FontCollection.Families[0];
                }
            }
            catch
            {
                _cairoFamily = null;
            }
            finally
            {
                _initialized = true;
            }
        }
    }

    public static Font CreateFont(float size, FontStyle style = FontStyle.Regular)
    {
        Initialize();

        if (_cairoFamily != null)
        {
            try
            {
                return new Font(_cairoFamily, size, style, GraphicsUnit.Point);
            }
            catch
            {
                // If specific style fails with variable font, fallback to name-based constructor
                try
                {
                    return new Font(_cairoFamily.Name, size, style, GraphicsUnit.Point);
                }
                catch
                {
                    // Fall back
                }
            }
        }

        try
        {
            return new Font("Cairo", size, style, GraphicsUnit.Point);
        }
        catch
        {
            return new Font("Segoe UI", size, style, GraphicsUnit.Point);
        }
    }
}
