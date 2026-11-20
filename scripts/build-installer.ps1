<#
.SYNOPSIS
    Builds the ParkGate release package and compiles the Inno Setup installer.

.DESCRIPTION
    1. Publishes the WinForms client self-contained into dist/client (desk PCs need no .NET runtime).
    2. Builds a fresh EF Core migration bundle into dist/database (never a leftover one, R8.4).
    3. Publishes the Sync Agent self-contained into dist/sync-agent (installed on the server only).
    4. Compiles scripts/installer.iss into dist/installer/ParkGate_Setup_v<Version>.exe.
#>

[CmdletBinding()]
param (
    [string]$Configuration = "Release",
    [string]$Version = "1.1.0",
    [string]$RuntimeVersion = "10.0.15"
)

$ErrorActionPreference = "Stop"
# The .NET runtime bundled into self-contained apps. Keep it on the latest servicing release: the SDK alone
# bundles the runtime it shipped with (10.0.401 -> 10.0.12, whose Pkcs has GHSA-846f-f764-76mx; R8.2).

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Write-Host "=== Building ParkGate Release and Installer (v$Version) ===" -ForegroundColor Cyan
Write-Host "Repository Root: $repoRoot"

# 1. Publish WinForms client
$projectPath = Join-Path $repoRoot "src\ParkGate.WinForms\ParkGate.WinForms.csproj"
$publishDir = Join-Path $repoRoot "dist\client"

Write-Host "`n1. Publishing WinForms client to $publishDir..." -ForegroundColor Yellow
# Start from an empty folder so files removed from the app do not linger in the installer.
foreach ($dir in @("dist\client", "dist\database", "dist\sync-agent")) {
    $full = Join-Path $repoRoot $dir
    if (Test-Path $full) { Remove-Item $full -Recurse -Force }
}
dotnet publish $projectPath -c $Configuration /p:PublishProfile=win-x64 -p:SelfContained=true -p:RuntimeFrameworkVersion=$RuntimeVersion --nologo
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}
Write-Host "Client binaries published successfully." -ForegroundColor Green

$clientAppSettings = Join-Path $publishDir "appsettings.json"
if (Test-Path $clientAppSettings) {
    $json = Get-Content $clientAppSettings -Raw | ConvertFrom-Json
    # One backslash: PowerShell has no "\\" escape (R8.1).
    $productionServer = ".\SQLEXPRESS"
    $json.ConnectionStrings.DefaultConnection = "Server=$productionServer;Database=ParkGate;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
    $json | ConvertTo-Json -Depth 10 | Set-Content $clientAppSettings -Encoding UTF8
    $written = (Get-Content $clientAppSettings -Raw | ConvertFrom-Json).ConnectionStrings.DefaultConnection
    if (-not $written.StartsWith("Server=$productionServer;")) {
        throw "dist/client appsettings.json has a wrong server: $written"
    }
    Write-Host "Configured dist/client appsettings.json for SQLEXPRESS." -ForegroundColor Green
}

# 2. Package database migration bundle for server mode
$dbOutputDir = Join-Path $repoRoot "dist\database"
if (-not (Test-Path $dbOutputDir)) {
    New-Item -ItemType Directory -Path $dbOutputDir -Force | Out-Null
}
$migrationBundleDst = Join-Path $dbOutputDir "ParkGate.Migrations.exe"

# Always a fresh bundle: a leftover one may miss the latest migrations (R8.4).
Write-Host "`n2. Building self-contained EF Core migration bundle..." -ForegroundColor Yellow
$infraProject = Join-Path $repoRoot "src\ParkGate.Infrastructure\ParkGate.Infrastructure.csproj"
dotnet restore $infraProject --runtime win-x64
if ($LASTEXITCODE -ne 0) {
    throw "Restore for win-x64 failed with exit code $LASTEXITCODE"
}
dotnet ef migrations bundle --self-contained --runtime win-x64 --context ParkGateDbContext --configuration $Configuration --project $infraProject --startup-project $infraProject --output $migrationBundleDst --force
if ($LASTEXITCODE -ne 0) {
    throw "Migration bundle failed with exit code $LASTEXITCODE"
}

Write-Host "`n3. Publishing the Sync Agent..." -ForegroundColor Yellow
$agentProject = Join-Path $repoRoot "src\ParkGate.SyncAgent\ParkGate.SyncAgent.csproj"
dotnet publish $agentProject -c $Configuration -r win-x64 --self-contained true -p:RuntimeFrameworkVersion=$RuntimeVersion -o (Join-Path $repoRoot "dist\sync-agent") --nologo
if ($LASTEXITCODE -ne 0) {
    throw "Sync Agent publish failed with exit code $LASTEXITCODE"
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
    Write-Host "`n4. Compiling Inno Setup script with $iscc..." -ForegroundColor Yellow
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
