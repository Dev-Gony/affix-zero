# AFFIX: ZERO

Unity 6000.3.24f1 / C# / Built-in 2D로 만드는 PC 가로형 Hero Siege 오마주 액션 RPG입니다. 개발선은 `restart/unity-6`, [Draft PR #19](https://github.com/Dev-Gony/affix-zero/pull/19)입니다.

## 현재 작업

사용자가 직접 제공한 Stitch의 영웅 선택·던전 전투·타운·장비/특성 화면을 UI 기준으로 삼습니다. 네 화면 모두 현행 Unity에서 구현 가능합니다. [시안 검토와 구현 판단](docs/STITCH_UNITY_ASSESSMENT.md).

현재 전투에 **전리품 회수 → 비교 → 장착 → 특성 투자 → 다음 전투**를 연결했습니다. Stitch 관리 화면을 따라 왼쪽에 장착 무기/능력치/24칸 가방, 오른쪽에 특성 분기와 아이템 비교를 배치했습니다. [장비 비교 실행 화면](docs/media/progression/1080-comparison.png).

첫 전리품 장착으로 공격력이30→40, 특성 투자로43이 됩니다. 실제 다음 전투에서 방어력2를 적용한41피해를 검증했습니다. 장비·특성·누적 경험치/골드는 장면 전환에 유지됩니다. 단일 무기 슬롯과 한 분기부터 구현했으며 게임 종료 후 저장은 아직 없습니다.

코어90검사, Unity Windows 빌드 오류0/경고0, 실제720p/1080p 실행 PASS. 회수/선택/장착/투자/초기화/닫기/다음 전투 UI Toolkit 콜백9개를 검사했습니다. OS 마우스/키보드 입력과 사용자 시각 승인은 미완료입니다. [검증 기록](docs/validation/progression/) · [기능과 제한](docs/EQUIPMENT_LOOP.md).

영웅 선택·타운·마나/능동 스킬·디스크 저장은 남아 있습니다. 현재 맵은 단일 그림이고 캐릭터 손의 무기는 프레임에 포함되어 있어 장비 교체에 따라 외형이 바뀌지는 않습니다.

## 시작과 문서

- [인수인계](docs/HANDOFF.md) · [현재 상태](docs/STATUS.json) · [제품 목표](docs/PRODUCT_BRIEF.md)
- [로컬 반입/실행](docs/LOCAL_WORKFLOW.md) · [구조](docs/ARCHITECTURE.md) · [에셋 반입](docs/ASSET_INTAKE.md)
- [검증 범위](docs/VERIFICATION.json) · [트러블슈팅](docs/TROUBLESHOOTING.md)

공개 clone에는 게임 사용만 허가된 Zerie 캐릭터 원본이 없습니다. 공식 무료 팩을 확보한 뒤 검증 반입 스크립트와 새 씬 설정을 실행해야 합니다. 원본 공개 재배포 제한을 구매 여부와 구분합니다. 유료 에셋 구매·엔진/패키지/렌더러 변경·PR 병합은 하지 않았습니다.

이전 Ninja/초원/코너 HUD는 사용자가 거절한 기술 시제품이며 현재 방향이 아닙니다. 옛 Godot 코드·아트는 복원하지 않습니다.
