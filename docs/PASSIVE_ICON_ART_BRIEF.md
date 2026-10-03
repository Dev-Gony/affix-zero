# Painted passive icon art brief

Date: 2026-10-03

This is an art-readiness brief only. It does not replace any runtime texture,
change a talent rule, or claim user visual approval.

## Exact remaining pixel-style icons

`TalentPanel` displays each icon at 48 x 48 pixels. Direct inspection confirms
that the first three nodes still upscale 16 x 16 pixel assets, while the other
three nodes use 128 x 128 artwork.

| Node | Runtime meaning | Current resource | Current file |
| --- | --- | --- | --- |
| 격노 / Fury (`TalentId.Fury`) | +3 attack per rank, maximum rank 20; ranks 5 and 15 evolve automatic area damage and select ARC, QUAKE, or LANCE from the equipped weapon style | `AffixGenerated/PowerRune` | `Assets/Art/Lucifer/Resources/AffixGenerated/PowerRune.png` |
| 정밀 / Precision (`TalentId.Precision`) | +4 attack per rank, maximum rank 10; requires Fury rank 2; ranks 3 and 8 add +5 and +10 total critical chance | `AffixGenerated/PrecisionRune` | `Assets/Art/Lucifer/Resources/AffixGenerated/PrecisionRune.png` |
| 숙련 / Keystone (`TalentId.Keystone`) | +6 attack per rank, maximum rank 5; requires Precision rank 1; ranks 2 and 5 add 5 and 12 total armor penetration | `AffixGenerated/VeteranRune` | `Assets/Art/Lucifer/Resources/AffixGenerated/VeteranRune.png` |

The current images are respectively a tiny red flame/power mark, green target,
and pale shield. Their semantics are usable, but the pixel treatment and flat
symbolic shapes do not match the painted equipment set.

## Shared generation contract

- Generate a square, project-original painted ARPG talent symbol with a fully
  transparent background (`transparent_background=true`).
- Use a strong isolated silhouette, hand-painted metal/energy material,
  weathered edges, deep value separation, and restrained warm rim light. Match
  the illustrated-v2 equipment's dark steel, aged brass, burgundy and jewel
  accents without copying any external game asset.
- Keep roughly 8-10% transparent breathing room. The symbol must remain clear
  at the actual 48 x 48 node size and at 128 x 128 runtime resolution.
- Do not add a UI frame, square plate, opaque disc, text, letters, numbers,
  rank marks, rarity color, socket holes, gems, or rune-word text.
- Avoid flat vector geometry, pixel-art stair steps, generic mobile-app
  gradients, repeated silhouettes, and a plain recolor of another icon.
- Preserve the existing Unity `.meta` files when replacing runtime PNGs.
  Preserve cloud masters and provenance separately under
  `docs/art-source/passive-icons-v1/` before producing deterministic 128 x 128
  runtime versions at the exact current paths.

## Per-icon art direction

### 격노 / Fury — `PowerRune.png`

Paint a compact crimson battle-energy core wrapped by three asymmetric,
blade-like flame strokes and small aged-brass fragments. It should read as
controlled offensive fury and an expanding automatic strike, not as one
specific weapon, because the real skill becomes sword ARC, axe QUAKE, or staff
LANCE. Use hot orange-white at the core, deep crimson outer energy, dark steel
occlusion, and a few sharp sparks. Keep the outer silhouette irregular and
aggressive.

### 정밀 / Precision — `PrecisionRune.png`

Paint a fine steel needle or arrowhead crossing a broken, engraved brass
sighting ring with a tiny emerald focus glint. The focal point must read as
accuracy and critical timing at thumbnail size without becoming a flat modern
crosshair. Use narrow highlights, dark oxidized recesses, and one crisp central
alignment point.

### 숙련 / Keystone — `VeteranRune.png`

Paint a tempered steel chisel or spear tip driving through a cracked dark armor
plate shaped loosely like a keystone. Add restrained violet energy inside the
fracture and aged-gold edge inlay. It must read as mastery plus armor
penetration, not as a defensive shield. Keep the piercing diagonal and broken
plate silhouette unmistakable at 48 x 48.

## Required verification after generation

1. Inspect all three transparent masters directly, then inspect deterministic
   128, 64, 48 and 37 pixel previews together.
2. Confirm alpha does not touch the canvas border, no two output hashes match,
   and no opaque background or baked UI frame exists.
3. Capture the native TalentPanel at 1280 x 720 and 1920 x 1080. Confirm every
   node and the detail icon use the new resource, retain semantic distinction,
   and do not clip inside `FrameSlotSilver` or `FrameSlotGold`.

## Troubleshooting record

| 문제 발생 지점 | 원인 분석 | 해결 방법 및 적용된 코드 개념 | 배운 점 |
| --- | --- | --- | --- |
| 장비 원화와 특성 화면의 첫 세 아이콘 화풍이 불일치 | 실제 노드가 48 x 48 영역에서 16 x 16 픽셀 리소스를 확대하며, 후반 세 노드는 128 x 128 리소스를 사용함 | 코드의 `Resources.Load` 경로와 원본 PNG를 직접 대조하고, 세 노드의 실효과를 각각 다른 회화적 실루엣으로 고정 | 이름만 보고 추상 룬을 만들기보다 실제 효과와 실제 표시 크기를 함께 기준으로 삼아야 함 |
