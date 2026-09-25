# AFFIX: ZERO UI/UX Development Log

> Portfolio-facing record for verified gameplay/UI milestones.  
> Rule: every major stage records **Problem → Cause → Reference UX → Decision → Implementation → Failure/Revision → Verification → Before/After**.

## 2026-09-25 — G3.3 Epic / Economy / Damage Readability

### Problem

- Legendary loot still appeared often enough that the top-end chase weakened over long runs.
- Normal enemies could produce late-floor one-hit deaths while bosses did not feel proportionally more dangerous.
- Inventory cleanup required repetitive item-by-item selling.
- Inventory detail text was cramped and could be clipped.
- Sell prices were too low relative to the late-game gold economy.
- Adding a sixth rarity broke the legacy smoke test because it still assumed five rarity/filter entries.

### Cause

- The rarity table ended at legendary and the old regression contract hard-coded a five-tier model.
- Enemy damage scaling had penetration/minimum-damage pressure but no separate normal-vs-boss maximum-hit profile.
- Selling used old stored `sell_value` values and had only limited bulk-sale behavior.
- The management/inventory detail area was sized for shorter item descriptions.

### Reference UX

- **Hero Siege:** long-tail ARPG item chase, strong rarity hierarchy, equipment progression depth.
- **Survivor.io:** readable combat danger, quick reward recognition, low-friction inventory/growth interactions.
- **AFFIX: ZERO direction:** keep the ARPG depth while reducing repetitive inventory work and making combat outcomes easier to read during idle auto-hunt.

### Decision

- Add **Epic** above Legendary as rarity index 5 and make it dramatically rarer than Legendary.
- Keep normal enemies dangerous but cap a single normal hit at **32% of max HP**; bosses use a distinct profile capped at **60%** with stronger penetration/minimum chip pressure.
- Add **Sell All** that sells every unlocked inventory item and always preserves locked gear.
- Recalculate sell values dynamically from item level, rarity, and enhancement level.
- Expand the management/inventory detail region so the full item description is visible more reliably.
- Treat six rarity tiers as the new regression contract everywhere touched by G3.3.

### Implementation

- Added `resources/items/rarity_epic.tres`:
  - id: `epic`
  - index: `5`
  - max affixes: `6`
  - stat multiplier: `4.5`
  - base drop weight: `0.2`
- Wired Epic into `ItemGenerator.gd` and applied an additional top-tier probability reduction.
- Updated loot-filter bounds and labels from 5 to 6 rarity choices.
- Added `GameManager.calculate_item_sell_value()` / `item_sell_value()`.
- Added `GameManager.sell_all_unlocked()` with locked-item protection.
- Split enemy damage into normal/boss profiles through `take_damage(raw_damage, is_boss)`.
- Increased boss HP/ATK/DEF multipliers independently from normal floor scaling.
- Expanded management window and inventory detail layout in `GameUI.gd`.
- Added G3.3 regression checks to `tests/gameplay_v3_balance.gd`.
- Updated `tests/smoke_test.gd` from the legacy 5-rarity expectation to 6.

### Failure / Revision

- First G3.3 CI attempt failed because the older smoke suite still expected:
  - 5 loot-filter entries
  - 5 loaded rarity resources
- Gameplay V3 balance tests themselves passed, proving the new behavior was internally consistent while the legacy regression contract was stale.
- Fixed the smoke assertions to require six rarity/filter entries instead of weakening or bypassing the test.

### Verification

GitHub Actions run **#111** (`Godot smoke test`) on commit `00380c75e3d8416c4510b635011982febcef780a` completed successfully.

Verified in that run:

- PR-A backup / build identity safety: PASS
- PR-B pause / filter / atomic-save safety: PASS
- Gameplay V3 balance contracts: PASS
- Existing gameplay smoke test: PASS
- Baseline capture fixture: PASS
- Exported runtime resource verification: PASS
- Windows export: PASS
- Web export: PASS

Automated contracts include:

- Epic probability below 0.02% of generated field items at floor 31 / rebirth 5.
- Rarity ordering remains Unique > Legendary > Epic by probability.
- Normal-enemy armor penetration remains bounded.
- Boss penetration/minimum chip damage exceed normal-enemy values.
- A full-health player cannot be one-shot by one normal hit under the tested profile.
- Boss hits are materially stronger while still bounded.
- Sell All preserves locked Epic gear and credits the dynamic sale value.
- Inventory UI exposes six pickup-filter choices and expanded detail space.

