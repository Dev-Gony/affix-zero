# 2026-10-03 - passive icon art readiness and socket proposal

The exact remaining pixel-style TalentPanel assets are `PowerRune.png` for
Fury/격노, `PrecisionRune.png` for Precision/정밀, and `VeteranRune.png` for
Keystone/숙련. All three are 16 x 16 and are displayed at 48 x 48; their exact
runtime semantics, paths and separate painted transparent-art prompts are in
`PASSIVE_ICON_ART_BRIEF.md`. No PNG or runtime code was changed in this
documentation-only pass.

`ITEM_SOCKET_REQUIREMENT.md` now contains a bounded recommended v1 contract for
eligibility, proposed capacity ranges, insertion/removal/salvage behavior and a
fail-closed v5 migration shape. Every new count and rule is explicitly marked
as proposed rather than user-agreed. No socket field, fake pip, rune, currency,
drop rule or gameplay behavior was implemented.

# 2026-10-03 - complete illustrated equipment set and verified talent framing

All 26 remaining flat placeholders were replaced with original illustrated
masters from asset-only commit `98ac9a3614f44bd0c3104e945dd091792285785a`
(tree `326a3705993045185dd691e8e65e69b6a7f5c444`). Only the exact 26 PNG blobs
were extracted; the source branch was not merged, cherry-picked or checked out.
The prior `longsword`, `plate_armor` and `battle_gloves` runtime hashes remain
unchanged. All 29 current base IDs now resolve to illustrated 128 x 128 icons.

The generator aspect-fits the new 26 masters with four pixels of outer padding,
keeps alpha off every canvas border, rejects duplicate output hashes and emits
both a 29-icon contact sheet and a 128/64/37 px readability sheet. The final
Arcane Staff, clean dagger, single leather glove, single sandal and gemless
engraved copper signet are intentional. No sockets, rarity state or gameplay
rules are painted into the art.

Build `66363a4ba5fa4fd1a991d9617ccaf2d4` passes CoreSmoke 321, static compile
0/0, Unity build/import 0/0, 720p/1080p native UI probes with 19 callbacks and
all 29 resources, player HP present/non-player HP absent, and exact-window OS
input for ten actions in 17.189 seconds. A final D-isolated speed/equipment
observe also passes with 29 kills, 37 callbacks, live equip, two-step salvage,
adjacent selection fallback, 720p bounds and schema 3 -> 4 migration. The
pre-existing TalentPanel WIP is now
included: it has authored frames and six distinct icons and was inspected at
both resolutions. The first empty-profile OS-input attempt failed closed before
input and remains preserved; pass2 used only a hash-verified copied D profile.

Current Library evidence: contact sheet
`libfile_af59bff5da448191ba99186f6b65708a`, 1080p equipment
`libfile_8b7dc6abc6808191b2de6ec8b5c31f8e`, and 1080p skills
`libfile_5b3ab34c48a88191a5c2d0be2a238908`. Final user visual acceptance remains
pending. Item sockets remain a documented product requirement with no approved
data/save/drop/forge contract, so no fake socket UI or rules were added.

# 2026-10-03 - remaining illustrated-item brief and socket requirement

The user approved the painted material-rendered direction of `longsword`,
`plate_armor`, and `battle_gloves`. The exact remaining 26 base IDs, tier
identity, material/silhouette distinctions, cloud source names and Unity target
paths are fixed in `ILLUSTRATED_EQUIPMENT_BATCH_V2.md`. Do not generate more
flat polygon icons or report the remaining placeholders as acceptable.

The user also requires item sockets in the product direction. Repository audit
found no data/save/drop/forge contract: existing documents explicitly mark
sockets/runes as unimplemented, and the old forge reference is layout-only.
`ITEM_SOCKET_REQUIREMENT.md` records required future decisions. Until that
contract is approved, do not invent counts, probabilities, effects, currencies
or fake socket pips, and do not bake sockets into base paintings.

# 2026-10-03 - illustrated equipment v2 representative pass

The user rejected the first 29-icon contact sheet despite its technical
one-to-one mapping. Its flat polygons, repeated/color-swapped forms and weak
material rendering did not meet the supplied Hero Siege references. Do not
reuse that local visual-quality PASS.

Only `longsword`, `plate_armor`, and `battle_gloves` now use the new
original illustrated direction. Their exact cloud-generated transparent
masters were transferred through Git commit
`5b4933ccb2b2bfad1158a26aa7917d561bdc60cf` without merging, cherry-picking,
or checking out its isolated branch. Originals and blob provenance are in
`docs/art-source/illustrated-equipment-v2/`; the existing generator only
downsamples these three to 128 px. At actual 37 px bag size, steel/brass/leather
separation and gauntlet finger anatomy remain readable, while fine engraving
necessarily reduces.

