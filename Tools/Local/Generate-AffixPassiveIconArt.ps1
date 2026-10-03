param([string] $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sourceDirectory = Join-Path $RepoRoot 'docs\art-source\passive-icons-v2'
$outputDirectory = Join-Path $RepoRoot 'Assets\Art\Lucifer\Resources\AffixGenerated'
$reportDirectory = Join-Path $RepoRoot 'Build\Reports\passive-icon-art'
New-Item -ItemType Directory -Force -Path $outputDirectory, $reportDirectory | Out-Null

$expectedSources = [ordered]@{
    'PowerRune.png' = '6E45B098AD2918196375A973BEAFF668A046DCBA71F21997777365F883FF13EF'
    'PrecisionRune.png' = '58F65C43ED07BAB6456262263A5B399DFE79F841FAB9388C0E183705DF260F08'
    'VeteranRune.png' = 'CBEEF4F0D2960CEAD8973693B89ED81734098040579E009300F9AEFDD5C17F0D'
    'VitalityRune.png' = 'DDC698C627F9A4EDA7768341185E48F8CFFDBF554BD319A588FA7AE885B8CC78'
    'CleaveRune.png' = '8E3F285A22A4D7540F5CBFFF2155F8BE06873B6C8C3F9E67A0F2BA652F620F81'
    'HasteRune.png' = '079D97C8C467217F046D77E728A24FFFB0BF2E741DEA39CF7321EC1C601CFA34'
}

function Save-PassiveIcon([string] $name) {
    $sourcePath = Join-Path $sourceDirectory $name
    $outputPath = Join-Path $outputDirectory $name
    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) { throw "Missing passive icon source: $sourcePath" }
    $sourceHash = (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash
    if ($sourceHash -ne $expectedSources[$name]) { throw "Passive icon source hash differs for ${name}: $sourceHash" }

    $source = [System.Drawing.Bitmap]::new($sourcePath)
    $icon = [System.Drawing.Bitmap]::new(128, 128, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($icon)
    try {
        $graphics.Clear([System.Drawing.Color]::Transparent)
        $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $available = 120
        $scale = [Math]::Min($available / [double]$source.Width, $available / [double]$source.Height)
        $width = [Math]::Max(1, [int][Math]::Round($source.Width * $scale))
        $height = [Math]::Max(1, [int][Math]::Round($source.Height * $scale))
        $x = [int][Math]::Floor((128 - $width) / 2.0)
        $y = [int][Math]::Floor((128 - $height) / 2.0)
        $graphics.DrawImage($source, [System.Drawing.Rectangle]::new($x, $y, $width, $height),
            0, 0, $source.Width, $source.Height, [System.Drawing.GraphicsUnit]::Pixel)
    }
    finally {
        $graphics.Dispose()
        $source.Dispose()
    }
    try { $icon.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png) }
    finally { $icon.Dispose() }
}

foreach ($name in $expectedSources.Keys) { Save-PassiveIcon $name }

$seenHashes = @{}
foreach ($name in $expectedSources.Keys) {
    $path = Join-Path $outputDirectory $name
    $bitmap = [System.Drawing.Bitmap]::new($path)
    $nonTransparent = 0
    try {
        if ($bitmap.Width -ne 128 -or $bitmap.Height -ne 128) { throw "Unexpected passive icon dimensions: $name" }
        for ($y = 0; $y -lt 128; $y++) {
            for ($x = 0; $x -lt 128; $x++) {
                $alpha = $bitmap.GetPixel($x, $y).A
                if ($alpha -gt 0) { $nonTransparent++ }
                if (($x -eq 0 -or $y -eq 0 -or $x -eq 127 -or $y -eq 127) -and $alpha -gt 0) {
                    throw "Passive icon alpha touches the canvas border: $name"
                }
            }
        }
    }
    finally { $bitmap.Dispose() }
    if ($nonTransparent -lt 500) { throw "Passive icon appears empty: $name" }
    $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
    if ($seenHashes.ContainsKey($hash)) { throw "Duplicate passive icon pixels: $name and $($seenHashes[$hash])" }
    $seenHashes[$hash] = $name
    Write-Output "$name|128x128|nonTransparent=$nonTransparent|sha256=$hash"
}

$sheetHeight = 68 + $expectedSources.Count * 138
$sheet = [System.Drawing.Bitmap]::new(900, $sheetHeight, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$graphics = [System.Drawing.Graphics]::FromImage($sheet)
$background = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 12, 10, 13))
$cell = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 31, 19, 24))
$gold = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 233, 195, 73))
$cream = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 226, 226, 230))
$edge = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 116, 68, 53), 2)
$titleFont = [System.Drawing.Font]::new('Segoe UI', 18, [System.Drawing.FontStyle]::Bold)
$labelFont = [System.Drawing.Font]::new('Segoe UI', 13, [System.Drawing.FontStyle]::Bold)
$smallFont = [System.Drawing.Font]::new('Consolas', 10)
try {
    $graphics.FillRectangle($background, 0, 0, $sheet.Width, $sheet.Height)
    $graphics.DrawString('AFFIX: ZERO - ALL SIX PAINTED PASSIVES AT 128 / 64 / 48 / 37 PX', $titleFont, $gold, 20, 12)
    $row = 0
    foreach ($name in $expectedSources.Keys) {
        $y = 52 + $row * 138
        $graphics.FillRectangle($cell, 18, $y, 864, 126)
        $graphics.DrawRectangle($edge, 18, $y, 864, 126)
        $graphics.DrawString(($name -replace '\.png$', ''), $labelFont, $cream, 32, $y + 15)
        $image = [System.Drawing.Image]::FromFile((Join-Path $outputDirectory $name))
        try {
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.DrawImage($image, 250, $y - 1, 128, 128)
            $graphics.DrawImage($image, 430, $y + 31, 64, 64)
            $graphics.DrawImage($image, 550, $y + 39, 48, 48)
            $graphics.DrawImage($image, 660, $y + 45, 37, 37)
        }
        finally { $image.Dispose() }
        $graphics.DrawString('128 px', $smallFont, $gold, 286, $y + 103)
        $graphics.DrawString('64 px', $smallFont, $gold, 439, $y + 96)
        $graphics.DrawString('48 px', $smallFont, $gold, 554, $y + 90)
        $graphics.DrawString('37 px', $smallFont, $gold, 660, $y + 84)
        $row++
    }
}
finally {
    $titleFont.Dispose(); $labelFont.Dispose(); $smallFont.Dispose()
    $background.Dispose(); $cell.Dispose(); $gold.Dispose(); $cream.Dispose(); $edge.Dispose()
    $graphics.Dispose()
}
$sheetPath = Join-Path $reportDirectory 'AFFIX-ZERO-passive-icons-128-64-48-37.png'
try { $sheet.Save($sheetPath, [System.Drawing.Imaging.ImageFormat]::Png) }
finally { $sheet.Dispose() }
Write-Output $sheetPath
