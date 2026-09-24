# Game HUD Override

This page override adapts the project-wide pixel-art direction to the Godot 4.3 runtime.

## Layout

- Treat 640x400 as the source viewport and verify the default 1280x800 2x presentation.
- Reserve y=0..41 for the combat HUD and y=356..399 for a compact management dock. The arena remains visible behind management windows.
- Management is progressive disclosure: open one focused window at a time over the right side of the arena; clicking its dock button again or pressing Escape closes it.
- Hide the runtime HUD, dock, and management windows while class selection is open.
- Equipment uses a three-by-three paper-doll layout with the class portrait at its center and all seven slots visible without scrolling.
- Inventory holds 60 items in a six-column scrollable grid, matching the density of a loot-heavy ARPG. Keep selected-item comparison and equip/sell actions fixed below the grid.
- Skills, rebirth, and statistics use dedicated windows rather than sharing permanent vertical space with combat.

## Visual language

- Use crisp, square edges and disable anti-aliasing on UI style boxes.
- Use near-black plum surfaces, aged bronze borders, blood-red selection states, loot-grade accents, and green equipped markers.
- Keep normal Korean text readable at the 2x presentation. Use 7px only for compact equipment metadata; use 9-11px for actions and primary information.
- Preserve a quiet circular combat area around the fixed-center player. Props belong near the arena edges.
- Management windows use opaque near-black surfaces so item silhouettes and Korean text remain readable over combat.
- Combat notifications stay in the left battle column and never cover a management-window title or close action.

## Interaction

- Every button needs normal, hover, pressed, disabled, and visible keyboard-focus states.
- Number keys 1-5 open or switch management windows; pressing the active number or Escape closes it. Z/X/C select x1/x2/x5 combat speed.
- Class selection must focus the first unlocked class for keyboard play.
- Color cannot be the only state indicator: include grade names, arrows/signs for comparisons, and text labels for locked states.

## Motion

- Keep hit flashes and button feedback immediate.
- Use short 150-350ms combat and notification effects; avoid layout-moving animations.
- Camera shake must remain subtle enough that UI text stays stable.