Inspected actual-UI evidence is tracked at
`docs/media/illustrated-equipment-v2/equipment-before-after-1080p.png`; the
128/64/37 px comparison is
`docs/media/illustrated-equipment-v2/actual-scale-preview.png`.

Final representative build GUID `642c56a9fd5e477e90d819f25a98c8ac`
passes static compile 0/0 and fresh D-isolated 720p/1080p UI probes with 19
callbacks, all 29 paths loaded, player HP present and non-player HP absent.
The first hidden-window 720 capture failed only at framebuffer `ReadPixels`
and remains preserved/excluded; normal-window reruns pass. The remaining 26
icons are rejected placeholders pending replacement after the three-item art
direction receives user approval. The paused TalentPanel/ReferenceUiProbe
visual WIP remains local and is not part of this art-direction conclusion.

# 2026-10-03 - 29 base icons and Hero Siege equipment dock

The current `feature/open-dungeon-loot-skills` branch now maps all 29 existing
base items to distinct original 128 x 128 illustrated icons and replaces the
earlier flat equipment layout with a full-height 39.0625%-wide right dock:
burgundy paper doll, dense 6 x 4 bag, weathered metal/red-corner frames, rarity
edge markers, and a floating black comparison card on the left. The compact
player HUD/skill dock remains; player HP remains; normal, elite, and guardian HP
bars remain absent. No item/stat/gameplay system was added. Read
`UI_ICON_POLISH.md`.

Final build GUID `a536a92cb1194263bfed35ec1344020b`: CoreSmoke 321,
static compile 0/0, Windows build/import 0/0, isolated speed/equipment/salvage
observe and separate-process restore PASS, 720/1080 UI probes PASS with all 29
icons and 19 callbacks each, and final-build OS input PASS for all ten actions.
Production primary/backup/lock hashes are unchanged. Four inspected PNGs were
saved to Library, including 720p/1080p equipment, 1080p battle, and the full
icon contact sheet. User visual approval, a deeper biome material pass, and a
real boss/boss-only HP treatment remain open; town, five tiers, gacha, and pets
remain deferred.

# 2026-10-03 - speed controls and equipment/skill UX

The current `feature/open-dungeon-loot-skills` branch adds visible session-only
`1x` / `2x` / `4x` controls, honest game/wall timing, a 0.65-second post-clear
transition, stronger speed-safe hit feedback, a three-column paper-doll / 24-slot
inventory / comparison equipment screen, and clearer six-branch talent effects.
Tooltips separate BASE from ROLLED values and derive roll bands from production
loot rules. Natural Staff drops, salvage with schema-v4 Gold provenance, and a
75-useful-rank point cap complete the sustained-progression loop. See
`SPEED_EQUIPMENT_SKILLS_UI.md`.

Final build GUID `2495dc6d61c74d0f96b45bea3c79cde9`: CoreSmoke 302 PASS,
static Unity source compile 0/0, Windows build/import 0/0, real 149.123s wall /
595.759s simulated `4x` production run PASS (431 kills, 17 clears, 44 natural
drops seen, 19 salvages, all three weapon styles, zero deaths/failures), exact
separate-process restore +2 kills, and 720/1080 UI probes with 19 callbacks each.
Final-build OS-level automated input passed all ten Start/equipment/talent/forge/
pause/resume checks in 14.069s against the exact native window. Human play and
user visual approval remain NOT RUN/pending. Town/hub, further named difficulty
tiers, gacha, and fully authored separate biome sets remain deferred.

Final 1080p Library evidence: battle/speed
`libfile_fd8257c232ec8191829d32d50b5ac219`, equipment comparison
`libfile_3e36a8f174188191bc235e3cc22728ec`, and talent evolution
`libfile_ff43eb54b5f4819196b263e1b4fc897b`. All creates succeeded; Windows
`os.setxattr` remains unavailable for the optional local metadata marker only.

# 2026-10-02 - procedural dungeon visual identities

The current `feature/open-dungeon-loot-skills` branch now gives each connected
layout an original readable runtime identity over the shared temple base:
Forge Citadel ember trenches/battlements/hearth/entrance seal, Sunken Archive
walkable canals/bridges/plinths, and Eclipse Sanctum ritual courts/spokes/well/
altars. The HUD exposes the identity and signature. These layers add no collider,
save field, external art, or commercial asset. See `DUNGEON_VISUAL_IDENTITIES.md`.

