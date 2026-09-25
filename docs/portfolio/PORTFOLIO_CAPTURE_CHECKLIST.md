# AFFIX: ZERO Portfolio Capture Checklist

This checklist is for **real Windows Godot 4.3 play captures**, not headless fixtures. Capture only after pulling the exact branch/commit and launching the project with **F5**.

## G3.3 — Epic / Economy / Damage Readability

### Required capture set

- [ ] **Inventory overview — six rarity filter choices**
  - Open the inventory management window.
  - Show the pickup filter with Normal / Magic / Rare / Unique / Legendary / Epic choices.
  - Capture the expanded item-detail area in the same frame if possible.

- [ ] **Sell All — locked item protection**
  - Put at least two unlocked items and one locked item in the bag.
  - Capture before pressing **전체 판매**.
  - Press **전체 판매**.
  - Capture after sale showing the locked item still present and the gold increase/notification.

- [ ] **Expanded item details**
  - Select an item with multiple stats/affixes.
  - Capture the full details, enhancement information, sale value, and equipment comparison without bottom clipping.

- [ ] **Epic rarity evidence**
  - If an Epic item naturally drops during play, capture the field drop and inventory detail.
  - Do **not** grind solely to force this screenshot. Epic is intentionally ultra-rare.
  - CI probability evidence is acceptable until a natural player-facing Epic capture exists.

- [ ] **Normal-enemy damage readability**
  - At a sufficiently high floor, capture/video several normal-enemy contacts.
  - Confirm one normal hit does not erase a full-health character.
  - Record whether repeated hits still feel dangerous rather than harmless.

- [ ] **Boss threat readability**
  - Capture/video a boss encounter at a comparable progression point.
  - Confirm boss hits are visibly more threatening than normal-enemy hits.
  - Note if the boss still feels too weak/too strong despite the automated bounds.

### Optional portfolio evidence

- [ ] Enhancement panel showing +10 / +20 / +30 success-risk information.
- [ ] High-floor normal vs boss HP loss shown side-by-side in a short clip.
- [ ] Inventory Before/After pair emphasizing reduced cleanup friction.
- [ ] Epic / Legendary visual comparison once a natural Epic drop exists.

## G3.3 validation notes

Record after Windows play:

- Build commit: 8bf5186 lineage / G3.3 branch
- Floor / rebirth: existing geared save
- Character / class: existing geared character
- Normal-enemy hit feel: Normal on geared character; fresh-character retest required
- Boss hit feel: Normal on geared character; fresh-character retest required
- Sell All result: PASS
- Inventory detail clipping: PASS
- Enhancement level increment: PASS; live stat application confirmed by code + regression test
- Auto-hunt / movement: PASS
- Skills: PASS
- Save / rebirth: PASS
- Epic seen naturally: Not required
- Bugs found: none in tested G3.3 flows
- Merge approval: gameplay validated; early-progression damage tuning still follow-up

## Portfolio story to preserve

**Problem → Cause → Reference UX → Decision → Implementation → Failure/Revision → Verification → Before/After**

For G3.3, the important story is not merely “added Epic.” The portfolio evidence should show that rarity depth, combat readability, and inventory economy were changed together, then guarded by regression tests after the sixth rarity exposed a stale five-tier smoke assumption.


## G4.0 — Elite Monsters

### Required capture set

- [ ] **Elite first-read**
  - Reach floor 6+ and capture the first elite encounter.
  - The elite should be distinguishable from normal enemies without reading a menu.
  - Capture the larger body and pulsing colored aura.

- [ ] **Three elite archetypes**
  - Capture Brutal / Swift / Bulwark when naturally encountered.
  - Brutal should feel damage-focused, Swift should move/attack faster, Bulwark should take longer to kill.

- [ ] **x5 readability**
  - Run at x5 speed in a crowded wave.
  - Confirm the elite aura remains visible without obscuring attacks or normal enemy silhouettes.

- [ ] **Reward feel**
  - Compare normal and elite XP/gold pickups.
  - Record whether the elite feels worth noticing even when no bonus equipment drops.

