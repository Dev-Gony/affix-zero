extends Node

var _failures: Array[String] = []
var _checks: int = 0


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		push_error("Gameplay V4 boss-pattern tests require --affix-test-mode")
		get_tree().quit(2)
		return
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	_checks += 1
	if not condition:
		_failures.append(message)
		push_error("GAMEPLAY_V4_BOSS: " + message)


func _run() -> void:
	_check(BattleManager.BOSS_PATTERN_WARNING >= 1.30, "Boss telegraphs give base-speed auto-hunt enough reaction time")
	_check(BattleManager.BOSS_PATTERN_INTERVAL >= 5.0, "Boss patterns leave meaningful normal-combat windows")
	_check(BattleManager.boss_pattern_radius("ground_slam") > BattleManager.boss_pattern_radius("doom_mark"), "Ground slam has the larger danger radius")
	_check(BattleManager.boss_pattern_damage_multiplier("ground_slam") <= 1.30, "Ground slam adds pattern pressure without extreme raw damage")
	_check(BattleManager.boss_pattern_damage_multiplier("doom_mark") <= 1.15, "Doom mark remains a readable punishment rather than a one-shot multiplier")
	_check(GameManager.enemy_maximum_hit_ratio(true) <= 0.60, "Boss pattern damage still inherits the global 60 percent max-hit cap")
	_check(BattleManager.BOSS_RETRY_KILLS == 5, "Failed boss attempts require only five normal kills before retry")
	_check(BattleManager.boss_retry_progress_for_floor(49) == 52, "Floor 49 retry progress starts five kills before its 57-kill threshold")

	var arena := Rect2(100, 100, 500, 360)
	var hazard := Vector2(350, 280)
	var player_start := Vector2(360, 280)
	var slam_target: Vector2 = BattleManager.compute_boss_evade_target(player_start, hazard, BattleManager.BOSS_SLAM_RADIUS, arena)
	var mark_target: Vector2 = BattleManager.compute_boss_evade_target(player_start, player_start, BattleManager.BOSS_MARK_RADIUS, arena)
	_check(arena.has_point(slam_target), "Computed slam evade target stays inside the combat room")
	_check(slam_target.distance_to(hazard) > BattleManager.BOSS_SLAM_RADIUS, "Computed slam evade target exits the danger radius")
	_check(arena.has_point(mark_target), "Computed mark evade target stays inside the combat room")
	_check(mark_target.distance_to(player_start) > BattleManager.BOSS_MARK_RADIUS, "Computed doom-mark evade target exits the locked danger radius")

	var target := Node2D.new()
	add_child(target)
	target.global_position = Vector2(300, 240)
	var boss_data: EnemyData = load("res://resources/enemies/demon_lord.tres")
	var boss := EnemyAI.new()
	add_child(boss)
	boss.global_position = Vector2(240, 240)
	boss.setup(boss_data, 50, target, arena)
	_check(boss.behavior == "boss", "Demon Lord remains the boss behavior")
	boss.set_special_casting(true)
	_check(boss.is_special_casting(), "Boss AI can be paused during a telegraphed special pattern")
	boss.set_special_casting(false)
	_check(not boss.is_special_casting(), "Boss AI resumes after the special pattern resolves")

	var effect := EffectLayer.new()
	add_child(effect)
	effect.show_boss_telegraph(Vector2(200, 200), 80.0, BattleManager.BOSS_PATTERN_WARNING, Color("ff5b61"), "테스트")
	_check(effect._telegraphs.size() == 1, "Boss telegraph is registered in the effect layer")
	if effect._telegraphs.size() == 1:
		var telegraph: Dictionary = effect._telegraphs[0]
		_check(is_equal_approx(float(telegraph.get("radius", 0.0)), 80.0), "Boss telegraph preserves its gameplay radius")
		_check(float(telegraph.get("duration", 0.0)) >= 1.30, "Boss telegraph visual duration matches the reaction window")

	_finish()


func _finish() -> void:
	var output_dir: String = ProjectSettings.globalize_path("res://build/gameplay-v4")
	DirAccess.make_dir_recursive_absolute(output_dir)
	var report := FileAccess.open(output_dir.path_join("boss_patterns.json"), FileAccess.WRITE)
	if report != null:
		report.store_string(JSON.stringify({
			"checks": _checks,
			"failures": _failures,
			"status": "PASS" if _failures.is_empty() else "FAIL",
			"scope": "boss telegraph timing, auto-evade geometry, bounded damage, special-cast pause, effect registration"
		}, "\t"))
		report.close()
	print("GAMEPLAY_V4_BOSS %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
