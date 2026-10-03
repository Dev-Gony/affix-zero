# Probe save-path isolation follow-up

Date: 2026-10-03

## Incident and provenance

An initial `SpeedUiStressProbe` diagnostic supplied an isolated `-affixSaveDir`,
but `ProfilePersistence` did not classify `-affixSpeedUiStressTest` as a save
test. The effective path therefore fell back to the player's default profile:

`C:\Users\Gwony\AppData\LocalLow\Dev-Gony\AFFIX ZERO\profile-v1.json`

That process loaded the default profile and its normal `OnApplicationQuit`
path rewrote the primary envelope as schema 4 at
`2026-10-03T01:01:38.3648325Z`. The post-incident primary is 13,301 bytes with
SHA-256
`6A3B7B1A6E2197F9E44C643690F522DA14B0F09D471D503A19793C0EE6C03AA1`.
No pre-incident byte hash was captured, so byte-for-byte or complete semantic
identity with the earlier primary is **not** claimed. A read-only comparison
with the backup covered only a subset of progression values; it was not a full
field comparison and must not be described as one.

The existing backup was not restored, deleted, renamed, or rewritten. It is
10,351 bytes, timestamped `2026-10-01T23:24:59.1296559Z`, SHA-256
`739B214FB53377368C606BB8993796E9C0CB5FD7B5518EDCB4B20097678BF275`.
The empty lock file remains SHA-256
`E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855`.

## Fix

`ProbePathPolicy` is now evaluated before the ephemeral/writable persistence
split. Every standalone probe flag requires one explicit, absolute, non-root
D: `-affixSaveDir`; duplicate arguments, another drive, the production path,
and descendants of the production path fail closed. Writable probes open only
`<save-dir>/profile-v1.json`; ephemeral probes validate the sandbox argument
but still open no store. Physical-input and speed/UI probes also assert their
effective file path exactly. CoreSmoke covers all nine probe flags and the
negative path cases.

The old save probe also compared the hero position with a retired coordinate;
it now uses `DungeonWorld.Entrance`. This was a probe defect, not a gameplay
spawn change.

## Sentinel and regression evidence

All commands in this follow-up set `TEMP` and `TMP` to
`D:\github\affix-zero-original-temple\Build\Temp`.

- Missing `-affixSaveDir`: expected FAIL before gameplay, exact problem
  `Probe verification requires an explicit -affixSaveDir.` The production
  directory was byte-identical before/after. Report:
  `Build/Reports/isolation-missing-save-rejected-20261003-0148/`.
- Isolated speed/UI observe: PASS from copied D: schema 3 to schema 4. The D:
  profile changed from SHA-256
  `4E27A297739A4602403666E260F0632921563CA1F297BD46E1928F84063BC7F9`
  to
  `36B2E9B71426E272F6E2351773F54D0A215303377CADD9A18ED179D1BD09D2ED`
  while the production directory remained byte-identical. It verified 29 real
  kills, 15 speed changes, panel-open pause/resume, equipment, two-step
  salvage provenance, forge, a live natural drop, clear transition, elite
  telegraph, 720p bounds, unique ledgers, migration, and disk round-trip.
  Report: `Build/Reports/speed-ui-final4-observe-20261003-0157/`.
- Separate-process speed/UI read: PASS. Session speed reset to 1x, combat
  continued, and the isolated profile changed again to
  `78283BEF1C1A4827AF332AF36063FDC5894D8AE9132F4E861FA34B0A120D4921`;
  production remained byte-identical. Report:
  `Build/Reports/speed-ui-final4-read-20261003-0158/`.
- Final physical OS input: PASS against exact PID `23180`, title `AFFIX ZERO`,
  and 1280x720 client. Start, I, equipment select/equip, K, talent reset,
  Vitality invest, F, forge, and Escape pause/resume were all observed by the
  player probe. It is automation, not human play. The copied D: profile changed
  from the read hash above to
  `24FED0E383094EBF55345A52C3DAD422E1BB69E6A7EE424ECE801E34C4350B77`;
  production remained byte-identical. Report:
  `Build/Reports/physical-input-final5-20261003-0208/`.

Two earlier physical-input attempts sent no input because the exact window did
not own foreground; a third reached equipment and then timed out on obsolete
coordinates. They are excluded from PASS evidence. The final local driver
reactivates the exact PID before every action, retains the foreground refusal,
and uses current verified 720p control centers. `Build/` is Git-ignored.

## Ordinary-enemy HP audit

The repository contains no ordinary, elite, or guardian world-space health
renderer. The removed top-center target strip was the only non-player HP UI.
The current game has no genuine boss type, so it does not invent a boss bar.
Player HP remains in `hero-hp-value`.

Final 720p and 1080p reference runs each captured four real framebuffers after
a real hero hit, with 24 active enemies and 19 native UI callbacks. Runtime
checks found no Canvas, TextMesh, health-named child, or non-hero health-like
UI Toolkit element under any active enemy, while the player HP label remained
visible. Both runs PASS:

- `Build/Reports/ui-reference-hp-final-720-20261003/`
- `Build/Reports/ui-reference-hp-final-1080-20261003/`

Direct image review also shows no overhead or top-center non-player HP bar.

Final Library evidence:

- 720p battle: `libfile_2482b83697c08191b2e530e1dec5739f`
- 1080p battle: `libfile_0179c6cfa3208191881a5632489ce913`

Both creates succeeded. The returned local `user.library-file-version=0`
metadata could not be attached because this Windows Python lacks
`os.setxattr`; the Library files themselves are unaffected.

## Final verification and honest visual gaps

- CoreSmoke: PASS, 320 checks.
- Unity API static compile: PASS, 0 warnings / 0 errors.
- Unity 6000.3.24f1 Windows build: PASS, 0 warnings / 0 errors.
- Final player build GUID: `3360c8b3dafa484d9a1afed8985cc4ec`.
- User visual approval: pending.

The implementation now has the requested compact density, small readable
actors, original temple set, attacks, impacts, and unobstructed combat view,
but it is not a pixel-for-pixel Hero Siege recreation. Remaining gaps are one
temple art family across three arrangements rather than several fully distinct
biomes, shared icons for some item bases, simple typography, some softness in
scaled frame corners, and no genuine boss encounter/boss-only health bar.
