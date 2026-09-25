# AFFIX: ZERO — implementation rules

## Authority and current milestone

This project is developed through ChatGPT + GitHub PRs. Codex CLI is not required.
Read `docs/uiux-v1/README.md`, `docs/uiux-v1/spec_lock.json`, and `docs/uiux-v1/IMPLEMENTATION_ORDER.md` before editing.
The user-approved **AFFIX_ZERO_UIUX_MASTER_v1.0.md** and its contracts/QA package are the design authority. Its exact package and document hashes are pinned in spec_lock.json. The package is a detached handoff, not an implemented game or a claim that all its files already exist in this repository. If implementing a later milestone without that handoff, obtain it before guessing missing details.

Precedence: explicit user change recorded in a decision record > approved v1.0 master/contracts > these engineering rules > stage-specific design pages. Files under `docs/uiux-v1/archive/` are historical evidence, not instructions.

Current milestone: **PR-A / baseline, backup, build identification**. Runtime remains 640x400, default window 1280x800 in PR-A. Target UI1280x720/world640x360 is implemented only in PR-C. Do not partially rescale the world, change movement ranges, or insert future UI while doing PR-A.

## Non-negotiable invariants

- Godot 4.3-compatible GDScript. Keep static typing where supported.
- Preserve 7 equipment slots, 60 inventory slots, 5 rarity IDs, 6 classes and x1/x2/x5.
- Rebirth preserves gold, inventory, equipped/locked items, class mastery, permanent progress and settings.
- View filtering, future pickup policy, and selling existing items are separate actions. A filter change must never sell possessions.
- Menus must eventually pause ALL simulation through a single coordinator; do not independently force GameState.RUNNING from every close handler.
- A reward's ownership must not depend on the lifetime of a visual effect.
- Save failure must not be presented as success. PR-A protects the original input with an immutable raw backup; full atomic saving and controlled exit belong to PR-B.
- Never downgrade future save versions or silently discard legacy/unknown data.
- No paid currencies, ads, shops, synthesis, offline rewards, or other scope additions.
- Use existing approved art until a replacement has separate visual approval. Do not replace sprites with geometric stand-ins and call it an art upgrade.

## Implementation discipline

- Use custom Resource/.tres data for balance. Do not duplicate formulas in UI strings.
- Keep GameManager as a compatibility facade while moving responsibilities incrementally. Do not create an autoload for every helper.
- Keep UI commands out of rendering callbacks. Use stable item IDs and signals.
- Inspect the actual branch and HEAD; code search on the default branch is not evidence about a feature branch.
- Stage changes in a dedicated PR branched from the verified development baseline. Do not mix PR-A through PR-H in a single change.
- Do not merge into main without user play approval.

## Verification and reporting

- Existing `tests/smoke_test.gd` assertions must not be weakened to make a PR pass.
- Launch tests with `-- --affix-test-mode` and an isolated XDG_DATA_HOME. This prevents autoload startup from loading/writing a real player save before the test scene can disable persistence.
- PR-A adds `tests/pr_a_safety.tscn` and `tests/pr_a_capture.tscn`; CI publishes logs and actual rendered PNGs.
- Report automation, actual render inspection and Windows play approval separately. A fixture is not a player's recovered data; a headless pass is not an art approval.
- For local sync: close the game AND editor first. Preserve local changes with an explicitly named stash/commit. Use fast-forward-only pulls. Never default to reset --hard or git clean.
- Run the project with F5, not an arbitrary current scene with F6.

## Handoff format

State: changed items (max 3), tests actually run, unverified items, exact branch/commit, safe local commands, user checks (max 3), rollback. Never claim work continues in the background after the response ends.
