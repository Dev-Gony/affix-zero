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

Validated on Windows + Godot 4.3 on 2026-09-25.

Observed in live play:

- Equipment enhancement controls correctly disable when gold is insufficient.
- Enhancement success-rate / cost display was visible and readable in the equipment grid.
- Skill growth header and upgrade state were confirmed in live play.
- Boss defeat preserved the existing one-floor retreat and automatic farming/retry loop while showing the new progression-oriented message.
- The enlarged equipment layout remained readable without problematic clipping.

G4.1 growth-loop UX is considered player-validated. PR remains unmerged by policy.


## 2026-09-25 — G4.2 Rarity-Scaled Loot Feedback

### Problem

- Field equipment drops were mechanically functional but visually too similar across rarity tiers.
- In a mostly automated game, rare rewards need to be recognizable during passive observation without opening the bag.
- Boss and elite bonus equipment entered the inventory immediately, so the strongest reward moments could happen with little or no world-space celebration.

### Cause

- The existing drop effect used one short icon bounce and a thin fixed beam for every rarity.
- Only Legendary triggered a global flash, and Epic had no stronger presentation despite being the chase tier above Legendary.
- Boss/elite equipment rewards bypassed the field-drop visual path because they were collected directly.

### Reference UX

- **Survivor.io:** drops use immediate, readable reward feedback during crowded combat.
- **ARPG loot language:** beam height, width, color and persistence communicate value before the player reads item text.
- **AFFIX: ZERO:** because drops are intentionally scarce, strong visual hierarchy can be used without turning every wave into a fireworks display.

### Decision

- Scale drop presentation by rarity rather than giving every item the same visual weight.
- Keep Normal restrained and progressively increase beam height, width and duration through Magic, Rare, Unique, Legendary and Epic.
- Give Rare+ an expanding ring, Unique+ additional particles, Legendary a screen flash, and Epic a stronger flash plus the tallest/widest beam.
- Include `[등급] 아이템명` in the world label.
- Reuse the same celebration for direct Elite and Boss equipment rewards.
- Do not change drop probabilities, loot-filter logic or item stats in this stage.

### Implementation

- Added deterministic loot-visual profile helpers to `EffectLayer.gd`.
- Light-pillar height now rises from a subtle Normal marker to a 100px+ Epic pillar.
- Beam width and effect duration also scale by rarity.
- Rare/Unique/Legendary/Epic tiers add progressively stronger rings, particles and screen feedback.
- Boss and Elite equipment rewards now call the same world-space drop presentation even though the item is already collected.
- Added dedicated `gameplay_v4_loot_feedback` tests and CI coverage.
- Build identity advanced to **g4.2**.

### Failure / Revision

- During implementation review, the old premium screen flash was found to cover only the original 640x356 origin area. On later dungeon rooms the camera could be nowhere near that rectangle, making the flash effectively invisible.
- Expanded the flash overlay across the full dungeon world and moved premium burst particles to the actual item position.
- Epic originally inherited the normal item-drop sound because the audio branch checked only the literal Legendary rarity id. Premium audio now keys off rarity index 4+ so Legendary and Epic both use the premium cue.
- Windows visual validation is still pending for clutter and x5 readability.

### Verification

Automated contracts cover:

- Every higher rarity has a taller beam than the tier below it.
- Higher rarity never has a shorter celebration duration.
- Normal remains visually restrained.
- Epic uses a 100px+ light pillar.
- Unique and below do not flash the full screen.
- Legendary flashes the screen; Epic flashes more strongly.
- Epic beam width is materially larger than Rare.
- `show_drop()` preserves rarity index, creates an icon/beam payload, primes Epic flash and includes the player-facing rarity name in the drop label.

### Before / After

| Area | Before | After |
|---|---|---|
| Normal drop | Same basic presentation | Small, short marker |
| Rare/Unique | Mostly color difference | Taller beam + ring/particles |
| Legendary | Gold-ish flash + same beam size | Strong red-tier beam + flash |
| Epic | No unique premium treatment | Tallest/widest purple pillar + strongest flash |
| Boss/Elite gear | Inventory reward with weak world feedback | Same rarity-scaled celebration in the field |
| Player reading | Check bag/notification | Rarity visible immediately in combat |

### Windows play approval

Pending. Validate that Normal/Magic do not create clutter, Rare/Unique are noticeable, Legendary/Epic feel exceptional, and x5 combat still remains readable.


## 2026-09-25 — UIUX V2.0 Full Management Overhaul

