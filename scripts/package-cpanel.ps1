<#
.SYNOPSIS
    Packages the ParkGate Laravel web application into a cPanel deployment ZIP (R8.5: the only web deployment path).
.DESCRIPTION
    1. Runs the web tests (skip with -SkipTests only when they already ran on this commit).
    2. Regenerates database/mysql_schema.sql from the migrations (R2.9).
    3. Builds the frontend assets (npm run build).
    4. Copies the application into a clean staging folder: no .env, no tests, no node_modules, no local logs,
       sessions, caches or SQLite database.
    5. Installs the production Composer packages in the staging folder (no dev packages).
    6. Checks the staging folder and zips it.
    See docs/DEPLOYMENT_WEB_CPANEL.md for the steps on the server.
#>

[CmdletBinding()]
param(
    [string]$OutputDir = "$PSScriptRoot\..\artifacts",
    [string]$ZipFileName = "parkgate-web-cpanel.zip",
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Invoke-Checked {
    param([string]$Description, [scriptblock]$Command)

    # Native tools write notices to stderr; in Windows PowerShell 5.1 that becomes a terminating error under
    # "Stop" when stderr is redirected. The exit code is what decides.
    $ErrorActionPreference = "Continue"
    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE."
    }
}

$WebDir = (Resolve-Path "$PSScriptRoot\..\web").Path
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}
$TargetZipPath = [System.IO.Path]::GetFullPath((Join-Path $OutputDir $ZipFileName))

Write-Host "ParkGate web: cPanel package" -ForegroundColor Cyan

Push-Location $WebDir
try {
    if ($SkipTests) {
        Write-Host "`n[1/6] Web tests skipped (-SkipTests)." -ForegroundColor DarkYellow
    }
    else {
        Write-Host "`n[1/6] Running the web tests..." -ForegroundColor Yellow
        Invoke-Checked "php artisan test" { php artisan test }
    }

    # The file is never edited by hand; a web test fails if the committed file and the migrations disagree.
    Write-Host "`n[2/6] Generating mysql_schema.sql from the migrations..." -ForegroundColor Yellow
    Invoke-Checked "Schema generation" { php artisan parkgate:generate-mysql-schema }

    Write-Host "`n[3/6] Building frontend assets..." -ForegroundColor Yellow
    Invoke-Checked "npm run build" { npm run build }
}
finally {
    Pop-Location
}

Write-Host "`n[4/6] Staging files..." -ForegroundColor Yellow
$StagingDir = Join-Path ([System.IO.Path]::GetTempPath()) "parkgate_cpanel_staging_$(Get-Random)"
New-Item -ItemType Directory -Path $StagingDir | Out-Null

try {
    $IncludeItems = @(
        "app", "bootstrap", "config", "database", "public", "resources", "routes",
        ".htaccess", ".env.production.example", "artisan", "composer.json", "composer.lock"
    )
    foreach ($item in $IncludeItems) {
        $srcPath = Join-Path $WebDir $item
        if (-not (Test-Path $srcPath)) {
            throw "Missing web item: $item"
        }

        Copy-Item -Path $srcPath -Destination $StagingDir -Recurse -Force
    }

    # Local state that must never reach the server: the dev package cache (it lists dev-only providers),
    # the SQLite database, the Vite dev-server marker and the public/storage link.
    $remove = @(
        "bootstrap\cache\*.php",
        "database\*.sqlite",
        "public\hot",
        "public\storage"
    )
    foreach ($pattern in $remove) {
        Get-ChildItem -Path (Join-Path $StagingDir $pattern) -Force -ErrorAction SilentlyContinue |
            Remove-Item -Recurse -Force
    }

    # Empty storage folders; the server writes its own logs, sessions and caches.
    foreach ($dir in @(
            "storage\app\public",
            "storage\framework\cache\data",
            "storage\framework\sessions",
            "storage\framework\views",
            "storage\logs")) {
        New-Item -ItemType Directory -Path (Join-Path $StagingDir $dir) -Force | Out-Null
    }

    Write-Host "`n[5/6] Installing production Composer packages..." -ForegroundColor Yellow
    Push-Location $StagingDir
    try {
        Invoke-Checked "composer install" { composer install --no-dev --optimize-autoloader --no-interaction --no-progress --prefer-dist }
    }
    finally {
        Pop-Location
    }

    # Leave a clean package cache; Laravel rebuilds it on the server for the production packages only.
    Get-ChildItem -Path (Join-Path $StagingDir "bootstrap\cache\*.php") -Force -ErrorAction SilentlyContinue | Remove-Item -Force

    Write-Host "`n[6/6] Checking and zipping..." -ForegroundColor Yellow
    $forbidden = @(".env", "tests", "node_modules", "vendor\phpunit", "vendor\laravel\pint", "database\database.sqlite")
    foreach ($item in $forbidden) {
        if (Test-Path (Join-Path $StagingDir $item)) {
            throw "The package must not contain '$item'."
        }
    }
    foreach ($item in @("vendor\autoload.php", "public\build\manifest.json", "database\mysql_schema.sql", ".htaccess")) {
        if (-not (Test-Path (Join-Path $StagingDir $item))) {
            throw "The package is missing '$item'."
        }
    }

    if (Test-Path $TargetZipPath) {
        Remove-Item $TargetZipPath -Force
    }

    Push-Location $StagingDir
    try {
        Invoke-Checked "tar" { tar.exe -a -c -f $TargetZipPath . }
    }
    finally {
        Pop-Location
    }
}
finally {
    Remove-Item $StagingDir -Recurse -Force -ErrorAction SilentlyContinue
}

$ZipSizeMB = [math]::Round((Get-Item $TargetZipPath).Length / 1MB, 2)
Write-Host "`nPackage created: $TargetZipPath ($ZipSizeMB MB)" -ForegroundColor Green
Write-Host "MySQL schema:    $WebDir\database\mysql_schema.sql" -ForegroundColor Green
Write-Host "Next: docs\DEPLOYMENT_WEB_CPANEL.md" -ForegroundColor Green
