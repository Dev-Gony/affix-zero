# Player skill trajectory evolution

Date: 2026-10-02. Fury rank 5 now changes the automatic area skill from a
numeric radial upgrade into a weapon-selected attack trajectory. The choice
uses the already-saved equipped weapon style and talent ranks, so existing
profiles need no migration or new save field.

## Choices and tradeoffs

| Weapon | HUD name | Actual behavior | Tradeoff |
| --- | --- | --- | --- |
| Sword | `ARC` / `ARC II` | Starts on the current or nearest visible target, then jumps through the nearest still-unhit target with LOS from the previous victim. | Focused and reliable, but capped at 3 / 4 targets and damage decays to 72% / 82% per jump. |
| Axe | `QUAKE` / `QUAKE II` | Hits the inner 55% radius immediately, then resolves a second outer annulus 0.18s later. Targets hit by the inner impact are explicitly excluded from the outer wave. | Broadest potential target count, but the outer wave deals 65% damage and cannot travel outside the 3.2-4.1m centered radius. |
| Staff | `LANCE` / `LANCE II` | Commits a 5.5m / 6.2m line through the aimed target and hits only enemies inside a 0.45m / 0.54m lane. | Longest reach, but alignment matters, damage is 86%, and targets are capped at 4 / 5. |

Switching the equipped weapon changes the route immediately. The combat dock
shows the active name and its tooltip lists the focused-chain, two-stage quake, or
long-narrow-pierce tradeoff. Fury detail lists all three weapon routes. Before
Fury rank 5 the original radial `AREA` remains unchanged.

`AutoAreaSkill` selects only living, active targets that satisfy the relevant
range and dungeon LOS query. ARC tracks an explicit unique list across jumps;
LANCE sorts eligible targets by forward distance; QUAKE rechecks life, range,
LOS, and run generation before its delayed outer hit. `MeleeActor.CastSkillSequence`
also rejects repeated actor references before one shared attack receipt is
resolved. Armor, criticals, vampirism, death, hit FX, and `StrikeResolved`
continue through the production combat path.

The procedural cyan ARC path, magenta LANCE lane, and orange QUAKE ring use
Unity's built-in sprite shader and `LineRenderer`. No external or commercial
art was added.

## Verification

- CoreSmoke: 272 PASS, including fresh/reset behavior and instant Sword/Axe/
  Staff route switching without new save state.
- Unity 6000.3.24f1 Windows build: 0 errors, 0 warnings; build GUID
  `7dff73dc3e884968beb5f7b3d1e063fd`.
- Final 150.068s normal-drop 1080p run: PASS; 95 kills, 3 clears, all three
  layouts, guardian and elite pattern families, 8 collected items, and 11
  naturally earned/spent points at Fury 5 and Vitality 6. The naturally
  equipped axe selected QUAKE: three casts produced six real trajectory hits,
  including four delayed outer-wave hits; duplicate candidates remained 0.
  There were zero deaths, failed runs, or safety restarts.
- Separate-process restore: PASS; Fury 5 and `QUAKE` / `Quake` restored exactly,
  then combat continued for two kills and saved again.
- Final 720p and 1080p UI probes: PASS with four framebuffer captures and 16
  native UI Toolkit callbacks each; equip, talent, forge, bounds, and overlap
  checks passed.
- Final-build physical input: PASS in 14.147s against the validated native
  `AFFIX ZERO` PID/title and 1280x720 client. All ten Start, equipment, talent,
  forge, pause, and resume booleans are true. This is OS automation, not a
  claim of human play.

Gameplay evidence: `libfile_b708eea1c8a88191954afcc9a7d76488`
(`AFFIX-ZERO-QUAKE-two-stage-1080.png`) shows the two ring stages and separate
resolved targets. Earlier ARC evidence remains at
`libfile_c449ce9ae33481919fcb8f9aaf963c55`. Both exact Library titles and IDs
were verified. Windows does not expose the requested local xattr API, so only
the Library upload succeeded; this does not affect either retained file.
