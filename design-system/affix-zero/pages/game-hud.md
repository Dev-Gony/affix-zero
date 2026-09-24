# Game HUD Override

This page override adapts the project-wide pixel-art direction to the Godot 4.3 runtime.

## Layout

- Treat 640x360 as the source viewport and verify the default 1280x720 2x presentation.
- Reserve y=0..41 for the combat HUD, y=42..213 for the arena, and y=214..359 for management tabs.
- Hide the runtime HUD and management tabs while class selection is open.
- Show all seven equipment slots at once. Do not require horizontal scrolling for the equipment overview.
- Inventory may scroll vertically, but primary equip and sell actions must remain visible on each row.

## Visual language

- Use crisp, square edges and disable anti-aliasing on UI style boxes.
- Use near-black plum surfaces, aged bronze borders, blood-red selection states, loot-grade accents, and green equipped markers.
- Keep normal Korean text readable at the 2x presentation. Use 7px only for compact equipment metadata; use 9-11px for actions and primary information.
- Preserve a quiet circular combat area around the fixed-center player. Props belong near the arena edges.

## Interaction

- Every button needs normal, hover, pressed, disabled, and visible keyboard-focus states.
- Number keys 1-5 switch management tabs. Z/X/C select x1/x2/x5 combat speed.
- Class selection must focus the first unlocked class for keyboard play.
- Color cannot be the only state indicator: include grade names, arrows/signs for comparisons, and text labels for locked states.

## Motion

- Keep hit flashes and button feedback immediate.
- Use short 150-350ms combat and notification effects; avoid layout-moving animations.
- Camera shake must remain subtle enough that UI text stays stable.
