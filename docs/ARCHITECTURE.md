# Unity 구현 구조 — Hero Siege 오마주 검토 씬

갱신: 2026-09-26. Unity 6000.3.24f1 / C# / Built-in 2D 유지. 사용자 제공 Stitch 7개 시안을 최우선 기준으로 삼으며, 현재는 자동 던전 순환과 장비·특성·대장간을 연결했다. 관리 UI는 네이티브 UI Toolkit이다. 펫 시안은 참조만 보관한다. 무료 Soldier·Orc, 생성 사원 방, CC0 아이콘5개와 Noto Sans KR을 사용한다. 영웅 선택·타운·모든 장비 부위까지 구현한 것은 아니다. Godot 소스는 복원하지 않았다. 이전 Ninja·초원·두 HUD 시안은 deprecated이며 사용자에게 거절됐다.

| 계층/파일 | 책임과 현재 범위 |
|---|---|
| `Core/AttackTimeline.cs` | 대상 잠금, 준비·타격1회·회복, 취소. Unity/파일 접근 없음 |
| `Core/DungeonNavigation.cs` | 통행 가능 셀, BFS 경로와 도달 가능한 대상 탐색 |
| `Presentation/DungeonWorld.cs` | 단일 마당의 28×16 그리드·장애물 표시·이동 여유와 시야 판정 |
| `Presentation/AutoHuntDirector.cs` | 세 구간·적 두 개체 재사용, 탐색/전투/수거/귀환/반복, 실패·포화 중단 |
| `Presentation/AutoHuntSmokeProbe.cs` | 실제 1배속 자동 순환·통행·시야/impact·메뉴 중 사냥·수동 pause 검사 |
| `Core/CombatHealth.cs` | HP/방어, 공격자+공격ID 중복 차단, 사망1회 |
| `Core/EncounterRewards.cs` | 원정/적/사망ID별 보상1회, overflow 검사. 저장·인벤토리 없음 |
| `Core/HeroProgression.cs` | 24칸 가방, 단일 장착 무기, 첫 확정 드랍, 누적 XP·가용 Gold, 3노드 특성과 최대 +3 확정 강화의 규칙·피해 계산 |
| `Presentation/ActorAnimationSet.cs` | Sprite 배열, 동작별 FPS, impact, 기본 방향, 출처 |
| `Presentation/MeleeActor.cs` | 자동 접근, 전투 시간표, sprite 프레임, 피격·사망 |
| `Presentation/FirstEncounter.cs` | 전투 조정자. 전투별 보상, static 세션 성장 상태, 회수·장착·특성 적용, pause·다음 전투 |
| `Presentation/EncounterHud.cs` | UI Toolkit UIDocument·PanelSettings, Stitch 기준 하단 HP/MP 구체·중앙 공격바, 상단 적 HP·탐색, 우측 미니맵, 장비 UI와 게임 상태 연결 |
| `Presentation/EquipmentPanel.cs` | 24칸 가방·장비 비교/교체·두 번 확인하는 버리기·3노드 특성 UI. 규칙은 Core에 위임 |
| `Presentation/TalentPanel.cs` | 전용 3노드 그래프·선택 상세·투자·무료 초기화. 잠긴 노드도 상세를 볼 수 있고 선택은 포인트를 쓰지 않음 |
| `Presentation/ForgePanel.cs` | 장착 무기·피해 미리보기·비용/잔액·확정 강화, 골드 부족/최대 단계 실행 제한 |
| `Resources/AffixUI/RuntimeTheme.tss` | 기본 Unity 테마를 명시적으로 불러오는 런타임 ThemeStyleSheet |
| `Editor/HeroSiegeArtSetup.cs` | 제한 원본 hash/dimension 검사 후 slice·세트·새 씬 생성. 누락 시 중단 |
| `Editor/EncounterVerification.cs` | 실제 Play·타격·사망·보상·재시작 관측, Windows build. 새 씬별 보고 필요 |
| `Presentation/PlayerSmokeProbe.cs` | 명시적 검사 플래그의 standalone 전투와 HUD framebuffer 검수 |
| `Editor/ReviewedArtSetup.cs` | deprecated Ninja/초원 생성 경로. 현행 아트 기준으로 사용하지 않음 |

## 새 씬과 제한 원본

