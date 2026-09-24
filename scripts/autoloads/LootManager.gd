extends Node

signal item_dropped(item: Dictionary)

var _generator: ItemGenerator


func _ready() -> void:
	_generator = ItemGenerator.new()


func try_drop() -> Dictionary:
	var drop_chance: float = 12.0 + GameManager.floor * 0.4 + GameManager.rebirth_count * 2.0
	if randf() * 100.0 >= drop_chance:
		return {}
	var item: Dictionary = _generator.generate_item(GameManager.floor, GameManager.rebirth_count)
	if item.is_empty():
		return {}
	if not passes_loot_filter(item):
		return {}
	if not GameManager.add_inventory_item(item):
		return {}
	item_dropped.emit(item)
	return item


func passes_loot_filter(item: Dictionary) -> bool:
	return int(item.get("rarity_index", 0)) >= GameManager.loot_min_rarity_index


func drop_boss_reward() -> Dictionary:
	var item: Dictionary = _generator.generate_item(GameManager.floor + 3, GameManager.rebirth_count + 1)
	if not item.is_empty():
		item["boss_reward"] = true
		item["sell_value"] = maxi(int(item.get("sell_value", 0)), GameManager.floor * 10)
		if passes_loot_filter(item):
			if GameManager.add_inventory_item(item):
				item_dropped.emit(item)
				return item
		else:
			var filtered_gold: int = maxi(int(item.get("sell_value", 0)), GameManager.floor * 15)
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
