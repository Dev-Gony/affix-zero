#requires -Version 5.1
[CmdletBinding()]
param(
    [string]$GodotExe = ''
)
$ErrorActionPreference = 'Stop'

function Find-Godot472 {
    $candidates = New-Object System.Collections.Generic.List[string]

    if ($env:GODOT_EXE) { $candidates.Add($env:GODOT_EXE) }

    $names = @(
        'Godot_v4.7.2-stable_win64.exe',
        'Godot_v4.7.2-stable_win64_console.exe'
    )
    $roots = @(
        [Environment]::GetFolderPath('Desktop'),
        [Environment]::GetFolderPath('UserProfile') + '\Downloads'
    ) | Where-Object { $_ -and (Test-Path -LiteralPath $_ -PathType Container) }

    foreach ($root in $roots) {
        foreach ($name in $names) {
            Get-ChildItem -LiteralPath $root -Filter $name -File -Recurse -ErrorAction SilentlyContinue |
                ForEach-Object { $candidates.Add($_.FullName) }
        }
    }

    $pathCommand = Get-Command 'godot.exe' -ErrorAction SilentlyContinue
    if ($pathCommand) { $candidates.Add($pathCommand.Source) }

    return @($candidates | Where-Object { $_ -and (Test-Path -LiteralPath $_ -PathType Leaf) } | Select-Object -Unique)
}

if ([string]::IsNullOrWhiteSpace($GodotExe) -or -not (Test-Path -LiteralPath $GodotExe -PathType Leaf)) {
    $found = @(Find-Godot472)
    if ($found.Count -eq 1) {
        $GodotExe = $found[0]
        Write-Host "Auto-detected Godot: $GodotExe"
    } elseif ($found.Count -gt 1) {
        Write-Host "Multiple Godot 4.7.2 executables found:"
        $found | ForEach-Object { Write-Host "  $_" }
        throw "Pass -GodotExe with one of the paths above."
    } else {
        throw "Godot 4.7.2 executable not found. Run: Get-ChildItem -Path $HOME\Downloads,$HOME\Desktop -Filter 'Godot_v4.7.2-stable_win64*.exe' -File -Recurse"
    }
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
