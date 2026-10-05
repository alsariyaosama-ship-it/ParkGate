<#
.SYNOPSIS
    Builds the self-contained EF Core migration executable (efbundle.exe) for ParkGate deployment.

.DESCRIPTION
    Creates an executable bundle containing all pending and applied EF Core migrations.
    Applied on the SQL Server host during deployment per ADR-011 and DATABASE.md §7.

.PARAMETER OutputPath
    Destination file path for the generated migration bundle. Defaults to "<repo>\artifacts\efbundle.exe".

.PARAMETER TargetRuntime
    Target runtime RID. Defaults to "win-x64".

.PARAMETER Configuration
    Build configuration. Defaults to "Release".

.EXAMPLE
    .\scripts\build-migration-bundle.ps1
    .\scripts\build-migration-bundle.ps1 -OutputPath "C:\deploy\efbundle.exe"
#>

[CmdletBinding()]
param (
    [string]$OutputPath,
    [string]$TargetRuntime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$RepoRoot = (Resolve-Path "$PSScriptRoot\..").Path
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $RepoRoot "artifacts\efbundle.exe"
}

$InfrastructureProject = Join-Path $RepoRoot "src\ParkGate.Infrastructure"
$ResolvedOutput = [System.IO.Path]::GetFullPath($OutputPath)
$OutputDirectory = [System.IO.Path]::GetDirectoryName($ResolvedOutput)

if (-not (Test-Path $OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Building ParkGate EF Core Migration Bundle" -ForegroundColor Cyan
Write-Host " Project:   $InfrastructureProject"
Write-Host " Output:    $ResolvedOutput"
Write-Host " Runtime:   $TargetRuntime"
Write-Host " Config:    $Configuration"
Write-Host "==========================================================" -ForegroundColor Cyan

$bundleArgs = @(
    "ef", "migrations", "bundle",
    "--project", $InfrastructureProject,
    "--startup-project", $InfrastructureProject,
    "--output", $ResolvedOutput,
    "--target-runtime", $TargetRuntime,
    "--configuration", $Configuration,
    "--force"
)

Write-Host "Running: dotnet $($bundleArgs -join ' ')..." -ForegroundColor Yellow

& dotnet $bundleArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to generate EF Core migration bundle (exit code: $LASTEXITCODE)."
    exit $LASTEXITCODE
}

if (Test-Path $ResolvedOutput) {
    $bundleSize = (Get-Item $ResolvedOutput).Length / 1MB
    Write-Host "Migration bundle generated successfully: $ResolvedOutput ($('{0:N2}' -f $bundleSize) MB)" -ForegroundColor Green
    Write-Host "`nTo execute migrations on server:" -ForegroundColor Gray
    Write-Host "  $ResolvedOutput --connection ""Server=SQL_SERVER;Database=ParkGate;User Id=...;Password=...""" -ForegroundColor Gray
} else {
    Write-Error "Migration bundle was not found at $ResolvedOutput."
    exit 1
}
