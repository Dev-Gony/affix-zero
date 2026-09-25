# 현재 작업: U0 Unity 새 시작

기존 Godot 유지 결정은 최신 사용자 요청으로 폐기됐다. 엔진은 Unity 6.3 LTS 6000.3.24f1, 언어 C#, 첫 렌더러 Built-in 2D다. restart/unity-6의 현재 트리에는 옛 코드/이미지/보존용 Godot 구현 폴더가 없다. Git 과거와 다른 브랜치는 이력일 뿐 새 개발 기준이 아니다.

현재는 Unity 프로젝트 기반, 새 순수 C# AttackTimeline, 실제 프레임을 요구하는 ActorAnimationSet, Editor 씬 준비 메뉴와 코어 테스트 소스다. 새 게임플레이/새 아트/Unity import/Windows 실행은 미완료다. 순수 C# CI 결과는 실제 실행 이후에만 별도 기록한다.

사용자 로컬 작업: 기존 affix/affix-e0에서 더 이상 pull/merge/stash하지 않는다. 빈 affix-unity 경로로 restart/unity-6을 clone한 뒤 Unity Hub로 해당 저장소 루트를 연다. 기존 두 폴더 및 Godot 세이브는 삭제/덮어쓰기하지 않는다. 사용자 요청은 저장소 새 개발선의 코드 폐기이며 로컬 개인 데이터 삭제는 아니다.

다음 작업 U1: Unity 설치/import 결과 확인 -> 정확한 라이선스의 새 자산 확보 및 실제 프레임 검수 -> 영웅1/근접적1의 밝은 필드에서 실제 동작과 타격. 기존 E0 순서, Godot 경로/backup 명령, 40마리 벤치마크로 되돌아가지 않는다.

다음 채팅용:

Dev-Gony/affix-zero의 restart/unity-6 최신 HEAD와 docs/HANDOFF.md, docs/RESTART_DECISION.md를 읽고 이어가라. 기존 코드 전체 삭제/Unity6.3LTS 전환은 사용자 명시 결정이다. 옛 코드는 archive나 .gdignore로 옮긴 것이 아니라 새 트리에서 삭제했다. 기획만 PRODUCT_BRIEF.md에 유지한다. 엔진6000.3.24f1, C#, Built-in2D, 새 폴더 affix-unity. 옛 이미지/막대 SVG/다크 분위기 재사용 금지. 현재는 U0 기초 소스와 순수C# 검사이며 Unity 에디터/게임 완성은 아니다. 사용자 Unity Hub 설치/import 상태와 신규 라이선스 에셋을 연결해 U1 첫 실제 화면을 만든다. PR #18/옛 worktree는 현재 작업 기준이 아니다. 정확한 로컬 절차, 실제 검증 범위, 트러블슈팅 네열 표, 마지막 세항목 체크포인트를 유지하라.
