# AFFIX: ZERO 현재 인수인계

기록일: 2026-09-26 KST. 첫 진입은 이 파일, BASELINE.json, ENGINE_GATE.md, E0_ACCEPTANCE.md다. 실제 원격 HEAD/PR/CI는 매 세션 재조회한다.

## 정확한 작업 기준

- 작업 브랜치: `chore/r0-preservation`, PR [#18](https://github.com/Dev-Gony/affix-zero/pull/18), Draft/open, 미병합.
- 이번 문서 갱신의 부모 스냅샷: `5409a1f41d50cf4df2fc4d2a95c9856bfbabf7a8`. 이 문서 자체의 커밋은 GitHub의 최신 HEAD로 조회한다.
- 백업 도구/테스트/workflow의 마지막 변경: `288ae4f0df8894e7fe3a12a6d672740f332d9d47`. 도구 blob은 `029d565172abe9545e788447f3596218090d8a7f`다.
- 기준 복구 후보와 사용자 로컬: `fix/g6-playtest-recovery` / `c95b7ba0ab64104470076e4a78cce172b1fd602d`, PR #17 미승인.
- 기존 개발선: `dev/gameplay-v5-pets-items` / `433948879fdfd23cf1cccaa0ae0c8ce3a0784eae`.
- main 기준: `a1418ad6a52f1a9e41607e29c7ecb56edbb96ab0`. 이 작업은 main/개발선/복구선을 이동시키지 않는다.

## 사용자 자료 수신 완료: 다시 처음부터 요구하지 말 것

사용자가 share-summary.json을 첨부했다. 생성 시각은 2026-09-25 15:47:42 UTC, 2026-09-26 00:47:42 KST다. 원격 대상 일치, 로컬 HEAD c95b7ba, 변경 항목 0개, stash 15개, 감지된 게임/엔진 프로세스 0개다. 이는 수집 시점의 상태이며 이후 변경을 가정하지 않는다. stash는 그대로 유지하고 자동 pop/apply/drop하지 않는다.

테스트 PC: Ryzen 7 5700X, GeForce GTX 1050, RAM 31.9 GiB, Windows 10 Pro build 19045, Windows PowerShell 5.1.19041.6456. PC 사양은 성능 통과 증거가 아니다.

사용자는 `Godot_v4.3-stable_win64.exe` 사용을 직접 확인했다. 보고서의 NOT_DETECTED는 exe 경로를 지정하지 않아 생긴 값이다. 버전을 다시 묻거나 미설치라고 판단하지 않는다. 실제 exe 경로나 --version 출력까지 확인한 것은 아니다.

반면 실제 user:// 경로와 백업은 아직 NOT_CONFIRMED / NOT_RUN이다. 이 채팅 환경은 사용자 Windows 파일시스템을 직접 조작할 수 없다. 도구가 준비됐다고 백업 완료로 기록하지 않는다.

## 이번 문서/검증 변경

1. 사용자 보고서를 기준 기록에 반영하고 첫 Inspect 반복 요청을 제거했다. 원본 보고서/세이브/개인 경로는 저장소에 올리지 않았다.
2. 도구 해시 차이를 실제 재계산으로 설명했다. Git blob 029d565의 LF 원문은 12801바이트, SHA-256 3A36CA2F...FAD16로 사용자 보고서와 일치한다. LF를 CRLF로 바꾼 해시는 A74FE34F...0F637로 이전 Windows CI 로그와 일치한다. 저장 파일에는 이런 텍스트 정규화를 적용하지 않는다.
3. E0 최소 전투/에셋/측정 승인 기준을 E0_ACCEPTANCE.md에 구체화했다. 실제 시험 장면/새 에셋/엔진 비교를 구현·실행했다고 주장하지 않는다.

게임 코드, 장면, 리소스, 아트, 저장 형식, 기존 백업 도구는 변경하지 않았다. 45페이지 개발 마스터 v0.2 전체는 이전 채팅 첨부이며 이 실행 기록이 원문을 대체하지 않는다.

## 검증 증거

- 288ae4f 직접 push 36155308288, PR 6b71fde 실행 36155807362: PS5.1과 PS7 각각 34 checks / 0 failures는 이전 세션에서 원본 로그까지 확인했다.
- 부모 HEAD 5409a1f의 R0 36155964584와 Godot smoke 36155964770은 이번 세션에서 모두 completed/success로 재조회했다. 새 문서 HEAD의 결과로 옮겨 적지 않는다.
- 이번에는 Git blob 재구성 일치, LF SHA-256의 사용자 보고서 일치, CRLF SHA-256의 CI 기록 일치만 별도 계산했다. Godot 실행이나 사용자 백업 실행이 아니다.
- 사용자 Windows 게임 플레이, 새로운 아트 승인, 엔진 비교/성능 측정은 NOT_RUN이다.

## 사용자가 지금 해야 할 로컬 작업

기존 Godot 4.3에서 기존 프로젝트를 열되 F5/F6를 누르지 않는다. 상단 Project > Open User Data Folder로 실제 데이터 폴더를 열고 주소를 확인한다. 새 엔진이나 새 프로젝트로 경로를 추정하지 않는다. 폴더를 확인한 뒤 게임과 에디터/프로젝트 매니저를 종료한다. 탐색기는 열어 둬도 된다.

기존 작업 폴더가 D:\github\affix일 때 아래 Git Bash 블록을 실행한다. 다른 위치라면 첫 cd만 실제 위치로 바꾼다. 전 단계에서 썼던 R0_SCRIPT 변수가 남아 있다고 가정하지 않는다.

```bash
cd /d/github/affix &&
git fetch origin chore/r0-preservation &&
R0_SCRIPT="$(mktemp --suffix=.ps1)" &&
git show 5409a1f41d50cf4df2fc4d2a95c9856bfbabf7a8:tools/local/Invoke-AffixR0.ps1 > "$R0_SCRIPT" &&
test "$(git hash-object "$R0_SCRIPT")" = "029d565172abe9545e788447f3596218090d8a7f" &&
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File "$(cygpath -w "$R0_SCRIPT")" -RepositoryPath "$(pwd -W)" -Mode Backup -GameClosed -ChooseSaveDirectory
```

선택 창에서는 방금 Godot이 열어 준 데이터 폴더 자체를 선택한다. save.json 파일이나 D:\github\affix 프로젝트 폴더를 선택하는 것이 아니다. 파일이 없거나 복사 검증에 실패하면 중단하며 저장을 새로 만들거나 초기화하지 않는다. 조직 정책으로 실행이 막히면 정책을 바꾸지 말고 오류를 확인한다.

정상 백업은 backup.status=VERIFIED_BYTES, file_count>0, primary_save_present=true다. false이면 .bak/.tmp만 보존된 상태이므로 원본 저장 복구 완료로 인정하지 않는다. 입력/복사본 내용 의미 검증은 여전히 NOT_RUN이다. 백업 출력은 저장소 밖 %LOCALAPPDATA%\AFFIX_ZERO_R0\날짜-고유ID다.

사용자가 공유할 것은 이번 Backup으로 새로 생긴 share-summary.json이다. 원본 save.json, save-snapshot 폴더, VERIFIED.json은 공개 GitHub에 올리지 않는다. Inspect 보고서 재제출을 요구하지 않는다. 백업 후에도 Godot NOT_DETECTED가 남을 수 있으며 사용자 직접 확인한 4.3 정보는 유효하다.

## 백업 다음 작업

새 Backup 보고서를 확인한 뒤 E0-02를 시작한다. 기존 user://와 분리된 프로젝트에서 캐릭터 1종/근접 적 1종/원거리 적 1종의 실제 애니메이션, 타격, 투사체, 사망, 드랍을 만든다. 해당 프로젝트는 아직 없으므로 존재하는 것처럼 실행 명령을 주지 않는다. 에셋 라이선스와 동작 세트가 충족되지 않으면 BLOCKED_ASSET를 분리하고 필요한 자산을 실제로 확보/승인한다.

40적 x1부터 측정하며 150/300적과 x2/x5는 확장 부하다. 첫 측정 플랫폼은 사용자 Windows PC이고 최종 출시 플랫폼 확정과 다르다. Godot 4.3은 legacy 재현용이다. 지원 버전 Godot/Unity의 정확한 버전과 도구 호환은 설치 시 공식 자료로 재확인한다. 다른 엔진을 같은 장면으로 실측하기 전 비교 우열을 확정하지 않는다. 전체 게임을 수리한 뒤에야 엔진 검토하는 순서로 돌아가지 않는다.

## 새 채팅용 복사문

```text
Dev-Gony/affix-zero 개발을 이어간다. ChatGPT 채팅+GitHub 방식이며 Codex를 요구하지 마라. chore/r0-preservation의 docs/project/HANDOFF.md, BASELINE.json, ENGINE_GATE.md, E0_ACCEPTANCE.md와 PR #18을 실제 조회하라. 사용자 Inspect 보고서는 이미 수신했다: 로컬 fix/g6-playtest-recovery c95b7ba, 변경 0, stash 15, Ryzen 5700X/GTX1050/31.9GiB/Win10이다. 사용 엔진 Godot_v4.3-stable_win64.exe도 직접 확인했으므로 다시 묻지 마라. 아직 새 Backup 보고서를 받아 VERIFIED_BYTES와 primary_save_present를 확인해야 한다. 실제 저장 경로·백업을 원격 완료했다고 말하지 마라. 기존 저장/작업/stash는 보존한다. 백업 확인 뒤 독립 E0 장면을 만들고 실제 에셋/공격/성능으로 엔진을 결정한다. Godot 고정 아님. E0 실측/시각 승인과 사용자 Windows 플레이는 NOT_RUN이다. 현재 문서 부모 SHA는 5409a1f이며 최신 원격 HEAD를 재조회한다. PR #17/#18 승인 없는 병합, reset --hard, git clean, 저장 초기화, 자동 stash pop은 금지다. 마지막은 항상 세 항목 진행 상황 체크포인트로 끝내라.
```
