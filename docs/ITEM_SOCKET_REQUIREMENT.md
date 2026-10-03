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

## Reference facts — not AFFIX rules

### Diablo II official legacy reference

The official legacy guide makes socket capacity category- and base-dependent,
not one universal value. Weapons, helms, body armor and shields are eligible;
their category ceilings are respectively 6, 3, 4 and 4, while the actual result
is further constrained by the base, item level, quality and socketing method.
The official sword table alone contains bases with maxima of 2, 3, 4, 5 and 6.
Larzuk, natural socketed drops and recipes do not all produce the same result.

- [Official socketed-items rules](https://classic.battle.net/diablo2exp/items/socketeditems.shtml)
- [Official sword base table](https://classic.battle.net/diablo2exp/items/normal/swords.shtml)

The official Cube unsocket recipe destroys inserted runes/gems/jewels. The
AFFIX proposal to return runes during salvage is therefore a user-preferred
original rule, not a claimed Diablo II behavior.

### Hero Siege community-wiki examples

The community wiki records item-specific ranges rather than one global cap:
Bonetti's Rapier 1-3, Glowstick Estoc 3-5, Shishkebab 3-6, Gem King's Raiment
6, Stoneplate 5, Knight Captain's Helmet 1-2, Eternal Death 4, Pirate Captain's
Boots 1-2, Eagle's Claw 0-1 and Azazel's Despair 1. These examples support
explicit per-item exceptions, including boots and jewelry that Diablo II does
not socket.

- [Heroic item examples](https://herosiege.wiki.gg/wiki/Heroic)
- [Helmet examples](https://herosiege.wiki.gg/wiki/Helmets)
- [Pirate Captain's Boots](https://herosiege.wiki.gg/wiki/Pirate_Captain%27s_Boots)
- [Eagle's Claw](https://herosiege.wiki.gg/wiki/Eagle%27s_Claw)
- [Azazel's Despair](https://herosiege.wiki.gg/wiki/Azazel%27s_Despair)

This community wiki may be outdated and is not treated as a complete statement
of current maxima. No positive glove example was verified in this source set.

## Recommended AFFIX contract — design recommendation only

Everything below is a coherent **DESIGN PROPOSAL, NOT IMPLEMENTATION OR A
USER-APPROVED NUMERICAL TABLE**. The user delegated the per-base/reference
approach, so the 29 capacities are one internally consistent recommendation
rather than 29 separate questions. It is an original AFFIX balance design
informed by the reference patterns above, not a copy of either game and not
authority to add socket code yet.

### Per-base data model and one validator

- **Proposed `SocketProfile`:** base-item ID, progression band, capacity and an
  optional named-item override ID. Capacity comes from this registry, never
  directly from rarity.
- **Proposed item state:** persist `openedCount` plus ordered socket contents.
  The invariant is `0 <= openedCount <= resolved profile capacity`.
- Natural generation and any future add-socket forge action must call the same
  resolver and validator. Enhancement, equip swaps, reloads and difficulty
  changes never reroll capacity or opened sockets.
- Rarity or dungeon progression may later bias the initial `openedCount` roll,
  but cannot replace the per-base capacity. No roll weights are proposed yet.
- A named unique may supply an explicit override without changing every item in
  its slot. `Relic` remains capacity 0 until a real relic source and identity
  contract exist.

### Current 29-base capacity recommendation

The tier column is the existing `LootGenerator` base tier. The capacity column
is a **design recommendation, not implementation**, and deliberately balances
innate stats, affix headroom and progression instead of assigning every
high-rarity item the same maximum.

| Slot | Base ID | Existing tier | Recommended capacity |
| --- | --- | ---: | ---: |
| Weapon | `dagger` | 1 | 2 |
| Weapon | `longsword` | 4 | 3 |
| Weapon | `axe` | 8 | 3 |
| Weapon / Staff | `magic_sword` | 13 | 4 |
| Weapon | `divine_sword` | 20 | 3 |
| Helmet | `leather_hat` | 1 | 1 |
| Helmet | `iron_helm` | 5 | 2 |
| Helmet | `mithril_helm` | 11 | 2 |
| Helmet | `dragon_helm` | 18 | 3 |
| Armor | `cloth` | 1 | 2 |
| Armor | `leather_armor` | 5 | 3 |
| Armor | `plate_armor` | 11 | 3 |
| Armor | `dragonscale` | 18 | 4 |
| Gloves | `cloth_gloves` | 1 | 0 |
| Gloves | `leather_gloves` | 5 | 1 |
| Gloves | `battle_gloves` | 11 | 1 |
| Gloves | `dragon_gloves` | 18 | 2 |
| Boots | `sandals` | 1 | 0 |
| Boots | `leather_boots` | 5 | 1 |
| Boots | `swift_boots` | 11 | 1 |
| Boots | `gale_boots` | 18 | 2 |
| Ring | `copper_ring` | 1 | 0 |
| Ring | `silver_ring` | 5 | 1 |
| Ring | `gold_ring` | 11 | 1 |
| Ring | `diamond_ring` | 18 | 2 |
| Amulet | `bone_necklace` | 1 | 0 |
| Amulet | `crystal_necklace` | 5 | 1 |
| Amulet | `ruby_necklace` | 11 | 1 |
| Amulet | `dragon_tear` | 18 | 2 |

### Balancing rationale

- Weapons receive 2-4 because they carry the most build-defining offensive
  choices. The staff peaks at 4 for its specialist two-handed identity, while
  the highest innate-damage sword stays at 3 to retain affix headroom.
- Body armor receives 2-4 and helmets 1-3: both can support defensive builds,
  but armor is the larger progression canvas and therefore owns the higher
  ceiling.
- Gloves and boots range from 0-2 so early utility pieces remain simple and
  later pieces gain customization without matching primary-slot power.
- Rings and amulets range from 0-2 because two jewelry slots can otherwise
  multiply flexible stats too quickly. The progression rises by base tier but
  is not a universal rarity formula.
- A named item may override its base profile later. This is the controlled
  exception path for Hero Siege-like individual ranges without destabilizing
  every item in a slot.

### Opened-count policy

Capacity is the immutable ceiling resolved from `SocketProfile`; `openedCount`
is the per-instance number currently available. A drop may open fewer sockets
than its capacity according to a future progression-source roll. Rarity may
bias that roll but never rewrites capacity. Forge opening may increase
`openedCount` only up to the resolved capacity, and equip, enhance, reload,
difficulty changes and salvage preview must never reroll it. Exact roll weights
and any opening cost remain implementation inputs, not part of this art task.

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

### Insert, remove and enhance

- **Proposed insertion:** from a paused management screen, consume one rune
  stack and fill one chosen empty socket in one atomic profile mutation.
- **Proposed removal:** return the rune intact and charge only existing Gold;
  the exact Gold cost is deliberately TBD. No rune destruction and no new
  extraction currency are proposed.
- **Proposed replacement:** removal must complete first; replacing a filled
  socket cannot silently destroy its rune.
- Enhancement and equipment swaps preserve capacity and filled sockets.

### Recovery and salvage policy

- **Proposed default salvage policy:** return all inserted runes to the rune
  inventory before applying the existing Gold salvage result. This is the
  user's preferred AFFIX rule. A pre-confirmation quote must list every rune
  returned or lost; if recovery cannot fit or validate, the whole transaction
  fails without changing the item or balances.
- **Future-policy seam only:** resolve salvage through a versioned recovery
  policy instead of hard-coding the outcome inside item deletion. `ReturnAll`
  is the proposed default. A future extraction service, entitlement or
  convenience item could request a separately approved policy without changing
  item identity or socket serialization. No cash item, shop, paid restriction,
  destructive baseline or monetization rule is implemented or approved now.

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
- Restore must fail closed before mutating the live profile when capacity does
  not match the resolved base/named profile, `openedCount` exceeds that
  capacity, indexes are duplicate/out of range, a rune ID is unknown, a rune
  is incompatible with the equipment slot, or a stack count is invalid.
  Atomic primary/backup behavior remains unchanged.
- Required later tests: v1-v4 to v5 migration, v5 exact round-trip, enhancement
  preservation, equip swap preservation, insert/remove rollback, salvage rune
  return, corrupt/unknown-rune rejection and separate-process restore. Every
  save-writing probe must continue using an explicit D-only test directory.

### Implementation gates

The user has approved the per-slot/per-base direction and rejected a universal
maximum of three. Keep the complete table above as the coherent v1 proposal;
do not ask for 29 individual confirmations or treat the numbers as user-approved
runtime values. It may be revised later through one explicit balance pass.
Initial `openedCount` weights, the legacy
zero-socket migration policy, add-socket source, removal cost, rune effect
catalog and acquisition source remain implementation inputs. Until those are
designed and socket code is authorized, the existing no-fake-socket rule
remains authoritative.

## Extensibility rule for current art/UI

- Do not paint permanent socket holes, gems, runes or counts into base icons.
- Keep item silhouettes centered with breathing room so runtime overlays can be
  added without repainting.
- Future item presentation should provide a separate overlay/detail region for
  empty/filled socket state rather than using rarity color or affix text.
- Do not show fake socket pips until the data model supplies authoritative
  values.

## Inputs required before implementation

1. Version the recommended 29-base table as the initial `SocketProfile`
   baseline; any later numerical change is one balance revision.
2. Define initial `openedCount` weights by progression source; rarity is not
   capacity.
3. Choose drop-time open sockets and/or a separately contracted forge
   add-socket path.
4. Define the rune catalog, effects, acquisition, insertion and removal cost.
5. Preserve the recommended atomic `ReturnAll` salvage default and versioned
   recovery-policy seam.
6. Implement and verify the fail-closed v5 migration and validation bounds.

No paid gacha or new currency is implied by the socket requirement.

## Troubleshooting record

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
| --- | --- | --- | --- |
| 아이템 소켓 UI/아트 확장 범위를 정할 수 없음 | 현행 아이템 모델, 드롭 규칙, 저장 스키마와 대장간에 소켓 상태의 권위 있는 값이 없고 과거 시안은 배치 참고뿐임 | 먼저 결정이 필요한 6개 계약을 명시하고, 현재 아이콘은 소켓을 굽지 않은 독립 원화와 향후 런타임 오버레이 영역으로 분리 | 데이터 계약 없이 소켓 구멍이나 개수를 그리면 허위 기능이 되므로 시각 표현보다 상태 모델이 선행되어야 함 |
| 안전한 소켓 초안과 현행 구조의 접점을 정리해야 함 | `WeaponItem`이 모든 장비를 표현하고 강화 시 새 인스턴스를 만들며, v4 스냅샷은 소켓/룬 인벤토리 필드가 없음 | 기존 `AffixStat`, 아이템 ID 검증, 원자적 프로필 저장을 재사용하는 v5 제안과 레거시 0소켓 마이그레이션을 비권위 초안으로 분리 | 수치와 획득 규칙을 승인값처럼 고정하지 않으면서도 복사·검증·롤백 지점을 먼저 명시할 수 있음 |
| 범용 최대 3소켓 제안이 사용자 방향과 불일치 | Diablo II와 Hero Siege 참고 모두 부위·베이스·개별 아이템에 따라 용량이 달라지며 사용자가 범용 상한을 명시적으로 거절함 | 범용 상한과 희귀도 표를 제거하고 29개 베이스별 `SocketProfile`·열린 수·명시적 named override와 단일 validator 제안으로 교체 | 참고작의 숫자를 평평하게 복사하지 말고 부위·기본 성능·확장 예외를 데이터로 분리해야 함 |
| 29개 권장 수치를 다시 개별 승인 질문으로 남김 | 사용자는 이미 참고작 기반의 부위·베이스별 설계를 위임했으며 개별 숫자 질의는 설계 책임을 되돌림 | 표 전체를 하나의 v1 권장 기준으로 확정하고 향후 변경은 단일 밸런스 개정으로 분리; 구현 입력만 별도 명시 | 위임된 설계 수치는 권장안으로 완결하되 런타임 구현 승인과 혼동하지 않아야 함 |
