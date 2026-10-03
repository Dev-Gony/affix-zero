# Painted passive icon art brief

Date: 2026-10-03

This document began as an art-readiness brief and now records the completed
six-icon integration. It does not change a talent rule or claim final user
visual approval.

## Final integration

Only the three requested PNG blobs were extracted from source-only commit
`386485529268e2a964a5f168e56e69fe524ad607` (tree
`38a0a5051e277703b6acf72977aa7b639a8d7e29`). The asset branch was not merged,
cherry-picked or checked out. Exact original blob and SHA-256 provenance is in
`docs/art-source/passive-icons-v2/README.md`.

The final three requested blobs were then extracted from asset-only commit
`2c35377ae79cfceea4fa7fae8134e95427d9014e` (tree
`f8fa817956ac1529f6b799319ff534ffa7cb2109`) by the same no-merge,
no-cherry-pick and no-checkout process. That commit adds only
`VitalityRune.png`, `CleaveRune.png` and `HasteRune.png`.

`Tools/Local/Generate-AffixPassiveIconArt.ps1` creates deterministic 128 x 128
transparent runtime icons with four pixels of outer padding. All six have
zero border-alpha pixels and unique output hashes:

- `PowerRune.png`: `6B6E3B1FA97F278D0F9B1C5D447ADF18C310646DBD04591976517A61668FB03E`
- `PrecisionRune.png`: `83D35C386C532AD80EE04884F243F539866D5EE236C5D5F51C85360E0F07A5B0`
- `VeteranRune.png`: `FA2203FEB45E1D7E31F15E0D1123DD89D6BF4A160D086E18B40F3138F8F66CE5`
- `VitalityRune.png`: `016B6DC1CA2B960D0694676624DFDA775B31CC64315403FECA70B096E3A78DEE`
- `CleaveRune.png`: `9AB99DB8673E4B1C6F22C85374822ACFF608DCC86085B5F7735210702E110FE4`
- `HasteRune.png`: `E74D69A4354CA9DF197FA94E1460871D115EA2BD0872FD415635C03865CBAB66`

The first three resource paths and Unity GUIDs remain unchanged. The final
three use new dedicated `AffixGenerated` paths and GUIDs, while the existing
HUD `SkillHeal`/`SkillArea` and equipment `gale_boots` assets remain unchanged.
Import filtering is bilinear to preserve the painted downscale at the actual
48 x 48 node size. No rank, stat, save, socket or gameplay behavior changed.

## Exact six-node mapping

`TalentPanel` displays each dedicated painted icon at 48 x 48 pixels.

| Node | Runtime meaning | Current resource | Current file |
| --- | --- | --- | --- |
| 분노 / Fury (`TalentId.Fury`) | +3 attack per rank, maximum rank 20; ranks 5 and 15 evolve automatic area damage and select ARC, QUAKE, or LANCE from the equipped weapon style | `AffixGenerated/PowerRune` | `Assets/Art/Lucifer/Resources/AffixGenerated/PowerRune.png` |
| 정밀 / Precision (`TalentId.Precision`) | +4 attack per rank, maximum rank 10; requires Fury rank 2; ranks 3 and 8 add +5 and +10 total critical chance | `AffixGenerated/PrecisionRune` | `Assets/Art/Lucifer/Resources/AffixGenerated/PrecisionRune.png` |
| 숙련 / Keystone (`TalentId.Keystone`) | +6 attack per rank, maximum rank 5; requires Precision rank 1; ranks 2 and 5 add 5 and 12 total armor penetration | `AffixGenerated/VeteranRune` | `Assets/Art/Lucifer/Resources/AffixGenerated/VeteranRune.png` |
| 생명력 / Vitality (`TalentId.Vitality`) | +10 maximum health per rank; ranks 5 and 15 evolve automatic low-health recovery | `AffixGenerated/VitalityRune` | `Assets/Art/Lucifer/Resources/AffixGenerated/VitalityRune.png` |
| 휩쓸기 / Cleave (`TalentId.Cleave`) | +0.08 melee splash radius and +2.5% surrounding damage per rank; ranks 3 and 8 evolve area behavior | `AffixGenerated/CleaveRune` | `Assets/Art/Lucifer/Resources/AffixGenerated/CleaveRune.png` |
| 가속 / Haste (`TalentId.Haste`) | -2% attack wait per rank; ranks 3 and 8 improve auto-skill arming behavior | `AffixGenerated/HasteRune` | `Assets/Art/Lucifer/Resources/AffixGenerated/HasteRune.png` |

The final three replace only the TalentPanel references. `EncounterHud` still
uses `AffixUIVisual/SkillHeal` and `SkillArea`, and equipment still uses
`AffixUIVisual/Items/gale_boots`.