Final Unity 6000.3.24f1 build GUID
`d233c103846346e5bb032dcfd4f1c353` passes CoreSmoke 272, static compile and
Windows build 0/0, final 150.062s 1080p normal-drop play (94 kills, three clears,
identity/layout/pattern masks 7, minimum 20 decorations, zero identity colliders,
zero deaths/failures/safety restarts), separate-process restore +2 kills,
720/1080 framebuffer UI, and final-build OS-level automated input. Human play
and user visual approval remain NOT RUN.

Library evidence: Forge `libfile_6780674a96248191bfbc2df6a4c211b2`, Archive
`libfile_7b6de6b254c88191851e94c5d22c7c99`, Eclipse
`libfile_5cbbec0a729881918309580c34e6c6ad`. Exact titles were verified. The
prepared upload interface was unavailable, so direct batch-create fallback was
used; Windows `os.setxattr` remains unavailable for local metadata only.

# 2026-10-02 - player skill trajectories

Fury rank 5 now evolves the automatic area skill by saved weapon choice:
Sword ARC chains through unique LOS-valid targets with falloff, Axe QUAKE keeps
an immediate inner impact plus a 0.18-second 65% outer annulus, and Staff LANCE
pierces a narrow long lane. HUD and
Fury detail expose the route and tradeoff. No save field or external art was
added. See `PLAYER_SKILL_TRAJECTORIES.md`.

Final build GUID `7dff73dc3e884968beb5f7b3d1e063fd`: CoreSmoke 272, Unity
build 0/0, 150.068s normal-drop natural unlock/use (95 kills, three QUAKE casts,
six real trajectory hits including four delayed outer-wave hits), separate-process restore,
720/1080 UI, and safely targeted OS-level automated input all PASS. Library
gameplay evidence: QUAKE `libfile_b708eea1c8a88191954afcc9a7d76488` and ARC
`libfile_c449ce9ae33481919fcb8f9aaf963c55`.

The three dungeon layouts remain temple arrangements sharing one material and
prop family; they are not yet three finished biomes. The remaining visual gap
is recorded in `DUNGEON_VARIETY_DIFFICULTY.md`.

# 2026-10-02 - layout-specific elite attack mechanics

Branch `feature/open-dungeon-loot-skills` now gives each connected dungeon a
different resolved attack, not only different stats or colors: Ember radial
slam, Gallery dash, and Ritual piercing lane. See
`ELITE_ATTACK_PATTERNS.md` for exact timings, hit rules, evidence, and Library
IDs. The final validated Windows build GUID is
`56b6cfe08be34bfabbf669796f5ffb5e`; CoreSmoke 269, build 0/0, 150.056s
normal-drop run, separate-process restore, 720/1080 UI probes, and final-build
OS-level automated input all PASS. No save schema changed and no external or
commercial art was added.

# 2026-10-02 visible build evolution handoff

The active feature/open-dungeon-loot-skills branch now gives all six saved
talent branches two mechanically real evolution milestones without changing
the save schema. Critical chance, penetration, automatic area damage/radius/
target requirement/arming, and recovery trigger/heal/arming now evolve from
persisted ranks. The compact combat dock names the active skill form and the
talent detail shows current and next milestones. Read
BUILD_EVOLUTION_MILESTONES.md.

Final verification is CoreSmoke 269 PASS; Unity 6000.3.24f1 Windows build
GUID 2d211235839a4a348d357bae63677f07 with 0 errors / 0 warnings; final
720p/1080p framebuffer UI probes with four screens and 16 callbacks each; and
a 150.042-second normal-drop 1080p run with 83 kills, three clears, all three
layouts/patterns/guardians, active WIND recovery at 50%/25%, three recovery
casts, 106 saves, and zero deaths/failures. Separate-process restore plus two
continued kills also passed.

Library evidence: evolved combat
libfile_c3bd75f8af788191aeec9fa64e21ef79 and talent tree
libfile_8d50ff8e9f508191926dfe9eca4fa439. Both creates succeeded;
Windows local xattr attachment remains unavailable and does not affect Library
storage.

# 2026-10-02 layout-specific elite encounter handoff

`feature/open-dungeon-loot-skills` now gives Ember Bastion, Split Galleries,
and Ritual Crucible distinct elite/guardian combat profiles rather than only
different obstacle arrays. Colored procedural floor sigils and timing-correct
attack warnings identify the slow armored Bulwark, fast Stalker, and long-reach
Reaver families; the compact target bar shows their real names. No external or
commercial art was added. Read `ELITE_ENCOUNTER_IDENTITIES.md`.

