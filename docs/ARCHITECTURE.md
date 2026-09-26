# Unity 구현 구조 — Hero Siege 오마주 검토 씬

갱신: 2026-09-26. Unity 6000.3.24f1 / C# / Built-in 2D 유지. 사용자 제공 Stitch 4개 시안을 최우선 기준으로 삼으며, 현재는 그중 전투 화면을 네이티브 UI Toolkit으로 구현한 첫 단계다. 무료 Soldier·Orc, 생성 사원 방, CC0 공격 아이콘과 Noto Sans KR을 사용한다. 영웅 선택·타운·완전한 장비/특성 화면까지 구현한 것은 아니다. Godot 소스는 복원하지 않았다. 이전 Ninja·초원·두 HUD 시안은 deprecated이며 사용자에게 거절됐다.

| 계층/파일 | 책임과 현재 범위 |
|---|---|
| `Core/AttackTimeline.cs` | 대상 잠금, 준비·타격1회·회복, 취소. Unity/파일 접근 없음 |
| `Core/CombatHealth.cs` | HP/방어, 공격자+공격ID 중복 차단, 사망1회 |
| `Core/EncounterRewards.cs` | 원정/적/사망ID별 보상1회, overflow 검사. 저장·인벤토리 없음 |
| `Presentation/ActorAnimationSet.cs` | Sprite 배열, 동작별 FPS, impact, 기본 방향, 출처 |
| `Presentation/MeleeActor.cs` | 자동 접근, 전투 시간표, sprite 프레임, 피격·사망 |
| `Presentation/FirstEncounter.cs` | 새 씬에서도 사용하는 전투 조정자. XP25/Gold8 자동수령, pause·캐릭터 창 상태, reload |
| `Presentation/EncounterHud.cs` | UI Toolkit UIDocument·PanelSettings, Stitch 기준 하단 HP/MP 구체·중앙 공격바, 상단 적 HP·탐색, 우측 미니맵, 읽기 전용 캐릭터 창 |
| `Editor/HeroSiegeArtSetup.cs` | 제한 원본 hash/dimension 검사 후 slice·세트·새 씬 생성. 누락 시 중단 |
| `Editor/EncounterVerification.cs` | 실제 Play·타격·사망·보상·재시작 관측, Windows build. 새 씬별 보고 필요 |
| `Presentation/PlayerSmokeProbe.cs` | 명시적 검사 플래그의 standalone 전투와 HUD framebuffer 검수 |
| `Editor/ReviewedArtSetup.cs` | deprecated Ninja/초원 생성 경로. 현행 아트 기준으로 사용하지 않음 |

## 새 씬과 제한 원본

씬은 `Assets/_Game/Scenes/HeroSiegeEncounter.unity`, 세트는 `Assets/_Game/Data/HeroSiege/{Hero,Enemy}.asset`다. 메뉴 `AFFIX/Setup/Build Hero Siege Art Review Scene`은 기존 새 씬이 있으면 내용을 덮어쓰지 않고 연다. Build Settings에 새 씬을 등록하고 이전 FirstEncounter 씬은 비활성화한다.

`Assets/LocalLicensed/Zerie/{Soldier,Orc}`의 Idle·Walk·Attack01·Hurt·Death PNG10개는 gitignored다. [zerie-local-manifest.json](assets/zerie-local-manifest.json)과 실제 SHA256을 비교하고 PNG 크기가 각각 frameCount×100 by100인지 검사한다. 누락/불일치 시 대체·빈 씬 생성 전에 실패한다. 공개 clone만으로 제한 원본이 생기지 않는다. 검증한 `Tools/Local/Import-FreeCharacters.ps1 -ZipPath <공식 무료 ZIP 경로>`가 ZIP 및 파일 hash·허용 경로·Git 제외 여부를 확인해 10 PNG만 로컬 반입한다. 이후 씬을 재생성한다.

slice는 PPU32, Soldier pivot `(0.5,0.4)`, Orc `(0.55,0.43)`다. 프레임 수6/8/6/4/4, 기본 오른쪽이고 좌우 반전만 있다. 몸 sheet에 무기·효과가 이미 있어 `attackWeapon`은 비운다. 별도 무기 합성 코드가 남아 있어도 현재 Zerie 경로는 사용하지 않는다. 이동/공격/사망10fps, 피격20fps, impact3으로 연결했다. 고유 피격 자세의 제한은 [원본 검수](assets/HERO_SIEGE_CHARACTER_CANDIDATES.md)에 있다.

## 전투와 UI 상태