### Before / After

| Area | Before | After |
|---|---|---|
| Highest rarity | Legendary | Epic above Legendary |
| Top-tier chase | Legendary still appeared too often | Epic is an ultra-rare chase tier; Legendary also remains rare |
| Normal enemy hit | Could spike into one-hit deaths late game | Single normal hit bounded to 32% max HP |
| Boss threat | Could feel weaker than random normal spikes | Dedicated stronger boss profile, up to 60% max HP per hit |
| Inventory cleanup | Repetitive individual/limited sale flow | One-click Sell All, locked gear protected |
| Sell economy | Old low stored sale values | Dynamic level/rarity/enhancement-based values |
| Item details | Cramped / clipping risk | Taller management and detail area |
| Rarity regression | Smoke test assumed 5 tiers | Smoke + balance tests enforce 6 tiers |

### Windows play approval

Automated CI is complete. Windows Godot 4.3 play validation was completed on 2026-09-25; fresh-character combat tuning remains a follow-up before treating early-progression balance as final.


### Windows play validation — 2026-09-25

User-tested on Windows + Godot 4.3 with F5 after syncing `dev/gameplay-v3-balance`.

Observed:

- Inventory: normal.
- Sell All: normal; locked-item protection works.
- Expanded detail panel: normal.
- Normal-enemy damage: acceptable on the current geared character; **fresh-character retest remains a follow-up** because strong equipment may mask early-progression tuning.
- Boss damage: acceptable on the current geared character; **fresh-character retest remains a follow-up** for the same reason.
- Auto-hunt / room movement: normal.
- Enhancement flow: enhancement level increments correctly.
- Skills: normal.
- Save / rebirth: normal.

Enhancement implementation was re-checked after play feedback:

- Enhancement modifies the item's **base stats**, not only the displayed +level.
- +10 base-stat multiplier: **1.75x**
- +20 base-stat multiplier: **2.75x**
- +30 base-stat multiplier: **4.25x**
- Random affix values are intentionally not multiplied by enhancement.
- A new regression assertion verifies that a +10 equipped weapon changes the live ATK stat.

### Deferred item-rarity rework

The current six-tier rarity model is temporary. The next dedicated item-system rework must migrate to the approved 12-tier order:

1. 하급 (Junk)
2. 일반 (Normal)
3. 매직 (Magic)
4. 희귀 (Rare)
5. 에픽 (Epic)
6. 고유 (Unique)
7. 유물 (Artifact)
8. 전설 (Legendary)
9. 고대 (Ancient)
10. 신화 (Mythic)
11. 신성 (Divine)
12. 초월 (Transcendent)

This is deliberately deferred until the item pass can update rarity data, weights, affix limits, colors, filters, drop presentation, economy, migration, and regression tests together rather than adding names without systems behind them.


## 2026-09-25 — G4.0 Elite Monsters

### Problem

- Normal waves are mechanically readable but lack occasional high-attention combat moments between boss floors.
- Auto-hunt needs rare enemies that are visible at a glance without requiring manual targeting or UI inspection.
- Elite rewards must feel better than normal enemies without undoing the intentionally scarce field-item economy.

### Cause

- Every non-boss enemy currently uses only its base enemy resource plus floor scaling.
- There is no wave-level modifier layer between normal monsters and bosses.
- Normal drop logic is intentionally only 0.8–2.0%, so simply multiplying every elite drop into a guaranteed item would flood long idle runs.

### Reference UX

- **Hero Siege:** elite/champion enemies create ARPG texture between bosses through modifiers and better rewards.
- **Survivor.io:** dangerous targets are readable immediately during crowded automated combat.
- **AFFIX: ZERO direction:** elite identity must survive x5 speed and auto-hunt without turning every room into visual noise.

### Decision

- Elites begin at floor 6 and never replace boss encounters.
- At most one elite can appear in a normal wave.
- Encounter chance starts at 12%, scales with floor/rebirth, and caps at 35%.
- Ship three readable archetypes:
  - **폭군 (Brutal):** HP/attack-focused.
  - **질풍 (Swift):** movement/attack-speed-focused.
  - **철벽 (Bulwark):** HP/defense-focused.
- Elites use a larger sprite footprint plus a pulsing two-ring aura in their affix color.
- XP/gold rewards scale to roughly 2.4–2.8x normal.
- Extra equipment is a separate 18–30% bonus roll, preserving normal field-drop scarcity.

