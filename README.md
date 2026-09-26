# AFFIX: ZERO

Unity 6000.3.24f1 / C# / Built-in 2D로 만드는 PC 가로형 Hero Siege 오마주 액션 RPG입니다. 개발선은 `restart/unity-6`, [Draft PR #19](https://github.com/Dev-Gony/affix-zero/pull/19)입니다.

## 현재 작업

사용자가 직접 제공한 Stitch의 영웅 선택·던전 전투·타운·장비/특성 화면을 UI 기준으로 삼습니다. 네 화면 모두 현행 Unity에서 구현 가능합니다. [시안 검토와 구현 판단](docs/STITCH_UNITY_ASSESSMENT.md).

현재 **던전 전투 HUD의 첫 네이티브 구현**을 만들었습니다. UI Toolkit의 상단 적 HP·우측 미니맵·하단 HP 구체/공격 슬롯에 실제 1대1 전투 상태를 연결했습니다. 무료 캐릭터, 직접 생성한 방, CC0 공격 아이콘, OFL 한글 폰트를 사용합니다. HTML이나 웹뷰를 게임에 띄우지 않습니다.

Unity Windows 빌드 오류0/경고0, 실제 1280×720 및 1920×1080 실행 검사 PASS. [실제 실행 캡처](docs/media/stitch/1080-cleared.png) · [검증 기록](docs/validation/stitch/). 실제 마우스/키보드 조작과 사용자 시각 승인은 별도 미완료입니다. 이것은 전체 시안 완성이 아닙니다. 영웅 선택·타운·인벤토리 장착·특성·마나/능동 스킬은 남아 있습니다.

## 시작과 문서

- [인수인계](docs/HANDOFF.md) · [현재 상태](docs/STATUS.json) · [제품 목표](docs/PRODUCT_BRIEF.md)
- [로컬 반입/실행](docs/LOCAL_WORKFLOW.md) · [구조](docs/ARCHITECTURE.md) · [에셋 반입](docs/ASSET_INTAKE.md)
- [검증 범위](docs/VERIFICATION.json) · [트러블슈팅](docs/TROUBLESHOOTING.md)

공개 clone에는 게임 사용만 허가된 Zerie 캐릭터 원본이 없습니다. 공식 무료 팩을 확보한 뒤 검증 반입 스크립트와 새 씬 설정을 실행해야 합니다. 원본 공개 재배포 제한을 구매 여부와 구분합니다. 유료 에셋 구매·엔진/패키지/렌더러 변경·PR 병합은 하지 않았습니다.

이전 Ninja/초원/코너 HUD는 사용자가 거절한 기술 시제품이며 현재 방향이 아닙니다. 옛 Godot 코드·아트는 복원하지 않습니다.
