# AFFIX: ZERO

자동사냥을 중심으로 캐주얼 슈팅·로그라이크·핵앤슬래시·액션 RPG를 결합하는 새 Unity 프로젝트입니다.

**개발선:** restart/unity-6 · [Draft PR #19](https://github.com/Dev-Gony/affix-zero/pull/19) · Unity 6000.3.24f1 · C# · Built-in 2D.

## 현재 구현

새 CC0 Ninja Adventure 원본의 실제 걷기·공격·피격·사망 프레임과 Katana/Axe를 반입했습니다. 첫 영웅과 적은 같은 신규 캐릭터를 공유하고 무기·진영 색으로 구분합니다. 밝은 초원 씬 생성, 자동 접근·공격, 몸/무기 타격 동기화, HP HUD, 처치 보상 1회, 재시작을 연결했습니다. 폐기한 Godot 코드와 이미지는 사용하지 않습니다.

코어 **56 checks PASS**, 설치 Unity API 참조 C# 정적 컴파일 **오류0/경고0**입니다. **실제 Unity import·Play·Windows 빌드는 미검증**입니다. 사용자는 Hub에서 에디터 설치 중이며, 발견한 설치본의 실행 시도는 라이선스 오류198로 import 이전 중단됐습니다. 첫 빌드 완료 상태가 아닙니다.

## 설치 완료 후

프로젝트 루트를 승인 버전 Unity에서 연 뒤 AFFIX → Setup → Import Reviewed Art and Create Encounter로 검수 원본을 slice하고 첫 씬을 생성합니다. 기존 씬은 덮어쓰지 않습니다. 실행 검사와 현재 인수인계는 [HANDOFF](docs/HANDOFF.md), 자세한 절차는 [LOCAL_WORKFLOW](docs/LOCAL_WORKFLOW.md)에 있습니다.

원본 프레임 미리보기는 python Tools/preview_reviewed_art.py로 Build/Reports/art-preview.html에 만듭니다. 이는 Unity 실행화면이 아닙니다.

## 문서

- [제품 기획](docs/PRODUCT_BRIEF.md) · [로드맵](docs/ROADMAP.md) · [구조](docs/ARCHITECTURE.md)
- [신규 아트 검수](docs/assets/ninja-adventure.md) · [아트 방향](docs/ART_DIRECTION.md) · [반입 원칙](docs/ASSET_INTAKE.md)
- [현재 상태](docs/STATUS.json) · [검증 기록](docs/VERIFICATION.json) · [트러블슈팅](docs/TROUBLESHOOTING.md)
- [문서 목차](docs/README.md) · [텍스트 전용 과거 회고](docs/history/LEGACY_RETROSPECTIVE.md)

Assets/**/*.meta는 추적하고 Library, Build, 원본 ZIP/검사 임시 출력은 제외합니다. 옛 폴더·세이브·stash·Git history 삭제와 PR 병합은 수행하지 않았습니다.