### Problem

- The existing UI was functionally complete but visually read like a developer/admin overlay rather than a finished game interface.
- Equipment, inventory, skills, rebirth and information all shared the same narrow side-modal treatment, so important systems lacked distinct hierarchy.
- The equipment screen compressed seven slots, the character portrait, growth information and actions into tiny 3x3 cards.
- The inventory stacked the item grid above the detail pane, forcing the player to scan vertically instead of comparing gear and details at the same time.
- The brown/red border language competed with rarity colors and made nearly every panel look equally urgent.
- The bottom dock, modal window, HUD and notifications all remained visually active at once, creating hierarchy noise.

### Cause

- The original UI was built incrementally around a 316px-wide management popup.
- New features were added inside the existing shell instead of revisiting the shell itself.
- The same compact card patterns were reused for desktop management even after inventory, enhancement and skill systems became denser.
- Navigation treated the management window as a secondary popup, even though it had become the main place where players convert idle farming into progression.

### Reference UX

- **Survivor.io / 탕탕특공대**
  - Character-centered equipment composition.
  - Strong rarity-colored equipment cards.
  - Dense inventory grid with clear upgrade affordances.
  - Management screens feel like primary game surfaces, not debug popups.
- **Hero Siege**
  - Dark desktop-first management panels.
  - Dense information without losing readability.
  - Strong tab/system separation and detailed item inspection.
- **AFFIX: ZERO direction**
  - Keep the dark ARPG mood and information density of Hero Siege.
  - Borrow the immediate equipment/inventory hierarchy and reward readability of Survivor.io.
  - Do not copy either UI literally; preserve the game's own pixel-dungeon identity.

### Decision

- Replace the narrow right-side management popup with an almost full-width desktop management hub.
- Keep combat running behind a stronger dim layer, preserving the idle-game identity while making management the visual focus.
- Add dedicated in-window top navigation for 장비 / 가방 / 스킬 / 환생 / 정보.
- Hide the compact bottom dock while the management hub is open and restore it after closing.
- Keep the character portrait at the visual center of the equipment composition and enlarge all equipment cards.
- Rebuild inventory as an **8-column grid + persistent right-side detail/comparison pane**.
- Use slate/blue-black structural colors and reserve gold for selection/progression so rarity colors remain meaningful.
- Make skills and rebirth visually communicate progression through bars, not text alone.
- Show a green inventory upgrade arrow only when an item is a clean visible-stat improvement with no visible downgrade.
- Fix ESC behavior so closing management does not immediately open the pause menu.

### Implementation

- Combat HUD:
  - Removed always-visible 저장 / 종료 actions from active combat.
  - Replaced them with one 메뉴 entry that opens pause/settings, where save/load/quit already belong.
  - Reallocated HUD width to run-state and speed controls so active combat reads more like a game HUD and less like a debug toolbar.
- Management hub expanded to roughly the full 640px desktop canvas:
  - 612x346 primary panel.
  - Dedicated header with section title, current gold and close affordance.
  - Persistent five-section navigation row.
  - 596px-wide content region.
- Combat dock is now compact and visible only when management is closed.
- Equipment:
  - 3x3 centered composition preserved, but cards expanded from ~88px to ~176px.
  - Character portrait stays in the center cell.
  - Larger equipment icons, labels and enhancement actions.
  - Strong rarity border color while structural chrome remains neutral.
- Inventory:
  - 8-column scrollable item grid.
  - Persistent 188px detail/comparison pane on the right.
  - Larger item cells and clearer filter/sell controls.
  - Strict-upgrade green arrow derived from the existing comparison system.
- Skills:
  - Added visible level progress bars.
  - Increased card/action widths for desktop readability.
- Rebirth:
  - Added explicit level-to-rebirth progress bar.
- Theme:
  - Replaced most brown/red structural chrome with slate/blue-black panels.
  - Gold is used for selected/progression states rather than every border.
- Class selection:
  - Enlarged all six class cards.
  - Unified card chrome with the V2 slate/gold language while preserving each class color as identity.
  - Reframed the screen around "자동사냥 성장형 ARPG" rather than a generic selection modal.
- Interaction:
  - ESC closes management only; a second ESC can then open pause/settings.
  - Top management navigation switches tabs without closing the hub.
- Added a dedicated `uiux_v2_overhaul` regression contract.
- Build identity advanced to **uiux-v2.0**.

### Failure / Revision