- [ ] **Boss separation**
  - Reach a boss floor after seeing elites.
  - Confirm bosses remain visually and mechanically distinct and never show normal elite affixes.

### G4.0 validation notes

- Build commit: 0efce28 lineage / G4.0
- Floor / rebirth: high-floor geared save, around floor 49
- Elite frequency: Good enough for live validation; longer idle-run tuning remains optional
- Brutal feel: PASS; encountered repeatedly
- Swift feel: not captured in this short validation session
- Bulwark feel: PASS; Bulwark Goblin encountered naturally
- x5 readability: aura/silhouette remains readable in crowded combat
- Reward feel: no blocking issue observed
- Boss separation: no elite-affix overlap observed
- Bugs found: none in tested elite flow
- Merge approval: gameplay validated; PR remains unmerged by policy


## G4.1 — Idle Growth Loop UX

### Required capture set

- [ ] **Equipment growth at a glance**
  - Open 장비.
  - Capture at least one equipped item whose next enhancement is affordable.
  - Confirm success rate and cost are visible without opening a tooltip.
  - Confirm the 강화 button shows the next +level.

- [ ] **Low-probability enhancement readability**
  - Inspect a high enhancement milestone if available.
  - Confirm values such as 0.3% / 0.01% are not rounded to 0% or clipped.

- [ ] **Growth summary**
  - Capture the equipment summary tile.
  - Confirm 강화 가능 / 스킬 가능 counts match the current gold state.
  - Spend gold and confirm the counts refresh immediately.

- [ ] **Skill growth header**
  - Open 스킬.
  - Confirm current gold and affordable skill-option count are visible at the top.
  - Buy +1 / +10 / MAX and confirm the header refreshes.

- [ ] **Epic inventory badge**
  - If an Epic item is available in the current save, confirm its bag badge reads 에픽 rather than 일반.
  - Natural Epic acquisition is not required solely for this capture.

- [ ] **Boss wall loop**
  - Lose to a boss naturally.
  - Confirm the message says 파밍 후 자동 재도전.
  - Confirm mechanics remain unchanged: one-floor retreat, automatic farming, eventual automatic boss re-entry.

### G4.1 validation notes

- Build commit: d2664d4 lineage / G4.1
- Equipment card readability: PASS
- Enhancement cost/rate visibility: PASS
- Very-low-rate formatting: implementation/CI PASS; live milestone item not required
- Growth summary refresh: PASS
- Skill header refresh: PASS
- Epic badge: implementation/CI PASS
- Boss defeat loop message: PASS
- Existing retreat/retry behavior unchanged: PASS
- Bugs found: none in tested flow
- Merge approval: gameplay validated; PR remains unmerged by policy


## G4.2 — Rarity-Scaled Loot Feedback

### Required capture set

- [ ] **Normal / Magic restraint**
  - Let the game run at x5 until lower-rarity equipment drops.
  - Confirm the pillar is visible but does not dominate combat.

- [ ] **Rare / Unique escalation**
  - Capture a Rare or Unique drop if one appears naturally.
  - Confirm the beam is clearly taller than low tiers and the ring/particle layer is readable.

- [ ] **Legendary / Epic premium moment**
  - Capture if naturally available from field, elite or boss reward.
  - Confirm Legendary triggers a strong screen cue.
  - Confirm Epic is visibly stronger than Legendary and uses the tallest purple pillar.
  - Do not force or rebalance rarity solely to obtain this capture.

- [ ] **Elite / Boss reward reuse**
  - When an Elite or Boss grants equipment, confirm the same rarity-scaled world effect appears at the death position.

- [ ] **x5 readability**
  - Confirm beams are recognizable at x5 without covering enemies, damage numbers or movement paths.

### G4.2 validation notes

- Build commit:
- Normal clutter: Low / Good / Too much
- Magic readability:
- Rare readability:
- Unique readability:
- Legendary feedback:
- Epic feedback:
- Elite reward effect:
- Boss reward effect:
- x5 combat readability:
- Bugs found:
- Merge approval: Pending / Approved / Rejected


## UIUX V2.0 — Full Management Overhaul

### Required capture set