### Implementation

- Added elite affix definitions and runtime state to `EnemyAI.gd`.
- Extended enemy setup with an optional elite affix while explicitly excluding bosses.
- Added wave-level elite selection to `BattleManager.gd`.
- Added elite encounter probability contract with a hard 35% ceiling.
- Added elite bonus loot logic to `LootManager.gd`.
- Added `tests/gameplay_v4_elite.gd/.tscn` and CI coverage.
- Build identity advanced to **g4.0**.

### Failure / Revision

- No player-facing revision recorded yet. Automated validation and Windows play are required before this stage is considered accepted.

### Verification

Automated contract targets:

- No elites below floor 6.
- 12% starting encounter chance and 35% maximum.
- Rebirth increases encounter frequency without bypassing the cap.
- Brutal, Swift, and Bulwark create materially different combat profiles.
- Elite XP/gold exceeds normal rewards.
- Bosses cannot accidentally receive normal elite affixes.
- Elite bonus item chance remains capped at 30%.
- Existing G3.3 enhancement test now also proves that enhancement changes the live equipped ATK stat.

### Before / After

| Area | Before | After |
|---|---|---|
| Mid-floor combat | Uniform normal waves | Rare elite encounter creates attention spikes |
| Enemy identity | Base monster behavior only | Brutal / Swift / Bulwark modifier identity |
| Readability | Same visual weight for normal enemies | Larger elite body + pulsing colored aura |
| Reward | Normal XP/gold/drop | 2.4–2.8x resources + restrained bonus gear roll |
| Boss separation | Bosses are the only special combat tier | Elites bridge normal waves and bosses without replacing boss floors |
| Regression | No elite contract | Dedicated Gameplay V4 elite CI contract |

### Windows play approval

Validated on Windows + Godot 4.3 on 2026-09-25.

Observed in live play:

- Brutal elites were encountered repeatedly and were immediately readable through the red pulse aura.
- A Bulwark Goblin was also encountered, confirming multiple affix archetypes spawn naturally.
- Elite silhouettes remained distinguishable in a crowded wave at the tested viewport.
- The player repeatedly returned to the same floor range because the next boss remained a progression wall; this exposed a useful follow-up: boss difficulty needs **more readable/fair patterns**, not simply more raw stats.
- No elite/boss overlap bug was observed in the tested run.

The supplied play captures show the elite aura clearly around enemies during normal auto-hunt. G4.0 is considered player-validated; reward-frequency tuning can continue opportunistically with longer idle runs.


## 2026-09-25 — G4.1 Direction Revision: Boss Patterns Rejected

### Problem

- G4.0 live play exposed a boss wall around the current high-floor progression.
- A first response was prototyped around telegraphed boss patterns and automatic evasion.

### Cause

- The boss wall was initially interpreted as a combat-readability problem.
- After reviewing the actual product loop, that interpretation conflicted with the game's core identity: the character should keep hunting on its own, accumulate resources and gear, grow stronger, and eventually clear the wall.

### Reference UX

- **Idle RPG progression:** a boss wall acts as a stat/progression check, not necessarily a dexterity check.
- **AFFIX: ZERO direction:** repeated hunting should create the resources and equipment needed to break through progression walls without requiring manual dodge mastery.

### Decision

- Reject the boss-pattern / auto-evade prototype before merge.
- Keep bosses as progression gates rather than pattern-learning encounters.
- Keep the existing defeat setback and boss stat profile for now.
- Redirect development effort toward the loop that actually resolves a wall:
  - hunt repeatedly,
  - collect gold/items,
  - improve skills/equipment,
  - retry automatically,
  - eventually break through.

### Implementation

- Draft PR #8 (Gameplay V4.1: 보스 텔레그래프와 자동 회피) was closed without merge.
- Its implementation branch is retained only as historical prototype evidence.
- No boss-pattern code is included in the accepted G4.0 line.

### Failure / Revision

- The prototype was technically viable but product-directionally wrong.
- This is an intentional rejection, not a failed implementation: adding more combat mechanics would have increased complexity while weakening the idle-game identity.

### Verification

- Decision is based on the Windows G4.0 play session where elites worked as intended and the boss behaved as a progression wall.
- The accepted branch remains dev/gameplay-v4-elites; no boss-pattern changes were merged.

### Before / After