- The previous incremental UI approach kept solving local clipping problems while preserving the larger hierarchy problem.
- During V2 implementation, the old ESC flow was identified as a UX bug: pressing ESC while management was open closed management **and immediately opened pause/settings** in the same keypress.
- V2 changes ESC to perform one state transition per keypress.
- The first V2 CI run failed because the older PR-B safety test still encoded the previous double-transition behavior as the expected contract.
- The regression was deliberately updated, not bypassed: first ESC closes management while idle combat keeps running; a second ESC explicitly opens the modal pause menu.
- The next CI pass exposed an older G3.3 layout contract requiring at least 350px management height. V2 had landed at 346px, so the hub was increased by 4px rather than weakening the existing full-detail readability guard.
- The following full smoke run then failed because it still hard-coded the pre-V2 **six-column inventory grid**. V2 intentionally uses an eight-column desktop grid with a persistent detail pane, so the legacy smoke contract was updated from six to eight columns rather than shrinking the new layout back to fit an obsolete assumption.
- The redesign intentionally does not replace the existing game art or create a new asset pack yet; layout/hierarchy must be validated first before spending time on decorative art.

### Verification

Automated contracts cover:

- Management hub width >= 600px and height >= 340px.
- Hub positioned as a primary screen rather than a side popup.
- Five in-window navigation buttons exist.
- Inventory uses eight columns.
- Inventory maintains a persistent detail pane >= 170px wide.
- Character portrait remains in the equipment layout center.
- Opening management hides the compact combat dock.
- Switching top tabs does not close management.
- Closing management restores the compact dock.
- Equipment cards remain >= 170px wide.
- All six class choices remain visible in one screen with desktop-sized cards >= 180px wide.

### Before / After

| Area | Before | UIUX V2 |
|---|---|---|
| Combat HUD | Save/quit mixed into active combat controls | Run state + speed + one menu entry |
| Management shell | 316px right-side popup | 612px primary desktop hub |
| Navigation | Bottom dock only | In-window top navigation + compact closed-state dock |
| Equipment | Tiny 3x3 admin-like cards | Large character-centered loadout composition |
| Inventory | Grid stacked above details | 8-column grid + persistent right detail pane |
| Skills | Text-heavy rows | Level bars + roomier actions |
| Rebirth | Text summary + buttons | Explicit progression bar + permanent upgrades |
| Color hierarchy | Brown/red chrome everywhere | Slate structure, gold progression, rarity colors reserved for loot |
| ESC behavior | Close management then open pause immediately | One state transition per keypress |
| Class selection | Separate older brown/red visual language | Same slate/gold hierarchy as management V2 |
| Overall read | Functional debug overlay | Dedicated game management surface |

### Windows play approval

Pending. The critical review is now visual rather than mechanical: panel scale, information hierarchy, equipment/inventory scanning speed, management-vs-combat separation, and whether the larger desktop hub feels like a finished game screen rather than a developer overlay.


## 2026-09-25 — UIUX V2.1 Playtest Polish

### Problem

- Windows playtest confirmed the new full-width management shell works, but several secondary screens still looked like stretched developer panels.
- The Info tab was a large raw text wall with duplicated save/load/quit actions even though those actions had already moved to the pause menu.
- The Rebirth tab used only the top-left portion of the available desktop canvas, leaving large dead space around a 2x2 button block.
- An empty inventory could look broken or content-less because the UI did not explain that a strict acquisition filter such as Epic-only was active.
- While management correctly leaves idle combat running, the dimmed background made that rule easy to miss.

### Cause

- V2.0 primarily solved shell/layout hierarchy first; several old tab internals were simply enlarged inside the new container.
- Save/quit utilities remained duplicated in Info from the pre-overhaul information architecture.
- Rebirth and statistics were still text-first layouts designed for the old narrow popup.
- Empty-state copy described only what would happen after acquiring an item, not why the current inventory could remain empty.

### Reference UX

- **Survivor.io:** progression screens use distinct cards and large at-a-glance status blocks instead of raw system text.
- **Hero Siege:** dense statistics are grouped by system so the player can scan categories without reading one long paragraph.
- **AFFIX: ZERO:** management must clearly communicate that auto-hunt is still running in the background because this is an idle-first ARPG.

### Decision

