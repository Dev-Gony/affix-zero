# AFFIX: ZERO Portfolio Capture Checklist

All previous checklists and recorded results remain byte-for-byte in
[the through-G6.1 archive](archive/PORTFOLIO_CAPTURE_CHECKLIST_THROUGH_G6_1.md).
This is the current recovery checklist, not retroactive acceptance of G6.1.

## Baseline rejection

- Windows baseline: `4339488`, G6.1, `dev/gameplay-v5-pets-items`.
- User supplied combat, equipment, and pet-tab screenshots on 2026-09-25.
- Rejected: enemy animation quality, detached sword, pet art/occlusion, attack/drop
  readability, and summon feedback.
- Screenshot evidence: floor 85 main HUD `0/93` versus objective `0/13`.
- Do not label the prior headless/CI success as Windows art approval.

## Recovery evidence

Candidate: `fix/g6-playtest-recovery`, PR #17, build label `g6.2-rc1`.

Automated evidence is published by `G6 playtest recovery` under
`AFFIX-ZERO-g6-recovery-evidence`:

- `contracts.log`: batch rollback, shard grants, receipt persistence, real UI button
  path, early-hit prevention, projectile impact, room-transition cancellation,
  pet clearance, HUD consistency, and ownership independent of VFX.
- `render/summon_single.png`: actual one-result screen.
- `render/summon_ten.png`: ten cards, each grant shown, funds and pity shown.
- `render/summon_ten_after_combat_refresh.png`: result survives pet-tab rebuild.
- `render/pet_shards.png`: roster shard balance and receipt-history control.
- `render/combat.png` and `render/motion_*.png`: actual Godot frames, not mockups.
- `render/metadata.json`: renderer, engine, source identity, redraw observations,
  and explicit unapproved artwork status. All data is a synthetic isolated fixture.

## Windows checks (not yet performed on the recovery commit)

1. **Summon transaction and visible outcome**
   - Spend only crystals already available; do not reset the save to fund testing.
   - One-pull/ten-pull shows the exact outcomes until dismissed. New pet or duplicate
     shards are explicit. Reopening the receipt costs nothing.
   - With six pets already owned, duplicates must increase shards rather than silently
     appearing to grant nothing. Existing trained levels/stars must remain.
2. **Combat timing and screen clearance**
   - Hero remains visible, caster has no second procedural sword, companion does not
     cross the hero at the old three-second interval.
   - Melee has preparation before damage; caster/pet projectiles reach the target before
     damage. Enemy movement redraw and strike/recovery continue at x1 and x5.
3. **Progression/ownership regression**
   - Both HUDs agree on kills required, room travel remains automatic, and accepted loot
     survives leaving a room. Existing gold/gear/pets load unchanged after restart.

Build commit tested: NOT_RUN
Windows result: NOT_RUN
Visual approval: REJECTED for G6.1; recovery art overhaul NOT_IMPLEMENTED
Merge approval: PENDING

## Still required before a visual-completion milestone

- Authored walk/attack/hit/death frame sequences or a properly authored segmented rig,
  not just static-atlas translation and a few overlay lines.
- Pet artwork matching the hero/monster style, plus distinct readable attack/support motion.
- Final drop visuals and map/corridor art consistency.
- Real x1/x5 clips and user visual approval, kept separate from numerical test results.