Build GUID `e4db1b39184e4381980d2a5dc70171c8` passes CoreSmoke 267,
720p/1080p framebuffer and management regression, a 120.054-second normal-drop
run with 72 kills / 13 elites / three guardians / all three pattern bits and no
deaths, and separate-process restore. OS-level automated mouse/keyboard input
also passed all ten recorded interactions against the validated native window;
this is distinct from UI Toolkit callback coverage and is not human play.

# 2026-10-02 compact combat HUD handoff

The active `feature/open-dungeon-loot-skills` branch now replaces the previous
full-width gauge dashboard with a compact Hero Siege-informed combat layout:
top-left original hero portrait plus real HP/XP and cooldowns, centered current
target, upper-right region plus minimap, and a lower-left skill/action dock.
The implementation uses only project-native UI Toolkit styling and existing
original AFFIX: ZERO art; it does not copy or extract commercial assets. Read
`HERO_SIEGE_HUD_REDESIGN.md` for the exact reference mapping and evidence.

Final build GUID `6e217fe60e554e19ad071d542189c03e` passes the 720p and
1080p actual-framebuffer probes with four screens and 16 native callbacks at
each resolution. CoreSmoke is 267 PASS. A 120-second normal-drop run passed
with 75 kills, all three layouts/difficulties and disk round-trip, and the final
executable passed a separate-process restore and continued combat. Physical OS
input remains NOT RUN and user visual approval is pending.

# 2026-10-02 dungeon-variety and difficulty handoff

The same feature/open-dungeon-loot-skills branch now adds three materially
different connected layouts (EMBER BASTION, SPLIT GALLERIES, and
RITUAL CRUCIBLE), saved Scout/Veteran/Torment difficulty, explicit enemy /
reward / drop multipliers, same-base affix/value variance tests, a staged
75-rank XP curve, critical/kill/anticipation feedback, weapon-specific strike
rendering, generated runtime hit audio, kill chains, and an FX FULL / FX LOW
budget control. Classes remain deliberately deferred until they can have
separate animation, resource, and skill behavior instead of labels only.

Current verification is CoreSmoke 267 PASS; final Unity Windows build GUID
8fded397ea654f1db965eb326ff9c911 with 0 errors / 0 warnings; 720p final-build
regression PASS (120 seconds, 75 kills, three layouts and difficulties);
separate-process restore/continuation PASS; and the immediately preceding
1080p twenty-minute production-flow run PASS (868 kills, 36 clears, 102 natural
drops seen, 37 upgrades, 60/75 ranks, zero deaths/failures/safety restarts).
The only delta after that long run was correcting the displayed talent-point
notice; the final build and 720p regression include it. Read
DUNGEON_VARIETY_DIFFICULTY.md and the reports under
Build/Reports/variety-natural-final*.

Library delivery: evidence ZIP libfile_0dafb6e55b1c8191a6263b3b47c97761,
1080p AVI libfile_f3588b7d7fc08191928f3e5af293156a, Ember screenshot
libfile_283e7ab0b5188191b8cd409eb0d4b91d, Galleries screenshot
libfile_d6c2e12898388191813ba76d696d3c9e, and Crucible screenshot
libfile_25f167d61ff08191ad33fcf156ec144a. Library creation succeeded for all
five files. Local extended-attribute writeback is unavailable on this Windows
Python because os.setxattr is absent; this does not affect the uploaded files.

# 2026-10-02 connected-dungeon handoff

The active work is on `feature/open-dungeon-loot-skills`, based on
`origin/restart/unity-6` at `1d98100`. Read `OPEN_DUNGEON_LOOT_SKILLS.md` and
`STATUS.json` first. The existing Draft PR #19 and the dirty
`D:\github\affix-unity` checkout were not modified.

The current build contains a connected 72x32 temple dungeon, 24 persistent
roaming enemies, production-weight loot (six rarities, 29 bases, seven
generated slots plus the preserved Relic slot, ten affixes), an eight-slot
Stitch-derived equipment/comparison surface, the complete 75-rank six-node
tree, and automatic area/recovery skills. Original generated pixel atlases are
documented in `docs/assets/original-temple-combat-set.md`; no Hero Siege files,
tracing, or extracted commercial art are used.

