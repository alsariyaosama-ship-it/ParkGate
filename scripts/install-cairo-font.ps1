# Install Cairo font for the current Windows user
$ErrorActionPreference = 'Stop'

$fontSource = "C:\dev\ParkGate\src\ParkGate.WinForms\Resources\Fonts\Cairo.ttf"
if (-not (Test-Path $fontSource)) {
    Write-Error "Font file not found at $fontSource"
    exit 1
}

$userFontDir = [System.IO.Path]::Combine($env:LOCALAPPDATA, 'Microsoft', 'Windows', 'Fonts')
if (-not (Test-Path $userFontDir)) {
    New-Item -ItemType Directory -Path $userFontDir -Force | Out-Null
}

$destFont = [System.IO.Path]::Combine($userFontDir, 'Cairo.ttf')
Copy-Item -Path $fontSource -Destination $destFont -Force
Write-Host "Font copied to $destFont"

# Register in registry for current user
$regPath = 'HKCU:\Software\Microsoft\Windows NT\CurrentVersion\Fonts'
Set-ItemProperty -Path $regPath -Name 'Cairo (TrueType)' -Value $destFont -Type String
Write-Host "Font registered in registry at $regPath"

# Add font resource via GDI32 and broadcast change
Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public class FontHelper
{
    [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
    public static extern int AddFontResource([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    public const int HWND_BROADCAST = 0xffff;
    public const uint WM_FONTCHANGE = 0x001D;

    public static void NotifyFontChange(string fontPath)
    {
        AddFontResource(fontPath);
        SendMessage((IntPtr)HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
    }
}
"@

[FontHelper]::NotifyFontChange($destFont)
Write-Host "AddFontResource and WM_FONTCHANGE broadcasted successfully."

# Verify with System.Drawing
Add-Type -AssemblyName System.Drawing
$testFont = [System.Drawing.Font]::new('Cairo', 12)
Write-Host "Test Font Name: $($testFont.Name)"
if ($testFont.Name -eq 'Cairo') {
    Write-Host "SUCCESS: Cairo font is now recognized by Windows GDI and System.Drawing!"
} else {
    Write-Warning "Font name is still: $($testFont.Name)"
}
