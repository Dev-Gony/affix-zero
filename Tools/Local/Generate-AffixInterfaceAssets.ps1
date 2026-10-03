param([string] $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$source = Join-Path $RepoRoot 'Assets\Art\Generated\Resources\AffixGenerated\HudFrames-v1.png'
$output = Join-Path $RepoRoot 'Assets\Art\Interface\Resources\AffixUIVisual'
if (-not (Test-Path -LiteralPath $source)) { throw "Missing original HUD atlas: $source" }
New-Item -ItemType Directory -Force -Path $output | Out-Null

function Save-Crop([System.Drawing.Bitmap] $atlas, [string] $name, [int] $x, [int] $y, [int] $width, [int] $height) {
    $rect = [System.Drawing.Rectangle]::new($x, $y, $width, $height)
    $crop = $atlas.Clone($rect, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try { $crop.Save((Join-Path $output $name), [System.Drawing.Imaging.ImageFormat]::Png) } finally { $crop.Dispose() }
}

$atlas = [System.Drawing.Bitmap]::FromFile($source)
try {
    Save-Crop $atlas 'FramePanel.png' 482 438 430 438
    Save-Crop $atlas 'FrameDock.png' 0 548 466 220
    Save-Crop $atlas 'FrameSlotGold.png' 1062 126 205 210
    Save-Crop $atlas 'FrameSlotSilver.png' 1472 126 205 210
    Save-Crop $atlas 'FrameBar.png' 438 156 548 168
    Save-Crop $atlas 'FrameCrest.png' 35 28 350 342
    Save-Crop $atlas 'FrameButton.png' 1388 584 380 145
} finally { $atlas.Dispose() }

function Save-Leather {
    $bitmap = [System.Drawing.Bitmap]::new(512, 512, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $random = [System.Random]::new(1701)
    for ($y = 0; $y -lt 512; $y++) {
        for ($x = 0; $x -lt 512; $x++) {
            $grain = $random.Next(-10, 11)
            $vein = if ((($x + 2 * $y) % 47) -lt 2) { 8 } else { 0 }
            $r = [Math]::Max(18, [Math]::Min(76, 45 + $grain + $vein))
            $g = [Math]::Max(7, [Math]::Min(35, 15 + [int]($grain / 3)))
            $b = [Math]::Max(12, [Math]::Min(43, 24 + [int]($grain / 2)))
            $bitmap.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $r, $g, $b))
        }
    }
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.DrawRectangle([System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(130, 174, 87, 59), 4), 8, 8, 495, 495)
        $graphics.DrawRectangle([System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(180, 22, 8, 13), 2), 15, 15, 481, 481)
        $stitch = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(120, 214, 150, 91), 2)
        $stitch.DashPattern = [single[]](3, 5)
        $graphics.DrawLine($stitch, 26, 30, 486, 30); $graphics.DrawLine($stitch, 26, 482, 486, 482)
        $stitch.Dispose()
    } finally { $graphics.Dispose() }
    try { $bitmap.Save((Join-Path $output 'LeatherBurgundy.png'), [System.Drawing.Imaging.ImageFormat]::Png) } finally { $bitmap.Dispose() }
}

function Save-BagCell {
    $bitmap = [System.Drawing.Bitmap]::new(64, 64, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.Clear([System.Drawing.Color]::FromArgb(255, 7, 11, 13))
        $graphics.FillRectangle([System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 13, 24, 24)), 4, 4, 56, 56)
        $graphics.DrawRectangle([System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 83, 68, 39), 1), 1, 1, 61, 61)
        $graphics.DrawRectangle([System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 37, 45, 42), 1), 4, 4, 55, 55)
        $graphics.DrawLine([System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(120, 160, 115, 52), 1), 5, 5, 58, 5)
    } finally { $graphics.Dispose() }
    try { $bitmap.Save((Join-Path $output 'BagCell.png'), [System.Drawing.Imaging.ImageFormat]::Png) } finally { $bitmap.Dispose() }
}

