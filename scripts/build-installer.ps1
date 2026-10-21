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

$clientAppSettings = Join-Path $publishDir "appsettings.json"
if (Test-Path $clientAppSettings) {
    $json = Get-Content $clientAppSettings -Raw | ConvertFrom-Json
    $json.ConnectionStrings.DefaultConnection = "Server=.\\SQLEXPRESS;Database=ParkGate;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
    $json | ConvertTo-Json -Depth 10 | Set-Content $clientAppSettings -Encoding UTF8
    Write-Host "Configured dist/client appsettings.json for SQLEXPRESS." -ForegroundColor Green
}

# 2. Package database migration bundle for server mode
$dbOutputDir = Join-Path $repoRoot "dist\database"
if (-not (Test-Path $dbOutputDir)) {
    New-Item -ItemType Directory -Path $dbOutputDir -Force | Out-Null
}
$migrationBundleDst = Join-Path $dbOutputDir "ParkGate.Migrations.exe"
$existingBundle = Join-Path $repoRoot "artifacts\production\database\ParkGate.Migrations.exe"

if (Test-Path $existingBundle) {
    Write-Host "`n2. Using existing migration bundle from $existingBundle..." -ForegroundColor Cyan
    Copy-Item $existingBundle $migrationBundleDst -Force
} else {
    Write-Host "`n2. Building self-contained EF Core migration bundle..." -ForegroundColor Yellow
    $infraProject = Join-Path $repoRoot "src\ParkGate.Infrastructure\ParkGate.Infrastructure.csproj"
    dotnet ef migrations bundle --self-contained --runtime win-x64 --context ParkGateDbContext --configuration $Configuration --project $infraProject --startup-project $infraProject --output $migrationBundleDst
}

# 3. Check for Inno Setup compiler (ISCC.exe)
$isccPaths = @(
    "ISCC.exe",
    "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe",
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