- Keep the V2.0 combat HUD, primary management shell, equipment composition and 8-column inventory structure.
- Convert Info into a four-card dashboard: adventure history, combat stats, current-floor threat and auxiliary bonuses.
- Remove duplicate save/load/quit actions from Info; keep those utilities only in the pause/settings menu.
- Rebuild Rebirth around a large progression card plus one full-width row of four permanent-upgrade cards.
- Surface the active loot acquisition filter directly in the inventory header and empty-state copy.
- Add a green 자동사냥 계속 indicator to the management header.
- Give equipped item cards a subtle rarity-tinted background and stronger rarity border while keeping structural chrome neutral.
- Enrich the center equipment portrait with current level and rebirth count.

### Implementation

- Management header now includes ● 자동사냥 계속.
- Equipment cards use a subtle rarity tint and 2px rarity border when occupied.
- Character portrait now shows class, current level and rebirth count.
- Inventory header includes current acquisition threshold, e.g. 획득 에픽만.
- Empty inventory detail now explicitly explains the active loot filter and that lower tiers are skipped.
- Info tab now uses a 2x2 dashboard card layout and no longer duplicates save/load/quit.
- Rebirth tab now uses:
  - large rebirth/progression card,
  - current permanent point count,
  - explicit levels remaining until rebirth,
  - full-width four-card permanent growth row.
- Added V2.1 regression contracts for management-live state, info dashboard, rebirth grid and empty-inventory filter context.
- Build identity advanced to **uiux-v2.1**.

### Failure / Revision

- The screenshots showed that V2.0 solved the major shell problem but exposed a second-order issue: enlarging old content does not automatically make it feel designed for the new canvas.
- V2.1 therefore changes content hierarchy rather than merely adding more spacing or decoration.
- The first V2.1 CI run then exposed one more legacy contract: Gameplay V3 still reached directly into the removed raw `_stats_label`. The product behavior was intact, but the test was coupled to the old widget implementation. The contract was migrated to the new current-floor threat dashboard label and still verifies the same player-facing information.

### Verification

Automated contracts cover:

- Management header states that auto-hunt continues.
- Info contains exactly four dashboard cards in a 2-column layout.
- Rebirth contains four permanent-growth cards in one row.
- Epic-only acquisition state is visible in the inventory header.
- Empty inventory explains the active acquisition filter.
- Existing full-width hub, 8-column inventory, center portrait and management-open/close behavior remain intact.

### Before / After

| Area | V2.0 playtest | V2.1 |
|---|---|---|
| Info | Large raw text wall + duplicate utilities | Four category cards, menu owns utilities |
| Rebirth | Top-left content + large dead area | Progression hero card + four full-width upgrades |
| Empty inventory | Looks merely empty | Explains active acquisition threshold |
| Management state | Combat continues but subtly | Explicit green idle-combat indicator |
| Equipment rarity | Border-only emphasis | Subtle rarity surface tint + stronger border |
| Center portrait | Class only | Class + level + rebirth context |

### Windows play approval

Pending. Validate card density, text wrapping, permanent-upgrade row width, Info readability and whether the management-live indicator is helpful rather than distracting.

## 2026-09-25 — UIUX V2.1 Visual Hierarchy Polish

### Problem

- The V2 overhaul fixed the screen structure, but the first Windows capture still looked one step short of a finished game UI.
- Equipment cards used full rarity-colored frames, so a screen full of high-rarity gear became a wall of red outlines instead of a readable loadout.
- Skill rows devoted a large square to text-only pseudo-icons, making the screen feel like a placeholder implementation despite otherwise solid information layout.
- Rebirth growth still underused horizontal space and did not visually prioritize permanent upgrades strongly enough.

### Cause

- V2 correctly focused on information architecture first, but several visual treatments were inherited from earlier debug/admin cards.
- Rarity signaling was applied to the whole equipment frame instead of being concentrated into a smaller accent.
- Skill cards reserved icon real estate before real skill art existed, which exaggerated the unfinished look.

### Reference UX

- **Survivor.io:** rarity is loud on the item itself, while the surrounding management structure remains stable.
- **Hero Siege:** dense progression screens use restrained structural chrome so stats and item identity carry the visual weight.
- **AFFIX: ZERO:** placeholder visuals should be removed rather than enlarged until real dedicated art exists.

### Decision

- Keep the V2 layout and navigation intact.
- Replace full rarity-colored equipment frames with a neutral structural border plus a slim rarity accent strip and rarity-colored item name.
- Remove text-only square skill pseudo-icons and replace them with a narrow gold progression accent.
- Increase the width of skill purchase controls for better desktop legibility.
- Strengthen the rebirth permanent-upgrade row so all four upgrades read as a first-class full-width system.
- Keep this pass visual only; no balance, progression or save behavior changes.

### Implementation

