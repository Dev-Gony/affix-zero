# AFFIX: ZERO - Codex Project Context

## Game concept

AFFIX: ZERO is an idle top-down hack-and-slash game built with Godot 4.3. Its direction combines Hero Siege-style pixel combat with Diablo-style randomized loot. Automatic combat, item affixes, progression, class selection, rebirth, effects, and the tabbed management UI are implemented.

The game uses a 640x360 pixel-art viewport. Preserve crisp scaling and keep combat content in the upper region while upgrade and management UI belongs in the lower region.

## Folder structure

- `scenes/`: Godot scenes. Use `battle/`, `ui/`, and `effects/` for their respective scene types.
- `scripts/autoloads/`: project-wide state, save, loot, and audio managers registered in `project.godot`.
- `scripts/combat/`: combatants, attacks, targeting, damage, and waves.
- `scripts/loot/`: item generation, rarities, affixes, and equipment behavior.
- `scripts/progression/`: levels, classes, upgrades, and long-term progression.
- `scripts/ui/`: UI controllers and presentation logic.
- `resources/items/`: item data resources.
- `resources/enemies/`: enemy data resources.
- `resources/classes/`: player-class data resources.
- `assets/sprites/`, `assets/sfx/`, `assets/bgm/`: source game assets.
- `tests/`: headless Godot smoke tests. Keep tests deterministic and do not write over a player's save file.

## Coding conventions

- Use GDScript compatible with Godot 4.3.
- Use static typing for variables, parameters, return values, arrays, and dictionaries where practical.
- Use `snake_case` for variables, functions, and general script filenames. Use `PascalCase` for node types, enums, and the existing autoload filenames.
- Prefer signals for communication between independent systems; avoid tight cross-system node references.
- Model reusable game data with custom `Resource` classes and `.tres` files rather than hard-coded dictionaries.
- Keep scene scripts focused on scene behavior and place reusable rules in the appropriate system directory.
- Keep autoloads small and intentional. Do not turn them into catch-all dependency containers.
- Add gameplay systems only within the scope of the active task.
- Keep balance values in `.tres` resources. Code may contain resource paths and algorithms, but not duplicate resource balance tables.
- Treat the 640x360 internal viewport as the layout source of truth. Verify both 640x360 and the default 2x desktop presentation.
- Read `design-system/affix-zero/MASTER.md` and the relevant page override before changing UI. The game HUD override is `design-system/affix-zero/pages/game-hud.md`.
- Run `res://tests/smoke_test.tscn` headlessly after changes to combat, progression, loot, saves, or UI state.