- [ ] **Management hub overview**
  - Open any management tab.
  - Capture the new almost-full-width hub.
  - Confirm combat is visually dimmed behind it and the bottom combat dock is hidden.
  - Confirm top navigation shows 장비 / 가방 / 스킬 / 환생 / 정보.

- [ ] **Class selection V2**
  - Trigger the class-selection screen from a fresh/rebirth flow.
  - Confirm all six class cards fit in one screen.
  - Confirm each card remains readable at desktop scale and class color is used as identity rather than full UI chrome.

- [ ] **Equipment composition**
  - Open 장비.
  - Confirm the character portrait sits in the center of the 3x3 composition.
  - Confirm equipment cards are materially larger than V1 and rarity colors remain readable.
  - Check long item names, enhancement rate/cost and buttons for clipping.

- [ ] **Inventory desktop layout**
  - Open 가방.
  - Confirm the item grid and detail pane are visible side-by-side.
  - Confirm eight columns fit without horizontal scrolling.
  - Select several items and verify comparison/details update without moving the grid.
  - Confirm clean upgrades show the green ↑ hint.

- [ ] **Skill progression**
  - Open 스킬.
  - Confirm each skill row includes a visible level progress bar.
  - Confirm +1 / +10 / MAX remain readable and clickable.

- [ ] **Rebirth progression**
  - Open 환생.
  - Confirm the level-to-rebirth progress bar is visible.
  - Confirm permanent upgrade buttons remain readable.

- [ ] **Navigation / ESC**
  - Switch tabs using the in-window top navigation.
  - Confirm the management hub stays open.
  - Press ESC once: management should close.
  - Confirm pause/settings does **not** open on that same keypress.
  - Press ESC again: pause/settings should open.

- [ ] **Combat dock restoration**
  - Close management.
  - Confirm the compact bottom dock returns and keyboard shortcuts 1-5 still work.

- [ ] **Combat HUD cleanup**
  - Confirm active combat no longer shows separate 저장 / 종료 buttons.
  - Confirm the single 메뉴 button opens pause/settings.
  - Verify save/load/quit remain available inside the dedicated menu/info surfaces.

### UIUX V2.0 validation notes

- Build commit:
- Management scale: Too small / Good / Too large
- Class selection readability:
- Equipment scan speed:
- Inventory grid readability:
- Detail pane readability:
- Upgrade arrow usefulness:
- Skill progress readability:
- Rebirth progress readability:
- Top navigation:
- ESC behavior:
- Combat dock restore:
- Combat HUD menu/readability:
- Clipping / overlap:
- Overall visual direction: Reject / Iterate / Accept
- Bugs found:
- Merge approval: Pending / Approved / Rejected


## UIUX V2.1 — Playtest Polish

- [ ] **Management live state**
  - Open any management tab during active combat.
  - Confirm the header shows 자동사냥 계속 and combat is still visibly running behind the dim layer.

- [ ] **Equipment hierarchy**
  - Confirm occupied cards have subtle rarity tint + stronger rarity border without making the whole screen look neon.
  - Confirm center portrait shows class, current level and rebirth count.

- [ ] **Empty inventory context**
  - With a strict filter such as 에픽만 and an empty bag, confirm both header and detail pane explain the active acquisition filter.

- [ ] **Rebirth layout**
  - Confirm the top progression card fills the width cleanly.
  - Confirm all four permanent upgrades are presented in one balanced row.
  - Confirm long remaining-level text does not clip.

- [ ] **Info dashboard**
  - Confirm 모험 기록 / 전투 능력 / 현재 층 위협도 / 보조 능력 appear as four balanced cards.
  - Confirm save/load/quit are absent from Info and remain available only in 메뉴.

### UIUX V2.1 validation notes

- Build commit:
- Management-live indicator:
- Equipment rarity treatment:
- Empty inventory explanation:
- Rebirth card balance:
- Info dashboard readability:
- Text clipping:
- Bugs found:
- Merge approval: Pending / Approved / Rejected

## UIUX V2.1 — Visual Hierarchy Polish

- [ ] 관리 화면 프레이밍
  - 관리창을 열면 상단 전투 HUD와 하단 독이 모두 숨는지 확인
  - 관리창이 거의 전체 화면을 사용하고 별도 게임 화면처럼 느껴지는지 확인

