# 트러블슈팅 v0.2

과거 회고는 history/LEGACY_RETROSPECTIVE.md에 있다. 현재 문제와 예방 조치를 구분하며 실제 검증 결과는 VERIFICATION.json을 참조한다.

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
| --- | --- | --- | --- |
| project.godot 반복 동기화 중단 | 로컬 수정과 원격 변경이 겹침. 엔진 결함을 증명한 것은 아님 | 별도 Unity clone으로 전환하고 옛 폴더에 merge를 요구하지 않음 | 작업 경로와 에디터 메타 정책을 먼저 정해야 한다 |
| 기존 아트/구현 재사용 | 실패한 화면과 지침을 새 단계에 반복 반입 | 새 Unity 트리와 텍스트 전용 회고, 과거 소스 복원 금지 | 이력 보존과 재사용은 별개다 |
| 검사 통과를 게임 완성처럼 표현 | 코어/렌더/플레이/시각 검사 혼합 | 코드·코어·Unity import·Play·시각·빌드 상태 분리 | PASS는 검증 환경과 범위까지 포함한다 |
| D드라이브 설치 경로 오입력 위험 | 실제 Unity 경로를 아직 모름 | 에디터 API로 version/path 보고, 외부 exe 경로를 추측하지 않음 | 확인 가능한 도구에서 직접 환경 정보를 읽는다 |
| 옛 부모 저장소를 먼저 삭제할 위험 | linked worktree가 공용 Git metadata를 사용 | 읽기 전용 용량/common-dir/worktree 상태 보고 후 순차 정리 계획 | 파일 삭제와 Git 연결 해제는 다르다 |
| shallow clone에서 과거 삭제 검사 실패 위험 | baseline commit을 얕은 clone이 포함하지 않음 | 일반 감사는 현재 트리 검사, CI --require-history는 과거 비교를 필수로 유지 | 용량 절약과 검증 범위를 명시적으로 나눠야 한다 |
| 새 에셋 없이 가짜 화면을 만드는 위험 | 리소스 공급과 구현 준비를 혼합 | 실프레임/ground/라이선스 검수 전 씬 생성 차단 | 결손 상태를 숨기지 말고 정확히 노출한다 |
| 저장공간 도구 Windows CI에서 PASS 출력 후 exit 1 | 합성 폴더가 Git repo가 아닌 것은 정상인데 Git 조회 실패가 LASTEXITCODE에 남아 runner가 실패로 종료. 최초 run 36174165918/job108200697247에서 재현 | Git 실패는 JSON의 GIT_READ_FAILED로 남기고 보고서 저장 성공 시 도구 종료 상태를 명시적으로 정상화. 종료 코드와 비저장소 상태를 검사에 추가. Dictionary 표시는 PSCustomObject로 변환 | 예상된 데이터 부재와 도구 실행 실패를 분리하되 보고서에 부재를 숨기지 않아야 한다 |
| 첫 Unity batch 실행 | 기존 설치본 버전은 일치하나 유효 license/entitlement 없음, exit198 | import 이전 중단을 기록. 당시 Hub 설치 완료 안내까지 추가 실행을 보류했고, 이후 설치 완료 후 import 성공 | 설치 exe 존재와 실제 사용 가능한 에디터는 구분한다 |
| Tiny Swords 후보 반입 | CC0 구버전 Warrior에 Hit 태그/클립 부재 | 동봉 CC0 전문과 모든 필수 클립이 있는 신규 Ninja Adventure로 전환 | 보기 좋은 공격 예시만으로 U1 계약을 충족하지 않는다 |
| 치명타 직후 공격 자세 끊김 | 사망 대상 분기가 recovery까지 즉시 취소 | impact 소비 후에는 피해 없이 recovery 유지, object 대상 잠금 | 피해 판정과 표시 동작 수명을 구분한다 |
| 첫 전투 무한 경직 수치 조합 | 피격2프레임/6fps가 영웅 다음 impact보다 길어 적이 반복 취소 | reactionFps10 적용. 30/60/120fps·양측 실행 순서 수치 시뮬6조건 검토. Unity Play는 대기 | 실제 속도와 취소 규칙의 조합을 확인한다 |
| 몸32px와 무기64px 정렬 | 동일 normalized pivot은 서로 다른 발 위치를 가리킴 | body(.5,.25), weapon(.5,.375), PPU16 및 동일 시간표 | 절대 픽셀 기준점으로 정렬한다 |
| 자동 Play 검사 재시작 상태 오독 | LoadScene 요청 직후 이전 액터를 검사할 가능성 | sceneLoaded 완료 후 신규 액터 바인딩·이벤트 해제 | 요청과 완료 이벤트를 분리한다 |
| 첫 Play 진입 직후 Unity Search 예외 | 에디터 delayCall 검색 초기화보다 EnterPlaymode가 먼저 실행되어 기본 인덱스 미생성 | 시작 delayCall 두 차례 후 compiling/updating 종료·1초 안정 상태를 기다려 Play 진입. 오류 감시는 계속 유지. 재검사 PASS | 에디터 시작 요청과 모든 지연 초기화 완료는 다르다 |
| Windows player 컴파일에서 EncodeToPNG 부재 | 전체 설치 DLL에는 ImageConversion API가 있지만 프로젝트 player 모듈에는 없음 | 승인 없이 패키지를 바꾸지 않고 System.IO로 24-bit BMP 저장. 실제 Windows 빌드0오류0경고 | 설치 DLL 정적 컴파일은 player 모듈 선택을 재현하지 못한다 |
| 숨긴 게임 창에서 ReadPixels 실패 | 화면 프레임을 렌더하지 않는 숨김 창에서 framebuffer 읽기 요청 | 실제 검수용 게임 창을 표시해 동일 exe 재실행, HUD 캡처3장과 전체 smoke PASS | 백그라운드 로직 검사와 실제 화면 렌더 검사를 구분한다 |
| 실제 player의 잔디/꽃 가장자리에 얇은 선 | 기본 MSAA2가 인접 atlas 영역을 혼합. 원본 잔디 셀은 단색인데 경계 픽셀은 이웃 주황색과50%혼합 | 픽셀 sprite 카메라만 allowMSAA=false. 최종 player 재빌드/실행 PASS, 비교 구간330픽셀 모두 원본 잔디색 확인 | 원본 이미지 결함과 렌더 샘플링 결함을 픽셀·환경 비교로 분리한다 |
| 빌드 실패 JSON의 오류 수가0으로 덮임 | catch가 기존 BuildReport 요약을 새 객체로 덮음 | 단일 보고 객체를 공유하고 exception은 problem에 기록, 요약 미획득은-1로 구분 | 진단 과정에서 원래 실패 증거를 잃지 않는다 |

