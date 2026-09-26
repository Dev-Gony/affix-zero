# Hero Siege 방향 환경·픽셀 GUI 후보 검토

검토일: 2026-09-26. 상태: **공식 페이지·실제 브라우저 프리뷰 검토만 완료 / 구매·원본 다운로드·Unity 반입·사용자 승인 미실행**.

이번 검토 기준은 사용자가 기존 닌자 팩과 새 HUD를 거절한 뒤 제시한 Hero Siege 화면이다. 탕탕특공대식 성장 선택과 단순하고 귀여운 8비트 스타일은 이번 후보 기준에서 제외한다. 석조 던전/사원의 재질, 높이 있는 벽과 기둥, 넓은 시야 안의 작은 영웅, 고대비 공격 효과, 아이콘 밀도가 있는 PC ARPG 화면을 목표로 삼는다. 아래 적합성 평가는 공개 미리보기를 보고 내린 판단이며, Hero Siege 원본 이미지나 게임 에셋을 가져오는 제안이 아니다.

## 후보 비교

| 후보 | 역할·공식 규격 | 실제 프리뷰에서 본 장점 | 차이·확인할 점 | 가격·원본 공개 Git 재배포 |
| --- | --- | --- | --- | --- |
| [Lucifer - Dungeon Tileset / Foozle](https://foozlecc.itch.io/lucifer-dungeon-tileset) | 환경, 탑다운 32×32, ASE 포함 | 석재 바닥의 균열·음영, 여러 타일 높이의 벽, 문과 벽 장식, 작은 캐릭터가 들어간 넓은 공간 | 미리보기 바닥 반복이 뚜렷하고 석상/기둥 다양성이 제한적으로 보인다. 목표 사원 분위기와 FX 가독성을 별도 한 장면에서 확인해야 한다. 미리보기 캐릭터가 타일 팩에 들어 있다고 가정하지 않는다. | Name your own price. 공식 팩 페이지 CC0 1.0. **CC0 적용 원본은 공개 재배포 가능**하나 다운로드 파일별 라이선스 대조는 미실행 |
| [Winlu Fantasy Tileset - Dungeon / WinLu](https://winlu.itch.io/winlu-fantasy-tileset-dungeon) | 환경, 48×48, 일반 엔진용 `Non_RPGMaker_textures.zip` 제공 | 큰 석재 블록과 바닥 문양, 입체적인 높은 기둥/석상, 문·단차·석관 등 풍부한 깊이 표현. 텍스처와 사원 구조 측면에서 우선 비교할 유료 후보 | 32×32 캐릭터·UI와 픽셀 밀도를 맞춰야 한다. 프리뷰의 안개·빛 연출이 원본 PNG에 포함되는지/별도 합성인지 미확인. 현재 자료만으로 전투 FX나 영웅 애니메이션 확보를 주장할 수 없다. | 라이브 페이지 **US$17.50 세일 / 정가 US$25**. 상업 프로젝트·수정 허용, 원본 재배포·재판매 금지. **공개 Git 원본 커밋 불가** |
| [Lucifer - RPG UI / Foozle](https://foozlecc.itch.io/lucifer-rpg-ui) | 픽셀 GUI, HUD·폰트·100개 이상 스킬 아이콘·패널·버튼·ASE 제공 | 픽셀 테두리, 하단 orb와 스킬 슬롯, 촘촘한 장비·인벤토리 패널, 탭과 아이콘 등 PC ARPG 구성 재료가 실제 프리뷰에 있다 | 프리뷰 전체 배치를 복사하면 패널이 전장을 크게 가린다. 필요한 부품만 가져와 화면을 구성해야 한다. 녹색/적색 기본 팔레트, 글꼴의 한글 지원, 작은 아이콘 가독성과 슬라이스 범위는 원본 검수 필요. UI 아트가 실제 인벤토리 시스템 구현을 뜻하지 않는다. | Name your own price. 공식 팩 페이지 CC0 1.0. **CC0 적용 원본은 공개 재배포 가능**하나 동봉 폰트·원본별 권리 대조는 미실행 |

가격은 이 문서 검토일에 공식 페이지와 브라우저에서 관측한 표시이며 구매 시 다시 확인한다. 구매나 기부는 수행하지 않았다.

## 브라우저 시각 검토 근거

Chrome에서 각 공식 페이지를 열고 이미지 갤러리의 확대 프리뷰를 실제로 확인했다. iab 브라우저는 사용할 수 없어 연결된 Chrome을 사용했다. 아래는 해당 페이지에 노출된 원본 프리뷰 링크이며 저장소에 이미지를 다운로드하거나 복사하지 않았다.

- Lucifer Dungeon: [공식 장면 프리뷰](https://img.itch.zone/aW1hZ2UvMTY1ODEzNC85NzU4MDQ3LnBuZw==/original/PtmF%2B4.png). 밝은 회색 석판·어두운 높은 벽·계단형 통로와 작은 인물의 상대 크기를 확인했다. 빨간 장식과 용암도 보이지만 이 요소들을 채택하자는 뜻은 아니다.
- Winlu Dungeon: [공식 석조 방 프리뷰](https://img.itch.zone/aW1hZ2UvMTg1NDMwMi8yNDkxODU0Ny5wbmc=/original/x4Ezbu.png). 양쪽의 높은 석상/기둥과 석재 바닥 문양, 벽의 입체감, 조명 대비를 확인했다. 넓은 전장을 만들 때 장식 밀도를 줄이고 바닥 가독성을 유지할 필요가 있다.
- Lucifer UI: [공식 GUI 프리뷰](https://img.itch.zone/aW1hZ2UvMTAyMDg1OC81ODE5MjkwLnBuZw==/original/5Iwhu%2B.png). 하단 자원 orb·액션 슬롯과 오른쪽 장비/격자형 패널을 확인했다. 이 프리뷰는 UI 구성 재료의 증거이며 현재 프로젝트 화면이나 작동하는 시스템의 증거가 아니다.

## 라이선스와 Git 정책

Lucifer 두 팩의 공식 배포자는 Foozle이고 페이지에는 제작 의뢰 대상이 David / chroma_dave로 표기되어 있다. 두 팩 모두 정확한 제품 페이지에서 CC0 1.0을 명시한다. [CC0 공식 요약](https://creativecommons.org/publicdomain/zero/1.0/)은 복제·수정·배포·상업 이용을 허용한다. 따라서 해당 CC0가 실제 내려받는 파일에 적용됨을 확인하면 공개 소스 저장소에 원본을 포함할 수 있다. **CC0 표기만으로 원본 구성·동봉 폰트·시각 적합성 검수를 완료 처리하지 않는다.**

Winlu는 [공식 제품 페이지의 License](https://winlu.itch.io/winlu-fantasy-tileset-dungeon)에 상업 프로젝트 사용과 수정은 허용하지만 에셋 재배포·재판매를 금지한다고 명시한다. 유료 구매는 원본 공개 배포 권리를 사는 것이 아니다. 이 후보를 채택한다면 공개 Git에는 출처·도입 문서·로더 코드만 두고 유료 원본은 제외하거나, 권리자로부터 공개 저장소 재배포에 관한 별도 허락을 받아야 한다. 사용자의 구매 승인이 권리자의 재배포 허락을 대체하지 않는다. 이번 조사에서는 구매·권리자 연락·비공개 업로드를 하지 않았다.

## Lucifer UI 원본 검수 — 2026-09-26 후속

이 절은 위의 최초 조사 중 **UI 원본 미확보** 상태만 갱신한다. 공식 itch 페이지의 무료 다운로드 흐름에서 ZIP을 확보했으며, 구매·기부·Assets 반입·Git 원본 업로드는 하지 않았다. 보관 위치는 Git에서 무시되는 `Build/Downloads/HeroSiegeCandidates/LuciferUI/`다.

### 파일과 권리

- 공식 파일: `Foozle_UI_0002_Lucifer_RPG_UI_Pixel_Art.zip`, 31,018,893 bytes. SHA-256: `c4cddb70ad02696bbe250c83e5e1f5ab248dd2d06adfb7ed2694f206454d16cf`.
- ZIP의 파일 421개: PNG 245, ASEPRITE 169, GIF 3, TXT 2, TTF 1, RAR 1. 전체 경로·크기는 로컬 `manifest.json`, 이미지 크기·투명 영역은 `image-metadata.json`, ASE 프레임 수는 `ase-metadata.json`에 기록했다.
- 루트 `Readme.txt`는 Lucifer RPG UI 1.0 전체 콘텐츠에 CC0 1.0을 명시하고 상업 이용·수정을 허용한다. 이미지/아이콘 하위 폴더에 상충하는 별도 라이선스는 없었다. 따라서 **이 패키지의 UI PNG·ASE와 아이콘은 동봉 CC0에 근거해 공개 Git 원본 재배포가 가능한 후보**다. 실제 채택·시각 승인을 뜻하지 않는다.
- 출처 표기에 차이가 있다. [현재 공식 제품 페이지](https://foozlecc.itch.io/lucifer-rpg-ui)는 David/chroma_dave, ZIP의 Readme는 Baldur/the__baldur를 위탁 제작자로 표기한다. 배포자 Foozle·제품명·CC0 조건은 일치한다. 어느 제작자 표기가 최신인지 추정하여 수정하지 않는다.
- `Panels/Panels.rar`는 중첩 압축이다. Windows tar로 목록을 확인한 뒤 같은 ignored 루트의 `nested-panels/`에 추출했다. 64×64 PNG 4개와 대응 ASEPRITE 4개뿐이며, 실행 파일·추가 폰트·별도 라이선스는 없다. 중첩 파일 8개는 위 ZIP 파일 수 421개와 별개다.

### 폰트 별도 판정

`Font/PixelRpgFont-Regular.ttf`는 8,100 bytes이며 SHA-256은 `8b7cdfb1338a4c3eeeaa3b4c2f2b3d379926734adcb7340b5330fa7ee0d3fd92`다. 같은 폴더에 제작 원본 `Font.aseprite`가 있다. TTF name table은 Pixel Rpg Font Regular, Version 001.003, Calligraphr 생성(2021-03-27)을 담고 있다. 독립 copyright·제작자·license text·license URL 필드는 없다. Arial 등 다른 폰트명이나 별도 상용 폰트 라이선스는 발견되지 않았다. **이름이 없다는 사실만으로 독립적인 권리 증명이 되는 것은 아니다.**

공식 제품 설명이 Font를 포함한다고 명시하고 ZIP Readme가 전체 콘텐츠를 CC0로 제공하므로, 동봉 폰트에도 패키지 CC0를 적용한다는 배포 근거는 있다. 다만 독립 폰트 권리자의 명시와 위 제작자 표기 차이까지 해소되지는 않았다. 이번 작업에서는 TTF를 Assets나 공개 Git에 넣지 않았다. 이미지와 폰트를 일괄 승인 처리하지 말고 도입 결정에서 이 차이를 기록한다. 폰트의 Unicode cmap에 한글 음절은 **0개**이며 라틴 알파벳·숫자·일부 구두점 중심이다. 따라서 한국어 UI 폰트로 사용할 수 없고, 한글은 별도 검수된 폰트가 필요하다.

### 실측 규격과 슬라이스 판단

| 원본 | 실측 | 도입 시 주의 |
| --- | --- | --- |
| `HUD/Hotbar/Hotbar.png` | 198×40 RGBA | 두 원형 자원 프레임과 액션 슬롯이 한 장에 있다. 전체 9-slice 확대는 원과 슬롯 비율을 찌그러뜨린다. 정수 배율 또는 부품별 분리가 필요하다. |
| `HUD/Hotbar/MainBars/Png/MainBars1.png` 등 4개 | 각각 30×30 | ASE의 4프레임은 색상/자원 변형이다. 전투 애니메이션으로 오인하지 않는다. |
| `HUD/Hotbar/SphereMask/Png/SphereMask.png` | 300×30 | 30×30 가로 10프레임. 같은 ASE·GIF도 10프레임으로 일치한다. |
| `Generic/Menu/Png/GenericPanel.png` | 48×48 RGBA | 중앙 단색과 픽셀 모서리 장식. 사방 13px를 보존하는 9-slice 후보를 픽셀 측정으로 제안한다. Unity 적용/크기별 시각 검증은 미실행이다. |
| `Generic/Menu buttons/Png/GenericButton.png` | 48×16, 실제 alpha bbox `(0,1)-(48,14)` | 위·아래 투명 여백이 비대칭이다. 파일 전체 rect 기준 정렬과 그림 기준 정렬을 구분한다. |
| `Generic/Additional buttons/*/Png/*.png` | 16×16 | Equipment/Inventory/Leaderboard/Settings/Skills 각각 Normal/Active/Pressed 상태. 이름만으로 클릭 구현을 완료 처리하지 않는다. |
| `HUD/Inventory/Png/Inventory.png` | 98×131 | 슬롯과 장비 영역이 그려진 정적 프레임. 실제 장비/인벤토리 로직은 포함하지 않는다. |
| `Skill Icons/*/*/Png/*.png` | **111개, 모두 16×16** | 고대비 녹색/보라/적색 계열. `Skill Icons/spritesheet.png`는 198×198이다. 개별 PNG를 사용하면 시트 간격 추정이 필요 없다. |
| `Panels.rar`의 `SimplePanel01~04.png` | 각각 64×64 | 별도 사각 패널 후보. 실제 HUD 채택 전 색상·테두리 조합 검토 필요. |

Hotbar·아이콘 시트·GenericPanel 원본을 직접 열어 확인했다. PNG/ASE에 Unity pivot 또는 Sprite border 메타데이터는 동봉되어 있지 않다. UI 앵커/pivot/border는 도입할 때 명시해야 한다. 로그인/메인 메뉴 ASE는 각각 16/40프레임이지만 이 검수는 해당 장식 애니메이션의 런타임 재생을 검증하지 않았다.

### 부모 시각 검토용 원본 경로

공통 루트는 `D:/github/affix-unity/Build/Downloads/HeroSiegeCandidates/LuciferUI/extracted/Foozle_UI_0002_Lucifer_RPG_UI_Pixel_Art/`다. 모두 가공하지 않은 원본이다.

- `HUD/HUD Mockup.png` — 4800×2700 전체 구성 참고.
- `HUD/Hotbar/Hotbar.png` — 198×40 실제 하단 프레임.
- `Skill Icons/spritesheet.png` — 198×198 아이콘 미리보기.
- `Generic/Menu/Png/GenericPanel.png` — 48×48 패널 원본.
- `HUD/Inventory/Png/Inventory.png` — 98×131 장비 패널 원본.
- 별도 루트 `D:/github/affix-unity/Build/Downloads/HeroSiegeCandidates/LuciferUI/nested-panels/Panels/SimplePanel01.png` — 64×64 패널 원본.

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
| --- | --- | --- | --- |
| 브라우저 Download 클릭 후 파일 경로 미확보 | 클릭 성공 UI만으로 파일 저장 완료를 알 수 없었음 | 공식 Download 클릭 전 다운로드 이벤트를 구독하고 반환된 정확한 파일 경로를 복사·해시·ZIP 경계 검사 후 추출 | 다운로드 시도와 원본 확보를 분리해 기록 |
| 동봉 폰트 권리 판정 | 팩 CC0는 있지만 폰트 독립 권리 필드가 없고 제작자 표기가 웹/Readme에서 다름 | TTF name/cmap을 읽고 팩 CC0 근거·독립 권리 미확인·한글 미지원 구분 | 폰트 파일명이나 CC0 문자열만으로 모든 권리/언어 지원 승인 금지 |

## 다음 결정에 필요한 최소 장면

환경 후보 선택 전 새 아트로 **한 장의 넓은 전장 + 영웅 1명 + 적 1명 + 실제 공격 FX + 필요한 픽셀 HUD 부품**을 묶어 상대 크기와 픽셀 밀도를 비교해야 한다. 무료 여부보다 사용자가 제시한 화면에 맞는지가 우선이다. Winlu는 재질·기둥 깊이 비교 우선, Lucifer는 32×32 밀도와 공개 저장소 작업 적합성 비교 우선으로 둔다. 어느 쪽도 이번 문서 작성만으로 채택하지 않는다.

원본 확보 후에는 각 파일의 동봉 라이선스, PNG/ASE 규격, 타일 경계, 다층 벽 정렬, 바닥·기둥 pivot, 투명 여백, 프리뷰에만 들어 있는 요소, UI 폰트/아이콘 범위를 검수한다. 환경이나 UI 팩이 있다는 이유로 영웅·적의 방향별 walk/attack/hit/death 프레임 검수와 실제 impact 검증을 생략하지 않는다.

두 환경과 GUI 후보는 현재 Unity Built-in 2D에서 스프라이트·타일·UI 이미지로 검토 가능한 유형이다. 미리보기의 예술적 차이만으로 엔진 전환이 필요하다는 근거는 확인되지 않았다. 실제 import·레이어·픽셀 스케일 검증은 하지 않았다.

## 진행 상황 체크포인트

1. 완료: 공식 페이지·라이선스 문구와 브라우저 확대 프리뷰로 환경 2종, 픽셀 GUI 1종을 비교했다.
2. 미완료: 원본 확보·파일별 라이선스·프레임·Unity 반입·사용자 시각 승인 전 단계다.
3. 변경 범위: 이 후보 문서만 작성했으며 기존 Unity 소스·설정·에셋을 수정하거나 구매하지 않았다.
