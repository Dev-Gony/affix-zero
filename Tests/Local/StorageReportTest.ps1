#requires -Version 5.1
$ErrorActionPreference = 'Stop'
$root = Join-Path ([IO.Path]::GetTempPath()) ('affix-storage-test-' + [Guid]::NewGuid().ToString('N'))
[void][IO.Directory]::CreateDirectory($root)
try {
    $current = Join-Path $root 'current'
    [void][IO.Directory]::CreateDirectory((Join-Path $current 'ProjectSettings'))
    [IO.File]::WriteAllText((Join-Path $current 'ProjectSettings/ProjectVersion.txt'), 'm_EditorVersion: 6000.3.24f1')
    $old = Join-Path $root 'old'
    [void][IO.Directory]::CreateDirectory($old)
    $data = Join-Path $old 'preserve.bin'
    [IO.File]::WriteAllBytes($data, [byte[]](1,2,3,4))
    $before = (Get-FileHash -LiteralPath $data).Hash
    & (Join-Path $PSScriptRoot '../../Tools/Local/Inspect-LegacyStorage.ps1') -CurrentProject $current -LegacyPaths @($old, (Join-Path $root 'missing'))
    if ($LASTEXITCODE -ne 0) { throw 'Informational Git failures leaked into successful report exit status.' }
    $reports = @(Get-ChildItem -LiteralPath (Join-Path $current 'Build/Reports') -Filter '*.json')
    if ($reports.Count -ne 1) { throw 'Expected one report.' }
    $report = Get-Content -LiteralPath $reports[0].FullName -Raw | ConvertFrom-Json
    if ($report.deletion_performed -or $report.folders[0].bytes -ne 4 -or $report.folders[1].status -ne 'MISSING') { throw 'Bad report.' }
    if ((Get-FileHash -LiteralPath $data).Hash -ne $before) { throw 'Source changed.' }
    if (-not (Test-Path -LiteralPath $old)) { throw 'Legacy path removed.' }
    if ($report.git[0].head[0] -ne 'GIT_READ_FAILED') { throw 'Missing repository status must be explicit.' }
    Write-Host 'STORAGE_REPORT_TEST_PASSED synthetic-fixtures-only'
} finally {
    Remove-Item -LiteralPath $root -Recurse -Force
}
