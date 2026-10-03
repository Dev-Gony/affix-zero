# Painted passive icon v2 source

These six transparent PNGs are project-original dark-fantasy passive symbols
generated and visually inspected in the parent cloud workflow. Together they
provide one dedicated painted symbol for every existing `TalentPanel` node.

They were transferred without merging, cherry-picking or checking out the
isolated asset-only branch:

- source commit: `386485529268e2a964a5f168e56e69fe524ad607`
- source tree: `38a0a5051e277703b6acf72977aa7b639a8d7e29`
- `PowerRune.png` blob: `a2204c490e5e893cf7e69277b711a8d44217169f`
- `PrecisionRune.png` blob: `37f29038082e40ad0f3d0d1e328133babc8d462c`
- `VeteranRune.png` blob: `9d3919c7370b082fb701c070fce6450cf033f477`

The final three were transferred from the next asset-only commit, again
without merging, cherry-picking or checking out that branch:

- source commit: `2c35377ae79cfceea4fa7fae8134e95427d9014e`
- source tree: `f8fa817956ac1529f6b799319ff534ffa7cb2109`
- `VitalityRune.png` blob: `7c5f51d45509f4888a343131af3d7c41ff3a507d`
- `CleaveRune.png` blob: `b59ac4ccfc3e8923eb7098e2cfbd305c1bb7052b`
- `HasteRune.png` blob: `9a1a511a7a92f18e60cf6f1f2fbc4dee96c8e9c4`

`Tools/Local/Generate-AffixPassiveIconArt.ps1` validates the exact source
SHA-256 values, performs only a high-quality transparent downsample into a
128 x 128 canvas with four pixels of outer padding, rejects border alpha and
duplicate output hashes, and emits a 128/64/48/37 pixel readability sheet.

The original masters remain outside `Assets/` so Unity does not import the
large files. Runtime files use `AffixGenerated/{PowerRune,PrecisionRune,
VeteranRune,VitalityRune,CleaveRune,HasteRune}`. The first three keep their
existing `.meta` GUIDs; the final three receive new dedicated GUIDs. The
separate `SkillHeal`, `SkillArea` and `Items/gale_boots` resources remain
unchanged for HUD and equipment use. No talent rule, rank, save field, socket
state or gameplay system is encoded in the paintings.
