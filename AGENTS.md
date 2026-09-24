# AFFIX: ZERO - Codex Project Context

## Game concept

AFFIX: ZERO is an idle top-down hack-and-slash game built with Godot 4.3. Its direction combines Hero Siege-style pixel combat with Diablo-style randomized loot. Automatic combat, item affixes, progression, and replay systems are planned, but should be implemented only when a task explicitly requests them.

The game uses a 640x360 pixel-art viewport. Preserve crisp scaling and keep combat content in the upper region while upgrade and management UI belongs in the lower region.

## Folder structure

- `scenes/`: Godot scenes. Use `battle/`, `ui/`, and `effects/` for their respective scene types.
- `scripts/autoloads/`: project-wide managers registered in `project.godot`.
- `scripts/combat/`: combatants, attacks, targeting, damage, and waves.
- `scripts/loot/`: item generation, rarities, affixes, and equipment behavior.
- `scripts/progression/`: levels, classes, upgrades, and long-term progression.
- `scripts/ui/`: UI controllers and presentation logic.
- `resources/items/`: item data resources.
- `resources/enemies/`: enemy data resources.
- `resources/classes/`: player-class data resources.
- `assets/sprites/`, `assets/sfx/`, `assets/bgm/`: source game assets.

## Coding conventions

- Use GDScript compatible with Godot 4.3.
- Use static typing for variables, parameters, return values, arrays, and dictionaries where practical.
- Use `snake_case` for variables, functions, and general script filenames. Use `PascalCase` for node types, enums, and the existing autoload filenames.
- Prefer signals for communication between independent systems; avoid tight cross-system node references.
- Model reusable game data with custom `Resource` classes and `.tres` files rather than hard-coded dictionaries.
- Keep scene scripts focused on scene behavior and place reusable rules in the appropriate system directory.
- Keep autoloads small and intentional. Do not turn them into catch-all dependency containers.
- Add gameplay systems only within the scope of the active task.
