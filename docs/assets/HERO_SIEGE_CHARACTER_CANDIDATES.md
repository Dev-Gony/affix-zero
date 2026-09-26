# Hero Siege 오마주: 신규 캐릭터 후보 검수

조사일: 2026-09-26. **사용자의 최신 요구는 탕탕특공대 요소 폐기, Hero Siege 오마주, 캐릭터와 환경 아트 재선정이다.** 이전의 밝은 초원 고정·Ninja Adventure 재활용·색 변경은 이번 후보의 기준이 아니다. 이 문서는 후보 조사이며 사용자의 새 아트 승인이나 Unity 구현 완료가 아니다.

## 결론

**첫 시각 검토 후보는 Zerie Tiny RPG 01의 무료 Soldier + Orc다.** 둘 다 실제 걷기·공격·사망 원본이 있고 별도 영웅/적 실루엣을 제공한다. 다만 좌우만 지원하는 시점과 단일 피격 자세+색상 점멸의 한계를 숨기지 않는다. 사용자 첨부 Hero Siege 화면에 충분히 부합하는지는 별도 시각 판단이 남아 있다. 유료 전체 팩을 구매해야 첫 영웅/적을 만들 수 있는 것은 아니다.

**Pixel Crawler는 이번 영웅1/근접적1 후보로 보류한다.** 새 4방향 베이스의 동작 목록을 기존 Orc/Skeleton의 동작으로 간주할 수 없다. 후자의 공격·피격 원본이 검증되지 않았고, 미리보기의 옷 없는 베이스를 완성된 갑옷 영웅으로 취급할 수 없다.

공개 Git에 원본을 올릴 수 있는지와 게임에서 시각적으로 적합한지는 분리한다. 권리 제약이 있는 적합한 팩은 원본을 ignored 로컬 폴더에 두고 공개 저장소에는 해시·입수 방법·import instructions만 기록하는 방식이 가능하다. 라이선스가 없는 다른 GitHub 재업로드를 입수 경로로 사용하지 않는다.

| 비교 | Tiny RPG 01 V2.0 | Pixel Crawler Free 2.11 |
|---|---|---|
| 제작자 | Zerie | Anokolisa |
| 후보 쌍 | Soldier / Orc | Base Character / Orc 또는 Skeleton |
| 방향 | 오른쪽 원본, 좌우 반전 사용. 상하 없음 | 신규 Base 4방향 WIP / 기존 적은 옆모습 |
| 필수 동작 | 무료 ZIP에서 idle, walk, attack, hurt, death 확인 | Base 목록에는 모두 대응 동작 있음. 적은 idle/run/death만 명시 |
| 원본 검수 수준 | ZIP·PNG·Aseprite 직접 검사 | 공식 페이지·Terms PDF·원본 GIF 직접 확인, ZIP 프레임 검수는 미실시 |
| 공개 Git 원본 | 재배포/재업로드 금지 | 원본 재배포의 명시적 허락은 확인하지 못함 |
| 이번 판단 | 조건부 시각 검토 1순위, 미채택 상태 | 첫 완성 캐릭터 쌍으로 보류 |

## 1. Tiny RPG Character Asset Pack 01 V2.0

