# 문서 목차 — Hero Siege 오마주

갱신: 2026-09-26. 최신 정책은 **탕탕특공대 요소 폐기, 유료 에셋 구매 제외, 무료 원본 + 직접 제작**이다. Ninja·밝은 초원·기존 두 HUD 시안은 deprecated다. 과거 기술 검사와 현행 생성 아트 씬의 결과를 구분한다.

**사용자 제공 Stitch 7개 시안이 UI 배치의 최우선 기준**이다. 현재는 전투와 단일 무기·24칸 가방, 전용 3노드 특성 화면과 대장간을 네이티브 UI Toolkit으로 구현했다. 전리품 회수 → 비교/장착 → 특성 선택/배분/초기화 → 골드 확정 강화 → 다음 전투의 피해 증가가 연결된다. 버리기는 같은 아이템에 두 번 눌러 확인한다. 관리 화면은 한 번에 하나만 열리고 전투를 일시정지한다. 특성 선택 자체는 포인트를 쓰지 않는다. 대장간은 장착 무기를 단계당 피해+2, 최대+3으로 강화하며 비용은8/16/24 Gold다. 펫은 참조만 보관한다. 영웅 선택·타운·다중 장비 부위·마나·능동 스킬·영구 저장은 후속 범위다. 현재 연결은 무료 Zerie Soldier/Orc10 PNG(로컬 제한 원본), 생성 TempleRoom, Lucifer CC0 아이콘5개, Noto Sans KR OFL 폰트다. HudFrames-v1은 원본 이력으로 보존하고 현행 UI에서는 사용하지 않는다.

성장은 `FirstEncounter.Progression`의 static 상태로 같은 실행 세션의 다음 전투에서 유지된다. XP는 누적되며 `TotalGold`는 강화 비용을 뺀 가용 잔액이다. 무기별 강화 단계는 장착 교체·다음 전투에 유지되고 전투별 보상은 초기화된다. 첫 처치의 확정 무기1개 이후에는 추가 무기 없이 처치 보상과 특성 포인트만 늘어난다. 프로세스 종료 후 저장은 없고, 장착 변경은 피해·UI에 적용되지만 원본에 구워진 월드 무기 외형은 바뀌지 않는다.

Windows build는 `Build/Reports/windows-build.json`의 2026-09-26T05:42:27.8768512Z Succeeded, 오류0/경고0이다. buildGuid=`863e316feb2d4b01b0667c7d636282c0`, 1280×720 run=`47da26d84f4745a69b72b26b27dc6503`과 1920×1080 run=`95ac18aaf2834c01a8bfd9bc2221326a` 모두 PASS다. 보존 보고는 [player-720.json](validation/management/player-720.json), [player-1080.json](validation/management/player-1080.json), [windows-build.json](validation/management/windows-build.json), 소스 식별은 [SHA manifest](validation/management/source-fingerprint.json)를 참조한다. 로컬 원본은 `Build/Reports/management-{720,1080}/player-smoke.json`, 화면 기록은 `docs/media/management/`다.

각 실행에서 UI Toolkit `ClickEvent` 콜백17회와 HUD framebuffer12개를 검사했다. 전투·전리품·장착 외에 관리 화면의 상호 배타 표시와 pause, 잠긴 정밀 노드 선택 시 포인트 미소모, 특성 투자/환불, 강화 비용 차감·피해 증가·다음 전투 유지와 실타격43(공격력45−적 방어2)을 확인했다. 코어 검사는105개 통과했다. 전투 pause는 공유 API, 관리 탭·전리품·장비·특성·대장간·다음 전투는 UI 이벤트 전달로 검사했다. **OS 물리 마우스/키보드 입력 NOT_RUN, 사용자 시각 승인 NOT_APPROVED**다. 기존 `validation/progression`의05:22:48Z 실행(콜백9/캡처9/실타격41)과 그 이전 Stitch·생성 아트 Play 보고는 당시 범위의 이력이다.

| 문서 | 역할 |
|---|---|
| [HANDOFF.md](HANDOFF.md) / [STATUS.json](STATUS.json) | 세션 재개, 최신 구현·검증·다음 행동 |
| [PRODUCT_BRIEF.md](PRODUCT_BRIEF.md) | Hero Siege 오마주 제품 목표와 범위 |
| [HERO_SIEGE_UX_SPEC.md](HERO_SIEGE_UX_SPEC.md) | 전장·HUD·전리품·장비 흐름의 목표 |
| [ART_DIRECTION.md](ART_DIRECTION.md) | 최신 사용자 아트 방향과 폐기 기준 |
| [STITCH_UNITY_ASSESSMENT.md](STITCH_UNITY_ASSESSMENT.md) / [UI_DIRECTION.md](UI_DIRECTION.md) | 사용자 Stitch 7개 시안과 Unity 구현 범위 |
| [STITCH_REFERENCE_EXPANSION.md](STITCH_REFERENCE_EXPANSION.md) | 추가 04 특성·05 펫·06 대장간 참조 검토와 실제 구현/검증 범위 |
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
