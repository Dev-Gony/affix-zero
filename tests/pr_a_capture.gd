extends Node

var _failed: bool = false
const OUTPUT: String = "res://build/pr-a"


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.get_cmdline_user_args().has("--affix-test-mode") or DisplayServer.get_name() == "headless":
		push_error("Capture requires an isolated --affix-test-mode graphical renderer")
		get_tree().quit(2)
		return
	call_deferred("_capture")


func _capture() -> void:
	var fixture: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://tests/fixtures/pr_a_legacy_v2.json")) as Dictionary
	var scene: PackedScene = load("res://scenes/main.tscn")
	var main: Node = scene.instantiate()
	get_tree().root.add_child(main)
	await get_tree().process_frame
	await get_tree().process_frame
	seed(20260925)
	GameManager.apply_save_dict(fixture)
	var warrior: ClassData = load("res://resources/classes/warrior.tres")
	GameManager.select_class(warrior)
	for frame: int in 4:
		await get_tree().process_frame
	GameManager.set_game_state(GameManager.GameState.PAUSED)
	get_tree().paused = true
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(OUTPUT))
	await _shot("baseline_combat.png")
	var ui: GameUI = main.get_node("UILayer/GameUI")
	ui._toggle_management(1)
	await _shot("baseline_inventory.png")
	ui._close_management(false)
	ui._toggle_pause_menu()
	await _shot("baseline_pause.png")
	ui._toggle_pause_menu()
	get_tree().paused = true
	GameManager.set_game_state(GameManager.GameState.PAUSED)
	main.get_node("BuildDiagnostics").toggle_details()
	await _shot("baseline_diagnostics.png")
	var metadata: Dictionary = BuildInfo.read_info()
	metadata["fixture"] = "tests/fixtures/pr_a_legacy_v2.json"
	metadata["fixture_sha256"] = FileAccess.get_sha256("res://tests/fixtures/pr_a_legacy_v2.json")
	metadata["seed"] = 20260925
	metadata["renderer"] = DisplayServer.get_name()
	metadata["viewport"] = str(get_viewport().get_visible_rect().size)
	metadata["window"] = str(DisplayServer.window_get_size())
	metadata["user_visual_approval"] = "NOT_RUN"
	var file := FileAccess.open(OUTPUT.path_join("capture_metadata.json"), FileAccess.WRITE)
	if file != null:
		file.store_string(JSON.stringify(metadata, "\t"))
		file.close()
	else:
		_failed = true
	get_tree().paused = false
	print("PR_A_CAPTURE " + ("FAILED" if _failed else "PASSED"))
	get_tree().quit(1 if _failed else 0)


func _shot(filename: String) -> void:
	await get_tree().process_frame
	await RenderingServer.frame_post_draw
	var image: Image = get_viewport().get_texture().get_image()
	if image.is_empty() or image.save_png(OUTPUT.path_join(filename)) != OK:
		_failed = true
		push_error("Screenshot failed: " + filename)
