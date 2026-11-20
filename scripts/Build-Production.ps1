[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputRoot = "artifacts\production"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$winFormsProject = Join-Path $repositoryRoot "src\ParkGate.WinForms\ParkGate.WinForms.csproj"
$infrastructureProject = Join-Path $repositoryRoot "src\ParkGate.Infrastructure\ParkGate.Infrastructure.csproj"
$outputPath = Join-Path $repositoryRoot $OutputRoot
$clientPath = Join-Path $outputPath "client"
$databasePath = Join-Path $outputPath "database"
$migrationBundlePath = Join-Path $databasePath "ParkGate.Migrations.exe"
$migrationScriptPath = Join-Path $databasePath "ParkGate.Migrations.sql"

function Invoke-DotNet {
    param([string[]]$Arguments)

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed with exit code $LASTEXITCODE."
    }
}

Write-Host "Cleaning production output..."
if (Test-Path $outputPath) {
    Remove-Item $outputPath -Recurse -Force
}
New-Item $clientPath -ItemType Directory -Force | Out-Null
New-Item $databasePath -ItemType Directory -Force | Out-Null

Write-Host "Publishing ParkGate WinForms ($Runtime, self-contained)..."
Invoke-DotNet @(
    "publish",
    $winFormsProject,
    "--configuration", $Configuration,
    "--runtime", $Runtime,
    "--self-contained", "true",
    "--output", $clientPath,
    "--property:PublishReadyToRun=true",
    "--property:DebugType=None",
    "--property:DebugSymbols=false"
)

Write-Host "Configuring production client appsettings.json for SQLEXPRESS..."
# PowerShell has no "\\" escape: one backslash here is one backslash in the JSON value (R8.1).
$productionServer = ".\SQLEXPRESS"
$clientAppSettings = Join-Path $clientPath "appsettings.json"
if (Test-Path $clientAppSettings) {
    $json = Get-Content $clientAppSettings -Raw | ConvertFrom-Json
    $json.ConnectionStrings.DefaultConnection = "Server=$productionServer;Database=ParkGate;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
    $json | ConvertTo-Json -Depth 10 | Set-Content $clientAppSettings -Encoding UTF8

    # Read the file back and check the server name exactly as the client will see it.
    $written = (Get-Content $clientAppSettings -Raw | ConvertFrom-Json).ConnectionStrings.DefaultConnection
    $writtenServer = (($written -split ';') | Where-Object { $_ -like 'Server=*' } | Select-Object -First 1).Substring(7)
    if ($writtenServer -ne $productionServer) {
        throw "appsettings.json has server '$writtenServer', expected '$productionServer'."
    }
}

Write-Host "Generating idempotent EF Core SQL script..."
Invoke-DotNet @(
    "ef", "migrations", "script",
    "--idempotent",
    "--no-build",
    "--context", "ParkGateDbContext",
    "--configuration", $Configuration,
    "--project", $infrastructureProject,
    "--startup-project", $infrastructureProject,
    "--output", $migrationScriptPath
)

Write-Host "Building the infrastructure project for the migration bundle..."
Invoke-DotNet @(
    "restore",
    $infrastructureProject,
    "--runtime", $Runtime
)
Invoke-DotNet @(
    "build",
    $infrastructureProject,
    "--configuration", $Configuration,
    "--runtime", $Runtime,
    "--no-restore"
)

Write-Host "Generating self-contained EF Core migration bundle..."
Invoke-DotNet @(
    "ef", "migrations", "bundle",
    "--self-contained",
    "--no-build",
    "--runtime", $Runtime,
    "--context", "ParkGateDbContext",
    "--configuration", $Configuration,
    "--project", $infrastructureProject,
    "--startup-project", $infrastructureProject,
    "--output", $migrationBundlePath
)

Write-Host "Production artifacts created under $outputPath"
Write-Host "Client package: $clientPath"
Write-Host "SQL migration:  $migrationScriptPath"
Write-Host "Migration exe:  $migrationBundlePath"
