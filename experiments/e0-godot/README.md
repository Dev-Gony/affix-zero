# E0 isolated combat slice

This is a separate Godot 4.7.2 project. It does not load the legacy root project, SaveManager, user://save.json, pets, rebirth, inventory, gacha, or production autoloads.

## E0-C01

- Warrior + one melee enemy.
- Real position-based approach.
- IDLE / APPROACH / WINDUP / ACTIVE / RECOVERY / HIT / DEAD.
- Damage occurs once in ACTIVE.
- Separate pose frames for idle, walk, attack, hit, death.

## E0-C02

- Adds a ranged enemy with separate cast frames.
- Ranged damage is not applied at cast time. A projectile node moves through world space and performs segment collision against the warrior.
- Enemy death creates one world drop but grants no reward.
- Only physical pickup calls RewardLedger.
- Drop IDs are idempotent, so duplicate collection cannot add gold/xp twice.
- After combat, the warrior physically walks to remaining drops.
- All visuals here remain E0 prototype art, not final production art.

## Windows run

Use the standard Godot 4.7.2 executable, not the .NET build.

Open only:

`D:\github\affix-e0\experiments\e0-godot\project.godot`

Do not open the legacy root `D:\github\affix\project.godot` with 4.7.2 yet.

Headless contracts:

```powershell
Godot_v4.7.2-stable_win64.exe --headless --editor --path experiments/e0-godot --quit-after 3
Godot_v4.7.2-stable_win64.exe --headless --path experiments/e0-godot res://tests/e0_c01_test.tscn
Godot_v4.7.2-stable_win64.exe --headless --path experiments/e0-godot res://tests/e0_c02_test.tscn
```

Expected final lines:
- `E0_C01_TEST PASSED`
- `E0_C02_TEST PASSED`
