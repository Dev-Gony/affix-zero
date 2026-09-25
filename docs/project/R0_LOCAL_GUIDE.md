# R0 로컬 점검과 백업

## 1. 이번 작업의 범위

`tools/local/Invoke-AffixR0.ps1`은 Inspect(기본)와 Backup을 분리한다. 게임 실행, 설치, Git switch/pull/stash/commit, 저장 JSON 수정, 저장 복구를 수행하지 않는다. Windows PowerShell 5.1 이상과 기존 Git이 필요하다. 관리자 권한은 필요 없다.

Inspect는 origin 대상 일치 여부, 실제 branch/HEAD, 수정 항목 개수, stash 개수, CPU/GPU/RAM/OS 정보를 수집해 저장소 밖에 `share-summary.json`을 쓴다. 원격 주소 원문, 토큰, 파일 변경 내용, 저장 내용, PC 이름과 사용자 이름은 보고서에 포함하지 않는다. 공유 전 보고서 내용을 확인한다. 엔진 실행 파일을 지정하지 않으면 버전은 NOT_DETECTED다. 이는 엔진이 설치되지 않았다는 뜻이 아니다.

## 2. 먼저 점검만 실행

게임과 에디터를 종료한다. 현재 작업 폴더에서 Git Bash를 연다. 경로가 이전과 같다면 다음 명령을 그대로 사용한다. 다른 폴더라면 첫 줄만 실제 위치로 바꾼다.

```bash
cd /d/github/affix &&
git fetch origin chore/r0-preservation &&
R0_SCRIPT="$(mktemp --suffix=.ps1)" &&
git show origin/chore/r0-preservation:tools/local/Invoke-AffixR0.ps1 > "$R0_SCRIPT" &&
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w "$R0_SCRIPT")" -RepositoryPath "$(pwd -W)"
```

각 단계는 성공한 경우에만 다음으로 진행한다. `-ExecutionPolicy Bypass`는 이 PowerShell 프로세스의 실행 정책만 지정한다. 영구 정책이나 관리자 설정은 변경하지 않는다. 조직 정책으로 차단되면 우회하지 말고 오류를 전달한다.

이 단계에 pull이나 switch가 없는 이유는 로컬 변경을 먼저 파악해야 하기 때문이다. fetch가 원격 추적 객체를 갱신하는 것과 작업 폴더 파일을 바꾸는 것은 다르다. Inspect 자체는 Git 조회만 사용한다.

마지막 `Report:` 경로의 `share-summary.json`과 현재 사용 중인 Godot 버전을 전달한다. 수정 개수가 0보다 크면 아직 switch하지 않는다. 기존 stash를 pop하지 않는다. 도구는 미커밋 작업이나 stash를 백업하지 않으며 원래 상태로 남겨둔다.

## 3. 실제 세이브 경로 확인 후 백업

실제 실행본의 F8 진단에 나온 data path 또는 같은 버전 Godot 에디터의 사용자 데이터 폴더 열기 기능으로 기존 user:// 위치를 확인한다. 경로를 알아내려고 새 코드나 새 엔진으로 게임을 실행하지 않는다. 저장 폴더를 확인한 후 게임과 에디터를 모두 종료한다.

위 점검 명령을 실행했던 같은 Git Bash 창에서 다음을 실행한다. 별도 창이면 2절부터 다시 실행해 임시 파일 경로를 만든다.

```bash
powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File "$(cygpath -w "$R0_SCRIPT")" -RepositoryPath "$(pwd -W)" -Mode Backup -GameClosed -ChooseSaveDirectory
```

폴더 선택 창에서 실제 `save.json`이 있는 데이터 폴더 자체를 선택한다. `save.json.bak` 또는 `.tmp`만 있는 경우도 원본 보존은 가능하지만 정상 저장 복구 성공을 뜻하지 않는다. 파일이 전혀 없으면 도구는 성공으로 처리하지 않는다.

기본 출력은 `%LOCALAPPDATA%\AFFIX_ZERO_R0\날짜-고유ID\` 아래다. 저장 폴더 전체의 파일 바이트와 중첩 백업/알 수 없는 파일을 보존한다. 심볼릭 링크/정션, 256MiB 초과, 20,000개 초과 항목, 원본/출력 경로 중첩은 중단한다. 다른 Godot 작업이 켜져 있어도 보수적으로 중단하며 프로세스를 자동 종료하지 않는다.

정상 시 `backup.status`는 `VERIFIED_BYTES`, 실제 원본 저장이 있을 때 `primary_save_present`는 true다. 이는 바이트 일치 확인이지 게임에서 불러오기 성공이나 엔진 이관 완료가 아니다. 실패 폴더에 `INCOMPLETE.txt`가 있으면 검증된 백업으로 사용하지 않는다. 검증 완료 백업은 덮어쓰지 않는다.

공유할 것은 `share-summary.json` 하나다. `save-snapshot/`과 `VERIFIED.json`에는 실제 저장과 비공개 경로가 들어 있으므로 GitHub에 올리지 않는다. 복원은 별도 승인 작업이다.

## 4. 이후 동기화 원칙

실제 로컬 상태와 백업 결과를 확인한 뒤 정확한 테스트 브랜치를 정하고 `git pull --ff-only`를 안내한다. 이번 도구는 switch/pull을 대신 수행하지 않는다. 코드 롤백과 세이브 복원은 별개다. 기존 runtime은 변경하지 않았으므로 R0 도구 실행만으로 세이브 복원이 필요해지지 않는다.

## 5. 테스트 실행

개발자/CI용 합성 테스트는 아래와 같다. 이 명령은 실제 저장을 사용하지 않으며 고유 임시 디렉터리만 만들고 정리한다.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tests\local\r0_preflight_test.ps1
pwsh.exe -NoProfile -File .\tests\local\r0_preflight_test.ps1
```

두 번째 명령은 PowerShell 7이 있는 경우의 개발 검증이다. 사용자가 점검 도구를 쓰기 위해 PowerShell 7을 설치할 필요는 없다. 합성 테스트 성공과 사용자 PC의 백업 실행 결과는 별도로 보고한다.
