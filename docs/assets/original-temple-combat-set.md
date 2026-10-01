# Original Temple Combat Set v1

Generated on 2026-10-01 with the built-in OpenAI `image_gen` tool through the repository-required imagegen workflow. The prompts required an original design and explicitly prohibited copying, tracing, extraction, logos, text, or assets from Hero Siege or any other commercial game. No third-party sprite, paid pack, Hero Siege file, or local-only Zerie file was supplied to the generator. These files are project-owned generated outputs; retain this record with redistributed binaries and do not describe them as Hero Siege assets.

## Runtime contract

- Character atlases: 1024x1024 RGBA, 8 columns x 8 rows, 128x128 cells, Point filtering, no mipmaps, uncompressed, PPU 80, pivot `(0.5, 0.14)`.
- Directions in row pairs: Down rows 0-1, Left 2-3, Right 4-5, Up 6-7.
- Locomotion/reaction row: idle columns 0-1, walk 2-5, hit 6-7.
- Combat row: attack columns 0-4, death 5-7.
- Attack: 12 FPS, impact frame index 2, five frames total. Movement 8 FPS; hit 16 FPS; death 8 FPS.
- Body and weapon are integrated in every character frame. `attackWeapon` is intentionally empty.
- Hit FX: 1024x1024 RGBA, 4 columns x 2 rows, eight frames in reading order, 20 FPS, PPU 160.
- Room: 1536x1024 opaque RGB, PPU 48, 32 world units wide. The 28x16 runtime grid remains authoritative for movement. Four corner platforms correspond to the blocked corner cells; the two central blocker locations remain open in the baked room and use `TempleObstacle-v2` at x=-4 and x=4.
- Obstacle: 1024x768 RGBA, displayed 2.4 world units wide with a 2x2 collider and the same two-cell navigation footprint as the existing runtime grid.

## Generation prompts

The two character prompts requested a strict 8x8 transparent production atlas for a compact top-down warrior / obsidian raider, respectively, with the exact row mapping above; consistent identity, palette and integrated weapon; visibly distinct idle, walk, attack, hit and death poses; crisp limited-palette 16-bit pixel clusters; four genuinely drawn directions; fixed foot anchor; and no text, grid, floor, UI, watermark, reference-game design, copying, or tracing.

The impact prompt requested a strict 4x2 transparent atlas progressing from a tiny white-gold contact spark through a full ivory/amber burst to separated copper fragments and final fading flecks, with hard pixel clusters and no characters, weapon, floor, smoke, bloom, text, watermark, copying, or tracing.

The room prompt requested an opaque 1536x1024 orthographic basalt temple chamber with a broad low-noise center, solid perimeter walls, north doorway, south exit, four 3x2-cell corner platforms, empty central blocker positions, teal inlays and restrained amber perimeter lamps. It prohibited characters, props at blocker locations, UI, text, logos, blood, lava, giant emblems, reference-game maps, copying, and tracing.

The obstacle prompt used the generated room only as a palette/material reference and requested one new low 4:3 basalt-and-teal barricade on transparent alpha, with a visible top and short south face, tight contact shadow, no room/floor/character/UI/text/copying/tracing.

Built-in square outputs were 1254x1254 and the obstacle was 1448x1086. `Tools/Art/NormalizeGeneratedAtlases.py` deterministically reslices each generated cell and applies nearest-neighbor normalization to the declared production dimensions. The room already matched 1536x1024. Final inspection found 64/64 unique non-empty cells in each character atlas and 8/8 unique non-empty hit-FX cells.

## Final files

| File | SHA-256 after normalization |
| --- | --- |
| `HeroAtlas-v1.png` | `30748cbd34d93dcdc2b349a23b7ffbc68f8d075f55e90c411a4390a28469b019` |
| `EnemyAtlas-v1.png` | `23b22dd9438a8b35e62873070ea9c2f424cb24e9d215149d20d34e53b3e38b3a` |
| `ImpactFxAtlas-v1.png` | `94eda9f5a885c222e2d3e6e6075d751d217b5800294338546a2c5d93f5a11aff` |
| `TempleRoom-v2.png` | `0381c593a003397c5e1347f3829703d332875a5c71b799ba2b2f38f6bbb83b5f` |
| `TempleObstacle-v2.png` | `3889f2a114df1e26a557a00c5e7e0758942b3884e6097be911e9d2b14218da67` |

Visual generation and deterministic checks do not establish user visual acceptance. Unity import, Play, and captured combat evidence are recorded separately.
