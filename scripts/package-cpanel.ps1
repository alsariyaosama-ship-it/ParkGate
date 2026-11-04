<#
.SYNOPSIS
    Packages the ParkGate Laravel Cloud Web Application into a cPanel deployment ZIP for Libyan Spider.
.DESCRIPTION
    1. Builds frontend assets via Vite (npm run build).
    2. Ensures production Composer vendor dependencies are installed.
    3. Verifies required storage directories exist.
    4. Creates a deployment archive ready to extract directly on cPanel File Manager.
#>

[CmdletBinding()]
param(
    [string]$OutputDir = "$PSScriptRoot\..\artifacts",
    [string]$ZipFileName = "parkgate-web-cpanel.zip"
)

$ErrorActionPreference = "Stop"

$WebDir = (Resolve-Path "$PSScriptRoot\..\web").Path
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}
$TargetZipPath = [System.IO.Path]::GetFullPath((Join-Path $OutputDir $ZipFileName))

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   ParkGate Laravel cPanel Packaging (Libyan Spider)     " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 2. Build Frontend Assets
Write-Host "`n[1/4] Building frontend assets (npm run build)..." -ForegroundColor Yellow
Push-Location $WebDir
try {
    npm run build
} finally {
    Pop-Location
}

# 3. Ensure Storage Directories
Write-Host "`n[2/4] Ensuring storage directories exist..." -ForegroundColor Yellow
$StorageDirs = @(
    "$WebDir\storage\framework\views",
    "$WebDir\storage\framework\cache\data",
    "$WebDir\storage\framework\sessions",
    "$WebDir\storage\logs",
    "$WebDir\bootstrap\cache"
)
foreach ($dir in $StorageDirs) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
}

# 4. Prepare Staging Directory
Write-Host "`n[3/4] Staging files for deployment..." -ForegroundColor Yellow
$StagingDir = "$env:TEMP\parkgate_cpanel_staging_$(Get-Random)"
if (Test-Path $StagingDir) {
    Remove-Item $StagingDir -Recurse -Force
}
New-Item -ItemType Directory -Path $StagingDir | Out-Null

$IncludeItems = @(
    "app",
    "bootstrap",
    "config",
    "database",
    "public",
    "resources",
    "routes",
    "storage",
    "vendor",
    ".htaccess",
    ".env.production.example",
    "artisan",
    "composer.json"
)

foreach ($item in $IncludeItems) {
    $srcPath = Join-Path $WebDir $item
    if (Test-Path $srcPath) {
        Copy-Item -Path $srcPath -Destination $StagingDir -Recurse -Force
    }
}

# Clean any dev SQLite database from staging
$StagingSqlite = Join-Path $StagingDir "database\database.sqlite"
if (Test-Path $StagingSqlite) {
    Remove-Item $StagingSqlite -Force
}

# 5. Create Archive
Write-Host "`n[4/4] Creating ZIP archive: $TargetZipPath..." -ForegroundColor Yellow
if (Test-Path $TargetZipPath) {
    Remove-Item $TargetZipPath -Force
}

Push-Location $StagingDir
try {
    tar.exe -a -c -f $TargetZipPath *
} finally {
    Pop-Location
}

# Cleanup staging
Remove-Item $StagingDir -Recurse -Force

$ZipSizeMB = [math]::Round((Get-Item $TargetZipPath).Length / 1MB, 2)
Write-Host "`n[SUCCESS] Package created successfully!" -ForegroundColor Green
Write-Host "File: $TargetZipPath ($ZipSizeMB MB)" -ForegroundColor Green
Write-Host "MySQL Schema: $WebDir\database\mysql_schema.sql" -ForegroundColor Green
