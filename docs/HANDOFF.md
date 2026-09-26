# 현재 인수인계 | 신규 CC0 아트와 U1 실행 준비

갱신: 2026-09-26. 저장소 Dev-Gony/affix-zero, 브랜치 restart/unity-6, PR #19 Draft. 구현 기준 커밋 dbe8d76e286cdecfcf302dd42a9672a40fb87b9a. 이후 도구/문서 커밋은 최신 HEAD를 확인한다.

## 사용자와 환경

사용자는 현재 Unity Hub에서 에디터 설치 중이며 완료 후 알려주겠다고 했다. 그때까지 추가 Unity 실행을 보류한다. 이미 존재하는 D:\Program Files\Unity 6000.3.24f1\Editor\Unity.exe의 ProductVersion은 6000.3.24f1_4e7b9b5b6244로 프로젝트와 일치했다. 1회 batch 실행은 import 전에 라이선스 부재로 종료 코드 198을 반환했다. 설치·라이선스 문제를 해결됐다고 추정하지 않는다. 재설치나 옛 Godot 폴더 동기화를 요구하지 않는다.

## 실제 반영한 것

- 새 Ninja Adventure 제작자 원본과 CC0 전문 확인, 최소 11 PNG와 라이선스/README 반입. SHA·프레임·방향·pivot·impact는 docs/assets에 기록. Tiny Swords CC0 구버전은 Hit 결손으로 미채택.
- NinjaGreen의 실제 idle/walk/attack/hit/death와 Katana/Axe 공격 프레임을 연결하는 ReviewedArtSetup. 영웅과 첫 적은 같은 신규 캐릭터를 공유하며 적은 진영 tint와 Axe로 구분한다. 별도 몬스터 아트를 완성한 것이 아니다.
- 몸·무기의 공통 공격 시간표, 공격 대상 object 잠금, 치명타 뒤 회복 동작, 취소/중복 피해 차단.
- 밝은 초원·흙길·연못·나무 씬 생성기, 아이보리/초록 HUD, 자동 접근·공격, 처치 후 XP25/Gold8 자동수령 1회, 재시작 초기화. 접촉 드랍·영구 저장은 없다.
- 실제 Play/피해/사망/보상/씬 재시작을 검사하는 EncounterVerification과 Windows 빌드 진입점. 도구 작성과 실행 성공은 별개.
- Tools/validate_reviewed_art.py와 preview_reviewed_art.py: 원본 SHA/실프레임 검사, 원본 재생 HTML 생성. HTML은 Unity 게임 실행화면이 아니다.

## 실행한 검사와 한계

코어 56 checks PASS. 설치된 Unity DLL을 참조한 전체 C# 정적 컴파일 오류0/경고0. 현재 트리 감사 PASS, 이전 엔진 잔존 파일0. 정적 컴파일은 Unity asmdef/패키지/import/Play 검증을 대신하지 않는다.

수치 시뮬에서 2프레임 피격을 6fps로 재생하면 영웅이 계속 경직됨을 확인해 reactionFps10으로 조정했다. hero attack8fps/enemy10fps, impact index1, 이동8fps. 시뮬은 실제 Unity Play 결과가 아니다.

**Unity import/씬 생성/Play/Windows build/사용자 시각 승인은 미완료.** 현재 PNG .meta는 GUID를 보존하는 초기 설정이며 실제 slice와 .asset/.unity는 승인 버전 에디터에서 생성한다. 빈 씬을 완성 게임으로 보고하지 않는다.

## 설치 완료 후 순서

1. 완료 안내 후 프로젝트를 사용하는 다른 Unity 프로세스가 없는지 확인하고 현행 버전으로 실행. 라이선스 오류가 지속될 때만 Hub 로그인/라이선스 활성화를 요청한다.
2. AffixZero.Editor.ReviewedArtSetup.Build를 -batchmode -quit -projectPath와 함께 실행하거나 에디터 AFFIX → Setup → Import Reviewed Art and Create Encounter를 사용. 기존 씬은 덮어쓰지 않는다.
3. AffixZero.Editor.EncounterVerification.Run은 -batchmode와 **-quit 없이** 실행. 실제 Play와 domain reload를 기다려 Build/Reports/encounter-verification.json을 생성. PNG는 Camera.Render 월드만 포함하며 HUD는 별도 확인.
4. Play 통과 후 AffixZero.Editor.EncounterVerification.BuildWindows 실행. 생성된 scene/data/.meta/ProjectSettings를 검토·추적. Windows 실행과 사용자 시각 승인은 따로 확인.

U2 웨이브·펫·가챠·시즌은 U1 승인 전 확장하지 않는다. PR 병합, 옛 사용자 폴더/세이브/stash/Git history 삭제는 수행하지 않았다.

## 진행 상황 체크포인트

1. 완료: 신규 원본 검수, U1 연결 소스, 코어/정적 검사, GitHub 구현 업데이트.
2. 준비: 실제 씬 생성·Play·빌드 도구와 원본 애니메이션 미리보기.
3. 대기: 설치 완료 안내 후 실제 Unity 검증과 시각 승인.