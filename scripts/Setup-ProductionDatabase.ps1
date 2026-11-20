<#
.SYNOPSIS
    Creates or upgrades the ParkGate database on the server and sets up the least-privilege SQL login (R6.5).
.DESCRIPTION
    Run on the SQL Server machine as Administrator. Safe to run again (every upgrade runs it).
    1. Applies the EF Core migrations with the administrator's Windows account (the migration account).
    2. Checks that SQL Server accepts SQL logins (Mixed Mode, DEPLOYMENT.md 2.1).
    3. Creates the SQL login the desks use (default parkgate_app): read, write and backup only, no schema rights.
       A new login gets a generated password, shown once. An existing login keeps its password unless
       -ResetAppPassword is given.
    4. Saves that login in %ProgramData%\ParkGate\connection.dat (DPAPI, this machine) when the password is known.
    5. Opens TCP 1433 and UDP 1434 for private and domain networks only, and warns about a Public network.
.EXAMPLE
    .\Setup-ProductionDatabase.ps1 -MigrationBundlePath "C:\Program Files\ParkGate\database\ParkGate.Migrations.exe"
.EXAMPLE
    .\Setup-ProductionDatabase.ps1 -ResetAppPassword     # new password for parkgate_app; then update every desk
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [string]$SqlServer = ".\SQLEXPRESS",
    [string]$Database = "ParkGate",
    [string]$MigrationBundlePath = "",
    [string]$AppLogin = "parkgate_app",
    [SecureString]$AppPassword,
    [switch]$ResetAppPassword,
    [string]$ConnectionFile = (Join-Path $env:ProgramData "ParkGate\connection.dat"),
    [switch]$SkipFirewall,
    [switch]$NoPause
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# The installer runs this in a visible window that closes when the script ends: keep an error readable.
trap {
    Write-Host "ERROR: $_" -ForegroundColor Red
    if (-not $NoPause) {
        [void](Read-Host "Press Enter to close")
    }

    exit 1
}

if ($AppLogin -notmatch '^[A-Za-z_][A-Za-z0-9_]{0,63}$') {
    throw "AppLogin may contain only letters, digits and underscores."
}

if ($Database -notmatch '^[A-Za-z_][A-Za-z0-9_]{0,63}$') {
    throw "Database may contain only letters, digits and underscores."
}

$repositoryRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($MigrationBundlePath)) {
    $MigrationBundlePath = Join-Path $repositoryRoot "artifacts\production\database\ParkGate.Migrations.exe"
}

if (-not (Test-Path -LiteralPath $MigrationBundlePath -PathType Leaf)) {
    throw "Migration bundle was not found at '$MigrationBundlePath'. Run scripts\Build-Production.ps1 first."
}

$adminConnection = "Server=$SqlServer;Database=master;Integrated Security=True;TrustServerCertificate=True"
$migrationConnection = "Server=$SqlServer;Database=$Database;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"

function Invoke-Sql {
    param([string]$Sql, [switch]$Scalar)

    $connection = New-Object System.Data.SqlClient.SqlConnection $adminConnection
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $Sql
        if ($Scalar) { return $command.ExecuteScalar() }
        [void]$command.ExecuteNonQuery()
    }
    finally {
        $connection.Dispose()
    }
}

function ConvertTo-PlainText([SecureString]$Secure) {
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Secure)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer) }
}

function New-AppPassword {
    # Letters and digits only, so it is easy to type on a desk and never breaks a connection string.
    # No look-alikes (0/O, 1/l/I). At least one upper, one lower and one digit for the Windows password policy.
    $upper = 'ABCDEFGHJKLMNPQRSTUVWXYZ'
    $lower = 'abcdefghijkmnopqrstuvwxyz'
    $digits = '23456789'
    $all = $upper + $lower + $digits
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        do {
            $bytes = New-Object byte[] 20
            $rng.GetBytes($bytes)
            $password = -join ($bytes | ForEach-Object { $all[$_ % $all.Length] })
        } until ($password -cmatch '[A-Z]' -and $password -cmatch '[a-z]' -and $password -match '[0-9]')
        return $password
    }
    finally {
        $rng.Dispose()
    }
}

