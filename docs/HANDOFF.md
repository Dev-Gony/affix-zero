# 현재 인수인계 | Unity U1 연결 준비

갱신: 2026-09-26. 현재 저장소 Dev-Gony/affix-zero, 브랜치 restart/unity-6, PR #19 Draft. 현재 Unity 소스 기준은 009740a1cee5b1e7846c5531f9a6269439b192eb이며 이후 문서와 검증 결과 커밋은 원격에서 재조회한다.

## 확정된 사용자 정보

Unity를 D드라이브에 설치 완료했다. 버전/정확한 exe 경로/프로젝트 import는 아직 보고서로 확인하지 않았다. 기존 Godot 코드와 이미지 전부 폐기, Unity/C#로 새 시작, 밝은 외부 라이선스 리소스 우선, 채팅+GitHub 개발이다. 과거 작업은 용량이 크면 지우고 문서로만 남기길 선호한다.

## 작성된 것

새 AttackTimeline에 더해 CombatHealth의 HP/방어/공격자+공격ID 중복 차단/사망1회와 코어 테스트를 추가했다. MeleeActor는 실제 이동·프레임 표시·impact 피해·피격·사망 연결 코드다. ActorAnimationSet은 클립 재생과 방향을 제공한다. Project Dashboard는 실제 에디터 경로/버전 보고와 검수된 영웅/적/바닥을 연결하는 씬 제작 도구다. 빈 씬 생성 메뉴는 Dashboard로 연결한다.

로컬 Tools/Local/Inspect-Storage.cmd는 옛 두 폴더 용량과 Git 연결 관계를 읽고 Build/Reports에 JSON만 저장한다. 삭제 기능은 없다. 초기 clone은 depth1/no-tags를 허용하며 audit는 shallow 상태와 CI의 역사 비교를 구분한다.

## 아직 완료되지 않은 것

신규 원본 ZIP/동봉 라이선스/실제 프레임은 미확보·미검수다. Unity import/Editor 메뉴 실행/실제 플레이/프레임 품질/Windows 빌드는 미검증이다. 신규 XP/Gold 회수·장비·스킬·펫·시즌은 미구현이다. 코어 CI와 도구 CI 결과는 VERIFICATION의 source SHA를 보고 사용한다. U1 전체 완료라고 쓰지 않는다.

## 다음 실제 행동

사용자는 새 Unity repo를 최신화하고 Hub에서 루트 폴더를 연 뒤 AFFIX/Project Dashboard → Export setup report를 실행한다. 정확한 Unity exe 경로를 다시 묻지 않는다. 삭제를 검토하려면 별도 Inspect-Storage.cmd 보고서를 만든다. 개인 경로는 공유 전 확인한다.

개발은 Tiny Swords 제작자 제공 CC0 구버전 ZIP 확보와 license/frame 검수 → 새 Sprite import/ActorAnimationSet → 밝은 첫 전투 씬 생성/실행 순서다. 에셋이 없다고 거절된 이미지·막대 도형을 복원하지 않는다. 그 뒤 U1-D 회수와 U2로 간다.

## 새 채팅 재개 프롬프트

Dev-Gony/affix-zero의 restart/unity-6 최신 HEAD, PR #19, docs/HANDOFF.md, STATUS.json, VERIFICATION.json을 읽고 이어가라. 사용자는 Unity를 D드라이브에 설치했고 기존 코드/아트는 완전히 폐기했다. 현재 U1 연결 소스 009740a에는 AttackTimeline, CombatHealth, MeleeActor, ActorAnimationSet, FirstEncounter, ProjectDashboard와 읽기 전용 저장공간 보고 도구가 있다. Unity 에디터 자체 검증과 새 원본 아트는 아직 없다. 단순 C# 테스트를 게임 완성으로 보고하지 마라. 새 리소스 후보는 Tiny Swords CC0 구버전이고 원본/라이선스/동작을 실제 검수해야 한다. 과거는 docs/history의 텍스트 회고만 보존하고 코드/이미지를 복원하지 마라. 원격 Git history와 사용자 옛 폴더는 삭제하지 않았으며 용량/worktree 관계 확인 후 구체적 승인으로만 정리한다. 필요한 로컬 행동은 최소화하고 끝에는 세 항목 진행 상황 체크포인트를 남겨라.
