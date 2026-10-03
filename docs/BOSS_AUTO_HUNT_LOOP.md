# Route-end boss loop

Date: 2026-10-03

## Implemented scope

Every connected layout now has one genuine route boss in enemy slot 24:

- Ember Bastion: `ASHEN TYRANT`
- Split Galleries: `DROWNED ARCHON`
- Ritual Crucible: `ECLIPSE SOVEREIGN`

The boss remains visible and has a reserved top-center health presentation, but
is sealed against targeting, attacks and area damage until the other 23 enemies
are dead. The former route guardian remains enemy 23 and is still an elite, not
a boss. Normal enemies, elites and guardians still have no health UI.

The boss uses a large procedural double-ring seal and crown silhouette. Its
`RUIN PULSE` draws an outer boundary, converging countdown ring and five spokes
for 0.95 seconds. The automatic hunter consumes that same hazard origin/radius,
clears its attack target and moves to a walkable point beyond the boundary.
Damage resolution continues through `CombatHealth`; no second combat model or
collider was added.

Boss death uses the existing 29-base loot pool, rarity roll and affix generator
through `LootGenerator.GenerateGuaranteed`. It does not add an item, stat,
currency, socket, class or save field. Pending loot retains the existing full-bag
fail-closed behavior. After collection and the existing 0.65-second rest, the
next saved layout is rebuilt and all 24 enemies, including its sealed boss, are
spawned for the next autonomous cycle.

## Final runtime evidence

Final Windows build GUID: `c40ab46dad6c4ec28f777356d0c545f8`.

- Unity 6000.3.24f1 import/Windows build: succeeded, 0 errors, 0 warnings.
- CoreSmoke: 321 checks PASS.
- Installed-Unity API static compilation: 0 errors, 0 warnings. This is not a
  substitute for the Unity import/build or player runs above.
- 1280x720 automatic-loop run
  `Build/Reports/boss-loop-release-720-20261003/auto-hunt-smoke.json`: PASS,
  24 kills, boss kill ordinal 24, one guaranteed boss loot offer, two resolved
  boss patterns, one successful avoidance decision, zero boss-pattern hits,
  zero death retries, one layout transition and a fresh 24-enemy next segment.
- 1920x1080 reference run
  `Build/Reports/boss-loop-release-ui-1080-20261003/ui-reference-probe.json`:
  PASS, five native framebuffers, 19 UI callbacks, all 29 base icons, player HP
  visible, boss HP visible, and normal/elite/guardian HP absent.
- Exact-window OS input
  `Build/Reports/boss-loop-release-physical-20261003/physical-input.json`: PASS
  for all ten Start/equipment/talent/forge/pause/resume actions against PID
  32212 and a 1280x720 client. This is automation, not human play.

Tracked inspected frames:

- `docs/media/boss-loop/boss-telegraph-720.png`
- `docs/media/boss-loop/boss-hud-1080.png`
- `docs/media/boss-loop/next-segment-720.png`

Library delivery mirrors those three inspected frames: telegraph
`libfile_bfaf54d538508191964cf2c8b7e5a212`, 1080p boss HUD
`libfile_0f4a836dae5c8191b719e203208bef39`, and next segment
`libfile_ee46e3c48bf08191ae2b45d216b1c50a`.

The images show the first boss telegraph at 23 kills with one enemy alive, the
sealed boss-only health treatment at 1080p, and the second layout already
repopulated after the first boss clear. Human play and final user visual
approval remain not run/pending.

## Save isolation

All player probes used explicit non-root D: save directories and D: `TEMP/TMP`.
The production primary, backup and lock hashes remained:

- primary: `6A3B7B1A6E2197F9E44C643690F522DA14B0F09D471D503A19793C0EE6C03AA1`
- backup: `739B214FB53377368C606BB8993796E9C0CB5FD7B5518EDCB4B20097678BF275`
- lock: `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855`

## Troubleshooting

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
|---|---|---|---|
| First CoreSmoke invocation wrote its disposable fixture under C: user Temp | `TEMP/TMP` was not set for that one command; production save hashes did not change | Preserved the fixture without deletion and set both variables to `Build/Temp` for every later command | D-only policy applies to pure .NET test fixtures as well as Unity/player runs |
| First 720p smoke stopped after 1.1 seconds | Probe expected a retired multiline hero HP string while the actual HUD uses `current / max HP` | Updated only the stale assertion to the current named `hero-hp-value` contract | A fail-closed probe can itself regress; compare it with the rendered authoritative UI before changing gameplay |
| Second 720p smoke rejected an accepted area hit | Probe treated every area receipt as a hero area skill, but existing elite patterns also use the same receipt metadata | Separated hero skill receipts from active elite/boss pattern receipts and retained LOS/hazard-radius checks | Shared damage receipts need attacker/context discrimination in observers |
| First boss-loop frame showed the boss dying at kill 21 | A far-end spawn did not guarantee final target order under nearest reachable selection | Added an explicit seal: exclude boss from AI targeting/aggro and reject damage until 23 non-boss deaths; report and assert `bossKillOrdinal == 24` | Spatial staging alone is not a gameplay phase contract |
| Library prepared-upload helper was unavailable; Windows xattr was unsupported | The helper reported unavailable before upload and Python on Windows lacked `os.setxattr` | Used the allowed ordered direct Library create; both icon deliverables succeeded, and optional local identity metadata was skipped | Upload success and optional local metadata are separate outcomes |

## Deliberate boundaries

This slice does not implement sockets, five difficulty tiers, town, gacha,
pets, classes, paid systems or new biome atlases. The boss reuses the current
enemy animation family with a larger procedural silhouette; it is readable and
mechanically distinct, but it is not a separately authored boss sprite sheet.
Socket capacity remains a proposal in `ITEM_SOCKET_REQUIREMENT.md`, not runtime
state or an approved 29-number contract.
