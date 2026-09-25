# R0 검증 기록

기록일: 2026-09-26 KST. 결과는 정확한 코드 SHA에 연결한다. 이 문서를 추가한 문서 전용 커밋과 테스트한 커밋을 구분한다.

## 실행 증거

- 테스트 코드/도구/워크플로 기준: `288ae4f0df8894e7fe3a12a6d672740f332d9d47`
- GitHub Actions: [R0 preservation tool / 36155308288](https://github.com/Dev-Gony/affix-zero/actions/runs/36155308288)
- job: `108138272736`, `windows-safety`
- 환경: Windows Server 2025, runner `windows-2025-vs2026`, Git 2.55.0.windows.5
- Windows PowerShell `5.1.26100.33296`: **34 checks, 0 failures**
- PowerShell `7.6.5`: **34 checks, 0 failures**
- 위 두 실행은 같은 34개 검사를 서로 다른 PowerShell에서 반복한 것이다. 68개 서로 다른 시나리오가 아니다.
- 실행된 도구 파일의 SHA-256: `A74FE34F585968DE3ECBC9DEBD7CF0EDDB8CCA09A0941CCC1DEBD51A0010F637`

결과는 workflow/job 상태와 원본 job 로그를 모두 읽어 확인했다. 모든 입력은 각 테스트가 만든 고유 임시 저장소/세이브이다. 사용자 실제 저장은 읽지 않았다.

## 검사 범위

| 구분 | 실제 검사한 동작 |
| --- | --- |
| Git 진단 | 실제 HEAD, dirty/stash 개수, index와 미추적 파일 불변, 잘못된 origin/저장소 거부 |
| 민감 정보 | origin에 들어 있는 합성 토큰이 보고서에 포함되지 않음 |
| 바이트 보존 | 손상된 JSON, .bak, .tmp, 중첩/알 수 없는 파일을 재직렬화 없이 그대로 복사 |
| 중복/경로 방어 | 기존 백업 덮어쓰기 거부, 매번 별도 폴더, 빈/없는 저장 거부, source/destination/repo 중첩 거부 |
| 중단/변경 | 복사 도중 합성 원본 변경 시 VERIFIED 없이 INCOMPLETE, .bak만 있을 때 primary 없음 구분 |
| 파일 경로 | 정션과 중첩 정션 거부, 공백/대괄호/한글 경로, 유사 접두어 구분 |
| 사용자 실행 경로 | 별도 PowerShell 프로세스의 Inspect CLI, 보고서 1개 생성, 도구 SHA/NOT_RUN 상태 검증 |

## 미검증과 경계

- 사용자 Windows PC의 실제 branch/HEAD/미커밋 상태: **UNKNOWN**
- 사용자 실제 user:// 위치와 세이브 백업: **NOT_RUN**
- 폴더 선택 GUI 수동 클릭과 사용자 PC의 Backup CLI: **NOT_RUN**. 백업 핵심 함수는 합성 파일로 실행했다.
- 실제 저장을 게임에서 다시 읽는 복원/마이그레이션 검증: **NOT_RUN**
- 사용자 아트/Windows 게임 플레이 승인: **없음**
- E0 엔진별 장면/성능/빌드 비교: **NOT_RUN**
- R0는 게임 코드/장면/에셋/저장 형식을 바꾸지 않는다. 이 CI로 기존 게임의 모든 회귀 검사를 통과했다고 주장하지 않는다.
- Linux 작업 컨테이너에서는 PowerShell을 실행하지 않았다. 위 결과는 실제 GitHub Windows runner의 결과다.

## 초기 실패와 후속 결과

초기 커밋 `1f1c556d2df85354694cae9150434784cf7f099f`의 실행 `36155150296`은 workflow 수준에서 failure, jobs 0건이었다. 상세 유효성 오류가 조회 도구에 반환되지 않았으므로 정확한 원인 확정은 하지 않았다. shell matrix를 명시적 PowerShell 5.1/7 단계로 바꾼 `288ae4f`에서 실제 두 테스트가 실행되고 통과했다.

다음 코드 변경 시 위 PASS를 복사하지 말고 새 SHA의 실행과 로그를 확인한다. 문서만 변경한 경우에도 코드/테스트/워크플로 blob이 동일한지 확인하고, 새 전체 HEAD에서 실행했는지는 별도로 적는다.