- Equipment cards now use neutral slate borders, subtle rarity-tinted backgrounds and a 4px rarity strip.
- Skill rows no longer render large text-only placeholder icon tiles.
- Skill actions widened from 104px to 118px and card spacing simplified.
- Rebirth permanent upgrades now receive a dedicated header/hint and stronger gold affordance when points are available.
- Management screens now hide both the top combat HUD and bottom dock, move to y=8 and expand to 384px height so they read as dedicated game surfaces rather than overlays stacked on active combat chrome.
- UIUX regression contracts now verify restrained equipment accents, slim skill accents, four-column permanent upgrades and the two-column info dashboard.
- Build identity advanced to **uiux-v2.1**.

### Failure / Revision

- The first V2 Windows capture confirmed that layout correctness alone did not remove the admin-tool impression.
- Rather than adding more decoration, V2.1 removes visually noisy or obviously provisional elements first.

### Verification

Automated contracts cover:

- Equipment cards keep desktop-scale width.
- Equipment rarity accent remains narrow instead of becoming a full-frame alarm color.
- Skill cards use a slim accent rather than a large placeholder block.
- Rebirth permanent upgrades remain a four-column desktop grid.
- Information screen remains a two-column dashboard rather than reverting to a text wall.

### Before / After

| Area | V2 capture | V2.1 direction |
|---|---|---|
| Equipment rarity | Full bright frame on every high-rarity card | Neutral card + focused rarity strip/name |
| Skill identity | Large text-only pseudo-icon block | Slim progression accent until real skill art exists |
| Skill actions | Narrow utility controls | Wider desktop purchase controls |
| Rebirth growth | Functional but visually secondary | Dedicated permanent-growth header + stronger affordance |
| Management framing | Combat HUD still visible behind the management shell | Dedicated near-fullscreen management surface; combat chrome hidden |
| Overall impression | Good structure, still slightly tool-like | Less debug chrome, stronger game-screen hierarchy |

### Windows play approval

Pending. Validate whether the loadout feels calmer, skill rows lose the placeholder feel, and permanent growth reads more clearly without increasing visual clutter.


## 2026-09-25 — UIUX V2.2 Final Polish

### Problem

- V2.1 Windows captures confirmed the new structure and visual hierarchy, but a few states still looked unfinished.
- An empty bag still rendered as a large field of blank slots, which read like missing content rather than an intentional state.
- Progression actions used nearly identical button styling whether they were affordable, premium or blocked.
- Rebirth progress used the same warning-like accent color even when the player had already met the requirement.

### Cause

- V2.1 focused on removing visual noise and placeholder elements, not on semantic state feedback.
- The UI theme treated most actions uniformly and left context to text alone.

### Reference UX

- **Survivor.io:** empty/locked states explain what the player should expect next instead of presenting dead space.
- **ARPG management screens:** upgrade-ready actions are visually distinguishable from utility actions without requiring the player to read every cost.
- **AFFIX: ZERO:** because play is automated, management screens should make 'what can I do now?' obvious at a glance.

### Decision

- Preserve the validated V2.1 layout.
- Add a centered, filter-aware empty-bag message over the slot grid.
- Give affordable equipment/skill upgrade buttons a restrained gold affordance.
- Give MAX skill spending and ready-to-rebirth actions a green affordance.
- Use semantic gold/green progress colors for rebirth instead of warning red.
- Keep destructive/utility actions neutral.

### Implementation

- Added `_inventory_empty_hint` with the active loot-filter context.
- Affordable equipment enhancement buttons now receive gold border emphasis.
- Affordable +1 skill upgrades use gold emphasis; MAX spending uses green emphasis.
- Rebirth progress uses gold while progressing and green when the requirement is met.
- Ready-to-rebirth button receives a green primary action treatment.
- Added a UIUX regression check for the filter-aware empty inventory state.
- Build identity advanced to **uiux-v2.2**.

### Failure / Revision

- No new layout was introduced. V2.2 deliberately avoids another redesign after V2.1 already solved the structural problems.
- The pass is limited to semantic affordance and empty-state polish so the UI does not enter an endless redesign loop.

### Verification

Automated contracts cover:

- Empty inventory still reports the active loot filter.
- Empty inventory now exposes a centered filter-aware message instead of only blank slots.
- Existing V2/V2.1 layout, navigation, rarity-strip and full-screen-management contracts remain active.

### Before / After