씬은 `Assets/_Game/Scenes/HeroSiegeEncounter.unity`, 세트는 `Assets/_Game/Data/HeroSiege/{Hero,Enemy}.asset`다. 메뉴 `AFFIX/Setup/Build Hero Siege Art Review Scene`은 기존 새 씬이 있으면 내용을 덮어쓰지 않고 연다. Build Settings에 새 씬을 등록하고 이전 FirstEncounter 씬은 비활성화한다.

`Assets/LocalLicensed/Zerie/{Soldier,Orc}`의 Idle·Walk·Attack01·Hurt·Death PNG10개는 gitignored다. [zerie-local-manifest.json](assets/zerie-local-manifest.json)과 실제 SHA256을 비교하고 PNG 크기가 각각 frameCount×100 by100인지 검사한다. 누락/불일치 시 대체·빈 씬 생성 전에 실패한다. 공개 clone만으로 제한 원본이 생기지 않는다. 검증한 `Tools/Local/Import-FreeCharacters.ps1 -ZipPath <공식 무료 ZIP 경로>`가 ZIP 및 파일 hash·허용 경로·Git 제외 여부를 확인해 10 PNG만 로컬 반입한다. 이후 씬을 재생성한다.

slice는 PPU32, Soldier pivot `(0.5,0.4)`, Orc `(0.55,0.43)`다. 프레임 수6/8/6/4/4, 기본 오른쪽이고 좌우 반전만 있다. 몸 sheet에 무기·효과가 이미 있어 `attackWeapon`은 비운다. 별도 무기 합성 코드가 남아 있어도 현재 Zerie 경로는 사용하지 않는다. 이동/공격/사망10fps, 피격20fps, impact3으로 연결했다. 고유 피격 자세의 제한은 [원본 검수](assets/HERO_SIEGE_CHARACTER_CANDIDATES.md)에 있다.

## 전투와 UI 상태

공격 시작 시 대상 object·ID와 `swingDamage`를 고정하고 공격 FPS와 impact index에서 판정 시점을 계산한다. 수신 키는 `(attackerId, attackId)`이며 동일/과거 공격을 다시 적용하지 않는다. 일반 피격은 약 0.12초 flash와 피해만 반영하고 진행 공격·이동을 취소하지 않는다. 사망 때만 피격 경로에서 공격을 취소한다. 치명타 후 회복 동작과 표시 프레임을 건너뛴 impact 표시는 코어/표시 계층에서 구분한다. 장비·특성이 공격 도중 바뀌어도 진행 중인 스윙 피해는 변하지 않고 다음 스윙부터 적용한다.

`FirstEncounter`는 `ManagementScreen`의 None/Equipment/Talents/Forge 단일 상태로 관리 화면을 상호 배타 표시한다. `IsPaused = manuallyPaused`이므로 관리 화면을 열어도 사냥한다. 메뉴 전환·닫기는 수동 pause를 바꾸지 않는다. `EquipmentPanel`은 실제 24칸 가방과 단일 무기 슬롯을 표시한다. 가방 아이템을 선택하면 장착 무기와 피해 차이를 비교하고, 장착하면 이전 무기가 선택 가방 칸으로 교체된다. 버리기는 같은 아이템에 두 번 눌러 확인하며 보상 없이 제거한다. 다중 부위 장비·능동 스킬 hotbar는 아직 없다.

`FirstEncounter.Progression`은 static `HeroProgression`을 통해 같은 실행 세션에서 유지된다. 첫 적 처치 시 잿불 강철검1개가 확정 드랍되며 회수 후 가방에 들어간다. 기본 공격력24에 시작 검6이 더해지고, 새 검은 기본 보너스12+잿불4를 준다. 모든 유효 처치는 XP25·Gold8·특성 포인트1을 지급한다. 첫 드랍 외에 순환 완료마다 사원의 강철검을 자동 수거한다. `completedRuns % 3`으로 기본 보너스11/12/13·어픽스 보너스2/3/4와 이름을 순서대로 고른다. 무작위 대규모 어픽스 시스템은 아니다. 총 XP는 누적된다. `TotalGold`는 이름과 달리 총 획득액이 아니라 강화 비용을 차감한 세션 내 가용 잔액이다. 주력 루프의 `BeginAutoRun`은 전투별 보상을 새로 만들고 영웅 HP를 초기화한다. 장비·가방·특성·무기별 강화 단계·골드 잔액은 같은 세션에 유지한다. 자동사냥은 씬을 다시 로드하지 않고 순환한다. `NextEncounter`/씬 재시작은 이전 검증 경로로 남아 있으며 매 순환에 버튼을 요구하지 않는다. 프로세스 종료 후 저장·복원은 없다.