[공식 배포·라이선스·가격](https://zerie.itch.io/tiny-rpg-character-asset-pack), [제작자의 2방향 설명](https://zerie.itch.io/tiny-rpg-character-asset-pack/comments?after=32), [효과 분리·Aseprite 추가 개발 기록](https://zerie.itch.io/tiny-rpg-character-asset-pack/devlog/764723/v103b-updateseparate-effect-animationadded-ase-filesbug-fixes).

가격 확인: 무료 Soldier/Orc ZIP, 전체 22종은 조사 시점 $2.50 USD 이상. 실제 다운로드 창에 정가 $5, 50% 할인 종료 2026-10-03으로 표시됐다. 첫 2종 검수에 유료 구매는 필요하지 않으며 결제하지 않았다.

### 직접 확인한 원본

- 공식 무료 다운로드: `Tiny RPG Character Asset Pack 01 v2.0 -Free Soldier&Orc.zip`, 배포 페이지 업로드 시각 2026-07-01 15:16 UTC.
- 검수용 복사: `Build/Downloads/HeroSiegeCandidates/TinyRPG01_v2_FreeSoldierOrc.zip`.
- ZIP 크기 **131,873B**, SHA256 **`9f1818c7ddc17b99e4bae81feae8b25a3692bf6bc269bb2797e36bd7faec3ba2`**.
- 추출 루트: `Build/Downloads/HeroSiegeCandidates/TinyRPG01_v2/Tiny RPG Character Asset Pack 01 v2.0 -Free Soldier&Orc`.
- ZIP과 추출물은 `git check-ignore`로 ignored 경로임을 확인. `Assets`로 반입하지 않았다. ZIP 내 별도 README/라이선스 텍스트 파일은 발견되지 않아 공식 페이지 조건을 기록한다.

원본 미리보기 링크: [Soldier GIF 300×144](https://img.itch.zone/aW1nLzI4MjI1NTA1LmdpZg==/original/qGz0FC.gif), [Orc GIF 300×132](https://img.itch.zone/aW1nLzI4MjI1NTQwLmdpZg==/original/u59Sw2.gif). 공식 미리보기는 확대된 홍보 재생물이며, 다운로드 PNG의 실제 cell 크기와 혼동하지 않는다. 브라우저에서 Soldier GIF와 공식 캐릭터 소개 이미지를 열었고 로컬 Soldier/Orc 전체 sprite sheet를 직접 보았다. 원본 이미지 변형·생성은 하지 않았다.

### 실제 프레임 검사

PNG 경로는 `Characters(100x100 split)/{Soldier|Orc}/{Soldier|Orc}/{이름}_{클립}.png`다. 모든 split sheet는 **100×100 cell**, 한 행에 시간 순서가 들어 있다. 유효 프레임은 모두 비어 있지 않았다. 아래 고유 수는 RGBA 픽셀 해시로 비교한 값이다. 원본의 의도적인 hold/repeat를 삭제하지 않는다.

| 동작 | Soldier 프레임/고유 이미지 | Orc 프레임/고유 이미지 | sheet 크기 |
|---|---:|---:|---|
| Idle | 6 / 3 | 6 / 3 | 600×100 |
| Walk | 8 / 8 | 8 / 8 | 800×100 |
| Attack01 | 6 / 6 | 6 / 6 | 600×100 |
| Attack02 | 6 / 6 | 6 / 6 | 600×100 |
| Hurt | 4 / 3 | 4 / 3 | 400×100 |
| Death | 4 / 4 | 4 / 4 | 400×100 |
| Attack03 | 9 / 9, 활 공격 | 없음 | 900×100 |

`Aseprite file/Soldier.aseprite`의 태그(0기준): Idle 0–5, Walk 6–13, Attack01 14–19, Attack02 20–25, Attack03 26–34, Hurt 35–38, Death 39–42. Orc: Idle 0–5, Walk 6–13, Attack01 14–19, Attack02 20–25, Hurt 26–29, Death 30–33. 양쪽 원본 Aseprite의 frame duration은 보통 **100ms(10fps)**, 마지막 death frame은 **600ms**다. 죽은 상태의 게임상 유지 시간은 이 원본 미리보기 hold와 별도 설계해야 한다.

첫 Idle 프레임 alpha 범위(좌상단 좌표, 우/하 exclusive): Soldier `(41,39,58,60)` 즉 17×21px, Orc `(44,42,66,57)` 즉 22×15px. 100×100은 캐릭터 자체 크기가 아니라 공격 여백을 포함한 cell이다. 무기·효과가 몸과 함께 그려져 있으며 Attack01의 강한 베기 효과는 양쪽 index3에서 시작하는 모습으로 확인했다. **impact index3은 관찰 기반 후보일 뿐, 게임 판정에 확정한 값이 아니다.** pivot과 실제 발 접점도 import 전에 따로 확정해야 한다.

### 시각 관찰과 한계

관찰: 금속 투구·갑옷·칼을 가진 Soldier와 초록 피부·도끼의 Orc가 확실히 구분된다. 걷기는 발·팔 자세가 바뀌고, 공격은 무기 준비와 큰 베기 궤적, 사망은 쓰러지는 자세 변화를 가진다. 위·아래를 바라보는 그림은 없다. 제작자도 side-looking 2방향 스타일이라고 설명했다.

Hurt는 4프레임이지만 **alpha 실루엣은 모두 동일**하고 RGB 이미지가 3종이다. Idle 첫 자세와는 다르다(alpha 차이 Soldier 52px, Orc 92px). 즉 기존 idle 그림에 단순 점멸만 넣은 것은 아니지만, 별도 피격 자세 하나에 붉은/주황 flash를 주는 구성이다. 여러 피격 몸동작을 갖춘 애니메이션이라고 과장하지 않는다. 피격 품질이 부족하다고 판단되면 새 작업이 필요한 지점이다.

원본 주의: Soldier의 `Soldier`와 `Soldier with shadows` 폴더에서 split 동작 PNG 7개가 각각 byte-identical이다. `Soldier.png` 전체 sheet는 서로 다르다. 폴더 이름만 보고 split PNG에 그림자가 없다고 가정하면 안 된다. Orc는 두 폴더의 이미지가 서로 달랐다.

판단: 기존 닌자의 단순한 실루엣에서 벗어난 중세 전투 캐릭터 후보로 비교할 가치가 있다. 다만 작은 실제 몸 크기·좌우 시점·제한된 hurt를 유지한 채 Hero Siege와 같아졌다고 선언해서는 안 된다. 사용자 첨부 화면의 캐릭터 크기/필드 비율에 맞춘 시각 검토가 다음 관문이다.

### 권리와 공개 저장소

공식 조건은 개인/상업 게임 사용과 수정은 허용하며, 원본·수정 에셋의 재배포·재업로드는 금지한다. 따라서 **공개 Git의 Assets나 docs/media에 원본 sheet/GIF를 복사하지 않는다**. 무료 다운로드와 공개 재배포 권리는 다르다. 원본은 ignored 로컬 보관, 공개 문서에는 제작자 링크·파일 이름·해시·향후 import 절차만 기록하는 경로를 사용할 수 있다. 이것은 시각적 탈락 사유가 아니라 원본 관리 방식의 제약이다.

## 2. Pixel Crawler Free 2.11

[공식 최신 무료 팩](https://anokolisa.itch.io/free-pixel-art-asset-pack-topdown-tileset-rpg-16x16-sprites), [제작자 Q&A](https://itch.io/t/4585143/pixel-crawler-qa), [공식 Terms PDF](https://drive.google.com/file/d/17y1gjuwVirE8V79WcTL9tgTUtMu6R1je/view?usp=sharing).

무료 다운로드 이름은 `Pixel Crawler - Free Pack 2.11.zip`, 페이지 표기 2.3MB다. 이 조사에서는 Pixel Crawler ZIP을 다운로드하지 않았고 source cell/프레임 수/pivot은 미검증이다. 제목의 16×16은 tileset 기준이므로 모든 캐릭터 cell도 16×16이라고 추정하지 않는다.

공식 페이지는 **Base Character 4 Directions - WIP**를 별도로 표시하며 idle/walk/run/hit/death 및 slice/crush/pierce를 나열한다. 기존 Orc/Skeleton 목록은 idle/run/death다. NPC도 side only라고 분리돼 있다. 제작자는 좌우 방향은 엔진 반전으로 사용한다고 Q&A에서 설명했다. 신형 베이스의 상하/좌우 구성과 기존 적의 옆모습을 하나의 완성된 4방향 전투 세트로 취급하지 않는다.

직접 브라우저에서 확인한 제작자 원본 GIF:

- [4방향 Base 미리보기, 1280×1856](https://img.itch.zone/aW1nLzIwMTM3NTg2LmdpZg==/original/BPD9sG.gif): 옷 없는 캐릭터의 방향별 검·방패 자세와 작업/전투 동작 모음. 이 그림만으로 갑옷·의상까지 제공된 완성 영웅을 확인한 것은 아니다.
- [Orc 미리보기, 1024×320](https://img.itch.zone/aW1nLzEwMzg5NDY2LmdpZg==/original/WmAuAI.gif): 큰 무기를 든 4종 옆모습 오크, 녹색 피부와 직업별 장비 실루엣. 무기를 들고 있는 모습 자체를 attack 클립 존재의 증거로 삼지 않는다.

시각 판단: 장비와 적 실루엣은 장르에 맞는 부분이 있으나 현재 베이스는 의상 제작 범위를 늘리고, 적은 필수 attack/hit 검증이 빠져 있다. 이번 첫 영웅/적용으로 우선 채택하지 않는다. 유료 환경 번들을 사면 누락 동작도 자동으로 해결된다는 근거는 없다.

Terms PDF 2페이지를 브라우저의 실제 본문으로 읽었다. 게임/상업 프로젝트 사용과 수정은 허용하고, 원본/수정 에셋 자체 판매는 제한한다. 공개 Git의 원본 무상 재배포를 명시적으로 허용하는 문장은 찾지 못했다. **금지라고 단정하지도, 공개 배포 허가가 확인됐다고 표시하지도 않는다.** 채택 시에는 원본을 공개 Git에서 제외하거나 제작자에게 명확한 권리를 확인해야 한다. 이번 조사에서는 제작자에게 메시지를 보내지 않았다.

## 완료 범위와 다음 관문

1. 완료: 두 후보 공식 출처·방향·동작·가격/조건 및 원본 미리보기 확인, Tiny RPG 무료 ZIP 프레임·Aseprite 시간표 직접 검사.
2. 선택 대기: Tiny RPG Soldier/Orc의 시점·비율·hurt 구성에 대한 시각 판단. Pixel Crawler는 적 필수 동작 확인 전 보류.
3. 미수행: 구매, Assets 반입, Unity 실행, 구현, 공개 원본 업로드, 사용자 시각 승인. Ninja 원본 재활용도 수행하지 않았다.
