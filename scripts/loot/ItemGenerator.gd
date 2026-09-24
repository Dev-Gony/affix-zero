extends RefCounted
class_name ItemGenerator

const ITEM_RESOURCE_DIRECTORY: String = "res://resources/items/"

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


func _roll_rarity(current_floor: int, rebirth_count: int) -> RarityData:
	var boost: float = (current_floor - 1) * 0.015 + rebirth_count * 0.08
	var adjusted_weights: Array[float] = []
	var total_weight: float = 0.0
	for rarity: RarityData in _rarities:
		var weight: float
		if rarity.index == 0:
			weight = maxf(5.0, rarity.drop_weight / (1.0 + boost))
		else:
			weight = rarity.drop_weight * (1.0 + boost * rarity.index)
		adjusted_weights.append(weight)
		total_weight += weight
	var roll: float = randf() * total_weight
	var cursor: float = 0.0
	for index: int in _rarities.size():
		cursor += adjusted_weights[index]
		if roll <= cursor:
			return _rarities[index]
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
	for file_name: String in DirAccess.get_files_at(ITEM_RESOURCE_DIRECTORY):
		if not file_name.ends_with(".tres"):
			continue
		var resource: Resource = load(ITEM_RESOURCE_DIRECTORY + file_name)
		if resource is ItemBaseData:
			_item_bases.append(resource as ItemBaseData)
		elif resource is AffixData:
			_affixes.append(resource as AffixData)
		elif resource is RarityData:
			_rarities.append(resource as RarityData)
	_rarities.sort_custom(func(a: RarityData, b: RarityData) -> bool: return a.index < b.index)
