# Unity 구현 구조 v0.2

엔진: Unity 6000.3.24f1, C#, Built-in 2D. 현재 범위는 첫 1대1 전투와 라이선스 리소스 연결이다. 기존 Godot 소스를 변환한 구조가 아니다.

| 계층/파일 | 책임 | 하지 않는 일 |
| --- | --- | --- |
| Core/AttackTimeline.cs | 대상 잠금, 공격 경과, 타격1회, 취소, 회복 | Sprite/파일/Unity 콜백 접근 |
| Core/CombatHealth.cs | HP/방어, 공격자+공격ID 중복 차단, 사망1회 | UI·보상·저장 |
| Core/EncounterRewards.cs | 원정/적/사망ID 단위 자동수령1회, 합산 전 overflow 검사 | 영구 저장·인벤토리 |
| Presentation/ActorAnimationSet.cs | 실제 Sprite 배열, FPS, impact index, 기본 방향, 출처 정보 | 라이선스 자동 승인·아트 품질 판정 |
| Presentation/MeleeActor.cs | 접근, 타임라인 실행, SpriteRenderer 프레임, 피격·사망 | 옛 자산 로드·개인 세이브 |
| Presentation/FirstEncounter.cs | 밝은 HP/HUD, 사망 이벤트의 XP25/Gold8 자동수령, 재시작 | 접촉 드랍·저장·온라인 상태 |
| Editor/ProjectDashboard.cs | 버전/경로 보고, 에셋 준비 확인, 씬 생성 | 사용자 폴더 삭제·에셋 다운로드 |
| Editor/ReviewedArtSetup.cs | 검수한 CC0 원본 slice·Hero/Enemy 세트·초원 씬 생성 | 원본 수정·기존 씬 덮어쓰기 |
| Editor/EncounterVerification.cs | 실제 Play/사망/단발 보상/씬 reload 관측, Windows build | 사용자 시각 승인 대행 |

## 공격 처리

공격 준비 시 target object와 ID를 잠근다. attackFps와 impactFrame에서 실제 타격 시간을 계산하며 몸과 별도 무기 Sprite 배열도 같은 시간표를 사용한다. Receive의 key는 `(attackerId, attackId)`다. 동일·과거 attack ID를 다시 받으면 HP 변화가 없으며 사망한 대상에게 중복 사망이 생기지 않는다. 치명타 뒤에는 피해 없이 recovery를 끝까지 재생한다. 원정마다 새 CombatHealth를 생성하고 actor ID는 그 원정 안에서 고유해야 한다.

Unity 표시 프레임을 건너뛴 타격은 impact Sprite를 명시적으로 표시한다. 이것은 소스상의 처리 규칙이고 화면에서 실제로 읽히는지는 별도 Play 검증 사항이다. 피격은 진행 중 공격을 취소하고 hit 프레임을 재생한다. 연속 피격으로 공격을 못 하는 밸런스는 실제 검수 뒤 조정하며 여기서 완성이라고 간주하지 않는다.

## 씬 생성

Project Dashboard에서 영웅·적 ActorAnimationSet, 바닥 Sprite, 출처/라이선스 검수 확인이 모두 있어야 FirstEncounter 생성 버튼이 활성화된다. 기존 FirstEncounter.unity가 있으면 덮어쓰지 않고 연다. 씬 생성은 사용자 에디터에서 수행되며 새 scene/.meta/BuildSettings 변경이 로컬에 생긴다. 이 변경은 이후 작업에서 의도적으로 반영하며 자동 discard하지 않는다.

원본 아트와 초기 GUID .meta는 저장소에 있다. Unity 실행이 license198로 import 전에 중단되어 실제 slice/.asset/.unity 생성은 대기 중이다. 설치 완료 후 ReviewedArtSetup.Build로 실행한다. HTML 프레임 미리보기는 Unity 씬을 대체하지 않는다. 2D 타일맵 전체 편집·소규모 군중 분리·AI 행동 다양화는 후속 범위다.

## 현재 한계

.NET CI는 Core만 컴파일한다. 로컬 전체 C#은 설치된 Unity API DLL 참조로 정적 컴파일을 통과했지만 실제 asmdef/패키지/import/Play는 미검증이다. 런타임 rect/texture 검사는 반복 참조만 잡으며, 신규 원본의 픽셀 내용과 SHA는 별도 Python 검사기로 확인한다. 라이선스 문자열은 실제 권리 검수를 대신하지 않는다. 실제 화면과 사용자 시각 검증은 남아 있다.

참조: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SpriteRenderer.html
