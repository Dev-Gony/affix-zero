# Compact combat HUD redesign

Date: 2026-10-02

This change replaces the full-width dashboard HUD with a compact combat layout
derived from five user-supplied 1920x1080 Hero Siege reference screenshots. The
references were pixel-reviewed for hierarchy and density only. No Hero Siege
art, files, tracing, extraction, or copied UI textures are present in the
project.

## Reference findings and implementation

- The top-left unit frame now combines the existing original hero sprite,
  name, level ribbon, real HP, current-level XP, selected difficulty, and the
  two real automatic-skill cooldowns.
- The current target is a narrow, centered bar and is hidden when no live
  target exists.
- Region, clear, kill, difficulty, and multiplier information is grouped beside
  the compact upper-right minimap.
- The lower-left dock contains the real attack, area, and recovery states, four
  explicitly empty future slots, the current-level XP line, real combat/bag/
  talent/gold values, and the existing navigation and run controls.
- The old decorative mana orb was removed because the game has no spendable
  mana loop. No DPS, ping, video controls, or unavailable resource values were
  invented.
- Existing equipment, talent, forge, save, progression, and combat behavior is
  unchanged. `ReferenceUiProbe` was updated to the current reward curve and to
  verify the new named compact regions.

The UI Toolkit reference canvas remains 1280x720. At 1080p the top-left cluster
is approximately 330 physical pixels wide, the lower-left dock 540 pixels wide,
and the minimap 234 pixels wide. This preserves the wide combat viewport rather
than covering it with a full-width header and footer.

## Verification

- CoreSmoke: PASS, 267 checks.
- Unity 6000.3.24f1 Windows build: PASS, build GUID
  `6e217fe60e554e19ad071d542189c03e`; no C# compile errors or build failure.
- Final 1280x720 UI/player probe: PASS, four actual framebuffer captures,
  16 native UI Toolkit callbacks, equipment/talent/forge changes verified.
- Final 1920x1080 UI/player probe: PASS, four actual framebuffer captures,
  16 native UI Toolkit callbacks, equipment/talent/forge changes verified.
- Normal-drop progression/write: PASS for 120.027 seconds, 75 kills, three
  clears, all three layouts and difficulties, 97 saves, and disk round-trip.
- Final-build separate-process restore: PASS; canonical restore and disk
  round-trip matched, then two more real kills completed.
- Actual OS mouse/keyboard automation: NOT RUN. The player probes use native UI
  Toolkit callback dispatch and do not claim physical-input coverage.
- User visual approval: pending.

Reports and screenshots are deliberately kept out of Git under `Build/`:

- `Build/Reports/hud-final-release-720/ui-reference-probe.json`
- `Build/Reports/hud-final-release-1080/ui-reference-probe.json`
- `Build/Reports/hud-natural-write/natural-progression-observe.json`
- `Build/Reports/hud-natural-final-read/natural-progression-read.json`
- `Build/Reports/hud-redesign-evidence/before-1080-battle.png`
- `Build/Reports/hud-redesign-evidence/after-720-battle.png`
- `Build/Reports/hud-redesign-evidence/after-1080-battle.png`
- `Build/Reports/hud-redesign-evidence/after-1080-equipment.png`

Runnable player: `Build/Windows/AffixZero.exe`.

Library delivery:

- Before, 1080p battle: `libfile_cc97fddf98c48191bc63d0bdefa49182`
- After, 720p battle: `libfile_75c6ac9a5de08191bb615c5178fd2ccb`
- After, 1080p battle: `libfile_21a4668d0df881918d956072fe6899cf`
- After, 1080p equipment: `libfile_5f54c5ad37908191a68aa537e040e0aa`

All four Library creates succeeded. Local Library identity metadata could not be
attached because this Windows Python build does not provide `os.setxattr`; this
does not affect the saved Library files.