특성은 분노(Fury)2단계까지 단계당 피해+3 → 정밀(Precision)1단계 피해+4 → 숙련(Keystone)1단계 피해+6의 한 분기다. 각 단계는 포인트1과 선행 조건을 요구하며 초기화는 사용 포인트를 돌려준다. 전용 `TalentPanel`에서 노드 선택 → 오른쪽 효과/선행 조건 확인 → 투자로 진행한다. 잠긴 정밀도 선택해 상세를 볼 수 있지만 투자할 수 없고, 선택만으로 포인트가 소모되지 않는다. 정밀은 현재 고정 피해 보너스다. 영웅 선택·타운·마나·능동 스킬·영구 저장은 미구현이다.

`ForgePanel`은 현재 장착 무기 하나를 확정 강화한다. 단계당 무기 피해+2, 최대+3이며 순서대로 Gold8/16/24를 소비한다. `HeroProgression.TryEnhanceEquipped`는 비용 부족/최대 단계일 때 무기와 잔액을 바꾸지 않는다. 성공하면 같은 아이템 ID와 원래 속성을 유지한 강화 무기로 교체하며 가방과 장착 슬롯 사이 교체에도 단계가 남는다. 미리보기·실제 피해·골드 잔액은 같은 Core 상태를 읽는다. 강화도 다음 스윙부터 적용된다. 확률·파괴·소켓·룬워드·재감정·분해와 펫 기능은 미구현이다.

`EncounterHud`는 런타임 UIDocument와 PanelSettings를 생성한다. 기준 해상도1280×720, ScaleWithScreenSize와 MatchWidthOrHeight(0.5)를 사용한다. 배치는 Stitch 전투 시안의 하단 좌우 구체·중앙 공격 영역·상단 적 HP·우측 미니맵 구성을 따른다. HP·공격 경과·보상·적 위치는 현재 전투 상태에서 읽는다. 마나는 미구현으로 표시하며 임의 수치로 채우지 않고, 레벨 기준이 없는 경험치는 획득 합계만 표시한다.

현재 읽는 리소스는 `AffixGenerated/TempleRoom-v1`, `AffixGenerated/TempleObstacle-v1`, `AffixGenerated/{AttackIcon,EmberSword,PowerRune,PrecisionRune,VeteranRune}`, `AffixUI/Korean`이다. Noto Sans KR을 `FontDefinition.FromFont`로 적용한다. 한글 글꼴은 공식 정적 OTF/OFL 원본이며 자세한 기록은 `Assets/Art/Fonts/PROVENANCE.txt`에 있다. 생성 `HudFrames-v1.png`는 원본 이력으로 보존하지만 **현행 EncounterHud가 로드·slice하지 않는다**. 예전의 비정규 atlas rect를 현행 UI 계약으로 사용하지 않는다.

미니맵은 방 thumbnail 위에 실제 영웅·적 위치 marker를 표시한다. 탐색 안개·방 연결·길찾기 지도는 아니다. 장비 변경은 능력치와 UI 아이콘에 반영된다. 월드 캐릭터의 무기는 원본 몸 sprite에 구워져 있어 장착에 따라 외형이 바뀌지 않는다.

## 단일 배경과 렌더링 한계

TempleRoom은1536×1024 단일 래스터다. SpriteRenderer1개, PPU48, 월드 너비32, 카메라 orthographic size 약10.667로 배치한다. baked 횃불·그림자는 실시간 조명이 아니다. Room Visual Bounds는 표시 범위다. 별도 `DungeonWorld`가 28×16 그리드에서 네 모서리 플랫폼과 두 석재 장애물의 통행·시야를 막고 BFS로 우회한다. 석재 두 개는 (−4,0), (4,0)에 있으며 runtime sprite 폭2.4·높이1.8, BoxCollider2D 2×2다. 이동의 기준은 물리 엔진 경로가 아닌 그리드/여유 검사다. navmesh·벽 가림 처리는 없다. 생성 배경을 완성된 타일맵·모듈형 던전으로 설명하지 않는다.

카메라는 `allowMSAA=false`, sprite는 Point/mipmap off/uncompressed/Clamp다. 엔진·렌더러·모듈은 변경하지 않았다. 예전 Ninja atlas의 MSAA 문제 해결은 과거 근거이며 새 PNG의 시각 품질 보증이 아니다.