function Save-ConnectionFile([string]$ConnectionString) {
    $directory = Split-Path -Parent $ConnectionFile
    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    Add-Type -AssemblyName System.Security
    $plainBytes = [System.Text.Encoding]::UTF8.GetBytes($ConnectionString)
    $entropy = [System.Text.Encoding]::UTF8.GetBytes("ParkGate.ConnectionProtection")
    $protected = [System.Security.Cryptography.ProtectedData]::Protect($plainBytes, $entropy, [System.Security.Cryptography.DataProtectionScope]::LocalMachine)
    [System.IO.File]::WriteAllBytes($ConnectionFile, $protected)
}

Write-Host "Target: $SqlServer / $Database"

# ── 1. Migrations, with the administrator's Windows account ──────────────────
if ($PSCmdlet.ShouldProcess("$SqlServer\$Database", "Apply ParkGate EF Core migrations")) {
    & $MigrationBundlePath --connection $migrationConnection
    if ($LASTEXITCODE -ne 0) {
        throw "Migration bundle failed with exit code $LASTEXITCODE."
    }

    Write-Host "Database created or updated." -ForegroundColor Green
}

# ── 2. SQL logins must be allowed (Mixed Mode) ───────────────────────────────
$windowsOnly = [int](Invoke-Sql -Scalar "SELECT CAST(SERVERPROPERTY('IsIntegratedSecurityOnly') AS int)")
if ($windowsOnly -eq 1) {
    throw "SQL Server accepts Windows logins only. Switch it to Mixed Mode (SSMS: Server Properties > Security > 'SQL Server and Windows Authentication mode'), restart the SQL Server service, then run this script again. See DEPLOYMENT.md 2.1."
}

# ── 3. The desks' login: read, write and backup, nothing else ────────────────
$loginExists = [int](Invoke-Sql -Scalar "SELECT COUNT(*) FROM sys.server_principals WHERE name = N'$AppLogin'") -gt 0
$plainPassword = $null
if ($AppPassword) {
    $plainPassword = ConvertTo-PlainText $AppPassword
}
elseif (-not $loginExists -or $ResetAppPassword) {
    $plainPassword = New-AppPassword
}

if ($PSCmdlet.ShouldProcess($AppLogin, "Create or update SQL login")) {
    if ($plainPassword) {
        $escaped = $plainPassword.Replace("'", "''")
        if ($loginExists) {
            Invoke-Sql "ALTER LOGIN [$AppLogin] WITH PASSWORD = N'$escaped'; ALTER LOGIN [$AppLogin] ENABLE;"
            Write-Host "Login ${AppLogin}: new password set." -ForegroundColor Green
        }
        else {
            # CHECK_EXPIRATION OFF: an expired password would stop every desk at once.
            Invoke-Sql "CREATE LOGIN [$AppLogin] WITH PASSWORD = N'$escaped', DEFAULT_DATABASE = [$Database], CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;"
            Write-Host "Login $AppLogin created." -ForegroundColor Green
        }
    }
    else {
        Write-Host "Login $AppLogin exists; password unchanged."
    }

    Invoke-Sql @"
USE [$Database];
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$AppLogin')
    CREATE USER [$AppLogin] FOR LOGIN [$AppLogin];
ALTER ROLE db_datareader ADD MEMBER [$AppLogin];
ALTER ROLE db_datawriter ADD MEMBER [$AppLogin];
ALTER ROLE db_backupoperator ADD MEMBER [$AppLogin];
IF IS_ROLEMEMBER('db_owner', N'$AppLogin') = 1 ALTER ROLE db_owner DROP MEMBER [$AppLogin];
IF IS_ROLEMEMBER('db_ddladmin', N'$AppLogin') = 1 ALTER ROLE db_ddladmin DROP MEMBER [$AppLogin];
"@
    Write-Host "User ${AppLogin} in ${Database}: db_datareader, db_datawriter, db_backupoperator." -ForegroundColor Green
}

