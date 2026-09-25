# AFFIX: ZERO Portfolio Capture Checklist

This checklist is for **real Windows Godot 4.3 play captures**, not headless fixtures. Capture only after pulling the exact branch/commit and launching the project with **F5**.

## G3.3 — Epic / Economy / Damage Readability

### Required capture set

- [ ] **Inventory overview — six rarity filter choices**
  - Open the inventory management window.
  - Show the pickup filter with Normal / Magic / Rare / Unique / Legendary / Epic choices.
  - Capture the expanded item-detail area in the same frame if possible.

- [ ] **Sell All — locked item protection**
  - Put at least two unlocked items and one locked item in the bag.
  - Capture before pressing **전체 판매**.
  - Press **전체 판매**.
  - Capture after sale showing the locked item still present and the gold increase/notification.

- [ ] **Expanded item details**
  - Select an item with multiple stats/affixes.
  - Capture the full details, enhancement information, sale value, and equipment comparison without bottom clipping.

- [ ] **Epic rarity evidence**
  - If an Epic item naturally drops during play, capture the field drop and inventory detail.
  - Do **not** grind solely to force this screenshot. Epic is intentionally ultra-rare.
  - CI probability evidence is acceptable until a natural player-facing Epic capture exists.

- [ ] **Normal-enemy damage readability**
  - At a sufficiently high floor, capture/video several normal-enemy contacts.
  - Confirm one normal hit does not erase a full-health character.
  - Record whether repeated hits still feel dangerous rather than harmless.

- [ ] **Boss threat readability**
  - Capture/video a boss encounter at a comparable progression point.
  - Confirm boss hits are visibly more threatening than normal-enemy hits.
  - Note if the boss still feels too weak/too strong despite the automated bounds.

### Optional portfolio evidence

- [ ] Enhancement panel showing +10 / +20 / +30 success-risk information.
- [ ] High-floor normal vs boss HP loss shown side-by-side in a short clip.
- [ ] Inventory Before/After pair emphasizing reduced cleanup friction.
- [ ] Epic / Legendary visual comparison once a natural Epic drop exists.

## G3.3 validation notes

Record after Windows play:

- Build commit: 8bf5186 lineage / G3.3 branch
- Floor / rebirth: existing geared save
- Character / class: existing geared character
- Normal-enemy hit feel: Normal on geared character; fresh-character retest required
- Boss hit feel: Normal on geared character; fresh-character retest required
- Sell All result: PASS
- Inventory detail clipping: PASS
- Enhancement level increment: PASS; live stat application confirmed by code + regression test
- Auto-hunt / movement: PASS
- Skills: PASS
- Save / rebirth: PASS
- Epic seen naturally: Not required
- Bugs found: none in tested G3.3 flows
- Merge approval: gameplay validated; early-progression damage tuning still follow-up

## Portfolio story to preserve

**Problem → Cause → Reference UX → Decision → Implementation → Failure/Revision → Verification → Before/After**

For G3.3, the important story is not merely “added Epic.” The portfolio evidence should show that rarity depth, combat readability, and inventory economy were changed together, then guarded by regression tests after the sixth rarity exposed a stale five-tier smoke assumption.


## G4.0 — Elite Monsters

### Required capture set

- [ ] **Elite first-read**
  - Reach floor 6+ and capture the first elite encounter.
  - The elite should be distinguishable from normal enemies without reading a menu.
  - Capture the larger body and pulsing colored aura.

- [ ] **Three elite archetypes**
  - Capture Brutal / Swift / Bulwark when naturally encountered.
  - Brutal should feel damage-focused, Swift should move/attack faster, Bulwark should take longer to kill.

- [ ] **x5 readability**
  - Run at x5 speed in a crowded wave.
  - Confirm the elite aura remains visible without obscuring attacks or normal enemy silhouettes.

- [ ] **Reward feel**
  - Compare normal and elite XP/gold pickups.
  - Record whether the elite feels worth noticing even when no bonus equipment drops.

