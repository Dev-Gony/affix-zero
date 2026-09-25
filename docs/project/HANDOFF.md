# AFFIX: ZERO 현재 인수인계

기록일: 2026-09-26 KST. 다음 세션은 이 파일과 BASELINE.json, DECISIONS.md, R0_LOCAL_GUIDE.md를 읽고 실제 원격 HEAD/PR/CI를 재조회한다.

## 정확한 작업 기준

- 작업 브랜치: `chore/r0-preservation`
- 작업 PR: [#18](https://github.com/Dev-Gony/affix-zero/pull/18), Draft/open, 병합하지 않음
- 이 기록 직전 구현/문서 스냅샷: `6b71fde43de76bd2a2d1fb269789df649744a93b`
- 도구/테스트/워크플로 변경의 마지막 SHA: `288ae4f0df8894e7fe3a12a6d672740f332d9d47`
- 기준 복구 후보: `fix/g6-playtest-recovery` / `c95b7ba0ab64104470076e4a78cce172b1fd602d`, PR #17, 미승인
- 기존 개발선: `dev/gameplay-v5-pets-items` / `433948879fdfd23cf1cccaa0ae0c8ce3a0784eae`
- main: `a1418ad6a52f1a9e41607e29c7ecb56edbb96ab0`

이 파일의 마지막 갱신은 인수인계 문서 전용이다. 현재 HEAD는 GitHub에서 다시 조회한다. 복구/기존 개발/main 브랜치를 이동시키거나 #17을 승인한 것이 아니다.

## 이번에 한 일

R0-01 실행 도구와 R0-04 문서 충돌 정리다. Windows 로컬 점검/명시적 세이브 백업 도구, 합성 테스트, Windows Actions, 현재 기준·엔진 게이트·로컬 가이드·트러블슈팅을 추가했다. 이전 README/AGENTS는 archive에 동일 blob으로 보존했다.

게임 코드·장면·리소스·아트·저장 형식은 변경하지 않았다. 45페이지 개발 마스터 v0.2 전체는 이전 채팅 첨부다. 이 폴더는 원문의 대체본이 아닌 최신 실행 상태다. 엔진은 고정하지 않으며 E0에서 작은 실제 전투/아트 제작 결과로 결정한다.

## 실제 검증

1. `288ae4f` 직접 push 실행 [36155308288](https://github.com/Dev-Gony/affix-zero/actions/runs/36155308288), job `108138272736`: Windows PowerShell 5.1 / PowerShell 7 각각 동일한 34 checks, 0 failures. 상태와 원본 로그 확인.
2. PR #18의 `6b71fde` 실행 [36155807362](https://github.com/Dev-Gony/affix-zero/actions/runs/36155807362), job `108139930991`: 두 PowerShell에서 각각 34 checks, 0 failures. 원본 로그의 실제 checkout은 PR 임시 merge `d997e2e3c6d7c704bb271cbe6f620c0028202128`이다. 이것은 실제 PR 병합이 아니다.
3. 두 실행의 도구 SHA-256은 `A74FE34F585968DE3ECBC9DEBD7CF0EDDB8CCA09A0941CCC1DEBD51A0010F637`로 같다. 검증 대상은 합성 데이터와 점검 CLI다. 사용자 실제 저장은 사용하지 않았다.
4. `6b71fde`의 기존 Godot smoke [36155807813](https://github.com/Dev-Gony/affix-zero/actions/runs/36155807813)는 이 기록 시 실행 중이었다. 완료 여부를 다시 조회한다. PASS를 가정하지 않는다.

세부 테스트 범위와 초기 workflow 실패/해결 경과는 VALIDATION.md, TROUBLESHOOTING.md를 읽는다. 다음 코드 변경은 새 SHA로 재검증한다.

## 사용자 확인과 엔진 비교는 아직 대기

실제 사용자 PC의 HEAD/미커밋/스태시, Godot 버전, CPU/GPU/RAM, user:// 경로와 백업은 아직 미확인이다. Windows 게임 플레이, 아트 승인, E0 엔진 비교/성능 측정은 NOT_RUN이다. 폴더 선택 GUI 수동 조작과 사용자 Backup CLI도 NOT_RUN이며 백업 핵심 함수만 합성 파일로 실행했다.

VERIFIED_BYTES는 파일 바이트 일치이지 게임 저장 복구 성공이 아니다. 도구는 미커밋 작업을 백업하거나 stash를 이동시키지 않는다. 상태만 알려주고 원본 그대로 남긴다.

## 다음 행동

사용자에게 R0_LOCAL_GUIDE 2절의 Git Bash 명령을 제공한다. fetch와 임시 스크립트 실행만 하며 로컬 switch/pull/stash/commit은 아직 하지 않는다. `share-summary.json`과 현재 Godot 버전을 받는다. 보고서는 PC 사양을 포함하므로 공유 전 내용을 확인하도록 한다. 실제 세이브/백업은 업로드하지 않는다.

이후 실제 user://를 확인하고 게임/에디터를 종료한 상태에서 Backup 모드로 보존한다. 상태/primary_save_present를 확인한다. 로컬 변경이 있으면 사용자 작업 내용에 맞춰 보존 방법을 정하며 자동 stash/pop하지 않는다.

R0-02는 기존 복구 후보의 소환 결과, 전투/펫 가림, 목표 HUD/소유권 보존 3개 검사다. 기존 버그 전체를 고친 뒤에야 엔진을 결정하지 않는다. R0-01 후 E0-01/02/03으로 최소 전투·에셋 제작과 엔진을 검증한다. 저장 중단 복구/환생 오류 안내/미지 필드는 독립 미해결 검증 대상이다.

## 새 채팅용 복사문

```text
Dev-Gony/affix-zero 개발을 이어간다. ChatGPT 채팅+GitHub로 작업하며 Codex를 요구하지 마라. chore/r0-preservation의 docs/project/HANDOFF.md, BASELINE.json, DECISIONS.md, R0_LOCAL_GUIDE.md와 PR #18/최신 Actions를 읽어라. 이 브랜치는 미승인 PR #17 c95b7ba 기준 R0 도구/문서 작업선이며 main 병합 승인이 아니다. 도구/테스트 기준 288ae4f, 문서 스냅샷 6b71fde의 Windows CI에서 PS5.1/7 각각 34개 검사를 통과했지만 사용자 실제 상태/백업은 아직 보고서가 필요하다. 최신 SHA와 CI는 재조회하라. share-summary.json을 받은 적이 있는지 확인하고 중복 요청하지 마라. Godot 고정 아님. 작은 전투 코어/아트 검증 E0 후 엔진을 결정하며 기존 코드 때문에 전환을 피하지 않는다. 실제 Windows/렌더/성능을 실행하지 않았다면 NOT_RUN이다. 저장 초기화, reset --hard, git clean, force push, 자동 stash pop, 승인 없는 병합을 하지 마라. 필수 로컬 명령을 제공하고 트러블슈팅 네 열 표와 마지막 세 항목 체크포인트를 유지하라.
```
