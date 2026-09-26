param([Parameter(Mandatory = $true)][string]$ZipPath)
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$archivePath = (Resolve-Path -LiteralPath $ZipPath).Path
$expectedZip = '9f1818c7ddc17b99e4bae81feae8b25a3692bf6bc269bb2797e36bd7faec3ba2'
if ((Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expectedZip) {
    throw 'ZIP does not match the reviewed free Soldier/Orc v2.0 source. No files were imported.'
}
$manifestPath = Join-Path $projectRoot 'docs\assets\zerie-local-manifest.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$privateRoot = [IO.Path]::GetFullPath((Join-Path $projectRoot 'Assets\LocalLicensed\Zerie'))
Push-Location -LiteralPath $projectRoot
try {
    $null = git check-ignore 'Assets/LocalLicensed/Zerie/Soldier/Idle.png'
    if ($LASTEXITCODE -ne 0) { throw 'Raw licensed source must be Git-ignored before import.' }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        $prepared = @()
        foreach ($entry in $manifest.files) {
            $target = [IO.Path]::GetFullPath((Join-Path $projectRoot $entry.path))
            if (-not $target.StartsWith($privateRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
                throw 'Manifest path is outside the private asset directory.'
            }
            $actor = Split-Path -Leaf (Split-Path -Parent $target)
            $clip = [IO.Path]::GetFileNameWithoutExtension($target)
            if ($actor -notin @('Soldier', 'Orc') -or $clip -notin @('Idle', 'Walk', 'Attack01', 'Hurt', 'Death')) {
                throw 'Manifest contains an unreviewed actor or clip.'
            }
            $suffix = 'Characters(100x100 split)/' + $actor + '/' + $actor + '/' + $actor + '_' + $clip + '.png'
            $matches = @($archive.Entries | Where-Object { $_.FullName.Replace('\', '/').EndsWith('/' + $suffix, [StringComparison]::Ordinal) })
            if ($matches.Count -ne 1) { throw ('Expected exactly one source entry: ' + $suffix) }
            $inputStream = $matches[0].Open()
            $memory = New-Object IO.MemoryStream
            try { $inputStream.CopyTo($memory); $bytes = $memory.ToArray() }
            finally { $inputStream.Dispose(); $memory.Dispose() }
            $sha = [Security.Cryptography.SHA256]::Create()
            try { $digest = [BitConverter]::ToString($sha.ComputeHash($bytes)).Replace('-', '').ToLowerInvariant() }
            finally { $sha.Dispose() }
            if ($digest -ne $entry.sha256 -or $bytes.Length -ne $entry.bytes) { throw ('Source mismatch: ' + $suffix) }
            if ((Test-Path -LiteralPath $target) -and (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash.ToLowerInvariant() -ne $digest) {
                throw ('Existing local file differs; refusing overwrite: ' + $target)
            }
            $prepared += [pscustomobject]@{ Target = $target; Bytes = $bytes }
        }
        if ($prepared.Count -ne 10) { throw 'Expected ten reviewed character sheets.' }
        foreach ($file in $prepared) {
            $null = [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($file.Target))
            if (-not (Test-Path -LiteralPath $file.Target)) { [IO.File]::WriteAllBytes($file.Target, $file.Bytes) }
        }
        Write-Output 'Verified ten local character sheets. Raw artwork remains excluded from Git.'
        Write-Output 'Next: Unity menu AFFIX / Setup / Build Hero Siege Art Review Scene.'
    } finally { $archive.Dispose() }
} finally { Pop-Location }
