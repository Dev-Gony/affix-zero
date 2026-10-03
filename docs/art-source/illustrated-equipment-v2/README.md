# Illustrated equipment v2 source

These 29 transparent PNGs are project-original dark-fantasy equipment art
generated in the parent cloud workflow. No local C-drive generation/cache,
third-party source image, paid asset, or discarded Godot art was used.

They were transferred without merging, cherry-picking or checking out the
isolated asset-only branch:

- final source commit: `98ac9a3614f44bd0c3104e945dd091792285785a`
- final source tree: `326a3705993045185dd691e8e65e69b6a7f5c444`
- approved representative source commit:
  `5b4933ccb2b2bfad1158a26aa7917d561bdc60cf`
- `sword.png` blob: `c10a8ecdc41d1c0b3676b193e5a09d30f856a0c9`
- `armor.png` blob: `2b42786d7a0b3e7dc4ba70fc6187211950945933`
- `gloves.png` blob: `70803f12e0b01e7addaae143d227aaf998a86992`

The three representative filenames map to `longsword`, `plate_armor` and
`battle_gloves`. The other 26 source filenames equal their base IDs. The final
`magic_sword.png` depicts the runtime Arcane Staff. Regenerated final versions
of `dagger`, `leather_gloves` and `sandals` use clean transparent cutouts;
the latter two intentionally show a single readable wearable. The engraved
`copper_ring` intentionally has no gemstone. These art choices do not change
item stats or rules.

`Tools/Local/Generate-AffixEquipmentArt.ps1` performs only a high-quality
transparent downsample. It preserves the existing scale of the three approved
representatives and aspect-fits each of the other 26 masters into a 128 x 128
canvas with four pixels of outer padding. Rarity edges and any future socket
state remain runtime overlays and are not baked into the paintings.

The original source PNGs remain outside `Assets/` so Unity does not import the
large masters.
