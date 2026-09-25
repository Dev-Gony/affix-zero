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

Automated CI is complete, but G3.3 is **not merge-approved** until the user validates the actual Windows Godot 4.3 play experience.
