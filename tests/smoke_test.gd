extends Node

var _failures: Array[String] = []


func _ready() -> void:
	# Rebirth saves immediately in normal play. Disable persistence so this test never
	# overwrites a developer's real user://save.json file.
	SaveManager.set_persistence_enabled(false)
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	if not condition:
		_failures.append(message)
		push_error("SMOKE: %s" % message)


func _run() -> void:
	seed(20260924)
	var main_scene: PackedScene = load("res://scenes/main.tscn")
	var main: Node = main_scene.instantiate()
	get_tree().root.add_child(main)
	await get_tree().process_frame
	await get_tree().process_frame
	var battle: BattleManager = main.get_node("BattleArea")

	GameManager.rebirth_count = 0
	GameManager.rebirth_points = 0
	GameManager.permanent_upgrades = {"atk": 0, "def": 0, "hp": 0, "spd": 0}
	GameManager.unlocked_classes = ["warrior", "mage"]
	GameManager.statistics = {"total_kills": 0, "total_gold_earned": 0, "highest_floor": 1, "total_drops": 0}
	GameManager.reset_run_progress()
	var warrior: ClassData = load("res://resources/classes/warrior.tres")
	GameManager.select_class(warrior)
	await get_tree().process_frame
	_check(BattleManager.WORLD_RECT.has_point(battle.player.position), "Player begins inside the scrolling hunt world")
	_check(not battle._enemies.is_empty(), "Selecting a class immediately spawns a visible enemy wave")
	var player_start: Vector2 = battle.player.position
	await get_tree().create_timer(0.35).timeout
	_check(battle.player.position.distance_to(player_start) > 0.5, "Auto-hunt moves the player toward enemies")
	for spawned_enemy: EnemyAI in battle._enemies:
		_check(BattleManager.WORLD_RECT.has_point(spawned_enemy.global_position), "Spawned enemies begin inside the scrolling world bounds")
	var animation_target: EnemyAI = battle._nearest_enemy()
	animation_target.global_position = battle.player.global_position + Vector2(30, 0)
	battle._perform_auto_attack()
	_check(battle.player._motion_kind == "attack", "Automatic attacks trigger the player combat animation")
	var expected_facing: Vector2 = battle.player.global_position.direction_to(animation_target.global_position)
	_check(battle.player._facing.dot(expected_facing) > 0.99, "Player attack motion faces the selected enemy")
	_check(animation_target._hit_flash_left > 0.0, "Enemy hits trigger readable impact feedback")
	var contact_enemy: EnemyAI = battle._enemies[0] if battle._enemies[0] != animation_target else battle._enemies[1]
	contact_enemy.global_position = battle.player.global_position + Vector2(contact_enemy.radius + 8.0, 0)
	contact_enemy._spawn_reveal_left = 0.0
	contact_enemy._attack_time_left = 0.0
	contact_enemy._process(0.01)
	_check(contact_enemy._attack_windup_left > 0.0, "Enemy contact attacks show a warning windup")
	GameManager.class_base_stats["atk"] = 999
	GameManager.recalculate_stats()
	GameManager.set_speed_multiplier(5.0)
	await get_tree().create_timer(12.0).timeout

	_check(battle != null, "BattleManager scene is available")
	_check(battle._enemy_resources.size() == 8, "All eight enemy resources load")
	_check(battle._boss_resource != null and battle._boss_resource.behavior == "boss", "Boss enemy resource is registered")
	_check(not battle.is_boss_floor(9) and battle.is_boss_floor(10) and battle.is_boss_floor(20), "Every tenth floor is a boss milestone")
	var enemy_behaviors: Dictionary = {}
	for enemy_resource: EnemyData in battle._enemy_resources:
		enemy_behaviors[enemy_resource.behavior] = true
	_check(enemy_behaviors.size() == 8, "Every enemy type has a distinct resource-driven movement behavior")
	_check(int(GameManager.statistics.get("total_kills", 0)) > 0, "Automatic combat defeats enemies")
	_check(GameManager.floor > 1, "Kill target advances the floor")
	_check(main.get_node("UILayer/GameUI") != null, "Game UI is available")
	_check(AudioManager.has_complete_audio_bank(), "Authored or procedural audio covers every BGM and SFX channel")
	var game_ui: GameUI = main.get_node("UILayer/GameUI")
	_check(game_ui._class_selection._grid.get_child_count() == 6, "Class selection renders all six cards")
	_check(game_ui._equipment_row.get_child_count() == 9, "Equipment paper doll renders seven slots, a class portrait, and a combat summary")
	var equipment_slot_count: int = 0
	for equipment_cell: Node in game_ui._equipment_row.get_children():
		if equipment_cell.has_meta("equipment_slot"):
			equipment_slot_count += 1
	_check(equipment_slot_count == 7, "Equipment paper doll keeps all seven functional equipment slots")
	_check(not game_ui._management_open and not game_ui._management_window.visible, "Management windows stay closed while combat is the primary view")
	game_ui._toggle_management(1)
	_check(game_ui._management_open and game_ui._management_window.visible and game_ui._main_tabs.current_tab == 1, "Inventory dock button opens the focused inventory window")
	game_ui._toggle_management(1)
	_check(not game_ui._management_open and not game_ui._management_window.visible, "Pressing the active dock button closes the management window")

	for class_id: String in ["warrior", "mage", "knight", "sage", "assassin", "saint"]:
		var class_data: ClassData = load("res://resources/classes/%s.tres" % class_id)
		_check(class_data != null, "%s class resource loads" % class_id)
		GameManager.select_class(class_data)
		GameManager.class_base_stats["atk"] = 999
		GameManager.recalculate_stats()
		await get_tree().process_frame
		var visuals_before: int = battle.effects._rings.size() + battle.effects._lines.size() + battle.projectiles_root.get_child_count()
		battle._perform_auto_skill()
		var visuals_after: int = battle.effects._rings.size() + battle.effects._lines.size() + battle.projectiles_root.get_child_count()
		_check(visuals_after > visuals_before, "%s skill creates a visual effect" % class_id)
		await get_tree().create_timer(0.2).timeout

	var generated_item: Dictionary = LootManager._generator.generate_item(20, 3)
	_check(LootManager._generator._item_bases.size() == 29, "All 29 item bases load")
	var item_icon_indices: Dictionary = {}
	for item_base: ItemBaseData in LootManager._generator._item_bases:
		item_icon_indices[item_base.icon_index] = true
	_check(item_icon_indices.size() == 29 and not item_icon_indices.has(5), "All item bases map to distinct atlas cells while preserving the empty cell")
	_check(LootManager._generator._affixes.size() == 10, "All ten affixes load")
	_check(LootManager._generator._rarities.size() == 5, "All five rarities load")
	_check(not generated_item.is_empty(), "Item generator creates an item")
	_check(generated_item.has("affixes") and generated_item.has("base_stats") and generated_item.has("icon_index"), "Generated item is fully serializable")
	var legacy_item: Dictionary = generated_item.duplicate(true)
	legacy_item.erase("icon_index")
	var legacy_save: Dictionary = {"version": 1, "inventory": [legacy_item], "equipment": {}}
	LootManager.migrate_save_data(legacy_save)
	var migrated_inventory: Array = legacy_save.get("inventory", [])
	_check(int(legacy_save.get("version", 0)) == 2 and not migrated_inventory.is_empty() and (migrated_inventory[0] as Dictionary).has("icon_index"), "Legacy saves migrate base-item icons without losing inventory")
	GameManager.inventory.clear()
	GameManager._ensure_equipment_slots(true)
	_check(GameManager.add_inventory_item(generated_item), "Generated item enters inventory")
	var locked_item_id: String = String(generated_item.get("id", ""))
	_check(GameManager.toggle_item_lock(locked_item_id), "Inventory items can be locked")
	var gold_before_locked_sale: int = GameManager.gold
	GameManager.sell_item(locked_item_id)
	_check(GameManager._find_inventory_index(locked_item_id) >= 0 and GameManager.gold == gold_before_locked_sale, "Locked items are protected from direct selling")
	_check(not GameManager.toggle_item_lock(locked_item_id), "Locked items can be unlocked")
	_check(GameManager.INVENTORY_CAPACITY == 60, "Loot-heavy progression provides a sixty-slot inventory")
	_check(game_ui._inventory_grid.columns == 6, "Inventory uses a readable six-column scrollable grid")
	_check(game_ui._inventory_grid.get_child_count() == GameManager.INVENTORY_CAPACITY, "Inventory renders every available loot slot")
	_check(game_ui._inventory_grid.get_child(0).get_child_count() >= 2, "Loot slots show text rarity and item-level badges")
	GameManager.inventory.clear()
	var boss_reward: Dictionary = LootManager.drop_boss_reward()
	_check(not boss_reward.is_empty(), "Boss clear always grants an item or fallback gold reward")
	if bool(boss_reward.get("boss_reward", false)):
		_check(GameManager.inventory.size() == 1, "Boss item reward enters inventory")
		_check(int(boss_reward.get("item_level", 0)) >= GameManager.floor, "Boss reward is generated above the current floor baseline")
	var rendered_icon: AtlasTexture = (game_ui._inventory_grid.get_child(0) as Button).icon as AtlasTexture
	_check(rendered_icon != null and rendered_icon.atlas == GameUI.ITEM_BASE_ATLAS, "Inventory renders the generated base-item icon atlas")
	battle.effects.clear_effects()
	battle.effects.show_drop(battle.player.global_position, generated_item)
	_check(battle.effects._loot_icons.size() == 1, "Loot drops display their base-item icon in the arena")
	var equip_event := InputEventMouseButton.new()
	equip_event.button_index = MOUSE_BUTTON_LEFT
	equip_event.pressed = true
	equip_event.double_click = true
	game_ui._on_inventory_slot_input(equip_event, String(generated_item.get("id", "")))
	_check(not Dictionary(GameManager.equipment.get(String(generated_item.get("slot", "")), {})).is_empty(), "Equipment flow equips an item")
	var comparison_item: Dictionary = generated_item.duplicate(true)
	var comparison_stats: Dictionary = comparison_item.get("base_stats", {})
	var comparison_stat: String = String(comparison_stats.keys()[0])
	comparison_stats[comparison_stat] = float(comparison_stats[comparison_stat]) + 5.0
	comparison_item["base_stats"] = comparison_stats
	_check(game_ui._comparison_text(comparison_item).contains("▲"), "Inventory comparison shows an explicit upgrade delta")

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

	GameManager.rebirth_count = 10
	GameManager.unlocked_classes = ["warrior", "mage"]
	RebirthManager.sync_unlocked_classes()
	_check(GameManager.unlocked_classes.size() == 6, "Class resources drive all rebirth unlocks")

	GameManager.set_speed_multiplier(1.0)
	main.queue_free()
	await get_tree().process_frame
	if _failures.is_empty():
		print("SMOKE TEST PASSED: combat, loot, equipment, save serialization, rebirth, UI, and audio")
		get_tree().quit(0)
	else:
		print("SMOKE TEST FAILED: %d assertion(s)" % _failures.size())
		get_tree().quit(1)
