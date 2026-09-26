# 현행 에셋 반입 — 무료 원본과 직접 생성 아트

갱신: 2026-09-26. 최신 사용자 정책은 **유료 에셋 구매 제외, 무료 에셋 + 직접 제작, Hero Siege 오마주**다. 탕탕특공대 요소, Ninja Adventure 캐릭터·UI, 밝은 초원 고정은 deprecated다. 이전 팩의 기술 검증을 현재 화면의 채택·시각 승인으로 이어받지 않는다.

사용자 제공 **Stitch 4개 시안이 UI 배치의 최우선 기준**이다. 현재는 전투와 단일 무기·24칸 가방·3노드 특성의 UI Toolkit 구현이다. 영웅 선택·타운·다중 장비 부위·마나·능동 스킬·영구 저장은 완료되지 않았다.

## 현재 씬에 연결한 자산

| 자산 | 현재 파일과 근거 | 권리·실제 구성 |
|---|---|---|
| Soldier / Orc | `Assets/LocalLicensed/Zerie/{Soldier,Orc}/`의 5클립씩 10 PNG. [해시 manifest](assets/zerie-local-manifest.json), [원본 검수](assets/HERO_SIEGE_CHARACTER_CANDIDATES.md) | Zerie 공식 무료 V2.0. 게임 사용 허용, raw 원본·수정본 공개 재배포 금지. 폴더와 .meta는 gitignored |
| TempleRoom-v1 | `Assets/Art/Generated/Resources/AffixGenerated/TempleRoom-v1.png`, 1536×1024 | 사용자 직접 제작 지시에 따라 image_gen으로 생성한 단일 방 배경 |
| 기본 공격 아이콘 | `Assets/Art/Lucifer/Resources/AffixGenerated/AttackIcon.png`, 16×16 | Foozle Lucifer RPG UI 검 아이콘 원본 1개. CC0 동봉문서·출처와 함께 반입. Lucifer 폰트는 미반입 |
| 전리품·특성 아이콘 | 같은 Lucifer 리소스 폴더의 `EmberSword`, `PowerRune`, `PrecisionRune`, `VeteranRune` PNG | 추가4개 모두16×16 원본 PNG, CC0. 각각 잿불 강철검·분노·정밀·숙련에 연결. 원본 경로·SHA256은 `Assets/Art/Lucifer/PROVENANCE.txt` |
| 한글 폰트 | `Assets/Art/Fonts/Resources/AffixUI/Korean.otf`, 4,644,748B | 공식 Noto Sans KR Regular2.004 정적 OTF. OFL1.1 원문·저작권·출처와 함께 공개 반입 |

생성 프롬프트·출처·크기·한계는 [generated-art-manifest.json](assets/generated-art-manifest.json)에 있다. 생성 이미지를 CC0 외부 팩이나 사람이 픽셀마다 그린 이미지라고 표시하지 않는다. Lucifer 아이콘은 기존 공격1개와 추가 전리품·특성4개, 총5개다. 추가 아이콘은 직접 원본을 검수하고 수정 없이 복사했다. 아이템 희귀도와 특성 의미는 게임에서 부여한 값이며 원본 파일의 고유 기능이 아니다. Lucifer 공격 아이콘 원본 경로는 `Skill Icons/Warrior Icons/Warrior Green Skills/Png/Warrior Icon 01.png`, SHA256은 `33dac9cf80969fdc25f938cc4fcace2f97d66490f3f73bf401df1171fdffdafb`다. `Assets/Art/Lucifer/LICENSE.txt`와 `PROVENANCE.txt`는 공식 배포자 및 크레딧 표기 차이를 보존한다. 유료 전체 팩은 구매하지 않았다. Noto는 `Assets/Art/Fonts/LICENSE.txt`와 `PROVENANCE.txt`에 고정 공식 revision·해시·공개 재배포 조건을 보존했다. 원본 바이트를 변환하지 않았고 한글 완성형11,172자 전체를 확인했다.

## 캐릭터의 정확한 계약

Zerie 무료 ZIP SHA256: `9f1818c7ddc17b99e4bae81feae8b25a3692bf6bc269bb2797e36bd7faec3ba2`. ZIP·추출물은 `Build/Downloads/HeroSiegeCandidates` 아래 로컬 검수용이다. 선택 파일은 Idle6, Walk8, Attack01 6, Hurt4, Death4 프레임이며 **100×100 cell** 한 행이다. 실제 몸은 cell보다 작고 공격 여백이 넓다. 10개 로컬 PNG의 byte size·SHA256은 manifest에서 확인한다.

오른쪽 원본과 좌우 반전으로 사용하며 상하 방향 그림은 없다. 장착 무기는 실제 피해와 UI 아이콘에 반영되지만 월드의 무기 외형은 원본 sprite에 포함되어 바뀌지 않는다. 무기·베기 효과가 몸 sheet에 포함돼 별도 Ninja 무기 sheet를 연결하지 않는다. Hurt는 idle과 다른 피격 자세 하나에 색상 flash를 주는 구성이다. 4프레임의 RGBA 고유 이미지는3개지만 alpha 실루엣은1개다. 프레임 수를 몸동작 수로 과장하지 않는다. Soldier split sheet의 shadow/no-shadow 폴더 파일이 동일한 원본 문제도 후보 문서에 기록했다.

