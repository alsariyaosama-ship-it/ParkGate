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

.EXAMPLE
    powershell.exe -ExecutionPolicy Bypass -File .\scripts\backup.ps1 -ServerInstance "SERVER\SQLEXPRESS" -RetentionDays 30

.NOTES
    Task Scheduler Setup:
    Action: Start a program
    Program/script: powershell.exe
    Add arguments: -ExecutionPolicy Bypass -NoProfile -File "C:\Program Files\ParkGate\scripts\backup.ps1"
    Trigger: Daily at 02:00 AM
#>

[CmdletBinding()]
param (
    [string]$ServerInstance = "localhost\SQLEXPRESS",
    [string]$DatabaseName = "ParkGate",
    [string]$BackupDirectory = "C:\ParkGate_Backups",
    [int]$RetentionDays = 14
)

$ErrorActionPreference = "Stop"

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
    # 1. Ensure backup and log directory exist
    if (-not (Test-Path -Path $BackupDirectory)) {
        New-Item -ItemType Directory -Path $BackupDirectory -Force | Out-Null
    }

    $logDir = Join-Path $BackupDirectory "logs"
    if (-not (Test-Path -Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }

    $script:LogFile = Join-Path $logDir ("backup_run_" + (Get-Date -Format "yyyyMM") + ".log")
    Write-Log "=== Starting ParkGate Database Backup ==="

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
    COMPRESSION,
    CHECKSUM,
    STATS = 10;
"@

    # 4. Execute Backup via sqlcmd or .NET SqlClient
    Write-Log "Executing backup query with compression and checksum..."
    $connectionString = "Server=$ServerInstance;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Timeout=120;"
    
    $connection = New-Object Microsoft.Data.SqlClient.SqlConnection($connectionString)
    try {
        $connection.Open()
    }
    catch {
        # Fallback to legacy System.Data.SqlClient if Microsoft.Data.SqlClient is not globally registered
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
    }

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
