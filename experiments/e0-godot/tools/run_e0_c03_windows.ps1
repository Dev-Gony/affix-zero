#requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)][string]$GodotExe
)
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $GodotExe -PathType Leaf)) {
    throw "GodotExe not found: $GodotExe"
}
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Write-Host "AFFIX ZERO E0-C03"
Write-Host "Project: $projectRoot"
Write-Host "Godot: $GodotExe"
Write-Host "Runs: 3 x (10s warmup + 60s measurement)"
Write-Host "Visible Windows benchmark. CI/headless numbers are not GTX 1050 evidence."
& $GodotExe --path $projectRoot res://perf/e0_c03_perf.tscn
if ($LASTEXITCODE -ne 0) {
    throw "Godot exited with code $LASTEXITCODE"
}
