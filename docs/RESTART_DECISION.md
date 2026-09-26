# 재시작 결정 v0.2

## RESTART-001 | 2026-09-26

사용자의 명시 요청에 따라 기존 코드/아트 전체를 폐기하고 Unity 6.3 LTS 6000.3.24f1, C#, Built-in 2D에서 시작한다. 새 브랜치는 restart/unity-6, PR #19, 새 로컬 위치는 affix-unity다. 기존 Godot/E0/V0 소스는 새 파일 트리에 없다. 과거 부모 commit과 다른 브랜치를 남긴 것은 이력 보존이며 현재 구현 재사용이 아니다.

엔진 선택은 AFFIX 제작 도구/2D 작업/C# 설계를 위한 결정이다. 과거 merge 오류나 거절된 화면만으로 Godot 엔진 자체의 결함 또는 악성코드 감염이 증명된 것은 아니다. Hero Siege는 제작자 배포 페이지에 GameMaker가 명시됐지만, Survivor.io의 특정 버전 엔진을 직접 확인한 제작사 기술자료는 확보하지 못했으므로 이를 Unity 선택의 확정 사실로 쓰지 않는다.

## WORKFLOW-002 | 설치 확인 후

사용자는 Unity D드라이브 설치 완료를 확인했다. 설치 폴더 이름이나 실행 파일을 추측하지 않고 Project Dashboard에서 실제 Editor 버전/경로를 보고한다. 설치 완료와 프로젝트 import/Play 완료는 구분한다. 아직 새 라이선스 팩이 없으므로 빈 씬을 게임으로 보여주지 않고 리소스 검수/연결부터 진행한다.

## RETENTION-003 | 문서 중심 정리

과거 소스/이미지를 현재 프로젝트의 archive로 복사하지 않는다. 실패 기록은 history/LEGACY_RETROSPECTIVE.md에 텍스트로만 보존한다. 사용자가 공간 확보 후 삭제를 검토하므로 읽기 전용 용량/worktree 검사부터 제공한다. 실제 로컬 삭제·세이브 포기·원격 history rewrite는 각각 별도 승인을 받아야 한다.

## 근거와 제한

현재 에디터 사용 버전 기준: ProjectSettings/ProjectVersion.txt.
공식 참고: https://unity.com/releases/editor/whats-new/6000.3.24f1
Hero Siege 제작자: https://panicartstudios.itch.io/hero-siege
Git 보존/정리: https://git-scm.com/docs/git-worktree ; https://git-scm.com/docs/git-clone
새 자산 원본 조건: https://pixelfrog-assets.itch.io/tiny-swords

이 문서는 이전 결정을 현재 사실과 구분해서 기록한다. Unity 에디터/실제 아트/새 게임 완성은 실제 검증 전까지 NOT_RUN이다.