Final verification: CoreSmoke 256 PASS; Unity Windows build PASS with zero
errors and zero warnings; 720p and 1080p technical full-clears PASS; and the
production-drop natural-progression run PASS for 1200.056 seconds with 935
kills, 38 clears, 43 collected items, 21 equipment upgrades/comparisons, all
75 talent ranks spent, three forge upgrades, and a verified separate-process
disk restore/continuation. The natural run used neither the survival fixture
nor the auto-hunt smoke flag. See
`Build/Reports/natural-20m-pass2/natural-progression-observe.json` and
`natural-progression-read.json`.

Physical OS input is explicitly NOT RUN: this managed desktop exposed no
enumerable AffixZero top-level window and `MainWindowHandle` stayed zero, so
user32 input could not be proven to target Unity. Native UI Toolkit callbacks
were exercised 484 times in the natural run, but that is not claimed as
physical-input coverage. Do not use the excluded stale-handle capture.

Library delivery IDs: evidence ZIP
`libfile_369b69b706d481919759563844993776`; exploration
`libfile_b4d684f57690819188f0bed766f49cff`; combat
`libfile_55fe0a0ba058819186dd5615008cbda0`; equipment
`libfile_96e56359eb708191bd16d0878b3990df`.

# Previous handoff — connected dungeon, loot, and auto skills

2026-10-02 작업 브랜치 `feature/open-dungeon-loot-skills`. 최신 구현/검증 기준은 `OPEN_DUNGEON_LOOT_SKILLS.md`다. 이전의 3구간·2체 재사용·빈 스킬 슬롯 설명은 역사 기록일 뿐 현재 구현이 아니다. 현재는 한 연결형 사원, 시작부터 24체 상주/배회, 실제 6등급·29 base·7 역사 슬롯+Relic·10옵션, 8칸 장비 비교, 6노드/75랭크, 자동 범위/회복 스킬이다. 720p 실제 완주 PASS와 1080p 실제 완주/캡처가 있으며 사용자 시각 승인은 아직 없다. Draft PR #19를 병합하지 말고 기존 dirty `D:\github\affix-unity`도 건드리지 않는다.

# 이전 인수인계 — Stitch UI 재구성 검증 완료

2026-09-26. `restart/unity-6`, draft PR #19, Unity6000.3.24f1 / Built-in2D 유지. 구현 커밋 `81c4b2aac57cad0cc9f8abc0faba10a943a6836e`. STATUS.json, PRODUCT_BRIEF.md, STITCH_REFERENCE_REFRESH.md를 우선 읽는다. 아래 이전 저장/자동사냥 단계는 당시 검증 기록이며 최신 빌드를 대체하지 않는다.

사용자 지시대로 Stitch 07–12를 확인하고 네이티브 전투 HUD·장비·특성·대장간의 배치와 정보 위계를 재구성했다. 13개 ZIP 반입 기록 중 07/09/10/11/12는 기존 추출 콘텐츠와 동일하다. 08은 HTML이 기존01과 같고 PNG byte만 다르다. 중복 자료도 기준으로 재확인했으며 내용이 새 기능을 자동 승인하는 것은 아니다. 사용자 목표는 Hero Siege 오마주 RPG + 자동 던전 사냥이며 탕탕특공대 요소는 제외한다.

최신 Windows 빌드 `e8115fc90b424d58a3fc868b8e140204`, 2026-09-26T07:39:41Z, 오류0/경고0. `docs/validation/ui-refresh/`에 실제720p/1080p UI·자동사냥·저장재실행 보고와 source fingerprint가 있다. Core202 PASS. Editor Play는 이번 UI 작업에서 다시 실행하지 않았다.

- UI 두 실행은 실제 첫 공격 후 검사용 임시 프로필에 장비를 공급해 네이티브 ClickEvent로 장착·비교·분노2/정밀1·강화1을 검사했다. 30→40→50→52공격,48→40골드. 주요 컨트롤 경계/비중첩 PASS, 8개 실제 framebuffer를 직접 열어 검수했다. 검사용 가방9칸은 자연 드랍 성과가 아니다. 사용자 저장 접근0; 물리 입력/사용자 시각 승인은 미완료.
- 새 빌드 별도 기본수치1080p 자동사냥: 실제1배속70.70초,3순환18처치4수거사망0,450XP144골드. 메뉴 중 사냥과 수동 정지/재개 PASS.
- 별도 디스크 시험 폴더에서 write/read 두 프로세스 PASS.6처치·장착/특성/강화 후52공격150XP40골드와 미수거검 저장, 동일복원/HUD52 확인 후 회수·새2처치로200XP56골드. 손상/미지원 복구는 이전 `validation/save/` 근거를 유지하며 이번에 반복하지 않았다.

