# 로컬 실행 — 현재 Stitch 전투 HUD

고정 Unity6000.3.24f1, D:/Program Files/Unity 6000.3.24f1/Editor/Unity.exe. 프로젝트는 D:/github/affix-unity. 설치/라이선스는 검증됐다.

## 무료 원본 반입

1. [공식 Zerie 무료 Soldier/Orc](https://zerie.itch.io/tiny-rpg-character-asset-pack)의 검수 버전 ZIP을 로컬 확보한다. ZIP SHA256과 파일 SHA는 docs/assets/zerie-local-manifest.json 및 Tools/Local/Import-FreeCharacters.ps1을 따른다.
2. PowerShell에서 `$ErrorActionPreference='Stop'`을 설정하고 프로젝트 루트에서 `./Tools/Local/Import-FreeCharacters.ps1 -ZipPath '<공식 ZIP 절대경로>'`를 실행한다. script가 경로/ZIP/파일해시/Git제외를 검증한다. 다른 버전이나 기존 파일 불일치는 중단하며 덮어쓰지 않는다.
3. Unity의 `AFFIX/Setup/Build Hero Siege Art Review Scene`을 실행한다. Assets/LocalLicensed 캐릭터는 공개 재배포하지 않는다. 공개 clone의 기존 animation data 참조는 이 단계에서 새 로컬 원본으로 갱신한다.

현재 실제 반입 스크립트 실행 결과 기존 PNG10개·ZIP·script·manifest의 해시/크기/수정시각이 그대로 유지됐다. 생성 방·CC0 아이콘·OFL 폰트는 공개 추적된다. 예전 ReviewedArtSetup은 거절된 Ninja용이며 새 씬에 사용하지 않는다.

## Unity 실행

같은 프로젝트를 다른 Unity 프로세스가 열고 있지 않을 때 실행한다. 모든 batch 명령에 정확한 `-projectPath`와 `-logFile`을 지정하고 exit0 및 최신 보고서 성공을 확인한 뒤 다음 단계로 이동한다.

| 단계 | executeMethod | 플래그 |
|---|---|---|
| 신규 자산 slice·씬 | AffixZero.Editor.HeroSiegeArtSetup.Build | -batchmode -quit |
| 월드 Editor Play | AffixZero.Editor.EncounterVerification.Run | -batchmode, -quit 제외 |
| Windows 빌드 | AffixZero.Editor.EncounterVerification.BuildWindows | -batchmode, -quit 제외 |

빌드 출력은 Build/Windows/AffixZero.exe. 일반 실행은 자동 검사 파일을 만들지 않는다. `-affixSmokeTest -affixReportDir '<절대 보고서 폴더>' -screen-fullscreen 0 -screen-width 1280 -screen-height 720`로 검사 실행할 수 있다. 1920×1080도 별도로 검증했다. 검사 실행은 완료 후 종료한다.

실제 화면 캡처를 위해 검수 게임 창은 표시해야 한다. Unity batch 에디터만 숨긴 창으로 실행한다. probe는 공유 API로 pause/정보창/재시작을 검증하고, 매 캡처에서 UIDocument·HP·XP·골드·창 상태를 검사한다. 실제 버튼 클릭/키보드 자동화는 아니다. Editor Camera.Render 캡처는 HUD를 포함하지 않는다.

최신 결과는 docs/validation/stitch와 docs/media/stitch. 새 빌드 GUID54d35624853d494cbee6127fe860214e, 720/1080 player모두PASS. BMP→PNG는 동일 RGB 픽셀임을 검사했다. 시안 전체 구현이나 사용자 승인으로 읽지 않는다.

## 빠른 소스 검사

프로젝트 루트에서 `$ErrorActionPreference='Stop'`을 설정하고 네이티브 명령마다 `$LASTEXITCODE`가 0이 아니면 throw로 중단한다.

- dotnet run --project Tests/CoreSmoke/CoreSmoke.csproj --configuration Release
- Tools/Local/Test-UnitySources.ps1 -UnityEditorPath 'D:/Program Files/Unity 6000.3.24f1/Editor/Unity.exe'
- python Tools/audit_restart.py --require-history
- python Tools/validate_reviewed_art.py (남겨 둔 이전 CC0 기술 fixture 검사이며 새 Zerie 아트 검증이 아님)

정적 DLL 컴파일/.NET CI는 Unity import·Play·실행·시각 검수를 대체하지 않는다. 소스 예시 art-preview.html도 게임이 아니다. 사용자 폴더·세이브·stash·Git history는 삭제하지 않는다.
