# AFFIX: ZERO 현재 인수인계

갱신: 2026-09-26 KST.

## 가장 중요한 현재 방향

E0는 엔진/판정/성능 계약을 확인하기 위한 실험 장치일 뿐이며 **게임의 기본 실행 화면으로 사용하지 않는다**.

사용자가 Windows에서 E0-C03 40적 화면을 확인한 결과, 원형 군집/도형형 임시 캐릭터/진단 HUD가 실제 AFFIX 게임 방향과 현저히 다르다고 판단했다. 이 피드백은 정당하며 E0를 더 확장하는 작업을 중단했다.

현재부터 개발 기준은 `V0 Vertical Slice`다.

## 엔진

- Godot 4.7.2 Standard
- typed GDScript
- Compatibility
- 엔진 전환은 현재 보류. 이유: E0 40적 x1에서 성능 여유가 충분히 관찰됐고, 현재 문제는 엔진이 아니라 임시 아트/배치/연출/게임 화면 구성에 있음.
- 이 판단은 Godot이 최종적으로 무조건 고정이라는 뜻이 아니다.

## V0 Vertical Slice

기본 실행 씬은 이제:
`res://vslice/v0_main.tscn`

E0-C03가 아니다.

구현:
- 저장소의 본게임 제작 자산 `assets/sprites/dungeon_courtyard.png` 사용
- `class_atlas_alpha.png` warrior 사용
- `enemy_atlas_alpha.png` slime/bat/skeleton/goblin/dark_knight 사용
- 화면 가장자리 스폰
- 적 간 separation 적용
- 최대 일반 적 18개
- 자동 타깃/접근/근접 공격
- 피격/넉백/데미지 숫자/타격 파편
- 적 사망
- XP/Gold world pickup + 자석 회수
- 레벨업 시 ATK 증가/회복
- 30킬 뒤 Dark Knight elite
- stage clear / retry
- compact HUD
- arena clamp + y-sort
- legacy save/manager는 계속 격리

중요: 현재 class/enemy atlas는 본게임용 원본 아트이지만 직업/몬스터별 단일 포즈 atlas다. V0의 공격/이동은 아직 production frame animation 완성이 아니다. 이 사실을 숨기지 않는다. 다음 아트 단계에서 walk/attack/hit/death 실제 프레임 세트를 새로 제작/연결해야 한다.

## 자동검증

V0 smoke가 Godot 4.7.2에서 다음을 검증:
- repository production texture 세 개를 실제 파일에서 로딩
- player 생성
- enemy spawn

커밋 `a450668ac1723bf8df639bc72d4c14e910bbe352` 계열 workflow의 V0 smoke step에서 `V0_SMOKE_TEST PASSED` 확인. 이후 arena clamp/y-sort 수정은 최신 HEAD의 CI를 계속 확인한다.

## 로컬 실행

사용자의 E0 worktree는 그대로 사용한다.

```bash
cd /d/github/affix-e0
git fetch origin chore/r0-preservation
git merge --ff-only origin/chore/r0-preservation
```

Godot 4.7.2 Standard에서:
`D:\github\affix-e0\experiments\e0-godot\project.godot`

F5를 누르면 V0 vertical slice가 기본 실행되어야 한다.

화면에 E0-C03/40 ENEMIES 진단화면이 다시 기본으로 뜨면 실패다.

## 다음 개발

1. 사용자 V0 실제 화면 확인
2. gameplay/visual 오류 즉시 수정
3. V0-02 production animation asset pipeline
   - warrior walk/attack/hit/death
   - 첫 melee monster walk/attack/hit/death
   - 공격 프레임과 damage active frame 동기화
4. V0-03 survivor-style spawn pacing + elite/boss telegraph
5. 그 다음에만 40/150/300 성능 재측정

## 새 채팅용

```text
Dev-Gony/affix-zero 개발을 이어간다. chore/r0-preservation 최신 HEAD와 PR #18, docs/project/HANDOFF.md를 확인한다. E0는 테스트 전용이며 기본 실행 화면으로 쓰지 않는다. 사용자는 E0-C03 40적 원형 군집/임시 도형 화면을 명확히 거절했고 성능 실험 확장을 중단했다. 기본 실행은 experiments/e0-godot/vslice/v0_main.tscn의 V0 Vertical Slice다. dungeon_courtyard + class_atlas_alpha + enemy_atlas_alpha를 실제 repo asset에서 로딩하고 edge spawn, separation, auto melee combat, hit FX, death, XP/gold pickups, level-up, 30kill dark knight elite, compact HUD를 구현했다. V0_SMOKE_TEST PASSED 증거가 있다. 단 class/enemy atlas는 아직 실제 walk/attack/hit/death frame animation asset이 아니므로 최종 애니메이션 완성이라고 주장하면 안 된다. 다음은 사용자 V0 화면 확인 후 production animation pipeline이다. legacy save/stash는 건드리지 않는다. 승인 없는 merge/reset/clean/stash pop 금지. 모든 답변 마지막에 3항목 진행 상황 체크포인트를 유지한다.
```
