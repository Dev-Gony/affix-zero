# Dungeon Variety, Difficulty, Loot Rolls, and Combat Feedback

Date: 2026-10-02. This document records deliberate AFFIX: ZERO tuning. Hero Siege and Survivor.io were used only as product references for dungeon variety, affix-driven item comparison, readable horde combat, and watchable automatic action. No proprietary timing, drop table, map, audio, or asset was copied, and the values below must not be presented as values from either game.

## Three topology variants

All layouts share the 72x32 world boundary so the existing camera, HUD, and save-safe entrance remain stable. They do not share obstacle or spawn arrays.

- `EMBER BASTION`: eight separated 2x2 strongpoints and two long crossing inlay routes. It has broad sightlines with isolated detours.
- `SPLIT GALLERIES`: thirteen staggered blockers form alternating north/south lanes and gates. Enemy groups are distributed inside five lateral bands, so target selection repeatedly changes corridors.
- `RITUAL CRUCIBLE`: twelve inner/outer ring blockers, inward-facing spawn pockets, and converging V-shaped floor inlays. It produces shorter central sightlines and more multi-enemy approaches.

Every `DungeonWorld` construction checks that all 24 spawn cells are walkable and reachable from the fixed entrance. Movement, line of sight, visible obstacles, and minimap obstacle markers use the same selected layout. A clear advances the persisted `dungeonClears` sequence and rotates to the next layout. Reopening the game reconstructs the layout from the saved clear count.

### Honest visual-identity boundary

These are three mechanically different temple arrangements, not three fully
distinct biomes. They currently share the same temple floor/wall material,
obstacle family, lighting treatment, enemy base atlas, and ambient presentation.
Topology, floor inlays, procedural sigils, elite names, attack patterns, and
minimap silhouettes distinguish play, but the next visual-identity slice still
needs original per-layout prop silhouettes, palette/material accents, and
environmental storytelling before Ember, Gallery, and Ritual can be described
as separate biomes. No current screenshot should be labeled as proof of three
complete biome art sets.

## Layout-specific elite encounters

The topology now changes combat pressure as well as pathing. Ember Bastion
fields slow armored Bulwarks, Split Galleries fields fast short-reach Stalkers,
and Ritual Crucible fields long-reach Reavers. Enemy 24 is a named guardian
variant of the same family. Each family has a distinct procedural floor sigil,
and its attack warning uses the real reach and impact timing. Full tuning,
provenance, and current evidence are recorded in
`ELITE_ENCOUNTER_IDENTITIES.md`.

## Difficulty rules

Difficulty may be changed only while hunting is explicitly stopped and no pending field item is blocking re-entry. The choice is saved in profile schema v3. The HUD lists the actual multipliers, not a vague difficulty label.

| Difficulty | Enemy HP | Enemy damage | Elite spacing | XP/gold | Drop chance | Rarity progression bonus |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Scout | 1.00x | 1.00x | every 8th enemy | 1.00x | 1.00x | +0.00 |
| Veteran | 1.45x | 1.25x | every 6th enemy | 1.20x | 1.25x | +0.12 |
| Torment | 2.05x | 1.60x | every 4th enemy | 1.50x | 1.60x | +0.28 |

At floor 20, the configured field chances are 1.028%, 1.285%, and 1.6448%. With the difficulty-specific elite counts and floor-20 elite chance, the expected drops per 24-enemy clear are approximately 0.80 Scout, 1.23 Veteran, and 2.16 Torment. These are AFFIX tuning values. Historical Scout baselines remain 0.8-2% field, 18-30% elite, and rarity weights 60/25/10/4/1/0.2.

## Same-base item variance

Magic and higher items draw one to the rarity affix cap without replacement from ATK, DEF, HP, MP, SPD, CRIT, VAMP, XP, GOLD, and PEN. Each selected affix rolls within its configured numerical range, scaled by floor. Consequently two items with the same base, rarity, and floor can differ in both affix combination and values, while one item cannot roll the same stat twice.

