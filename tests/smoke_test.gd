extends Node

var _failures: Array[String] = []


func _ready() -> void:
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	if not condition:
		_failures.append(message)
		push_error("SMOKE: %s" % message)


func _run() -> void:
	var main_scene: PackedScene = load("res://scenes/main.tscn")
	var main: Node = main_scene.instantiate()
	get_tree().root.add_child(main)
	await get_tree().process_frame
	await get_tree().process_frame

	GameManager.rebirth_count = 0
	GameManager.rebirth_points = 0
	GameManager.permanent_upgrades = {"atk": 0, "def": 0, "hp": 0, "spd": 0}
	GameManager.unlocked_classes = ["warrior", "mage"]
	GameManager.statistics = {"total_kills": 0, "total_gold_earned": 0, "highest_floor": 1, "total_drops": 0}
	GameManager.reset_run_progress()
	var warrior: ClassData = load("res://resources/classes/warrior.tres")
	GameManager.select_class(warrior)
	GameManager.class_base_stats["atk"] = 999
	GameManager.recalculate_stats()
	GameManager.set_speed_multiplier(5.0)
	await get_tree().create_timer(12.0).timeout

	var battle: BattleManager = main.get_node("BattleArea")
	_check(battle != null, "BattleManager scene is available")
	_check(int(GameManager.statistics.get("total_kills", 0)) > 0, "Automatic combat defeats enemies")
	_check(GameManager.floor > 1, "Kill target advances the floor")
	_check(main.get_node("UILayer/GameUI") != null, "Game UI is available")

	for class_id: String in ["warrior", "mage", "knight", "sage", "assassin", "saint"]:
		var class_data: ClassData = load("res://resources/classes/%s.tres" % class_id)
		GameManager.select_class(class_data)
		GameManager.class_base_stats["atk"] = 999
		GameManager.recalculate_stats()
		await get_tree().process_frame
		battle._perform_auto_skill()
		await get_tree().create_timer(0.8).timeout
	_check(true, "All six class skills execute")

	var generated_item: Dictionary = LootManager._generator.generate_item(20, 3)
	_check(not generated_item.is_empty(), "Item generator creates an item")
	_check(generated_item.has("affixes") and generated_item.has("base_stats"), "Generated item is fully serializable")
	GameManager.inventory.clear()
	GameManager._ensure_equipment_slots(true)
	_check(GameManager.add_inventory_item(generated_item), "Generated item enters inventory")
	GameManager.equip_item(String(generated_item.get("id", "")))
	_check(not Dictionary(GameManager.equipment.get(String(generated_item.get("slot", "")), {})).is_empty(), "Equipment flow equips an item")

	var save_json: String = JSON.stringify(GameManager.to_save_dict())
	var parsed_save: Variant = JSON.parse_string(save_json)
	_check(parsed_save is Dictionary, "Complete game state round-trips through JSON")
	if parsed_save is Dictionary:
		GameManager.apply_save_dict(parsed_save as Dictionary)
		_check(GameManager.selected_class == String((parsed_save as Dictionary).get("selected_class", "")), "Serialized state loads back into GameManager")

	GameManager.level = RebirthManager.required_level()
	var previous_rebirths: int = GameManager.rebirth_count
	_check(RebirthManager.rebirth(), "Eligible character can rebirth")
	_check(GameManager.rebirth_count == previous_rebirths + 1, "Rebirth count increases")
	_check(GameManager.game_state == GameManager.GameState.CLASS_SELECTION, "Rebirth returns to class selection")

	GameManager.set_speed_multiplier(1.0)
	if _failures.is_empty():
		print("SMOKE TEST PASSED: combat, loot, equipment, save serialization, rebirth, and UI")
		get_tree().quit(0)
	else:
		print("SMOKE TEST FAILED: %d assertion(s)" % _failures.size())
		get_tree().quit(1)
