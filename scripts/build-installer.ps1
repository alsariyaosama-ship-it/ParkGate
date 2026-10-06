<#
.SYNOPSIS
    Builds the ParkGate release package and compiles the Inno Setup installer.

.DESCRIPTION
    1. Runs dotnet publish using win-x64 profile into dist/client.
    2. Searches for ISCC.exe (Inno Setup Compiler) and compiles scripts/installer.iss.
    3. Outputs the final installer to dist/installer/ParkGate_Setup_v1.0.0.exe.
#>

[CmdletBinding()]
param (
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Write-Host "=== Building ParkGate Release and Installer (v$Version) ===" -ForegroundColor Cyan
Write-Host "Repository Root: $repoRoot"

# 1. Publish WinForms client
$projectPath = Join-Path $repoRoot "src\ParkGate.WinForms\ParkGate.WinForms.csproj"
$publishDir = Join-Path $repoRoot "dist\client"

Write-Host "`n1. Publishing WinForms client to $publishDir..." -ForegroundColor Yellow
dotnet publish $projectPath -c $Configuration /p:PublishProfile=win-x64 --nologo
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}
Write-Host "Client binaries published successfully." -ForegroundColor Green

# 2. Check for Inno Setup compiler (ISCC.exe)
$isccPaths = @(
    "ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
)

$iscc = $null
foreach ($path in $isccPaths) {
    if (Get-Command $path -ErrorAction SilentlyContinue) {
        $iscc = $path
        break
    }
    if (Test-Path $path) {
        $iscc = $path
        break
    }
}

if ($iscc) {
    Write-Host "`n2. Compiling Inno Setup script with $iscc..." -ForegroundColor Yellow
    $issFile = Join-Path $PSScriptRoot "installer.iss"
    & $iscc "/DMyAppVersion=$Version" $issFile
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup compilation failed with exit code $LASTEXITCODE"
    }

    $installerExe = Join-Path $repoRoot "dist\installer\ParkGate_Setup_v$Version.exe"
    if (Test-Path $installerExe) {
        $sizeMb = [Math]::Round((Get-Item $installerExe).Length / 1MB, 2)
        Write-Host "`nInstaller generated successfully: $installerExe ($sizeMb MB)" -ForegroundColor Green
    }
} else {
    Write-Host "`nNotice: Inno Setup 6 compiler (ISCC.exe) not found on this machine." -ForegroundColor DarkYellow
    Write-Host "The client has been published to dist\client."
    Write-Host "To generate the single-file installer, install Inno Setup 6 and run:"
    Write-Host "  ISCC.exe scripts\installer.iss" -ForegroundColor Cyan
}

Write-Host "`n=== Build & Packaging Finished ===" -ForegroundColor Cyan