## 자동 순환과 안전 중단

현재는 단일 사원 마당 안의 세 구간을 자동으로 순회한다. 적 두 개체를 구간마다 재사용하고, BFS 경로 탐색으로 통행 불가 모서리와 석재 장애물을 피한다. 탐색 → 대상 선택/접근 → 자동 공격 → 자동 수거 → 다음 구간 → 입구 귀환/반복을 연결했다. 세 개의 별도 방이나 절차 생성 던전은 아니다.

일반 피격은 체력과 짧은 flash만 반영하며 공격·이동을 취소하지 않는다. 사망할 때 공격을 취소한다. 연속 두 번 사망하면 자동 중단하고, 가방 포화 시 보류 전리품을 보존한 채 멈춘다. 장비·특성·대장간을 열어도 사냥하며 수동 정지만 전투를 멈춘다. 연속 사망·포화 안전 검사는 PASS다. 별도 경로 막힘과 중단 후 재개 검증은 남아 있다.

## 실제 검증 범위

최종 Windows 자동사냥 실행은 [720p 보고](validation/autohunt/player-720.json)와 [1080p 보고](validation/autohunt/player-1080.json) 모두 PASS다. 실행 ID는 각각 `26b0296dea004e06a40c4a3cfab8caa8`(70.9288초), `23dbebd8a24b4397bca82dce83b771bd`(70.6474초), Build GUID는 `6c03e8d2bc6e457abc31582911701d3d`다. 두 실행 모두 실제 1배속으로 3구간·3순환·18처치·4개 수거, 사망 0회·재시도 0회, XP 450·골드 144를 확인했다. 영웅 HP 120/공격력 30·적 HP 54/공격력 6을 바꾸지 않았다. 메뉴 중 사냥과 수동 정지/재개, UI 콜백 7회·실제 framebuffer 6개도 검사했다. [720p 전투](media/autohunt/720-combat.png) · [1080p 관리 중 사냥](media/autohunt/1080-management.png). 로컬 원본은 `Build/Reports/autohunt-diagnostic-{720,1080}/auto-hunt-smoke.json`이다.

최종 빌드는 `Build/Reports/windows-build.json`의 2026-09-26T06:36:34.0607289Z Succeeded, 오류 0/경고 0이다. 정상 두 해상도는 위 최종 빌드의 근거다. 안전 중단은 아래 명시한 직전 빌드에서 검사했으며 safety-build/source-fingerprint 보고서로 구분한다. 앞선 `autohunt-fixed-720` 후보 실행은 이력으로 구분한다. 이전 `docs/validation/management/`는 관리 기능 시제품의 역사적 근거다. 디스크 저장, 20분 연속 실행, OS 물리 입력 검사, 사용자 시각 승인은 완료되지 않았다.

.NET CI는 Core 검사이며 Unity import·실행과 다르다. 정상 실행과 안전 조건 주입 검사, OS 물리 입력, 사용자 시각 승인을 서로 대체하지 않는다. 이전 management 보고의 메뉴 pause는 당시 동작이며 현재의 메뉴 중 사냥 정책으로 바뀌었다.


안전 검사의 빌드 GUID는 `0a94c21a725b4d3a9a01e269067fa423`다. 이후 변경은 관리 창 위 피해 숫자 숨김과 검사 보고의 최종 성장 수치 추가이며 전투·안전 중단 코드는 동일하다. [연속 사망 안전 검사](validation/autohunt/safety-deaths.json)는 run `f508cbfae00844128c310426a8abbbc3`, 약 29.4초에 실제 적 공격으로 두 번 사망·한 번 재시도 후 자동 중단, 4.01초 중단 유지 PASS다. 이 격리 검사만 영웅 공격력을 1로 주입했으며 정상 밸런스 증거가 아니다. [가방 포화 안전 검사](validation/autohunt/safety-bag-full.json)는 run `a8fd4677ac5741bb90de2caeccef30fa`, 시험용 무기 24개로 가방을 채운 뒤 실제 첫 처치의 전리품을 보류 상태로 보존하고 중단했다. 약 7.65초, 골드 8·XP 25·기존 아이템 24개 유지, 중단 4.01초 PASS다. 두 검사는 중단 후 재개나 20분 안정성을 증명하지 않는다.
