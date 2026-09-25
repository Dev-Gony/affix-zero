# RESTART-001 | Godot 종료, Unity에서 새 시작

결정일: 2026-09-26. 사용자 명시 요청: 기존 코드 전부 삭제하고 새로 시작, Hero Siege/Survivor.io에서 사용한 계열의 엔진 검토.

## 결정

Unity 6.3 LTS 6000.3.24f1 / C# / 첫 2D 화면은 Built-in 렌더러 / Windows 우선. Unity 지원 문서의 6.3 LTS와 정식 6000.3.24f1 릴리스를 확인했다. 처음부터 URP/온라인/모바일 SDK/전역 매니저를 추가하지 않는다. 렌더 파이프라인은 실제 새 에셋 요구가 생길 때 검토한다.

기존 트리를 상속하지 않는 새 tree를 생성하되 기존 HEAD를 정상 Git 부모로 연결한다. 새 개발선에서는 기존 코드와 이미지가 사라지고 역사에만 남는다. history rewrite와 사용자 로컬 삭제는 하지 않는다. 새 브랜치 restart/unity-6, 새 로컬 폴더 권장명 affix-unity. chore/r0-preservation이나 기존 두 Godot worktree로 다시 merge하지 않는다.

## 엔진 사실과 판단의 구분

- Hero Siege: Panic Art Studios의 자체 itch.io 배포 페이지가 Made with GameMaker를 명시한다. 확인된 1차 근거다.
- Survivor.io: 검색에서 개발사 관련 Unity 채용 공고를 확인했지만, 원작 특정 빌드의 엔진/버전을 직접 선언한 개발사 기술문서는 확보하지 못했다. 복제 게임 판매 페이지를 원작 엔진 근거로 사용하지 않는다.
- Unity 선택은 AFFIX의 2D 애니메이션/에셋 작업과 C# 데이터 설계, 향후 확장 요구에 대한 설계 판단이다. 두 게임의 실제 내부 구조를 복제한다는 뜻이 아니다.
- 이번 project.godot merge 오류는 로컬 수정과 원격 변경이 충돌하여 Git이 중단한 사건이다. 이것만으로 Godot 엔진 결함이나 바이러스 감염을 진단할 수 없다.
- 앞선 벤치마크는 매 프레임 누적 샘플을 정렬하고 긴 지연 샘플을 걸러내는 문제가 있었다. 그 결과로 엔진 적합성을 확정하지 않는다. 해당 코드도 재사용하지 않는다.

## 출처 (확인일 2026-09-26)

- Hero Siege 개발사 배포: https://panicartstudios.itch.io/hero-siege
- 개발사 관련 채용 정보, 원작 엔진 확정 자료와 구분: https://www.magesbox.com/recruit/job/id/2076.html
- Unity 6 지원: https://unity.com/releases/unity-6/support
- 정확한 Editor 릴리스/changeset: https://unity.com/releases/editor/whats-new/6000.3.24f1
- Unity 2D 제작: https://docs.unity3d.com/6000.3/Documentation/Manual/2d-game-creation-wokflow.html
- Input System 1.20.0: https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.inputsystem.html

## 이번 검증 경계

Unity 설치/로그인/라이선스와 에디터 실제 import가 이 작업 환경에 없으므로 Unity 실행/렌더/빌드/시각 검증은 NOT_RUN이다. CI는 새 파일 트리 및 순수 C# 공격 시간표만 검사한다. 설치 기반을 만들었다는 것과 게임을 완성했다는 것은 다르다.
