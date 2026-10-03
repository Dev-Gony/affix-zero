# Illustrated equipment v2 source

These three transparent 1254 x 1254 PNGs are project-original dark-fantasy
equipment prototypes generated in the parent cloud workflow. No local C-drive
generation/cache, third-party source image, paid asset, or discarded Godot art
was used.

They were transferred without merging or checking out the isolated transfer
branch:

- transfer commit: `5b4933ccb2b2bfad1158a26aa7917d561bdc60cf`
- `sword.png` blob: `c10a8ecdc41d1c0b3676b193e5a09d30f856a0c9`
- `armor.png` blob: `2b42786d7a0b3e7dc4ba70fc6187211950945933`
- `gloves.png` blob: `70803f12e0b01e7addaae143d227aaf998a86992`

`Tools/Local/Generate-AffixEquipmentArt.ps1` performs only a high-quality
transparent downsample for:

- `sword.png` -> `AffixUIVisual/Items/longsword.png`
- `armor.png` -> `AffixUIVisual/Items/plate_armor.png`
- `gloves.png` -> `AffixUIVisual/Items/battle_gloves.png`

The original source PNGs remain outside `Assets/` so Unity does not import the
large masters. The 26 other base-item icons are earlier placeholders pending
replacement and are not approved as final art.
