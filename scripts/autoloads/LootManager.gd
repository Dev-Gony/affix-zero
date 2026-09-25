extends Node

signal item_dropped(item: Dictionary)

var _generator: ItemGenerator


func _ready() -> void:
	_generator = ItemGenerator.new()


func drop_chance_percent(floor_number: int = GameManager.floor, rebirths: int = GameManager.rebirth_count) -> float:
	# High kill counts at x5 speed made even a 5-12% curve flood the screen.
	# Field equipment is now intentionally scarce; bosses remain the reliable gear event.
	return clampf(0.8 + maxf(0.0, floor_number - 1) * 0.012 + maxf(0.0, rebirths) * 0.08, 0.8, 2.0)


func roll_drop() -> Dictionary:
	var drop_chance: float = drop_chance_percent()
	if randf() * 100.0 >= drop_chance:
		return {}
	var item: Dictionary = _generator.generate_item(GameManager.floor, GameManager.rebirth_count)
	return item


func elite_drop_chance_percent(floor_number: int = GameManager.floor, rebirths: int = GameManager.rebirth_count) -> float:
	return clampf(18.0 + maxf(0.0, floor_number - 6) * 0.10 + maxf(0.0, rebirths) * 0.8, 18.0, 30.0)


func roll_elite_drop() -> Dictionary:
	if randf() * 100.0 >= elite_drop_chance_percent():
		return {}
	var item: Dictionary = _generator.generate_item(GameManager.floor + 2, GameManager.rebirth_count + 1)
	if not item.is_empty():
		item["elite_reward"] = true
	return item


func try_elite_drop() -> Dictionary:
	var item: Dictionary = roll_elite_drop()
	if item.is_empty() or not collect_item(item):
		return {}
	return item


func collect_item(item: Dictionary) -> bool:
	if item.is_empty() or not passes_loot_filter(item):
		return false
	if not GameManager.add_inventory_item(item):
		return false
	item_dropped.emit(item)
	return true


func try_drop() -> Dictionary:
	var item: Dictionary = roll_drop()
	if item.is_empty() or not collect_item(item):
		return {}
	return item


func passes_loot_filter(item: Dictionary) -> bool:
	return int(item.get("rarity_index", 0)) >= GameManager.loot_min_rarity_index


func drop_boss_reward() -> Dictionary:
	# Field legendaries are intentionally rare, so bosses get three independent rolls
	# and keep the highest rarity. Bosses feel rewarding without flooding normal combat.
	var item: Dictionary = {}
	for _roll: int in 3:
		var candidate: Dictionary = _generator.generate_item(GameManager.floor + 3, GameManager.rebirth_count + 1)
		if candidate.is_empty():
			continue
		if item.is_empty() or int(candidate.get("rarity_index", 0)) > int(item.get("rarity_index", 0)):
			item = candidate
	if not item.is_empty():
		item["boss_reward"] = true
		item["sell_value"] = maxi(GameManager.item_sell_value(item), GameManager.floor * 10)
		if passes_loot_filter(item):
			if GameManager.add_inventory_item(item):
				item_dropped.emit(item)
				return item
		else:
			var filtered_gold: int = maxi(GameManager.item_sell_value(item), GameManager.floor * 15)
			GameManager.add_gold(filtered_gold)
			return {"name": "필터 판매 %dG" % filtered_gold, "fallback_gold": filtered_gold, "filtered": true}
	var fallback_gold: int = maxi(50, GameManager.floor * 25)
	GameManager.add_gold(fallback_gold)
	return {"name": "%dG" % fallback_gold, "fallback_gold": fallback_gold}


func migrate_save_data(data: Dictionary) -> void:
	var inventory: Array = data.get("inventory", [])
	for item_value: Variant in inventory:
		if item_value is Dictionary:
			_ensure_item_icon(item_value as Dictionary)
	var equipment: Dictionary = data.get("equipment", {})
	for slot: Variant in equipment.keys():
		var item_value: Variant = equipment[slot]
		if item_value is Dictionary and not (item_value as Dictionary).is_empty():
			_ensure_item_icon(item_value as Dictionary)
	data["version"] = 2


func _ensure_item_icon(item: Dictionary) -> void:
	if item.has("icon_index"):
		return
	var icon_index: int = _generator.icon_index_for_base(String(item.get("base_id", "")))
	if icon_index >= 0:
		item["icon_index"] = icon_index
