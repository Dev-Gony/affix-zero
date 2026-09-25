# AFFIX: ZERO | Unity 새 시작

기준: Unity 6.3 LTS `6000.3.24f1`, C#, 2D(Built-in), Windows 우선.

기존 Godot 코드, E0/V0, 이미지, 캐시 우회 스크립트, 오래된 구현 지침은 이 개발선에 없다. GDScript를 C#으로 기계 번역하거나 옛 팔레트를 되살리지 않는다.

현재는 **U0 엔진 전환 기초 소스**다. 플레이 가능한 게임, 새 아트, Unity 에디터 실행 성공을 주장하지 않는다.

## 열기

1. Unity Hub에서 `6000.3.24f1`을 설치하고 로그인/라이선스를 활성화한다. Android/iOS 모듈은 지금 필요하지 않다.
2. 새 폴더에 `restart/unity-6` 브랜치만 clone한다. 기존 Godot 폴더에 merge하지 않는다.
3. Hub에서 Projects > Add > Add project from disk로 이 저장소 루트 폴더를 선택한다. New Project로 덮어쓰지 않는다.
4. 패키지 복원/스크립트 컴파일을 기다린다. Unity 실행 검증은 아직 NOT_RUN이다.
5. `AFFIX > Setup > Create First Field Scene`은 빈 2D 씬을 만드는 개발 도구다. 캐릭터나 전투가 만들어진 메뉴가 아니다. 기존 씬은 덮어쓰지 않는다.

## 작성된 소스

- `Assets/_Game/Core/AttackTimeline.cs`: 공격 대상 고정, 타격 순간 1회, 취소, 회복 시간. 순수 C#.
- `Assets/_Game/Presentation/ActorAnimationSet.cs`: 실제 스프라이트/텍스처/동작 세트와 출처 검사. 타격 프레임에서 공격 시간표 생성.
- `Assets/_Game/Editor/FoundationSetup.cs`: 2D 씬 준비와 선택한 동작 세트 검사 메뉴.
- `Tests/CoreSmoke`: Unity 없는 C# 코어 실행 검사. 에디터/렌더 테스트와 별개.

`Assets/**/*.meta`를 Git에서 추적한다. Library/Temp/Logs/UserSettings만 캐시로 취급한다. 새 아트는 정확한 라이선스, 출처, 실제 프레임을 확인하기 전 자동 반입하지 않는다.

다음 단계: `docs/HANDOFF.md`, `docs/PRODUCT_BRIEF.md`, `docs/RESTART_DECISION.md`.