- [ ] 장비 화면
  - 고등급 장비가 여러 개 있어도 화면 전체가 빨간 테두리 벽처럼 보이지 않는지 확인
  - 희귀도는 좌측 얇은 스트립 + 아이템명 색으로 충분히 읽히는지 확인
  - 강화/해제 버튼 가독성 확인

- [ ] 스킬 화면
  - 기존 큰 텍스트 네모 블록이 제거되어 임시 UI 느낌이 줄었는지 확인
  - 스킬명 / 레벨 / 진행바 / 설명 / +1/+10/MAX 순서가 자연스러운지 확인
  - 우측 강화 버튼 폭이 충분한지 확인

- [ ] 환생 화면
  - 영구 성장 헤더와 4개 강화 카드가 한 시스템처럼 읽히는지 확인
  - 포인트 보유 시 강화 가능 상태가 명확한지 확인

- [ ] 정보 화면
  - 2열 카드 대시보드 유지 여부 확인
  - 긴 텍스트 벽처럼 보이지 않는지 확인

### V2.1 validation notes

- Build commit:
- Equipment hierarchy:
- Skill placeholder feel: Removed / Still present
- Rebirth growth readability:
- Info dashboard readability:
- Visual clutter: Low / Good / Too much
- Bugs found:
- Merge approval: Pending / Approved / Rejected


## UIUX V2.2 — Final Polish

- [ ] 빈 가방
  - 에픽만 등 높은 획득 필터 상태에서 가방이 비었을 때 중앙 안내가 표시되는지 확인
  - 빈 슬롯만 덩그러니 보여서 오류처럼 느껴지지 않는지 확인

- [ ] 장비 강화
  - 현재 골드로 강화 가능한 장비 버튼이 금색 계열로 자연스럽게 강조되는지 확인
  - 강화 불가/해제 버튼과 시각적으로 구분되는지 확인

- [ ] 스킬 강화
  - +1 강화 가능 버튼은 금색, MAX 가능 버튼은 녹색 계열로 읽히는지 확인
  - 강조가 과해서 모든 버튼이 싸우는 느낌은 없는지 확인

- [ ] 환생
  - 아직 조건 미달이면 진행바가 금색 계열인지 확인
  - 환생 가능 시 진행바/주요 버튼이 녹색으로 바뀌는지 확인

### V2.2 validation notes

- Build commit:
- Empty bag state:
- Equipment affordance:
- Skill affordance:
- Rebirth readiness:
- Visual noise: Low / Good / Too much
- Bugs found:
- Merge approval: Pending / Approved / Rejected


## G5.0 — Pet System + Item Art Pipeline

- [ ] **전투 펫**
  - 청월호가 플레이어 옆을 자연스럽게 따라다니는지 확인
  - 플레이어가 이동/방 이동할 때 순간이동하거나 벽에 박혀 보이지 않는지 확인
  - x1 / x5에서 펫 자동 공격이 보이는지 확인

- [ ] **펫 지원 효과**
  - HP가 감소했을 때 청월호의 자동 회복 텍스트가 가끔 표시되는지 확인
  - 회복이 전투를 방해할 정도로 과도하지 않은지 확인

- [ ] **펫 성장**
  - 골드가 충분할 때 `훈련 +1Lv`가 작동하고 즉시 레벨이 오르는지 확인
  - 요구 레벨을 만족한 펫은 진화 버튼이 활성화되고 ★ 등급이 상승하는지 확인
  - 훈련/진화 후 저장했다가 불러와도 유지되는지 확인

- [ ] **펫 탭**
  - 상단 관리 메뉴에 펫이 장비/가방/스킬/환생/정보와 함께 표시되는지 확인
  - 단축키 4가 펫, 5가 환생, 6이 정보 순서로 맞는지 확인
  - 현재 출전 펫/레벨/역할/설명이 읽히는지 확인
  - 해금되지 않은 펫은 요구 층수가 보이고 선택 불가인지 확인

- [ ] **펫 해금**
  - 현재 진행 층 기준으로 조건을 만족한 펫이 자동 해금되는지 확인
  - 새 펫 해금 알림이 표시되는지 확인