## Shared generation contract

- Generate a square, project-original painted ARPG talent symbol with a fully
  transparent background (`transparent_background=true`).
- Use a strong isolated silhouette, hand-painted metal/energy material,
  weathered edges, deep value separation, and restrained warm rim light. Match
  the illustrated-v2 equipment's dark steel, aged brass, burgundy and jewel
  accents without copying any external game asset.
- Keep roughly 8-10% transparent breathing room. The symbol must remain clear
  at the actual 48 x 48 node size and at 128 x 128 runtime resolution.
- Do not add a UI frame, square plate, opaque disc, text, letters, numbers,
  rank marks, rarity color, socket holes, gems, or rune-word text.
- Avoid flat vector geometry, pixel-art stair steps, generic mobile-app
  gradients, repeated silhouettes, and a plain recolor of another icon.
- Preserve existing Unity `.meta` files when replacing runtime PNGs and assign
  unique stable GUIDs to genuinely new dedicated resources.
  Preserve cloud masters and provenance separately under
  `docs/art-source/passive-icons-v2/` before producing deterministic 128 x 128
  runtime versions at the exact current paths.

## Per-icon art direction

### 분노 / Fury — `PowerRune.png`

Paint a compact crimson battle-energy core wrapped by three asymmetric,
blade-like flame strokes and small aged-brass fragments. It should read as
controlled offensive fury and an expanding automatic strike, not as one
specific weapon, because the real skill becomes sword ARC, axe QUAKE, or staff
LANCE. Use hot orange-white at the core, deep crimson outer energy, dark steel
occlusion, and a few sharp sparks. Keep the outer silhouette irregular and
aggressive.

### 정밀 / Precision — `PrecisionRune.png`

Paint a fine steel needle or arrowhead crossing a broken, engraved brass
sighting ring with a tiny emerald focus glint. The focal point must read as
accuracy and critical timing at thumbnail size without becoming a flat modern
crosshair. Use narrow highlights, dark oxidized recesses, and one crisp central
alignment point.

### 숙련 / Keystone — `VeteranRune.png`

Paint a tempered steel chisel or spear tip driving through a cracked dark armor
plate shaped loosely like a keystone. Add restrained violet energy inside the
fracture and aged-gold edge inlay. It must read as mastery plus armor
penetration, not as a defensive shield. Keep the piercing diagonal and broken
plate silhouette unmistakable at 48 x 48.

### 생명력 / Vitality — `VitalityRune.png`

The final art is a faceted red heart gem protected by layered silver and aged
gold leaves, with a restrained green recovery pulse. It reads as maximum health
plus emergency recovery rather than a generic medical cross.

### 휩쓸기 / Cleave — `CleaveRune.png`

The final art is a weathered steel axe driving a broad amber crescent and stone
fragments. Its curved motion remains legible as surrounding damage at 37 px.

### 가속 / Haste — `HasteRune.png`

The final art is a cyan hourglass surrounded by forward-swept silver wings and
energy ribbons. It reads as shortened attack waiting and auto-skill arming,
without reusing footwear.

## Completed verification

- All six transparent masters and deterministic 128/64/48/37 pixel previews
  were directly inspected. The silhouettes remain distinct at 48 and 37 px.
- CoreSmoke: 321 PASS. Unity API static compile: 0 errors / 0 warnings.
- Unity 6000.3.24f1 import and Windows build: PASS, 0 errors / 0 warnings;
  final six-icon build GUID `04032b1b12884ed8a78a349fda6c50d3`.
- Native 1280 x 720 and 1920 x 1080 UI probes: PASS with five frames and 19
  callbacks each. Both separately capture locked Precision and selected
  Keystone states; all 29 base icons load, player HP is present and non-player
  HP remains absent.
- Final run IDs are `aaa422f4174e459ba3440b704e376368` at 720p and
  `fc7c1558bcfb48d49d4a38096af0b561` at 1080p. Physical OS input was not rerun
  because other D-drive projects were active; prior ten-action evidence remains
  historical and native UI callbacks cover this visual-only mapping change.
- Production primary, backup and lock hashes remain unchanged. Every save-
  writing run used a distinct explicit D-only test directory.

Final tracked evidence:

- `docs/media/passive-icons-v2/talent-panel-six-painted-1080p.png` — 1920 x
  1080, SHA-256 `A1321F4054DB4C72B892F1893097D0341234CF23043AD87B86B2438323764A22`;
  decoded RGB is identical to the native BMP.
- `docs/media/passive-icons-v2/passive-icons-six-scale-sheet.png` — 900 x 896,
  SHA-256 `4755CD2DFB39C4D6A0CF82097EF1D90911DB946157B5FAF1E45C231CD3F42BF5`.

Library evidence:

