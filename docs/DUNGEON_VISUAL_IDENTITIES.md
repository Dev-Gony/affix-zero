# Procedural dungeon visual identities

Date: 2026-10-02. This slice gives the three connected layouts visibly different
landmarks and terrain language while retaining the original temple atlas, the
72x32 navigation grid, the fixed entrance, and the existing save sequence.
These are original runtime compositions made from a one-pixel point-filtered
sprite and Unity Built-in `LineRenderer`; no Hero Siege, extracted, traced,
commercial, or other external art is included.

## Identity set

| Layout | HUD identity | Signature | Original readable landmarks |
| --- | --- | --- | --- |
| Ember Bastion | `FORGE CITADEL` | `ember-trenches/crenellated-hearth` | Warm floor grade, north/south heat trenches and molten seams, paired crenellated battlements, a central forge hearth, and a framed entrance forge seal. |
| Split Galleries | `SUNKEN ARCHIVE` | `azure-canals/bridge-plinths` | Cool floor grade, three walkable blue canals with edge lines, translucent bridge strips, and six archive plinth/lens markers. |
| Ritual Crucible | `ECLIPSE SANCTUM` | `ritual-rings/radial-altars` | Purple floor grade, nested eclipse courts, eight ritual spokes, a shaded central well, and four radial altar/halo landmarks. |

The HUD map title uses the identity name; its tooltip retains both the original
layout name and signature. All identity sprites and lines live under a dedicated
runtime root below the scenery, render behind actors and combat effects, and add
no collider. Existing obstacle arrays remain the only collision source. The
identity texture, sprite, and materials are explicitly disposed with the world.

This is a visual-identity pass over a shared temple base, not a claim of three
fully painted biomes. The layouts still share the source floor/wall atlas,
obstacle sprite family, enemy atlas, and ambient lighting. The distinct palette,
terrain overlays, landmarks, topology, encounters, and attacks are real; unique
painted wall/floor tiles, props, lighting, and environmental story sets remain
future art scope.

## Verification

- CoreSmoke: 272 PASS on the final source tree with `TEMP`/`TMP` redirected to D.
- Static Unity source compile: 0 errors, 0 warnings.
- Unity 6000.3.24f1 Windows build: Succeeded, 0 errors, 0 warnings; build GUID
  `d233c103846346e5bb032dcfd4f1c353`.
- Final 150.062-second 1080p normal-drop run: PASS; 94 kills, three clears,
  all three layout and visual-identity mask bits, minimum 20 decorations,
  maximum zero identity colliders, all three elite attack patterns, six collected
  items, 125 saves, and zero deaths, failures, or safety restarts.
- Separate-process restore: PASS; exact disk profile restored, combat continued
  for two kills, and the continued state saved.
- Final 720p and 1080p UI probes: PASS; four actual framebuffer captures and
  16 native UI Toolkit callbacks at each resolution, with equip, talent, forge,
  bounds, and startup identity checks passing.
- Final-build OS automation: PASS in 14.149 seconds against exact PID 65816,
  title `AFFIX ZERO`, and 1280x720 client. All ten Start/equipment/talent/forge/
  pause/resume booleans are true. This is automated input, not human play.
- Direct review of the final Forge, Archive, and Eclipse frames found the
  landmarks readable, no HUD overlap, and character/damage/attack feedback
  visually dominant over the floor markings. User visual approval remains
  `NOT RUN`.

An earlier pre-tone diagnostic run also reached all three identities with 90
kills, but recorded one death and one failed run and showed an over-bright
Ritual treatment. It is retained only as diagnostic evidence. Ritual line width
and alpha were reduced before the final build and the complete final run above
passed without changing combat balance.

## Library evidence

- Forge Citadel: `libfile_6780674a96248191bfbc2df6a4c211b2`
  (`file_0000000020b0820c9569882654003eba`)
- Sunken Archive: `libfile_7b6de6b254c88191851e94c5d22c7c99`
  (`file_00000000917c81fb850e71e8b501c967`)
- Eclipse Sanctum: `libfile_5cbbec0a729881918309580c34e6c6ad`
  (`file_00000000174c82098c191fdaef3e1b42`)

All three exact titles and byte sizes were verified after upload. The prepared
Library interface was unavailable in this session, so the documented direct
batch-create fallback was used. Windows Python lacks `os.setxattr`, so local
Library identity metadata could not be attached to the retained D PNGs; the
Library creates themselves succeeded.
