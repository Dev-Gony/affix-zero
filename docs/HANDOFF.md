# 인수인계 — 사용자 Stitch 시안의 네이티브 Unity 구현

갱신: 2026-09-26. `restart/unity-6`, Draft PR19. 먼저 STATUS.json, PRODUCT_BRIEF.md, STITCH_UNITY_ASSESSMENT.md, UI_DIRECTION.md를 읽는다.

## 유효 방향

사용자 제공 Stitch ZIP4의 영웅 선택/던전 전투/타운/장비·특성이 구체적인 UI 기준이다. 기존 Hero Siege 참고만으로 임의 배치를 반복하지 않는다. 탕탕특공대와 3지선다, 이전 Ninja·밝은 초원·코너 HUD는 폐기 방향이다. 무료 에셋+직접 제작만 사용하며 유료 구매를 진행하지 않는다. 전체 ZIP 원본은 ignored Build/Reference/Stitch/00..03, 참조 SHA와 구성은 docs/assets/stitch-reference-manifest.json에 있다.

Unity6000.3.24f1 / C# / Built-in2D 유지. 실제 에디터 D:/Program Files/Unity 6000.3.24f1/Editor/Unity.exe. 설치/라이선스는 작동하므로 재설치 요구를 반복하지 않는다. 이번 UI 때문에 엔진/패키지/렌더러를 변경하지 않았다.

## 현재 구현

HeroSiegeEncounter에서 새 무료 Soldier/Orc 애니메이션·생성 방·CC0 Lucifer 공격 아이콘·OFL Noto Sans KR로 1대1 전투한다. EncounterHud는 OnGUI를 제거하고 C# UI Toolkit UIDocument/PanelSettings/VisualElement로 만들었다. 상단 메뉴와 실제 적 HP, 우측 실제 위치 미니맵, 하단 HP 구체/공격 경과/획득 XP를 연결했다. MP는 아직 없고, 장비 창은 읽기 전용이다. 생성 HudFrames-v1은 앞선 중간 산출물이며 새 HUD가 로드하지 않는다.

지금은 **전투 화면 첫 단계**다. 영웅 선택·타운·실제 인벤토리/비교/장착·특성 트리·마나·능동 스킬·저장·오프라인 보상은 구현되지 않았다. 캐릭터는 좌우 방향에 한정되고 방은 단일 그림으로 충돌/가림이 없다. UI 위에 원작 스크린샷을 붙여 완성으로 처리하지 않는다.

## 실제 확인

- 새 아트 Editor Play: 04:38:29Z PASS. Camera.Render 월드 전용, 새 Stitch HUD 검사가 아님.
- 새 네이티브 HUD Windows build: 04:55:46Z 성공, 오류0/경고0.
- buildGuid: 54d35624853d494cbee6127fe860214e.
- 720 player run: 15048b15916b455daa9126e3372c7ac1 PASS.
- 1080 player run: c689d0cd4e0041bd8c5217777dae77b3 PASS.
- 두 실행에서 이동/공격/타격프레임/적사망/단발보상/일시정지/정보창/재시작과 네이티브 HP·XP·골드 텍스트·창 표시를 확인. 5상태씩 framebuffer 캡처.
- 에이전트가 720 전투·장비,1080 결과 PNG를 직접 확인. 한글 표시와 화면 가장자리 배치 관찰. 사용자 시각 승인 아님.
- **실제 마우스/키보드 입력 미검사**. probe는 UI와 공유한 API를 직접 호출한다.

보고서와 소스 SHA는 docs/validation/stitch, 무손실 캡처는 docs/media/stitch에 있다. Build/Windows/AffixZero.exe는 이제 위 새 네이티브 HUD 빌드다. 이전 docs/media/first-encounter-* 미추적 캡처를 최신 결과로 추가하지 않는다.

## 재현과 다음 작업

공개 저장소에는 Zerie PNG원본10개가 없으며 Assets/LocalLicensed 전체가 Git 제외다. 공식 무료 ZIP → Tools/Local/Import-FreeCharacters.ps1 → HeroSiegeArtSetup.Build 순서로 반입한다. 누락/해시 불일치 시 새 씬을 만들기 전에 중단한다. 자세한 실행은 LOCAL_WORKFLOW.md.

다음 우선순위는 Stitch 장비 화면에 실제 드랍→비교→장착→타격 변화와 특성1분기를 연결하는 것이다. 고정 수치표를 인벤토리 완성으로 부르지 않는다. 이후 영웅 선택→타운→던전→귀환 전환으로 확장한다. 다수 스폰·시즌·가챠부터 늘리지 않는다. 사용자 시각 피드백이 오면 현재 시안과 대조해 수정한다.

45c6205 및 이전 실행 PASS는 거절된 Ninja 시제품의 기술 기록이다. 이후 시안 검토 문서 1802dae와 네이티브 UI 소스 변경을 구분한다. Git 상태를 먼저 확인하고 미추적 거절 시안/개인 라이선스 원본을 git add .로 반입하지 않는다. 원작 이미지·Godot/E0/V0/PR18 복원, 사용자 자료 삭제, 자동 stash, force push, PR 병합은 금지한다.
