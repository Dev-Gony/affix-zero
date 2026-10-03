# Speed, equipment, and skill UI slice

Date: 2026-10-03. This slice addresses two production needs: shorten repeated
play/QA observation without changing progression rules, and make equipment and
talent decisions readable as game UI rather than as a developer list. Hero
Siege screenshots were used only as layout-density and hierarchy references.
All panels, icons, colors, code, and runtime effects remain original AFFIX:
ZERO work.

## Session speed and pacing

The combat dock now exposes `1x`, `2x`, and `4x`. The selected value changes
Unity simulation time while the HUD keeps an unscaled wall clock for honest QA
reporting. Pause always sets time scale to zero and resume restores the selected
speed. Speed is deliberately session-only: it adds no save field and a new
process starts at `1x`.

The dungeon-clear reward and save still complete before the next layout begins,
but the inactive transition was reduced from 3.0 seconds to 0.65 seconds. This
removes dead air without changing enemy statistics, loot rolls, reward amounts,
or failure behavior.

Feedback remains readable at accelerated speed: pulse lifetime and camera shake
use unscaled time, ordinary hit audio is throttled within 35 ms, and critical or
killing impacts are never suppressed. `4x` reduces procedural line segment
density only; combat outcomes are unchanged. Normal, critical, area, and kill
impacts have stronger distinct tones, flashes, and shake than before.

## Equipment and talent decisions

The equipment screen is now a three-column PC layout:

- a dark metal/burgundy paper doll with eight equipment slots around the hero;
- a dense 6-by-4, 24-slot inventory grid;
- a dedicated comparison panel with rarity color, deltas, and compact
  `EQUIP` / `SALVAGE` actions.

Item details separate deterministic `BASE` statistics from randomized `ROLLED`
statistics. Each rolled option shows its configured legal range, sourced from
the same `LootGenerator` rules that create the item instead of duplicated UI
constants. The talent screen keeps six icon-led branches, adds the live current
effect to every node, and gives the selected branch more room for current/next
milestone, prerequisite, and invest information.

The natural weapon pool still has 29 historical base entries. The former
`magic_sword` entry is now the original `Arcane Staff` Staff route, so Sword
ARC, Axe QUAKE, and Staff LANCE can all appear through production rolls without
increasing drop-pool size or requiring a fixture. Inferior inventory items can
be salvaged into bounded existing Gold; profile schema v4 records cumulative
salvage Gold so save validation can prove its provenance. XP and displayed level
continue after the complete 75-rank tree, while new spendable points stop at 75.

## Verification

- CoreSmoke: 302/302 PASS. This includes schema v1-v4 migration, 75-rank cap,
  salvage provenance, configured tooltip ranges, three natural weapon routes,
  and three deterministic one-hour domain-equivalent models. Those models run
  in zero real-time combat seconds and are economy/build simulations, not player
  survival evidence.
- Static Unity source validation against installed engine API references:
  0 warnings, 0 errors. This check is not Unity import, Play, or rendering.
- Unity 6000.3.24f1 Windows build GUID
  `2495dc6d61c74d0f96b45bea3c79cde9`: build/import succeeded with 0 warnings
  and 0 errors.
- Real Windows `4x` production-flow run: PASS. In 149.123 wall-clock gameplay
  seconds it advanced 595.759 simulated seconds (3.995x), killed 431 enemies,
  cleared/transitioned 17 layouts, collected 43 of 44 natural drops, equipped
  25 upgrades, salvaged 19 items for 132 Gold, observed all three weapon styles,
  reached Torment, saved 581 times, and recorded zero deaths, failed runs, or
  safety restarts. Report:
  `Build/Reports/speed-runtime-4x-150s-v2-20261003/natural-progression-observe.json`.
- Separate-process restore: PASS with exact profile match, disk roundtrip, and
  two further kills at `4x`:
  `Build/Reports/speed-runtime-read-4x-20261003/natural-progression-read.json`.
- Final 720p and 1080p UI probes: PASS with four actual framebuffers and 19
  native UI Toolkit callbacks each. Speed cycling, equipment, talent, forge,
  bounds, and overlap assertions passed.
- Final OS-level automated input: PASS in 14.069 seconds against the exact
  launched `AFFIX ZERO` PID/title and 1280x720 client. Start, equipment open,
  slot/equip, talent open/reset/invest, forge open/enhance, pause, and resume are
  all true. This is bounded automation, not a claim of human play.

Final 1080p Library evidence:

- battle and visible speed controls:
  `libfile_fd8257c232ec8191829d32d50b5ac219`;
- equipment / inventory / comparison:
  `libfile_3e36a8f174188191bc235e3cc22728ec`;
- talent effects and evolution detail:
  `libfile_ff43eb54b5f4819196b263e1b4fc897b`.

All three Library creates succeeded as `image/png`. The prepared-upload
interface was unavailable, so the Library skill's ordered direct-batch fallback
was used. Windows Python lacks `os.setxattr`; only the optional local identity
marker failed, not the retained Library files.

The first short `4x` diagnostic proved acceleration but failed because one old
probe assertion still required time scale `1`; the assertion was generalized
and the full run passed. The earlier long `1x` observation was intentionally
stopped at 570 seconds when priorities changed; its partial data is retained but
is not labeled a completed run.

## Deliberately deferred

Town/hub flow, additional named difficulty tiers, gacha, and fully separate
biome tile/prop families were not added in this bounded slice. The current three
layouts remain mechanically and procedurally distinct temple districts. Human
play and user visual approval remain `NOT RUN` / pending.
