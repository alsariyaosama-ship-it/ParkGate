using System.Drawing;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ParkGate.WinForms.Common;

/// <summary>
/// Provides application-wide Cairo typography with fallback to system fonts.
/// Automatically registers Cairo font into the GDI subsystem and Windows user fonts.
/// </summary>
public static class AppFont
{
    private static readonly PrivateFontCollection FontCollection = new();
    private static FontFamily? _cairoFamily;
    private static bool _initialized;
    private static readonly object SyncLock = new();

    private const uint FrPrivate = 0x10;

    [DllImport("gdi32.dll", EntryPoint = "AddFontResourceExW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int AddFontResourceEx(string lpszFilename, uint fl, IntPtr pdv);

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
                // 1. Check if Cairo font is already available and functioning in OS
                using (var testFont = new Font("Cairo", 10F, FontStyle.Regular, GraphicsUnit.Point))
                {
                    if (testFont.Name.Equals("Cairo", StringComparison.OrdinalIgnoreCase))
                    {
                        _cairoFamily = new FontFamily("Cairo");
                        _initialized = true;
                        return;
                    }
                }

                // 2. Locate font file from application folder or resources
                string? fontPath = null;
                var localFontPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Fonts", "Cairo.ttf");
                if (File.Exists(localFontPath))
                {
                    fontPath = localFontPath;
                }
                else
                {
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
                        fontPath = targetFile;
                    }
                }

                // 3. Register font with GDI32 and user Windows registry if font file found
                if (!string.IsNullOrEmpty(fontPath) && File.Exists(fontPath))
                {
                    // Register into GDI process font table for TextRenderer/WinForms controls
                    AddFontResourceEx(fontPath, FrPrivate, IntPtr.Zero);

                    // Also add to GDI+ PrivateFontCollection
                    FontCollection.AddFontFile(fontPath);

                    // Try user-level Windows font registration so Visual Studio and system know about it
                    try
                    {
                        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var userFontsDir = Path.Combine(localAppData, "Microsoft", "Windows", "Fonts");
                        Directory.CreateDirectory(userFontsDir);
                        var userDest = Path.Combine(userFontsDir, "Cairo.ttf");
                        if (!File.Exists(userDest) && fontPath != userDest)
                        {
                            File.Copy(fontPath, userDest, true);
                        }

                        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Fonts", true);
                        key?.SetValue("Cairo (TrueType)", userDest);
                    }
                    catch
                    {
                        // Non-critical if user registry access is restricted
                    }
                }

                // 4. Resolve FontFamily
                using (var testFont = new Font("Cairo", 10F, FontStyle.Regular, GraphicsUnit.Point))
                {
                    if (testFont.Name.Equals("Cairo", StringComparison.OrdinalIgnoreCase))
                    {
                        _cairoFamily = new FontFamily("Cairo");
                        return;
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

        // 1. Try Cairo from system or resolved family
        if (_cairoFamily != null)
        {
            try
            {
                return new Font(_cairoFamily, size, style, GraphicsUnit.Point);
            }
            catch
            {
                // Variable fonts might fail with specific style constructor
            }
        }

        try
        {
            var font = new Font("Cairo", size, style, GraphicsUnit.Point);
            if (font.Name.Equals("Cairo", StringComparison.OrdinalIgnoreCase))
            {
                return font;
            }
        }
        catch
        {
            // Fall back
        }

        // 2. System fallbacks
        try
        {
            return new Font("Segoe UI", size, style, GraphicsUnit.Point);
        }
        catch
        {
            return new Font(FontFamily.GenericSansSerif, size, style, GraphicsUnit.Point);
        }
    }
}