현재 importer는 PPU32, Soldier pivot `(0.5,0.4)`, Orc `(0.55,0.43)`, Point·mipmap off·uncompressed·NPOT 크기 유지다. Idle/Walk/Attack/Death10fps, Hurt20fps, impact index3으로 연결했다. 원본 Aseprite의 일반100ms·마지막 death600ms와 현재 런타임 선택을 구분한다. Hurt 속도는 연속 경직을 피하기 위한 구현 선택이며 사용자 체감 검증을 대신하지 않는다.

## 공개 저장소와 로컬 원본

무료 다운로드와 공개 GitHub raw 재배포 권리는 별개다. Zerie 원본·.meta는 `Assets/LocalLicensed/`와 함께 Git에서 제외한다. 공개 저장소에는 허용된 코드·해시·공식 입수 링크·반입 절차를 둔다. 검증한 `Tools/Local/Import-FreeCharacters.ps1`은 공식 무료 ZIP hash·파일 hash·허용 경로·Git 제외 여부를 검사한 뒤 10개 PNG를 로컬 반입한다. 새 체크아웃에서 원본이 없거나 해시가 다르면 `HeroSiegeArtSetup`은 씬 생성 전에 중단하며 Ninja나 빈 씬으로 대체하지 않는다.

허용된 Lucifer CC0 원본, Noto OFL 폰트와 생성 아트에는 .meta를 추적한다. 공개 전 staged 파일에 제한 원본·ZIP이 포함됐는지 확인한다. 유료 구매나 라이선스 제한 해제는 이번 정책에 없다.

## 생성 결과의 한계

TempleRoom은 **그림 한 장**이다. 벽·횃불 빛·그림자가 이미지에 구워져 있으며 타일셋, 실시간 광원, 충돌체, 내비게이션, 벽 가림 처리가 구현된 것은 아니다. Room Visual Bounds도 표시 범위 marker다. Built-in 2D를 유지했고 URP·엔진·모듈 변경은 하지 않았다.

생성 `HudFrames-v1.png`(1774×887)는 요청한 정규 grid와 달라 과거 구현에서 개별 rect를 사용했다. 현재는 원본 이력으로만 보존하며 **UI Toolkit EncounterHud는 이 atlas를 사용하지 않는다**. 현행 프레임·구체·장비/특성 배치는 `EncounterHud`·`EquipmentPanel`의 UI Toolkit 요소로 구현했고, `RuntimeTheme.tss`는 Unity 기본 테마를 명시적으로 불러온다. TempleRoom·Lucifer 아이콘·Noto 폰트만 현재 HUD 리소스로 읽는다. 생성 atlas의 rect 검수를 현재 Stitch UI 검증이라고 설명하지 않는다.

## 검증 상태

`HeroSiegeArtSetup.Build`와 새 씬 생성은 실제 Unity 종료코드0으로 확인했다.

Windows build는 `Build/Reports/windows-build.json`의 2026-09-26T05:22:48Z Succeeded, 오류0/경고0이다. buildGuid=`e86a4c30328a47ec9bf8cf3ba918d0f8`, 1280×720 run=`e3f3eec47a4b4751b59f5dc715697a93`과 1920×1080 run=`4b47acfcaa8948a8a003eca4e936c4db` 모두 PASS다. 보존 보고는 [player-720.json](validation/progression/player-720.json), [player-1080.json](validation/progression/player-1080.json), [windows-build.json](validation/progression/windows-build.json)에 있다. 로컬 원본은 `Build/Reports/progression-{720,1080}/player-smoke.json`이다.

각 실행에서 실제 전투·pause·전리품 회수·무기 교체·특성 초기화·다음 전투의 성장 유지와 강화 후 실타격41을 확인했다. 각 9개 HUD framebuffer 캡처와 UI Toolkit `ClickEvent` 콜백9회를 검사했다. pause는 공유 API 호출, 장비·특성·다음 전투는 UI 이벤트 전달로 검사했다. **OS 물리 마우스/키보드 입력 NOT_RUN, 사용자 시각 승인 NOT_APPROVED**다. 코어 검사는90개 통과했으며 Unity 표시 검증과 구분한다. 04:38:29Z의 생성 아트 Play와 04:55:46Z의 첫 native HUD 결과는 이전 범위의 기록이다.

`assets/ninja-*`, `Tools/validate_reviewed_art.py`, 기존 원본 재생 HTML은 **deprecated 기술 시제품의 이력**이다. 그 검사기의 PASS는 새 Zerie·생성 아트·Lucifer 검사가 아니다. Tiny Swords/Pixel Crawler 미채택 사유도 이력·후보 문서로만 유지한다.
