# 생성 장애물 — TempleObstacle v1

제작/검수: 2026-09-26. 사용자 정책인 무료 에셋 + 직접 제작에 따라 built-in `image_gen`과 `imagegen` 스킬을 사용했다. 외부 팩·원격 이미지·과거 Godot/Ninja 아트는 사용하지 않았다. 사전 시각 기준은 현행 생성 `TempleRoom-v1.png`를 직접 읽어 확인한 갈색 석재·짧은 남쪽 면·따뜻한 모서리 강조다. 방 이미지를 수정하거나 일부를 잘라내지 않고 새로운 장애물 한 개를 생성했다.

## 원본 식별

| 항목 | 값 |
|---|---|
| 프로젝트 파일 | `Assets/Art/Generated/Resources/AffixGenerated/TempleObstacle-v1.png` |
| 런타임 리소스 | `AffixGenerated/TempleObstacle-v1` |
| 도구 원본 | `C:/Users/Gwony/.codex/generated_images/01a0dbbc-8bc8-7343-92dc-35fb4f16abae/exec-a6515c24-59ac-4e8f-8721-3610cd5a7ae4.png` |
| 형식 / 크기 | PNG RGBA, 1448×1086, 964327 bytes |
| SHA-256 | `8c443612cff02699e3d1398fbe52ef3574e90560945287f8da746bfdd88dabd1` |
| 반입 방식 | 생성 PNG를 바이트 그대로 복사, 자르기·리사이즈·재색칠·알파 수정 없음 |
| 권리/출처 표기 | 직접 AI 생성 산출물. 외부 CC0 팩 또는 사람이 픽셀마다 그린 원본이라고 표시하지 않음 |

기존 방은 스타일을 판단하기 위해 도구 호출 전에 확인했으며 생성 호출에는 방의 파일 경로나 편집 대상을 전달하지 않았다. 설명 프롬프트로 새로운 석재 sprite를 제작했다. 원본은 Codex 생성 폴더에 그대로 보존한다.

## 직접 관찰과 수치 검사

파손된 짧은 기둥 몸체와 받침 주변의 붙은 잔해가 한 개의 넓은 장애물 실루엣을 만든다. 위쪽 갈색 cap, 짧은 남쪽 면, 따뜻한 황갈색 강조가 있다. 기존 방 바닥보다 밝아 실제 월드에서의 색상·크기 조화는 후속 화면 검증이 필요하다. 글자·UI·캐릭터·독립 바닥 판은 없다. 픽셀 느낌의 각진 클러스터는 있지만 정확한 96×72 논리 격자로 손제작한 sprite라는 보장은 없다.

- 전체1448×1086을 PPU724(`texture.width / 2f`)로 만들면 너비2·높이1.5 월드 단위다.
- alpha≥128 영역의 top-left 기준 bbox는 `(49,88)-(1404,995)`이며 오른쪽/아래 끝은 exclusive다. 이 본체는 약1.872×1.253 월드 단위다.
- 완전 투명 pixel은833619/1572528(약53.0%)이다. alpha>0 전체 bbox는 `(46,35)-(1426,1086)`으로 미세한 저알파 pixel이 더 바깥에 있다.
- 재질 대부분의 alpha는253이며 alpha255 pixel은1752개다. alpha≥240은720813개다. 원본의 약한 반투명도와 가장자리 잔여 pixel을 편집해 제거하지 않았다. 마지막 행에도 저알파 pixel5개가 있으므로 전체 alpha bbox를 충돌 크기로 쓰지 않는다.
- 기본 메타는 Single Sprite, Point, Clamp, mipmap off, NPOT none, uncompressed, PPU724, custom pivot `(0.5,0.1)`, fallback physics shape off다. 이 pivot은 하단 접지 기준 배치용 제안이며 alpha 기반 자동 충돌을 생성하지 않는다.
- 런타임에서 `Sprite.Create`로 만드는 경우 메타 pivot 대신 호출의 pivot을 사용한다. 중심 기준 그리드 장애물이라면 `(0.5,0.5)`와 별도 접지 오프셋을 선택할 수 있다. 가로2·세로1.5의 실제 blocked footprint는 월드 코드에서 별도로 모델링해야 한다.

PNG dimensions/alpha/histogram/SHA 검사만 읽기 전용 Pillow로 수행했다. 이미지 저장·변형은 하지 않았다. 이 반입 작업은 Unity import·빌드·경로 탐색·충돌·사용자 시각 승인을 검증하지 않았으며 부모 작업에서 실제 월드에 연결한다.

## 최종 생성 프롬프트

