#requires -Version 5.1
<#
R0 only: read local state and optionally copy explicitly selected save files.
Never launches the game, migrates a save, changes a branch, or restores a stash.
Dot-source this file to load functions without running the entry point.
#>
[CmdletBinding()]
param(
    [string]$RepositoryPath = (Join-Path $PSScriptRoot '../..'),
    [ValidateSet('Inspect', 'Backup')][string]$Mode = 'Inspect',
    [string]$SaveDirectory = '',
    [string]$OutputRoot = '',
    [string]$GodotExe = '',
    [switch]$GameClosed,
    [switch]$ChooseSaveDirectory
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-AffixFullPath([string]$Path) {
    if ([string]::IsNullOrWhiteSpace($Path)) { throw 'A path is required.' }
    $full = [IO.Path]::GetFullPath($Path)
    if ($full -eq [IO.Path]::GetPathRoot($full)) { return $full }
    return $full.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
}

function Test-AffixWithin([string]$Child, [string]$Parent) {
    $c = Get-AffixFullPath $Child
    $p = Get-AffixFullPath $Parent
    return $c.Equals($p, [StringComparison]::OrdinalIgnoreCase) -or $c.StartsWith($p.TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)
}

function Assert-AffixNoLink([string]$Path) {
    $cursor = [IO.Path]::GetFullPath($Path)
    while (-not [string]::IsNullOrEmpty($cursor)) {
        if (Test-Path -LiteralPath $cursor) {
            $item = Get-Item -LiteralPath $cursor -Force
            if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw 'Symbolic links/junctions are not supported. Use a direct local directory.'
            }
        }
        $parent = [IO.Path]::GetDirectoryName($cursor)
        if ($parent -eq $cursor) { break }
        $cursor = $parent
    }
}

function Invoke-AffixGit([string]$Repo, [string[]]$Arguments) {
    # No optional index locks, no hooks, no remote access, and no git mutations.
    $text = @(& git --no-optional-locks -C $Repo @Arguments 2>&1)
    if ($LASTEXITCODE -ne 0) { throw ('Git inspection failed: ' + ($Arguments -join ' ')) }
    return $text
}

function Get-AffixGitState([string]$Repo) {
    $root = Get-AffixFullPath $Repo
    $actual = Get-AffixFullPath ([string](Invoke-AffixGit $root @('rev-parse', '--show-toplevel')))
    if (-not $root.Equals($actual, [StringComparison]::OrdinalIgnoreCase)) { throw 'RepositoryPath must be the repository root.' }
    if (-not (Test-Path -LiteralPath (Join-Path $root 'project.godot') -PathType Leaf)) { throw 'project.godot is missing.' }
    $remote = [string](Invoke-AffixGit $root @('config', '--get', 'remote.origin.url'))
    $matchesRepo = $remote -match '^(https://(?:[^/@]+@)?github\.com/|git@github\.com:)Dev-Gony/affix-zero(?:\.git)?/?$'
    $head = [string](Invoke-AffixGit $root @('rev-parse', 'HEAD'))
    $branch = [string](Invoke-AffixGit $root @('rev-parse', '--abbrev-ref', 'HEAD'))
    $changes = @(Invoke-AffixGit $root @('status', '--porcelain=v1', '--untracked-files=all'))
    $stashes = @(Invoke-AffixGit $root @('stash', 'list', '--format=%H'))
    return [ordered]@{
        expected_origin = [bool]$matchesRepo
        branch = $branch.Trim()
        head = $head.Trim()
        changed_entries = $changes.Count
        stash_count = $stashes.Count
        local_changes_preserved = $true
    }
}

