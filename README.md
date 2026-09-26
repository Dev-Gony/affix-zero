# AFFIX: ZERO

Unity 6000.3.24f1 / C# / Built-in 2D로 만드는 PC 가로형 Hero Siege 오마주 액션 RPG입니다. 개발선은 `restart/unity-6`, [Draft PR #19](https://github.com/Dev-Gony/affix-zero/pull/19)입니다.

## 현재 작업

사용자가 제공한 Stitch 시안 7개를 UI 기준으로 삼습니다. 새 특성·펫·대장간 레퍼런스를 검토하고, 전용 특성 화면과 무기 강화를 현재 Unity 전투에 연결했습니다. [추가 시안 검토](docs/STITCH_REFERENCE_EXPANSION.md).

**전리품 회수 → 비교·장착 → 특성 투자 → 대장간 강화 → 다음 전투**가 작동합니다. 특성 노드 선택은 상세만 표시하고 투자 버튼으로 포인트를 사용합니다. 대장간은 장착 무기를 최대 +3까지 확정 강화하며 단계마다 피해 +2, 비용은 8/16/24골드입니다. [실제 특성 화면](docs/media/management/1080-talent.png) · [실제 대장간 화면](docs/media/management/1080-forge-enhanced.png).

시작 공격력 30 → 장착 40 → 특성 43 → 강화 45를 연결했고, 다음 전투에서 방어력 2를 반영한 실제 피해 43을 확인했습니다. 강화 후 잔여 골드와 장비·특성은 장면 전환에 유지됩니다. 게임 종료 후 저장은 아직 없습니다.

코어 **105 checks**, Unity Windows 빌드 **오류 0 / 경고 0**, 실제 **720p·1080p PASS**. 각 UI Toolkit 콜백 17개와 화면 상태 12개를 검사했습니다. OS 마우스·키보드 검사 및 사용자 시각 승인은 미완료입니다. [최신 검증 기록](docs/validation/management/).

영웅 선택·타운 연결, 펫, 마나/능동 스킬, 소켓·재련, 디스크 저장은 남아 있습니다. 방은 단일 그림이며 캐릭터 손의 무기는 프레임에 포함되어 교체·강화에 따라 외형이 바뀌지 않습니다.

## 시작과 문서

- [인수인계](docs/HANDOFF.md) · [현재 상태](docs/STATUS.json) · [제품 목표](docs/PRODUCT_BRIEF.md)
- [로컬 반입/실행](docs/LOCAL_WORKFLOW.md) · [구조](docs/ARCHITECTURE.md) · [에셋 반입](docs/ASSET_INTAKE.md)
- [검증 범위](docs/VERIFICATION.json) · [트러블슈팅](docs/TROUBLESHOOTING.md)

공개 clone에는 게임 사용만 허가된 Zerie 캐릭터 원본이 없습니다. 공식 무료 팩을 확보한 뒤 검증 반입 스크립트와 새 씬 설정을 실행해야 합니다. 원본 공개 재배포 제한을 구매 여부와 구분합니다. 유료 에셋 구매·엔진/패키지/렌더러 변경·PR 병합은 하지 않았습니다.

이전 Ninja/초원/코너 HUD는 사용자가 거절한 기술 시제품이며 현재 방향이 아닙니다. 옛 Godot 코드·아트는 복원하지 않습니다.
