<#
.SYNOPSIS
    Uninstalls the ParkGate.SyncAgent Windows Service.
.PARAMETER ServiceName
    The name of the Windows Service to uninstall (default: "ParkGateSyncAgent").
#>
param (
    [string]$ServiceName = "ParkGateSyncAgent"
)

$ErrorActionPreference = "Stop"

# 1. Require Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Error "This script must be run as Administrator."
    exit 1
}

$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if (-not $service) {
    Write-Host "Service '$ServiceName' does not exist." -ForegroundColor Yellow
    exit 0
}

Write-Host "Stopping service '$ServiceName'..." -ForegroundColor Yellow
if ($service.Status -eq 'Running') {
    Stop-Service -Name $ServiceName -Force
}

Write-Host "Removing service '$ServiceName'..." -ForegroundColor Green
sc.exe delete $ServiceName | Out-Null

Write-Host "Service '$ServiceName' removed successfully." -ForegroundColor Green
