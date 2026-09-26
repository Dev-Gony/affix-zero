# 문서 목차 — Hero Siege 오마주

갱신: 2026-09-26. 최신 정책은 **탕탕특공대 요소 폐기, 유료 에셋 구매 제외, 무료 원본 + 직접 제작**이다. Ninja·밝은 초원·기존 두 HUD 시안은 deprecated다. 과거 기술 검사와 현행 생성 아트 씬의 결과를 구분한다.

**사용자 제공 Stitch 4개 시안이 UI 배치의 최우선 기준**이다. 현재는 전투 화면을 네이티브 UI Toolkit으로 구현한 첫 단계이며 영웅 선택·타운·완전한 장비/특성 화면은 후속 범위다. 현재 연결은 무료 Zerie Soldier/Orc10 PNG(로컬 제한 원본), 생성 TempleRoom, Lucifer CC0 공격 아이콘, Noto Sans KR OFL 폰트다. HudFrames-v1은 원본 이력으로 보존하고 현행 UI에서는 사용하지 않는다.

Windows build는 `Build/Reports/windows-build.json`의 2026-09-26T04:55:46Z Succeeded, 오류0/경고0이다. 실제 UI Toolkit HUD를 포함한 Windows 실행은 buildGuid=`54d35624853d494cbee6127fe860214e`, 1280×720 run=`15048b15916b455daa9126e3372c7ac1`과 1920×1080 run=`c689d0cd4e0041bd8c5217777dae77b3` 모두 PASS다. 각 보고는 `Build/Reports/stitch-720/player-smoke.json`, `stitch-1080/player-smoke.json`에 있다. 전투·양측 피해·적 사망·단발 보상·pause/열람 상태·재시작과 native HUD 상태/캡처5개를 확인했다. UI와 공유하는 API를 호출한 검사이며 **실제 클릭 NOT_RUN, 사용자 시각 승인 NOT_APPROVED**다. 04:38:29Z의 생성 아트 씬 Play PASS는 이전 월드 검증이며 Stitch UI 증거와 분리한다.

| 문서 | 역할 |
|---|---|
| [HANDOFF.md](HANDOFF.md) / [STATUS.json](STATUS.json) | 세션 재개, 최신 구현·검증·다음 행동 |
| [PRODUCT_BRIEF.md](PRODUCT_BRIEF.md) | Hero Siege 오마주 제품 목표와 범위 |
| [HERO_SIEGE_UX_SPEC.md](HERO_SIEGE_UX_SPEC.md) | 전장·HUD·전리품·장비 흐름의 목표 |
| [ART_DIRECTION.md](ART_DIRECTION.md) | 최신 사용자 아트 방향과 폐기 기준 |
| [STITCH_UNITY_ASSESSMENT.md](STITCH_UNITY_ASSESSMENT.md) / [UI_DIRECTION.md](UI_DIRECTION.md) | 사용자 Stitch 4개 시안과 Unity 구현 범위 |
| [ARCHITECTURE.md](ARCHITECTURE.md) | 코어·현재 표시 계층·새 씬 생성 구조와 한계 |
| [ASSET_INTAKE.md](ASSET_INTAKE.md) | 현행 무료·생성 아트의 반입·라이선스·검증 범위 |
| [assets/HERO_SIEGE_CHARACTER_CANDIDATES.md](assets/HERO_SIEGE_CHARACTER_CANDIDATES.md) | Zerie/Pixel Crawler 조사, 무료 Zerie 원본 프레임 검사 |
| [assets/zerie-local-manifest.json](assets/zerie-local-manifest.json) | 공개 재배포하지 않는 로컬 캐릭터10 PNG 해시 |
| [assets/generated-art-manifest.json](assets/generated-art-manifest.json) | 생성 배경·HUD의 프롬프트·출처·실제 크기·한계 |
| [ROADMAP.md](ROADMAP.md) | 단계별 진행. 과거 확장 아이디어는 현재 완료 기능이 아님 |
| [LOCAL_WORKFLOW.md](LOCAL_WORKFLOW.md) / [VERIFICATION.json](VERIFICATION.json) | 정확한 에디터 경로·명령·보고·소스와 검사 결과 |
| [TROUBLESHOOTING.md](TROUBLESHOOTING.md) | 발생 지점·원인·해결·배운 점 |
| [CLEANUP_PLAN.md](CLEANUP_PLAN.md) / [RESTART_DECISION.md](RESTART_DECISION.md) | 보존/삭제 경계와 재시작 결정 |
| [history/LEGACY_RETROSPECTIVE.md](history/LEGACY_RETROSPECTIVE.md) | 폐기 코드·이미지를 복원하지 않는 텍스트 회고 |
| `assets/ninja-*`, 과거 `validation/`·`media/` | deprecated Ninja 기술 시제품의 이력. 현재 채택/승인 근거 아님 |

새 채팅은 HANDOFF·STATUS·PRODUCT_BRIEF와 해당 작업 문서를 함께 읽는다. 생성 사원은 단일 그림이며 타일셋/실시간 조명이 아니다. 생성 HudFrames atlas는 현재 사용하지 않는다. 한글 폰트는 OFL 원문·저작권을 함께 보존한다. 제한된 Zerie 원본은 공개 Git에 넣지 않으며 검증한 `Tools/Local/Import-FreeCharacters.ps1`로 공식 무료 ZIP을 로컬 반입한다. Godot 코드·이미지·ZIP을 문서에 복원하거나 유료 구매를 우회하지 않는다.
