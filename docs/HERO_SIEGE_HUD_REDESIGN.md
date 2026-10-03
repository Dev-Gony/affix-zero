# Compact combat HUD redesign

Date: 2026-10-02

## 2026-10-03 superseding visual pass

The current implementation goes beyond the earlier compact-layout pass below.
It uses the reference only for hierarchy, density, proportions, and interaction
placement; no Hero Siege bitmap, extraction, trace, logo, or commercial asset
is present.

- `EquipmentPanel` is now a 520-by-704 right-edge dock on the 1280-by-720
  reference canvas. Its upper burgundy leather field contains eight spatial
  weapon/armor/relic slots around the hero, while the lower field is a dense
  six-by-four bag grid. Selecting loot opens a separate black, metal-framed
  comparison card to the left; it is not a permanent third column.
- `EncounterHud` now uses a 106-pixel sculpted crest, two thin truthful HP/XP
  bars, a 364-by-132 lower-left skill dock, illustrated area/recovery icons,
  and the existing approximately 235-pixel physical minimap at 1080p.
- The former general target-health strip is removed. Ordinary monsters never
  receive a boss-style health bar, and the current game does not invent a boss
  classification.
- Sixteen new UI raster files live under
  `Assets/Art/Interface/Resources/AffixUIVisual/`. The metal frames are crops
  from the project's existing authored `HudFrames-v1` atlas. Burgundy leather,
  bag cells, five missing equipment icons, and two ability icons are original
  deterministic pixel constructions from
  `Tools/Local/Generate-AffixInterfaceAssets.ps1`. None is derived from a
  commercial game.
- New loot now routes helmet, gloves, boots, ring, and amulet to their distinct
  authored icon resources. Save-path validation accepts those reviewed paths;
  existing saved items and all progression schemas remain compatible.

The combat art remains the built-in `image_gen` Original Temple Combat Set
documented in `docs/assets/original-temple-combat-set.md`: two 64-cell directional
atlases, eight-frame impact atlas, temple room, and obstacle. Runtime mapping is
128-by-128 cells, four row-pair directions, idle 0-1, walk 2-5, hit 6-7,
attack 0-4 with impact index 2 at 12 FPS, death 5-7, pivot `(0.5, 0.14)`.

Final verification for this pass:

- CoreSmoke: PASS, 303 checks.
- Unity API static compile: PASS, 0 warnings / 0 errors.
- Unity 6000.3.24f1 Windows build: PASS, 0 warnings / 0 errors, player build
  GUID `0b48241cb36746ba8a7c95d87247fd59`.
- 1280-by-720 player UI probe: PASS, four actual framebuffers and all native
  equipment/talent/forge callbacks.
- 1920-by-1080 player UI probe: PASS, four actual framebuffers and all native
  equipment/talent/forge callbacks.
- Direct visual inspection: battle center remains open; right dock, floating
  comparison, 24 bag cells, crest, thin bars, skill icons, and minimap are
  visible without clipping at both sizes.
- Physical OS input: NOT RERUN for this pass. User visual approval remains
  pending.

Final local evidence is under `Build/Reports/hero-siege-ui-720-release-20261003/`
and `Build/Reports/hero-siege-ui-1080-release-20261003/`; it is intentionally
excluded from Git.

Final Library evidence:

- 1080p battle: `libfile_68d5e77f784481919ea7349f91984609`
- 1080p equipment and floating comparison: `libfile_bcaf0907c99c8191a09473943cb14af8`

Both Library creates succeeded. Windows does not expose `os.setxattr`, so the
optional local version marker could not be attached; this does not affect the
stored Library files.

This change replaces the full-width dashboard HUD with a compact combat layout
derived from five user-supplied 1920x1080 Hero Siege reference screenshots. The
references were pixel-reviewed for hierarchy and density only. No Hero Siege
art, files, tracing, extraction, or copied UI textures are present in the
project.

## Reference findings and implementation

- The top-left unit frame now combines the existing original hero sprite,
  name, level ribbon, real HP, current-level XP, selected difficulty, and the
  two real automatic-skill cooldowns.
- The earlier current-target strip described in this historical section was
  removed by the superseding pass above; ordinary enemies have no health bar.
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
