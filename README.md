# AFFIX: ZERO

Unity 6000.3.24f1 / C# / Built-in 2D로 만드는 PC 가로형 Hero Siege 오마주 **자동사냥 방치형 RPG**입니다. 개발선은 `restart/unity-6`, [Draft PR #19](https://github.com/Dev-Gony/affix-zero/pull/19)입니다.

## 현재 작업

**현재는 자동 던전 순환을 연결한 MVP 이전 빌드입니다.** 현재는 단일 사원 마당 안의 세 구간을 자동으로 순회한다. 적 두 개체를 구간마다 재사용하고, BFS 경로 탐색으로 통행 불가 모서리와 석재 장애물을 피한다. 탐색 → 대상 선택/접근 → 자동 공격 → 자동 수거 → 다음 구간 → 입구 귀환/반복을 연결했다. 세 개의 별도 방이나 절차 생성 던전은 아니다.

일반 피격은 체력과 짧은 flash만 반영하며 공격·이동을 취소하지 않는다. 사망할 때 공격을 취소한다. 연속 두 번 사망하면 자동 중단하고, 가방 포화 시 보류 전리품을 보존한 채 멈춘다. 장비·특성·대장간을 열어도 사냥하며 수동 정지만 전투를 멈춘다. 연속 사망·포화 안전 검사는 PASS다. 별도 경로 막힘과 중단 후 재개 검증은 남아 있다. [방치형 전투 계약](docs/IDLE_COMBAT_CONTRACT.md) · [MVP 기준](docs/AUTO_HUNT_MVP.md).

사용자 Stitch 시안 7개를 UI 기준으로 사용합니다. 24칸 가방·단일 무기 비교/장착, 3노드 특성, 장착 무기 확정 강화가 연결돼 있습니다. 강화는 최대 +3, 단계당 피해 +2, 비용 8/16/24골드입니다. 자동사냥이 전리품을 회수하지만 장비·특성·강화 결정은 사용자가 합니다. 첫 확정 드랍 외에 매 순환 완료 시 사원의 강철검을 지급합니다. 장비·성장은 실행 세션에 유지되며 게임 종료 후 저장은 없습니다.

최종 Windows 자동사냥 실행은 [720p 보고](docs/validation/autohunt/player-720.json)와 [1080p 보고](docs/validation/autohunt/player-1080.json) 모두 PASS다. 실행 ID는 각각 `26b0296dea004e06a40c4a3cfab8caa8`(70.9288초), `23dbebd8a24b4397bca82dce83b771bd`(70.6474초), Build GUID는 `6c03e8d2bc6e457abc31582911701d3d`다. 두 실행 모두 실제 1배속으로 3구간·3순환·18처치·4개 수거, 사망 0회·재시도 0회, XP 450·골드 144를 확인했다. 영웅 HP 120/공격력 30·적 HP 54/공격력 6을 바꾸지 않았다. 메뉴 중 사냥과 수동 정지/재개, UI 콜백 7회·실제 framebuffer 6개도 검사했다. [720p 전투](docs/media/autohunt/720-combat.png) · [1080p 관리 중 사냥](docs/media/autohunt/1080-management.png). 로컬 원본은 `Build/Reports/autohunt-diagnostic-{720,1080}/auto-hunt-smoke.json`이다.

최종 빌드는 `Build/Reports/windows-build.json`의 2026-09-26T06:36:34.0607289Z Succeeded, 오류 0/경고 0이다. 정상 두 해상도는 위 최종 빌드의 근거다. 안전 중단은 아래 명시한 직전 빌드에서 검사했으며 safety-build/source-fingerprint 보고서로 구분한다. 앞선 `autohunt-fixed-720` 후보 실행은 이력으로 구분한다. 이전 `docs/validation/management/`는 관리 기능 시제품의 역사적 근거다. 디스크 저장, 20분 연속 실행, OS 물리 입력 검사, 사용자 시각 승인은 완료되지 않았다.

생성 방은 배경 그림 한 장이며 이동·시야 판정은 별도 그리드로 구현했습니다. 새 석재 장애물 sprite를 이 그리드에 배치했습니다. 손의 무기는 캐릭터 프레임에 포함되어 장착·강화 외형이 바뀌지 않습니다. 영웅 선택·타운·펫·마나/능동 스킬·소켓/재련·디스크 저장은 남아 있습니다.

## 시작과 문서

- [인수인계](docs/HANDOFF.md) · [현재 상태](docs/STATUS.json) · [제품 목표](docs/PRODUCT_BRIEF.md)
- [로컬 반입/실행](docs/LOCAL_WORKFLOW.md) · [구조](docs/ARCHITECTURE.md) · [에셋 반입](docs/ASSET_INTAKE.md)
- [검증 범위](docs/VERIFICATION.json) · [트러블슈팅](docs/TROUBLESHOOTING.md)

공개 clone에는 게임 사용만 허가된 Zerie 캐릭터 원본이 없습니다. 공식 무료 팩을 확보한 뒤 검증 반입 스크립트와 새 씬 설정을 실행해야 합니다. 원본 공개 재배포 제한을 구매 여부와 구분합니다. 유료 에셋 구매·엔진/패키지/렌더러 변경·PR 병합은 하지 않았습니다.

이전 Ninja/초원/코너 HUD는 사용자가 거절한 기술 시제품이며 현재 방향이 아닙니다. 옛 Godot 코드·아트는 복원하지 않습니다.


안전 검사의 빌드 GUID는 `0a94c21a725b4d3a9a01e269067fa423`다. 이후 변경은 관리 창 위 피해 숫자 숨김과 검사 보고의 최종 성장 수치 추가이며 전투·안전 중단 코드는 동일하다. [연속 사망 안전 검사](docs/validation/autohunt/safety-deaths.json)는 run `f508cbfae00844128c310426a8abbbc3`, 약 29.4초에 실제 적 공격으로 두 번 사망·한 번 재시도 후 자동 중단, 4.01초 중단 유지 PASS다. 이 격리 검사만 영웅 공격력을 1로 주입했으며 정상 밸런스 증거가 아니다. [가방 포화 안전 검사](docs/validation/autohunt/safety-bag-full.json)는 run `a8fd4677ac5741bb90de2caeccef30fa`, 시험용 무기 24개로 가방을 채운 뒤 실제 첫 처치의 전리품을 보류 상태로 보존하고 중단했다. 약 7.65초, 골드 8·XP 25·기존 아이템 24개 유지, 중단 4.01초 PASS다. 두 검사는 중단 후 재개나 20분 안정성을 증명하지 않는다.