# ── 4. This machine's connection (the server's own desk and the Sync Agent) ──
if ($plainPassword) {
    if ($PSCmdlet.ShouldProcess($ConnectionFile, "Save the $AppLogin connection (DPAPI)")) {
        Save-ConnectionFile "Server=$SqlServer;Database=$Database;User Id=$AppLogin;Password=$plainPassword;MultipleActiveResultSets=True;TrustServerCertificate=True"
        Write-Host "Connection saved to $ConnectionFile" -ForegroundColor Green
    }
}
elseif (-not (Test-Path $ConnectionFile)) {
    Write-Warning "No connection is saved on this machine and the $AppLogin password is not known here. Run again with -ResetAppPassword, or enter the connection in the ParkGate connection screen."
}

# ── 5. Firewall: private and domain networks only ───────────────────────────
if (-not $SkipFirewall -and $PSCmdlet.ShouldProcess("Windows Firewall", "Allow SQL Server on private and domain networks")) {
    try {
        # Remove the 1.0.0 rules, which were open on every network profile.
        netsh advfirewall firewall delete rule name="ParkGate SQL Server (Port 1433)" | Out-Null
        netsh advfirewall firewall delete rule name="ParkGate SQL Browser (Port 1434)" | Out-Null
        foreach ($rule in @(
                @{ Name = "ParkGate SQL Server (TCP 1433)"; Protocol = "TCP"; Port = 1433 },
                @{ Name = "ParkGate SQL Browser (UDP 1434)"; Protocol = "UDP"; Port = 1434 })) {
            Get-NetFirewallRule -DisplayName $rule.Name -ErrorAction SilentlyContinue | Remove-NetFirewallRule
            New-NetFirewallRule -DisplayName $rule.Name -Direction Inbound -Protocol $rule.Protocol -LocalPort $rule.Port -Action Allow -Profile Private, Domain | Out-Null
        }

        Write-Host "Firewall: TCP 1433 and UDP 1434 open on private and domain networks." -ForegroundColor Green
    }
    catch {
        Write-Warning "Could not configure Windows Firewall automatically: $_"
    }
}

$public = @(Get-NetConnectionProfile -ErrorAction SilentlyContinue | Where-Object { $_.NetworkCategory -eq 'Public' })
if ($public.Count -gt 0) {
    Write-Warning ("This network is set to Public: {0}. The desks cannot reach SQL Server until it is Private: Settings > Network > Properties > Private network, or Set-NetConnectionProfile -InterfaceAlias '{1}' -NetworkCategory Private" -f (($public | ForEach-Object { $_.Name }) -join ', '), $public[0].InterfaceAlias)
}

if ($SqlServer -match '^(\.|localhost|\(local\))\\' -and -not (Get-NetTCPConnection -LocalPort 1433 -State Listen -ErrorAction SilentlyContinue)) {
    Write-Warning "Nothing listens on TCP 1433. Enable TCP/IP on port 1433 in SQL Server Configuration Manager (DEPLOYMENT.md 2.2) so the desks can connect."
}

# ── Summary ──────────────────────────────────────────────────────────────────
if ($plainPassword -and -not $AppPassword) {
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Yellow
    Write-Host " Desk connection (ParkGate connection screen, SQL Server login):" -ForegroundColor Yellow
    Write-Host "   Server:   $env:COMPUTERNAME\$(($SqlServer -split '\\')[-1])"
    Write-Host "   Database: $Database"
    Write-Host "   User:     $AppLogin"
    Write-Host "   Password: $plainPassword"
    Write-Host " Write it down now. It is not shown again or saved in plain text." -ForegroundColor Yellow
    Write-Host "================================================================" -ForegroundColor Yellow
    if (-not $NoPause) {
        [void](Read-Host "Press Enter when you have written it down")
    }
}
