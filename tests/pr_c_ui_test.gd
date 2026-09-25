extends Node

var _failures: Array[String] = []
var _checks: int = 0


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		push_error("PR-C tests require --affix-test-mode")
		get_tree().quit(2)
		return
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	_checks += 1
	if not condition:
		_failures.append(message)
		push_error("PR_C: " + message)


func _run() -> void:
	_check(int(ProjectSettings.get_setting("display/window/size/viewport_width", 0)) == 1280, "UI viewport width is 1280")
	_check(int(ProjectSettings.get_setting("display/window/size/viewport_height", 0)) == 720, "UI viewport height is 720")
	_check(int(ProjectSettings.get_setting("display/window/size/window_width_override", 0)) == 1280, "Default window width is 1280")
	_check(int(ProjectSettings.get_setting("display/window/size/window_height_override", 0)) == 720, "Default window height is 720")

	var main_scene: PackedScene = load("res://scenes/main.tscn")
	_check(main_scene != null, "Main scene loads")
	var main: Node = main_scene.instantiate()
	get_tree().root.add_child(main)
	await get_tree().process_frame
	await get_tree().process_frame

	var ui: GameUI = main.get_node("UILayer/GameUI")
	var camera: Camera2D = main.get_node("BattleArea/Player/Camera2D")
	_check(ui != null, "Game UI instantiates")
	_check(camera != null and camera.zoom.is_equal_approx(Vector2(2.0, 2.0)), "Pixel world keeps 2x camera framing at 720p")
	_check(ui._hud_panel.size.is_equal_approx(Vector2(1280, 92)), "HUD spans the 1280px viewport")
	_check(ui._bottom_panel.position.is_equal_approx(Vector2(340, 646)), "Navigation dock is anchored near the 720p bottom edge")
	_check(ui._bottom_panel.size.is_equal_approx(Vector2(600, 64)), "Navigation dock uses the larger five-button layout")
	_check(ui._dock_buttons.size() == 5, "Navigation dock exposes five management destinations")
	_check(ui._speed_buttons.size() == 3, "HUD exposes three speed controls")
	_check(ui._management_window.scale.is_equal_approx(Vector2(1.72, 1.72)), "Legacy management content is temporarily scaled for PR-C readability")
	_check(ui._pause_panel.scale.is_equal_approx(Vector2(1.7, 1.7)), "Pause/settings window is readable at 720p")
	_check(ui._modal_blocker.size.is_equal_approx(Vector2(1280, 720)), "ESC modal blocker covers the full 720p UI")
	_check(ui._notification_label.position.is_equal_approx(Vector2(390, 108)), "Wave/notification banner sits below the HUD")

	var hud_has_quick_save: bool = false
	var hud_has_quick_quit: bool = false
	for child: Node in ui._hud_panel.find_children("*", "Button", true, false):
		var button := child as Button
		if button.text == "저장":
			hud_has_quick_save = true
		if button.text == "종료":
			hud_has_quick_quit = true
	_check(not hud_has_quick_save and not hud_has_quick_quit, "Save and quit are removed from the combat HUD")

	GameManager.reset_run_progress()
	var knight: ClassData = load("res://resources/classes/knight.tres")
	if not GameManager.unlocked_classes.has("knight"):
		GameManager.unlocked_classes.append("knight")
	GameManager.select_class(knight)
	await get_tree().process_frame
	ui._refresh_hud()
	_check(ui._hud_player_text.text.contains("기사"), "HUD player card reflects the active class")
	_check(ui._hud_info.text.contains("AUTO"), "Center HUD keeps auto-hunt status")
	_check(ui._hud_class_icon.texture != null, "HUD class portrait has a texture")

	ui._show_notification("적 증원 20마리 접근", Color.WHITE)
	_check(ui._notification_label.text == "WAVE  ·  적 20", "Long reinforcement copy collapses to a compact wave banner")
	ui._show_notification("보스 출현 · 고대 용", Color.WHITE)
	_check(ui._notification_label.text == "BOSS  ·  고대 용", "Boss copy collapses to a compact boss banner")

	main.queue_free()
	await get_tree().process_frame
	_finish()


func _finish() -> void:
	get_tree().paused = false
	SaveManager.set_persistence_enabled(false)
	var output_dir: String = ProjectSettings.globalize_path("res://build/pr-c")
	DirAccess.make_dir_recursive_absolute(output_dir)
	var report := FileAccess.open(output_dir.path_join("ui.json"), FileAccess.WRITE)
	if report != null:
		report.store_string(JSON.stringify({
			"checks": _checks,
			"failures": _failures,
			"status": "PASS" if _failures.is_empty() else "FAIL",
			"scope": "PR-C 1280x720 HUD/navigation layout"
		}, "\t"))
		report.close()
	print("PR_C_UI %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
