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
	if item.is_empty() or not GameManager.add_inventory_item(item):
		return {}
	item_dropped.emit(item)
	return item


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
