# Sprite asset notes

`dungeon_courtyard.png` is an original generated environment made for AFFIX: ZERO. The supplied game screenshot was used only as a reference for the broad top-down pixel-art camera, visual density, and dark-fantasy mood; no map layout, characters, labels, or UI were copied.

Production prompt summary: an original 16:9, 16-bit top-down ruined dungeon courtyard with a clear central combat circle, open approach lanes from all four edges, weathered charcoal stone, broken pillars, iron braziers, sparse crimson rune cracks, and dense props limited to the outer perimeter. The image contains no characters, monsters, items, text, logos, or UI.

`class_atlas_alpha.png`, `enemy_atlas_alpha.png`, and `equipment_atlas_alpha.png` are original generated pixel-art atlases for this project. The supplied screenshots were used only for broad dark-fantasy ARPG mood, sprite readability, and inventory density. No named character, costume, icon, interface frame, logo, or map was copied.

Prompt summaries:

- Six distinct top-down three-quarter heroes in a strict 3x2 atlas: warrior, mage, knight, sage, assassin, and saint.
- Eight progressively threatening enemies in a strict 4x2 atlas: slime, bat, skeleton, goblin, dark knight, lich, dragon, and demon lord.
- Seven equipment silhouettes in a 4x2 atlas: weapon, helmet, armor, gloves, boots, ring, and amulet, with one empty cell.

Generated checkerboard previews were background-extracted into true alpha atlases while preserving outlined sprite interiors; only the production alpha files are kept in the repository.

`item_base_atlas_v2.png` is a true-alpha 6x5 atlas containing individual artwork for all 29 equipment bases. Its source prompt requested a strict cell order for five weapons, four helmets, four armors, four gloves, four boots, four rings, and four amulets, with one deliberately empty cell. The existing equipment atlas was supplied only as a style reference for palette, outline weight, and small-size readability; every base-item design is original. A built-in image edit removed only the generated checkerboard backdrop while preserving the item silhouettes and grid positions.
