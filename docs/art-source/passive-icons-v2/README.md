# Painted passive icon v2 source

These three transparent PNGs are project-original dark-fantasy passive symbols
generated and visually inspected in the parent cloud workflow. They replace
only the three 16 x 16 pixel-style assets already used by `TalentPanel`.

They were transferred without merging, cherry-picking or checking out the
isolated asset-only branch:

- source commit: `386485529268e2a964a5f168e56e69fe524ad607`
- source tree: `38a0a5051e277703b6acf72977aa7b639a8d7e29`
- `PowerRune.png` blob: `a2204c490e5e893cf7e69277b711a8d44217169f`
- `PrecisionRune.png` blob: `37f29038082e40ad0f3d0d1e328133babc8d462c`
- `VeteranRune.png` blob: `9d3919c7370b082fb701c070fce6450cf033f477`

`Tools/Local/Generate-AffixPassiveIconArt.ps1` validates the exact source
SHA-256 values, performs only a high-quality transparent downsample into a
128 x 128 canvas with four pixels of outer padding, rejects border alpha and
duplicate output hashes, and emits a 128/64/48/37 pixel readability sheet.

The original masters remain outside `Assets/` so Unity does not import the
large files. Runtime files keep the existing `AffixGenerated/{PowerRune,
PrecisionRune,VeteranRune}` resource paths and existing `.meta` GUIDs. No
talent rule, rank, save field, socket state or gameplay system is encoded in
the paintings.