| Area | Prototype direction | Accepted direction |
|---|---|---|
| Boss wall | Read/evade special patterns | Grow stats until the wall breaks |
| Player attention | Watch and interpret hazards | Let auto-hunt continue producing growth |
| Failure response | Shortened retry + evade logic | Preserve progression pressure |
| Development priority | More combat mechanics | Better farming / upgrade / equipment UX |
| Idle identity | Weakened | Preserved |


## 2026-09-25 — G4.1 Idle Growth Loop UX

### Problem

- Live G4.0 play confirmed that a boss wall is better treated as a progression check than as a manual-action challenge.
- The existing growth systems already work, but the UI hides too much useful information behind tooltips and separate panels.
- Epic was added to the data model, but the inventory rarity badge map still lacked an Epic entry and could visually fall back to the wrong label.

### Cause

- Equipment cards only showed a generic 강화 button; success rate, cost and real stat gain were not visible at a glance.
- The equipment summary showed combat stats but not whether the player currently had affordable growth actions.
- The skill panel showed per-row costs but did not summarize how many skills could be upgraded with the current gold.
- Boss defeat copy described only the setback, not the intended idle loop of farming and automatic retry.

### Reference UX

- **Idle RPGs:** a progression wall should immediately answer 'what can I improve now?' without demanding continuous manual control.
- **Survivor.io:** growth choices are legible and reward loops are communicated quickly.
- **AFFIX: ZERO:** farming remains automatic; the management UI should make converting accumulated gold/items into power low-friction and obvious.

### Decision

- Keep the existing boss stat-check model and defeat setback unchanged.
- Make equipment enhancement affordability, next success rate and next enhancement level visible directly on each equipped-item card.
- Show real next-step base-stat gains in the enhancement tooltip rather than only a generic risk string.
- Add equipment/skill affordable-growth counts to the equipment summary.
- Add current gold + affordable skill option count to the skill-panel header.
- Change boss defeat messaging to explicitly frame the loop as 'retreat, farm, automatically retry'.
- Fix the Epic inventory badge mapping.

### Implementation

- Added GameManager.equipment_enhancement_preview() with target level, cost, odds, destruction/downgrade risk, multiplier change and real base-stat gains.
- Added affordable equipment-slot and affordable skill-option helpers plus a growth summary contract.
- Equipment cards now surface success probability and gold cost before clicking.
- Enhancement buttons display the next target level and disable when the current gold cannot afford the attempt.
- Equipment summary now includes affordable equipment/skill growth counts.
- Skill header now includes current gold and number of skills currently affordable to upgrade.
- Epic inventory slots now display 에픽 instead of falling back to 일반.
- Boss defeat notification now communicates 파밍 후 자동 재도전 while preserving the existing mechanics.
- Added dedicated gameplay_v4_growth_ux regression tests and CI coverage.
- Build identity advanced to **g4.1**.

### Failure / Revision

- The immediately preceding G4.1 boss-pattern prototype was rejected before merge because it weakened the idle identity.
- This revision redirects the same boss-wall problem toward progression readability instead of adding combat execution complexity.

### Verification

Automated contracts cover:

- Enhancement preview points to the actual next level.
- +1 preview exposes the true base-stat multiplier change and resulting ATK delta.
- Enhancement cost and success rate are included in the preview contract.
- Max-level gear returns a terminal preview state.
- Growth summary detects affordable equipped-item enhancement and class-skill growth.
- Epic has an explicit player-facing inventory badge.
- Very low probabilities such as 0.3% remain visible instead of rounding to 0%.
- Enhancement tooltip text exposes the actual stat gain.

### Before / After

| Area | Before | After |
|---|---|---|
| Boss wall response | Considered adding action patterns | Preserve idle stat gate and improve growth conversion UX |
| Equipment card | Generic 강화 button | Next +level, success odds and cost visible |
| Enhancement value | Multiplier/risk mostly hidden | Real next base-stat gain exposed |
| Growth awareness | Player manually checks every panel | Equipment summary shows affordable growth counts |
| Skills | Individual costs only | Current gold + affordable skill count at top |
| Epic badge | Missing mapping / fallback risk | Explicit 에픽 badge |
| Boss failure copy | Generic retreat notice | 파밍 후 자동 재도전 loop communicated |

### Windows play approval

Pending. Validate that equipment cards remain readable, low success rates are not visually clipped, skill/equipment growth counts update immediately after spending gold, and the boss defeat message appears without changing the existing automatic retreat/retry behavior.