`Build/Windows/AffixZero.exe`가 수정 실행본이다. 일반 실행은 실제 저장을 쓰며 입구 정지 상태로 시작한다. `-affixUiReferenceTest`는 항상 메모리 프로필이며 다른 시험flag와 같이 주면 probe가 거절한다. 새 probe/meta만 공개하고 원본 Stitch ZIP/HTML·제한된 캐릭터 원본은 ignored 영역에 유지했다.

다음은 별도 막힘·안전중단 후 재개와20분무조작/물리입력 검증이다. 그 다음 실제 능동스킬·전리품/빌드 선택을 보강한다. 여섯 빈 슬롯과 빈MP구체는 아직 없는 기능이며 영웅 선택·타운·펫·소켓은 후속범위다. 전체MVP완료로 보고하지 않는다. 이번작업은 UI표시 및 검증용저장격리 변경이며 전투수치/경로/실제보상은 변경하지 않았다.

기존 Ninja 관련 더티 파일/이미지/오래된 미추적 검증 파일은 그대로 남긴다. `git add .`, PR병합, forcepush, reset/clean, 사용자세이브삭제 금지.

# 이전 단계 — 로컬 저장 연결

갱신2026-09-26. restart/unity-6, Draft PR19, Unity6000.3.24f1/Built-in2D 유지. STATUS.json, PRODUCT_BRIEF.md, SAVE_PERSISTENCE.md를 먼저 읽는다.

최신 Windows빌드 GUID `baf9e73795d3447e9adc1d13daabe1d2`, 2026-09-26T07:00:29.9473535Z, 오류0/경고0. Core202 checks와 실제 Unity JsonUtility4검사 PASS. 저장을 사용하는 Windows720p 두 프로세스 write/read에서 성장·미수거 전리품의 동일 복원을 검증했다. write는 실제6처치 후 장착·특성·강화로52공격/150XP/40골드, read는동일복원후 미수거회수·새2처치로200XP/56골드다. 기록은 `docs/validation/save/`.

별도 실제 Windows 손상 fixture에서 백업 복구·손상 원본 보존, 둘 다 손상/미지원 버전의 덮어쓰기 차단 PASS. 최신1080p 임시프로필 자동사냥도3순환·18처치·4수거·사망0·XP450/G144 PASS. 검증 flag는 사용자 저장과 격리한다. 기존 profile을 지우거나 초기화하지 않는다.

저장 경로는 `%USERPROFILE%/AppData/LocalLow/Dev-Gony/AFFIX ZERO/profile-v1.json`. 성장 변경마다 저장하며 실패 시 사냥을 중단하고 HUD에 재시도를 표시한다. 읽기 실패는 새 프로필로 덮어쓰지 않는다. 재실행은 입구·정지 상태이며 진행 중 공격이나 오프라인 보상을 이어받지 않는다. IsPaused는 수동 정지뿐 아니라 자동사냥 중지/저장 실패도 유지하여 일시정지 버튼으로 우회하지 못하게 했다.

전체 MVP는 미완료다. 다음은 별도 막힌 경로·안전 중단 후 재개,20분 연속 실행,물리입력/사용자시각검수다. 아래 자동사냥보고는 저장추가전이력이며 최신빌드를대체하지않는다.

## 현재 기준

**주력은 적정 난이도에서 개입 없이 안정적으로 성장하는 자동사냥 방치형 게임이다.** 현재는 단일 사원 마당 안의 세 구간을 자동으로 순회한다. 적 두 개체를 구간마다 재사용하고, BFS 경로 탐색으로 통행 불가 모서리와 석재 장애물을 피한다. 탐색 → 대상 선택/접근 → 자동 공격 → 자동 수거 → 다음 구간 → 입구 귀환/반복을 연결했다. 세 개의 별도 방이나 절차 생성 던전은 아니다. 다음 우선순위는 중단 후 재개 검증, 저장·장시간 실행이다. 관리 화면 추가를 먼저 늘리지 않는다. AUTO_HUNT_MVP.md와 IDLE_COMBAT_CONTRACT.md를 따른다.

사용자 Stitch ZIP 총 7개의 화면 구성이 기준이다. 00..03은 영웅 선택·전투·타운·장비, 새 04는 특성 전용, 05는 펫, 06은 대장간이다. 로컬 원본은 ignored `Build/Reference/Stitch/00..06`, 해시와 크기는 `docs/assets/stitch-reference-manifest.json`에 있다. 원격 그림이나 HTML의 예시 재화·확률·온라인 상태를 실제 게임 데이터로 옮기지 않는다. 무료 에셋과 직접 제작만 사용한다.

