# AFFIX: ZERO UI/UX Development Log

## Record preservation

The complete previous log is preserved byte-for-byte in
[the through-G6.1 archive](archive/AFFIX_ZERO_UIUX_DEVLOG_THROUGH_G6_1.md).
Earlier implementation claims there are historical claims, not current art approval.
This entry point records the Windows rejection and the corrective work rather than
silently rewriting that history. The original chronology and every earlier milestone remain available.

## 2026-09-25: G6 playtest rejection and recovery candidate

Baseline: `433948879fdfd23cf1cccaa0ae0c8ce3a0784eae`.
Recovery branch: `fix/g6-playtest-recovery`, PR #17. Neither #15 nor #17 is approved for merge.

### Problem

Windows play feedback rejected G6.1: static-looking enemies, detached weapon swings,
oversized procedural pets obscuring the hero, poor attack/drop readability, and summons
that appeared to grant nothing. The supplied floor-85 screenshot also showed `0/93`
in the main HUD versus `0/13` in the objective widget.

### Cause

- Enemy animation clocks advanced without continuous redraw requests during movement.
  Recovery state was consumed by drawing code but was never started at the strike.
- A second geometric weapon was drawn over class artwork that already held a weapon.
  It was also drawn for spellcasters and reset the parent drawing transform.
- Pet follow logic switched sides every three seconds, crossing the hero, while rarity
  and evolution enlarged the same procedural silhouette and ambient aura.
- Summon output was a temporary notification behind the management panel. The roster
  did not show shard balances, so duplicates had almost no visible evidence.
- Currency was debited before validating every draw; a failed ten-pull could be partial.
- The top HUD duplicated an obsolete `8 + floor` goal instead of the actual encounter rule.
- Existing tests primarily asserted state/properties. They did not establish final visual quality.

### Reference UX

Retain the user's direction: Hero Siege-like ARPG equipment depth and a dark-fantasy
world, with Survivor-style readable combat/rewards and autonomous movement.
For this recovery, the concrete UX contract is visible reward receipts, an unobscured
hero, and preparation -> release -> hit feedback. This is not a new visual redesign.

### Decision

Fix observable behavior and transaction feedback first. Preserve saved pets, levels,
stars, gear, balances, and the existing art assets. Do not add another gameplay system,
payments, replacement geometric art, or declare sprite animation complete.

### Implementation

- `SummonResultPanel` presents every result from a one/ten summon with rarity, name,
  new/duplicate status, shard grant, before/cost/after crystals, and save status.
- The receipt is outside the rebuilt pet-tab subtree, blocks duplicate commands while
  open, persists in the pet save data, and can be reopened without rerolling or charging.
- The roster shows shard balances. Incomplete catalogs and impossible guaranteed draws
  leave ownership, currency, pity, and the prior receipt unchanged.
- `GamePresentation` extends the existing UI without replacing the tested inventory,
  enhancement, skills, or rebirth screens. Both HUDs use WorldLayout's encounter goal.
- `CombatDirector` extends the existing battle manager: melee hits wait for the release
  beat; caster and pet damage wait for projectile impact; pending attacks cancel on
  room/class transitions. Skills no longer overwrite an in-progress basic attack.
- Enemy redraw runs continuously and strike starts recovery. Decorative stick-limb
  overlays were removed; the underlying single-frame atlas remains.
- The duplicate procedural player weapon was removed, retaining class artwork and stats.
- The pet uses a stable offset, minimum 25-world-unit separation, and a smaller fixed
  silhouette. Support feedback remains; the rejected pet artwork itself is not replaced.
- Accepted equipment ownership commits before any presentation delay. Clearing VFX
  cannot remove an already accepted drop.

### Failure / Revision

The original smoke assertion expected damage in the same call as attack startup.
After implementing anticipation, this test correctly failed. It now waits for the
release beat and still asserts hit feedback. New tests additionally assert *no* early
hit, exactly one release hit, projectile-only impact, and cancellation on transition.
This is a timing-contract correction, not deleting the damage assertion to obtain green CI.

### Verification

Executed locally with the official Godot `4.3.stable.official.77dcf97d8` engine from the
same CI image, isolated XDG directories, and `--affix-test-mode`:

- PR-A safety: 37 checks passed.
- PR-B safety: 35 checks passed.
- V3 balance: 45; V4 elites: 14; growth: 15; loot: 21; V5 pets: 71; UIUX V2: 32 passed.
- Existing gameplay smoke: passed after the release-beat assertion correction.
- Recovery behavioral suite: 168 checks passed, including repeated pet clearance samples.
- Graphical recovery capture: passed using Xvfb + Mesa software OpenGL, not headless
  image placeholders. One/ten-result cards and confirmation controls fit the viewport;
  the receipt survived combat-driven pet refresh. Enemy redraws were observed across frames.
- Actual rendered one/ten-result and combat images were inspected. Their remaining
  procedural pet art and static enemy source artwork were not marked visually accepted.

CI reruns the baseline suites and Windows/Web exports; the recovery workflow separately
runs these behavior checks and publishes actual rendered evidence and metadata.
Consult the exact PR HEAD's Actions results; a prior commit's green result is not approval
for a later commit. Local exit emitted ObjectDB cleanup warnings in smoke/recovery; no
script or parser errors were reported. Treat the warning as follow-up, not a hidden pass.

### Before / After

| Behavior | G6.1 baseline | Recovery candidate |
|---|---|---|
| Summon feedback | Hidden transient message | Persistent per-result receipt and shard balances |
| Invalid batch | Debit before full validation | All-or-nothing preparation before state commit |
| Floor-85 HUD | 93 and 13 disagree | Both use 13 |
| Pet following | Crosses hero every three seconds | Stable offset and enforced clearance |
| Player weapon | Extra geometric blade over class art | Original class-held weapon only |
| Attack damage | Startup/instant pet hit | Release beat / projectile impact |
| Enemy pose update | Redraw missing during movement | Continuous redraw and started recovery |
| Accepted item | Delayed inventory ownership | Ownership independent of visual lifetime |
| Art acceptance | Overstated in prior explanations | Explicitly rejected/pending |

### Unfinished / not approved

**This patch does not provide authored multi-frame character/monster animations, a new
pet sprite sheet, final drop art, a cohesive final dungeon art pass, or x5 visual-quality
approval.** Existing single-frame enemy motion is not equivalent to limb animation.
Windows approval for this recovery is NOT_RUN. Preserve this gap visibly before any
next visual milestone. No merge without the user's explicit play approval.
