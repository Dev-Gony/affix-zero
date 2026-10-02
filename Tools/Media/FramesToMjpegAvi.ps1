param(
    [Parameter(Mandatory=$true)][string]$InputDirectory,
    [Parameter(Mandatory=$true)][string]$OutputFile,
    [ValidateRange(1,60)][int]$FramesPerSecond = 4
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Write-FourCC([System.IO.BinaryWriter]$Writer,[string]$Value) {
    if ($Value.Length -ne 4) { throw "FourCC must contain four characters: $Value" }
    $Writer.Write([System.Text.Encoding]::ASCII.GetBytes($Value))
}
function Patch-UInt32([System.IO.BinaryWriter]$Writer,[long]$Position,[uint32]$Value) {
    $return = $Writer.BaseStream.Position
    $Writer.BaseStream.Position = $Position
    $Writer.Write($Value)
    $Writer.BaseStream.Position = $return
}

$resolvedInput = (Resolve-Path -LiteralPath $InputDirectory).Path
$frames = @(Get-ChildItem -LiteralPath $resolvedInput -File | Where-Object Extension -in '.png','.bmp' | Sort-Object Name)
if ($frames.Count -eq 0) { throw "No PNG or BMP frames were found in $resolvedInput" }
$resolvedOutput = [System.IO.Path]::GetFullPath($OutputFile)
$outputParent = Split-Path -Parent $resolvedOutput
if (-not (Test-Path -LiteralPath $outputParent)) { New-Item -ItemType Directory -Path $outputParent | Out-Null }

$jpegCodec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object MimeType -eq 'image/jpeg' | Select-Object -First 1
if ($null -eq $jpegCodec) { throw 'Windows JPEG encoder is unavailable.' }
$quality = New-Object System.Drawing.Imaging.EncoderParameters(1)
$quality.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter([System.Drawing.Imaging.Encoder]::Quality,[long]82)
$jpegFrames = New-Object System.Collections.Generic.List[byte[]]
$width = 0; $height = 0; $largest = 0
try {
    foreach ($frame in $frames) {
        $image = [System.Drawing.Image]::FromFile($frame.FullName)
        try {
            if ($width -eq 0) { $width = $image.Width; $height = $image.Height }
            if ($image.Width -ne $width -or $image.Height -ne $height) { throw "Frame dimensions differ: $($frame.FullName)" }
            $memory = New-Object System.IO.MemoryStream
            try {
                $image.Save($memory,$jpegCodec,$quality)
                $bytes = $memory.ToArray(); $jpegFrames.Add($bytes)
                if ($bytes.Length -gt $largest) { $largest = $bytes.Length }
            } finally { $memory.Dispose() }
        } finally { $image.Dispose() }
    }
} finally { $quality.Dispose() }

$stream = [System.IO.File]::Create($resolvedOutput)
$writer = New-Object System.IO.BinaryWriter($stream)
try {
    Write-FourCC $writer 'RIFF'; $riffSizePosition = $stream.Position; $writer.Write([uint32]0); Write-FourCC $writer 'AVI '
    Write-FourCC $writer 'LIST'; $headerSizePosition = $stream.Position; $writer.Write([uint32]0); $headerStart = $stream.Position; Write-FourCC $writer 'hdrl'

    Write-FourCC $writer 'avih'; $writer.Write([uint32]56)
    $writer.Write([uint32][Math]::Round(1000000 / $FramesPerSecond)); $writer.Write([uint32]($largest * $FramesPerSecond))
    $writer.Write([uint32]0); $writer.Write([uint32]16); $writer.Write([uint32]$jpegFrames.Count); $writer.Write([uint32]0)
    $writer.Write([uint32]1); $writer.Write([uint32]$largest); $writer.Write([uint32]$width); $writer.Write([uint32]$height)
    1..4 | ForEach-Object { $writer.Write([uint32]0) }

    Write-FourCC $writer 'LIST'; $streamHeaderSizePosition = $stream.Position; $writer.Write([uint32]0); $streamHeaderStart = $stream.Position; Write-FourCC $writer 'strl'
    Write-FourCC $writer 'strh'; $writer.Write([uint32]56); Write-FourCC $writer 'vids'; Write-FourCC $writer 'MJPG'
    $writer.Write([uint32]0); $writer.Write([uint16]0); $writer.Write([uint16]0); $writer.Write([uint32]0)
    $writer.Write([uint32]1); $writer.Write([uint32]$FramesPerSecond); $writer.Write([uint32]0); $writer.Write([uint32]$jpegFrames.Count)
    $writer.Write([uint32]$largest); $writer.Write([uint32]10000); $writer.Write([uint32]0)
    $writer.Write([int16]0); $writer.Write([int16]0); $writer.Write([int16]$width); $writer.Write([int16]$height)
    Write-FourCC $writer 'strf'; $writer.Write([uint32]40); $writer.Write([uint32]40); $writer.Write([int32]$width); $writer.Write([int32]$height)
    $writer.Write([uint16]1); $writer.Write([uint16]24); Write-FourCC $writer 'MJPG'; $writer.Write([uint32]$largest)
    $writer.Write([int32]0); $writer.Write([int32]0); $writer.Write([uint32]0); $writer.Write([uint32]0)
    Patch-UInt32 $writer $streamHeaderSizePosition ([uint32]($stream.Position - $streamHeaderStart))
    Patch-UInt32 $writer $headerSizePosition ([uint32]($stream.Position - $headerStart))

    Write-FourCC $writer 'LIST'; $moviSizePosition = $stream.Position; $writer.Write([uint32]0); $moviStart = $stream.Position; Write-FourCC $writer 'movi'
    $index = New-Object System.Collections.Generic.List[object]
    foreach ($bytes in $jpegFrames) {
        $chunkPosition = $stream.Position
        Write-FourCC $writer '00dc'; $writer.Write([uint32]$bytes.Length); $writer.Write($bytes)
        if (($bytes.Length -band 1) -ne 0) { $writer.Write([byte]0) }
        $index.Add([pscustomobject]@{ Offset=[uint32]($chunkPosition - $moviStart); Size=[uint32]$bytes.Length })
    }
    Patch-UInt32 $writer $moviSizePosition ([uint32]($stream.Position - $moviStart))
    Write-FourCC $writer 'idx1'; $writer.Write([uint32]($index.Count * 16))
    foreach ($entry in $index) { Write-FourCC $writer '00dc'; $writer.Write([uint32]16); $writer.Write($entry.Offset); $writer.Write($entry.Size) }
    Patch-UInt32 $writer $riffSizePosition ([uint32]($stream.Length - 8))
} finally { $writer.Dispose(); $stream.Dispose() }

$hash = (Get-FileHash -LiteralPath $resolvedOutput -Algorithm SHA256).Hash
[pscustomobject]@{ File=$resolvedOutput; Frames=$frames.Count; FramesPerSecond=$FramesPerSecond; Width=$width; Height=$height; Bytes=(Get-Item -LiteralPath $resolvedOutput).Length; Sha256=$hash }