공격 시작 시 대상 object·ID를 고정하고 공격 FPS와 impact index에서 판정 시점을 계산한다. 수신 키는 `(attackerId, attackId)`이며 동일/과거 공격을 다시 적용하지 않는다. 피격은 진행 공격을 취소한다. 치명타 후 회복 동작과 표시 프레임을 건너뛴 impact 표시는 코어/표시 계층에서 구분한다. 새 아트의 판정·화면 일치는 새 Play 결과로 확인해야 한다.

`FirstEncounter`는 `Time.timeScale`로 수동 pause와 캐릭터 창 pause를 관리하고 닫기·파괴 시 기존 속도를 복원한다. 캐릭터 창과 무기 tooltip은 읽기 전용이다. 장비 드랍·비교·장착·공격 변화·스킬 배분·영구 저장은 구현 완료가 아니다. 기본 공격 아이콘1개를 완성된 다중 skill hotbar로 설명하지 않는다.

`EncounterHud`는 런타임 UIDocument와 PanelSettings를 생성한다. 기준 해상도1280×720, ScaleWithScreenSize와 MatchWidthOrHeight(0.5)를 사용한다. 배치는 Stitch 전투 시안의 하단 좌우 구체·중앙 공격 영역·상단 적 HP·우측 미니맵 구성을 따른다. HP·공격 경과·보상·적 위치는 현재 전투 상태에서 읽는다. 마나는 미구현으로 표시하며 임의 수치로 채우지 않고, 레벨 기준이 없는 경험치는 획득 합계만 표시한다.

현재 읽는 리소스는 `AffixGenerated/TempleRoom-v1`, `AffixGenerated/AttackIcon`, `AffixUI/Korean`이다. Noto Sans KR을 `FontDefinition.FromFont`로 적용한다. 한글 글꼴은 공식 정적 OTF/OFL 원본이며 자세한 기록은 `Assets/Art/Fonts/PROVENANCE.txt`에 있다. 생성 `HudFrames-v1.png`는 원본 이력으로 보존하지만 **현행 EncounterHud가 로드·slice하지 않는다**. 예전의 비정규 atlas rect를 현행 UI 계약으로 사용하지 않는다.

미니맵은 방 thumbnail 위에 실제 영웅·적 위치 marker를 표시한다. 탐색 안개·방 연결·길찾기 지도는 아니다. 캐릭터/장비 창은 현재 능력치를 읽는 보조 열람창이며 Stitch의 완전한 인벤토리·장착·특성 화면은 후속 범위다.

## 단일 배경과 렌더링 한계

TempleRoom은1536×1024 단일 래스터다. SpriteRenderer1개, PPU48, 월드 너비32, 카메라 orthographic size 약10.667로 배치한다. baked 횃불·그림자는 실시간 조명이 아니다. Room Visual Bounds는 표시 범위이며 충돌체·navmesh·벽 가림 처리가 아니다. 생성 배경을 완성된 타일맵·모듈형 던전으로 설명하지 않는다.

카메라는 `allowMSAA=false`, sprite는 Point/mipmap off/uncompressed/Clamp다. 엔진·렌더러·모듈은 변경하지 않았다. 예전 Ninja atlas의 MSAA 문제 해결은 과거 근거이며 새 PNG의 시각 품질 보증이 아니다.

## 실제 검증 범위

새 `HeroSiegeArtSetup.Build`와 씬 생성은 `Build/Reports/unity-generated-art-setup.log` 종료코드0을 확인했다.

Windows build는 `Build/Reports/windows-build.json`의 2026-09-26T04:55:46Z Succeeded, 오류0/경고0이다. 실제 UI Toolkit HUD를 포함한 Windows 실행은 buildGuid=`54d35624853d494cbee6127fe860214e`, 1280×720 run=`15048b15916b455daa9126e3372c7ac1`과 1920×1080 run=`c689d0cd4e0041bd8c5217777dae77b3` 모두 PASS다. 각 보고는 `Build/Reports/stitch-720/player-smoke.json`, `stitch-1080/player-smoke.json`에 있다. 전투·양측 피해·적 사망·단발 보상·pause/열람 상태·재시작과 native HUD 상태/캡처5개를 확인했다. UI와 공유하는 API를 호출한 검사이며 **실제 클릭 NOT_RUN, 사용자 시각 승인 NOT_APPROVED**다. 04:38:29Z의 생성 아트 씬 Play PASS는 이전 월드 검증이며 Stitch UI 증거와 분리한다.

.NET CI는 Core만 검사한다. 설치 API 정적 컴파일, Unity import, Play, build, 실제 실행, 사용자 시각 승인을 구분한다. Editor Camera.Render의 월드 전용 캡처와 standalone의 실제 UI Toolkit HUD 포함 framebuffer 캡처는 서로 다르다. 자동 상태 관측을 클릭 검증이나 사용자 체감 승인으로 표시하지 않는다.
