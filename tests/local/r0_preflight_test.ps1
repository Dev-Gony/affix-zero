#requires -Version 5.1
$ErrorActionPreference = 'Stop'
$tool = Join-Path $PSScriptRoot '../../tools/local/Invoke-AffixR0.ps1'
. $tool
$script:Checks = 0
function Assert-R0([bool]$Condition, [string]$Name) {
    if (-not $Condition) { throw ('FAIL: ' + $Name) }
    $script:Checks++
    Write-Host ('PASS: ' + $Name)
}
function Assert-R0Throws([scriptblock]$Action, [string]$Name) {
    $thrown = $false
    try { & $Action | Out-Null } catch { $thrown = $true }
    Assert-R0 $thrown $Name
}
function Fixture-Git([string[]]$Arguments) {
    & git -C $script:Repo @Arguments 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Fixture git setup failed.' }
}
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('affix-r0-test-' + [Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($fixture)
$script:Repo = Join-Path $fixture 'repo'
$save = Join-Path $fixture ('save [raw] ' + [char]0xAC00)
$copy = Join-Path $fixture 'snapshot-1'
try {
    [void][IO.Directory]::CreateDirectory($script:Repo)
    Fixture-Git @('init', '-q')
    Fixture-Git @('config', 'user.email', 'fixture@example.invalid')
    Fixture-Git @('config', 'user.name', 'Synthetic R0 Test')
    Set-Content -LiteralPath (Join-Path $script:Repo 'project.godot') -Value 'config_version=5' -Encoding ASCII
    Fixture-Git @('add', 'project.godot')
    Fixture-Git @('commit', '-qm', 'fixture')
    Fixture-Git @('remote', 'add', 'origin', 'https://github.com/Dev-Gony/affix-zero.git')
    $state = Get-AffixGitState $script:Repo
    Assert-R0 ($state.expected_origin -and $state.changed_entries -eq 0) 'clean expected repository'
    Assert-R0 ($state.head -match '^[0-9a-f]{40}$') 'exact local commit is captured'
    $indexBefore = (Get-FileHash -LiteralPath (Join-Path $script:Repo '.git/index')).Hash
    Set-Content -LiteralPath (Join-Path $script:Repo 'project.godot') -Value 'config_version=4' -Encoding ASCII
    Fixture-Git @('stash', 'push', '-qm', 'synthetic-existing-stash')
    Set-Content -LiteralPath (Join-Path $script:Repo 'untracked.txt') -Value 'preserve me' -Encoding ASCII
    $indexBefore = (Get-FileHash -LiteralPath (Join-Path $script:Repo '.git/index')).Hash
    $state = Get-AffixGitState $script:Repo
    Assert-R0 ($state.changed_entries -eq 1 -and $state.stash_count -eq 1) 'dirty state and existing stash reported'
    Assert-R0 ((Get-FileHash -LiteralPath (Join-Path $script:Repo '.git/index')).Hash -eq $indexBefore) 'inspection does not update the index'
    Assert-R0 ((Get-Content -LiteralPath (Join-Path $script:Repo 'untracked.txt') -Raw).Trim() -eq 'preserve me') 'untracked file stays unchanged'
    Fixture-Git @('remote', 'set-url', 'origin', 'https://fake-token-never-share@github.com/Dev-Gony/affix-zero.git')
    $state = Get-AffixGitState $script:Repo
    Assert-R0 ($state.expected_origin -and (($state | ConvertTo-Json) -notmatch 'fake-token')) 'credentials in origin are not included in report'
    Fixture-Git @('remote', 'set-url', 'origin', 'https://github.com/example/wrong.git')
    Assert-R0 (-not (Get-AffixGitState $script:Repo).expected_origin) 'wrong origin is reported'
    Fixture-Git @('remote', 'set-url', 'origin', 'https://github.com/Dev-Gony/affix-zero.git')
    Assert-R0Throws { Get-AffixGitState $fixture } 'non-repository rejected'
    [void][IO.Directory]::CreateDirectory((Join-Path $save 'backups'))
    [IO.File]::WriteAllBytes((Join-Path $save 'save.json'), [byte[]](123,34,255,0,13,10))
    [IO.File]::WriteAllBytes((Join-Path $save 'save.json.bak'), [byte[]](0,1,2,3))
    [IO.File]::WriteAllBytes((Join-Path $save 'save.json.tmp'), [byte[]](9,8,7))
    [IO.File]::WriteAllBytes((Join-Path $save 'backups/unknown.bin'), [byte[]](254,253,10))
    $before = ConvertTo-Json -InputObject @(Get-AffixInventory $save) -Depth 5 -Compress
    $snapshot = New-AffixSaveSnapshot $save $copy $script:Repo
    Assert-R0 ($snapshot.status -eq 'VERIFIED_BYTES' -and $snapshot.file_count -eq 4) 'raw primary, rollback, temp and nested files verified'
    Assert-R0 $snapshot.primary_save_present 'primary save presence recorded'
    Assert-R0 (Test-Path -LiteralPath (Join-Path $copy 'VERIFIED.json')) 'success marker is written only after verification'
    Assert-R0 ($before -ceq (ConvertTo-Json -InputObject @(Get-AffixInventory $save) -Depth 5 -Compress)) 'source bytes unchanged including malformed JSON'
    Assert-R0 ($before -ceq (ConvertTo-Json -InputObject @(Get-AffixInventory (Join-Path $copy 'files')) -Depth 5 -Compress)) 'nested and unknown files copied byte-for-byte'
    $manifest = Get-Content -LiteralPath (Join-Path $copy 'VERIFIED.json') -Raw | ConvertFrom-Json
    Assert-R0 ($manifest.save_semantic_validation -eq 'NOT_RUN' -and -not $manifest.auto_restore) 'byte preservation does not claim save recovery'
    Assert-R0Throws { New-AffixSaveSnapshot $save $copy $script:Repo } 'existing destination is never overwritten'
    $second = New-AffixSaveSnapshot $save (Join-Path $fixture 'snapshot-2') $script:Repo
    Assert-R0 ($second.status -eq 'VERIFIED_BYTES') 'repeat backup uses a separate directory'
    Assert-R0Throws { New-AffixSaveSnapshot (Join-Path $fixture 'missing') (Join-Path $fixture 'absent-result') $script:Repo } 'missing save directory is not a successful backup'
    $empty = Join-Path $fixture 'empty'
    [void][IO.Directory]::CreateDirectory($empty)
    Assert-R0Throws { New-AffixSaveSnapshot $empty (Join-Path $fixture 'empty-result') $script:Repo } 'empty save directory rejected'
    Assert-R0 (-not (Test-Path -LiteralPath (Join-Path $fixture 'empty-result'))) 'invalid input creates no success directory'
    Assert-R0Throws { New-AffixSaveSnapshot $save (Join-Path $save 'child') $script:Repo } 'output inside source rejected'
    Assert-R0Throws { New-AffixSaveSnapshot $save $fixture $script:Repo } 'output containing source rejected'
    Assert-R0Throws { New-AffixSaveSnapshot $save (Join-Path $script:Repo 'leak') $script:Repo } 'private backup inside repository rejected'
    Assert-R0Throws { New-AffixSaveSnapshot $script:Repo (Join-Path $fixture 'bad-source') $script:Repo } 'repository selected as source rejected'
    $bakOnly = Join-Path $fixture 'bak-only'
    [void][IO.Directory]::CreateDirectory($bakOnly)
    [IO.File]::WriteAllBytes((Join-Path $bakOnly 'save.json.bak'), [byte[]](1,2))
    $recovery = New-AffixSaveSnapshot $bakOnly (Join-Path $fixture 'bak-result') $script:Repo
    Assert-R0 ($recovery.status -eq 'VERIFIED_BYTES' -and -not $recovery.primary_save_present) 'rollback-only data preserved without claiming primary recovery'
    $realCopy = ${function:Copy-AffixRawFile}
    function Copy-AffixRawFile([string]$Source, [string]$Destination) {
        & $realCopy $Source $Destination
        if ([IO.Path]::GetFileName($Source) -eq 'save.json') { [IO.File]::AppendAllText($Source, 'simulated-concurrent-write') }
    }
    $unstable = Join-Path $fixture 'unstable'
    try { Assert-R0Throws { New-AffixSaveSnapshot $save $unstable $script:Repo } 'concurrent source change rejects verification' }
    finally { Set-Item -Path function:Copy-AffixRawFile -Value $realCopy }
    Assert-R0 ((Test-Path -LiteralPath (Join-Path $unstable 'INCOMPLETE.txt')) -and -not (Test-Path -LiteralPath (Join-Path $unstable 'VERIFIED.json'))) 'failed copy has only an incomplete marker'
    $junction = Join-Path $fixture 'junction'
    & cmd.exe /d /c "mklink /J `"$junction`" `"$save`"" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Could not create test junction.' }
    try { Assert-R0Throws { New-AffixSaveSnapshot $junction (Join-Path $fixture 'link-result') $script:Repo } 'junction source rejected' }
    finally { & cmd.exe /d /c "rmdir `"$junction`"" | Out-Null }
    $nested = Join-Path $save 'linked-directory'
    & cmd.exe /d /c "mklink /J `"$nested`" `"$empty`"" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Could not create nested test junction.' }
    try { Assert-R0Throws { Get-AffixInventory $save } 'nested junction rejected without following it' }
    finally { & cmd.exe /d /c "rmdir `"$nested`"" | Out-Null }
    Assert-R0 (Test-AffixWithin (Join-Path $save 'child') $save) 'path containment handles brackets and Unicode'
    Assert-R0 (-not (Test-AffixWithin ($save + '-other') $save)) 'similar path prefixes do not count as descendants'
    $shell = (Get-Process -Id $PID).Path
    $out = Join-Path $fixture 'cli-reports'
    & $shell -NoProfile -ExecutionPolicy Bypass -File $tool -RepositoryPath $script:Repo -OutputRoot $out
    Assert-R0 ($LASTEXITCODE -eq 0) 'CLI inspect succeeds'
    $report = @(Get-ChildItem -LiteralPath $out -Filter share-summary.json -Recurse)
    Assert-R0 ($report.Count -eq 1) 'CLI produces exactly one shareable report'
    $summary = Get-Content -LiteralPath $report[0].FullName -Raw | ConvertFrom-Json
    Assert-R0 ($summary.backup.status -eq 'NOT_RUN' -and $summary.engine_decision -eq 'OPEN_E0_NOT_RUN') 'inspection neither backs up nor decides engine'
    Assert-R0 ($summary.tool_sha256 -eq (Get-FileHash -LiteralPath $tool -Algorithm SHA256).Hash) 'report identifies the exact tool bytes'
    Write-Host ('R0 TESTS: ' + $script:Checks + ' checks, 0 failures; synthetic fixtures only.')
} finally {
    # Only this unique test-owned temporary directory is removed.
    Remove-Item -LiteralPath $fixture -Recurse -Force
}
