# Visible build evolution milestones

Date: 2026-10-02

This slice makes the existing six-node talent tree change real combat behavior
at readable milestones. It adds no save fields: every evolution is derived from
the already persisted rank values, so v3 profiles, resets, and old profiles
retain their existing compatibility.

## Real effects

Baseline behavior is unchanged until a milestone is reached.

| Branch | Evolution I | Evolution II |
| --- | --- | --- |
| Fury | rank 5: automatic area strike rises from 150% to 165% attack | rank 15: 180% attack |
| Precision | rank 3: +5 critical chance | rank 8: +10 critical chance total |
| Keystone | rank 2: +5 armor penetration | rank 5: +12 armor penetration total |
| Vitality | rank 5: WIND, trigger at 50% HP and heal 25% | rank 15: SURGE, trigger at 55% HP and heal 30% |
| Cleave | rank 3: WHIRL, area radius 3.6m | rank 8: TEMPEST, area radius 4.1m |
| Haste | rank 3: area strike can fire on one target; area/recovery arm at 2.2s/7s | rank 8: arm at 1.8s/6s |

The existing per-rank attack, health, splash, and cooldown effects remain
active. The area strike still uses its existing 4.5-second base cooldown and
the recovery skill its existing 14-second base cooldown, both modified by the
real attack-speed/cooldown build.

The combat dock now shows the active area and recovery names and exposes exact
damage, radius, target count, trigger, and heal values in native tooltips. The
talent detail panel shows the current real effect plus the next rank milestone.
Area names prioritize Cleave, then Fury, then Haste when several branches have
evolved. The impact ring reads the actual evolved hit radius rather than a
fixed presentation constant.

No new external, extracted, traced, or commercial art is used. The screen
retains the original AFFIX: ZERO sprites, procedural combat geometry, and
project-native UI Toolkit styling.

## Verification

- CoreSmoke: PASS, 269 checks, including baseline, full-tree, and reset
  evolution assertions.
- Unity 6000.3.24f1 Windows build: PASS, zero errors and zero warnings, build
  GUID 2d211235839a4a348d357bae63677f07.
- Final 150.042-second normal-drop 1080p run: PASS at 1x; 83 kills, three
  clears, three guardians, all three layouts/patterns, two natural items,
  WIND active at 50%/25%, three real recovery casts, 106 saves, zero deaths,
  failed runs, or safety restarts.
- Separate-process restore: PASS; canonical profile matched, evolved WIND
  state restored, two further kills completed, and disk round-trip matched.
- Final 720p and 1080p UI probes: PASS; four actual framebuffer screens and 16
  native UI Toolkit callbacks at each resolution, with equipment, talent, and
  forge mutations verified.
- Visual inspection: PASS for the final 1080p combat and talent frames. WIND,
  the compact HUD, milestone text, and the open battlefield remain readable
  without clipping or overlap.
- An earlier exact-120-second diagnostic reached 71 kills, activated
  SECOND WIND before the label was shortened, and saw all three patterns,
  but failed the pre-existing full-run assertion because only two of three
  guardians had died at the time boundary. No game balance was changed to hide
  that result; the final bounded run was extended to 150 seconds.
- OS-level physical input: NOT RERUN for this slice. The immediately preceding
  build passed ten validated native-window mouse/keyboard interactions; this
  slice used native callback coverage plus the real Windows player and does
  not claim human play.

Final evidence is under Build/Reports/evolution-final-* and
Build/Reports/evolution-deliverable-20261002.

Library evidence:

- evolved 1080p combat: libfile_c3bd75f8af788191aeec9fa64e21ef79
- 1080p talent tree: libfile_8d50ff8e9f508191926dfe9eca4fa439

Both Library creates succeeded and exact-title metadata search confirmed their
PNG sizes. Local Library identity metadata could not be attached because this
Windows Python build lacks os.setxattr; the saved Library files are unaffected.
