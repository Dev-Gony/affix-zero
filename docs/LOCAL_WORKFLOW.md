# 로컬 실행과 검증

프로젝트 D:\github\affix-unity, 브랜치 restart/unity-6, Unity 6000.3.24f1. 사용자는 현재 Hub에서 에디터를 설치 중이며 완료 후 알리기로 했다. 완료 안내 전 추가 실행은 보류한다.

기존 설치본 D:\Program Files\Unity 6000.3.24f1\Editor\Unity.exe의 버전/revision은 프로젝트와 일치했다. 최초 batch 실행은 No valid Unity Editor license found, exit198로 import 이전 중단됐다. 설치 완료 후에도 지속되면 Hub 본인 계정 로그인/사용 가능한 라이선스 활성화가 필요하다. 재설치부터 요구하지 않는다.

## 에디터에서

1. Hub에서 기존 D:\github\affix-unity를 연다. 같은 폴더에 New Project를 만들지 않는다.
2. AFFIX → Setup → Import Reviewed Art and Create Encounter. CC0 PNG를 slice하고 Hero/Enemy 세트, 초원 씬, Build Settings를 생성한다. 기존 FirstEncounter 씬은 보존한다.
3. Play에서 걷기/몸·무기 공격/피격/사망, HP, XP25·Gold8 자동수령 1회, HUNT AGAIN 초기화를 확인한다.
4. 생성된 .asset/.unity/.meta/ProjectSettings 변경을 검토·기록한다. 코어 PASS만으로 완료 처리하지 않는다.

AFFIX → Project Dashboard → Export setup report는 설치/로드 보고 도구다. 경로가 포함되므로 공유 전 확인한다.

## 실행 진입점

같은 프로젝트를 다른 Unity 프로세스가 열고 있지 않을 때만 실행한다. 각 프로세스 exit0과 오류 없는 로그를 확인하지 못하면 다음 단계로 가지 않는다. -projectPath는 위 프로젝트 루트, -logFile은 해당 Build/Reports 아래를 지정한다.

| 단계 | executeMethod | 플래그/완료 근거 |
|---|---|---|
| slice·씬 생성 | AffixZero.Editor.ReviewedArtSetup.Build | -batchmode -quit, scene/data 생성 및 로그 오류 없음 |
| 실제 Play | AffixZero.Editor.EncounterVerification.Run | -batchmode, **-quit 금지**, JSON PASS와 exit0 |
| Windows build | AffixZero.Editor.EncounterVerification.BuildWindows | -batchmode, windows-build.json Succeeded와 exit0 |

Play 검사는 짧은 idle 관측 후 정상 1대1 실행, 양측 피해/적 사망/유령 피해 없음/단발 보상/실제 씬 reload를 관측한다. 최대90초이며 로그 오류도 실패로 기록한다. PNG는 월드 카메라만 포함해 HUD는 별도 확인한다. 빌드 성공은 exe 실행·사용자 시각 승인이 아니다.

## 에디터 없이 가능한 검사

모든 명령은 PowerShell에서 $ErrorActionPreference = 'Stop'을 설정하고 정확한 프로젝트 루트로 이동한 뒤 실행한다. 네이티브 명령 후 $LASTEXITCODE가 0이 아니면 throw로 중단한다.

- dotnet run --project Tests/CoreSmoke/CoreSmoke.csproj --configuration Release
- Tools/Local/Test-UnitySources.ps1 -UnityEditorPath 'D:\Program Files\Unity 6000.3.24f1\Editor\Unity.exe'
- python Tools/validate_reviewed_art.py
- python Tools/preview_reviewed_art.py

에셋 도구에는 Python/Pillow가 필요하다. C# 정적 검사는 설치 DLL API 참조만 컴파일하므로 Unity asmdef/패키지/import/렌더/Play를 대신하지 않는다. Build/Reports/art-preview.html도 원본 재생 도구이며 게임 실행화면이 아니다.

옛 저장소 용량은 Tools/Local/Inspect-Storage.cmd의 읽기 전용 보고만 사용한다. 이번 개발에서 옛 폴더 삭제나 stash 적용을 하지 않았다.