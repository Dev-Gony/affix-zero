# 인수인계 — 사용자 Stitch 시안의 네이티브 Unity 구현

갱신: 2026-09-26. `restart/unity-6`, Draft PR19. 먼저 STATUS.json, PRODUCT_BRIEF.md, STITCH_UNITY_ASSESSMENT.md, UI_DIRECTION.md를 읽는다.

## 유효 방향

사용자 제공 Stitch ZIP4의 영웅 선택/던전 전투/타운/장비·특성이 구체적인 UI 기준이다. 기존 Hero Siege 참고만으로 임의 배치를 반복하지 않는다. 탕탕특공대와 3지선다, 이전 Ninja·밝은 초원·코너 HUD는 폐기 방향이다. 무료 에셋+직접 제작만 사용하며 유료 구매를 진행하지 않는다. 전체 ZIP 원본은 ignored Build/Reference/Stitch/00..03, 참조 SHA와 구성은 docs/assets/stitch-reference-manifest.json에 있다.

Unity6000.3.24f1 / C# / Built-in2D 유지. 실제 에디터 D:/Program Files/Unity 6000.3.24f1/Editor/Unity.exe. 설치/라이선스는 작동하므로 재설치 요구를 반복하지 않는다. 이번 UI 때문에 엔진/패키지/렌더러를 변경하지 않았다.

## 현재 구현 — 장비·특성 순환 완료

HeroSiegeEncounter에서 무료 Soldier/Orc·생성 방·CC0 아이콘·OFL 한글 폰트로 전투한다. 네이티브 UI Toolkit 관리 화면 EquipmentPanel은 왼쪽 장착 무기/능력치/24칸 가방, 오른쪽3노드 특성과 실제 아이템 비교다. 화면참고는 Stitch03이다.

HeroProgression(순수C#)은 기본24+시작검6=30을 제공한다. 첫 적 처치로만 잿불 강철검(12+잿불4) 하나가 월드에 나타나며 회수 버튼으로 가방에 들어온다. 장착하면 이전 무기를 같은 가방 칸에 돌려주고 공격력40이 된다. 각 고유 처치마다특성1point/25XP/8Gold. 분노2(+3/rank)→정밀1(+4)→숙련1(+6)이며 초기화는 사용분만 환불한다. 버리기는 두 클릭 확인이며 재화 보상은 없다.

FirstEncounter는 세션 모델을 장면재시작에도 보존한다. 다음 전투는 종료 후 대기 전리품을 회수해야 가능하다. 누적XP/골드와 해당전투의보상은 분리한다. MeleeActor는 공격 시작시에피해를잠가 장착/특성이 이미진행중인타격을 바꾸지 않는다. 과정/계약은 EQUIPMENT_LOOP.md와 ARCHITECTURE.md.

## 실제 확인

- 순수 코어90검사 PASS.
- 최신 Windows build 2026-09-26T05:22:48Z, 오류0/경고0, GUID e86a4c30328a47ec9bf8cf3ba918d0f8.
- 720 run e3f3eec47a4b4751b59f5dc715697a93,1080 run4b47acfcaa8948a8a003eca4e936c4db 모두PASS.
- 실제 전투/타격/단발보상/pause를 유지하면서 UI ClickEvent9개로 회수/선택/장착/특성투자/초기화/재투자/닫기/다음전투검증. 강화후 첫 타격41(43-방어2), 다음장면빌드와누적보상유지.
- 각9상태framebuffer캡처; 에이전트가720비교/특성,1080비교화면을관찰했다. 실제OS입력/사용자시각승인은NOT_RUN.
- 최초player에서는SpriteImage에sourceRect를설정해실패. TextureImage에top-left crop으로수정해위최종실행통과. 기본RuntimeTheme.tss도명시반입했다.

보고서/소스SHA는 docs/validation/progression, 캡처는docs/media/progression. 앞선docs/validation/stitch는이전HUD단계이며현재검증으로혼동하지않는다. Build/Windows/AffixZero.exe는위최신빌드다.

## 제한과 다음 작업

게임 종료 후 저장/불러오기, 영웅선택·타운, 마나/능동스킬, 여러장비슬롯은미구현. 첫보장검이후추가무기드랍은없고처치포인트는계속얻는다. 캐릭터손무기는이미지에포함되어장착외형은그대로다. 방은단일그림이고지형충돌/가림이없다. 다음우선순위는사용자Stitch의영웅선택→타운→던전진입과귀환연결이다. 여러적/가챠/시즌부터늘리지않는다.

## 반입과 작업 주의

공개 저장소에는 Zerie PNG원본10개가 없으며 Assets/LocalLicensed 전체가 Git 제외다. 공식 무료 ZIP → Tools/Local/Import-FreeCharacters.ps1 → HeroSiegeArtSetup.Build 순서로 반입한다. 누락/해시 불일치 시 새 씬을 만들기 전에 중단한다. 자세한 실행은 LOCAL_WORKFLOW.md.

사용자 시각 피드백이 오면 현재 시안과 대조해 수정한다. 원본 시안 전체와의 시각 일치를 검사 PASS만으로 주장하지 않는다.

45c6205 및 이전 실행 PASS는 거절된 Ninja 시제품의 기술 기록이다. 이후 시안 검토 문서 1802dae와 네이티브 UI 소스 변경을 구분한다. Git 상태를 먼저 확인하고 미추적 거절 시안/개인 라이선스 원본을 git add .로 반입하지 않는다. 원작 이미지·Godot/E0/V0/PR18 복원, 사용자 자료 삭제, 자동 stash, force push, PR 병합은 금지한다.
