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


## G4.2 — Rarity-Scaled Loot VFX

### Required capture set

Editor-only comparison shortcut: **Ctrl+Shift+8** spawns Normal → Epic sample drop VFX around the player for deterministic visual validation. It does not add items or change progression.

- [ ] **Normal / Magic restraint**
  - Capture ordinary field equipment if naturally encountered.
  - Normal should not create a tall light pillar.
  - Magic may show a small beam but should not dominate combat.

- [ ] **Rare+ hierarchy**
  - Capture a Rare or higher equipment event.
  - Confirm beam height/glow/ring strength is clearly greater than Magic.
  - Confirm the item rarity/name remains readable near the drop.

- [ ] **Boss / Elite reward accent**
  - Capture equipment from a boss or elite bonus reward.
  - Confirm the rarity-colored effect also receives the extra reward ring.

- [ ] **x5 readability**
  - Run at x5 until an equipment drop appears.
  - Confirm the drop does not vanish too quickly to identify.
  - Confirm the effect remains brief enough not to stack into visual clutter.

- [ ] **Pickup cleanup**
  - Watch a field item auto-pickup.
  - Confirm the beam collapses/fades rather than remaining as a ghost pillar after collection.

- [ ] **Combat clarity**
  - Confirm beams do not obscure nearby monsters, elite auras, damage numbers, or the player silhouette.

### G4.2 validation notes

- Build commit:
- Normal visibility:
- Magic visibility:
- Rare+ hierarchy:
- Boss/elite accent:
- x5 readability:
- Pickup cleanup:
- Combat clutter:
- Bugs found:
- Merge approval: Pending / Approved / Rejected
