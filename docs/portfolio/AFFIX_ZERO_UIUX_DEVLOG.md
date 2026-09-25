# AFFIX: ZERO UI/UX Development Log

> Purpose: preserve the full design and engineering evolution for portfolio use.
> Rule: keep broken, ugly, and failed iterations. They explain why later decisions exist.

## Product thesis

**AFFIX: ZERO = Hero Siege depth + Survivor.io clarity + unattended auto-hunting.**

- Hero Siege inspiration: dungeon traversal, class identity, loot/build depth, dark pixel ARPG tone.
- Survivor.io inspiration: readable equipment cards, dense enemy pressure, visible XP/gold drops, magnetic collection, quick progression feedback.
- AFFIX: ZERO differentiator: the character autonomously navigates rooms, fights, collects, progresses, and moves floors without continuous manual movement.

This is inspiration at the system/UX level. Original commercial assets, layouts, and branded UI are not treated as project-owned assets.

---

## Milestone 0 — Early MVP

### State
- Static arena-like combat.
- Oversized/mismatched character and enemy art.
- HUD occupied too much of the play area.
- Auto combat existed, but world traversal did not read as a dungeon journey.

### Problems observed
- Character could pass through walls.
- Map did not travel with the player.
- Camera behavior and room progression were unclear.
- Character silhouette looked like a resized placeholder rather than a deliberate class sprite.

### Portfolio angle
A functional prototype was not automatically a readable game. The first major task became separating **combat logic**, **world movement**, and **visual identity**.

---

## Milestone 1 — World traversal and camera experiments

### Changes
- Room-to-room travel introduced.
- Camera following introduced.
- Combat rectangle and room traversal logic iterated.
- Smaller player silhouette tested.

### Failures preserved
- Wall clipping.
- Empty brown/brick transition spaces.
- Repetitive tile rooms.
- Character/monster art styles clashed.
- Some iterations visually regressed despite technically adding movement.

### Decision
Do not count rescaling an existing sprite as a character redesign. World collision, walkable layout, camera rules, and art direction must be treated as separate systems.

---

## Milestone 2 — Pixel character / loot-management direction

### Changes
- Tiny Dungeon-style class silhouettes introduced.
- PixelBoy-style enemy families introduced.
- Equipment, inventory, rarity, locking, selling, save/exit, rebirth, and pickup filtering added.
- Class portrait and selected class were progressively unified.

### Bugs that changed the design
- Rebirth initially removed data users expected to keep.
- Multiple classes initially shared the same skill structure.
- Class portrait could show a warrior while the active class was a knight.
- Pickup rarity setting could mutate already-owned inventory.
- ESC initially paused the player but not the entire battle.

### Decision
UX rules became explicit contracts, not incidental button behavior.

---

## Milestone 3 — Hero Siege + Survivor.io hybrid direction

### Combat changes
- Enemy density increased.
- XP/gold/item rewards became visible field drops.
- Resource attraction toward the player was added.
- Combat became more watchable as an idle/auto-hunt loop.

### UI direction
- Hero Siege: keep build depth, dungeon/class fantasy.
- Survivor.io: use simpler equipment cards, quick rarity recognition, cleaner equipment management.
- AFFIX: keep autonomous hunting as the core interaction difference.

### Result
This was the first stage where the prototype began to feel entertaining to watch rather than merely functional.

---

## PR-A — Rebuild safety baseline

Baseline:
- `cdcf080d2916c053e72e5c3c4ecc57a30213d0f2`

Validated PR-A lineage:
- `269c7da40c49433a3d3db65331b39be18d42945d`
- later shortcut fix: `840316172a9c5ac2bfb8ba948eb97bf0619eec6d`

### Added
- Raw pre-rebuild save backup.
- Build/branch/commit diagnostics overlay.
- Migration guardrails.
- Evidence screenshots and CI checks.

