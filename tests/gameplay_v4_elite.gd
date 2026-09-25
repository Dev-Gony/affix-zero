extends Node

var _failures: Array[String] = []
var _checks: int = 0


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		push_error("Gameplay V4 elite tests require --affix-test-mode")
		get_tree().quit(2)
		return
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	_checks += 1
	if not condition:
		_failures.append(message)
		push_error("GAMEPLAY_V4_ELITE: " + message)


func _run() -> void:
	_check(BattleManager.elite_spawn_chance_percent(5, 0) == 0.0, "Elites do not spawn before floor 6")
	_check(is_equal_approx(BattleManager.elite_spawn_chance_percent(6, 0), 12.0), "Elite waves start at a readable 12 percent chance")
	_check(BattleManager.elite_spawn_chance_percent(60, 5) <= 35.0, "Elite wave chance is capped at 35 percent")
	_check(BattleManager.elite_spawn_chance_percent(20, 3) > BattleManager.elite_spawn_chance_percent(20, 0), "Rebirth raises elite encounter frequency")
	_check(EnemyAI.ELITE_AFFIXES.size() == 3, "V4 ships three distinct elite affixes")
	_check(LootManager.elite_drop_chance_percent(6, 0) >= 18.0, "Elite bonus item roll starts above normal field-drop frequency")
	_check(LootManager.elite_drop_chance_percent(200, 20) <= 30.0, "Elite bonus item roll stays capped to protect loot scarcity")

	var target := Node2D.new()
	add_child(target)
	var slime: EnemyData = load("res://resources/enemies/slime.tres")
	var boss_data: EnemyData = load("res://resources/enemies/demon_lord.tres")
	var bounds := Rect2(0, 0, 500, 500)

	var normal := EnemyAI.new()
	add_child(normal)
	normal.setup(slime, 20, target, bounds)
	var normal_hp: float = normal.max_hp
	var normal_attack: float = normal.attack
	var normal_defense: float = normal.defense
	var normal_speed: float = normal.move_speed
	var normal_cooldown: float = normal.attack_cooldown
	var normal_xp: int = normal.xp_reward
	var normal_gold: int = normal.gold_reward

	var brutal := EnemyAI.new()
	add_child(brutal)
	brutal.setup(slime, 20, target, bounds, "brutal")
	_check(brutal.is_elite and brutal.elite_affix_id == "brutal", "Brutal affix marks the enemy as elite")
	_check(brutal.max_hp > normal_hp and brutal.attack > normal_attack, "Brutal elite has meaningfully higher HP and attack")
	_check(brutal.xp_reward > normal_xp * 2 and brutal.gold_reward > normal_gold * 2, "Elites grant materially higher XP and gold")
	_check(brutal.elite_title().contains("폭군"), "Elite title exposes the affix for player-facing messaging")

	var swift := EnemyAI.new()
	add_child(swift)
	swift.setup(slime, 20, target, bounds, "swift")
	_check(swift.move_speed > normal_speed and swift.attack_cooldown < normal_cooldown, "Swift elite moves and attacks faster than the base enemy")

	var bulwark := EnemyAI.new()
	add_child(bulwark)
	bulwark.setup(slime, 20, target, bounds, "bulwark")
	_check(bulwark.max_hp > brutal.max_hp and bulwark.defense > normal_defense * 1.5, "Bulwark elite is the durable archetype")

	var boss := EnemyAI.new()
	add_child(boss)
	boss.setup(boss_data, 20, target, bounds, "brutal")
	_check(not boss.is_elite and boss.elite_affix_id.is_empty(), "Bosses cannot accidentally receive normal elite affixes")

	_finish()


func _finish() -> void:
	var output_dir: String = ProjectSettings.globalize_path("res://build/gameplay-v4")
	DirAccess.make_dir_recursive_absolute(output_dir)
	var report := FileAccess.open(output_dir.path_join("elite.json"), FileAccess.WRITE)
	if report != null:
		report.store_string(JSON.stringify({
			"checks": _checks,
			"failures": _failures,
			"status": "PASS" if _failures.is_empty() else "FAIL",
			"scope": "elite encounter frequency, three affixes, stat identity, reward scaling, boss exclusion"
		}, "	"))
		report.close()
	print("GAMEPLAY_V4_ELITE %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
