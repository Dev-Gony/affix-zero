# 트러블슈팅 기록

기록일: 2026-09-26 KST. 원인 확정/가설/미검증을 구분한다. 기존 전투 문제는 PR #17과 기존 개발 로그를 참조하고 이번 R0에서 고쳤다고 쓰지 않는다.

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
| --- | --- | --- | --- |
| 새 채팅의 개발 기준 | AGENTS와 spec_lock에는 예전 PR-A와 4.3 고정이 남았고 최신 사용자는 원본 보존 후 엔진 재검토를 요청함 | 원문 blob을 archive에 보존하고 현재 HANDOFF/BASELINE/결정 기록으로 진입점을 교체. 런타임은 변경 안 함 | 과거 명세와 현재 실행 단계를 분리해야 같은 회귀를 반복하지 않는다 |
| 점검 도구를 받으려다 로컬 변경 충돌 가능 | 사용자 로컬 HEAD/dirty/stash를 아직 모르므로 먼저 switch/pull하면 위험 | fetch 후 git show로 도구만 임시 파일에 추출. 도구는 index optional lock을 끄고 조회만 수행 | 진단 도구 배포 자체가 작업 상태를 변경하면 안 된다 |
| 백업을 저장 성공/복구 성공으로 오인 | JSON을 파싱·재직렬화하면 손상/미지 필드의 원형이 사라질 수 있음 | 바이트 복사와 전후 SHA-256 검증. .bak/.tmp/중첩 파일 포함, VERIFIED_BYTES와 의미 검증 NOT_RUN 분리 | 보존과 복구와 마이그레이션은 서로 다른 계약이다 |
| R0 초기 Actions 실행 36155150296 | workflow 수준 failure와 jobs 0건 확인. 상세 검증 오류가 도구에서 반환되지 않아 정확한 원인은 미확정 | matrix shell 대신 명시적 Windows PowerShell 5.1/7 단계로 단순화. 후속 실행 결과 별도 기록 | 작업이 시작되지 않은 CI를 테스트 실패나 테스트 통과로 세지 않는다 |
| 사용자 보고서와 CI의 도구 SHA-256 불일치 | 저장소 LF 원문과 Windows checkout CRLF의 바이트 차이. 이번에 직접 재계산해 일치 확인 | blob 029d565의 원문 12801바이트를 재구성해 Git SHA 검증. LF의 3A36CA2F...FAD16은 사용자 보고서, CRLF의 A74FE34F...0F637은 CI 기록과 일치 | 텍스트 도구의 소스 동등성과 바이트 해시를 구분하되, 실제 저장 백업에는 정규화를 적용하지 않는다 |
| 설치된 Godot이 NOT_DETECTED로 표시됨 | Inspect 호출에서 -GodotExe를 지정하지 않음. 사용자는 Godot_v4.3-stable_win64.exe를 직접 확인함 | 직접 확인된 버전을 별도 근거로 BASELINE/HANDOFF에 기록. 재설치나 반복 버전 질문을 요구하지 않음. 실행 파일의 --version까지 검증한 것은 아님 | 자동 탐지 미실행을 소프트웨어 부재로 해석하지 않는다 |
| 작업 컨테이너에서 원격 복제 시도 실패 | github.com DNS 해석 실패로 git clone 불가. 사용자 PC의 오류가 아님 | 연결된 GitHub 도구로 정확한 SHA의 파일 읽기와 문서 변경 수행. 컨테이너에서 Godot/PowerShell 실행을 했다고 주장하지 않음 | 개발 도구 접근 실패와 게임 결함을 분리하고 실제 수행한 검증만 보고한다 |

| E0-C01 Godot 4.7.2 parse error | E0 helper `draw_ellipse(center, radius, color)`가 Godot 4.7.2의 새 native `CanvasItem.draw_ellipse()`와 이름 충돌. warning-as-error까지 발생 | 전사/근접 적 양쪽 헬퍼를 `_draw_shadow_ellipse()`로 변경. E0 run 36160446393에서 import + contract 통과, `E0_C01_TEST PASSED` 확인 | 엔진 마이너/메이저 전환 시 새 native API와 로컬 헬퍼 이름 충돌까지 회귀검사해야 한다 |

실제 사용자 백업/게임 실행에서 새 문제가 확인되면 정확한 보고서 상태와 코드 SHA를 추가한다. 개인 저장이나 토큰은 로그에 첨부하지 않는다. 지금 사용자 Backup은 아직 NOT_RUN이다.