- [ ] **Boss separation**
  - Reach a boss floor after seeing elites.
  - Confirm bosses remain visually and mechanically distinct and never show normal elite affixes.

### G4.0 validation notes

- Build commit: 0efce28 lineage / G4.0
- Floor / rebirth: high-floor geared save, around floor 49
- Elite frequency: Good enough for live validation; longer idle-run tuning remains optional
- Brutal feel: PASS; encountered repeatedly
- Swift feel: not captured in this short validation session
- Bulwark feel: PASS; Bulwark Goblin encountered naturally
- x5 readability: aura/silhouette remains readable in crowded combat
- Reward feel: no blocking issue observed
- Boss separation: no elite-affix overlap observed
- Bugs found: none in tested elite flow
- Merge approval: gameplay validated; PR remains unmerged by policy


## G4.1 — Idle Growth Loop UX

### Required capture set

- [ ] **Equipment growth at a glance**
  - Open 장비.
  - Capture at least one equipped item whose next enhancement is affordable.
  - Confirm success rate and cost are visible without opening a tooltip.
  - Confirm the 강화 button shows the next +level.

- [ ] **Low-probability enhancement readability**
  - Inspect a high enhancement milestone if available.
  - Confirm values such as 0.3% / 0.01% are not rounded to 0% or clipped.

- [ ] **Growth summary**
  - Capture the equipment summary tile.
  - Confirm 강화 가능 / 스킬 가능 counts match the current gold state.
  - Spend gold and confirm the counts refresh immediately.

- [ ] **Skill growth header**
  - Open 스킬.
  - Confirm current gold and affordable skill-option count are visible at the top.
  - Buy +1 / +10 / MAX and confirm the header refreshes.

- [ ] **Epic inventory badge**
  - If an Epic item is available in the current save, confirm its bag badge reads 에픽 rather than 일반.
  - Natural Epic acquisition is not required solely for this capture.

- [ ] **Boss wall loop**
  - Lose to a boss naturally.
  - Confirm the message says 파밍 후 자동 재도전.
  - Confirm mechanics remain unchanged: one-floor retreat, automatic farming, eventual automatic boss re-entry.

### G4.1 validation notes

- Build commit: d2664d4 lineage / G4.1
- Equipment card readability: PASS
- Enhancement cost/rate visibility: PASS
- Very-low-rate formatting: implementation/CI PASS; live milestone item not required
- Growth summary refresh: PASS
- Skill header refresh: PASS
- Epic badge: implementation/CI PASS
- Boss defeat loop message: PASS
- Existing retreat/retry behavior unchanged: PASS
- Bugs found: none in tested flow
- Merge approval: gameplay validated; PR remains unmerged by policy


## G4.2 — Rarity-Scaled Loot Feedback

### Required capture set

- [ ] **Normal / Magic restraint**
  - Let the game run at x5 until lower-rarity equipment drops.
  - Confirm the pillar is visible but does not dominate combat.

- [ ] **Rare / Unique escalation**
  - Capture a Rare or Unique drop if one appears naturally.
  - Confirm the beam is clearly taller than low tiers and the ring/particle layer is readable.

- [ ] **Legendary / Epic premium moment**
  - Capture if naturally available from field, elite or boss reward.
  - Confirm Legendary triggers a strong screen cue.
  - Confirm Epic is visibly stronger than Legendary and uses the tallest purple pillar.
  - Do not force or rebalance rarity solely to obtain this capture.

- [ ] **Elite / Boss reward reuse**
  - When an Elite or Boss grants equipment, confirm the same rarity-scaled world effect appears at the death position.

- [ ] **x5 readability**
  - Confirm beams are recognizable at x5 without covering enemies, damage numbers or movement paths.

### G4.2 validation notes

- Build commit:
- Normal clutter: Low / Good / Too much
- Magic readability:
- Rare readability:
- Unique readability:
- Legendary feedback:
- Epic feedback:
- Elite reward effect:
- Boss reward effect:
- x5 combat readability:
- Bugs found:
- Merge approval: Pending / Approved / Rejected


## UIUX V2.0 — Full Management Overhaul

