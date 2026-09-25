#requires -Version 5.1
[CmdletBinding()]
param(
    [string[]]$LegacyPaths = @('D:\github\affix', 'D:\github\affix-e0'),
    [string]$CurrentProject = (Join-Path $PSScriptRoot '../..'),
    [int]$MaximumEntries = 250000,
    [int]$SecondsPerFolder = 60
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($MaximumEntries -lt 1 -or $SecondsPerFolder -lt 1) { throw 'Positive limits required.' }

function Measure-ReadOnly([string]$Path) {
    $result = [ordered]@{ path = $Path; status = 'MISSING'; bytes = [long]0; files = 0; skipped_links = 0; unreadable = 0 }
    if (-not (Test-Path -LiteralPath $Path -PathType Container)) { return $result }
    $watch = [Diagnostics.Stopwatch]::StartNew()
    $queue = New-Object 'System.Collections.Generic.Queue[string]'
    $queue.Enqueue([IO.Path]::GetFullPath($Path))
    $result.status = 'COMPLETE'
    $entries = 0
    while ($queue.Count -gt 0) {
        if ($watch.Elapsed.TotalSeconds -ge $SecondsPerFolder -or $entries -ge $MaximumEntries) { $result.status = 'PARTIAL_LIMIT'; break }
        $dir = $queue.Dequeue()
        try {
            if (((Get-Item -LiteralPath $dir -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { $result.skipped_links++; continue }
            foreach ($entry in Get-ChildItem -LiteralPath $dir -Force -ErrorAction Stop) {
                $entries++
                if ($entries -gt $MaximumEntries -or $watch.Elapsed.TotalSeconds -ge $SecondsPerFolder) { $result.status = 'PARTIAL_LIMIT'; break }
                if (($entry.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { $result.skipped_links++; continue }
                if ($entry.PSIsContainer) { $queue.Enqueue($entry.FullName) }
                else { $result.bytes += $entry.Length; $result.files++ }
            }
        } catch { $result.unreadable++; $result.status = 'PARTIAL_UNREADABLE' }
    }
    if ($result.skipped_links -gt 0 -and $result.status -eq 'COMPLETE') { $result.status = 'PARTIAL_LINKS_SKIPPED' }
    $result['gib'] = [Math]::Round($result.bytes / 1GB, 3)
    return $result
}
function Read-Git([string]$Repo, [string[]]$Arguments) {
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) { return @('GIT_UNAVAILABLE') }
    if (-not (Test-Path -LiteralPath $Repo -PathType Container)) { return @('PATH_MISSING') }
    $previousPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $lines = @(& git --no-optional-locks -C $Repo @Arguments 2>&1)
        $code = $LASTEXITCODE
    } finally { $ErrorActionPreference = $previousPreference }
    if ($code -ne 0) { return @('GIT_READ_FAILED') }
    return @($lines | ForEach-Object { [string]$_ })
}
function Inspect-Git([string]$Path) {
    return [ordered]@{
        path = $Path
        head = @(Read-Git $Path @('rev-parse', 'HEAD'))
        common_dir = @(Read-Git $Path @('rev-parse', '--path-format=absolute', '--git-common-dir'))
        status = @(Read-Git $Path @('status', '--porcelain=v1', '--untracked-files=normal'))
        worktrees = @(Read-Git $Path @('worktree', 'list', '--porcelain'))
        object_sizes = @(Read-Git $Path @('count-objects', '-v'))
        shallow = @(Read-Git $Path @('rev-parse', '--is-shallow-repository'))
        stashes = @(Read-Git $Path @('stash', 'list', '--format=%gd'))
    }
}
$current = [IO.Path]::GetFullPath($CurrentProject)
if (-not (Test-Path -LiteralPath (Join-Path $current 'ProjectSettings/ProjectVersion.txt'))) { throw 'CurrentProject is not the Unity project root.' }
Write-Host 'Read-only inspection. No delete, move, stash, restore, checkout or fetch is performed.'
$folders = @()
foreach ($path in $LegacyPaths) { Write-Host ('Measuring: ' + $path); $folders += (Measure-ReadOnly $path) }
$repos = @((Inspect-Git $current))
foreach ($path in $LegacyPaths) { $repos += (Inspect-Git $path) }
$report = [ordered]@{
    schema = 'affix-storage-readonly-v1'; utc = [DateTime]::UtcNow.ToString('o')
    current_project = $current; folders = $folders; git = $repos
    deletion_performed = $false; estimate = 'Logical file sizes; not exact reclaimable disk space. Partial measurements are lower bounds.'
    protected = @('Current Unity source and .meta', 'Personal saves outside repositories', 'Unreviewed local changes', 'Git stashes/history')
}
$out = Join-Path $current 'Build/Reports'
[void][IO.Directory]::CreateDirectory($out)
$file = Join-Path $out ('storage-' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N') + '.json')
$report | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $file -Encoding UTF8
$folders | ForEach-Object { [pscustomobject]$_ } | Format-Table path, status, gib, files -AutoSize
Write-Host ('Report: ' + $file)
Write-Host 'Review paths before sharing. No legacy folder or user save was deleted.'
# Expected Git read failures are recorded in JSON, not the process exit code.
# Reaching this point means the report was written successfully; errors above still throw.
$global:LASTEXITCODE = 0
