<#
.SYNOPSIS
    ParkGate Automated Database Backup Script (SQL Server Express).
    Suitable for execution via Windows Task Scheduler.

.DESCRIPTION
    Performs full backup with compression and checksum for the ParkGate database.
    Prunes old backup files older than the retention period.
    Logs execution results with timestamps.

.PARAMETER ServerInstance
    SQL Server instance name (default: "localhost\SQLEXPRESS" or "(local)").

.PARAMETER DatabaseName
    Target database name (default: "ParkGate").

.PARAMETER BackupDirectory
    Destination folder on the server machine (default: "C:\ParkGate_Backups").

.PARAMETER RetentionDays
    Number of days to keep backup files before auto-pruning (default: 14 days).

.PARAMETER SqlServiceAccount
    Windows account of the SQL Server service, which writes the .bak file. Default: derived from the
    instance name ("NT Service\MSSQL`$SQLEXPRESS" for "localhost\SQLEXPRESS", "NT Service\MSSQLSERVER"
    for a default instance).

.NOTES
    Backups hold phone numbers and national IDs in clear (SQL Server Express has no backup encryption,
    R6.9). Every run restricts the backup folder to Administrators, SYSTEM and the SQL Server service
    account. Copy backups off-site only inside an encrypted container (docs/DEPLOYMENT.md section 9).

.EXAMPLE
    powershell.exe -ExecutionPolicy Bypass -File .\scripts\backup.ps1 -ServerInstance "SERVER\SQLEXPRESS" -RetentionDays 30

.NOTES
    Task Scheduler Setup:
    Action: Start a program
    Program/script: powershell.exe
    Add arguments: -ExecutionPolicy Bypass -NoProfile -File "C:\Program Files\ParkGate\scripts\backup.ps1"
    Trigger: Daily at 02:00 AM
    Run as: SYSTEM (or an administrator, "Run with highest privileges")
#>

[CmdletBinding()]
param (
    [string]$ServerInstance = "localhost\SQLEXPRESS",
    [string]$DatabaseName = "ParkGate",
    [string]$BackupDirectory = "C:\ParkGate_Backups",
    [int]$RetentionDays = 14,
    [string]$SqlServiceAccount = ""
)

$ErrorActionPreference = "Stop"

function Get-SqlServiceAccount {
    param ([string]$Instance)
    $parts = $Instance.Split('\')
    if ($parts.Length -gt 1 -and $parts[1] -ne "MSSQLSERVER") {
        return "NT Service\MSSQL`$" + $parts[1].ToUpperInvariant()
    }
    return "NT Service\MSSQLSERVER"
}

function Set-BackupFolderAcl {
    param ([string]$Path, [string]$ServiceAccount)
    # Remove inherited access (Users, Authenticated Users) and grant only what the backup needs.
    # *S-1-5-32-544 = Administrators, *S-1-5-18 = SYSTEM; SIDs work on any Windows display language.
    & icacls.exe $Path /inheritance:r /grant:r "*S-1-5-32-544:(OI)(CI)F" "*S-1-5-18:(OI)(CI)F" "${ServiceAccount}:(OI)(CI)M" /T /Q | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Could not restrict access to $Path (icacls exit code $LASTEXITCODE). Run as Administrator."
    }
}

function Write-Log {
    param ([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $formatted = "[$timestamp] [$Level] $Message"
    Write-Host $formatted
    if (-not [string]::IsNullOrWhiteSpace($script:LogFile)) {
        Add-Content -Path $script:LogFile -Value $formatted -Encoding UTF8
    }
}

try {
    # 0. The folder is restricted to administrators, so only an elevated run or SYSTEM can use it (R6.9).
    $principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw "Run backup.ps1 as Administrator, or schedule it as SYSTEM: the backup folder is restricted to administrators."
    }

    # 1. Ensure backup and log directory exist
    if (-not (Test-Path -Path $BackupDirectory)) {
        New-Item -ItemType Directory -Path $BackupDirectory -Force | Out-Null
    }

    $logDir = Join-Path $BackupDirectory "logs"
    if (-not (Test-Path -Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }

    if ([string]::IsNullOrWhiteSpace($SqlServiceAccount)) {
        $SqlServiceAccount = Get-SqlServiceAccount -Instance $ServerInstance
    }
    Set-BackupFolderAcl -Path $BackupDirectory -ServiceAccount $SqlServiceAccount

    $script:LogFile = Join-Path $logDir ("backup_run_" + (Get-Date -Format "yyyyMM") + ".log")
    Write-Log "=== Starting ParkGate Database Backup ==="
    Write-Log "Backup folder restricted to Administrators, SYSTEM and $SqlServiceAccount"

    # 2. Build backup file name and path
    $timestampStr = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFileName = "${DatabaseName}_Full_${timestampStr}.bak"
    $backupFilePath = Join-Path $BackupDirectory $backupFileName

    Write-Log "Target Database: $DatabaseName"
    Write-Log "Server Instance: $ServerInstance"
    Write-Log "Backup File: $backupFilePath"

    # 3. Construct T-SQL Backup Statement
    $sqlStatement = @"
BACKUP DATABASE [$DatabaseName]
TO DISK = N'$backupFilePath'
WITH 
    FORMAT,
    INIT,
    NAME = N'ParkGate Automated Full Backup $timestampStr',
    SKIP,
    NOREWIND,
    NOUNLOAD,
    CHECKSUM,
    STATS = 10;
"@

    # 4. Execute Backup via sqlcmd or .NET SqlClient
    # Express has no backup compression; CHECKSUM still verifies the pages it writes.
    Write-Log "Executing backup query with checksum..."
    $connectionString = "Server=$ServerInstance;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Timeout=120;"

    try {
        $connection = New-Object Microsoft.Data.SqlClient.SqlConnection($connectionString)
    }
    catch {
        # Windows PowerShell does not load Microsoft.Data.SqlClient; the built-in client is enough for a script.
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    }
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = $sqlStatement
    $command.CommandTimeout = 600
    $command.ExecuteNonQuery() | Out-Null
    $connection.Close()

    # Verify file was generated and check size
    if (Test-Path -Path $backupFilePath) {
        $fileInfo = Get-Item -Path $backupFilePath
        $sizeMb = [Math]::Round($fileInfo.Length / 1MB, 2)
        Write-Log "Backup successfully created. Size: $sizeMb MB" "SUCCESS"
    } else {
        throw "Backup file was not created at expected path: $backupFilePath"
    }

    # 5. Prune backups older than retention window
    Write-Log "Checking for old backups to prune (Retention: $RetentionDays days)..."
    $cutoffDate = (Get-Date).AddDays(-$RetentionDays)
    $oldBackups = Get-ChildItem -Path $BackupDirectory -Filter "${DatabaseName}_*.bak" | Where-Object { $_.LastWriteTime -lt $cutoffDate }

    foreach ($oldFile in $oldBackups) {
        Write-Log "Pruning expired backup file: $($oldFile.FullName) (Date: $($oldFile.LastWriteTime))"
        Remove-Item -Path $oldFile.FullName -Force
    }

    Write-Log "=== Backup Process Completed Successfully ==="
    exit 0
}
catch {
    Write-Log "CRITICAL ERROR: $($_.Exception.Message)" "ERROR"
    if ($_.ScriptStackTrace) {
        Write-Log "Stack trace: $($_.ScriptStackTrace)" "ERROR"
    }
    exit 1
}
