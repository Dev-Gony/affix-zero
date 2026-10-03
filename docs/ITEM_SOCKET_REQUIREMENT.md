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
