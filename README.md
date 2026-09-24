# AFFIX: ZERO

AFFIX: ZERO is a Godot 4.3 idle hack-and-slash game inspired by the top-down action of Hero Siege and the randomized loot progression of Diablo. The project is currently a foundation only: it contains the initial scene, global state placeholders, save-file access, and directories for future systems.

## Project setup

- Engine: Godot 4.3
- Base resolution: 640x360
- Stretch mode: `canvas_items`
- Rendering: compatibility renderer with nearest-neighbor texture filtering for pixel art
- Main scene: `res://scenes/main.tscn`

Open `project.godot` in Godot 4.3 and run the project. The main scene contains a `BattleArea` for the upper combat region and a `UILayer` whose origin begins at the lower part of the viewport.

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
|   |-- autoloads/
|   |   |-- GameManager.gd
|   |   |-- SaveManager.gd
|   |   `-- LootManager.gd
|   |-- combat/
|   |-- loot/
|   |-- progression/
|   `-- ui/
|-- resources/
|   |-- items/
|   |-- enemies/
|   `-- classes/
`-- assets/
    |-- sprites/
    |-- sfx/
    `-- bgm/
```

## Autoloads

- `GameManager`: stores the initial player stats, game state, floor, currencies, and the selected x1/x2/x5 speed multiplier.
- `SaveManager`: provides JSON save and load helpers backed by `user://save.json`.
- `LootManager`: reserved for future item generation.

Gameplay, combat, loot generation, progression, and UI behavior are intentionally not implemented yet.