- [ ] **저장/불러오기**
  - 활성 펫을 바꾼 뒤 저장
  - 재실행/불러오기 후 같은 펫과 레벨이 유지되는지 확인

- [ ] **아이템 외형 교체**
  - 장비/가방에서 29개 베이스 전용 item atlas 아트가 표시되는지 확인
  - 무기/투구/갑옷/장갑/신발/반지/목걸이가 즉시 구분되는지 확인
  - 신검/마검/용비늘/다이아몬드 등 같은 슬롯 내부에서도 베이스 아트가 다르게 보이는지 확인
  - 필드 드랍 아이콘도 가방/장비와 같은 실제 아이템 아트를 사용하는지 확인
  - 희귀도 프레임과 아이템 자체 아트가 서로 싸우지 않는지 확인

- [ ] **아이템 아트 파이프라인**
  - dedicated icon 경로가 없는 기존 저장 아이템도 깨지지 않아야 함
  - 향후 전용 PNG를 연결할 수 있는 icon_path fallback 계약 유지

### G5.0 validation notes

- Build commit:
- Follower movement:
- x1 pet attack:
- x5 pet attack:
- Healing support:
- Pet tab readability:
- Unlock behavior:
- Save/load:
- Existing item icons intact:
- Bugs found:
- Merge approval: Pending / Approved / Rejected


## G5.1 — Combat Identity + Pet Loot Loop

- [ ] **프로젝트 전용 전투 그래픽**
  - 전투 캐릭터가 이전 CC0 단일 스프라이트가 아니라 새 6직업 atlas로 보이는지 확인
  - 몬스터 8종이 새 enemy atlas로 보이는지 확인
  - 장착 무기가 캐릭터 손에 실제 표시되고 공격 시 함께 움직이는지 확인

- [ ] **보스 / 엘리트 가독성**
  - 보스 등장 시 중앙 상단 보스 HP바가 표시되는지 확인
  - 보스 피격 시 HP바가 즉시 줄어드는지 확인
  - 보스 사망/층 이동 시 HP바가 사라지는지 확인
  - 폭군/질풍/철벽 엘리트 이름이 몬스터 위에 표시되는지 확인
  - x5에서도 보스/엘리트 등장 연출이 과도하게 화면을 가리지 않는지 확인

- [ ] **던전 / HUD**
  - 좌측 목표 패널, 우측 3x3 미니맵이 전투를 가리지 않는지 확인
  - 15층 단위로 붉은 성채 / 잿빛 납골당 / 푸른 금고 느낌이 순환하는지 확인
  - 보스 층 방 중앙이 일반층보다 위협적으로 보이는지 확인

- [ ] **펫 정수 루프**
  - 엘리트 처치 시 보라색 펫 정수가 드랍/흡수되는지 확인
  - 보스 처치 시 엘리트보다 많은 정수를 주는지 확인
  - 펫 탭 상단 정수 수량이 즉시 증가하는지 확인
  - 진화 버튼에 골드 + 정수 요구량이 함께 표시되는지 확인
  - 진화 시 정수가 실제로 차감되는지 확인

- [ ] **펫 패시브**
  - 청월호 / 밤그림자: 치명 보너스 표기 확인
  - 화염룡: 주인 피해 증가 표기 확인
  - 유령 슬라임: 골드 보너스 표기 확인
  - 수호 골렘: 받는 피해 감소 표기 확인
  - 초원의 요정: 경험치 보너스 표기 확인
  - ★ 진화 후 패시브 수치가 증가하는지 확인

- [ ] **스킬 이펙트**
  - x1에서 직업 스킬이 기존보다 명확히 구분되는지 확인
  - x5에서 이펙트가 너무 오래 남거나 시야를 완전히 덮지 않는지 확인
  - 마법사 화염구가 이동 중 꼬리/코어가 보이는지 확인

### G5.1 validation notes

- Build commit:
- Hero atlas:
- Enemy atlas:
- Equipped weapon visual:
- Item artwork:
- Boss HUD:
- Elite labels:
- Dungeon themes:
- Minimap/objective placement:
- Pet essence loop:
- Pet passive readability:
- x1 skill VFX:
- x5 skill VFX:
- Bugs found:
- Merge approval: Pending / Approved / Rejected


