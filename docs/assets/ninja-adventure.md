# Ninja Adventure — 새 U1 아트 반입 기록

검수일: 2026-09-26. 상태: 제작자 원본 확보·동봉 라이선스 대조·PNG 정적 프레임 검수 완료. Unity import/Play/Windows build/사용자 시각 승인은 별도 검증 단계이며 이 기록은 해당 완료를 의미하지 않는다.

## 출처와 공개 재배포 근거

- 제작자: Pixel-Boy, AAA.
- [제작자 배포 페이지](https://pixel-boy.itch.io/ninja-adventure-asset-pack), 다운로드 표시명 `Ninja Adventure - Asset Pack.zip`, itch upload ID `16981275`. Update #8 이후 2026-09-26에 제공된 원본; 별도 semantic version은 없다.
- 로컬 원본 `Build/Downloads/NinjaAdventure_AssetPack.zip` (Git 제외).
- 원본 SHA-256: `95a06f4fdcfd1882f061a45ff313b7c905dbe2de1e8512b281d7937df62a7b15`.
- ZIP 동봉 `LICENSE.txt`는 CC0 1.0 Universal 전문, `README.md`는 제작자·팩 링크와 CC0 적용을 명시한다. 두 파일을 `Assets/Art/NinjaAdventure/LICENSE.txt`, `SOURCE_README.md`에 그대로 복사했다.
- [CC0 공식 설명](https://creativecommons.org/publicdomain/zero/1.0/)과 동봉 원문 1~3항의 저작권 포기/배포 권리를 대조했다. 이 팩의 원본 PNG 및 수정본을 공개 GitHub에 포함할 수 있다는 근거다. 현행 Tiny Swords 제한 라이선스를 이 팩에 적용하거나 그 반대로 적용하지 않는다.
- GitHub에 새로 반입하는 것은 11 PNG와 2개 출처/라이선스 파일, 합계 121,702 bytes(메타 제외)다. 원본 ZIP의 오디오·폰트·예제 프로젝트·사용하지 않는 캐릭터는 반입하지 않았다. 제작자의 Godot 예제도 사용하지 않는다.
- 개별 파일 SHA-256/크기는 `ninja-source-manifest.json`에 기록했다. 폐기된 AFFIX Godot 소스·이미지·백업·PR에서 복원한 파일은 없다.

## 첫 영웅과 근접 적

원본 `Actor/CharacterAnimated/NinjaGreen/Separate`의 신규 NinjaGreen 동작을 사용한다. 영웅은 원본 초록 정찰병과 Katana, 첫 적은 같은 캐릭터의 다른 진영 근접 정찰병과 Axe로 구분한다. 적의 색은 Unity에서 새 리소스에 적용하는 진영 tint이며 별도 제작자 캐릭터나 새 적 애니메이션이라고 표현하지 않는다. 현재 패키지의 구형 Monster sheet를 완전한 attack/hit/death 동작으로 간주하지 않는다.

| 클립 | PNG | 셀 | 실제 시간 프레임 | 픽셀 내용 고유 프레임 | 초기 제안 | 현재 연결 선택 |
|---|---|---|---|---|---|---|
| Idle | 128×128 | 32×32 | 4 | 4 | 5 fps | 양쪽 8 fps |
| Walk | 128×128 | 32×32 | 4 | 4 | 8 fps | 양쪽 8 fps |
| Attack | 128×128 | 32×32 | 4 | 4 | 10 fps | 영웅 8 / 적 10 fps |
| Hit | 128×64 | 32×32 | 2 | 2 | 10 fps | 양쪽 10 fps |
| Dead | 32×64 | 32×32 | 2 | 2 | 6 fps | 양쪽 10 fps |

PNG는 열이 방향, 행이 시간이다. 방향열은 0=down, 1=up, 2=left, 3=right. U1 연결은 오른쪽 열과 수평 반전을 먼저 사용하며 상하 방향별 재생 구현 완료라고 주장하지 않는다. Dead는 방향 공용이다. 위 fps는 프로젝트 튜닝 값이며 제작자 고정 FPS 사양이 아니다. 제작자 Attack 미리보기 GIF는 200/60/60/100ms 등을 섞고 IdleWalk 미리보기는 200ms 간격이다. `ninja-animation.json`의 `framesPerSecondProposed`는 최초 정적 검수 제안값으로 보존하며 픽셀/해시 기록도 변경하지 않는다. 현재 `ReviewedArtSetup`의 movementFps=8, reactionFps=10, 공격은 영웅8/적10이며 HTML 검사 도구도 현재 선택값을 사용한다. 짧은 피격 동작이 연속 공격 사이를 과도하게 점유하지 않도록 공용 reaction 속도를 10 fps로 선택했다. Unity 실제 실행 검증과는 별개다.

오른쪽 클립의 top-left rect는 `(96,row*32,32,32)`, Dead는 `(0,row*32,32,32)`. Unity y좌표는 `sheetHeight-(row+1)*32`. PPU=16, point filtering, mipmap 없음, 무손실 RGBA sprite import, body pivot=(0.5,0.25). 발 기준점은 원본 셀의 top-left (16,24)이며 사망 자세는 그 자리에 주저앉아 발 영역이 2px 더 낮다. 전체 좌표·픽셀 해시는 `ninja-animation.json`에 있다.

## 몸과 무기의 공격 일치

Katana/Axe는 각 256×256, 셀 64×64, 동일한 4방향 열·4시간 행이다. 몸 32셀을 무기 64셀의 가운데에 두면 손과 무기 손잡이가 맞는다. 같은 transform 위치와 PPU=16에서 body pivot=(0.5,0.25), weapon pivot=(0.5,0.375)를 사용한다. 무기는 몸보다 위의 sorting order로 공격 중만 표시한다.

impact index=1(0부터 시작, 두 번째 프레임)에서 실제 검격/도끼 궤적이 펼쳐진다. index0은 들어올리는 준비, index1은 swing, index2~3은 회복이다. 프레임별 몸 자세와 손/무기 위치가 바뀜을 실제 PNG 합성 접촉시트로 확인했다. 접촉시트는 검사 파일이며 게임용 파생 PNG는 생성하지 않았다. Unity에서 대상 거리·크기와 맞춰 실제 impact와 피해가 일치하는지는 아직 별도 Play 검증이 필요하다.

## 밝은 야외 환경

같은 팩의 TilesetFloor/TilesetNature/TilesetWater/TilesetFloorDetail 원본을 채택했다. grass 변형, 흙길 중심과 4변, 물/연못, 나무/덤불/꽃/돌의 검수된 top-left rect는 `ninja-environment.json`에 있다. 작은 타일은 16×16, 나무 32×32, 연못 48×48다. 투명도가 있는 장식 PNG에서 투명 영역은 실제 alpha=0을 확인했다. 바닥은 중앙 pivot, 나무/장식은 아래 중앙 pivot을 사용한다. 낮의 노랑초록 초원·따뜻한 흙길·청록 물을 기준으로 삼는다.

## 검수 증거와 제한

- `Build/Downloads/Ninja-Inspection.png`: Idle/Walk/Attack/Hit/Dead 확대 접촉시트.
- `Build/Downloads/Katana-composite-inspect.png`, `Axe-composite-inspect.png`: 원본 몸+무기 셀의 중앙 정렬 확대검사.
- `Build/Downloads/Tileset*-grid.png`: 환경 원본 좌표 격자검사.
- Python/Pillow로 크기, alpha, 셀 경계, 프레임 해시를 검사했고 접촉시트를 직접 확인했다. 실제 Unity 렌더·움직임·프레임 타이밍·사용자 미감 승인을 대신하지 않는다.

## 트러블슈팅

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
|---|---|---|---|
| Tiny Swords 후보 검수 | CC0 구버전 Warrior Aseprite tag는 Idle/Run/Attack 계열뿐이고 Hit가 없으며 별도 라이선스 동봉 문서도 없음 | U1 필수 클립이 모두 있는 새 Ninja Adventure CharacterAnimated로 전환. Tiny Swords는 Assets에 반입하지 않음 | 팩 이름이나 멋진 공격 예시만으로 필수 동작 계약을 충족한다고 판단하지 않음 |
| Ninja 방향/프레임 해석 | sheet가 일반적인 방향별 행 구조와 반대 | 열=방향, 행=시간을 실제 자세로 확인하고 명시적 rect 사용 | 이름/크기만으로 slicing 방향을 추정하지 않음 |
| 몸/무기 셀 크기 차이 | 몸32와 무기64의 같은 normalized pivot이 발을 서로 다른 위치에 배치 | body .5/.25, weapon .5/.375로 같은 발 기준점 정렬 | 셀 크기에 맞춘 절대 픽셀 anchor가 필요함 |

## 3항목 진행 상황 체크포인트

1. 완료: 제작자 신규 원본·CC0 전문 확보, 최소 파일 반입, 프레임·방향·pivot·impact 정적 검수.
2. 구현 연결: 부모 작업에서 Unity importer·animation set·첫 야외 씬 연결.
3. 남은 검증: Unity import, 실제 Play/Windows build, 사용자 시각 승인 별도 확인.
