"""Read-only tracked-tree audit. Works with shallow clones; strict history is opt-in."""
import argparse
import json
import subprocess
from pathlib import PurePosixPath

BASE = "f58a919b442ea8a8956619b5e62c4c7e9340d355"

def tree(ref):
    return set(subprocess.check_output(["git", "ls-tree", "-r", "--name-only", ref], text=True).splitlines())

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--require-history", action="store_true")
    args = parser.parse_args()
    current = tree("HEAD")
    extensions = {".gd", ".tscn", ".tres", ".godot", ".gdignore"}
    roots = ("experiments/", "scripts/", "resources/", "scenes/", "art_reset/", "design-system/")
    problems = [p for p in current if p.startswith(roots) or PurePosixPath(p).suffix in extensions or p.endswith(".gdignore")]
    for p in current:
        if p.startswith("Assets/") and not p.endswith(".meta") and p + ".meta" not in current:
            problems.append("missing meta: " + p)
    available = subprocess.run(["git", "cat-file", "-e", BASE + "^{commit}"], capture_output=True).returncode == 0
    removed = None
    remaining = None
    if available:
        previous = tree(BASE)
        old = {p for p in previous if PurePosixPath(p).suffix in extensions or p.endswith(".gdignore")}
        removed, remaining = len(old - current), len(old & current)
        if remaining:
            problems.append("old engine files remain")
    elif args.require_history:
        problems.append("history comparison required but baseline commit is unavailable")
    print(json.dumps({"base": BASE, "history_available": available,
                      "old_engine_files_removed": removed, "old_engine_files_remaining": remaining,
                      "problems": problems, "scope": "tracked tree only; not Unity gameplay validation"}))
    if problems:
        raise SystemExit(1)
    print("RESTART_AUDIT_PASSED")

if __name__ == "__main__":
    main()
