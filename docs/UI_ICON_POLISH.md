# Hero Siege-inspired equipment UI and 29 base-item icons

Date: 2026-10-03

## 2026-10-03 full illustrated-v2 replacement

The user rejected the first 29-icon contact sheet. Although its resource
mapping and technical uniqueness checks passed, the icons were crude flat
polygon placeholders: weak material rendering, repeated silhouettes,
color-swapped rings, and unreadable glove anatomy. That visual-quality
judgment supersedes the earlier local PASS wording below.

The user approved the direction represented by these three items:

- `longsword`: layered steel blade, brass guard, leather grip, red gems
- `plate_armor`: shaded plate volumes, gold trim, leather straps and wear
- `battle_gloves`: a readable pair of articulated gauntlets with leather
  cuffs and individual fingers

The remaining 26 flat placeholders have now been replaced by original
illustrated masters from the verified asset-only source commit
`98ac9a3614f44bd0c3104e945dd091792285785a`. All 29 transparent masters are
preserved in `docs/art-source/illustrated-equipment-v2/`; Unity consumes
reproducible 128 x 128 downscales. The new 26 are aspect-fitted with four pixels
of outer padding, while the three approved representatives retain their exact
previous runtime hashes and scale.

Direct inspection of the complete set at 128, 64 and actual 37 pixels plus
native 720p/1080p inventory captures confirms that silhouettes and primary
materials survive at slot size. Fine engraving naturally collapses at 37 px,
but the weapon, armor, glove, boot, ring and amulet families remain distinct.
The final `magic_sword` is an Arcane Staff, and no socket state is painted into
the art.

Inspected evidence:

- `docs/media/illustrated-equipment-v2/equipment-before-after-1080p.png`
- `docs/media/illustrated-equipment-v2/actual-scale-preview.png`
- `Build/Reports/ui-icon-polish/AFFIX-ZERO-29-base-icons-contact-sheet.png`
- `Build/Reports/ui-icon-polish/AFFIX-ZERO-29-icons-128-64-37.png`
- `Build/Reports/illustrated-v2-full-ui-{720,1080}/`

## Bounded scope

This pass changes presentation only:

- 29 existing base-item definitions now resolve to 29 distinct original icons.
- The equipment screen is a full-height right dock occupying exactly 500 / 1280
  (39.0625%) of the reference canvas.
- The paper doll sits above a dense 6 x 4 bag. The comparison card floats to the
  left of the dock instead of becoming a permanent third column.
- Burgundy leather fields, weathered metal edges, red corner plates, cream/gold
  headings, white base values, purple rolled affixes, and rarity edge markers
  establish the current Hero Siege-inspired material language.
- The compact upper-left portrait with thin player HP/XP and the illustrated
  lower-left skill dock remain intact.
- Player HP remains visible. Ordinary, elite, and guardian enemy HP bars remain
  absent. A real boss and boss-only HP treatment are still not implemented.

No item, stat, rarity, save field, reward rule, dungeon rule, or other gameplay
system was added.

## Original asset provenance

All 29 item paintings are project-original cloud-generated sources with exact
commit/tree provenance in `docs/art-source/illustrated-equipment-v2/README.md`.
`Tools/Local/Generate-AffixEquipmentArt.ps1` performs deterministic transparent
aspect-fit/downsampling and fail-closed dimension, border-alpha and duplicate
checks. The frame assets remain original local System.Drawing output. No paid
asset or recovered Godot/E0/V0 material is used, and existing Unity `.meta`
files remain tracked.

The 29 one-to-one base IDs are:

- Weapons: `dagger`, `longsword`, `axe`, `magic_sword`, `divine_sword`
- Helmets: `leather_hat`, `iron_helm`, `mithril_helm`, `dragon_helm`
- Armor: `cloth`, `leather_armor`, `plate_armor`, `dragonscale`
- Gloves: `cloth_gloves`, `leather_gloves`, `battle_gloves`, `dragon_gloves`
- Boots: `sandals`, `leather_boots`, `swift_boots`, `gale_boots`
- Rings: `copper_ring`, `silver_ring`, `gold_ring`, `diamond_ring`
- Relics: `bone_necklace`, `crystal_necklace`, `ruby_necklace`, `dragon_tear`

`LootGenerator.BaseIconResources` is the canonical map.
`HeroProgression` accepts only an exact current base path or the existing
legacy generic paths when restoring older saves.

## Final verification

Final Windows build GUID:
`66363a4ba5fa4fd1a991d9617ccaf2d4`

