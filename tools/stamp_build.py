#!/usr/bin/env python3
"""Stamp exports with the exact checked-out source commit. No network access."""
from __future__ import annotations
import argparse
import datetime as dt
import json
import os
from pathlib import Path
import re
import subprocess


def git(root: Path, *args: str) -> str:
    return subprocess.check_output(["git", "-C", str(root), *args], text=True, stderr=subprocess.STDOUT).strip()


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--branch", default=os.environ.get("GITHUB_HEAD_REF", ""))
    args = parser.parse_args()
    root = args.root.resolve()
    target = root / "resources/build/build_info.json"
    info = json.loads(target.read_text(encoding="utf-8"))
    commit = git(root, "rev-parse", "HEAD")
    if not re.fullmatch(r"[0-9a-f]{40}", commit):
        raise SystemExit("Invalid source commit; refusing to stamp misleading metadata")
    # The metadata itself is generated and must not make an otherwise clean export dirty.
    changed = git(root, "diff", "--name-only", "HEAD", "--", ".", ":(exclude)resources/build/build_info.json")
    info.update(source_commit=commit, branch=args.branch or git(root, "branch", "--show-current") or "detached",
                built_at_utc=dt.datetime.now(dt.timezone.utc).isoformat(timespec="seconds"), dirty=bool(changed))
    temporary = target.with_suffix(".tmp")
    temporary.write_text(json.dumps(info, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    os.replace(temporary, target)
    print(json.dumps(info, ensure_ascii=False, sort_keys=True))


if __name__ == "__main__":
    main()