Unity 6000.3.24f1 / C# / Built-in 2D 유지. D:/Program Files/Unity 6000.3.24f1/Editor/Unity.exe 설치·라이선스가 작동한다. 엔진·패키지·렌더러 변경이나 재설치 요구는 필요하지 않았다. 탕탕특공대와 3지선다, 거절된 Ninja·초원·코너 HUD는 현행 방향이 아니다.

## 현재 기능

HeroSiegeEncounter에서 무료 Soldier/Orc와 생성 사원 방·석재 장애물로 자동 순환한다. 각 고유 처치는 25 XP·8골드·특성 1포인트를 지급한다. 최초 처치의 잿불 강철검 외에 순환 완료마다 사원의 강철검을 드랍하고 입구에서 자동 회수한 뒤 다음 순환을 시작한다. 변형은 순환 번호에 따른 제한된 세 가지 순서이며 무작위 어픽스 시스템 전체가 아니다.

일반 피격은 체력과 짧은 flash만 반영하며 공격·이동을 취소하지 않는다. 사망할 때 공격을 취소한다. 연속 두 번 사망하면 자동 중단하고, 가방 포화 시 보류 전리품을 보존한 채 멈춘다. 장비·특성·대장간을 열어도 사냥하며 수동 정지만 전투를 멈춘다. 연속 사망·포화 안전 검사는 PASS다. 별도 경로 막힘과 중단 후 재개 검증은 남아 있다.

- 장비 화면: 왼쪽 장착/능력치/가방, 오른쪽 특성 요약과 아이템 비교. 버리기는 같은 아이템 두 클릭 확인이며 재화 보상이 없다.
- 전용 특성 화면: 왼쪽 분노2 → 정밀1 → 숙련1 노드, 오른쪽 효과·조건·투자. 노드 선택 자체는 포인트를 쓰지 않는다. 단계별 피해는 +3/+4/+6, 초기화는 사용분만 반환한다.
- 대장간: 현재 장착 무기의 강화 전후 수치를 보여주고 골드를 소비한다. 최대 +3, 단계마다 피해 +2, 비용 8/16/24. 확정 강화이며 실패·파괴는 없다. 부족한 골드나 최대 단계에서는 상태를 바꾸지 않는다. 강화는 무기 객체에 남아 교체해도 보존된다.
- `TotalGold`는 강화 비용을 뺀 가용 잔액이다. `Experience/Gold`는 해당 전투 보상이며 세션의 `TotalExperience/TotalGold`와 구분한다.
- `FirstEncounter.Screen`으로 Equipment/Talents/Forge는 하나만 열지만 사냥은 계속된다. `IsPaused`는 수동 정지 상태만 읽는다. 메뉴 전환이나 닫기는 수동 정지를 해제하지 않는다.
- 장비·가방·강화·특성·XP·골드는 장면 재시작에 보존되지만 현재는 프로세스 종료 후에도 로컬 저장으로 복원된다. 공격 시작 시 피해를 고정하므로 변경은 다음 스윙부터 적용된다.

## 이전 자동사냥 빌드 검증 (저장 추가 전)

최종 Windows 자동사냥 실행은 [720p 보고](validation/autohunt/player-720.json)와 [1080p 보고](validation/autohunt/player-1080.json) 모두 PASS다. 실행 ID는 각각 `26b0296dea004e06a40c4a3cfab8caa8`(70.9288초), `23dbebd8a24b4397bca82dce83b771bd`(70.6474초), Build GUID는 `6c03e8d2bc6e457abc31582911701d3d`다. 두 실행 모두 실제 1배속으로 3구간·3순환·18처치·4개 수거, 사망 0회·재시도 0회, XP 450·골드 144를 확인했다. 영웅 HP 120/공격력 30·적 HP 54/공격력 6을 바꾸지 않았다. 메뉴 중 사냥과 수동 정지/재개, UI 콜백 7회·실제 framebuffer 6개도 검사했다. [720p 전투](media/autohunt/720-combat.png) · [1080p 관리 중 사냥](media/autohunt/1080-management.png). 로컬 원본은 `Build/Reports/autohunt-diagnostic-{720,1080}/auto-hunt-smoke.json`이다.