| Check | Result |
| --- | --- |
| CoreSmoke | PASS, 321 checks |
| Static Unity API compile | PASS, 0 errors / 0 warnings |
| Unity 6000.3.24f1 Windows build/import | PASS, 0 errors / 0 warnings |
| Runtime icon audit | PASS, 29 x 128 px, 29 unique SHA-256 values, zero alpha pixels on every canvas border |
| Final speed/equipment/salvage observe | PASS, final build GUID; 29 kills, 37 callbacks, live equip, two-step salvage, adjacent selection fallback, 720p bounds, schema 3 -> 4 |
| 1280 x 720 framebuffer UI | PASS, 19 callbacks, 29 icons loaded, 39.0625% dock, player HP present, non-player HP absent |
| 1920 x 1080 framebuffer UI | PASS, same assertions and 19 callbacks |
| OS-level native input | PASS, exact final-build 1280 x 720 window, ten mouse/keyboard actions in 17.189 s |
| Visual inspection | PASS by agent for all 29 at 128/64/37 px and native 720p/1080p equipment/talent frames; final user acceptance pending |

The nine save-writing probes used explicit D-only test directories. Production
`profile-v1.json`, backup, and lock hashes match before and after:

- primary:
  `6A3B7B1A6E2197F9E44C643690F522DA14B0F09D471D503A19793C0EE6C03AA1`
- backup:
  `739B214FB53377368C606BB8993796E9C0CB5FD7B5518EDCB4B20097678BF275`
- lock:
  `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855`

## Current delivered visual evidence

- 29-icon contact sheet:
  `libfile_af59bff5da448191ba99186f6b65708a`
- 1080p native equipment/comparison:
  `libfile_8b7dc6abc6808191b2de6ec8b5c31f8e`
- 1080p native skill tree:
  `libfile_5b3ab34c48a88191a5c2d0be2a238908`

All three Library creates succeeded. Prepared upload discovery was unavailable
inside the current helper runtime, so the supported direct batch-create fallback
was used. Windows lacks `os.setxattr`; only the optional local Library identity
marker failed, not upload or Library persistence.

## Troubleshooting record

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
| --- | --- | --- | --- |
| First speed stress invocation | Required mode/capture arguments were omitted | Re-ran with the documented observe mode, explicit D save/report/capture paths, and fail-closed isolation | A probe command is evidence only when every required isolation argument is explicit |
| Attack-speed phase ended one frame early | A nested coroutine left a parent-frame scheduling gap during a same-frame speed switch | Kept the attack wait in the parent loop so sampling and switching share one deterministic sequence | Coroutine boundaries can create observable races even when combat code is correct |
| Equipment visibility assertion raced panel resume | Probe dispatched and asserted in the same frame as panel resume/speed restoration | Added one frame before the assertion | UI Toolkit visibility should be sampled after the layout/update frame |
| Telegraph phase stopped before its assertion | The long fixture filled the bag and correctly stopped auto-hunt | Salvaged enough probe items to reserve four slots before the later phase | Stress fixtures must preserve capacity for the behavior they intend to observe |
| Library local identity writeback | Windows Python has no `os.setxattr` | Kept successful Library records as authoritative and reported the metadata-only limitation | Optional local metadata is not the Library upload result |
| First illustrated-v2 720p capture | A hidden player window called framebuffer `ReadPixels` outside the drawing frame | Preserved the failed report and reran with a normal render window in a new D-only report/save directory | Framebuffer evidence requires a real drawing window; hidden-window failures are not product failures or PASS evidence |
| Generator wrapper reported failure after producing outputs | PowerShell script invocation retained an unrelated native `$LASTEXITCODE` | Verified timestamps and hashes, then used PowerShell `$?` for script success | Native exit state and PowerShell script success are separate signals |
| First final OS-input attempt stopped at initialization | A new isolated save directory contained no copied natural-progression fixture | Preserved the fail-closed report and copied the hash-verified D-only fixture into a fresh pass2 directory | A safe input driver also requires the exact documented fixture precondition |

Failed/intermediate report folders are retained on D for audit; none is counted
as a final PASS.

## Honest remaining visual gaps

- Final user visual acceptance of the complete 29-icon set has not occurred.
- The shared dungeon material family still needs a larger authored biome pass.
- Typography remains the current project font treatment rather than a fully
  custom gothic display family.
- Existing starter and legacy-save generic icon paths remain only for backward
  compatibility; all current 29 base definitions use illustrated one-to-one
  paths. The actual native fixture shows seven representative bag icons while
  the generated contact/scale sheets cover the complete set.
- The framed six-node talent screen is verified and all six passive symbols now
  use distinct dedicated painted 128 px art, inspected together at 37/48 px and
  in locked/selected states at native 720p/1080p. This closes the bounded
  art/UI scope; final user visual acceptance remains pending.
- Town/hub, five difficulty tiers, gacha, pets, and new gameplay systems remain
  deferred by scope.
