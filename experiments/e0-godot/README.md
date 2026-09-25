# E0-C01 isolated combat slice

This directory is a separate Godot 4.7.2 project. It does not load the legacy root project, SaveManager, user://save.json, pets, rebirth, inventory, gacha, or other production systems.

## What is implemented

- Warrior and one melee enemy are separate actors.
- Both actors physically approach each other.
- Both use explicit IDLE/APPROACH/WINDUP/ACTIVE/RECOVERY/HIT/DEAD states.
- Damage occurs once in ACTIVE, not at animation start.
- Player attacks carry an attack_instance_id; the enemy rejects duplicate damage from the same instance.
- Idle/walk/attack/hit/death use distinct frame assets. Attack uses three actual pose frames.
- Visuals are E0 placeholder art authored for this structural test, not final approved production art.

## Run on Windows

Use the standard Godot 4.7.2 executable, not the .NET build.

1. Pull the branch after your existing local work is safe.
2. Import only `D:\github\affix\experiments\e0-godot\project.godot` in Godot 4.7.2.
3. Press F6/F5. Do not open the legacy root `project.godot` with 4.7.2 yet.
4. Watch the warrior and enemy walk, wind up, strike on the impact pose, react to hits, and finish with the enemy death pose.
5. Press R after the fight to replay.

Headless contract:

```powershell
Godot_v4.7.2-stable_win64.exe --headless --editor --path experiments/e0-godot --quit-after 3
Godot_v4.7.2-stable_win64.exe --headless --path experiments/e0-godot res://tests/e0_c01_test.tscn
```

Expected final line: `E0_C01_TEST PASSED`.
