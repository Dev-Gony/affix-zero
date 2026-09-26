# UI 방향 — 최신 사용자 지시

사용자가 이후 제공한 **Stitch ZIP 4개가 구체적인 화면 구성의 우선 기준**이다. [Unity 구현 판단](STITCH_UNITY_ASSESSMENT.md)에 영웅 선택·던전 전투·타운·장비/특성 화면을 각각 검토했다. Unity 6000.3.24f1 / Built-in 2D에서 구현 가능하며 UI Toolkit 모듈도 현재 프로젝트에 있다. 이 판단을 화면 구현 완료로 해석하지 않는다.

Stitch 기준의 전투 HUD는 **하단 좌우 HP/MP 구체 + 중앙 스킬바/경험치, 상단 지역/보스, 우측 미니맵**이다. 앞선 코너 HUD는 최종안이 아니다. 장비는 왼쪽 장착/가방/유물과 오른쪽 특성/아이템 비교 구조를 따른다. 긴 웹 페이지를 PC 16:9에 통째로 축소하지 않고 영역별 스크롤을 적용한다.

2026-09-26 **Hero Siege 오마주, PC 가로형**으로 변경됐다. 탕탕특공대 3지선다와 혼합 장르 UI 방향은 폐기한다. 이전 좌상단/좌하단 사각 패널 수정안도 사용자에게 재차 거절됐다.

현행 기준은 [HERO_SIEGE_UX_SPEC](HERO_SIEGE_UX_SPEC.md)와 [ART_DIRECTION](ART_DIRECTION.md)이다. [UI_REFERENCE_REVIEW](UI_REFERENCE_REVIEW.md)는 과거 조사/실패 원인 기록이며 Survivor.io 내용을 현행 요구로 사용하지 않는다.

현재 EncounterHud와 FirstEncounter 화면은 거절된 기술 시제품이다. 자동 검사 통과를 UI/UX 방향 승인으로 해석하지 않는다. 새 에셋/상호작용 검수 없이 평면 패널 색상·좌표 수정만 반복하지 않는다.


후속 구현: 현재 관리 화면은 [EQUIPMENT_LOOP](EQUIPMENT_LOOP.md)의 단일무기24칸가방·비교/장착·특성1분기를 실제데이터에연결한 상태다. 작은읽기전용창은교체됐다. 실물시안의전체기능/아트/사용자승인이완료됐다는뜻은아니다.
