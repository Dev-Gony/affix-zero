# AFFIX: ZERO 현재 인수인계

갱신: 2026-09-26 KST.

## 현재 결정과 구현

- 엔진 기준: Godot 4.7.2-stable 일반판 + typed GDScript + Compatibility.
- 기존 Godot 4.3 루트 프로젝트는 보존하며 새 엔진으로 열지 않는다.
- 독립 E0 프로젝트가 실제 생성됨: `experiments/e0-godot/project.godot`.
- E0-C01 구현 완료: 전사 1종 + 근접 적 1종, 자동 접근, WINDUP -> ACTIVE -> RECOVERY, 피격, 사망, 중복 공격 인스턴스 차단.
- 전사/적 모두 idle 2, walk 2, attack 3, hit 1, death 2개의 별도 SVG 포즈 프레임을 사용한다. 최종 아트 승인이 아니라 구조 검증용 임시 자산이다.
- 기존 save.json, SaveManager, autoload, 펫, 가챠, 환생, 인벤토리는 E0에서 읽거나 로드하지 않는다. custom user dir는 `AFFIX_ZERO_E0`다.

## 검증

- 최초 E0 CI에서 Godot 4.7.2의 native `CanvasItem.draw_ellipse()`와 로컬 헬퍼 이름이 충돌해 parse error 발생.
- 헬퍼를 `_draw_shadow_ellipse()`로 변경해 해결.
- 커밋 `4db9af2e99050198f28b6739e975230b7e0eb884`의 E0 workflow run `36160446393`에서 import 성공, contract 성공, 로그에 `E0_C01_TEST PASSED` 확인.
- 이 자동검사는 접근/실제 피해/사망/멀티프레임 계약을 확인한다. Windows 사용자의 시각 승인과 타격감 승인은 아직 NOT_RUN이다.
- 최신 브랜치에는 동일 런타임 코드에 CI fontconfig/concurrency 정리만 추가되었다. 최신 HEAD는 매 세션 재조회한다.

## 로컬 기준

- 사용자 원본 작업선: `fix/g6-playtest-recovery` / `c95b7ba0ab64104470076e4a78cce172b1fd602d`.
- 마지막 점검 당시 미커밋 변경 0, stash 15. 자동 stash pop/drop 금지.
- 실제 save.json은 v2이며 원본 4.3 데이터로 보존한다. 새 E0는 읽지 않는다.

## 사용자가 지금 확인할 것

원본 폴더를 건드리지 않기 위해 별도 worktree를 사용한다.

```bash
cd /d/github/affix
git fetch origin chore/r0-preservation
git worktree add -b e0-c01-local /d/github/affix-e0 origin/chore/r0-preservation
```

그 뒤 Godot 4.7.2 일반판에서 아래 파일만 연다.

```text
D:\github\affix-e0\experiments\e0-godot\project.godot
```

F6/F5로 실행한다. 전사와 근접 적이 서로 접근하고, 실제 공격 포즈 3프레임을 거쳐 타격하며, 피격/사망까지 진행되어야 한다. 기존 `D:\github\affix\project.godot`은 4.7.2로 열지 않는다.

다음 pull부터는:

```bash
cd /d/github/affix-e0
git pull --ff-only
```

## 다음 작업

E0-C02: 원거리 적 + 실제 투사체 이동/충돌 + 드랍/회수 원장. 이후 E0-C03: 40적 x1 부하 측정. 사용자 E0-C01 시각 확인에서 애니메이션/타격감 문제를 발견하면 C02보다 먼저 수정한다.

## 새 채팅용

```text
Dev-Gony/affix-zero 개발을 이어간다. chore/r0-preservation 최신 HEAD와 PR #18을 조회하고 docs/project/HANDOFF.md를 읽어라. 엔진은 Godot4.7.2 일반판 + typed GDScript + Compatibility로 결정됐다. experiments/e0-godot에 E0-C01이 실제 구현되어 있다: 전사/근접 적 자동 접근, WINDUP/ACTIVE/RECOVERY, 피해 1회, 피격/사망, 실제 포즈별 다중 프레임. 4db9af2의 E0 run 36160446393에서 E0_C01_TEST PASSED 확인. 최초 실패는 Godot4.7의 CanvasItem.draw_ellipse 이름 충돌이었고 _draw_shadow_ellipse로 수정했다. 최종 아트/Windows 사용자 승인은 아직 아니다. 원본 fix/g6-playtest-recovery c95b7ba와 stash15, save v2는 건드리지 않는다. 다음은 사용자 E0-C01 시각 확인 후 E0-C02 원거리/투사체/드랍이다. 승인 없는 병합/reset/clean/stash pop 금지. 마지막은 3항목 체크포인트로 끝내라.
```
