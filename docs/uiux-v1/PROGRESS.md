# Rebuild progress

## PR-A: baseline, raw-save backup and build identity

Implementation prepared on a dedicated branch from `cdcf080d2916c053e72e5c3c4ecc57a30213d0f2`.

- Completed in code: archived conflicting guidance, source-spec hash lock, immutable raw-save backup, backup/read/version failure write guard, source build badge and diagnostic paths, synthetic legacy fixture, pre-autoload test-mode isolation, CI capture and existing regression gates.
- CI: PENDING until the exact head workflow finishes. Evidence artifacts must be inspected before claiming verification.
- User Windows play approval: NOT_RUN.
- Full v1 acceptance suite: NOT_RUN. This change does not implement PR-B through PR-H.
- Local authoring environment has no reachable GitHub network or Godot executable; Godot execution is delegated to the repository's isolated Godot 4.3 CI job, not assumed to have run locally.
- `tools/stamp_build.py` was tested in a temporary local Git repository for real SHA/branch, clean/repeat stamping, and tracked source dirty detection.

## Review boundaries

The `A.1` overlay is diagnostic only and deliberately does not pause combat. Existing ESC behavior remains unchanged here. Full modal focus, transaction-safe quit and atomic saving belong to PR-B. Invalid/future save data is preserved and automatic writes are blocked, but the complete save repair/title UX is not part of this stage. Source backups are additional files, not automatic restore operations.

The approved master package is identified by `spec_lock.json`. Only adoption documents and the implementation-order file are currently stored here; the full master/mockup bundle remains the original handoff, not silently rewritten or claimed to have been imported.

Do not merge into main or advance to a visual redesign before this checkpoint is verified.