현재 실제 Unity import/Play·Windows 빌드·독립 실행 검사를 통과했다. 사용자 시각 승인은 별도 대기 상태다. 읽기 전용 도구 테스트는 임시 합성 데이터만 사용하며 실제 사용자 파일은 삭제하지 않는다.

## UI 방향 재검토 — 2026-09-26

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
|---|---|---|---|
| 사용자가 전투 UI 방향 거절 | 장르명과 밝은 팔레트만 반영하고 실제 레퍼런스의 정보 위계·화면 상태·전장 비중을 구현 전에 대조하지 않음 | 공식 Hero Siege 전투/장비 화면과 Survivor.io 전투/성장 선택을 조사. 사용자 PC 가로형 확정. 소개 카드 HUD를 폐기하고 가장자리 전투 HUD·별도 캐릭터 정보·상황성 결과로 재작업 | 자동 전투·빌드 PASS는 제품의 시각 방향 승인 근거가 아니다. 레퍼런스의 기능과 배치를 먼저 관찰하고 구현 범위와 연결한다 |


## Stitch 네이티브 UI — 2026-09-26

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
|---|---|---|---|
| UI 재차 거절 | 기존 코너 HUD 수정은 사용자 의도와 달랐고 이후 직접 제작한 Stitch 시안이 제공됨 | 화면4개를 직접 보고 HTML/의존성/실제JS를 조사, 전투 HUD를 UI Toolkit으로 재작성하고 실제1대1 상태 연결 | 기능 PASS와 시안의 구조·상호작용 일치는 별도다 |
| 네이티브 HUD 검사 시 화면 갱신 순서 차이 | probe가 LateUpdate에서 pause/창상태를 바꾸면 같은 프레임의 Update UI에는 반영 전일 수 있음 | HUD 수치 갱신을 LateUpdate로, 캡처는 다음 프레임 EndOfFrame 후 실제 label/visibility 비교 | 화면 캡처 전 모델과 레이아웃의 갱신 주기를 일치시킨다 |
| 참조 문서 hash 확인 도구의 첫 assertion 실패 | 파일 정렬 첫 항목이 DESIGN.md일 것이라고 가정 | 파일명으로 명시 선택해4개동일hash확인 | 경로 목록의 정렬 순서를 파일의 의미로 간주하지 않는다 |


## 장비·특성 화면 — 2026-09-26

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
|---|---|---|---|
| 캐릭터 초상 때문에 실제 player 시작 실패 | UI Toolkit Image.sprite 모드에서 sourceRect를 설정하면 런타임 오류 | Image.image에 원본 Texture를 사용하고 Sprite의 bottom-left rect를 Texture top-left crop으로 변환 | 정적 API 컴파일은 속성 조합의 런타임 제한을 검증하지 못한다 |
| 새 HUD PanelSettings에 기본 테마 경고 | 동적 PanelSettings에 ThemeStyleSheet 미지정 | RuntimeTheme.tss에서 unity-theme://default를 import하고 Resources로 명시 연결 | 요소에 직접 스타일을 지정해도 기본 런타임 패널 설정은 갖춘다 |
| 장착 중 진행하던 공격의 피해까지 바뀔 가능성 | impact 시점에 현재 damage 필드를 읽음 | 스윙 시작에 damage를 저장하고 해당 공격이 끝날 때까지 사용 | 빌드 변경과 진행 중 공격의 판정 시점을 분리한다 |
| 다음 전투에서 누적 보상이 사라져 보임 | HUD가 해당전투의 일회성 rewards만 표시 | 중복 처치 방지 모델에 세션 TotalExperience/TotalGold를 추가하고 HUD에 연결 | 전투별 검증 값과 플레이어 누적 진행 값은 분리해야 한다 |
