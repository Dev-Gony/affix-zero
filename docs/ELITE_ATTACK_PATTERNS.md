# Layout-specific elite attack patterns

This slice adds three mechanically distinct elite attacks on top of the
layout-specific stats and telegraphs introduced in `0b688c6`. It does not use
Hero Siege files, extracted commercial art, tracing, or third-party assets.
The telegraphs are original runtime geometry rendered with Unity's built-in
`LineRenderer`.

## Runtime mapping

| Layout | Elite / guardian | Telegraph | Resolved behavior |
| --- | --- | --- | --- |
| Ember Bastion | Ember Bulwark / Ember Warden | Orange radial ring for 0.58s / 0.48s | LOS-gated area slam at 50% actor damage in a 2.15m / 2.50m radius |
| Split Galleries | Gallery Stalker / Gallery Huntsmaster | Cyan approach line for 0.34s | LOS-gated dash to the committed endpoint, then a 60% damage hit within 0.88m / 1.05m |
| Ritual Crucible | Ritual Reaver / Ritual Hierophant | Magenta lane for 0.58s / 0.48s | 45% damage piercing-lane hit if the hero remains inside the 4.8m / 5.8m segment and LOS |

The source actor's ordinary swing is locked during a pattern tell. Resolution
uses `MeleeActor.ResolvePatternHit`, so armor, accepted-hit receipts, damage
feedback, death, and vampirism stay on the production combat path. Dashes use
the dungeon LOS query and do not cross blocked terrain. No save fields or
profile schema changes were added.

## Verification (2026-10-02)

- CoreSmoke: 269 PASS.
- Unity 6000.3.24f1 Windows build: PASS, 0 errors, 0 warnings, build GUID
  `56b6cfe08be34bfabbf669796f5ffb5e`.
- 150.056s fresh normal-drop 1080p run: PASS; 96 kills, 4 clears,
  4 guardians, 3 layouts, 0 failed runs, 0 death retries, 125 saves, and disk
  roundtrip. Pattern evidence: Ember 9 casts, Gallery 4, Ritual 6, 19 accepted
  pattern hits, observed mask 7.
- Separate-process restore/continuation: PASS with two additional kills.
- 720p and 1080p final-build UI probes: PASS; four actual framebuffer captures
  and 16 native UI Toolkit callbacks at each resolution; equip, talent, forge,
  bounds, and non-overlap checks passed.
- Final-build OS input: PASS in 11.352s against the exact PID, title
  `AFFIX ZERO`, and 1280x720 client. Mouse Start/equipment select/equip/talent
  reset/invest/forge enhance plus keyboard I/K/F/Escape pause/resume are all
  true. This is OS-level automated input, not a claim of human play.

Reports are under `Build/Reports/pattern-*` and remain ignored build evidence.
The first physical-input diagnostic stopped at `await-escape-pause` because an
open Forge screen consumes Escape to close itself. The clean rerun included
that close step and passed without changing game behavior.

## Library evidence

- Ember slam: `libfile_8e7b7a7edb708191bb64f5f067303e70`
- Gallery dash: `libfile_4c71fd7deca481919adbd6d24c7e9b7b`
- Ritual pierce: `libfile_7c35dd7d73e48191bd65b9e3ac41c3bc`

All three exact titles were confirmed by Library search. Upload succeeded;
Windows did not expose `os.setxattr`, so only the optional local xattr marker
could not be attached.
