# Layout-specific elite encounter identities

Date: 2026-10-02

This slice turns the three temple layouts into mechanically different combat
spaces without adding external art or changing the save schema. Existing
original AFFIX: ZERO monster sprites are retained. Elite floor sigils and
attack warnings are original procedural `LineRenderer` geometry using Unity's
built-in sprite shader; no Hero Siege or other commercial assets, timing, or
encounter data were copied.

## Encounter profiles

Each floor still contains 24 resident roaming enemies. The configured elite
stride remains difficulty-dependent, and enemy 24 is the floor guardian. It was
already an elite at every supported stride (4, 6, or 8), so the guardian does
not add an extra loot-eligible enemy or change population size.

| Layout | Elite identity | Guardian identity | Mechanical read |
| --- | --- | --- | --- |
| Ember Bastion | Ember Bulwark Elite | Bastion Warden Elite | slower movement and attack cadence, high HP/defense, heavier strike |
| Split Galleries | Gallery Stalker Elite | Gallery Huntsmaster Elite | fast lane pursuit and rapid short-reach attacks |
| Ritual Crucible | Ritual Reaver Elite | Crucible Hierophant Elite | medium cadence with long melee reach and converging pressure |

Base Scout values before the existing difficulty multipliers:

| Unit | HP | Damage | Defense | Move | Attack speed | Reach |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Ember Bulwark / Warden | 82 / 138 | 8 / 10 | 3 / 5 | 1.35 / 1.25 | 0.78 / 0.72 | 1.15 / 1.30 |
| Gallery Stalker / Huntsmaster | 68 / 112 | 6 / 8 | 1 / 2 | 2.15 / 2.35 | 1.38 / 1.50 | 0.90 / 1.00 |
| Ritual Reaver / Hierophant | 74 / 126 | 7 / 9 | 1 / 3 | 1.65 / 1.70 | 1.02 / 0.92 | 1.45 / 1.65 |

The sigil shape/color identifies the archetype before contact, while the
existing attack anticipation arc now uses that archetype's color, real reach,
and real impact timing. The compact target HUD shows the actor's actual name
instead of the generic `CURRENT TARGET` label.

## Verification

- CoreSmoke: PASS, 267 checks.
- Unity 6000.3.24f1 Windows build: PASS, zero C# errors/warnings, build GUID
  `e4db1b39184e4381980d2a5dc70171c8`.
- 120.054-second normal-drop 1080p run: PASS at 1x; 72 kills, three clears,
  13 elite kills, three guardian kills, all three elite pattern bits, all three
  layouts and difficulties, four natural items, three recovery casts, 96
  profile saves, zero failed runs/deaths/safety restarts.
- Separate-process restore: PASS; canonical profile and disk round-trip matched,
  then two further real kills completed.
- 720p and 1080p UI probes: PASS; four framebuffer screens and 16 native UI
  Toolkit callbacks per resolution; equipment, talent, and forge mutations
  verified.
- OS-level automated input: PASS against the validated `AFFIX ZERO` native
  window. Start click, I/equipment select/equip, K/reset/invest, F/enhance, and
  Escape pause/resume all reached the real input path. This is not claimed as
  human play.
- Visual inspection: PASS for one 1920x1080 elite-target frame from every
  layout; sigils, target names, combat effects, and the compact HUD were
  visible without clipping.
- Long-term fun/balance: NOT CLAIMED. The 120-second run verifies encounter
  distinction, survival, progression, and persistence, not a multi-session
  retention judgment.

Evidence is under `Build/Reports/elite-identity-*` and the runnable player is
`Build/Windows/AffixZero.exe`.

Library evidence:

- Ember Bulwark: `libfile_df8558f050008191919d38cb72c65ec4`
- Gallery Stalker: `libfile_4e2055d6d3008191bf6388c1a7b20218`
- Ritual Reaver: `libfile_2b21dd056fd08191960cf6c888c9065a`

All three Library creates succeeded. Local Library identity metadata could not
be attached because this Windows Python build lacks `os.setxattr`; the Library
files themselves are unaffected.
