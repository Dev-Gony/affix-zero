# 로컬 저장 검증

동일 Windows build `baf9e73795d3447e9adc1d13daabe1d2`의 결과다.

- `player-write.json`: 실제 여섯 처치 후 장착·특성·강화와 미수거 전리품을 저장하고 종료했다.
- `player-read.json`: 새 프로세스에서 변경 전 상태 전체가 동일함을 확인하고, 전리품 수거와 새 두 처치만 반영했다. 이미 저장한 보상 토큰은 거절했다.
- `backup-result.json` / `backup-player.json`: 새 시험 폴더에 손상 primary와 정상 backup을 구성했다. 백업 상태를 복원하고 사냥을 재개했다. 손상 원본이 별도 파일에 같은 bytes로 보존되는지 외부에서 확인했다.
- `both-corrupt-result.json`, `future-result.json`: 시작 시 저장 불러오기가 차단되는 것이 기대 결과다. 대응 `*-player.json`의 FAIL은 해당 조건에서 정상 프로필로 진행할 수 없다는 검사 응답이다. 외부 검사는 예상한 차단·비정상 종료와 두 파일의 불변 hash를 확인하여 PASS로 기록했다. 정상 플레이 PASS와 혼동하지 않는다.
- `unity-serialization.json`: 설치된 Unity에서 실제 JsonUtility 새 프로필/미수거/장착강화 왕복과 미래 버전 판별을 검사했다.
- `autohunt-1080.json`: 저장 연결 후 기본 자동사냥 회귀. 이 flag는 임시 메모리 프로필이므로 저장 부하 검사가 아니다.

Core202 checks는 상태 복원34개·파일 저장42개를 포함한다. `.NET` 직렬화/파일 검증과 실제 Unity/Windows 기록을 구분한다. `source-fingerprint.json`은 빌드 시점 소스 bytes, `windows-build.json`은 실제 빌드, `screenshots.json`은1080p framebuffer 무손실 변환이다. 사용자 저장 파일과 원본 에셋은 포함하지 않는다.

20분 실행, 저장 쓰기 실패 후 HUD 재시도 버튼의 실제 입력, 강제 OS 종료/전원 차단 실험, 물리 입력과 사용자 시각 승인은 미완료다. Core에서는 쓰기 실패 시 파일 보존을 검증했다.
