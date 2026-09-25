extends Node

var _failures: Array[String] = []
var _checks: int = 0


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		push_error("Gameplay V3 balance tests require --affix-test-mode")
		get_tree().quit(2)
		return
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	_checks += 1
	if not condition:
		_failures.append(message)
		push_error("GAMEPLAY_V3: " + message)


func _run() -> void:
	_check(not SaveManager.persistence_enabled, "Test mode protects the developer save")

	var old_midgame_drop: float = 12.0 + 31.0 * 0.4 + 5.0 * 2.0
	var new_midgame_drop: float = LootManager.drop_chance_percent(31, 5)
	_check(new_midgame_drop < old_midgame_drop, "Mid-game item drop frequency is lower than the old curve")
	_check(new_midgame_drop <= 12.0 and new_midgame_drop >= 5.0, "Field item drop chance stays inside the intended 5-12 percent band")

	var generator := ItemGenerator.new()
	var probabilities: Dictionary = generator.rarity_probabilities(31, 5)
	var legend_probability: float = float(probabilities.get("legend", 1.0))
	var unique_probability: float = float(probabilities.get("unique", 1.0))
	_check(legend_probability < 0.015, "Legendary items are below 1.5 percent of generated field items at floor 31 / rebirth 5")
	_check(unique_probability > legend_probability, "Unique remains more common than legendary")

	var floor10: Dictionary = EnemyAI.floor_scaling(10)
	var floor30: Dictionary = EnemyAI.floor_scaling(30)
	var boss30: Dictionary = EnemyAI.floor_scaling(30, true)
	_check(float(floor30.get("hp", 0.0)) > float(floor10.get("hp", 0.0)) * 3.0, "Enemy HP scaling accelerates meaningfully into late floors")
	_check(float(floor30.get("attack", 0.0)) > float(floor10.get("attack", 0.0)) * 3.0, "Enemy attack scaling accelerates meaningfully into late floors")
	_check(float(boss30.get("hp", 0.0)) > float(floor30.get("hp", 0.0)) * 2.0, "Bosses receive a separate HP multiplier")

	GameManager.reset_run_progress()
	if not GameManager.unlocked_classes.has("sage"):
		GameManager.unlocked_classes.append("sage")
	var main_scene: PackedScene = load("res://scenes/main.tscn")
	var main: Node = main_scene.instantiate()
	get_tree().root.add_child(main)
	await get_tree().process_frame
	await get_tree().process_frame
	var ui: GameUI = main.get_node("UILayer/GameUI")

	var warrior: ClassData = load("res://resources/classes/warrior.tres")
	var sage: ClassData = load("res://resources/classes/sage.tres")
	GameManager.select_class(warrior)
	await get_tree().process_frame
	ui._refresh_equipment()
	var first_portrait: TextureRect = ui._equipment_row.find_child("ClassPortrait", true, false) as TextureRect
	_check(first_portrait != null and first_portrait.texture != null, "Equipment screen has a named class portrait")

	GameManager.select_class(sage)
	await get_tree().process_frame
	var sage_portrait: TextureRect = ui._equipment_row.find_child("ClassPortrait", true, false) as TextureRect
	_check(GameManager.selected_class == "sage", "Sage becomes the active class")
	_check(sage_portrait != null and sage_portrait.texture != null and sage_portrait.texture.resource_path.ends_with("sage.png"), "Equipment portrait refreshes immediately to the active sage class")

	var definitions: Array = GameManager.class_skill_definitions("sage")
	_check(not definitions.is_empty(), "Sage has class-specific skill definitions")
	if not definitions.is_empty():
		var first: Dictionary = definitions[0]
		var skill_id: String = String(first.get("id", ""))
		var base_cost: int = int(first.get("base_cost", 100))
		var cost_step: int = int(first.get("cost_step", 80))
		GameManager.gold = 100000
		var expected_cost: int = GameManager.skill_upgrade_total_cost(skill_id, base_cost, cost_step, 10)
		var before_gold: int = GameManager.gold
		var bought: int = GameManager.buy_skill_levels(skill_id, base_cost, cost_step, 10)
		_check(bought == 10, "Bulk skill upgrade buys ten levels in one action")
		_check(GameManager.class_skill_level(skill_id) == 10, "Bulk skill upgrade applies all purchased levels")
		_check(GameManager.gold == before_gold - expected_cost, "Bulk skill upgrade charges the arithmetic-series total exactly")
		var before_max_level: int = GameManager.class_skill_level(skill_id)
		GameManager.gold = 25000
		var expected_max: int = GameManager.max_affordable_skill_upgrades(skill_id, base_cost, cost_step)
		var bought_max: int = GameManager.buy_skill_levels(skill_id, base_cost, cost_step, 0)
		_check(bought_max == expected_max and bought_max > 0, "MAX skill upgrade spends only what is affordable")
		_check(GameManager.class_skill_level(skill_id) == before_max_level + bought_max, "MAX skill upgrade applies the computed number of levels")
		_check(GameManager.class_skill_level(skill_id) <= GameManager.CLASS_SKILL_MAX_LEVEL, "Bulk upgrading never exceeds the class skill level cap")

	GameManager.floor = 30
	ui._refresh_stats()
	_check(ui._stats_label.text.contains("현재 층 위협도"), "Info panel exposes enemy threat scaling to the player")

	main.queue_free()
	await get_tree().process_frame
	_finish()


func _finish() -> void:
	get_tree().paused = false
	SaveManager.set_persistence_enabled(false)
	var output_dir: String = ProjectSettings.globalize_path("res://build/gameplay-v3")
	DirAccess.make_dir_recursive_absolute(output_dir)
	var report := FileAccess.open(output_dir.path_join("balance.json"), FileAccess.WRITE)
	if report != null:
		report.store_string(JSON.stringify({
			"checks": _checks,
			"failures": _failures,
			"status": "PASS" if _failures.is_empty() else "FAIL",
			"scope": "class portrait, bulk skills, loot rarity/drop frequency, late-floor enemy scaling"
		}, "\t"))
		report.close()
	print("GAMEPLAY_V3_BALANCE %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