function Save-PixelIcon([string] $name, [scriptblock] $draw) {
    $small = [System.Drawing.Bitmap]::new(64, 64, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($small)
    try {
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::None
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
        $graphics.Clear([System.Drawing.Color]::Transparent)
        & $draw $graphics
    } finally { $graphics.Dispose() }
    $large = [System.Drawing.Bitmap]::new(128, 128, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $scale = [System.Drawing.Graphics]::FromImage($large)
    try {
        $scale.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
        $scale.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
        $scale.DrawImage($small, [System.Drawing.Rectangle]::new(0, 0, 128, 128), 0, 0, 64, 64, [System.Drawing.GraphicsUnit]::Pixel)
    } finally { $scale.Dispose(); $small.Dispose() }
    try { $large.Save((Join-Path $output $name), [System.Drawing.Imaging.ImageFormat]::Png) } finally { $large.Dispose() }
}

$outline = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 15, 12, 18), 4)
$steel = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 104, 116, 132))
$steelDark = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 49, 55, 70))
$shine = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 220, 224, 215), 3)
$gold = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 205, 145, 42))
$goldLight = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 255, 222, 110), 3)
$crimson = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 145, 24, 49))
$purple = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 132, 56, 220))
$cyan = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 55, 204, 230))

Save-PixelIcon 'GearHelmet.png' {
    param($g)
    $hornL = [System.Drawing.Point[]]@([System.Drawing.Point]::new(18,20),[System.Drawing.Point]::new(6,8),[System.Drawing.Point]::new(12,27),[System.Drawing.Point]::new(22,31))
    $hornR = [System.Drawing.Point[]]@([System.Drawing.Point]::new(46,20),[System.Drawing.Point]::new(58,8),[System.Drawing.Point]::new(52,27),[System.Drawing.Point]::new(42,31))
    $g.FillPolygon($gold,$hornL);$g.DrawPolygon($outline,$hornL);$g.FillPolygon($gold,$hornR);$g.DrawPolygon($outline,$hornR)
    $body=[System.Drawing.Point[]]@([System.Drawing.Point]::new(15,22),[System.Drawing.Point]::new(24,14),[System.Drawing.Point]::new(40,14),[System.Drawing.Point]::new(49,22),[System.Drawing.Point]::new(47,51),[System.Drawing.Point]::new(37,58),[System.Drawing.Point]::new(27,58),[System.Drawing.Point]::new(17,51))
    $g.FillPolygon($steelDark,$body);$g.DrawPolygon($outline,$body);$g.FillRectangle($steel,19,25,26,16);$g.DrawLine($shine,21,27,42,27)
    $g.FillRectangle($crimson,29,8,6,18);$g.FillRectangle([System.Drawing.Brushes]::Black,22,39,20,5);$g.FillRectangle($gold,29,39,6,16)
}
Save-PixelIcon 'GearGloves.png' {
    param($g)
    $p=[System.Drawing.Point[]]@([System.Drawing.Point]::new(13,27),[System.Drawing.Point]::new(19,14),[System.Drawing.Point]::new(25,24),[System.Drawing.Point]::new(29,10),[System.Drawing.Point]::new(35,23),[System.Drawing.Point]::new(40,12),[System.Drawing.Point]::new(45,28),[System.Drawing.Point]::new(54,25),[System.Drawing.Point]::new(52,42),[System.Drawing.Point]::new(42,55),[System.Drawing.Point]::new(22,54),[System.Drawing.Point]::new(12,43))
    $g.FillPolygon($steelDark,$p);$g.DrawPolygon($outline,$p);$g.FillRectangle($steel,18,31,30,17);$g.DrawLine($shine,20,33,45,33)
    $g.FillRectangle($crimson,19,47,28,9);$g.DrawRectangle($outline,19,47,28,9);$g.FillEllipse($gold,29,48,7,7)
}
Save-PixelIcon 'GearBoots.png' {
    param($g)
    $left=[System.Drawing.Point[]]@([System.Drawing.Point]::new(13,9),[System.Drawing.Point]::new(29,10),[System.Drawing.Point]::new(28,40),[System.Drawing.Point]::new(35,49),[System.Drawing.Point]::new(31,58),[System.Drawing.Point]::new(8,57),[System.Drawing.Point]::new(8,47),[System.Drawing.Point]::new(16,39))
    $right=[System.Drawing.Point[]]@([System.Drawing.Point]::new(37,8),[System.Drawing.Point]::new(51,10),[System.Drawing.Point]::new(49,39),[System.Drawing.Point]::new(57,47),[System.Drawing.Point]::new(55,57),[System.Drawing.Point]::new(34,57),[System.Drawing.Point]::new(31,49),[System.Drawing.Point]::new(39,39))
    $g.FillPolygon($steelDark,$left);$g.DrawPolygon($outline,$left);$g.FillPolygon($steel,$right);$g.DrawPolygon($outline,$right)
    $g.FillRectangle($crimson,12,24,17,6);$g.FillRectangle($crimson,35,23,16,6);$g.DrawLine($shine,40,12,48,13)
}
Save-PixelIcon 'GearRing.png' {
    param($g)
    $g.FillEllipse($gold,10,19,44,39);$g.DrawEllipse($outline,10,19,44,39);$g.FillEllipse([System.Drawing.Brushes]::Black,20,29,24,21)
    $gem=[System.Drawing.Point[]]@([System.Drawing.Point]::new(23,18),[System.Drawing.Point]::new(32,7),[System.Drawing.Point]::new(42,18),[System.Drawing.Point]::new(38,28),[System.Drawing.Point]::new(26,28))
    $g.FillPolygon($cyan,$gem);$g.DrawPolygon($outline,$gem);$g.DrawLine([System.Drawing.Pens]::White,27,17,33,11)
}
Save-PixelIcon 'GearAmulet.png' {
    param($g)
    $chain=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255,215,156,47),5);$g.DrawArc($chain,8,3,48,44,5,170);$g.DrawArc($chain,8,3,48,44,185,170);$chain.Dispose()
    $gem=[System.Drawing.Point[]]@([System.Drawing.Point]::new(32,24),[System.Drawing.Point]::new(48,38),[System.Drawing.Point]::new(39,57),[System.Drawing.Point]::new(25,57),[System.Drawing.Point]::new(16,38))
    $g.FillPolygon($purple,$gem);$g.DrawPolygon($outline,$gem);$g.DrawLine([System.Drawing.Pens]::White,25,36,31,29);$g.DrawLine($goldLight,20,38,31,53)
}
Save-PixelIcon 'SkillArea.png' {
    param($g)
    $g.FillEllipse([System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(140,100,25,12)),7,7,50,50)
    $g.DrawArc([System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255,255,177,49),6),8,8,48,48,25,125)
    $g.DrawArc([System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255,224,54,31),6),8,8,48,48,205,125)
    $g.FillPolygon($gold,[System.Drawing.Point[]]@([System.Drawing.Point]::new(48,8),[System.Drawing.Point]::new(58,13),[System.Drawing.Point]::new(48,20)))
    $g.FillPolygon($crimson,[System.Drawing.Point[]]@([System.Drawing.Point]::new(16,56),[System.Drawing.Point]::new(6,50),[System.Drawing.Point]::new(17,43)))
    $g.FillEllipse([System.Drawing.Brushes]::White,27,27,10,10)
}
Save-PixelIcon 'SkillHeal.png' {
    param($g)
    $g.FillEllipse([System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(180,14,72,87)),8,8,48,48);$g.DrawEllipse($outline,8,8,48,48)
    $g.FillRectangle($cyan,26,15,12,34);$g.FillRectangle($cyan,15,26,34,12)
    $g.FillRectangle([System.Drawing.Brushes]::White,29,18,5,28);$g.FillRectangle([System.Drawing.Brushes]::White,18,29,28,5)
    $g.DrawArc($goldLight,12,12,40,40,195,120)
}

Save-Leather
Save-BagCell

$outline.Dispose();$steel.Dispose();$steelDark.Dispose();$shine.Dispose();$gold.Dispose();$goldLight.Dispose();$crimson.Dispose();$purple.Dispose();$cyan.Dispose()
Get-ChildItem -LiteralPath $output -Filter '*.png' | Sort-Object Name | Select-Object Name,Length
