extends RefCounted
class_name ItemGenerator

const ITEM_BASE_RESOURCE_PATHS: Array[String] = [
	"res://resources/items/weapon_dagger.tres",
	"res://resources/items/weapon_longsword.tres",
	"res://resources/items/weapon_axe.tres",
	"res://resources/items/weapon_magic_sword.tres",
	"res://resources/items/weapon_divine_sword.tres",
	"res://resources/items/helmet_leather_hat.tres",
	"res://resources/items/helmet_iron_helm.tres",
	"res://resources/items/helmet_mithril_helm.tres",
	"res://resources/items/helmet_dragon_helm.tres",
	"res://resources/items/armor_cloth.tres",
	"res://resources/items/armor_leather.tres",
	"res://resources/items/armor_plate.tres",
	"res://resources/items/armor_dragonscale.tres",
	"res://resources/items/gloves_cloth.tres",
	"res://resources/items/gloves_leather.tres",
	"res://resources/items/gloves_battle.tres",
	"res://resources/items/gloves_dragon.tres",
	"res://resources/items/boots_sandals.tres",
	"res://resources/items/boots_leather.tres",
	"res://resources/items/boots_swift.tres",
	"res://resources/items/boots_gale.tres",
	"res://resources/items/ring_copper.tres",
	"res://resources/items/ring_silver.tres",
	"res://resources/items/ring_gold.tres",
	"res://resources/items/ring_diamond.tres",
	"res://resources/items/amulet_bone.tres",
	"res://resources/items/amulet_crystal.tres",
	"res://resources/items/amulet_ruby.tres",
	"res://resources/items/amulet_dragon_tear.tres",
]
const AFFIX_RESOURCE_PATHS: Array[String] = [
	"res://resources/items/affix_atk.tres",
	"res://resources/items/affix_def.tres",
	"res://resources/items/affix_hp.tres",
	"res://resources/items/affix_mp.tres",
	"res://resources/items/affix_spd.tres",
	"res://resources/items/affix_crit.tres",
	"res://resources/items/affix_vamp.tres",
	"res://resources/items/affix_xp.tres",
	"res://resources/items/affix_gold.tres",
	"res://resources/items/affix_pen.tres",
]
const RARITY_RESOURCE_PATHS: Array[String] = [
	"res://resources/items/rarity_normal.tres",
	"res://resources/items/rarity_magic.tres",
	"res://resources/items/rarity_rare.tres",
	"res://resources/items/rarity_unique.tres",
	"res://resources/items/rarity_legend.tres",
]

var _item_bases: Array[ItemBaseData] = []
var _affixes: Array[AffixData] = []
var _rarities: Array[RarityData] = []


func _init() -> void:
	_load_balance_resources()


func generate_item(current_floor: int, rebirth_count: int) -> Dictionary:
	if _item_bases.is_empty() or _rarities.is_empty():
		return {}
	var rarity: RarityData = _roll_rarity(current_floor, rebirth_count)
	var eligible_bases: Array[ItemBaseData] = []
	for item_base: ItemBaseData in _item_bases:
		if item_base.tier <= current_floor:
			eligible_bases.append(item_base)
	if eligible_bases.is_empty():
		return {}
	var item_base: ItemBaseData = eligible_bases.pick_random()
	var floor_multiplier: float = 1.0 + (current_floor - 1) * 0.15
	var scaled_base_stats: Dictionary = {}
	for stat_name: Variant in item_base.base_stats.keys():
		var value: float = float(item_base.base_stats[stat_name]) * floor_multiplier * rarity.stat_multiplier
		scaled_base_stats[String(stat_name)] = snappedf(value, 0.01) if String(stat_name) == "SPD" else roundi(value)

	var rolled_affixes: Array[Dictionary] = _roll_affixes(rarity.max_affixes, current_floor)
	var item_name: String = item_base.display_name
	if not rolled_affixes.is_empty():
		item_name = "%s %s" % [String(rolled_affixes[0].get("name", "")), item_name]
	return {
		"id": "%d-%d" % [Time.get_ticks_usec(), randi()],
		"base_id": item_base.id,
		"base_name": item_base.display_name,
		"icon_index": item_base.icon_index,
		"name": item_name,
		"slot": item_base.slot,
		"item_level": current_floor,
		"rarity_id": rarity.id,
		"rarity_name": rarity.display_name,
		"rarity_index": rarity.index,
		"rarity_color": rarity.color.to_html(false),
		"base_stats": scaled_base_stats,
		"affixes": rolled_affixes,
		"sell_value": roundi((5.0 + current_floor * 2.0) * (rarity.index + 1) * 0.8),
	}


