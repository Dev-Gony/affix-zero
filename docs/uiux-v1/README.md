# UI/UX rebuild v1.0 — implementation entry point

## Source of truth

The user approved AFFIX_ZERO_UIUX_REBUILD_v1.0.zip (master, contracts, mockups and 94 unexecuted acceptance cases). Exact hashes are recorded in spec_lock.json. Keep that original handoff alongside the repository when working in ChatGPT. This directory contains the engineering adoption and execution record; it is NOT a rewritten substitute for the complete 36-page master. Missing later-stage requirements must be read from the handoff rather than invented.

Current branch baseline: cdcf080d2916c053e72e5c3c4ecc57a30213d0f2. The pre-rebuild branch backup is backup/pre-uiux-v1-cdcf080. Old AGENTS and design pages are preserved byte-for-byte under archive/ and have no normative precedence.

## PR-A scope

1. Freeze specification/source identifiers and replace contradictory web/fixed-player guidance.
2. Preserve exact pre-load save bytes in a dated, SHA-256-addressed backup before v1/v2 migrations or writes. Never overwrite an existing different backup. Block writing when backup/read/format validation fails. A malformed save is preserved, not silently replaced with defaults.
3. Show a build ID in the game, expose read-only F8 diagnostics, and add deterministic synthetic fixture + CI evidence.

No inventory, skill, rebirth, loot rates, map artwork, resolution or menu-layout redesign is included. Existing known defects remain explicitly pending PR-B onward. Runtime save structure remains v2; this is not the v3 migration or atomic save implementation.

## User-visible checks

- Bottom left shows `A.1` and the running commit. F8 shows branch/commit/dirty state, save-backup status and actual data paths.
- On an existing save, gold, equipped items, inventory, class and progress load normally; the original bytes are additionally backed up under user://backups/uiux-v1/.
- A second start with identical input reuses its verified backup. Backups are never loaded automatically or deleted by this patch.

F8 is a read-only diagnostic overlay, not a pause menu. ESC remains the existing pause control until PR-B consolidates routing.

## Tests and limits

`tests/pr_a_safety.tscn` checks backup integrity, repeatability, error paths, write blocking and unchanged profile data using synthetic files only. `tests/pr_a_capture.tscn` renders the known fixture for visual inspection. Existing gameplay smoke and exported runtime checks remain required. All CI runs use isolated user directories and --affix-test-mode. Full v1.0 QA remains NOT_RUN unless a named case is actually executed.

Linux/Godot4.3 CI success does not establish Windows play approval, restore a lost past save, or approve new art. Never mark the milestone accepted until the user confirms the local build.

## Safe local update

Close the game and editor. Check git status first. Preserve modifications using a named stash or commit; do not delete them. Fetch the exact test branch, switch to it, then verify git log -1. If Git aborts, stop and fix sync before playing. Run project.godot with F5. Do not automatically pop the stash.

Rollback is a branch switch back to dev/gameplay-v2 after preserving any new editor changes. This patch does not change the gameplay save schema, so its backup files need not be restored merely to switch code versions. A player-data restore must be a separate explicit action with the game closed.
