"""Check the new branch, not user folders or Git history. No filesystem mutations."""
import json
import subprocess
from pathlib import PurePosixPath

BASE = "f58a919b442ea8a8956619b5e62c4c7e9340d355"

def tree(ref):
    data = subprocess.check_output(["git", "ls-tree", "-r", "--name-only", ref], text=True)
    return set(data.splitlines())

current, previous = tree("HEAD"), tree(BASE)
legacy_extensions = {".gd", ".tscn", ".tres", ".godot", ".gdignore"}
old_sources = {p for p in previous if PurePosixPath(p).suffix in legacy_extensions or p.endswith(".gdignore")}
forbidden_roots = ("experiments/", "scripts/", "resources/", "scenes/", "art_reset/", "design-system/")
problems = [p for p in current if p.startswith(forbidden_roots) or PurePosixPath(p).suffix in legacy_extensions or p.endswith(".gdignore")]
for path in current:
    if path.startswith("Assets/") and not path.endswith(".meta") and path + ".meta" not in current:
        problems.append("missing meta: " + path)
print(json.dumps({"base": BASE, "old_engine_files_removed": len(old_sources - current),
                  "old_engine_files_remaining": len(old_sources & current), "problems": problems,
                  "scope": "fresh tree and metadata only; Unity import, animation and gameplay NOT_RUN"}))
if problems or old_sources & current:
    raise SystemExit(1)
print("RESTART_AUDIT_PASSED")
