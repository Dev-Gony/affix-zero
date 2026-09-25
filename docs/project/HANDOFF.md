# 현재 인수인계: ART-RESET-01

2026-09-26 사용자 명시 요청으로 기존 게임 이미지 전부와 다크 판타지 시각 방향을 폐기했다. 다음 채팅도 이 결정부터 적용한다.

## 현재 상태

- 작업 브랜치 chore/r0-preservation, PR #18. 정확한 최신 HEAD/CI는 매 세션 조회한다.
- 삭제 전 기준 46ad2db3538734d3aed48a119856f32862f7c164.
- assets/ 및 experiments/e0-godot/assets/를 삭제한다. design-system/도 삭제한다. 옛 이미지 사본을 현재 트리의 다른 폴더로 옮기지 않는다.
- 게임 규칙/데이터 기존 파일은 보존하고 옛 렌더 코드는 비활성으로 둔다. 사용자 저장과 stash를 건드리지 않는다.
- root와 experiments/e0-godot 기본 실행은 이미지 없는 아트 교체 상태 안내다. 신규 게임/MVP 완성 화면이 아니다. 예전 화면을 다시 보여주지 않는다.
- 이 작업의 audit PASS는 기존 미디어 제거/게임 소스 바이트 보존만 뜻한다. E0/V0 옛 렌더 테스트는 docs/history/ci로 이동해 중단 사실을 남겼다.
- 기존 C03 수치로 최종 GPU 성능/엔진 적합성을 확정하지 않는다.

## 새 디자인 결정

ART_DIRECTION.md: 밝고 선명한 야외 캐주얼 2D RPG. 기존 다크 배경/아틀라스/CC0 타일/E0 SVG의 복원·색변경 재활용 금지. Hero Siege/Survivor.io는 플레이/장비/성장 구조 참고이며 기존 분위기를 가져오지 않는다.

1차 시각 레퍼런스 Tiny Swords. 현행 팩은 원본 재배포 제한, 제작자 제공 TS_old version_CC0 Licensed는 별도로 실물/라이선스 검사해야 한다. CC0 대안은 Ninja Adventure, UI 보조 후보는 Kenney UI Pack - Adventure. 새 원본 ZIP은 아직 확보/반입하지 않았다. Game UI Database는 접근 제한으로 실제 화면 미검수, Pinterest/Dribbble도 채택 작품 미확정.

## 다음 작업 ART-02

Tiny Swords의 정확한 CC0 구버전 ZIP 확보 → 원본 라이선스/해시/프레임/방향/pivot/impact 검수 → 밝은 작은 필드와 영웅1/적1 → 보존 규칙 연결. 실제 에셋 확보 전에 임의 SVG나 옛 아틀라스를 가져와 진행한 척하지 않는다.

## 로컬

사용자는 D:\github\affix-e0 / e0-c01-local을 사용한다. 현재 Godot 실행파일의 폴더/파일 구조는 이미 확인됐으므로 다시 묻지 않는다. 에디터를 닫고 clean 상태에서 fetch + ff-only 업데이트한다. 로컬 수정이 있으면 실패 메시지를 확인하고 자동 stash/pop/reset/clean은 하지 않는다. Git history/다른 worktree까지 삭제하지 않았다.

## 새 채팅 재개

```text
Dev-Gony/affix-zero의 chore/r0-preservation 최신 HEAD/PR #18과 docs/project/HANDOFF.md, ART_RESET.md, ART_DIRECTION.md, ASSET_CANDIDATES.md, ASSET_INTAKE.md를 읽고 이어가라. 사용자 요청은 기존 이미지 전부 삭제와 다크 아트 완전 폐기다. 기존 assets 및 E0 SVG는 재사용/복원/참고하지 마라. 게임 규칙과 아이템·스킬·성장/저장 데이터는 보존되어 있다. 현재 기본 씬은 이미지 없는 교체 안내이지 새 게임이 아니다. 새 시각은 밝은 Tiny Swords 계열 캐주얼 2D RPG. 현행 Tiny Swords는 재배포 제한이므로 제작자가 제공하는 TS_old version_CC0 Licensed ZIP을 실물 검수한 뒤 반입한다. 아직 신규 ZIP/프레임 검수 완료라고 쓰지 마라. 다음 ART-02는 원본 라이선스/해시/idle-walk-attack-hit-death/발 pivot/impact frame 검사 후 작은 실제 필드를 만드는 작업이다. 검수 전 block/SVG 대체 제작이나 옛 다크 아틀라스 복원 금지. 사용자 승인 없는 병합/force/reset/clean/stash pop/save migration 금지. 마지막은 기존 세 항목 진행 상황 체크포인트다.
```
