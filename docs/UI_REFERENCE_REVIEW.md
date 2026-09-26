> 과거 조사 기록. 최신 지시로 Survivor.io 요소는 전면 폐기됐다. 현행 기준은 HERO_SIEGE_UX_SPEC.md이며 아래 혼합 방향은 더 이상 유효하지 않다.

# UI 레퍼런스 검토 — Hero Siege / Survivor.io와 거절된 화면의 차이

검토일: 2026-09-26. 사용자에게 현재 AFFIX 화면의 UI 방향을 명시적으로 거절받았다. 현재 시각 승인 상태는 REJECTED이며 U2 확장은 보류한다. 이 문서는 레퍼런스 조사 기록으로, 새 UI 템플릿이나 확정 디자인 사양이 아니다. 지정된 기존 인수인계 문서 7개와 UI 코드는 이 조사에서 수정하지 않았다.

## 확인한 출처와 범위

1. [HABBY 공식 Google Play Survivor.io](https://play.google.com/store/apps/details?id=com.dxx.firenow&hl=en_US): Chrome에서 스크린샷 뷰어의 전투/Skill Selection 이미지를 직접 확인했다. 게임 화면에 대형 캐릭터·로고·마케팅 문구가 합성되어 있으므로 이 장식은 실제 HUD로 분류하지 않는다. 개발자 설명은 한 손 조작과 roguelite skill 조합을 명시한다.
2. [HABBY 공식 App Store](https://apps.apple.com/gb/app/survivor-io/id1528941310): 개발자가 HABBY임과 같은 장르/조작 설명을 교차 확인했다.
3. [Uptodown의 Survivor.io 플레이 소개](https://blog.en.uptodown.com/survivor-io-requirements-and-how-to-play/), [실제 화면 3종 이미지](https://blog.uptodown.com/wp-content/uploads/survivor-io-887x526.jpg.webp): Chrome으로 일반 전투·보스 경고·스킬 선택을 나란히 담은 원본 이미지를 직접 확인했다. 공식 자료가 아닌 과거 플레이 기록으로서 배치 관찰에만 사용한다. 현행 버전의 모든 버튼/수치를 보증하지 않는다.
4. [Game UI Database](https://www.gameuidatabase.com/): 웹 검색 도구는 robots 제한이 있었지만 Chrome에서 사이트와 실제 검색 UI를 확인했다. Survivor 검색은 Star Wars Jedi: Survivor와 Soulstone Survivors를 반환했으며 Survivor.io 직접 항목은 확보하지 못했다. 다른 게임을 Survivor.io인 것처럼 대체하지 않는다.

Game UI Database에서 실제 확인한 분류 링크: [Player Vitals](https://www.gameuidatabase.com/index.php?&scrn=133), [Equipped Items & Abilities](https://www.gameuidatabase.com/index.php?&scrn=131), [Collection Counters](https://www.gameuidatabase.com/index.php?&scrn=139), [Clock & Timer](https://www.gameuidatabase.com/index.php?&scrn=137), [Enemy Health & Damage](https://www.gameuidatabase.com/index.php?&scrn=143). 이는 각 정보의 역할을 나눠 살펴보는 참고 분류이며 이 페이지의 모든 예시를 AFFIX에 도입하라는 지시가 아니다.

주의: `survivor.io` 도메인의 현재 웹사이트는 HABBY와 무관한 별도 브라우저 미니게임임을 자체 고지한다. 공식 HABBY 레퍼런스로 사용하지 않는다. Game UI Database는 AI asset generation 등에 콘텐츠 사용을 금지한다고 고지한다. 이 조사에서는 외부 UI 이미지를 게임 자산이나 생성형 이미지 입력으로 반입하지 않았고 배치만 관찰했다.

## 직접 관찰한 배치

| 항목 | 이미지에서 확인한 사실 | 현재 AFFIX의 다른 점 |
|---|---|---|
| 화면 형태 | 플레이 화면 각각은 세로형이며 전장이 화면 대부분을 차지한다 | 현재 1280×720 가로 화면은 큰 상·하단 카드가 있는 고정 1대1 장면이다 |
| 최상단 진행 | 얇은 가로 레벨 진행 바와 끝의 레벨 숫자, 그 위에 중앙 시간·작은 pause·우측 자원/처치 수치가 모인다 | 큰 AFFIX: ZERO 제목/지역명 카드가 상단 면적을 사용하고 진행 바·시간·레벨 정보는 없다 |
| 생존 정보 | 일반 전투 이미지에서 캐릭터 바로 아래 짧은 초록 HP 바가 따라붙는다 | 좌상단 YOUR HERO 카드와 우상단 MELEE ENEMY 카드가 화면 양끝에 고정돼 있다 |
| 전장 | 플레이어 주변에 여러 방향의 위협·보석·피해 숫자가 있고 전장을 덮는 설명 패널은 없다 | 영웅과 적 1명, 장식물이 상대적으로 크게 보이며 문장형 안내가 하단을 지속 점유한다 |
| 하단 입력 | 일반 전투 이미지 하단 중앙에 투명 원형 이동 패드가 있고 뒤의 전장이 계속 보인다 | 불투명한 전체 폭 카드에 Auto hunt / 보상 설명 / XP·Gold / HUNT AGAIN이 모인다 |
| 성장 선택 | 선택 시 전장 배경을 어둡게 하고 중앙에 Skill Choice, 3개의 세로 카드가 가로로 놓인다 | 현재 스킬 선택 자체가 없으므로 같은 로그라이크 성장 의사결정 화면이 존재하지 않는다 |
| 카드의 내용 | 스킬 아이콘·이름·짧은 효과 설명·별 단계·NEW/EVO 표시가 선택을 비교하게 한다 | 현재 카드의 내용은 제목·HP·상태·고정 보상 설명이며 빌드 비교 정보가 아니다 |
| 장착 정보 | 스킬 선택 위쪽에 작은 아이콘 슬롯 묶음이 있어 이미 갖춘 공격/보조 수단을 보여준다 | 현재 장착/스킬 슬롯 또는 빌드 상태 표현이 없다 |
| 경고 | Boss Assault/Zombies Incoming 같은 상황성 중앙 경고가 보인다 | 현재 지속 상태 문장이 하단 패널에 남는다. 시간성 경고와 상시 정보의 구분이 약하다 |

## 관찰에서 도출한 해석 — 사용자 확정 사항 아님

- 차이는 아이보리/초록 색상만이 아니다. 현재 화면은 큰 제목·설명·상태 카드 중심이고 레퍼런스는 전장·성장 진행·순간 선택 중심이다. 색만 바꿔서는 정보 위계 차이가 해결되지 않는다.
- 일반 전투, 레벨업 선택, 전투 결과를 서로 다른 상태로 읽게 하는 것이 핵심으로 보인다. 현재는 전투 안내와 결과가 동일한 큰 하단 카드에 들어가 전투 중과 결과의 문법이 비슷하다.
- Hero Siege의 장비/스킬 깊이를 결합하려면 무엇을 장착했고 어떤 공격이 가능한지가 보여야 하지만, 아직 없는 장비/스킬을 가짜 버튼으로 채우는 것은 실제 기능과 UI를 다시 어긋나게 한다.
- 세로 레퍼런스는 모바일 조작과 전장 비중을 이해하는 근거다. 이것만으로 AFFIX의 최종 플랫폼·가로/세로 방향을 사용자 대신 확정하지 않는다.
- 레퍼런스의 많은 적은 비교 관찰이며 U1 승인 전에 대량 스폰을 시작하자는 결론이 아니다. 또한 Survivor.io 도시 이미지·아이콘·폰트·UI 조각을 추출해 사용하는 근거가 아니다.

## Hero Siege 공식 PC 화면 직접 관찰

출처는 Panic Art Studios의 [공식 Steam 페이지](https://store.steampowered.com/app/269210/Hero_Siege/)에서 제공하는 실제 화면이다. 스토어 설명만으로 위치를 추정하지 않고 아래 이미지들을 브라우저에 열어 확인했다.

- [전투 화면](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/269210/0d0fa907adf86fb0069961a2cbfc366872d504bb/ss_0d0fa907adf86fb0069961a2cbfc366872d504bb.1920x1080.jpg?t=1787306700): 좌상단 초상·이름·HP/자원, 좌하단 Q/E/R/Y 스킬·얇은 진행 바·재화·번호형 단축 슬롯. 우상단 미니맵·지역·퀘스트. 적 HP는 몬스터 머리 위, 전리품 이름은 해당 필드 위치. 중앙 전장을 덮는 큰 제목이나 상시 설명판이 없다.
- [인벤토리 화면](https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/269210/598f1b591249e531352cf263539c3dcac412efa6/ss_598f1b591249e531352cf263539c3dcac412efa6.1920x1080.jpg?t=1787306700): 우측에 별도로 열린 창. 위쪽 장비 슬롯, 오른쪽 Charms, 아래 가방 격자·탭·정렬. 툴팁은 이름·종류·피해/공격속도·색으로 구분한 옵션·요구 조건 순으로 정보를 묶는다. 게임 월드와 아이템 비교 정보가 서로 다른 영역이다.

이 배치를 참고하는 것과 어두운 배경·붉은 위험 연출·원본 프레임을 가져오는 것은 별개다. AFFIX의 밝은 야외 아트는 유지한다. 작은 대비 높은 HUD와 기능별 정보 분리를 적용한다는 결론은 위 관찰에서 도출한 설계 판단이다.

## 사용자 확정과 적용 범위

조사 중 사용자 답변으로 **PC 가로형: 히어로 시즈 화면 구성 + 탕탕특공대 성장 선택**이 확정됐다. Survivor.io의 세로 화면 비율을 AFFIX에 도입하지 않는다. 전투 HUD 구조는 Hero Siege, 원정 중 3지선다 성장 방식은 Survivor.io를 주된 참고로 삼는다. 화면별 실제 구현/계획 구분은 [UI_DIRECTION](UI_DIRECTION.md)에 둔다.

현재 U1 수정은 실제 전투 데이터에 연결된 가장자리 HUD·캐릭터 열람·일시정지·결과 표시다. 장비 교체·어픽스·레벨업 3지선다 시스템은 이번 수정으로 구현됐다고 표현하지 않는다. 이 두 레퍼런스의 이미지 자체는 저장소/게임 에셋에 반입하지 않았다.

## 3항목 진행 상황 체크포인트

1. 완료: 공식 스토어 전투/스킬 화면과 과거 실제 플레이 이미지 직접 관찰, Game UI Database 분류/검색 확인.
2. 확인: 현재 화면과 레퍼런스의 정보 위계·화면 상태·전장 비중·입력/성장 표현 차이를 기록.
3. 확정: PC 가로형의 Hero Siege 전투 구조 + Survivor.io 성장 선택. 기존 UI는 REJECTED, 새 수정 화면의 사용자 검토 전 U2 보류.
