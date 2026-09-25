#!/usr/bin/env python3
"""Verify removal, no renamed old images, and byte preservation of rules."""
from __future__ import annotations
import argparse
import json
from pathlib import Path
import subprocess

BASE = '46ad2db3538734d3aed48a119856f32862f7c164'
MEDIA = {'.png', '.jpg', '.jpeg', '.svg', '.webp', '.gif', '.bmp', '.tga', '.ico', '.ase', '.aseprite', '.psd', '.xcf'}
PRESERVED = ('scripts/', 'resources/', 'scenes/', 'tests/', 'experiments/e0-godot/scripts/', 'experiments/e0-godot/scenes/', 'experiments/e0-godot/tests/', 'experiments/e0-godot/vslice/', 'experiments/e0-godot/perf/')
GUARDS = ('scripts', 'resources', 'scenes', 'tests', 'experiments', 'experiments/e0-godot/scripts', 'experiments/e0-godot/scenes', 'experiments/e0-godot/tests', 'experiments/e0-godot/vslice', 'experiments/e0-godot/perf')


def tree(ref: str) -> dict[str, str]:
    raw = subprocess.check_output(['git', 'ls-tree', '-r', '-z', '--full-tree', ref])
    result: dict[str, str] = {}
    for record in raw.split(b'\0'):
        if not record:
            continue
        meta, name = record.split(b'\t', 1)
        _, kind, sha = meta.split()
        if kind == b'blob':
            result[name.decode('utf-8')] = sha.decode('ascii')
    return result


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument('--base', default=BASE)
    parser.add_argument('--output', default='build/art-reset/audit.json')
    args = parser.parse_args()
    before, after = tree(args.base), tree('HEAD')
    old_images = {p: sha for p, sha in before.items() if Path(p).suffix.lower() in MEDIA}
    if not old_images:
        raise RuntimeError('Baseline contains no images; wrong baseline or incomplete checkout.')
    old_hashes = set(old_images.values())
    remaining = [p for p in old_images if p in after]
    renamed = [p for p, sha in after.items() if sha in old_hashes]
    current_images = [p for p in after if Path(p).suffix.lower() in MEDIA]
    rules = {p: sha for p, sha in before.items() if p.startswith(PRESERVED) and Path(p).suffix in {'.gd', '.tres', '.tscn', '.json'}}
    modified_rules = [p for p, sha in rules.items() if after.get(p) != sha]
    missing_guards = [p for p in GUARDS if f'{p}/.gdignore' not in after]
    problems: list[str] = []
    if remaining: problems.append('Old image paths remain: ' + ', '.join(remaining))
    if renamed: problems.append('Old image blobs remain: ' + ', '.join(renamed))
    if current_images: problems.append('Reset requires an empty image baseline: ' + ', '.join(current_images))
    if modified_rules: problems.append('Existing rule/source files changed: ' + ', '.join(modified_rules))
    if missing_guards: problems.append('Missing old-render import guards: ' + ', '.join(missing_guards))
    for project in ('project.godot', 'experiments/e0-godot/project.godot'):
        text = Path(project).read_text(encoding='utf-8')
        if 'run/main_scene="res://art_reset/reset_notice.tscn"' not in text or '[autoload]' in text:
            problems.append('Unsafe entry: ' + project)
    for scene in ('art_reset/reset_notice.tscn', 'experiments/e0-godot/art_reset/reset_notice.tscn'):
        if '[ext_resource' in Path(scene).read_text(encoding='utf-8'):
            problems.append('Reset entry must be self-contained: ' + scene)
    report = {
        'schema': 'affix-art-reset-audit-v1',
        'head': subprocess.check_output(['git', 'rev-parse', 'HEAD'], text=True).strip(),
        'base': args.base,
        'deleted_image_count': len(old_images) - len(remaining),
        'current_image_count': len(current_images),
        'old_blob_occurrences': len(renamed),
        'preserved_rule_file_count': len(rules) - len(modified_rules),
        'removed_images': old_images,
        'problems': problems,
        'scope': 'working tree media removal and code preservation; not gameplay/art acceptance',
        'history_rewritten': False,
    }
    output = Path(args.output)
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    print(json.dumps({k: v for k, v in report.items() if k != 'removed_images'}, ensure_ascii=False))
    print('ART_RESET_AUDIT ' + ('FAILED' if problems else 'PASSED'))
    return int(bool(problems))


if __name__ == '__main__':
    raise SystemExit(main())
