extends Node

var _failures: Array[String] = []
var _checks: int = 0


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		push_error("PR-B tests require --affix-test-mode")
		get_tree().quit(2)
		return
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	_checks += 1
	if not condition:
		_failures.append(message)
		push_error("PR_B: " + message)


func _read_dict(path: String) -> Dictionary:
	if not FileAccess.file_exists(path):
		return {}
	var parsed: Variant = JSON.parse_string(FileAccess.get_file_as_string(path))
	return parsed as Dictionary if parsed is Dictionary else {}


func _run() -> void:
	_check(not SaveManager.persistence_enabled, "Test mode leaves the developer save untouched")
	var root_path: String = "user://pr_b_tests/%d-%d" % [OS.get_process_id(), Time.get_ticks_usec()]
	_check(DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(root_path)) == OK, "Synthetic PR-B directory can be created")

	var atomic_path: String = root_path.path_join("atomic.json")
	var first := {"version": 2, "marker": "first", "gold": 111}
	var second := {"version": 2, "marker": "second", "gold": 222}
	_check(SaveManager.write_atomic(atomic_path, first) == OK, "Atomic writer publishes an initial save")
	_check(String(_read_dict(atomic_path).get("marker", "")) == "first", "Initial atomic save is readable")
	_check(not FileAccess.file_exists(atomic_path + SaveManager.TRANSACTION_TEMP_SUFFIX), "No temp file remains after initial commit")
	_check(SaveManager.write_atomic(atomic_path, second) == OK, "Atomic writer publishes a replacement save")
	_check(String(_read_dict(atomic_path).get("marker", "")) == "second", "Replacement save becomes current")
	_check(String(_read_dict(atomic_path + SaveManager.TRANSACTION_BACKUP_SUFFIX).get("marker", "")) == "first", "Previous verified save remains as rollback backup")
	_check(not FileAccess.file_exists(atomic_path + SaveManager.TRANSACTION_TEMP_SUFFIX), "No temp file remains after replacement")

	var blocker_path: String = root_path.path_join("not_a_directory")
	var blocker := FileAccess.open(blocker_path, FileAccess.WRITE)
	_check(blocker != null, "Synthetic path blocker can be created")
	if blocker != null:
		blocker.store_string("keep-me")
		blocker.close()
	var failed_path: String = blocker_path.path_join("save.json")
	_check(SaveManager.write_atomic(failed_path, second) != OK, "Atomic writer reports an unwritable destination")
	_check(FileAccess.get_file_as_string(blocker_path) == "keep-me", "Failed atomic write does not damage unrelated source data")

	var main_scene: PackedScene = load("res://scenes/main.tscn")
	var main: Node = main_scene.instantiate()
	get_tree().root.add_child(main)
	await get_tree().process_frame
	await get_tree().process_frame
	var battle: BattleManager = main.get_node("BattleArea")
	var ui: GameUI = main.get_node("UILayer/GameUI")
	GameManager.reset_run_progress()
	var warrior: ClassData = load("res://resources/classes/warrior.tres")
	GameManager.select_class(warrior)
	await get_tree().process_frame
	await get_tree().process_frame

	_check(not PauseCoordinator.is_paused() and not get_tree().paused, "Gameplay starts without a stale pause reason")
	ui._toggle_management(1)
	_check(PauseCoordinator.has_reason("management"), "Opening management acquires the management pause reason")
	_check(PauseCoordinator.is_paused() and get_tree().paused, "Management UI pauses the entire SceneTree")
	_check(GameManager.game_state == GameManager.GameState.PAUSED, "Management pause is reflected in game state")
	_check(ui._modal_blocker.visible, "Management window enables the modal input blocker")
	var player_position: Vector2 = battle.player.global_position
	var enemy_position: Vector2 = battle._enemies[0].global_position
	await get_tree().create_timer(0.25, true).timeout
	_check(battle.player.global_position.is_equal_approx(player_position), "Player cannot move while management UI is open")
	_check(battle._enemies[0].global_position.is_equal_approx(enemy_position), "Enemy cannot move while management UI is open")
	ui._close_management(false)
	_check(not PauseCoordinator.has_reason("management") and not get_tree().paused, "Closing management releases its pause reason")
	_check(GameManager.game_state == GameManager.GameState.RUNNING, "Gameplay returns to RUNNING after the final pause reason closes")
	_check(not ui._modal_blocker.visible, "Modal blocker closes with management UI")

	ui._toggle_pause_menu()
	_check(PauseCoordinator.has_reason("pause_menu") and get_tree().paused, "ESC menu uses the same central pause coordinator")
	_check(ui._pause_panel.visible and ui._modal_blocker.visible, "Pause menu is modal")
	ui._toggle_pause_menu()
	_check(not PauseCoordinator.is_paused() and not get_tree().paused, "Closing ESC menu resumes only after pause reasons are clear")

	GameManager.inventory = [
		{"id": "kept-normal", "rarity_index": 0, "locked": false},
		{"id": "kept-legend", "rarity_index": 4, "locked": false},
	]
	GameManager.gold = 4321
	var inventory_before: Array[Dictionary] = GameManager.inventory.duplicate(true)
	var gold_before: int = GameManager.gold
	ui._on_loot_filter_selected(4)
	_check(GameManager.loot_min_rarity_index == 4, "Pickup policy changes to legend-only")
	_check(GameManager.inventory == inventory_before, "Changing pickup policy never sells or deletes existing inventory")
	_check(GameManager.gold == gold_before, "Changing pickup policy never changes gold")
	var low_item := {"id": "future-normal", "rarity_index": 0}
	_check(not LootManager.collect_item(low_item), "Below-policy future drop is rejected at pickup time")
	_check(GameManager.inventory == inventory_before, "Rejected future drop does not mutate existing inventory")
	var legend_item := {"id": "future-legend", "rarity_index": 4, "slot": "ring", "name": "test legend", "base_stats": {}, "affixes": [], "sell_value": 1}
	_check(LootManager.collect_item(legend_item), "At-policy future drop is accepted")
	_check(GameManager.inventory.size() == inventory_before.size() + 1, "Accepted future drop is added exactly once")

	PauseCoordinator.clear_all()
	main.queue_free()
	await get_tree().process_frame
	_finish()


func _finish() -> void:
	PauseCoordinator.clear_all()
	SaveManager.set_persistence_enabled(false)
	var output_dir: String = ProjectSettings.globalize_path("res://build/pr-b")
	DirAccess.make_dir_recursive_absolute(output_dir)
	var report := FileAccess.open(output_dir.path_join("safety.json"), FileAccess.WRITE)
	if report != null:
		report.store_string(JSON.stringify({
			"checks": _checks,
			"failures": _failures,
			"status": "PASS" if _failures.is_empty() else "FAIL",
			"scope": "PR-B pause/input, pickup policy and atomic save safety"
		}, "	"))
		report.close()
	print("PR_B_SAFETY %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
