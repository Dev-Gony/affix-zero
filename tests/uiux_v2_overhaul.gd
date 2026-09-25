extends Node

var _checks: int = 0
var _failures: Array[String] = []


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		push_error("UI/UX V2 tests require --affix-test-mode")
		get_tree().quit(2)
		return
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	_checks += 1
	if not condition:
		_failures.append(message)
		push_error("UIUX_V2: " + message)


func _run() -> void:
	SaveManager.set_persistence_enabled(false)
	GameManager.reset_run_progress()
	var warrior: ClassData = load("res://resources/classes/warrior.tres")
	GameManager.select_class(warrior)

	var ui := GameUI.new()
	add_child(ui)
	await get_tree().process_frame

	_check(ui._management_window.size.x >= 600.0, "Management hub uses almost the full desktop width")
	_check(ui._management_window.size.y >= 340.0, "Management hub uses the available vertical space")
	_check(ui._management_window.position.x <= 16.0, "Management hub is centered as a primary screen, not a side popup")
	_check(ui._main_tabs.size.x >= 590.0, "Management content area is wide enough for desktop inventory and equipment layouts")
	_check(ui._section_buttons.size() == GameUI.MANAGEMENT_TITLES.size(), "Every management section has an in-window navigation tab")
	_check(ui._inventory_grid.columns == 8, "Inventory uses a dense eight-column desktop grid")
	_check(ui._inventory_detail.custom_minimum_size.x >= 170.0, "Inventory keeps a persistent comparison/detail pane")
	_check(GameUI.EQUIPMENT_LAYOUT.size() == 9 and GameUI.EQUIPMENT_LAYOUT[4] == "portrait", "Equipment layout keeps the character portrait at the visual center")

	ui._toggle_management(0)
	await get_tree().process_frame
	_check(ui._management_open, "Dock opens the management hub")
	_check(ui._management_window.visible, "Management hub becomes visible when opened")
	_check(not ui._bottom_panel.visible, "Compact combat dock hides while the full management hub is open")
	_check(ui._section_buttons[0].button_pressed, "Current management section is visibly selected")

	ui._switch_management_tab(1, false)
	await get_tree().process_frame
	_check(ui._management_open and ui._main_tabs.current_tab == 1, "In-window navigation switches sections without closing the hub")
	_check(ui._section_buttons[1].button_pressed, "Top navigation selection follows the active section")

	ui._close_management(false)
	await get_tree().process_frame
	_check(not ui._management_open, "Management hub closes deterministically")
	_check(ui._bottom_panel.visible, "Compact combat dock returns after closing management")

	var equipment_card: PanelContainer = ui._build_equipment_card("weapon")
	_check(equipment_card.custom_minimum_size.x >= 170.0, "Equipment cards are large enough for icon, item identity and upgrade action")
	equipment_card.queue_free()

	_finish()


func _finish() -> void:
	var output_dir: String = ProjectSettings.globalize_path("res://build/uiux-v2")
	DirAccess.make_dir_recursive_absolute(output_dir)
	var report := FileAccess.open(output_dir.path_join("contracts.json"), FileAccess.WRITE)
	if report != null:
		report.store_string(JSON.stringify({
			"checks": _checks,
			"failures": _failures,
			"status": "PASS" if _failures.is_empty() else "FAIL",
			"scope": "full-width management hub, top navigation, equipment scale, 8-column inventory, detail pane, compact dock behavior"
		}, "\t"))
		report.close()
	print("UIUX_V2 %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
