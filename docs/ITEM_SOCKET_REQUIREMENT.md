# Item socket requirement

Date recorded: 2026-10-03

The user explicitly requires item sockets in the product direction.

## Current contract audit

There is no coherent implementation contract yet:

- `WeaponItem`, `ProgressionSnapshot`, loot generation and save schema have
  no socket fields.
- Current forge rules are deterministic enhancement only.
- Existing architecture, product and verification documents explicitly list
  sockets/runes/reforge as unimplemented or post-MVP.
- The old forge reference shows socket/rune-word UI structure, but its HTML
  performs no socket mutation and is not a gameplay specification.

Therefore this record does **not** authorize invented socket counts,
rarity/tier probabilities, rune effects, drilling costs, removal/destruction
rules, currencies, drop tables or save migration.

## Recommended v1 contract — proposal only

Everything in this section is **PROPOSED, NOT USER-AGREED**. It is a bounded
design recommendation for a later implementation review, not authority to add
socket UI, fields, drops, items or balance today.

### Eligible gear and capacity

- **Proposed:** newly generated Weapon, Helmet, Armor, Gloves, Boots, Ring and
  Amulet items can roll socket capacity. `Relic` remains schema-compatible but
  does not roll sockets until a real relic source joins the current 29-base
  loot pool.
- **Proposed hard cap:** three sockets per item.
- **Proposed rarity ranges:** Normal 0; Magic 0-1; Rare 0-2; Unique 1-2;
  Legend 1-3; Epic 2-3. These counts are not approved balance values.
- **Proposed roll rule:** capacity is rolled once by the existing seeded item
  generation path and persisted on the item. Enhancement, equip swaps, reloads
  and difficulty changes never reroll it. Base tier may bias a roll within its
  rarity range later, but no tier weighting or probability is proposed yet.
- **Proposed legacy rule:** schema-v1 through schema-v4 items migrate with zero
  sockets rather than silently gaining a rerolled advantage. A future forge
  drilling feature would require a separate user-approved contract.

### Rune effects and inventory

- **Proposed:** socket runes are stackable loot objects in a dedicated rune
  inventory, not a currency and never a paid-gacha result. Their exact drop
  source and weighting remain undecided.
- **Proposed v1 effect model:** one registered rune ID contributes one existing
  `AffixStat` value. Weapon runes favor Attack, Critical and Penetration; body
  armor runes favor Defense, Health and Speed; jewelry runes favor Mana,
  Vampirism, Experience and Gold. Exact rune values and tiers remain
  unproposed pending balance review.
- Socket contributions use the current aggregate caps already enforced by
  `HeroProgression` (critical 75, vampirism 40, experience 20, gold 30,
  penetration 50 and cooldown reduction 50). Conditional procs, rune words,
  set bonuses and new combat stats stay outside proposed v1.
- The comparison card should list socket contribution separately from white
  base stats and purple rolled affixes. Base item paintings remain socket-free;
  empty/filled state is a runtime overlay/detail row backed by real data.

### Insert, remove, enhance and salvage

- **Proposed insertion:** from a paused management screen, consume one rune
  stack and fill one chosen empty socket in one atomic profile mutation.
- **Proposed removal:** return the rune intact and charge only existing Gold;
  the exact Gold cost is deliberately TBD. No rune destruction and no new
  extraction currency are proposed.
- **Proposed replacement:** removal must complete first; replacing a filled
  socket cannot silently destroy its rune.
- Enhancement and equipment swaps preserve capacity and filled sockets.
- **Proposed salvage:** return all inserted runes to the rune inventory before
  applying the existing Gold salvage result. If either operation cannot fit or
  validate, the whole mutation fails without changing the item or balances.

### Save compatibility and validation

- **Proposed schema:** bump the progression payload from v4 to v5. Add
  `socketCapacity` plus indexed `SocketSnapshot[]` data to `WeaponSnapshot`,
  and a stack array such as `RuneStackSnapshot[]` to `ProgressionSnapshot`.
  Unity `JsonUtility` should not be asked to serialize a dictionary.
- Schema-v1 through schema-v4 migration treats missing socket arrays as empty
  and keeps every existing item ID, option, enhancement rank and ledger entry
  unchanged.
- Capture/restore and the `TryEnhance` reconstruction path must copy socket
  state explicitly. The existing issued-item identity remains authoritative;
  stackable runes need validated non-negative counts rather than fake item IDs.
- Restore must fail closed before mutating the live profile when capacity is
  outside the **proposed 0-3 bound**, indexes are duplicate/out of range, a
  rune ID is unknown, a rune is incompatible with the equipment slot, or a
  stack count is invalid. Atomic primary/backup behavior remains unchanged.
- Required later tests: v1-v4 to v5 migration, v5 exact round-trip, enhancement
  preservation, equip swap preservation, insert/remove rollback, salvage rune
  return, corrupt/unknown-rune rejection and separate-process restore. Every
  save-writing probe must continue using an explicit D-only test directory.

### Approval gates before implementation

The user still needs to approve or revise the proposed eligibility list,
rarity capacity table, legacy-zero-socket policy, removal cost/destruction
policy, rune effect catalog and rune acquisition source. Until then, the
existing no-fake-socket rule remains authoritative.

## Extensibility rule for current art/UI

- Do not paint permanent socket holes, gems, runes or counts into base icons.
- Keep item silhouettes centered with breathing room so runtime overlays can be
  added without repainting.
- Future item presentation should provide a separate overlay/detail region for
  empty/filled socket state rather than using rarity color or affix text.
- Do not show fake socket pips until the data model supplies authoritative
  values.

## Decisions required before implementation

1. Eligible equipment slots and whether every item can roll sockets.
2. Minimum/maximum count and its relationship to base tier and rarity.
3. Drop-time sockets versus forge-added sockets.
4. Rune inventory, effects, insertion, removal and destruction rules.
5. Comparison, salvage, enhancement and identity behavior.
6. Save-schema migration and validation bounds.

No paid gacha or new currency is implied by the socket requirement.

## Troubleshooting record

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
| --- | --- | --- | --- |
| 아이템 소켓 UI/아트 확장 범위를 정할 수 없음 | 현행 아이템 모델, 드롭 규칙, 저장 스키마와 대장간에 소켓 상태의 권위 있는 값이 없고 과거 시안은 배치 참고뿐임 | 먼저 결정이 필요한 6개 계약을 명시하고, 현재 아이콘은 소켓을 굽지 않은 독립 원화와 향후 런타임 오버레이 영역으로 분리 | 데이터 계약 없이 소켓 구멍이나 개수를 그리면 허위 기능이 되므로 시각 표현보다 상태 모델이 선행되어야 함 |
| 안전한 소켓 초안과 현행 구조의 접점을 정리해야 함 | `WeaponItem`이 모든 장비를 표현하고 강화 시 새 인스턴스를 만들며, v4 스냅샷은 소켓/룬 인벤토리 필드가 없음 | 기존 `AffixStat`, 아이템 ID 검증, 원자적 프로필 저장을 재사용하는 v5 제안과 레거시 0소켓 마이그레이션을 비권위 초안으로 분리 | 수치와 획득 규칙을 승인값처럼 고정하지 않으면서도 복사·검증·롤백 지점을 먼저 명시할 수 있음 |
