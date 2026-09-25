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

- Build commit:
- Floor / rebirth:
- Character / class:
- Normal-enemy hit feel:
- Boss hit feel:
- Sell All result:
- Inventory detail clipping:
- Epic seen naturally: Yes / No
- Bugs found:
- Merge approval: Pending / Approved / Rejected

## Portfolio story to preserve

**Problem → Cause → Reference UX → Decision → Implementation → Failure/Revision → Verification → Before/After**

For G3.3, the important story is not merely “added Epic.” The portfolio evidence should show that rarity depth, combat readability, and inventory economy were changed together, then guarded by regression tests after the sixth rarity exposed a stale five-tier smoke assumption.
