# Illustrated equipment v2 — remaining 26 masters

Date: 2026-10-03

## Integration status

All 26 masters in this brief were integrated on 2026-10-03 from the verified
asset-only source commit `98ac9a3614f44bd0c3104e945dd091792285785a`
(tree `326a3705993045185dd691e8e65e69b6a7f5c444`) without merging,
cherry-picking or checking out that branch. Together with the three approved
representatives, all 29 runtime base icons now use illustrated sources.

The final `magic_sword` source is the Arcane Staff. Final regenerated
`dagger`, `leather_gloves` and `sandals` are clean transparent cutouts; the
latter two intentionally use a single readable item. `copper_ring` uses an
engraved copper signet without a gemstone. These are presentation choices only.
No socket, gem, rarity or gameplay state is baked into any source image.

The user approved the painted material-rendered direction represented by
`longsword`, `plate_armor`, and `battle_gloves`. Preserve those three.
This document is the exact cloud-art brief for the remaining 26 base IDs.

## Shared art contract

- Transparent square master, isolated item, no frame, text, badge or watermark.
- Cohesive dark-fantasy rendering: worn steel, brass/gold trim, burgundy or
  brown leather, controlled jewel colors, upper-left key light and readable
  shadowed volume.
- Strong silhouette and material blocks must survive 128 px, 64 px and the
  actual 37 px bag icon. Fine engraving is secondary.
- Center with roughly 8–10% breathing room. Paired wearables must show both
  pieces and anatomically readable orientation.
- Do not bake rarity colors, affixes, socket holes, socket counts, gems or runes
  into the painting. Those are runtime state and require a separate contract.
- Cloud transfer master path:
  `art-source/illustrated-equipment-v2/<base_id>.png`
- Tracked source target:
  `docs/art-source/illustrated-equipment-v2/<base_id>.png`
- Unity 128 px target:
  `Assets/Art/Interface/Resources/AffixUIVisual/Items/<base_id>.png`

## Exact remaining IDs

| Slot / tier | Base ID | Identity and material distinction | Required Unity target |
| --- | --- | --- | --- |
| Weapon / 1 | `dagger` | Compact broad steel dagger; short leaf-point blade, small brass guard, burgundy leather grip. Must not read as a scaled longsword. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/dagger.png` |
| Weapon / 8 | `axe` | One-handed battle axe with a forged crescent steel edge, reinforced poll, dark wood haft and leather wraps. Never a flat mallet. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/axe.png` |
| Weapon / 13 | `magic_sword` | Runtime identity is **Arcane Staff**: dark wood/brass shaft and a blue-violet faceted crystal held by a metal claw. It must not look like a sword. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/magic_sword.png` |
| Weapon / 20 | `divine_sword` | High-tier ceremonial sword distinct from `longsword`: pale polished blade, winged gold guard, ivory/burgundy grip and restrained radiant jewel accents. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/divine_sword.png` |
| Helmet / 1 | `leather_hat` | Padded brown/burgundy scout cap with stitched panels, ear/neck protection and a soft silhouette; no plate-metal crown shape. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/leather_hat.png` |
| Helmet / 5 | `iron_helm` | Plain practical iron nasal/barbute helmet with rivets, rolled rim and worn dark steel. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/iron_helm.png` |
| Helmet / 11 | `mithril_helm` | Lightweight silver-blue mithril closed helm with elegant ridges and narrow luminous highlights; distinct from heavy iron. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/mithril_helm.png` |
| Helmet / 18 | `dragon_helm` | Crimson-black scaled war helm with a draconic brow, swept horns and brass/obsidian reinforcement; helmet, not a crown. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/dragon_helm.png` |
| Armor / 1 | `cloth` | Layered blue/cream adventurer tunic or robe with visible folds, sash and reinforced seams. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/cloth.png` |
| Armor / 5 | `leather_armor` | Brown/burgundy boiled-leather cuirass with layered panels, straps, stitching and sparse studs; no polished breastplate. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/leather_armor.png` |
| Armor / 18 | `dragonscale` | Red-black overlapping scale cuirass with horned shoulders and obsidian/gold edging; visibly organic and distinct from `plate_armor`. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/dragonscale.png` |
| Gloves / 1 | `cloth_gloves` | Pair of cream/blue wrapped fabric gloves with five-finger hand structure, layered bindings and soft folds. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/cloth_gloves.png` |
| Gloves / 5 | `leather_gloves` | Pair of fitted brown/burgundy leather gloves with stitched fingers, reinforced palms and buckled cuffs. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/leather_gloves.png` |
| Gloves / 18 | `dragon_gloves` | Pair of articulated red-black scaled claw gauntlets with horn/talon accents and readable fingers; distinct from the approved steel battle gauntlets. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/dragon_gloves.png` |
| Boots / 1 | `sandals` | Pair of simple worn leather strap sandals with visible soles, toe/ankle straps and open construction. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/sandals.png` |
| Boots / 5 | `leather_boots` | Pair of sturdy brown calf boots with stitching, folded leather, straps and buckles. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/leather_boots.png` |
| Boots / 11 | `swift_boots` | Pair of light silver-blue ankle boots with flexible segmented panels and small wing-like fins; not a leather-boot color swap. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/swift_boots.png` |
| Boots / 18 | `gale_boots` | Pair of high-tier silver/cyan aerodynamic greaves with swept fins and restrained wind motifs; solid equipment, not a glow blob. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/gale_boots.png` |
| Ring / 1 | `copper_ring` | Broad hammered copper band with an asymmetric square carnelian setting and visible wear. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/copper_ring.png` |
| Ring / 5 | `silver_ring` | Interlaced silver band with a small blue teardrop sapphire and cool polished highlights. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/silver_ring.png` |
| Ring / 11 | `gold_ring` | Heavy gold signet/halo construction with a raised ruby crest and filigree shoulders. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/gold_ring.png` |
| Ring / 18 | `diamond_ring` | White-gold pronged crown setting with a large clear/cyan faceted diamond and a distinct tall silhouette. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/diamond_ring.png` |
| Amulet / 1 | `bone_necklace` | Rough cord necklace with asymmetric carved jaw/claw bones, knots and a primitive central charm. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/bone_necklace.png` |
| Amulet / 5 | `crystal_necklace` | Silver chain holding an elongated cyan crystal cluster with visible facets and small side shards. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/crystal_necklace.png` |
| Amulet / 11 | `ruby_necklace` | Ornate gold filigree collar pendant with a deep red ruby drop and small linked settings. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/ruby_necklace.png` |
| Amulet / 18 | `dragon_tear` | Unique violet/cyan tear gem cradled by a dragon-claw or wing setting on an obsidian/gold chain; not a simple polygon pendant. | `Assets/Art/Interface/Resources/AffixUIVisual/Items/dragon_tear.png` |

The 26 masters were reviewed in a generated 128/64/37 px sheet and in actual
720p/1080p Unity inventory frames. Technical and agent visual inspection pass;
final user visual acceptance remains pending.