| Area | V2.1 | V2.2 |
|---|---|---|
| Empty bag | 60 blank slots + explanation at right | Centered filter-aware message over the grid |
| Affordable enhancement | Same visual weight as utility actions | Gold action affordance |
| MAX skill spend | Same visual weight as other buttons | Green growth affordance |
| Rebirth progress | Generic accent | Gold while progressing, green when ready |
| Rebirth action | Text-only readiness | Green primary action when available |

### Windows play approval

Pending. Validate that the empty bag no longer feels broken, affordable growth actions stand out without becoming noisy, and ready-to-rebirth state reads immediately.


## 2026-09-25 — G5.0 Pet System + Item Art Pipeline

### Problem

- The project had reached a much stronger UI structure, but the actual game layer still lacked a second progression axis beyond character/gear/skills.
- The combat fantasy reference direction called for a visible companion system, while the current equipment pipeline was still tightly coupled to one shared atlas index.
- Replacing every item visually later would require touching UI/combat code repeatedly unless the data model first accepted per-item art.

### Cause

- There was no persistent pet catalog, no active-pet state, no combat companion node, no pet progression save contract and no pet management screen.
- Generated equipment dictionaries only carried `icon_index`, so art replacement was effectively locked to the existing 6x5 atlas.

### Reference UX

- **Survivor-style progression:** pets/companions create another clear long-term growth lane while combat remains automatic.
- **ARPG equipment identity:** individual items need strong art identity instead of looking like anonymous atlas cells.
- **AFFIX: ZERO:** the pet should fight and support automatically, preserving the idle-first direction rather than becoming another manual-control system.

### Decision

- Add pets as a fully automatic combat/progression system.
- Launch with six companions and one guaranteed starter pet.
- Unlock additional pets by floor progression for the first implementation rather than adding gacha/currency immediately.
- Give pets autonomous attacks, optional periodic healing support, XP/levels, active selection and persistence.
- Add a dedicated Pet management tab between Skills and Rebirth.
- Add `icon_texture_path` / generated `icon_path` support so every equipment base can move to dedicated sprite art without rewriting inventory/equipment UI.

### Implementation

- Added `PetData` Resource schema and six launch pets:
  - 청월호 / spirit fox
  - 화염룡 / ember drake
  - 유령 슬라임
  - 수호 골렘
  - 밤그림자 박쥐
  - 초원의 요정
- Added `PetManager` autoload:
  - catalog loading
  - owned roster
  - active pet
  - levels / XP / stars
  - floor unlocks
  - attack/support scaling
  - save/load payload
- Added `PetCompanion` world node:
  - follows the player automatically
  - hides outside active gameplay
  - renders a distinct procedural pixel silhouette per pet until dedicated sprites replace it
- Added BattleManager integration:
  - automatic pet attacks
  - pet support healing
  - pet XP from enemy kills
  - floor-based pet unlock notifications
- Added pet combat VFX for projectiles/impact and healing feedback.
- Added Pet tab to the management hub and keyboard shortcut 6.
- Added reusable `PetPortrait` UI renderer.
- Added per-item sprite-path support while retaining atlas fallback compatibility.
- Added dedicated G5 pet/item-art CI contracts.

### Failure / Revision

- The first reaction to the graphics push was to generate visual mockups instead of shipping playable systems. That was the wrong execution order.
- G5.0 corrects that by treating the mockup only as direction and moving immediately to persistent gameplay code and data contracts.
- Dedicated binary sprite assets are intentionally not faked into the repo from screenshot mockups. The item/pet code now supports real dedicated art assets cleanly when those are produced.

### Verification

Automated contracts cover:

- six-pet catalog availability
- guaranteed starter pet ownership/selection
- autonomous attack contribution and cadence
- starter support healing
- pet level/star/active state persistence
- floor-progression unlocks
- visible companion binding to the player
- generated equipment carrying an overrideable art path
- item base resources accepting dedicated sprite paths

### Before / After

| Area | Before | After |
|---|---|---|
| Companion system | None | Persistent active pet with auto combat/support |
| Pet progression | None | XP, level, stars, floor unlock roster |
| Combat presence | Player + enemies only | Player + visible following combat companion |
| Management | 5 growth tabs | 6 tabs including dedicated Pet screen |
| Item artwork | Shared atlas index only | Dedicated sprite-path override + atlas fallback |
| Save data | Character/gear/progression only | Pet roster + active companion persisted |

### Windows play approval

Pending. Validate follower movement, pet attack readability at x1/x5, healing feedback, pet tab switching, floor unlock behavior and save/load persistence before merge.