## G6.0 — Dark ARPG Dungeon / Combat Rebuild

- [ ] **던전 동선**
  - 이전 3x3 보드가 보이지 않고 방/통로가 이어진 굴곡 던전처럼 느껴지는지 확인
  - 층 클리어 후 캐릭터가 실제 통로를 걸어서 다음 방으로 이동하는지 확인
  - 카메라가 긴 이동 중 월드 끝/검은 영역을 이상하게 노출하지 않는지 확인
  - 방 크기/형태가 지나치게 동일하게 반복되지 않는지 확인

- [ ] **맵 아트**
  - 캐릭터/몬스터 대비 바닥과 벽이 지나치게 싸 보이지 않는지 확인
  - 횃불/기둥/잔해/룬이 전투 가독성을 해치지 않으면서 공간 밀도를 올리는지 확인
  - 전체 톤이 밝은 모바일풍이 아니라 dark ARPG로 일관되는지 확인

- [ ] **몬스터 animation**
  - 이동 시 정적 JPG가 미끄러지는 느낌이 줄었는지 확인
  - 슬라임 squash, 박쥐 flap, 대형 몬스터 hover가 보이는지 확인
  - 공격 전 pullback/windup → 공격 lunge가 이어지는지 확인
  - 피격 시 hit reaction, 사망 시 fall/flatten이 보이는지 확인

- [ ] **적 공격 VFX**
  - 근접 slash / slime slam / bat dive가 구분되는지 확인
  - 리치 shadow bolt / dragon flame / demon hellfire가 서로 다른 공격으로 보이는지 확인
  - x5에서도 선딜 텔레그래프가 너무 늦거나 너무 오래 남지 않는지 확인

- [ ] **직업별 기본 공격**
  - 전사 / 기사 / 마법사 / 현자 / 암살자 / 성자의 기본 공격 색/형태가 서로 구분되는지 확인
  - 기사 shield charge 사용 시 캐릭터가 실제로 앞으로 이동하는지 확인

- [ ] **필드 드랍**
  - XP가 녹색 점 대신 결정 조각으로 보이는지 확인
  - 골드가 노란 점 대신 동전 묶음으로 보이는지 확인
  - 펫 정수가 보라색 별 결정로 구분되는지 확인
  - 장비가 실제 아이템 그림 + 희귀도 링/빛기둥으로 바닥에서 식별되는지 확인
  - 고등급 장비가 너무 빨리 흡수되어 그림을 못 보는 문제는 없는지 확인

- [ ] **펫 가챠**
  - 펫 탭에 1회/10회 소환 버튼과 정수 비용이 표시되는지 확인
  - 정수가 부족하면 소환 버튼이 비활성화되는지 확인
  - 10회 소환 후 영웅 이상 결과가 최소 1개 포함되는지 확인
  - 신규 펫은 보유 목록에 추가되고, 중복은 조각이 증가하는지 확인
  - 진화가 정수가 아닌 골드 + 해당 펫 조각을 요구하는지 확인

- [ ] **펫 실제 외형**
  - 청월호 꼬리, 화염룡 날개, 밤그림자 flap 등 각 펫 실루엣이 전투 중 구별되는지 확인
  - 펫 자동 공격 시 짧은 lunge가 보이는지 확인
  - 회복 지원 시 pulse 연출이 보이는지 확인
  - 캐릭터 옆에서 크기/채도/이펙트가 과하게 튀거나 지나치게 초라하지 않은지 확인

### G6.0 validation notes

- Build commit:
- Dungeon route:
- Corridor travel:
- Map art quality:
- Enemy movement animation:
- Enemy windup/attack:
- Enemy hit/death:
- Class basic attack VFX:
- Knight real charge:
- XP/gold/essence readability:
- Gear drop readability:
- 1x summon:
- 10x guarantee:
- Duplicate fragments:
- Live pet appearance:
- x1 readability:
- x5 readability:
- Bugs found:
- Merge approval: Pending / Approved / Rejected
