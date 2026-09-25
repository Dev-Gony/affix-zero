# 외부 아트 반입 절차 / ART-02

## 현재 요청할 원본

Tiny Swords 제작자 페이지의 다운로드 목록에서 정확히 **TS_old version_CC0 Licensed**를 확보해 ZIP 그대로 전달한다. **Tiny Swords (Free Pack)**은 같은 라이선스라고 가정하지 않는다. 사용자에게 결제를 요구하지 않는다. CC0 구버전이 더 이상 제공되지 않으면 다른 공개 저장소/미러에서 주워오지 말고 현재 제작자 배포 상태를 다시 확인한다.

새 원본 파일은 아직 이 저장소에 들어오지 않았다. 기존 이미지는 이미 폐기 대상이며 새 파일을 못 구했다는 이유로 복원하지 않는다.

## 검사 순서

1. ZIP 파일명, 원본 출처, 확보일, SHA-256, 동봉 라이선스를 기록한다. 압축 경로 탈출/절대 경로를 거부하고 이미지 메타데이터를 검사한다.
2. 신규 파일 SHA-256을 기록하고 삭제 전 Git blob들과 중복을 검사한다. 폐기 자산 재반입 금지.
3. 영웅/근접 적 1종을 골라 idle, walk, attack, hit, death 실제 행/열/프레임을 접촉시트와 재생으로 검사한다. 페이지의 '애니메이션 제공'만으로 전 동작 완비라고 기록하지 않는다.
4. 무기가 이미 포함된 프레임인지 확인한다. 프레임별 발 pivot/손 위치/impact/recovery를 연결한다. 정지 원화 fallback은 없다.
5. 해당 팩의 야외 타일로 작은 필드만 만든다. UI는 종이/밝은 패널과 충분한 대비를 쓰며 기존 검정 패널/어두운 배경을 재활용하지 않는다.
6. 보존된 규칙에서 이동/타격/수집을 순서대로 연결한다. 1대1 → 소규모 무리 → 장비 비교 순서다. 기존 배율/밸런스/저장 소유권을 변경하려면 별도 기록한다.
7. 실제 게임 캡처와 자동 판정 결과를 구분해 사용자에게 보여준다. 아직 검수하지 않은 확장을 완료로 쓰지 않는다.

## 신규 manifest 필수 필드

id, author, source_url, package_filename, package_sha256, license_id, license_file, redistribution_allowed, original_path, imported_path, file_sha256, frame_width, frame_height, frame_count, directions, fps, pivot, includes_weapon, impact_frame, loop, validation_status.

아이템/스킬의 데이터 ID를 아트 파일명과 분리한다. 아트 교체가 아이템 삭제나 소유권 초기화로 이어지지 않도록 한다.
