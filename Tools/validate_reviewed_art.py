"""Read-only checks of reviewed source files, clip pixels and sprite bounds.

Run from any directory: python Tools/validate_reviewed_art.py
Requires Pillow. This does not launch Unity or establish visual approval.
"""
from __future__ import annotations

import hashlib
import json
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]


def read_record(name: str) -> dict:
    return json.loads((ROOT / "docs/assets" / name).read_text(encoding="utf-8"))


def source_path(relative: str) -> Path:
    path = (ROOT / relative).resolve()
    if not path.is_relative_to(ROOT / "Assets/Art/NinjaAdventure"):
        raise ValueError(f"Source path outside reviewed art folder: {relative}")
    return path


def check(condition: bool, message: str) -> None:
    if not condition:
        raise ValueError(message)


def validate() -> dict:
    manifest = read_record("ninja-source-manifest.json")
    animation = read_record("ninja-animation.json")
    environment = read_record("ninja-environment.json")
    images = {}
    file_hashes = set()
    reviewed_paths = set()
    for entry in manifest["files"]:
        path = source_path(entry["path"])
        data = path.read_bytes()
        digest = hashlib.sha256(data).hexdigest()
        check(len(data) == entry["bytes"], f"File size changed: {entry['path']}")
        check(digest == entry["sha256"], f"Source SHA mismatch: {entry['path']}")
        check(digest not in file_hashes, f"Duplicate source file: {entry['path']}")
        check(entry["path"] not in reviewed_paths, f"Duplicate manifest path: {entry['path']}")
        file_hashes.add(digest)
        reviewed_paths.add(entry["path"])
        if path.suffix.lower() == ".png":
            with Image.open(path) as image:
                images[entry["path"]] = image.convert("RGBA")

    check({c["name"] for c in animation["clips"]} == {"Idle", "Walk", "Attack", "Hit", "Dead"},
          "Five required animation clips must be present")
    frame_count = 0
    clips = {}
    for clip in animation["clips"]:
        image = images[clip["path"]]
        expected = (32 if clip["name"] == "Dead" else 128, len(clip["frames"]) * 32)
        check(image.size == expected, f"Unexpected sheet dimensions: {clip['name']}")
        digests = set()
        check([f["index"] for f in clip["frames"]] == list(range(len(clip["frames"]))),
              f"Invalid frame order: {clip['name']}")
        for frame in clip["frames"]:
            x, y, width, height = frame["rectTopLeft"]
            check(0 <= x < x + width <= image.width and 0 <= y < y + height <= image.height,
                  f"Out of bounds frame: {clip['name']} {frame['index']}")
            crop = image.crop((x, y, x + width, y + height))
            digest = hashlib.sha256(crop.tobytes()).hexdigest()
            check(digest == frame["pixelSha256"], f"Frame pixels changed: {clip['name']} {frame['index']}")
            bounds = crop.getchannel("A").getbbox()
            check(bounds is not None, f"Empty frame: {clip['name']} {frame['index']}")
            check(list(bounds) == frame["alphaBounds"], f"Alpha bounds changed: {clip['name']}")
            digests.add(digest)
            frame_count += 1
        check(len(digests) == clip["uniquePixelFrames"] == len(clip["frames"]),
              f"Repeated pixels masquerading as motion: {clip['name']}")
        clips[clip["name"]] = len(digests)
        for col in range(image.width // 32):
            for row in range(image.height // 32):
                check(image.crop((col * 32, row * 32, col * 32 + 32, row * 32 + 32)).getchannel("A").getbbox() is not None,
                      f"Empty direction frame: {clip['name']} col {col} row {row}")

    for weapon in ("Katana", "Axe"):
        image = images[f"Assets/Art/NinjaAdventure/Weapons/{weapon}.png"]
        check(image.size == (256, 256), f"Weapon sheet dimensions changed: {weapon}")
        for col in range(4):
            for row in range(4):
                check(image.crop((col * 64, row * 64, col * 64 + 64, row * 64 + 64)).getchannel("A").getbbox() is not None,
                      f"Empty weapon frame: {weapon} col {col} row {row}")
    check(0 <= animation["attackImpactFrameIndex"] < clips["Attack"], "Impact index outside attack")

    names = set()
    for sprite in environment["sprites"]:
        check(sprite["name"] not in names, f"Duplicate environment sprite name: {sprite['name']}")
        names.add(sprite["name"])
        image = images[sprite["path"]]
        x, y, width, height = (sprite[k] for k in ("x", "y", "width", "height"))
        check(0 <= x < x + width <= image.width and 0 <= y < y + height <= image.height,
              f"Environment rect outside source: {sprite['name']}")
        check(image.crop((x, y, x + width, y + height)).getchannel("A").getbbox() is not None,
              f"Empty environment sprite: {sprite['name']}")
    return {"status": "PASS", "sourceFiles": len(reviewed_paths), "sourcePngs": len(images),
            "auditedRightFacingFrames": frame_count, "uniqueFramesByClip": clips,
            "environmentSprites": len(names), "unityExecuted": False, "visualApproval": "NOT_RUN"}


if __name__ == "__main__":
    try:
        print(json.dumps(validate(), ensure_ascii=False, indent=2))
    except (OSError, ValueError, KeyError) as error:
        raise SystemExit(f"FAIL: {error}") from error
