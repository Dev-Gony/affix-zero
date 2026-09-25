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
	_check(ui._management_window.position.y <= 10.0, "Management hub uses the full screen instead of sitting below the combat HUD")
	_check(ui._management_window.size.y >= 380.0, "Management hub uses nearly the full logical screen height")
	_check(ui._main_tabs.size.x >= 590.0, "Management content area is wide enough for desktop inventory and equipment layouts")
	_check(ui._section_buttons.size() == GameUI.MANAGEMENT_TITLES.size(), "Every management section has an in-window navigation tab")
	_check(ui._inventory_grid.columns == 8, "Inventory uses a dense eight-column desktop grid")
	_check(ui._inventory_detail.custom_minimum_size.x >= 170.0, "Inventory keeps a persistent comparison/detail pane")
	_check(ui._management_live_status != null and ui._management_live_status.text.contains("자동사냥"), "Management header explicitly communicates that idle combat continues")
	var info_dashboard := ui.find_child("InfoDashboard", true, false) as GridContainer
	_check(info_dashboard != null and info_dashboard.columns == 2 and info_dashboard.get_child_count() == 4, "Info screen uses four dashboard cards instead of a raw text wall")
	var permanent_grid := ui.find_child("PermanentUpgradeGrid", true, false) as GridContainer
	_check(permanent_grid != null and permanent_grid.columns == 4 and permanent_grid.get_child_count() == 4, "Rebirth permanent upgrades use a full-width four-card row")
	GameManager.set_loot_min_rarity(5)
	ui._refresh_inventory()
	_check(ui._inventory_count.text.contains("에픽만"), "Empty inventory header explains the active acquisition filter")
	_check(ui._inventory_detail.text.contains("획득 필터"), "Empty inventory detail explains why lower-rarity drops may not appear")
	_check(ui._inventory_empty_hint.visible and ui._inventory_empty_hint.text.contains("에픽만"), "Empty bag uses a centered filter-aware message instead of only showing dead slots")
	_check(GameUI.EQUIPMENT_LAYOUT.size() == 9 and GameUI.EQUIPMENT_LAYOUT[4] == "portrait", "Equipment layout keeps the character portrait at the visual center")
	_check(ui._class_selection._grid.get_child_count() == 6, "Class selection keeps all six class choices in one readable screen")
	if ui._class_selection._grid.get_child_count() > 0:
		var first_class_card: Control = ui._class_selection._grid.get_child(0)
		_check(first_class_card.custom_minimum_size.x >= 180.0, "Class selection cards are large enough for desktop stats and descriptions")

	ui._toggle_management(0)
	await get_tree().process_frame
	_check(ui._management_open, "Dock opens the management hub")
	_check(ui._management_window.visible, "Management hub becomes visible when opened")
	_check(not ui._hud_panel.visible, "Combat HUD hides while the management hub is open")
	_check(not ui._bottom_panel.visible, "Compact combat dock hides while the full management hub is open")
	_check(ui._section_buttons[0].button_pressed, "Current management section is visibly selected")

	ui._switch_management_tab(1, false)
	await get_tree().process_frame
	_check(ui._management_open and ui._main_tabs.current_tab == 1, "In-window navigation switches sections without closing the hub")
	_check(ui._section_buttons[1].button_pressed, "Top navigation selection follows the active section")

	ui._close_management(false)
	await get_tree().process_frame
	_check(not ui._management_open, "Management hub closes deterministically")
	_check(ui._hud_panel.visible, "Combat HUD returns after closing management")
	_check(ui._bottom_panel.visible, "Compact combat dock returns after closing management")

	var equipment_card: PanelContainer = ui._build_equipment_card("weapon")
	_check(equipment_card.custom_minimum_size.x >= 170.0, "Equipment cards are large enough for icon, item identity and upgrade action")
	var equipment_row: HBoxContainer = equipment_card.get_child(0)
	var equipment_rarity_strip: Control = equipment_row.get_child(0)
	_check(equipment_rarity_strip.custom_minimum_size.x <= 6.0, "Equipment rarity is expressed as a restrained accent strip instead of a full loud frame")
	equipment_card.queue_free()

	_check(ui._skills_list.get_child_count() >= 2, "Skill screen exposes progression cards below its summary")
	var first_skill_panel: PanelContainer = ui._skills_list.get_child(1)
	var first_skill_row: HBoxContainer = first_skill_panel.get_child(0)
	var skill_accent_strip: Control = first_skill_row.get_child(0)
	_check(skill_accent_strip.custom_minimum_size.x <= 6.0, "Skill rows use a slim progression accent instead of placeholder icon blocks")

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
			"scope": "full-width management hub, top navigation, equipment scale, 8-column inventory, empty-state context, info dashboard, rebirth card grid, compact dock behavior"
		}, "\t"))
		report.close()
	print("UIUX_V2 %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
