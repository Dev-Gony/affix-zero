# AFFIX: ZERO | Unity 재시작

**현재 개발선:** `restart/unity-6` · PR #19 · Unity 6.3 LTS `6000.3.24f1` · C# · Built-in 2D.

사용자는 Unity를 D드라이브에 설치했다고 확인했다. 정확한 실행 파일 경로, 설치 버전, 이 프로젝트의 에디터 import/Play 완료 여부는 아직 보고받지 않았다. 설치 질문을 반복하지 말고 Unity에서 실제 경로를 보고하게 한다.

## 현재 상태

기존 Godot 코드와 이미지는 현재 파일 트리에 없다. 새로 작성한 C# 공격 시간표와 체력/중복 타격 규칙, Unity 액터 연결 코드, 실제 프레임 데이터 검사, 첫 전투 씬 제작 도구를 제공한다. **새 아트 미반입, Unity 실행 미검증이므로 플레이 가능한 첫 빌드 완료 상태가 아니다.**

Unity 상단 `AFFIX → Project Dashboard`에서 실행 중인 에디터 정보와 에셋 준비 상태를 확인한다. `Export setup report`는 `Build/Reports`에 JSON을 저장하고 위치를 연다. 설치 경로를 Git Bash에 추측해 입력할 필요가 없다.

## 문서

문서 목차는 [docs/README.md](docs/README.md), 재개 지점은 [docs/HANDOFF.md](docs/HANDOFF.md).

- [PRODUCT_BRIEF.md](docs/PRODUCT_BRIEF.md): 핵심 기획·MVP·수락 조건
- [ARCHITECTURE.md](docs/ARCHITECTURE.md): Unity 구현 책임과 검증 범위
- [ROADMAP.md](docs/ROADMAP.md): 실제 순서와 장기 콘텐츠
- [ART_DIRECTION.md](docs/ART_DIRECTION.md), [ASSET_INTAKE.md](docs/ASSET_INTAKE.md): 새 리소스 원칙
- [LOCAL_WORKFLOW.md](docs/LOCAL_WORKFLOW.md): 로컬 업데이트·메뉴·보고서
- [CLEANUP_PLAN.md](docs/CLEANUP_PLAN.md): 과거 폴더 용량·삭제 계획
- [history/LEGACY_RETROSPECTIVE.md](docs/history/LEGACY_RETROSPECTIVE.md): 코드·이미지를 포함하지 않은 회고
- [VERIFICATION.json](docs/VERIFICATION.json): 실행한 검사 증거

## 과거 정리

현재 프로젝트에는 과거 구현 사본을 보관하지 않는다. 회고는 텍스트만 보관한다. 로컬 예전 폴더 삭제와 원격 Git 이력 삭제는 별도 작업이며 이번 갱신에서 수행하지 않았다. `Tools/Local/Inspect-Storage.cmd`는 용량·worktree 연결 관계를 읽고 보고서만 만든다.

## 실행 전제

아직 영웅·적·바닥 원본 리소스가 확보되지 않았다. 검수 전 임시 막대 그림이나 거절된 아틀라스로 빈자리를 채우지 않는다. 라이선스와 실제 프레임을 확인한 뒤 첫 전투 씬을 생성한다. `.meta`는 소스와 함께 추적하며 `Library`, `Temp`, `Build` 같은 캐시는 제외한다.
