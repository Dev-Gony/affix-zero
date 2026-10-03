# AFFIX: ZERO 한국어 기본 표시

작성일: 2026-10-03

## 범위와 원칙

현재 플레이어 기본 언어는 한국어다. 이번 작업은 HUD, 장비/가방, 비교 툴팁,
희귀도와 옵션, 특성, 대장간, 소켓과 룬, 지역과 난이도, 보스 이름과 공격
예고, 저장 및 진행 안내를 한국어로 통일했다. `AFFIX: ZERO` 서비스명과 C#
enum, 세이브 키, 아이템·룬 ID, 리소스 경로는 바꾸지 않았다.

`Assets/_Game/Presentation/KoreanDisplay.cs`가 안정 ID를 현재 한국어 표시명으로
변환한다. 기존 저장에 영어 아이템/옵션 이름이 있어도 29개 기본 아이템은
아이콘 리소스 ID, 옵션은 `AffixStat`, 룬은 룬 ID를 기준으로 표시한다. 이
경계는 향후 영어 카탈로그를 추가할 수 있게 하지만, 이번 범위에는 언어 선택
UI나 완전한 이중 언어 시스템을 넣지 않았다.

이미지 안에 영어 문구를 새로 굽지 않았다. 현재 프레임·아이콘은 장식과 그림이며
표시 문구는 UI Toolkit 텍스트다. `I`, `Tab`, `K`, `Esc`는 실제 키 이름,
`1x/2x/4x`는 속도 배율, `m`은 거리 단위로 유지한다.

## 기준 용어

| 내부 ID/기존 계열 | 한국어 표시 |
|---|---|
| Fury / Precision / Keystone | 분노 / 정밀 / 숙련 |
| Vitality / Cleave / Haste | 강인함 / 휩쓸기 / 가속 |
| Scout / Veteran / Torment | 정찰 / 숙련 / 고행 |
| normal / magic / rare / unique / legend / epic | 일반 / 매직 / 레어 / 유니크 / 전설 / 에픽 |
| Attack / Defense / Health / Mana | 공격력 / 방어력 / 최대 체력 / 마나 |
| Speed / Critical / Vampirism | 공격 속도 / 치명타 확률 / 흡혈 |
| Experience / Gold / Penetration | 경험치 획득 / 골드 획득 / 방어 관통 |
| ARC / QUAKE / LANCE | 연쇄 참격 / 충격파 / 관통 창 |
| AREA / HEAL / WIND / SURGE | 범위 참격 / 자동 회복 / 생명의 바람 / 생명 쇄도 |
| SOCKET / RUNE / SALVAGE | 소켓 / 룬 / 분해 |
| ASHEN TYRANT | 잿빛 폭군 |
| DROWNED ARCHON | 익사한 집정관 |
| ECLIPSE SOVEREIGN | 일식의 군주 |
| RUIN PULSE | 파멸의 파동 |

룬 10종은 불씨, 보루, 생명, 에테르, 질풍, 예리함, 피, 현자, 행운,
관통 룬으로 표시한다. 룬 효과는 문자열 이름이 아니라 실제 `AffixStat`과 값을
읽어 한국어로 만든다.

## 실제 검증

- CoreSmoke: 348 PASS.
- Unity API 정적 컴파일: 경고 0, 오류 0.
- Unity 6000.3.24f1 import/Windows build: 경고 0, 오류 0.
- 최종 빌드 GUID: `4228d559f7c94a7289e52e66a69f2fb0`.
- 1280x720 Reference UI: PASS, 실제 framebuffer 6장, 네이티브 콜백 23개,
  한국어 라벨 32개, 29개 아이콘, 1x/2x/4x, 장착/비교/룬 삽입·회수,
  특성/강화, 플레이어·보스 전용 체력 표시 확인.
- 1920x1080 Reference UI: 같은 계약으로 PASS, 실제 framebuffer 6장.
- 1280x720 자동사냥 회귀: 24번째 처치가 보스, 보스 패턴 2회, 자동 회피
  1회, 보스 전리품 1개, 룬 1개, 다음 구간 전환 1회 PASS.
- 번들 `Resources/AffixUI/Korean.otf`가 대표 한글 음절과 실제 라벨을 렌더링함을
  런타임에서 검사했다. 직접 픽셀 검사에서도 대체 문자, 잘린 긴 옵션, 버튼 겹침,
  작은 아이콘 위 글자 충돌을 발견하지 않았다.
- 모든 플레이어 프로브는 명시적 D: 테스트 저장 경로를 사용했다. 운영 primary,
  backup, lock SHA-256은 전후 동일하다.

추적 화면은 `docs/media/korean-localization/`에 있다. 전체 보고서는 Git 제외
경로 `Build/Reports/korean-localization-ui-{720,1080}-visible-20261003/`와
`Build/Reports/korean-localization-boss-loop-720-20261003/`에 남아 있다.

## 시각 검토와 남은 범위

실제 720p/1080p 화면에서 장비 비교, 소켓 작업대, 6개 특성, 대장간, 보스 예고를
검토했다. 720p의 우측 전투 상태 글자는 의도적으로 작고 조밀하지만 판독 가능하다.
번역 때문에 새로 겹치거나 화면 밖으로 나간 주요 컨트롤은 없다. 최종 사용자 시각
승인은 아직 받지 않았다. 영어 버전, 언어 선택 UI, 이미지 현지화 시스템은 후속
범위다.

마을/5단계 난이도 실험은 한국어 우선 지시 전에 시작됐고 검증되지 않았다.
현재 브랜치에는 적용하지 않았으며 로컬 `stash@{0}`
(`wip-town-five-difficulties-before-korean-localization-2026-10-03`)에 보존했다.

## 트러블슈팅

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
|---|---|---|---|
| 첫 720p 한국어 검증 초기화 | 프로브의 `Text` 도우미가 이름 있는 버튼 자체를 `Label`로 단정했다. | 버튼이면 자식 `Label`까지 조회하도록 도우미를 보정하고 새 빌드에서 재실행했다. | UI Toolkit의 클릭 요소와 표시 Label은 별도 요소일 수 있다. |
| 숨김 창 720p 두 번째 캡처 | 숨김 Windows 플레이어에서 `ReadPixels`가 그리기 프레임 밖에서 호출됐다. | 실제 화면 검증 범위에 한해 정상 표시 창으로 재실행했고 6개 framebuffer를 모두 획득했다. | 다중 실제 framebuffer 검증은 숨김 창 PASS로 대체할 수 없다. |
| 보고서 한글의 PowerShell 출력 | 기본 콘솔 디코딩이 UTF-8 JSON을 잘못 표시했다. 파일과 실제 화면 글리프는 정상이다. | `Get-Content -Encoding utf8 -Raw`로 JSON을 읽고 실제 BMP/PNG를 직접 검사했다. | 콘솔 모지바케와 게임 렌더 글리프 결함을 구분해야 한다. |

