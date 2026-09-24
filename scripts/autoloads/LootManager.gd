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