function Get-AffixInventory([string]$Directory) {
    $root = Get-AffixFullPath $Directory
    Assert-AffixNoLink $root
    if (-not (Test-Path -LiteralPath $root -PathType Container)) { throw 'Save directory does not exist.' }
    $queue = New-Object 'System.Collections.Generic.Queue[string]'
    $queue.Enqueue($root)
    $items = New-Object 'System.Collections.Generic.List[object]'
    $seen = 0
    [long]$bytes = 0
    while ($queue.Count -gt 0) {
        $current = $queue.Dequeue()
        foreach ($entry in @(Get-ChildItem -LiteralPath $current -Force)) {
            $seen++
            if ($seen -gt 20000) { throw 'Directory contains too many entries; confirm the selected game data folder.' }
            if (($entry.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw 'Save directory contains a link/junction; no verified backup was produced.' }
            if ($entry.PSIsContainer) { $queue.Enqueue($entry.FullName); continue }
            $bytes += $entry.Length
            if ($bytes -gt 268435456) { throw 'Save directory exceeds 256 MiB; inspect the folder before backing it up.' }
            $relative = $entry.FullName.Substring($root.Length + 1)
            $items.Add([pscustomobject][ordered]@{
                path = $relative
                bytes = [long]$entry.Length
                sha256 = (Get-FileHash -LiteralPath $entry.FullName -Algorithm SHA256).Hash
            })
        }
    }
    return @($items | Sort-Object -Property path)
}

function Copy-AffixRawFile([string]$Source, [string]$Destination) {
    # FileShare.Read rejects a concurrent writer while this particular file is copied.
    $inputFile = [IO.File]::Open($Source, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
    try {
        $outputFile = [IO.File]::Open($Destination, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
        try { $inputFile.CopyTo($outputFile); $outputFile.Flush() } finally { $outputFile.Dispose() }
    } finally { $inputFile.Dispose() }
}

function New-AffixSaveSnapshot([string]$Source, [string]$Destination, [string]$Repo) {
    $sourceRoot = Get-AffixFullPath $Source
    $destRoot = Get-AffixFullPath $Destination
    Assert-AffixNoLink $sourceRoot
    Assert-AffixNoLink $destRoot
    if ((Test-AffixWithin $destRoot $sourceRoot) -or (Test-AffixWithin $sourceRoot $destRoot) -or (Test-AffixWithin $destRoot $Repo) -or (Test-AffixWithin $sourceRoot $Repo)) {
        throw 'Save source and backup destination must be separate and outside the repository.'
    }
    if (Test-Path -LiteralPath $destRoot) { throw 'Destination already exists; backups are never overwritten.' }
    $known = @('save.json', 'save.json.bak', 'save.json.tmp' | Where-Object { Test-Path -LiteralPath (Join-Path $sourceRoot $_) -PathType Leaf })
    if ($known.Count -eq 0) { throw 'No save.json, save.json.bak or save.json.tmp found. Missing is not backed up.' }
    $before = @(Get-AffixInventory $sourceRoot)
    $beforeJson = ConvertTo-Json -InputObject $before -Depth 5 -Compress
    [void][IO.Directory]::CreateDirectory($destRoot)
    try {
        $payload = Join-Path $destRoot 'files'
        [void][IO.Directory]::CreateDirectory($payload)
        foreach ($item in $before) {
            $target = Join-Path $payload $item.path
            [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($target))
            Copy-AffixRawFile (Join-Path $sourceRoot $item.path) $target
        }
        $afterJson = ConvertTo-Json -InputObject @(Get-AffixInventory $sourceRoot) -Depth 5 -Compress
        $copyJson = ConvertTo-Json -InputObject @(Get-AffixInventory $payload) -Depth 5 -Compress
        if (($beforeJson -cne $afterJson) -or ($beforeJson -cne $copyJson)) { throw 'Source changed or copy verification failed. This backup is NOT verified.' }
        $manifest = [ordered]@{
            status = 'VERIFIED_BYTES'
            created_utc = [DateTime]::UtcNow.ToString('o')
            original_source_private = $sourceRoot
            files = $before
            save_semantic_validation = 'NOT_RUN'
            auto_restore = $false
        }
        $manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $destRoot 'VERIFIED.json') -Encoding UTF8
        return [ordered]@{ status = 'VERIFIED_BYTES'; file_count = $before.Count; primary_save_present = (Test-Path -LiteralPath (Join-Path $payload 'save.json') -PathType Leaf) }
    } catch {
        'INCOMPLETE: do not use as a verified backup. Original source was not modified by this tool.' | Set-Content -LiteralPath (Join-Path $destRoot 'INCOMPLETE.txt') -Encoding UTF8
        throw
    }
}

function Get-AffixHardware {
    try {
        $cpu = @(Get-CimInstance Win32_Processor | ForEach-Object { $_.Name })
        $gpu = @(Get-CimInstance Win32_VideoController | ForEach-Object { $_.Name })
        $system = Get-CimInstance Win32_ComputerSystem
        $os = Get-CimInstance Win32_OperatingSystem
        return [ordered]@{ cpu = $cpu; gpu = $gpu; ram_gib = [Math]::Round($system.TotalPhysicalMemory / 1GB, 1); os = $os.Caption; os_build = $os.BuildNumber }
    } catch { return [ordered]@{ status = 'NOT_DETECTED' } }
}

function Invoke-AffixR0 {
    if ($env:OS -ne 'Windows_NT') { throw 'Run the entry point in Windows PowerShell 5.1 or later.' }
    if (($Mode -eq 'Inspect') -and ($ChooseSaveDirectory -or $GameClosed -or $SaveDirectory)) { throw 'Backup options require -Mode Backup.' }
    $repo = Get-AffixFullPath $RepositoryPath
    $state = Get-AffixGitState $repo
    if (-not $state.expected_origin) { throw 'origin is not Dev-Gony/affix-zero. No files were copied.' }
    $running = @(Get-Process | Where-Object { $_.ProcessName -match '^(godot|affix)' })
    if ($Mode -eq 'Backup') {
        if (-not $GameClosed -or $running.Count -gt 0) { throw 'Close the game AND editor; then run with -GameClosed. No process is killed automatically.' }
        if ($ChooseSaveDirectory) {
            if ($SaveDirectory) { throw 'ChooseSaveDirectory and SaveDirectory cannot be used together.' }
            Add-Type -AssemblyName System.Windows.Forms
            $dialog = New-Object System.Windows.Forms.FolderBrowserDialog
            $dialog.Description = 'Select the actual AFFIX user data folder containing save.json (or .bak/.tmp).'
            $dialog.ShowNewFolderButton = $false
            try {
                if ($dialog.ShowDialog() -ne [System.Windows.Forms.DialogResult]::OK) { throw 'Selection cancelled. No backup created.' }
                $SaveDirectory = $dialog.SelectedPath
            } finally { $dialog.Dispose() }
        }
        if ([string]::IsNullOrWhiteSpace($SaveDirectory)) { throw 'Provide -SaveDirectory or -ChooseSaveDirectory. The user data path is not guessed.' }
    }
    $outBase = $OutputRoot
    if ([string]::IsNullOrWhiteSpace($outBase)) { $outBase = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'AFFIX_ZERO_R0' }
    $outBase = Get-AffixFullPath $outBase
    Assert-AffixNoLink $outBase
    if (Test-AffixWithin $outBase $repo) { throw 'Reports/backups must be outside the repository.' }
    if ($SaveDirectory -and ((Test-AffixWithin $outBase $SaveDirectory) -or (Test-AffixWithin $SaveDirectory $outBase))) { throw 'OutputRoot and SaveDirectory must not overlap.' }
    $runDir = Join-Path $outBase ((Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N'))
    [void][IO.Directory]::CreateDirectory($runDir)
    $backup = [ordered]@{ status = 'NOT_RUN' }
    if ($Mode -eq 'Backup') { $backup = New-AffixSaveSnapshot $SaveDirectory (Join-Path $runDir 'save-snapshot') $repo }
    $engine = 'NOT_DETECTED'
    if ($GodotExe) {
        if (-not (Test-Path -LiteralPath $GodotExe -PathType Leaf)) { throw 'GodotExe must be an existing executable.' }
        # Only the explicitly supplied executable is queried; no game is launched.
        $versionText = @(& $GodotExe --version 2>&1)
        if ($LASTEXITCODE -ne 0) { throw 'Godot version query failed.' }
        $engine = ([string]($versionText -join ' ')).Trim()
    }
    $summary = [ordered]@{
        tool = 'AFFIX_R0_v1'
        tool_sha256 = (Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash
        created_utc = [DateTime]::UtcNow.ToString('o')
        powershell = $PSVersionTable.PSVersion.ToString()
        git = $state
        godot_version = $engine
        hardware = Get-AffixHardware
        matching_process_count = $running.Count
        backup = $backup
        engine_decision = 'OPEN_E0_NOT_RUN'
        windows_play_approval = 'NOT_RUN'
    }
    $report = Join-Path $runDir 'share-summary.json'
    $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $report -Encoding UTF8
    Write-Host ($summary | ConvertTo-Json -Depth 8)
    Write-Host ('Report: ' + $report)
    Write-Host 'Share only share-summary.json. Do not upload save-snapshot or raw saves.'
    if ($state.changed_entries -gt 0) { Write-Warning 'Local changes exist and were NOT committed, stashed, or backed up by this tool. Do not switch the working branch yet.' }
    if ($state.stash_count -gt 0) { Write-Host 'Existing stashes were left untouched. Do not pop them automatically.' }
}

if ($MyInvocation.InvocationName -ne '.') {
    try { Invoke-AffixR0 } catch { Write-Error $_; exit 1 }
}
