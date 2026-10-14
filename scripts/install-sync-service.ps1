<#
.SYNOPSIS
    Installs ParkGate.SyncAgent as a Windows Service.
.DESCRIPTION
    Configures and installs ParkGate.SyncAgent as an auto-starting Windows Service with automatic recovery.
.PARAMETER ServiceName
    The name of the Windows Service (default: "ParkGateSyncAgent").
.PARAMETER DisplayName
    The human-readable display name (default: "ParkGate Sync Agent").
.PARAMETER BinaryPath
    The path to ParkGate.SyncAgent.exe. If omitted, searches default publish and bin paths.
#>
param (
    [string]$ServiceName = "ParkGateSyncAgent",
    [string]$DisplayName = "ParkGate Sync Agent",
    [string]$BinaryPath = ""
)

$ErrorActionPreference = "Stop"

# 1. Require Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Error "This script must be run as Administrator."
    exit 1
}

# 2. Locate binary if not specified
if ([string]::IsNullOrWhiteSpace($BinaryPath)) {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $rootDir = Split-Path -Parent $scriptDir
    $candidates = @(
        Join-Path $rootDir "publish\sync-agent\ParkGate.SyncAgent.exe",
        Join-Path $rootDir "src\ParkGate.SyncAgent\bin\Release\net10.0\publish\ParkGate.SyncAgent.exe",
        Join-Path $rootDir "src\ParkGate.SyncAgent\bin\Release\net10.0\ParkGate.SyncAgent.exe",
        Join-Path $rootDir "src\ParkGate.SyncAgent\bin\Debug\net10.0\ParkGate.SyncAgent.exe"
    )

    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            $BinaryPath = (Resolve-Path $candidate).Path
            break
        }
    }
}

if (-not (Test-Path $BinaryPath)) {
    Write-Error "Could not find ParkGate.SyncAgent.exe. Please publish the project first or provide -BinaryPath."
    exit 1
}

Write-Host "Found binary: $BinaryPath" -ForegroundColor Cyan

# 3. Check if service already exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Service '$ServiceName' already exists. Stopping and removing..." -ForegroundColor Yellow
    if ($existingService.Status -eq 'Running') {
        Stop-Service -Name $ServiceName -Force
    }
    sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
}

# 4. Create Service
Write-Host "Creating Windows Service '$ServiceName'..." -ForegroundColor Green
$binPathArg = "`"$BinaryPath`""
sc.exe create $ServiceName binPath= $binPathArg DisplayName= "$DisplayName" start= auto | Out-Null

# 5. Set Description
sc.exe description $ServiceName "Background synchronization worker for ParkGate cloud dashboard." | Out-Null

# 6. Configure Service Failure / Recovery (Restart after 60s)
sc.exe failure $ServiceName reset= 86400 actions= restart/60000/restart/60000/restart/60000 | Out-Null

# 7. Start the Service
Write-Host "Starting service '$ServiceName'..." -ForegroundColor Green
Start-Service -Name $ServiceName

$status = (Get-Service -Name $ServiceName).Status
Write-Host "Service '$ServiceName' installed successfully and is currently: $status" -ForegroundColor Green