최종 빌드는 `Build/Reports/windows-build.json`의 2026-09-26T06:36:34.0607289Z Succeeded, 오류 0/경고 0이다. 정상 두 해상도는 위 최종 빌드의 근거다. 안전 중단은 아래 명시한 직전 빌드에서 검사했으며 safety-build/source-fingerprint 보고서로 구분한다. 앞선 `autohunt-fixed-720` 후보 실행은 이력으로 구분한다. 이전 `docs/validation/management/`는 관리 기능 시제품의 역사적 근거다. 당시 빌드에는 디스크 저장이 없었다. 현재 저장 검증은 위 내용을 우선하며 20분·OS 입력·사용자 승인은 남아 있다.

연속 피격으로 공격이 막힌 이전 실패(run `86b8d28820274fabb557631bb0c16821`)는 85.2초·0순환·재시도 5회를 기록했다. 실패를 삭제하지 않고 `Build/Reports/autohunt-stagger-failure/`에 보존한다. 원인과 계약은 IDLE_COMBAT_CONTRACT.md를 참조한다.

## 남은 범위와 다음 순서

완료된 정상 두 해상도·연속 실패·포화 중단 검사에 더해 경로 막힘과 중단 후 재개를 검증한다. 로컬 저장/복원은 새 빌드에서 검증했고 20분 연속 실행은 남아 있다. 최종 정상 3순환 성공만으로 저장·장시간 검사를 완료 처리하지 않는다. 영웅 선택·타운·펫, 대형 스킬 계열, 마나/능동 스킬, 소켓·재련·분해와 여러 장비 슬롯은 후속 범위다. 배경은 단일 그림이고 이동/시야는 그리드이며 벽 가림·실시간 조명은 없다. 손의 무기는 sprite에 포함되어 교체·강화 외형이 바뀌지 않는다.

공개 저장소에는 Zerie 원본이 없다. `Assets/LocalLicensed`는 Git 제외다. 공식 무료 ZIP → `Tools/Local/Import-FreeCharacters.ps1` → `HeroSiegeArtSetup.Build`의 검증 반입 절차를 사용한다. 누락·해시 불일치면 씬 생성 전에 중단한다. `LOCAL_WORKFLOW.md`를 참조한다.

기존 미추적 Ninja 리소스·오래된 검증 PNG와 `docs/assets/ninja-adventure.md` 변경은 이번 작업과 무관하므로 그대로 둔다. `git add .` 금지. Godot/E0/V0/PR18 코드·아트 복원, 사용자 자료 삭제, 자동 stash, force push, PR 병합은 하지 않는다.


안전 검사의 빌드 GUID는 `0a94c21a725b4d3a9a01e269067fa423`다. 이후 변경은 관리 창 위 피해 숫자 숨김과 검사 보고의 최종 성장 수치 추가이며 전투·안전 중단 코드는 동일하다. [연속 사망 안전 검사](validation/autohunt/safety-deaths.json)는 run `f508cbfae00844128c310426a8abbbc3`, 약 29.4초에 실제 적 공격으로 두 번 사망·한 번 재시도 후 자동 중단, 4.01초 중단 유지 PASS다. 이 격리 검사만 영웅 공격력을 1로 주입했으며 정상 밸런스 증거가 아니다. [가방 포화 안전 검사](validation/autohunt/safety-bag-full.json)는 run `a8fd4677ac5741bb90de2caeccef30fa`, 시험용 무기 24개로 가방을 채운 뒤 실제 첫 처치의 전리품을 보류 상태로 보존하고 중단했다. 약 7.65초, 골드 8·XP 25·기존 아이템 24개 유지, 중단 4.01초 PASS다. 두 검사는 중단 후 재개나 20분 안정성을 증명하지 않는다.
# 2026-10-03 probe-isolation and enemy-HP follow-up

The current follow-up is documented in `PROBE_SAVE_ISOLATION.md`. All nine
standalone probes now fail closed without an explicit isolated D: save path;
the speed/UI stress observe+read and final exact-PID physical-input regressions
PASS while the user's production profile directory remains byte-identical.
CoreSmoke is 320 PASS, static compile is 0/0, and final Windows player GUID is
`3360c8b3dafa484d9a1afed8985cc4ec`.

The first unsafe stress diagnostic had already rewritten the production
primary envelope before this fix. The exact path, post-incident hash, preserved
backup hash, and limits of the earlier partial comparison are recorded without
claiming full semantic identity. Do not restore or delete either generation
without user approval.

Ordinary, elite, and guardian HP UI is absent; only player HP remains. Final
720p/1080p player captures each verified 24 active enemies, no enemy-attached
Canvas/TextMesh/health child, no non-hero health-like UI Toolkit element, four
actual framebuffers, and 19 native callbacks. The current game has no genuine
boss class, so no boss bar is shown.