- 1080p locked Precision: `libfile_32fb720532b48191abf17fb5d2658dd4`
- 1080p selected Keystone: `libfile_6aed0536f5888191a33444950e73daeb`
- 128/64/48/37 readability sheet: `libfile_95a67962ff5081918154adfb7390b788`

Prepared upload discovery was unavailable inside the helper runtime, so the
supported ordered direct-create fallback stored all three files. Windows lacks
`os.setxattr`; only optional local Library identity metadata failed.

## Troubleshooting record

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
| --- | --- | --- | --- |
| 장비 원화와 특성 화면의 첫 세 아이콘 화풍이 불일치 | 실제 노드가 48 x 48 영역에서 16 x 16 픽셀 리소스를 확대하며, 후반 세 노드는 128 x 128 리소스를 사용함 | 코드의 `Resources.Load` 경로와 원본 PNG를 직접 대조하고, 세 노드의 실효과를 각각 다른 회화적 실루엣으로 고정 | 이름만 보고 추상 룬을 만들기보다 실제 효과와 실제 표시 크기를 함께 기준으로 삼아야 함 |
| 첫 최종 OS 입력 시도가 시작 단계에 머묾 | 기존 드라이버가 Start 버튼의 우측 경계 x=274를 클릭해 실제 히트 영역을 놓침 | 실패 보고서를 보존하고 D 로컬 드라이버의 클릭을 중앙 x=251로 이동 | 자동 입력 좌표는 시각적으로 보이는 경계가 아니라 안정적인 내부 지점을 사용해야 함 |
| 두 번째 OS 입력 시도가 입력 전에 중단 | Windows가 정확한 플레이어 창의 foreground 전환을 일시적으로 거부함 | 정확한 D 빌드 PID만 종료하고 새 세이브/보고 폴더에서 재실행하여 세 번째 시도 PASS | 포커스 획득 실패는 게임 회귀와 분리하고 새 격리 실행으로 판정해야 함 |
| Library 준비 업로드 helper가 시작 전 중단 | helper 실행 환경에 `prepare_uploads`가 노출되지 않음 | 중복 업로드 없이 신규 파일 전용 ordered direct-create fallback 사용 | 준비 단계가 시작되지 않은 실패만 명시된 대체 경로로 전환할 수 있음 |
| Fury의 UI 노드와 코어 상세 한글명이 다름 | `TalentPanel`은 `분노`, `TalentDefinition`은 `격노`를 사용해 선택 전후 명칭이 달라짐 | 기존 사용자-facing UI와 주요 문서에서 우세한 `분노`로 코어 정의와 아트 문서를 통일 | 동일한 enum이라도 노드 배열과 권위 있는 정의 이름을 함께 대조해야 함 |
| 표기 수정 후 정적 컴파일 명령이 두 번 시작 전 중단 | 첫 명령은 스크립트 위치를 `Tests/CoreSmoke`로 오인했고, 두 번째는 필수 `UnityEditorPath`를 생략함 | 실제 `Tools/Local/Test-UnitySources.ps1`를 승인된 D Unity 절대 경로 매개변수와 함께 실행해 0경고/0오류 확인 | 검증 실패와 호출 오류를 분리하고 스크립트의 실제 위치·필수 매개변수를 먼저 확인해야 함 |
| Unity 빌드 래퍼가 성공 빌드를 실패처럼 표시 | Windows GUI 실행 뒤 PowerShell `$LASTEXITCODE`가 비어 있었음 | 새 프로세스를 중복 실행하지 않고 Unity 종료, 최신 보고서 시각, `Succeeded` 결과와 0/0 로그를 직접 확인 | GUI 프로세스는 셸 exit 변수보다 생성된 보고서와 실제 프로세스 종료를 판정 근거로 사용해야 함 |
| 최종 BMP→PNG 변환 검증 helper가 시작 전 두 번 중단 | 인메모리 C# 컴파일에 PowerShell 런타임의 전달된 GDI 어셈블리 참조가 연쇄적으로 필요했음 | 파일 생성 전 중단을 확인하고 PowerShell `BitmapData` 기반 RGB 해시 비교로 전환 | 보조 검증기는 런타임 어셈블리 결합을 피하고 단순한 디코딩 바이트 비교를 우선해야 함 |
| BMP를 직접 PNG로 저장한 첫 파일이 투명하게 보임 | 32-bit BMP의 사용하지 않는 알파 채널이 PNG 알파로 보존됨 | 검사를 실패 처리하고 24-bit RGB 캔버스에 복사해 다시 저장, 원본 BMP와 디코딩 RGB SHA-256 일치 확인 | framebuffer BMP의 RGB 증거는 명시적 불투명 변환과 디코딩 픽셀 비교가 필요함 |
