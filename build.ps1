param(
    [ValidateSet("unified", "all")]
    [string]$Target = "unified"
)

$ErrorActionPreference = "Stop"
$IAN_HOME = Split-Path -Parent $PSCommandPath
$CSC = "$env:SystemRoot\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$REFERENCES = @(
    "System.dll",
    "System.Core.dll",
    "System.Net.dll",
    "System.Xml.dll"
)

function Build-Unified {
    Write-Host "=== Building unified ian.exe ===" -ForegroundColor Cyan

    $csFiles = @(
        "src\IanLang\Program.cs",
        "ian_engine.cs",
        "ian_shell.cs",
        "ian_brain.cs",
        "ian_agent.cs",
        "ian_api.cs",
        "ian_architect.cs",
        "ian_human.cs",
        "ian_teacher.cs",
        "ian_supervisor.cs",
        "ian_daily.cs",
        "ian_auto.cs"
    ) | ForEach-Object { Join-Path $IAN_HOME $_ }
    $refArgs = $REFERENCES | ForEach-Object { "-reference:" + $_ }
    $outExe = Join-Path $IAN_HOME "ian.exe"

    $argList = @(
        "-nologo",
        "-target:exe",
        "-out:$outExe"
    ) + $refArgs + $csFiles

    Write-Host "Compiling $($csFiles.Count) source files..." -ForegroundColor Yellow
    & $CSC $argList 2>&1

    if ($LASTEXITCODE -eq 0) {
        $size = (Get-Item $outExe).Length
        Write-Host "SUCCESS: ian.exe ($($size / 1024 -as [int]) KB)" -ForegroundColor Green
    } else {
        Write-Host "FAILED with exit code $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
}

function Build-All {
    Write-Host "=== Building all standalone EXEs ===" -ForegroundColor Cyan
    Write-Host "Use build.ps1 for unified build instead" -ForegroundColor Yellow
    Build-Unified
}

switch ($Target) {
    "unified" { Build-Unified }
    "all"     { Build-All }
}
