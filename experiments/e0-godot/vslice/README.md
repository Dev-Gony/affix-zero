# AFFIX: ZERO V0 Vertical Slice

This is the product-facing playable slice. It replaced the E0-C03 performance fixture as the default F5 scene after the user rejected the engineering fixture as representative game presentation.

## Current slice

- Uses the repository production-made `dungeon_courtyard.png`, `class_atlas_alpha.png`, and `enemy_atlas_alpha.png`.
- Warrior auto-hunts from the arena center.
- Slime, bat, skeleton, and goblin spawn from the arena edges.
- Enemy separation prevents the deliberate E0 circular pile-up.
- Melee attack wind-up, hit flash, knockback, damage text, hit sparks, death, XP/gold pickups, magnet collection, and level-up are active.
- 30 kills spawn a Dark Knight elite. Elite death clears the slice.
- Compact HUD replaces engineering benchmark diagnostics.
- R restarts after defeat or clear.

## Important art limitation

The production atlases currently contain one pose per class/enemy, not true walk/attack/hit/death frame sequences. V0 therefore proves gameplay presentation and asset direction, but it is **not** final animation approval.

V0-02 must add real frame animation assets and bind damage ACTIVE timing to the actual impact frame. Do not describe transform/bob/lunge motion as completed production animation.

## Engineering fixtures

E0-C01/C02/C03 remain under `res://tests` and `res://perf`. They are not the default game presentation.
