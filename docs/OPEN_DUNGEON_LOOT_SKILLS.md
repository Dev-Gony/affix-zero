# Connected Temple Dungeon, Loot, and Automatic Skills

Date: 2026-10-02. Branch: `feature/open-dungeon-loot-skills`. Engine: Unity 6000.3.24f1, Built-in 2D. Draft PR #19 and `restart/unity-6` were not modified or merged.

## Product loop

- One connected 72x32 temple floor with a following, clamped camera.
- Twenty-one normal monsters and three elites are present at entry. They patrol before aggro; at most three may attack at once.
- The rendered landmark walls, BFS blocked cells, and line-of-sight data share the same source.
- The BFS waypoint aligns only the axis perpendicular to the next cardinal step. The former both-axis recentering caused center +/- 0.12 oscillation and was removed.
- Two consecutive deaths still enter a safety stop. Explicit Start after review creates a fresh entrance state. It does not make the hero invulnerable.
- Inventory pressure is a real part of the loop: compare upgrades, equip them, and use the existing two-step discard action on inferior items.

## Loot and equipment

Recovered production data from the historical `c95b7ba0` line is retained:

- Rarities: normal, magic, rare, unique, legend, epic.
- Base rarity weights: 60 / 25 / 10 / 4 / 1 / 0.2.
- Affix caps: 0 / 2 / 3 / 4 / 5 / 6.
- Rarity multipliers: 1 / 1.3 / 1.7 / 2.2 / 3 / 4.5.
- Field drops: 0.8% to 2%; elites: 18% to 30%.
- Twenty-nine bases and ten stats: ATK, DEF, HP, MP, SPD, CRIT, VAMP, XP, GOLD, PEN.
- The 20-minute production-flow test did not pass `-affixAutoHuntTest`; no guaranteed fourth-kill drop or survival fixture was active.

The historical data has seven generated equipment categories: weapon, armor, helmet, gloves, boots, ring, and amulet. The Unity save model exposes eight slots because Relic already existed in the Unity implementation and must remain for save compatibility. Thus seven historical random-drop categories plus the preserved Relic slot equals eight visible slots. Relic remains supported by saves and authored/test items; the recovered seven-slot loot pool is not silently rewritten.

## Six-node, 75-rank tree

- Fury 20: +3 attack per rank, +60 at cap.
- Precision 10: +4 attack per rank, +40 at cap; requires Fury 2.
- Keystone 5: +6 attack per rank, +30 at cap; requires Precision 1.
- Vitality 20: +10 maximum HP per rank, +200 at cap.
- Cleave 10: +0.08 radius and +2.5 percentage points splash damage per rank; requires Fury 2.
- Haste 10: -2% automatic-skill cooldown per rank, -20% at cap; requires Precision 1.

The complete tree contains 75 ranks. Extra earned points remain unspent after the cap rather than being lost or repeatedly applied.

## Automatic skills and combat pressure

- Area skill: fires only with at least two enemies in radius 3.2 and valid line of sight.
- Recovery skill: triggers at or below 45% HP, heals max(12, 20% maximum HP), starts each run with an 8-second arming delay, then has a 14-second base cooldown divided by attack-speed multiplier.
- The discarded experiments (85% full heal, 4-second/full heal, 0.25-second/full heal) are not present.
- The maximum three simultaneous attackers remains. The discarded one-attacker experiment is not present.
- A full-clear technical smoke test still uses an explicitly reported ephemeral Epic fixture; production-flow evidence below does not.

## Stitch references actually reviewed

The locally supplied Stitch archive intake was inspected through each archive's `screen.png`, `code.html`, and `DESIGN.md`, with hashes recorded under `docs/assets/stitch-reference-*.json`. The runtime work specifically follows:

- Screen 08: dungeon HUD and bottom action dock.
- Screen 10: equipment/talent management structure and comparison area.
- Screen 11: forge layout.
- Screen 12: full talent tree.

The archive inventory includes duplicates identified by hashes. The Unity UI is a native UI Toolkit adaptation, not embedded HTML. No additional equipment reference outside the recorded Stitch archive set was available.

## Validation

### PASS

- CoreSmoke: 256 checks.
- Unity import/compile and Windows build: 0 errors, 0 warnings.
- 720p fixture full clear: 24 kills, six collected items, zero deaths, six framebuffer captures.
- 1080p fixture full clear: 24 kills, eight collected items, zero deaths, six framebuffer captures.
- Fresh natural progression, actual 1x Windows player: 1200.056 seconds.
  - 935 kills, 38 full clears, zero failed runs, zero deaths/retries, zero safety restarts.
  - 43 naturally rolled collected items; 21 comparison/equip upgrades; 18 two-step discards; 19 bag items at finish.
  - 75 invested ranks plus 27 retained surplus points.
  - 124 area-skill casts.
  - Combat build grew from attack 30 / HP 120 / defense 2 to attack 742 / HP 1685 / defense 826.
  - 1096 successful profile saves during the observed flow.
  - Isolated profile SHA-256 after observation: `50dbf2540e06a8752d0b8bfabdba780f672f01a3ae9de64be3ab0cee9e85d409`.
- Separate-process reload: exact canonical restore matched before mutation, two additional kills completed, and the updated profile saved again.

The final natural run's zero recovery casts are expected: naturally equipped defense/health made the 45% trigger unnecessary. The skill itself was exercised in the prior 720p combat run.

### FAIL / discarded evidence

- Pre-fix natural runs exposed the forge `HasItem` binding bug, both-axis path oscillation, 75-rank probe overflow, and a full-bag stop. Those runs are retained as failure reports and are not counted as the final 20-minute PASS.
- The corrected final run completed with none of those stops.

### NOT RUN

Physical Windows mouse/keyboard verification remains NOT RUN. The managed execution environment launched the player process but exposed zero enumerable top-level windows and `MainWindowHandle == 0`. An attempted client capture therefore captured the Chrome window behind it, not Unity, and is excluded from evidence. No permission bypass was attempted. Native UI Toolkit callbacks for Start, equipment, talents, forge, pause, and resume were exercised; that is not claimed as physical input.

## Generated assets and provenance

Hero, monster, temple, obstacle, and impact atlases are original built-in `image_gen` outputs. GearArmor, GearAxe, GearRelic, and GearStaff are also original built-in `image_gen` outputs. Commercial Hero Siege files were not supplied, extracted, traced, or copied. Frame mapping, output hashes, alpha checks, and import settings are recorded in `docs/assets/original-temple-combat-set.md`, `docs/assets/generated-art-manifest.json`, and `docs/assets/generated-gear-v1.json`.

## Storage note

After the request to keep subsequent work on D:, no additional C: artifact was created. Three earlier Library-delivery artifacts remain on C: because deletion was not authorized:

- Evidence ZIP: 12,412,836 bytes (11.84 MiB).
- Expanded evidence folder: 12,543,270 bytes (11.96 MiB).
- Library helper copy: 11,085 bytes (0.01 MiB).

They were created solely to package and upload the first evidence bundle. All later reports, profiles, PNGs, builds, and helper refreshes are on D:.
