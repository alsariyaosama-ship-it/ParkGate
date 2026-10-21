[CmdletBinding(SupportsShouldProcess)]
param(
    [string]$SqlServer = ".\SQLEXPRESS",
    [string]$Database = "ParkGate",
    [string]$MigrationBundlePath = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($MigrationBundlePath)) {
    $MigrationBundlePath = Join-Path $repositoryRoot "artifacts\production\database\ParkGate.Migrations.exe"
}

if (-not (Test-Path -LiteralPath $MigrationBundlePath -PathType Leaf)) {
    throw "Migration bundle was not found at '$MigrationBundlePath'. Run scripts\Build-Production.ps1 first."
}

$connectionString = "Server=$SqlServer;Database=$Database;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
Write-Host "Target database: $SqlServer / $Database"
Write-Host "Authentication: Windows"

if ($PSCmdlet.ShouldProcess("$SqlServer\$Database", "Apply ParkGate EF Core migrations")) {
    & $MigrationBundlePath --connection $connectionString
    if ($LASTEXITCODE -ne 0) {
        throw "Migration bundle failed with exit code $LASTEXITCODE."
    }

    # Save encrypted connection settings for client and sync agent (DPAPI LocalMachine)
    $programDataDir = Join-Path $env:ProgramData "ParkGate"
    if (-not (Test-Path $programDataDir)) {
        New-Item -ItemType Directory -Path $programDataDir -Force | Out-Null
    }
    $connectionDat = Join-Path $programDataDir "connection.dat"
    Add-Type -AssemblyName System.Security
    $plainBytes = [System.Text.Encoding]::UTF8.GetBytes($connectionString)
    $entropy = [System.Text.Encoding]::UTF8.GetBytes("ParkGate.ConnectionProtection")
    $encrypted = [System.Security.Cryptography.ProtectedData]::Protect($plainBytes, $entropy, [System.Security.Cryptography.DataProtectionScope]::LocalMachine)
    [System.IO.File]::WriteAllBytes($connectionDat, $encrypted)

    # Allow incoming SQL Server traffic in Windows Firewall for LAN clients
    try {
        netsh advfirewall firewall add rule name="ParkGate SQL Server (Port 1433)" dir=in action=allow protocol=TCP localport=1433 profile=any | Out-Null
        netsh advfirewall firewall add rule name="ParkGate SQL Browser (Port 1434)" dir=in action=allow protocol=UDP localport=1434 profile=any | Out-Null
        Write-Host "Windows Firewall rules configured for LAN clients." -ForegroundColor Green
    } catch {
        Write-Warning "Could not configure Windows Firewall automatically: $_"
    }

    Write-Host "Database created or updated successfully."
    Write-Host "Connection string securely saved to: $connectionDat" -ForegroundColor Green
}
