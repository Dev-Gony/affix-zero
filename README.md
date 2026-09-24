# AFFIX: ZERO

AFFIX: ZERO is a complete Godot 4.3 idle hack-and-slash prototype. It combines fixed-center, top-down automatic combat with randomized Diablo-style equipment, six playable classes, permanent rebirth progression, and a compact Korean-language pixel UI.

## Run the game

1. Open `project.godot` in Godot 4.3.
2. Press **F6/F5** to run `scenes/main.tscn`.
3. Select Warrior or Mage. The remaining four classes unlock through rebirths.

The internal pixel-art resolution is 640x360 and the default desktop window is 1280x720. The compatibility renderer keeps the project suitable for desktop and web exports.

Keyboard shortcuts: `1`-`5` switch the management tabs, while `Z` / `X` / `C` select x1 / x2 / x5 combat speed. Class cards and all buttons support keyboard focus and activation.

Every pull request CI run also publishes two downloadable playable artifacts:

- `AFFIX-ZERO-windows`: unzip and run `AFFIX_ZERO.exe`.
- `AFFIX-ZERO-web`: serve the extracted folder with any static HTTP server and open `index.html`.

## Implemented systems

- Fixed-center automatic combat with nearest-target attacks and 3-second class skills
- Floor-scaled waves, eight enemy families, death, immediate revival, and floor retreat
- Six classes with distinct stats, unlock requirements, and skill visuals
- Five loot rarities, seven equipment slots, 29 item bases, and ten non-duplicating affixes
- 20-slot loot grid, icon-based equipment overview, item comparison, selling, passive skills, stats, and rebirth UI
- Level progression, permanent upgrades, class unlocks, and multiplicative rebirth gold gain
- Damage numbers, pixel fragments, critical feedback, level-up effects, legendary flash, and camera shake
- Original dark-fantasy pixel courtyard plus production class, enemy, and equipment atlases
- Visible edge-spawn telegraphs and equipment comparison deltas in the selected-item detail panel
- x1/x2/x5 combat speed control
- Complete JSON save/load state with a real-time 30-second autosave interval
- Three floor-range BGM themes and fourteen SFX channels with built-in procedural chiptune fallbacks
- Drop-in OGG overrides under `assets/bgm/` and `assets/sfx/` automatically replace procedural audio

Procedural audio is active by default. Matching `.ogg` files placed in `assets/bgm/` and `assets/sfx/` override it automatically; expected filenames are documented in `scripts/autoloads/AudioManager.gd`.

## Project structure

```text
affix-zero/
|-- project.godot
|-- scenes/
|   |-- main.tscn
|   |-- battle/
|   |-- ui/
|   `-- effects/
|-- scripts/
|   |-- autoloads/       # State, save, loot, and audio managers
|   |-- combat/          # Battle loop, enemies, damage, projectiles, effects
|   |-- loot/            # Resource-driven item generation
|   |-- progression/     # Level and rebirth systems
|   `-- ui/              # HUD, tabs, inventory, and class selection
|-- resources/
|   |-- items/           # Item bases, rarities, and affixes
|   |-- enemies/         # Eight enemy balance resources
|   `-- classes/         # Six class balance resources
|-- assets/
|   |-- sprites/
|   |-- sfx/
|   `-- bgm/
`-- tests/
    `-- smoke_test.tscn
```

## Validation

Run the automated Godot smoke test from a terminal:

```powershell
godot --headless --path . res://tests/smoke_test.tscn
```

The test exercises automatic combat and floor progression, all six class skills, loot generation, equipment, JSON state serialization, rebirth, and UI loading. Persistence is disabled by the test runner, so it never overwrites the player's `user://save.json` file.

Pull requests and pushes to `main` run the same smoke test through `.github/workflows/godot-smoke-test.yml` using Godot 4.3.