func icon_index_for_base(base_id: String) -> int:
	for item_base: ItemBaseData in _item_bases:
		if item_base.id == base_id:
			return item_base.icon_index
	return -1


func rarity_probabilities(current_floor: int, rebirth_count: int) -> Dictionary:
	var progression: float = clampf(maxf(0.0, current_floor - 1) * 0.004 + maxf(0.0, rebirth_count) * 0.02, 0.0, 0.6)
	var adjusted_weights: Array[float] = []
	var total_weight: float = 0.0
	for rarity: RarityData in _rarities:
		var weight: float = rarity.drop_weight
		match rarity.index:
			0:
				weight *= maxf(0.78, 1.0 - progression * 0.25)
			1:
				weight *= 1.0
			2:
				weight *= 1.0 + progression * 0.40
			3:
				weight *= 0.35 + progression * 0.25
			4:
				# Field legendary drops should feel shocking, not routine.
				weight *= 0.05 + progression * 0.05
		adjusted_weights.append(maxf(0.0, weight))
		total_weight += maxf(0.0, weight)

	var probabilities: Dictionary = {}
	if total_weight <= 0.0:
		return probabilities
	for index: int in _rarities.size():
		probabilities[_rarities[index].id] = adjusted_weights[index] / total_weight
	return probabilities


func _roll_rarity(current_floor: int, rebirth_count: int) -> RarityData:
	var probabilities: Dictionary = rarity_probabilities(current_floor, rebirth_count)
	var roll: float = randf()
	var cursor: float = 0.0
	for rarity: RarityData in _rarities:
		cursor += float(probabilities.get(rarity.id, 0.0))
		if roll <= cursor:
			return rarity
	return _rarities.back()


func _roll_affixes(max_affixes: int, current_floor: int) -> Array[Dictionary]:
	var results: Array[Dictionary] = []
	if max_affixes <= 0 or _affixes.is_empty():
		return results
	var count: int = randi_range(1, max_affixes)
	var candidates: Array[AffixData] = _affixes.duplicate()
	candidates.shuffle()
	var affix_scale: float = 1.0 + (current_floor - 1) * 0.03
	for index: int in mini(count, candidates.size()):
		var affix: AffixData = candidates[index]
		var value: float = randf_range(affix.min_value, affix.max_value) * affix_scale
		value = snappedf(value, 0.01) if affix.decimal_value else roundf(value)
		results.append({
			"id": affix.id,
			"name": affix.display_name,
			"stat": affix.stat,
			"value": value,
			"decimal": affix.decimal_value,
		})
	return results


func _load_balance_resources() -> void:
	_item_bases.clear()
	_affixes.clear()
	_rarities.clear()
	for resource_path: String in ITEM_BASE_RESOURCE_PATHS:
		var resource: Resource = load(resource_path)
		if resource is ItemBaseData:
			_item_bases.append(resource as ItemBaseData)
	for resource_path: String in AFFIX_RESOURCE_PATHS:
		var resource: Resource = load(resource_path)
		if resource is AffixData:
			_affixes.append(resource as AffixData)
	for resource_path: String in RARITY_RESOURCE_PATHS:
		var resource: Resource = load(resource_path)
		if resource is RarityData:
			_rarities.append(resource as RarityData)
	if _item_bases.size() != ITEM_BASE_RESOURCE_PATHS.size() or _affixes.size() != AFFIX_RESOURCE_PATHS.size() or _rarities.size() != RARITY_RESOURCE_PATHS.size():
		push_error("Loot resource catalog incomplete: bases %d/%d, affixes %d/%d, rarities %d/%d" % [
			_item_bases.size(), ITEM_BASE_RESOURCE_PATHS.size(), _affixes.size(), AFFIX_RESOURCE_PATHS.size(), _rarities.size(), RARITY_RESOURCE_PATHS.size()
		])
	_rarities.sort_custom(func(a: RarityData, b: RarityData) -> bool: return a.index < b.index)
