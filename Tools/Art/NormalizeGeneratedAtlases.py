from pathlib import Path
from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "Assets/Art/OriginalTemple/Resources/AffixOriginal"


def normalize_grid(name: str, columns: int, rows: int, cell_width: int, cell_height: int) -> None:
    path = ART / name
    source = Image.open(path).convert("RGBA")
    output = Image.new("RGBA", (columns * cell_width, rows * cell_height), (0, 0, 0, 0))
    for row in range(rows):
        for column in range(columns):
            left = round(column * source.width / columns)
            right = round((column + 1) * source.width / columns)
            top = round(row * source.height / rows)
            bottom = round((row + 1) * source.height / rows)
            frame = source.crop((left, top, right, bottom)).resize(
                (cell_width, cell_height), Image.Resampling.NEAREST
            )
            output.alpha_composite(frame, (column * cell_width, row * cell_height))
    output.save(path, optimize=True)


def normalize_single(name: str, width: int, height: int) -> None:
    path = ART / name
    source = Image.open(path).convert("RGBA")
    source.resize((width, height), Image.Resampling.NEAREST).save(path, optimize=True)


normalize_grid("HeroAtlas-v1.png", 8, 8, 128, 128)
normalize_grid("EnemyAtlas-v1.png", 8, 8, 128, 128)
normalize_grid("ImpactFxAtlas-v1.png", 4, 2, 256, 512)
normalize_single("TempleObstacle-v2.png", 1024, 768)