### Bugs found in real Windows testing
- F8 conflicted with Godot Editor's stop shortcut.
- A replacement shortcut was initially generated with invalid Godot key syntax.

### Lesson
A green CI run does not replace local editor/runtime validation. Build identity must be visible when testing screenshots.

---

## PR-B — Interaction and persistence safety

Validated Windows test head:
- `904208be5c2d73f14b87ddb6e1da81226b4fc7fe`

### Final rules
- Equipment / inventory / skills / rebirth / info windows **do not pause combat**.
- ESC settings menu **does pause the entire game**.
- Closing ESC resumes combat.
- Pickup rarity affects future pickup acceptance only.
- Changing rarity must never delete or sell already-owned items.
- Save/quit preserves current character, equipment, inventory, and progression.

### Engineering changes
- Atomic-style save transaction and rollback path.
- Pickup-time rarity validation.
- Diagnostic build stage display.
- Startup pause-state recovery.

### Failed attempt preserved
A global pause coordinator was introduced, then removed after a Windows run produced a grey screen with diagnostics only.

### Lesson
Do not keep an abstraction merely because it looks architecturally cleaner. Prefer the simpler path already proven in the actual runtime when the abstraction destabilizes startup.

---

## PR-C — 720p HUD/UI rebuild

Branch:
- `dev/uiux-pr-c-hud-720`

Archive-time head:
- `3c0d30ecb3b82f94d9d78c9f2dc049d7b4dbfe03`

### Goals
- Move UI logical space to 1280x720.
- Preserve pixel-world framing independently from UI scale.
- Reduce HUD obstruction.
- Remove save/quit from the combat HUD.
- Make speed controls readable.
- Replace the spreadsheet-like bottom bar with larger navigation.
- Shorten wave/boss notifications.
- Prepare PR-D integrated equipment/inventory layout.

### Current state
In progress. Do not describe PR-C as complete until the Windows runtime screenshot is reviewed.

---

## Next planned stages

### PR-D — Integrated equipment + inventory
- Character/equipment overview on the left.
- 60-slot inventory grid in the center.
- Selected item details and comparison on the right.
- Separate **view filter**, **pickup rule**, and **sell action**.
- Clear rarity borders and lock state.

### PR-E — Class-active combat identity
- Real class-specific active skills.
- Per-run enhancement choices inspired by survivor-style progression.
- Persistent class progression remains separate from run-only choices.

### PR-F — Dungeon quality
- Multiple room layouts and themes.
- Walkable geometry and visible walls from the same navigation data.
- Better corridor transitions and camera motion.

### PR-G — Loot/combat polish
- Rarity beams.
- Legendary emphasis.
- Better pickup arcs.
- Boss/elite readability.
- Damage-number density control.

---

## Screenshot archive policy

For every meaningful milestone preserve at minimum:

1. combat screen,
2. equipment/inventory screen,
3. pause/settings screen when changed,
4. broken/failed screen when it changed a design decision,
5. branch and commit SHA,
6. observed problem,
7. reason for the change,
8. validation result.

**Never delete embarrassing early screenshots.** They are useful portfolio evidence of iteration.

---

## Portfolio story

Suggested narrative:

> AFFIX: ZERO began as a functional but visually inconsistent auto-battle MVP. I used repeated local playtests and UI screenshots to separate three problems: ARPG build depth, survivor-style combat readability, and unattended progression. Instead of only reskinning the prototype, I introduced explicit UX contracts for pause behavior, item pickup filtering, rebirth persistence, save safety, and build identity, then rebuilt the visual hierarchy around a 720p HUD and integrated equipment workflow.

Useful evidence:
- before/after combat screenshots,
- wall-clipping and map-transition failures,
- class portrait mismatch,
- destructive loot-filter bug and its corrected contract,
- partial-pause bug,
- grey-screen pause-abstraction regression,
- PR-A/B diagnostic screenshots,
- PR-C onward 720p UI comparisons.