```text
Use case: stylized-concept. Asset type: ONE original production game sprite, a low broken brown stone pillar barricade for a 2D orthographic top-down pixel-art dungeon. Style reference is the already-viewed TempleRoom-v1 room: weathered warm brown-gray sandstone masonry, dark brown seams, restrained ochre edge highlights, old chipped blocks, deliberate crisp 16-bit pixel clusters. Create a NEW standalone object, do not alter or reproduce the room. Subject: one squat ruined square pillar base with a broken uneven stone cap and a small compact cluster of fallen blocks attached to its base, forming one low wide obstacle. Camera: classic top-down RPG view with the top face visible and a short south-facing vertical face; no isometric diamond, no vanishing point. Composition: horizontal silhouette approximately 4:3 width to height, object occupies about 90 percent of the canvas width and 85 percent height, closely framed with minimal transparent padding, not cropped. Ground footprint suitable for width2 world units and height1.5; low enough that a small hero is not hidden behind a tall tower. Pixel art: visibly hard square pixel clusters, approximate 96x72 logical sprite detailing enlarged cleanly, limited palette, sharp silhouette, no smooth painted or 3D render texture. Gentle upper-left warm illumination, modest very tight contact shadow only, no broad floor patch or large soft shadow. Background: truly transparent alpha PNG. No floor, no room, no tile platform, no grass, no characters, no weapons, no runes, no letters, no text, no UI, no logo, no border, no separate detached debris far from the central silhouette. Original directly generated art; one sprite only, no sheet, no variants.
```


## 런타임 연결 후속 기록 — 2026-09-26

`DungeonWorld`가 원본 Texture를 읽어 `Sprite.Create`에서 중심 pivot `(0.5,0.5)`, PPU=`texture.width / 2.4`로 생성한다. 현재 실제 표시 크기는2.4×1.8 월드이며 최초 반입 메타의2×1.5·하단 pivot과 구분한다. (−4,0)과 (4,0)에 같은 sprite 두 개를 놓고 각각2×2 BoxCollider2D와 그리드 통행 불가 셀을 사용한다. 그리드·이동 여유·시야 판정이 실제 자동 경로의 기준이고 픽셀 알파가 충돌을 자동 생성하지 않는다. 이는 단일 마당의 장애물이며 별도 방·navmesh·벽 가림 구현은 아니다. 생성 PNG 바이트와 해시는 변경하지 않았다.

최종 Windows 자동사냥 실행은 [720p 보고](../validation/autohunt/player-720.json)와 [1080p 보고](../validation/autohunt/player-1080.json) 모두 PASS다. 실행 ID는 각각 `26b0296dea004e06a40c4a3cfab8caa8`(70.9288초), `23dbebd8a24b4397bca82dce83b771bd`(70.6474초), Build GUID는 `6c03e8d2bc6e457abc31582911701d3d`다. 두 실행 모두 실제 1배속으로 3구간·3순환·18처치·4개 수거, 사망 0회·재시도 0회, XP 450·골드 144를 확인했다. 영웅 HP 120/공격력 30·적 HP 54/공격력 6을 바꾸지 않았다. 메뉴 중 사냥과 수동 정지/재개, UI 콜백 7회·실제 framebuffer 6개도 검사했다. [720p 전투](../media/autohunt/720-combat.png) · [1080p 관리 중 사냥](../media/autohunt/1080-management.png). 로컬 원본은 `Build/Reports/autohunt-diagnostic-{720,1080}/auto-hunt-smoke.json`이다.

최종 빌드는 `Build/Reports/windows-build.json`의 2026-09-26T06:36:34.0607289Z Succeeded, 오류 0/경고 0이다. 정상 두 해상도는 위 최종 빌드의 근거다. 안전 중단은 아래 명시한 직전 빌드에서 검사했으며 safety-build/source-fingerprint 보고서로 구분한다. 앞선 `autohunt-fixed-720` 후보 실행은 이력으로 구분한다. 이전 `docs/validation/management/`는 관리 기능 시제품의 역사적 근거다. 디스크 저장, 20분 연속 실행, OS 물리 입력 검사, 사용자 시각 승인은 완료되지 않았다.


안전 검사의 빌드 GUID는 `0a94c21a725b4d3a9a01e269067fa423`다. 이후 변경은 관리 창 위 피해 숫자 숨김과 검사 보고의 최종 성장 수치 추가이며 전투·안전 중단 코드는 동일하다. [연속 사망 안전 검사](../validation/autohunt/safety-deaths.json)는 run `f508cbfae00844128c310426a8abbbc3`, 약 29.4초에 실제 적 공격으로 두 번 사망·한 번 재시도 후 자동 중단, 4.01초 중단 유지 PASS다. 이 격리 검사만 영웅 공격력을 1로 주입했으며 정상 밸런스 증거가 아니다. [가방 포화 안전 검사](../validation/autohunt/safety-bag-full.json)는 run `a8fd4677ac5741bb90de2caeccef30fa`, 시험용 무기 24개로 가방을 채운 뒤 실제 첫 처치의 전리품을 보류 상태로 보존하고 중단했다. 약 7.65초, 골드 8·XP 25·기존 아이템 24개 유지, 중단 4.01초 PASS다. 두 검사는 중단 후 재개나 20분 안정성을 증명하지 않는다.
