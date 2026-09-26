# 새 외부 에셋 반입 절차 v0.2

확인일: 2026-09-26. 새 Ninja Adventure 제작자 원본과 CC0 동봉 전문을 확보했고, 최소 11 PNG/라이선스/README를 반입했다. 상세 권리·SHA·프레임 검수는 [반입 기록](assets/ninja-adventure.md)에 있다. Unity slicing/Play/사용자 시각 승인은 별도 미완료 단계다.

## 1. 미채택 후보의 권리 구분

제작자 페이지: https://pixelfrog-assets.itch.io/tiny-swords

페이지는 현행 Free Pack/Enemy Pack의 수정·개인/상업 사용을 허용하면서 재배포/재판매/재포장을 제한한다. 별도로 `TS_old version_CC0 Licensed` 파일을 제공한다. **공개 GitHub 반입 대상으로 검토하는 것은 정확히 CC0 구버전이며, 현행 무료 팩에 그 권리를 임의 적용하지 않는다.**

Tiny Swords CC0 구버전 ZIP을 실제 검수한 결과 Warrior의 Hit 클립이 없어서 U1 계약에 미달했다. 현재 팩과 CC0 구버전의 권리를 혼합하지 않았으며 이 팩은 Assets에 반입하지 않았다. 자세한 내용은 [미채택 기록](assets/tiny-swords-rejected.md)에 있다. 대신 제작자 CC0 전문을 동봉한 신규 Ninja Adventure를 채택했다.

## 2. 반입 기록

사용할 팩마다 출처 URL, 정확한 파일명/버전, 확보일, 원본 SHA-256, 동봉 라이선스 원문과 공개 원본 재배포 근거를 `docs/assets/<pack>.md`에 적는다. 문서만 남길 과거 실패 아트와 현재 사용 중인 합법적 새 원본은 구분한다. 현재 사용 중인 새 팩은 실제 구현에 필요한 파일과 라이선스만 유지하고 불필요한 큰 원본 ZIP은 제외한다.

## 3. 이미지 검수

영웅/적 idle·walk·attack·hit·death 및 바닥부터 선택한다. dimensions, frame rect, 프레임 수, FPS, 기본 방향, 좌우 반전 허용, 발 pivot, 실제 타격 index, 투명 배경을 기록한다. 공격의 손/무기와 목표 위치가 겹치는지 실제 재생으로 확인한다. 서로 다른 rect라는 검사만으로 픽셀 내용의 동작을 보장하지 않는다.

## 4. Unity 연결

SpriteRenderer에 사용할 import 설정, pixels-per-unit, point filter, sprite mode, pivot을 검수 결과로 결정한다. `ActorAnimationSet`에 순서대로 배열을 구성하고 `sourceLicenseRecord`에 기록 경로를 지정한다. `.meta`를 함께 추적한다. Unity Project Dashboard의 hero/enemy/ground에 연결한 뒤 첫 씬을 만든다.

필수 클립이 없다면 그 팩이 U1 계약을 충족하지 않는다고 기록한다. 없는 사망 프레임을 있다고 표시하거나 기존 거절 이미지로 채우지 않는다. 필요하면 팩을 바꾸거나 결손 애니메이션 작업을 명시적으로 새 작업으로 잡는다.

## 현재 상태

원본 확보 DONE / 라이선스 파일·공개 재배포 근거 검수 DONE / 실제 프레임·pivot·impact 정적 확인 DONE / 원본 파일 무결성 PASS / Unity import BLOCKED_BEFORE_IMPORT_EXIT198 / 사용자 시각 승인 NOT_RUN. 캐릭터 방향열과 환경 rect는 JSON으로 기록하고 Tools/validate_reviewed_art.py로 원본 SHA·픽셀 내용·범위를 확인한다.
