# 로컬 작업 절차 v0.2

사용자는 Unity를 D드라이브에 설치 완료했다. Hub만 설치됐는지, Editor가 어떤 버전인지, 정확한 실행 파일 경로는 아직 실제 프로젝트 보고서로 확인하지 않았다. 설치를 반복 요구하지 않으며 아래 메뉴로 현재 설치를 읽는다.

## 새 프로젝트 받기 / 업데이트

작업 폴더는 `D:\github\affix-unity`, 브랜치는 `restart/unity-6`이다. 기존 Godot 두 폴더에서는 작업하지 않는다.

아직 새 폴더가 없는 경우에만 Git Bash:

```bash
git clone --depth 1 --no-tags --single-branch --branch restart/unity-6 https://github.com/Dev-Gony/affix-zero.git /d/github/affix-unity
```

이미 새 저장소를 받은 경우, Unity에서 변경 작업을 저장하고 에디터를 닫은 뒤:

```bash
cd /d/github/affix-unity && git pull --ff-only
```

로컬 변경으로 중단되면 덮어쓰기·reset·전체 stash를 자동으로 하지 않는다. 어떤 파일이 바뀌었는지 확인하고 의도적인 씬/.meta를 보존한다. 이번에 새 Unity 프로젝트까지 지웠다가 clone하라는 뜻이 아니다.

## Unity에서 확인

Unity Hub의 Projects에서 `D:\github\affix-unity` 루트를 추가/열기 한다. 이미 있는 폴더에 New Project를 생성하지 않는다. 기대 버전은 6000.3.24f1. 패키지 import와 C# 컴파일이 끝난 후 상단 `AFFIX → Project Dashboard`를 연다.

`Export setup report`를 누르면 실제 에디터 버전/실행 파일 경로/프로젝트 경로/애니메이션 세트 현황을 JSON으로 저장하고 탐색기가 열린다. 파일은 `Build/Reports/unity-setup-*.json`이다. 경로에 개인 정보가 있으면 공유 전 확인한다. 이 보고서가 생성되는 것은 메뉴가 로드됐다는 근거이지 전투 실행 성공 근거가 아니다.

AFFIX 메뉴가 없으면 컴파일 오류가 있을 수 있다. Console의 첫 오류를 전달하며 Unity 재설치부터 하지 않는다. 에셋이 없다는 경고와 컴파일 오류를 구분한다.

## 아트 연결 전후

현재 새 원본 팩이 없으므로 Create first encounter scene 버튼이 비활성인 것이 정상이다. 임의 캐릭터로 대체하지 않는다. ZIP 검수와 실제 Sprite 연결 뒤에만 씬을 만들며, 생성기는 기존 FirstEncounter 씬을 덮어쓰지 않는다. 플레이 후 source/.meta 변경은 작업 기록과 함께 명시적으로 반영한다.

## 과거 용량 확인

탐색기에서 `Tools\Local\Inspect-Storage.cmd`를 더블클릭한다. Python 설치나 Godot exe 경로는 필요 없다. 기본 검사 대상은 옛 `D:\github\affix`, `D:\github\affix-e0`이며 새 프로젝트의 Git 독립성 정보도 읽는다. 출력은 `Build/Reports/storage-*.json`.

이 도구는 원본 디렉터리와 Git 상태를 읽고 보고서만 생성한다. delete/move/stash/fetch/checkout을 하지 않는다. 읽기 제한·링크·시간 제한이면 PARTIAL로 표시하고 정확한 총용량이라고 보고하지 않는다.
