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


## G4.1 — Boss Telegraph / Auto-Evade

### Required capture set

- [ ] **Ground Slam telegraph**
  - Reach a boss floor.
  - Capture the red `마왕 강타` danger circle before impact.
  - Confirm the boss visibly pauses during the warning.

- [ ] **Doom Mark telegraph**
  - Capture the purple `파멸 표식` circle locked to the character's original position.
  - Confirm the character immediately abandons normal chase movement and exits the marked area.

- [ ] **Auto-evade result**
  - Capture a successful `회피!` result.
  - Confirm ordinary auto-hunt resumes immediately after the pattern resolves.

- [ ] **x5 readability**
  - Run the boss at x5.
  - Confirm the warning circle/countdown is still understandable and the character's evasive movement remains visible.

- [ ] **Fairness / danger**
  - Confirm the boss still feels threatening.
  - Confirm special attacks do not feel like unavoidable hits.
  - Note whether the new cast pauses make the boss too easy.

### G4.1 validation notes

- Build commit:
- Boss floor:
- Ground Slam visibility:
- Ground Slam evade:
- Doom Mark visibility:
- Doom Mark evade:
- x5 readability:
- Boss difficulty after change: Too easy / Good / Too hard
- Unavoidable-looking hits:
- Bugs found:
- Merge approval: Pending / Approved / Rejected
