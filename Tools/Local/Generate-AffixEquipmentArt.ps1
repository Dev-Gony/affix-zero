param([string] $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$output = Join-Path $RepoRoot 'Assets\Art\Interface\Resources\AffixUIVisual\Items'
$uiOutput = Split-Path -Parent $output
New-Item -ItemType Directory -Force -Path $output | Out-Null

function Points([int[]] $xy) {
    $points = [System.Drawing.Point[]]::new($xy.Count / 2)
    for ($i = 0; $i -lt $points.Length; $i++) { $points[$i] = [System.Drawing.Point]::new($xy[$i*2], $xy[$i*2+1]) }
    return ,$points
}
function Poly($g, [System.Drawing.Color]$fill, [int[]]$xy, [int]$stroke=3) {
    $brush=[System.Drawing.SolidBrush]::new($fill); $pen=[System.Drawing.Pen]::new($script:ink,$stroke); $pen.LineJoin='Miter'
    try { $p=Points $xy; $g.FillPolygon($brush,$p); $g.DrawPolygon($pen,$p) } finally { $brush.Dispose(); $pen.Dispose() }
}
function Rect($g,[System.Drawing.Color]$color,[int]$x,[int]$y,[int]$w,[int]$h) { $b=[System.Drawing.SolidBrush]::new($color);try{$g.FillRectangle($b,$x,$y,$w,$h)}finally{$b.Dispose()} }
function Ellipse($g,[System.Drawing.Color]$color,[int]$x,[int]$y,[int]$w,[int]$h) { $b=[System.Drawing.SolidBrush]::new($color);try{$g.FillEllipse($b,$x,$y,$w,$h)}finally{$b.Dispose()} }
function Line($g,[System.Drawing.Color]$color,[int]$width,[int]$x1,[int]$y1,[int]$x2,[int]$y2) { $p=[System.Drawing.Pen]::new($color,$width);$p.StartCap=$p.EndCap='Square';try{$g.DrawLine($p,$x1,$y1,$x2,$y2)}finally{$p.Dispose()} }
function Ring($g,[System.Drawing.Color]$metal,[System.Drawing.Color]$gem,[bool]$faceted) {
    Ellipse $g $metal 10 21 44 36; $p=[System.Drawing.Pen]::new($script:ink,3);try{$g.DrawEllipse($p,10,21,44,36)}finally{$p.Dispose()}; Ellipse $g $script:ink 20 30 24 20
    $shape=if($faceted){@(21,21,32,7,44,21,39,31,25,31)}else{@(23,22,32,10,42,22,38,31,26,31)}; Poly $g $gem $shape; Line $g $script:highlight 2 27 20 33 13
}

$ink=[System.Drawing.Color]::FromArgb(255,12,9,14); $shadow=[System.Drawing.Color]::FromArgb(150,3,2,5)
$steelDark=[System.Drawing.Color]::FromArgb(255,45,54,66); $steel=[System.Drawing.Color]::FromArgb(255,113,129,145); $silver=[System.Drawing.Color]::FromArgb(255,190,205,213)
$highlight=[System.Drawing.Color]::FromArgb(255,241,232,199); $bronze=[System.Drawing.Color]::FromArgb(255,147,84,38); $gold=[System.Drawing.Color]::FromArgb(255,220,164,52)
$leather=[System.Drawing.Color]::FromArgb(255,105,54,36); $leatherLight=[System.Drawing.Color]::FromArgb(255,158,87,50); $crimson=[System.Drawing.Color]::FromArgb(255,151,27,47)
$ember=[System.Drawing.Color]::FromArgb(255,235,70,35); $violet=[System.Drawing.Color]::FromArgb(255,143,72,218); $cyan=[System.Drawing.Color]::FromArgb(255,65,193,218)
$blue=[System.Drawing.Color]::FromArgb(255,66,111,178); $bone=[System.Drawing.Color]::FromArgb(255,216,203,165)

function Draw-Item($g,[string]$id) {
    Ellipse $g $shadow 8 51 48 8
    switch($id) {
        'dagger' { Poly $g $steel @(18,43,38,13,45,8,42,18,26,47);Line $g $highlight 2 27 37 40 15;Line $g $gold 5 17 42 29 50;Line $g $leather 6 20 48 12 57 }
        'longsword' { Poly $g $silver @(19,45,42,8,49,5,46,16,27,49);Line $g $highlight 2 29 39 44 13;Line $g $gold 6 16 43 31 52;Line $g $blue 7 22 50 12 59 }
        'axe' { Line $g $leather 8 23 57 39 19;Poly $g $steel @(27,12,43,8,56,16,49,32,34,28,24,20);Line $g $highlight 2 42 12 51 17;Rect $g $gold 17 43 14 6 }
        'magic_sword' { Line $g $steelDark 8 19 58 38 22;Line $g $gold 3 21 56 39 23;Poly $g $violet @(38,5,51,16,43,30,31,23);Line $g $highlight 2 39 10 46 16;Ellipse $g $cyan 12 50 9 9 }
        'divine_sword' { Poly $g $gold @(18,44,40,7,47,4,45,15,26,49);Line $g $highlight 2 28 39 42 12;Line $g $crimson 6 15 43 31 52;Line $g $gold 8 21 50 11 59;Line $g $highlight 2 49 8 56 3;Line $g $highlight 2 48 13 58 14 }
        'leather_hat' { Poly $g $leather @(11,47,16,25,26,14,45,16,54,29,51,51,39,58,22,56);Rect $g $leatherLight 17 28 34 9;Line $g $gold 3 17 37 50 37;Rect $g $ink 22 42 24 6 }
        'iron_helm' { Poly $g $steel @(10,31,20,15,43,14,54,29,50,54,39,59,22,57,14,50);Line $g $highlight 2 22 19 42 18;Rect $g $ink 19 34 28 7;Rect $g $bronze 29 17 7 38 }
        'mithril_helm' { Poly $g $silver @(11,30,20,13,43,12,54,29,49,55,38,59,22,56,14,49);Poly $g $blue @(19,18,7,8,13,29,22,34);Poly $g $blue @(45,18,57,8,51,29,42,34);Rect $g $ink 20 34 26 6;Poly $g $cyan @(29,11,35,5,39,13,34,48,29,48) }
        'dragon_helm' { Poly $g $crimson @(11,31,20,15,43,14,54,30,49,55,39,59,22,56,14,49);Poly $g $bone @(20,19,7,4,13,26,22,31);Poly $g $bone @(44,19,57,4,51,26,42,31);Rect $g $ink 19 36 28 6;Poly $g $ember @(29,12,35,7,39,18,34,49,29,49) }
        'cloth' { Poly $g $blue @(21,11,43,11,53,25,47,58,17,58,11,25);Poly $g $bone @(21,11,32,23,43,11,40,31,24,31);Line $g $gold 3 18 42 47 42 }
        'leather_armor' { Poly $g $leather @(20,10,44,10,55,26,48,58,16,58,9,26);Line $g $leatherLight 5 18 22 47 49;Line $g $leatherLight 5 46 22 18 49;Ellipse $g $gold 28 31 8 8 }
        'plate_armor' { Poly $g $steel @(20,9,44,9,55,24,48,58,16,58,9,24);Poly $g $silver @(21,14,32,22,43,14,48,28,42,50,22,50,16,28);Line $g $gold 4 32 22 32 53 }
        'dragonscale' { Poly $g $crimson @(20,8,44,8,56,24,48,59,16,59,8,24);foreach($y in @(20,30,40)){foreach($x in @(18,28,38)){Poly $g $ember @($x,$y,($x+7),($y-4),($x+12),$y,($x+6),($y+7)) 2}};Line $g $gold 3 16 52 48 52 }
        'cloth_gloves' { Poly $g $bone @(12,24,19,13,24,25,30,11,35,25,41,14,47,28,55,25,52,47,42,58,21,56,11,43);Line $g $blue 3 17 39 49 43;Line $g $blue 3 18 47 45 52 }
        'leather_gloves' { Poly $g $leather @(11,25,18,12,24,25,30,10,35,25,41,13,47,28,55,25,52,47,42,58,21,56,10,43);Rect $g $leatherLight 17 34 32 15;Ellipse $g $gold 29 38 8 8 }
        'battle_gloves' { Poly $g $steel @(11,25,18,12,24,25,30,10,35,25,41,13,47,28,55,25,52,47,42,58,21,56,10,43);Rect $g $steelDark 16 34 34 16;Line $g $highlight 2 18 36 47 36;Rect $g $crimson 18 49 30 8 }
        'dragon_gloves' { Poly $g $crimson @(10,26,18,10,24,26,30,8,35,26,42,11,48,28,57,23,52,48,42,59,20,56,9,43);Poly $g $bone @(14,27,18,5,23,26);Poly $g $bone @(28,27,31,3,36,26);Poly $g $bone @(43,29,46,8,50,29);Ellipse $g $ember 27 38 11 11 }
        'sandals' { Poly $g $leather @(13,13,28,13,27,41,35,49,31,58,8,58,7,48,15,39);Poly $g $leather @(38,12,51,13,49,40,57,48,55,58,34,58,31,49,39,39);Line $g $gold 3 12 29 28 35;Line $g $gold 3 36 28 51 34 }
        'leather_boots' { Poly $g $leather @(13,8,29,10,27,40,35,49,31,59,8,58,7,48,15,39);Poly $g $leather @(37,8,52,10,49,40,57,48,55,58,34,58,31,49,39,39);Rect $g $bronze 11 26 17 6;Rect $g $bronze 35 25 16 6 }
        'swift_boots' { Poly $g $steel @(13,8,29,10,27,40,35,49,31,59,8,58,7,48,15,39);Poly $g $silver @(37,8,52,10,49,40,57,48,55,58,34,58,31,49,39,39);Poly $g $cyan @(13,22,2,16,9,29,1,33,16,36);Poly $g $cyan @(51,21,62,15,55,29,63,33,48,36) }
        'gale_boots' { Poly $g $blue @(13,8,29,10,27,40,35,49,31,59,8,58,7,48,15,39);Poly $g $cyan @(37,8,52,10,49,40,57,48,55,58,34,58,31,49,39,39);Line $g $highlight 3 3 26 16 26;Line $g $highlight 3 0 34 14 34;Line $g $highlight 3 49 24 62 20 }
        'copper_ring' { Ring $g $bronze $ember $false }
        'silver_ring' { Ring $g $silver $blue $false }
        'gold_ring' { Ring $g $gold $crimson $false }
        'diamond_ring' { Ring $g $silver $cyan $true }
        'bone_necklace' { Line $g $bone 4 12 12 21 32;Line $g $bone 4 52 12 43 32;Line $g $bone 4 21 32 43 32;Poly $g $bone @(23,30,32,21,41,30,38,51,32,58,26,51);Ellipse $g $ink 29 32 6 6 }
        'crystal_necklace' { Line $g $silver 4 11 9 22 31;Line $g $silver 4 53 9 42 31;Poly $g $cyan @(22,30,32,18,43,31,38,56,26,56);Line $g $highlight 2 28 31 33 24 }
        'ruby_necklace' { Line $g $gold 5 10 9 23 32;Line $g $gold 5 54 9 41 32;Poly $g $crimson @(20,31,32,18,45,31,39,54,32,59,25,54);Line $g $highlight 2 27 31 33 24 }
        'dragon_tear' { Line $g $gold 5 10 8 22 29;Line $g $gold 5 54 8 42 29;Poly $g $violet @(32,16,45,35,39,54,32,60,24,53,19,35);Poly $g $cyan @(31,21,39,36,34,49,27,38) 2;Line $g $highlight 2 28 31 32 23 }
        default { throw "Unknown item icon id: $id" }
    }
}

function Save-ItemIcon([string]$id) {
    $small=[System.Drawing.Bitmap]::new(64,64,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($small)
    try{$g.SmoothingMode='None';$g.PixelOffsetMode='Half';$g.Clear([System.Drawing.Color]::Transparent);Draw-Item $g $id}finally{$g.Dispose()}
    $large=[System.Drawing.Bitmap]::new(128,128,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$scale=[System.Drawing.Graphics]::FromImage($large)
    try{$scale.InterpolationMode='NearestNeighbor';$scale.PixelOffsetMode='Half';$scale.DrawImage($small,[System.Drawing.Rectangle]::new(0,0,128,128),0,0,64,64,[System.Drawing.GraphicsUnit]::Pixel)}finally{$scale.Dispose();$small.Dispose()}
    try{$large.Save((Join-Path $output ($id+'.png')),[System.Drawing.Imaging.ImageFormat]::Png)}finally{$large.Dispose()}
}

function Save-IllustratedItemIcon([string]$sourceName,[string]$id) {
    $sourcePath=Join-Path $RepoRoot ('docs\art-source\illustrated-equipment-v2\'+$sourceName)
    if(-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)){throw "Missing illustrated source: $sourcePath"}
    $source=[System.Drawing.Bitmap]::new($sourcePath)
    $icon=[System.Drawing.Bitmap]::new(128,128,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g=[System.Drawing.Graphics]::FromImage($icon)
    try{
        $g.Clear([System.Drawing.Color]::Transparent)
        $g.CompositingMode=[System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $g.CompositingQuality=[System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        $g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $g.DrawImage($source,[System.Drawing.Rectangle]::new(0,0,128,128),0,0,$source.Width,$source.Height,[System.Drawing.GraphicsUnit]::Pixel)
    }finally{$g.Dispose();$source.Dispose()}
    try{$icon.Save((Join-Path $output ($id+'.png')),[System.Drawing.Imaging.ImageFormat]::Png)}finally{$icon.Dispose()}
}

function Save-WeatheredFrame([string]$name,[int]$width,[int]$height,[bool]$leatherCenter) {
    $bitmap=[System.Drawing.Bitmap]::new($width,$height,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$random=[System.Random]::new($width*7919+$height)
    for($y=0;$y -lt $height;$y++){for($x=0;$x -lt $width;$x++){
        $edge=[Math]::Min([Math]::Min($x,$width-1-$x),[Math]::Min($y,$height-1-$y));$grain=$random.Next(-5,6)
        if($edge -lt 12){$v=[Math]::Max(22,[Math]::Min(86,55+$random.Next(-13,14)));$c=[System.Drawing.Color]::FromArgb(255,$v,[Math]::Max(18,$v-13),[Math]::Max(17,$v-18))}
        elseif($leatherCenter){$c=[System.Drawing.Color]::FromArgb(252,40+$grain,12+[int]($grain/2),20+[int]($grain/2))}
        else{$c=[System.Drawing.Color]::FromArgb(252,10+$grain,11+$grain,13+$grain)}
        $bitmap.SetPixel($x,$y,$c)
    }}
    $g=[System.Drawing.Graphics]::FromImage($bitmap)
    try{
        foreach($spec in @(@(1,1,3,19,13,12),@(5,5,2,151,108,61),@(11,11,3,33,27,28))){$p=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255,$spec[3],$spec[4],$spec[5]),$spec[2]);try{$g.DrawRectangle($p,$spec[0],$spec[1],$width-($spec[0]*2)-1,$height-($spec[1]*2)-1)}finally{$p.Dispose()}}
        foreach($fx in @(0,1)){foreach($fy in @(0,1)){$x0=if($fx){$width-5}else{5};$sx=if($fx){-1}else{1};$y0=if($fy){$height-5}else{5};$sy=if($fy){-1}else{1};$plate=@($x0,$y0,($x0+$sx*30),$y0,($x0+$sx*30),($y0+$sy*8),($x0+$sx*8),($y0+$sy*30),$x0,($y0+$sy*30));Poly $g $crimson $plate 2;Ellipse $g $gold ($x0+$sx*12-3) ($y0+$sy*12-3) 7 7}}
        for($i=0;$i -lt 18;$i++){$x=$random.Next(15,$width-15);$y=$random.Next(15,$height-15);if($random.Next(2)-eq 0){Line $g ([System.Drawing.Color]::FromArgb(90,205,153,78)) 1 $x $y ([Math]::Min($width-15,$x+$random.Next(4,14))) $y}}
    }finally{$g.Dispose()}
    try{$bitmap.Save((Join-Path $uiOutput $name),[System.Drawing.Imaging.ImageFormat]::Png)}finally{$bitmap.Dispose()}
}

function Save-BagCellWide {
    $bitmap=[System.Drawing.Bitmap]::new(70,45,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$g=[System.Drawing.Graphics]::FromImage($bitmap)
    try{$g.Clear([System.Drawing.Color]::FromArgb(255,7,9,11));Rect $g ([System.Drawing.Color]::FromArgb(255,14,18,19)) 4 4 62 37;$p=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255,74,63,48),1);try{$g.DrawRectangle($p,1,1,67,42)}finally{$p.Dispose()};Line $g ([System.Drawing.Color]::FromArgb(180,149,104,52)) 1 5 5 64 5;Poly $g ([System.Drawing.Color]::FromArgb(190,102,21,35)) @(1,1,12,1,1,12) 1;Poly $g ([System.Drawing.Color]::FromArgb(190,102,21,35)) @(69,44,58,44,69,33) 1}finally{$g.Dispose()}
    try{$bitmap.Save((Join-Path $uiOutput 'BagCellWide.png'),[System.Drawing.Imaging.ImageFormat]::Png)}finally{$bitmap.Dispose()}
}

$ids=@('dagger','longsword','axe','magic_sword','divine_sword','leather_hat','iron_helm','mithril_helm','dragon_helm','cloth','leather_armor','plate_armor','dragonscale','cloth_gloves','leather_gloves','battle_gloves','dragon_gloves','sandals','leather_boots','swift_boots','gale_boots','copper_ring','silver_ring','gold_ring','diamond_ring','bone_necklace','crystal_necklace','ruby_necklace','dragon_tear')
$illustrated=@{longsword='sword.png';plate_armor='armor.png';battle_gloves='gloves.png'}
foreach($id in $ids){if($illustrated.ContainsKey($id)){Save-IllustratedItemIcon $illustrated[$id] $id}else{Save-ItemIcon $id}}
Save-WeatheredFrame 'FramePanelTall.png' 500 712 $true
Save-WeatheredFrame 'FrameTooltip.png' 300 424 $false
Save-BagCellWide
$created=Get-ChildItem -LiteralPath $output -Filter '*.png' | Sort-Object Name
if($created.Count -ne 29){throw "Expected 29 item icons, found $($created.Count)."}
if(($created|ForEach-Object Length|Measure-Object -Minimum).Minimum -lt 700){throw 'One or more item icons appear empty.'}
$reportDir=Join-Path $RepoRoot 'Build\Reports\ui-icon-polish';New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
$sheet=[System.Drawing.Bitmap]::new(900,1020,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb);$sg=[System.Drawing.Graphics]::FromImage($sheet)
try{
    $sg.Clear([System.Drawing.Color]::FromArgb(255,12,10,13));$titleFont=[System.Drawing.Font]::new('Segoe UI',18,[System.Drawing.FontStyle]::Bold);$labelFont=[System.Drawing.Font]::new('Consolas',10);$creamBrush=[System.Drawing.SolidBrush]::new($highlight);$goldBrush=[System.Drawing.SolidBrush]::new($gold);$cellBrush=[System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255,24,20,23));$edgePen=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255,105,68,45),2)
    try{
        $sg.DrawString('AFFIX: ZERO — 29 DISTINCT BASE ITEM ICONS',$titleFont,$goldBrush,22,14)
        for($i=0;$i -lt $ids.Count;$i++){$col=$i%5;$row=[Math]::Floor($i/5);$x=20+$col*176;$y=58+$row*158;$sg.FillRectangle($cellBrush,$x,$y,156,142);$sg.DrawRectangle($edgePen,$x,$y,156,142);$icon=[System.Drawing.Image]::FromFile((Join-Path $output ($ids[$i]+'.png')));try{$sg.DrawImage($icon,$x+14,$y+6,112,112)}finally{$icon.Dispose()};$sg.DrawString($ids[$i],$labelFont,$creamBrush,$x+8,$y+120)}
    }finally{$titleFont.Dispose();$labelFont.Dispose();$creamBrush.Dispose();$goldBrush.Dispose();$cellBrush.Dispose();$edgePen.Dispose()}
}finally{$sg.Dispose()}
try{$sheet.Save((Join-Path $reportDir 'AFFIX-ZERO-29-base-icons-contact-sheet.png'),[System.Drawing.Imaging.ImageFormat]::Png)}finally{$sheet.Dispose()}
$created | Select-Object Name,Length
