from pathlib import Path
from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "Assets/Art/OriginalTemple/Resources/AffixOriginal"
OUTPUT = ROOT / "Build/Deliverables/TempleCombatAnimationPreview.gif"
DIRECTIONS = ("Down", "Left", "Right", "Up")
CLIPS = (
    ("Idle", 0, 0, 2),
    ("Walk", 0, 2, 4),
    ("Attack", 1, 0, 5),
    ("Hit", 0, 6, 2),
    ("Death", 1, 5, 3),
)


def cell(sheet: Image.Image, row: int, column: int) -> Image.Image:
    return sheet.crop((column * 128, row * 128, (column + 1) * 128, (row + 1) * 128))


hero = Image.open(ART / "HeroAtlas-v1.png").convert("RGBA")
enemy = Image.open(ART / "EnemyAtlas-v1.png").convert("RGBA")
frames = []
durations = []
for direction_index, direction in enumerate(DIRECTIONS):
    for clip, row_offset, start, count in CLIPS:
        row = direction_index * 2 + row_offset
        sequence = list(range(start, start + count))
        if clip in ("Idle", "Walk"):
            sequence *= 2
        for index, column in enumerate(sequence):
            canvas = Image.new("RGBA", (640, 320), (18, 21, 24, 255))
            draw = ImageDraw.Draw(canvas)
            draw.text((20, 14), f"AFFIX: ZERO original combat set  |  {direction} / {clip}", fill=(232, 224, 196, 255))
            draw.text((106, 278), "Temple Vanguard", fill=(202, 186, 132, 255))
            draw.text((414, 278), "Obsidian Raider", fill=(202, 186, 132, 255))
            canvas.alpha_composite(cell(hero, row, column).resize((256, 256), Image.Resampling.NEAREST), (32, 34))
            canvas.alpha_composite(cell(enemy, row, column).resize((256, 256), Image.Resampling.NEAREST), (352, 34))
            frames.append(canvas.convert("P", palette=Image.Palette.ADAPTIVE, colors=256))
            durations.append(420 if index == len(sequence) - 1 else 125)

OUTPUT.parent.mkdir(parents=True, exist_ok=True)
frames[0].save(OUTPUT, save_all=True, append_images=frames[1:], duration=durations, loop=0, disposal=2, optimize=False)
print(OUTPUT)
