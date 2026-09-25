# AFFIX: ZERO 현재 인수인계

기록일: 2026-09-26 KST. 다음 세션은 이 파일과 BASELINE.json, DECISIONS.md, R0_LOCAL_GUIDE.md를 읽고 실제 원격 HEAD/PR/CI를 재조회한다.

## 현재 작업

작업 브랜치: `chore/r0-preservation`. 기준은 PR #17의 `fix/g6-playtest-recovery` / `c95b7ba0ab64104470076e4a78cce172b1fd602d`다. main은 `a1418ad6a52f1a9e41607e29c7ecb56edbb96ab0`, 기존 개발선은 `dev/gameplay-v5-pets-items` / `433948879fdfd23cf1cccaa0ae0c8ce3a0784eae`로 재확인했다. 원본 PR #17은 Draft/open이며 병합·Windows 플레이 승인 없음.

이번 변경은 R0-01 실행 도구와 R0-04 문서 충돌 정리다. 게임 코드·장면·리소스·아트·저장 형식은 변경하지 않는다. 새로운 브랜치를 만들었다고 기존 복구 후보를 승인한 것이 아니다.

추가 파일: Windows 로컬 점검/명시적 세이브 백업 도구, PowerShell 합성 테스트, Windows Actions, 현재 기준·엔진 결정·로컬 가이드·트러블슈팅. 이전 README/AGENTS는 archive에 원본 blob으로 보존했다. 45페이지 개발 마스터 v0.2 전체는 이전 채팅 첨부다. 이번 폴더는 원문의 대체본이 아닌 최신 실행 상태다.

## 완료와 대기 구분

도구 구현은 코드 존재 상태다. 실제 검증 결과는 VALIDATION.md와 해당 SHA의 Actions를 확인한다. 사용자 PC의 HEAD/수정 파일/실제 저장 위치/백업은 아직 미확인이다. 사용자 Windows 게임 플레이, 아트 승인, E0 엔진 비교/성능 측정은 NOT_RUN이다.

현재는 R0 도구로 로컬 사실을 확인하는 단계이며 가챠·직업 확장 단계가 아니다. 도구의 VERIFIED_BYTES는 파일 일치이지 저장 복구 성공이 아니다. 합성 fixture로 사용자의 장비/펫을 복원했다고 말하지 않는다.

## 다음 행동

사용자에게 R0_LOCAL_GUIDE 2절의 Git Bash 명령을 제공한다. fetch와 임시 스크립트 실행만 하며 로컬 switch/pull/stash/commit은 아직 하지 않는다. `share-summary.json`과 현재 Godot 버전을 받는다. 이후 실제 user://를 확인하고 같은 도구의 Backup 모드로 보존한다. 백업 결과의 상태/primary_save_present를 확인한다.

R0-02는 기존 복구 후보의 소환 결과, 전투/펫 가림, 목표 HUD/소유권 보존 3개 검사다. 기존 버그 전체를 고친 뒤에야 엔진을 결정하지 않는다. R0-01 후 E0-01/02/03으로 최소 전투·에셋 제작과 엔진을 검증한다. 저장 중단 복구/환생 오류 안내/미지 필드는 독립 미해결 검증 대상이다.

## 새 채팅용 복사문

```text
Dev-Gony/affix-zero 개발을 이어간다. ChatGPT 채팅+GitHub로 작업하며 Codex를 요구하지 마라. chore/r0-preservation의 docs/project/HANDOFF.md, BASELINE.json, DECISIONS.md, R0_LOCAL_GUIDE.md와 최신 PR/Actions를 읽어라. 이 브랜치는 미승인 PR #17 c95b7ba를 기준으로 R0 점검/백업 도구만 더한 작업선이며 main 병합 승인이 아니다. 실제 사용자 로컬 상태와 save 백업은 보고서를 받아야 완료다. share-summary.json을 받은 적이 있는지 확인하고 중복 요청하지 마라. Godot 고정 아님. 작은 전투 코어/아트 검증 E0 후 엔진을 결정하며 기존 코드 때문에 전환을 피하지 않는다. 실제 Windows/렌더/성능을 실행하지 않았다면 NOT_RUN이다. 저장 초기화, reset --hard, git clean, force push, 자동 stash pop, 승인 없는 병합을 하지 마라. 필수 로컬 명령을 제공하고 트러블슈팅 네 열 표와 마지막 세 항목 체크포인트를 유지하라.
```