`GenerateGuaranteedBase` is an audit-only seeded entry point. Runtime drops continue to select from all eligible bases. CoreSmoke samples 300 level-12 rare longswords, checks more than 80 distinct option signatures, numerical ATK variance, and zero duplicate stats. A separate 100,000-seed rarity sample must stay within 0.6 percentage points of the normalized configured probabilities.

## Progression curve

Player level retains the historical 250-XP display cadence. Talent points now use cumulative bands:

- points 1-10: 250 XP each;
- points 11-30: 400 XP each;
- points 31-50: 650 XP each;
- points 51-75: 1,000 XP each;
- surplus after the tree: 1,500 XP each.

This preserves immediate early decisions but moves a complete 75-rank build from 18,750 XP to 48,500 XP. The prior 20-minute natural run's 25,570 XP maps to 52 points rather than exhausting the entire tree. Existing v1/v2 saves migrate to v3 with an explicit compatibility credit so already-earned ranks are not removed.

## Combat readability and effect budget

- Enemy attacks emit a short red anticipation arc before their existing impact frame.
- Normal, critical, and killing hits use distinct impact scale/color, floating labels, and short generated audio transients.
- Sword, axe, and staff retain different real reach/timing rules and now render distinct slash width/color or ranged beam feedback.
- Kill chains remain visible for a 2.4-second window.
- Hit effects are capped at 32 live sprites and combat lines reuse a fixed pool.
- `FX FULL / FX LOW` reduces line tessellation, camera shake, and audio volume without changing combat outcomes.

No external audio file was added. The short hit tones are created in memory at runtime. Classes remain deferred: adding labels without separate animation, resource, and skill behavior would be superficial. The current verified distinction is weapon-build behavior (sword, axe, staff), while the core dungeon/loot loop remains the priority.

## Verification contract

Required evidence is separated into CoreSmoke distribution/save checks, Unity import/build, an isolated production-drop natural progression run across all three layouts and difficulties, separate-process reload, actual 720p/1080p frame inspection, and a short D:-resident gameplay recording. UI Toolkit callback probes are not reported as physical mouse/keyboard testing.

## 2026-10-02 verification result

- CoreSmoke: 267/267 PASS. The deterministic Veteran floor-20 sample observed
  0.603720 / 0.264530 / 0.114090 / 0.016900 / 0.000630 / 0.000130 across
  100,000 rolls, within 0.6 percentage points of every normalized target.
- Same-base audit: 300 level-12 rare longswords produced 287 distinct legal
  option signatures. Attack affixes ranged from 2 to 20, with no duplicate
  stat on one item.
- Twenty-minute 1080p production-flow observation:
  Build/Reports/variety-natural-final/natural-progression-observe.json.
  It passed at 1x speed with 868 kills, 36 clears, zero failed runs, zero
  deaths/retries, zero safety restarts, 102 natural drops seen, 101 collected,
  37 equipment upgrades, 76 explicit discards, 60/75 talent ranks, 129 area
  casts, one recovery cast, all three layouts, and all three difficulties.
- The final notification-only correction changed the kill notice to compare
  earned talent points instead of the retained 250-XP level display. The
  rebuilt Windows player then passed a fresh 720p 120-second production-flow
  regression with 75 kills, three clears, all three layouts/difficulties, and
  zero failures/deaths/safety restarts:
  Build/Reports/variety-natural-final-720/natural-progression-observe.json.
- Final Unity 6000.3.24f1 Windows build: Succeeded, 0 errors, 0 warnings,
  build GUID 8fded397ea654f1db965eb326ff9c911.
- Separate-process reload on that final build restored the 20-minute profile
  exactly, completed two additional kills, and saved the continued state:
  Build/Reports/variety-natural-final/natural-progression-read.json.
- Visual evidence: 48 equal-size 1920x1080 RGB frames (16 per layout), three
  inspected 1080p PNGs, and a 12-second 1920x1080/4fps MJPEG AVI. Windows Media
  Player parsed the AVI duration as 12 seconds. The AVI SHA-256 is
  9EA8FF3618904EA3F78791CB1EE27D4EF1FFCFB8AA19DD523333C98FCFB49906.
- Physical mouse/keyboard input remains NOT RUN. The production probes exercise
  native UI Toolkit callbacks and explicitly do not claim OS input coverage.