### Required capture set

- [ ] **Management hub overview**
  - Open any management tab.
  - Capture the new almost-full-width hub.
  - Confirm combat is visually dimmed behind it and the bottom combat dock is hidden.
  - Confirm top navigation shows 장비 / 가방 / 스킬 / 환생 / 정보.

- [ ] **Class selection V2**
  - Trigger the class-selection screen from a fresh/rebirth flow.
  - Confirm all six class cards fit in one screen.
  - Confirm each card remains readable at desktop scale and class color is used as identity rather than full UI chrome.

- [ ] **Equipment composition**
  - Open 장비.
  - Confirm the character portrait sits in the center of the 3x3 composition.
  - Confirm equipment cards are materially larger than V1 and rarity colors remain readable.
  - Check long item names, enhancement rate/cost and buttons for clipping.

- [ ] **Inventory desktop layout**
  - Open 가방.
  - Confirm the item grid and detail pane are visible side-by-side.
  - Confirm eight columns fit without horizontal scrolling.
  - Select several items and verify comparison/details update without moving the grid.
  - Confirm clean upgrades show the green ↑ hint.

- [ ] **Skill progression**
  - Open 스킬.
  - Confirm each skill row includes a visible level progress bar.
  - Confirm +1 / +10 / MAX remain readable and clickable.

- [ ] **Rebirth progression**
  - Open 환생.
  - Confirm the level-to-rebirth progress bar is visible.
  - Confirm permanent upgrade buttons remain readable.

- [ ] **Navigation / ESC**
  - Switch tabs using the in-window top navigation.
  - Confirm the management hub stays open.
  - Press ESC once: management should close.
  - Confirm pause/settings does **not** open on that same keypress.
  - Press ESC again: pause/settings should open.

- [ ] **Combat dock restoration**
  - Close management.
  - Confirm the compact bottom dock returns and keyboard shortcuts 1-5 still work.

- [ ] **Combat HUD cleanup**
  - Confirm active combat no longer shows separate 저장 / 종료 buttons.
  - Confirm the single 메뉴 button opens pause/settings.
  - Verify save/load/quit remain available inside the dedicated menu/info surfaces.

### UIUX V2.0 validation notes

- Build commit:
- Management scale: Too small / Good / Too large
- Class selection readability:
- Equipment scan speed:
- Inventory grid readability:
- Detail pane readability:
- Upgrade arrow usefulness:
- Skill progress readability:
- Rebirth progress readability:
- Top navigation:
- ESC behavior:
- Combat dock restore:
- Combat HUD menu/readability:
- Clipping / overlap:
- Overall visual direction: Reject / Iterate / Accept
- Bugs found:
- Merge approval: Pending / Approved / Rejected


## UIUX V2.1 — Playtest Polish

- [ ] **Management live state**
  - Open any management tab during active combat.
  - Confirm the header shows 자동사냥 계속 and combat is still visibly running behind the dim layer.

- [ ] **Equipment hierarchy**
  - Confirm occupied cards have subtle rarity tint + stronger rarity border without making the whole screen look neon.
  - Confirm center portrait shows class, current level and rebirth count.

- [ ] **Empty inventory context**
  - With a strict filter such as 에픽만 and an empty bag, confirm both header and detail pane explain the active acquisition filter.

- [ ] **Rebirth layout**
  - Confirm the top progression card fills the width cleanly.
  - Confirm all four permanent upgrades are presented in one balanced row.
  - Confirm long remaining-level text does not clip.

- [ ] **Info dashboard**
  - Confirm 모험 기록 / 전투 능력 / 현재 층 위협도 / 보조 능력 appear as four balanced cards.
  - Confirm save/load/quit are absent from Info and remain available only in 메뉴.

### UIUX V2.1 validation notes

- Build commit:
- Management-live indicator:
- Equipment rarity treatment:
- Empty inventory explanation:
- Rebirth card balance:
- Info dashboard readability:
- Text clipping:
- Bugs found:
- Merge approval: Pending / Approved / Rejected