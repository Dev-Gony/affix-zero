# Socket and rune vertical slice

Date: 2026-10-03

Status: implemented and verified on Unity `6000.3.24f1`, Built-in 2D.

## Shipped scope

- `SocketCatalog` is the single slot-checked source for all 29 base capacities.
  The exact capacity table in `ITEM_SOCKET_REQUIREMENT.md` is now the v1 runtime
  table; there is no universal maximum of three.
- Every socketable generated drop opens at least one socket. Additional opened
  sockets use the seeded loot RNG with a bounded rarity/floor bias and never
  exceed the immutable base capacity.
- Ten original runes use existing `AffixStat` channels only: Ember Attack +3,
  Bastion Defense +3, Vital Health +20, Aether Mana +12, Gale Speed +4%, Keen
  Critical +4, Blood Vampirism +2, Scholar Experience +5, Fortune Gold +7 and
  Piercing Penetration +4. Each rune has an explicit compatible-slot set.
- Rune inventory is a dedicated stack collection, capped at 999 per rune. It
  is not Gold, a paid currency, a shop item, a gacha result or a new stat system.
- Insert and remove are atomic. Insert consumes one compatible stack; remove
  returns the exact rune. A filled socket cannot be overwritten. If a return
  would exceed the stack cap, no item or stack state changes.
- Equip swaps and enhancement preserve capacity, opened count and ordered
  contents. Socket contributions enter the existing aggregate stat/cap path.
- Discard and salvage use `ReturnAll`. Every inserted rune is returned before
  the item is removed or Gold is credited; failure rolls back the whole action.
- A genuine route boss grants one deterministic rune through the same save
  transaction as the existing boss reward.
- Progression payload schema is v5. Schema v1-v4 loads with zero socket/rune
  state. v5 rejects mismatched capacity, invalid opened counts/indexes, unknown
  or incompatible runes, invalid stacks and broken issued-rune conservation.

## Playable UI

The right-docked 39% full-height equipment panel remains the inventory anchor.
The black comparison card floats to its left and now contains the selected
item''s authoritative capacity/open/filled summary, up to four socket cells,
the compatible owned-rune selector, `INSERT RUNE`, `RETURN RUNE`, and salvage
return preview. Cream/gold headings, white base values and purple affix/socket
rows retain the current burgundy leather and weathered frame language.

The final inspected frames show a two-capacity dagger with one Ember rune and
one open socket, an enabled return action, and `SALVAGE +4G · R1`:

- `docs/media/socket-runes/socket-rune-workbench-1280x720.png`
- `docs/media/socket-runes/socket-rune-workbench-1920x1080.png`

Library delivery: 720p `libfile_a9f819f9b3288191a8daef26790ce77f`
and 1080p `libfile_6b050e0af9e88191923894dfa7070ee9`.

The rune cell uses a compact colored gem mark rather than a separate painted
icon for every rune. This is readable and functional, but a future dedicated
rune-art pass could improve material identity. It is not required for this
vertical slice and no placeholder letter is used.

## Verification

Release-candidate build GUID: `adf711bf0a8a40b386fb012cdc944ffb`.

- CoreSmoke: `348` PASS, including 29 capacities, ten rune definitions,
  compatibility, insert/remove, enhancement/equip preservation, ReturnAll,
  rollback and v1-v4/v5 corruption/migration coverage.
- Installed Unity API static compile: 0 warnings, 0 errors.
- Unity import/Windows build: succeeded, 0 warnings, 0 errors.
- D-isolated write/read in separate Windows processes: PASS. Canonical restore,
  disk roundtrip, socketed attack 45, ordered Ember socket, rune conservation,
  resumed combat and old-kill rejection all passed.
- Native 1280x720 and 1920x1080 UI: PASS, six frames, 23 callbacks, insert,
  remove, reinsert, equip, 29 distinct icons and non-boss HP absence.
- 720p automatic hunt: PASS, boss ordinal 24, boss loot 1, rune issued 1,
  completed run 1, layout transition 1 and death retries 0.
- Exact-PID Windows input: PASS all ten actions in 17.223 seconds against an
  isolated copied D profile.
- Production profile primary/backup/lock hashes are checked before and after;
  no production restore, deletion or rewrite is part of this work.

Final user visual acceptance and human play remain pending. Town, five-tier
difficulty expansion, gacha, pets, classes, cash systems and boss sprite art
remain outside this bounded slice.

## Troubleshooting

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
| --- | --- | --- | --- |
| Unity API static compile initially rejected a socket-cell color ternary | One branch was `Color32` while the other was a resolved `StyleColor` | Cast the authored branch to `Color` so both branches have one concrete type | UI Toolkit style wrappers should be normalized before conditional assignment |
| The first save probe expected exactly six deaths | It still encoded the retired 1v1 reward timing while the current route ends on boss ordinal 24 | Bind verification to the current 24-enemy route and genuine boss reward | Runtime probes must assert current product milestones, not historical pacing |
| A later save run equipped the wrong bag index | Natural field-drop count varies by seeded run, so the socket fixture was not always index 1 | Locate the fixture by immutable item ID before equipping and allow legitimate extra bag entries | Persistent identity is authoritative; collection order is not |
| The old physical-input driver clicked the former comparison-card button position | The taller socket workbench moved `EQUIP` to the bottom of the floating card | Read final 720p bounds and move the exact-PID click to the verified button center | OS automation coordinates must be refreshed from the same final layout it verifies |
