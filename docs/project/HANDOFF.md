# AFFIX: ZERO 현재 인수인계

갱신: 2026-09-26 KST.

## 현재 구현 상태

- 엔진: Godot 4.7.2-stable 일반판 + typed GDScript + Compatibility.
- 독립 프로젝트: `experiments/e0-godot/project.godot`.
- legacy Godot 4.3 루트 프로젝트와 실제 save v2는 E0에서 읽지 않는다.
- E0-C01: 전사 + 근접 적, 자동 접근, WINDUP/ACTIVE/RECOVERY, 피해 1회, 피격/사망, 실제 포즈 프레임 구현.
- 사용자가 Windows에서 E0-C01을 직접 실행했고 2026-09-26 채팅에서 확인 완료를 보고하며 실행 화면을 제공했다. 이는 로컬 실행/렌더 확인이며 최종 아트 승인으로 확대하지 않는다.
- E0-C02: 원거리 적 + 실제 이동 투사체 + 구간 충돌 판정 + 적별 드랍 + 전사의 물리적 드랍 접근/회수 + idempotent RewardLedger 구현.
- 보상은 적 사망 순간 지급하지 않고 드랍 접촉 시에만 ledger에 반영한다. 같은 drop_id는 두 번 반영되지 않는다.
- 원거리 적은 E0 arena bounds 안에서 이동한다.

## 최신 자동검증

런타임/CI 기준 커밋: `4dc503d58961f668f13eacb5f724c948dad1370f`.
E0 Godot 4.7.2 workflow run: `36162170304`, job `108161582259`, conclusion success.

확인 로그:
- Godot Engine v4.7.2.stable
- import success
- `E0_C01_TEST PASSED`
- `E0_C02_TEST PASSED`
- workflow가 SCRIPT ERROR / Parse Error / missing resource loader를 실패 조건으로 검사한다.

E0-C02 contract가 자동 확인한 것:
- 전사와 근접/원거리 적의 실제 전투 완료
- 원거리 적 projectile request와 실제 projectile node 1:1 생성
- 투사체가 40px보다 많이 이동한 뒤 충돌하므로 발사 순간 즉시 피해가 아님
- 각 적 death signal 1회
- 사망 순간 ledger 미지급
- 적 2마리 -> drop 2개 -> pickup 2회
- GOLD 20 / XP 10
- duplicate drop id 재수령 거부
- 전사가 드랍을 회수하기 위해 실제 좌표를 이동

## 이번에 해결한 C02 검증 문제

첫 C02 run에서는 E0 editor import를 `--quit-after 3`으로 종료해 import scan이 너무 일찍 끝났고, runtime `load(svg)`에서 `No loader found for resource`가 발생했다. 당시 frame contract는 프레임 슬롯 개수만 세어 null texture도 통과시키는 결함이 있었다.

수정:
1. CI import를 `godot --headless --editor --path experiments/e0-godot --import`로 변경.
2. frame contract가 각 frame texture의 non-null까지 검사하도록 강화.
3. 원거리 적 이동을 arena bounds로 제한하고 C02 timeout snapshot을 추가.
4. push/PR E0 workflow concurrency key를 같은 branch 기준으로 통일해 중복 검증을 줄임.

## 로컬 기준

- legacy 작업선: `fix/g6-playtest-recovery / c95b7ba0ab64104470076e4a78cce172b1fd602d`.
- 마지막 점검 당시 legacy working tree 변경 0, stash 15.
- E0 로컬 worktree: 사용자가 `D:\github\affix-e0`에서 C01 실행 확인.
- 자동 stash pop/drop, reset --hard, git clean 금지.
- 실제 save v2는 E0에서 로드하지 않는다.

## 사용자가 다음에 실행할 명령

E0 worktree에서:

```bash
cd /d/github/affix-e0
git fetch origin chore/r0-preservation
git merge --ff-only origin/chore/r0-preservation
```

Godot 4.7.2 일반판에서 기존에 열었던:

```text
D:\github\affix-e0\experiments\e0-godot\project.godot
```

을 다시 실행한다. 화면 제목이 `E0-C02`로 바뀌고 전사/근접 적/보라색 원거리 적, 날아가는 보라색 투사체, 노란 드랍, RewardLedger GOLD/XP 표시가 보여야 한다.

## 다음 개발

사용자 C02 화면 확인 뒤 E0-C03을 진행한다.

E0-C03:
- 40적 x1 부하 장면
- 고정된 스폰/효과 조건
- frame-time p50/p95/p99 및 최대 중단
- 적/투사체/드랍 수
- CI 수치는 사용자 GTX1050 성능으로 쓰지 않음
- 실제 Windows 사용자 PC 측정 절차 제공

C03 전에 C02 시각 확인에서 투사체/카이팅/드랍 회수 표현이 이상하면 그 문제를 먼저 수정한다.

## 새 채팅용

```text
Dev-Gony/affix-zero를 이어서 개발한다. chore/r0-preservation 최신 HEAD, PR #18, docs/project/HANDOFF.md를 먼저 확인한다. 엔진은 Godot4.7.2 Standard + typed GDScript + Compatibility. experiments/e0-godot에 C01과 C02가 구현되어 있다. 사용자는 Windows에서 C01을 실행 확인했다. C02는 원거리 적, 실제 이동 투사체/segment collision, 적별 world drop, 전사 물리 회수, idempotent RewardLedger까지 구현됐다. runtime 기준 commit 4dc503d, E0 run 36162170304/job108161582259에서 import, C01, C02 전부 success이며 로그에 E0_C01_TEST PASSED / E0_C02_TEST PASSED가 있다. 이전 C02 실패는 --quit-after 3 import 조기종료와 null texture contract 문제였고 --import + non-null texture 검사로 해결했다. legacy fix/g6-playtest-recovery c95b7ba, stash15, save v2는 건드리지 않는다. 다음은 사용자 C02 시각 확인 후 E0-C03 40적 x1 성능 하네스다. 승인 없는 병합/reset/clean/stash pop 금지. 모든 답변 마지막에 세 항목 체크포인트를 유지한다.
```
