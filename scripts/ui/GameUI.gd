extends Control
class_name GameUI

const COLOR_BACKGROUND := Color("0b0910")
const COLOR_PANEL := Color("141019")
const COLOR_PANEL_ALT := Color("1c1520")
const COLOR_BORDER := Color("69432f")
const COLOR_TEXT := Color("f3f4ff")
const COLOR_MUTED := Color("c5b4a8")
const COLOR_ACCENT := Color("dc3d33")
const COLOR_GREEN := Color("4fd675")
const COLOR_GOLD := Color("d9a441")
const EQUIPMENT_ATLAS: Texture2D = preload("res://assets/sprites/equipment_atlas_alpha.png")
const ITEM_BASE_ATLAS: Texture2D = preload("res://assets/sprites/item_base_atlas_v2.png")
const CLASS_ATLAS: Texture2D = preload("res://assets/sprites/class_atlas_alpha.png")
const EQUIPMENT_REGIONS: Dictionary = {
	"weapon": Vector2i(0, 0), "helmet": Vector2i(1, 0), "armor": Vector2i(2, 0), "gloves": Vector2i(3, 0),
	"boots": Vector2i(0, 1), "ring": Vector2i(1, 1), "amulet": Vector2i(2, 1),
}

const SLOT_NAMES: Dictionary = {
	"weapon": "무기", "helmet": "투구", "armor": "갑옷", "gloves": "장갑",
	"boots": "신발", "ring": "반지", "amulet": "목걸이",
}
const STAT_NAMES: Dictionary = {
	"ATK": "공격", "DEF": "방어", "HP": "체력", "MP": "마나", "SPD": "속도",
	"CRIT": "치명", "VAMP": "흡혈", "XP_BONUS": "경험", "GOLD_BONUS": "골드", "PEN": "관통",
}
const RARITY_BADGES: Dictionary = {
	"normal": "일반", "magic": "마법", "rare": "희귀", "unique": "고유", "legend": "전설",
}
const CLASS_REGIONS: Dictionary = {
	"warrior": Vector2i(0, 0), "mage": Vector2i(1, 0), "knight": Vector2i(2, 0),
	"sage": Vector2i(0, 1), "assassin": Vector2i(1, 1), "saint": Vector2i(2, 1),
}
const MANAGEMENT_TITLES: Array[String] = ["장비", "가방", "스킬", "환생", "정보"]
const EQUIPMENT_LAYOUT: Array[String] = ["amulet", "helmet", "ring", "weapon", "portrait", "gloves", "boots", "armor", "summary"]

var _hp_bar: ProgressBar
var _mp_bar: ProgressBar
var _xp_bar: ProgressBar
var _hud_info: Label
var _speed_buttons: Dictionary = {}
var _equipment_row: GridContainer
var _inventory_grid: GridContainer
var _inventory_count: Label
var _inventory_detail: Label
var _inventory_equip_button: Button
var _inventory_sell_button: Button
var _inventory_lock_button: Button
var _loot_filter_option: OptionButton
var _inventory_selected_id: String = ""
var _skills_list: VBoxContainer
var _rebirth_content: VBoxContainer
var _stats_label: Label
var _notification_label: Label
var _notification_tween: Tween
var _class_selection: ClassSelection
var _hud_panel: Panel
var _bottom_panel: Panel
var _main_tabs: TabContainer
var _management_window: Panel
var _management_title: Label
var _management_open: bool = false
var _dock_buttons: Dictionary = {}


func _ready() -> void:
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	theme = _create_theme()
	_build_hud()
	_build_bottom_panel()
	_build_notification()
	_build_class_selection()
	_connect_signals()
	_refresh_all()
	_apply_game_state_visibility(GameManager.game_state)


func _create_theme() -> Theme:
	var game_theme := Theme.new()
	game_theme.default_font_size = 10
	game_theme.set_color("font_color", "Label", COLOR_TEXT)
	game_theme.set_color("font_color", "Button", COLOR_TEXT)
	game_theme.set_color("font_hover_color", "Button", Color.WHITE)
	game_theme.set_color("font_pressed_color", "Button", Color.WHITE)
	game_theme.set_color("font_disabled_color", "Button", Color("64647a"))
	game_theme.set_stylebox("panel", "Panel", _style_box(COLOR_PANEL, COLOR_BORDER, 2, 0))
	game_theme.set_stylebox("panel", "PanelContainer", _style_box(COLOR_PANEL_ALT, COLOR_BORDER, 1, 0))
	game_theme.set_stylebox("normal", "Button", _style_box(Color("211720"), Color("69432f"), 1, 0))
	game_theme.set_stylebox("hover", "Button", _style_box(Color("38202a"), Color("c17a43"), 1, 0))
	game_theme.set_stylebox("pressed", "Button", _style_box(Color("4b1f28"), Color("e04f46"), 2, 0))
	game_theme.set_stylebox("disabled", "Button", _style_box(Color("100d13"), Color("35271f"), 1, 0))
	game_theme.set_stylebox("focus", "Button", _style_box(Color(0, 0, 0, 0), COLOR_GOLD, 2, 0))
	game_theme.set_stylebox("panel", "TabContainer", _style_box(COLOR_PANEL, COLOR_BORDER, 1, 0))
	game_theme.set_stylebox("tab_selected", "TabContainer", _style_box(Color("4a1c25"), COLOR_GOLD, 2, 0))
	game_theme.set_stylebox("tab_unselected", "TabContainer", _style_box(Color("171018"), COLOR_BORDER, 1, 0))
	game_theme.set_color("font_selected_color", "TabContainer", Color.WHITE)
	game_theme.set_color("font_unselected_color", "TabContainer", COLOR_MUTED)
	game_theme.set_constant("side_margin", "TabContainer", 5)
	return game_theme


func _style_box(background: Color, border: Color, width: int, radius: int) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = background
	style.border_color = border
	style.set_border_width_all(width)
	style.set_corner_radius_all(radius)
	style.anti_aliasing = false
	style.content_margin_left = 4
	style.content_margin_right = 4
	style.content_margin_top = 3
	style.content_margin_bottom = 3
	return style


func _compact_style_box(background: Color, border: Color, width: int = 1) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = background
	style.border_color = border
	style.set_border_width_all(width)
	style.anti_aliasing = false
	style.content_margin_left = 3
	style.content_margin_right = 3
	style.content_margin_top = 1
	style.content_margin_bottom = 1
	return style


func _build_hud() -> void:
	var panel := Panel.new()
	_hud_panel = panel
	panel.position = Vector2.ZERO
	panel.size = Vector2(640, 34)
	panel.add_theme_stylebox_override("panel", _style_box(Color(0.035, 0.025, 0.045, 0.74), Color("6f4934"), 1, 0))
	add_child(panel)
	var row := HBoxContainer.new()
	row.position = Vector2(7, 2)
	row.size = Vector2(626, 29)
	row.add_theme_constant_override("separation", 8)
	panel.add_child(row)

	var bars := VBoxContainer.new()
	bars.custom_minimum_size = Vector2(214, 28)
	bars.add_theme_constant_override("separation", 1)
	row.add_child(bars)
	_hp_bar = _add_bar(bars, "HP", Color("dc2626"))
	_mp_bar = _add_bar(bars, "MP", Color("2563eb"))
	_xp_bar = _add_bar(bars, "XP", Color("22c55e"))

	_hud_info = Label.new()
	_hud_info.custom_minimum_size = Vector2(206, 28)
	_hud_info.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	_hud_info.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_hud_info.add_theme_font_size_override("font_size", 9)
	row.add_child(_hud_info)

	var speed_row := HBoxContainer.new()
	speed_row.custom_minimum_size = Vector2(116, 28)
	speed_row.alignment = BoxContainer.ALIGNMENT_END
	speed_row.add_theme_constant_override("separation", 4)
	row.add_child(speed_row)
	var speed_title := Label.new()
	speed_title.text = "속도"
	speed_title.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	speed_title.add_theme_color_override("font_color", COLOR_MUTED)
	speed_row.add_child(speed_title)
	for multiplier: float in GameManager.AVAILABLE_SPEED_MULTIPLIERS:
		var speed_button := Button.new()
		speed_button.text = "x%d" % int(multiplier)
		speed_button.custom_minimum_size = Vector2(30, 24)
		speed_button.toggle_mode = true
		speed_button.tooltip_text = "전투 속도를 x%d로 변경" % int(multiplier)
		speed_button.pressed.connect(_set_speed.bind(multiplier))
		speed_row.add_child(speed_button)
		_speed_buttons[multiplier] = speed_button

	var quick_row := HBoxContainer.new()
	quick_row.custom_minimum_size = Vector2(82, 28)
	quick_row.alignment = BoxContainer.ALIGNMENT_END
	quick_row.add_theme_constant_override("separation", 2)
	row.add_child(quick_row)
	var quick_save := Button.new()
	quick_save.text = "저장"
	quick_save.custom_minimum_size = Vector2(38, 24)
	quick_save.add_theme_font_size_override("font_size", 7)
	quick_save.tooltip_text = "즉시 저장"
	quick_save.pressed.connect(_manual_save)
	quick_row.add_child(quick_save)
	var quick_quit := Button.new()
	quick_quit.text = "종료"
	quick_quit.custom_minimum_size = Vector2(38, 24)
	quick_quit.add_theme_font_size_override("font_size", 7)
	quick_quit.tooltip_text = "저장 후 게임 종료"
	quick_quit.pressed.connect(_save_and_quit)
	quick_row.add_child(quick_quit)


func _add_bar(parent: VBoxContainer, title: String, fill_color: Color) -> ProgressBar:
	var row := HBoxContainer.new()
	row.custom_minimum_size = Vector2(228, 10)
	row.add_theme_constant_override("separation", 4)
	parent.add_child(row)
	var label := Label.new()
	label.text = title
	label.custom_minimum_size = Vector2(18, 8)
	label.add_theme_font_size_override("font_size", 8)
	row.add_child(label)
	var bar := ProgressBar.new()
	bar.custom_minimum_size = Vector2(202, 7)
	bar.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	bar.show_percentage = false
	bar.add_theme_stylebox_override("background", _style_box(Color("090910"), Color("34344c"), 1, 1))
	bar.add_theme_stylebox_override("fill", _style_box(fill_color, fill_color.lightened(0.18), 1, 1))
	row.add_child(bar)
	return bar


func _build_bottom_panel() -> void:
	var window := Panel.new()
	_management_window = window
	window.position = Vector2(344, 38)
	window.size = Vector2(290, 318)
	window.z_index = 20
	window.add_theme_stylebox_override("panel", _style_box(Color(0.045, 0.032, 0.050, 0.98), Color("9a6240"), 2, 0))
	add_child(window)
	var header := HBoxContainer.new()
	header.position = Vector2(6, 4)
	header.size = Vector2(278, 22)
	window.add_child(header)
	_management_title = Label.new()
	_management_title.text = MANAGEMENT_TITLES[0]
	_management_title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_management_title.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	_management_title.add_theme_font_size_override("font_size", 11)
	_management_title.add_theme_color_override("font_color", COLOR_GOLD)
	header.add_child(_management_title)
	var close_button := Button.new()
	close_button.text = "닫기  ESC"
	close_button.custom_minimum_size = Vector2(74, 22)
	close_button.add_theme_font_size_override("font_size", 7)
	close_button.pressed.connect(_close_management)
	header.add_child(close_button)
	var tabs := TabContainer.new()
	_main_tabs = tabs
	tabs.name = "MainTabs"
	tabs.position = Vector2(4, 28)
	tabs.size = Vector2(282, 286)
	tabs.tabs_visible = false
	window.add_child(tabs)

	_build_equipment_tab(tabs)
	_build_inventory_tab(tabs)
	_build_skills_tab(tabs)
	_build_rebirth_tab(tabs)
	_build_stats_tab(tabs)
	window.visible = false

	var dock := Panel.new()
	_bottom_panel = dock
	dock.position = Vector2(142, 366)
	dock.size = Vector2(356, 32)
	dock.z_index = 30
	dock.add_theme_stylebox_override("panel", _style_box(Color(0.04, 0.03, 0.05, 0.72), Color("6f4934"), 1, 0))
	add_child(dock)
	var dock_row := HBoxContainer.new()
	dock_row.position = Vector2(6, 3)
	dock_row.size = Vector2(344, 26)
	dock_row.add_theme_constant_override("separation", 4)
	dock.add_child(dock_row)
	for index: int in MANAGEMENT_TITLES.size():
		var dock_button := Button.new()
		dock_button.text = "%d %s" % [index + 1, MANAGEMENT_TITLES[index]]
		dock_button.custom_minimum_size = Vector2(65, 24)
		dock_button.toggle_mode = true
		dock_button.tooltip_text = "%s 창 열기/닫기 · 단축키 %d" % [MANAGEMENT_TITLES[index], index + 1]
		dock_button.pressed.connect(_toggle_management.bind(index))
		dock_row.add_child(dock_button)
		_dock_buttons[index] = dock_button


func _unhandled_key_input(event: InputEvent) -> void:
	if not event is InputEventKey or not event.pressed or event.echo:
		return
	if GameManager.game_state != GameManager.GameState.RUNNING:
		return
	match event.keycode:
		KEY_1, KEY_2, KEY_3, KEY_4, KEY_5:
			_toggle_management(int(event.keycode - KEY_1))
		KEY_ESCAPE:
			_close_management()
		KEY_Z: _set_speed(1.0)
		KEY_X: _set_speed(2.0)
		KEY_C: _set_speed(5.0)
		KEY_E:
			if _management_open and _main_tabs.current_tab == 1:
				_equip_selected_inventory()


func _toggle_management(tab_index: int) -> void:
	AudioManager.play_sfx("ui_click")
	if _management_open and _main_tabs.current_tab == tab_index:
		_close_management(false)
		return
	_management_open = true
	_main_tabs.current_tab = tab_index
	_management_title.text = MANAGEMENT_TITLES[tab_index]
	_management_window.visible = true
	_sync_dock_buttons()


func _close_management(play_sound: bool = true) -> void:
	if not _management_open:
		return
	if play_sound:
		AudioManager.play_sfx("ui_click")
	_management_open = false
	_management_window.visible = false
	_sync_dock_buttons()


func _sync_dock_buttons() -> void:
	for index: Variant in _dock_buttons.keys():
		var button: Button = _dock_buttons[index]
		button.set_pressed_no_signal(_management_open and int(index) == _main_tabs.current_tab)


func _build_equipment_tab(tabs: TabContainer) -> void:
	var tab := MarginContainer.new()
	tab.name = "장비"
	tab.add_theme_constant_override("margin_left", 6)
	tab.add_theme_constant_override("margin_right", 6)
	tab.add_theme_constant_override("margin_top", 4)
	tab.add_theme_constant_override("margin_bottom", 4)
	tabs.add_child(tab)
	_equipment_row = GridContainer.new()
	_equipment_row.columns = 3
	_equipment_row.add_theme_constant_override("h_separation", 4)
	_equipment_row.add_theme_constant_override("v_separation", 4)
	tab.add_child(_equipment_row)


func _build_inventory_tab(tabs: TabContainer) -> void:
	var tab := VBoxContainer.new()
	tab.name = "가방"
	tab.add_theme_constant_override("separation", 2)
	tabs.add_child(tab)
	var header := HBoxContainer.new()
	header.custom_minimum_size.y = 22
	tab.add_child(header)
	_inventory_count = Label.new()
	_inventory_count.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_inventory_count.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	header.add_child(_inventory_count)
	_loot_filter_option = OptionButton.new()
	_loot_filter_option.custom_minimum_size = Vector2(90, 21)
	_loot_filter_option.tooltip_text = "이 등급 이상만 자동 획득"
	for rarity_name: String in ["일반+", "마법+", "희귀+", "고유+", "전설만"]:
		_loot_filter_option.add_item(rarity_name)
	_loot_filter_option.item_selected.connect(_on_loot_filter_selected)
	header.add_child(_loot_filter_option)
	var sell_all := Button.new()
	sell_all.text = "일반 판매"
	sell_all.custom_minimum_size = Vector2(76, 21)
	sell_all.add_theme_font_size_override("font_size", 7)
	sell_all.pressed.connect(_sell_all_normal)
	header.add_child(sell_all)
	var grid_panel := PanelContainer.new()
	grid_panel.custom_minimum_size = Vector2(0, 184)
	grid_panel.add_theme_stylebox_override("panel", _style_box(Color("0b0910"), Color("352b38"), 1, 0))
	tab.add_child(grid_panel)
	var grid_scroll := ScrollContainer.new()
	grid_scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	grid_scroll.vertical_scroll_mode = ScrollContainer.SCROLL_MODE_AUTO
	grid_panel.add_child(grid_scroll)
	var grid_margin := MarginContainer.new()
	grid_margin.add_theme_constant_override("margin_left", 3)
	grid_margin.add_theme_constant_override("margin_right", 3)
	grid_margin.add_theme_constant_override("margin_top", 3)
	grid_margin.add_theme_constant_override("margin_bottom", 3)
	grid_scroll.add_child(grid_margin)
	_inventory_grid = GridContainer.new()
	_inventory_grid.columns = 6
	_inventory_grid.add_theme_constant_override("h_separation", 2)
	_inventory_grid.add_theme_constant_override("v_separation", 2)
	grid_margin.add_child(_inventory_grid)
	var detail_panel := PanelContainer.new()
	detail_panel.size_flags_vertical = Control.SIZE_EXPAND_FILL
	detail_panel.add_theme_stylebox_override("panel", _style_box(Color("171119"), Color("69432f"), 1, 0))
	tab.add_child(detail_panel)
	var detail_column := VBoxContainer.new()
	detail_column.add_theme_constant_override("separation", 2)
	detail_panel.add_child(detail_column)
	var detail_scroll := ScrollContainer.new()
	detail_scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	detail_scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	detail_column.add_child(detail_scroll)
	_inventory_detail = Label.new()
	_inventory_detail.custom_minimum_size.x = 270
	_inventory_detail.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	_inventory_detail.add_theme_font_size_override("font_size", 6)
	detail_scroll.add_child(_inventory_detail)
	var actions := HBoxContainer.new()
	actions.add_theme_constant_override("separation", 3)
	detail_column.add_child(actions)
	_inventory_equip_button = Button.new()
	_inventory_equip_button.text = "장착"
	_inventory_equip_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_inventory_equip_button.custom_minimum_size.y = 22
	_inventory_equip_button.pressed.connect(_equip_selected_inventory)
	actions.add_child(_inventory_equip_button)
	_inventory_lock_button = Button.new()
	_inventory_lock_button.text = "잠금"
	_inventory_lock_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_inventory_lock_button.custom_minimum_size.y = 22
	_inventory_lock_button.pressed.connect(_toggle_selected_inventory_lock)
	actions.add_child(_inventory_lock_button)
	_inventory_sell_button = Button.new()
	_inventory_sell_button.text = "판매"
	_inventory_sell_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_inventory_sell_button.custom_minimum_size.y = 22
	_inventory_sell_button.pressed.connect(_sell_selected_inventory)
	actions.add_child(_inventory_sell_button)


func _build_skills_tab(tabs: TabContainer) -> void:
	var tab := MarginContainer.new()
	tab.name = "스킬"
	tab.add_theme_constant_override("margin_left", 6)
	tab.add_theme_constant_override("margin_right", 6)
	tab.add_theme_constant_override("margin_top", 5)
	tabs.add_child(tab)
	_skills_list = VBoxContainer.new()
	_skills_list.add_theme_constant_override("separation", 5)
	tab.add_child(_skills_list)


func _build_rebirth_tab(tabs: TabContainer) -> void:
	var tab := MarginContainer.new()
	tab.name = "환생"
	tab.add_theme_constant_override("margin_left", 6)
	tab.add_theme_constant_override("margin_right", 6)
	tab.add_theme_constant_override("margin_top", 5)
	tabs.add_child(tab)
	_rebirth_content = VBoxContainer.new()
	_rebirth_content.add_theme_constant_override("separation", 8)
	tab.add_child(_rebirth_content)


func _build_stats_tab(tabs: TabContainer) -> void:
	var tab := VBoxContainer.new()
	tab.name = "정보"
	tab.add_theme_constant_override("separation", 6)
	tabs.add_child(tab)
	_stats_label = Label.new()
	_stats_label.vertical_alignment = VERTICAL_ALIGNMENT_TOP
	_stats_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	_stats_label.add_theme_font_size_override("font_size", 9)
	_stats_label.size_flags_vertical = Control.SIZE_EXPAND_FILL
	tab.add_child(_stats_label)

	var save_row := HBoxContainer.new()
	save_row.add_theme_constant_override("separation", 4)
	tab.add_child(save_row)
	var save_button := Button.new()
	save_button.text = "저장"
	save_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	save_button.pressed.connect(_manual_save)
	save_row.add_child(save_button)
	var load_button := Button.new()
	load_button.text = "불러오기"
	load_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	load_button.pressed.connect(_manual_load)
	save_row.add_child(load_button)
	var quit_button := Button.new()
	quit_button.text = "저장 후 종료"
	quit_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	quit_button.pressed.connect(_save_and_quit)
	save_row.add_child(quit_button)

	var save_hint := Label.new()
	save_hint.text = "자동 저장: 30초마다 · 창 종료 시 자동 저장"
	save_hint.add_theme_font_size_override("font_size", 6)
	save_hint.add_theme_color_override("font_color", COLOR_MUTED)
	tab.add_child(save_hint)


func _build_notification() -> void:
	_notification_label = Label.new()
	_notification_label.position = Vector2(12, 47)
	_notification_label.size = Vector2(306, 24)
	_notification_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_notification_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	_notification_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_notification_label.modulate.a = 0.0
	_notification_label.add_theme_stylebox_override("normal", _style_box(Color(0.04, 0.04, 0.08, 0.88), Color("34345b"), 1, 2))
	_notification_label.z_index = 40
	add_child(_notification_label)


func _build_class_selection() -> void:
	_class_selection = ClassSelection.new()
	_class_selection.add_theme_stylebox_override("panel", _style_box(Color(0.025, 0.018, 0.03, 0.90), Color("9a6240"), 2, 0))
	add_child(_class_selection)


func _connect_signals() -> void:
	GameManager.stats_changed.connect(_on_stats_changed)
	GameManager.inventory_changed.connect(_refresh_inventory)
	GameManager.equipment_changed.connect(_on_equipment_changed)
	GameManager.skills_changed.connect(_refresh_skills)
	GameManager.floor_changed.connect(func(_floor: int) -> void: _refresh_hud())
	GameManager.speed_changed.connect(_refresh_speed_buttons)
	GameManager.game_state_changed.connect(_on_game_state_changed)
	GameManager.notification_requested.connect(_show_notification)
	RebirthManager.rebirth_completed.connect(func(_count: int) -> void: _on_rebirth_changed())
	RebirthManager.permanent_upgrade_purchased.connect(func(_stat: String) -> void: _on_rebirth_changed())


func _refresh_all() -> void:
	_refresh_hud()
	_refresh_speed_buttons(GameManager.speed_multiplier)
	if _loot_filter_option != null:
		_loot_filter_option.select(GameManager.loot_min_rarity_index)
	_refresh_equipment()
	_refresh_inventory()
	_refresh_skills()
	_refresh_rebirth()
	_refresh_stats()
	_class_selection.refresh()


func _refresh_hud() -> void:
	_hp_bar.max_value = maxi(1, GameManager.max_hp)
	_hp_bar.value = GameManager.hp
	_mp_bar.max_value = maxi(1, GameManager.max_mp)
	_mp_bar.value = GameManager.mp
	_xp_bar.max_value = LevelManager.xp_needed(GameManager.level)
	_xp_bar.value = GameManager.xp
	_hud_info.text = "◆ %d층  ·  Lv.%d  ·  %dG\nAUTO  처치 %d/%d  ·  %s" % [
		GameManager.floor, GameManager.level, GameManager.gold, GameManager.kills_on_floor,
		8 + GameManager.floor, GameManager.selected_class_name if not GameManager.selected_class_name.is_empty() else "직업 선택"
	]


func _refresh_speed_buttons(multiplier: float) -> void:
	for speed: Variant in _speed_buttons.keys():
		var button: Button = _speed_buttons[speed]
		button.set_pressed_no_signal(is_equal_approx(float(speed), multiplier))


func _refresh_equipment() -> void:
	_clear_container(_equipment_row)
	for cell_id: String in EQUIPMENT_LAYOUT:
		if cell_id == "portrait":
			_equipment_row.add_child(_build_equipment_portrait())
		elif cell_id == "summary":
			_equipment_row.add_child(_build_equipment_summary())
		else:
			_equipment_row.add_child(_build_equipment_card(cell_id))


func _build_equipment_card(slot: String) -> PanelContainer:
	var item: Dictionary = GameManager.equipment.get(slot, {})
	var card := PanelContainer.new()
	card.custom_minimum_size = Vector2(88, 82)
	card.set_meta("equipment_slot", slot)
	var border_color := Color("34345b") if item.is_empty() else Color.from_string(String(item.get("rarity_color", "ffffff")), Color.WHITE)
	card.add_theme_stylebox_override("panel", _compact_style_box(Color("151018"), border_color))
	var card_row := HBoxContainer.new()
	card_row.add_theme_constant_override("separation", 2)
	card.add_child(card_row)
	var equipped_marker := ColorRect.new()
	equipped_marker.custom_minimum_size = Vector2(3, 0)
	equipped_marker.color = COLOR_GREEN if not item.is_empty() else Color(0, 0, 0, 0)
	card_row.add_child(equipped_marker)
	var content := VBoxContainer.new()
	content.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	content.add_theme_constant_override("separation", 0)
	card_row.add_child(content)
	var slot_label := Label.new()
	slot_label.text = String(SLOT_NAMES.get(slot, slot))
	slot_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	slot_label.add_theme_font_size_override("font_size", 6)
	slot_label.add_theme_color_override("font_color", COLOR_MUTED)
	content.add_child(slot_label)
	var icon := TextureRect.new()
	icon.texture = _item_icon(item) if not item.is_empty() else _equipment_icon(slot)
	icon.custom_minimum_size = Vector2(0, 24)
	icon.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	icon.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	icon.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	icon.modulate = Color.WHITE if not item.is_empty() else Color(0.30, 0.28, 0.36, 0.58)
	content.add_child(icon)
	var item_label := Label.new()
	item_label.size_flags_vertical = Control.SIZE_EXPAND_FILL
	item_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	item_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	item_label.custom_minimum_size.y = 14
	item_label.clip_text = true
	item_label.text_overrun_behavior = TextServer.OVERRUN_TRIM_ELLIPSIS
	item_label.add_theme_font_size_override("font_size", 5)
	if item.is_empty():
		item_label.text = "비어 있음"
		item_label.add_theme_color_override("font_color", Color("70708a"))
	else:
		item_label.text = "%s · %d" % [String(item.get("base_name", item.get("name", ""))), int(item.get("item_level", 1))]
		item_label.tooltip_text = _format_item_details(item)
		item_label.add_theme_color_override("font_color", border_color)
	content.add_child(item_label)
	var action := Button.new()
	action.text = "해제"
	action.custom_minimum_size.y = 17
	action.add_theme_font_size_override("font_size", 5)
	action.add_theme_stylebox_override("normal", _compact_style_box(Color("211720"), Color("69432f")))
	action.add_theme_stylebox_override("hover", _compact_style_box(Color("38202a"), Color("c17a43")))
	action.add_theme_stylebox_override("pressed", _compact_style_box(Color("4b1f28"), Color("e04f46"), 2))
	action.add_theme_stylebox_override("disabled", _compact_style_box(Color("100d13"), Color("35271f")))
	action.add_theme_stylebox_override("focus", _compact_style_box(Color(0, 0, 0, 0), COLOR_GOLD, 2))
	action.disabled = item.is_empty()
	action.pressed.connect(_unequip.bind(slot))
	content.add_child(action)
	return card


func _build_equipment_portrait() -> PanelContainer:
	var panel := PanelContainer.new()
	panel.custom_minimum_size = Vector2(88, 82)
	panel.add_theme_stylebox_override("panel", _compact_style_box(Color("0d0a10"), Color("9a6240"), 2))
	var column := VBoxContainer.new()
	panel.add_child(column)
	var portrait := TextureRect.new()
	portrait.texture = _class_portrait_icon()
	portrait.custom_minimum_size = Vector2(0, 54)
	portrait.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	portrait.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	portrait.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	column.add_child(portrait)
	var class_label := Label.new()
	class_label.text = GameManager.selected_class_name
	class_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	class_label.add_theme_font_size_override("font_size", 7)
	class_label.add_theme_color_override("font_color", COLOR_GOLD)
	column.add_child(class_label)
	return panel


func _build_equipment_summary() -> PanelContainer:
	var panel := PanelContainer.new()
	panel.custom_minimum_size = Vector2(88, 82)
	panel.add_theme_stylebox_override("panel", _compact_style_box(Color("171119"), Color("59443a")))
	var label := Label.new()
	label.text = "전투력\nATK  %d\nDEF  %d\nCRIT %.0f%%" % [GameManager.atk, GameManager.def, GameManager.crit]
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	label.add_theme_font_size_override("font_size", 6)
	label.add_theme_color_override("font_color", COLOR_MUTED)
	panel.add_child(label)
	return panel


func _class_portrait_icon() -> AtlasTexture:
	var atlas_cell: Vector2i = CLASS_REGIONS.get(GameManager.selected_class, Vector2i.ZERO)
	var icon := AtlasTexture.new()
	icon.atlas = CLASS_ATLAS
	icon.region = Rect2(atlas_cell.x * 512, atlas_cell.y * 512, 512, 512)
	return icon


func _refresh_inventory() -> void:
	_inventory_count.text = "가방  %d/%d · 6열 보관함" % [GameManager.inventory.size(), GameManager.INVENTORY_CAPACITY]
	_clear_container(_inventory_grid)
	var sorted_items: Array[Dictionary] = GameManager.inventory.duplicate(true)
	sorted_items.sort_custom(func(a: Dictionary, b: Dictionary) -> bool:
		var rarity_a: int = int(a.get("rarity_index", 0))
		var rarity_b: int = int(b.get("rarity_index", 0))
		if rarity_a != rarity_b:
			return rarity_a > rarity_b
		return int(a.get("item_level", 1)) > int(b.get("item_level", 1))
	)
	if not sorted_items.any(func(item: Dictionary) -> bool: return String(item.get("id", "")) == _inventory_selected_id):
		_inventory_selected_id = String(sorted_items[0].get("id", "")) if not sorted_items.is_empty() else ""
	for index: int in GameManager.INVENTORY_CAPACITY:
		if index < sorted_items.size():
			var item: Dictionary = sorted_items[index]
			var item_color := Color.from_string(String(item.get("rarity_color", "ffffff")), Color.WHITE)
			var slot_button := Button.new()
			slot_button.custom_minimum_size = Vector2(42, 42)
			slot_button.icon = _item_icon(item)
			slot_button.expand_icon = true
			slot_button.tooltip_text = "%s\n\n더블클릭 또는 E: 장착" % _format_item_details(item)
			slot_button.add_theme_stylebox_override("normal", _style_box(Color("151018"), item_color, 1, 0))
			slot_button.add_theme_stylebox_override("hover", _style_box(Color("2b1b25"), item_color.lightened(0.2), 2, 0))
			slot_button.add_theme_stylebox_override("pressed", _style_box(Color("4b1f28"), item_color.lightened(0.25), 2, 0))
			if String(item.get("id", "")) == _inventory_selected_id:
				slot_button.add_theme_stylebox_override("normal", _style_box(Color("3b2028"), COLOR_GOLD, 2, 0))
				slot_button.add_theme_stylebox_override("hover", _style_box(Color("4b2730"), COLOR_GOLD.lightened(0.18), 2, 0))
			slot_button.pressed.connect(_select_inventory_item.bind(String(item.get("id", ""))))
			slot_button.focus_entered.connect(_focus_inventory_item.bind(String(item.get("id", ""))))
			slot_button.gui_input.connect(_on_inventory_slot_input.bind(String(item.get("id", ""))))
			_add_inventory_slot_labels(slot_button, item, String(item.get("id", "")) == _inventory_selected_id)
			_inventory_grid.add_child(slot_button)
		else:
			var empty_slot := Panel.new()
			empty_slot.custom_minimum_size = Vector2(42, 42)
			empty_slot.add_theme_stylebox_override("panel", _style_box(Color("0d0b11"), Color("2e2938"), 1, 0))
			_inventory_grid.add_child(empty_slot)
	_refresh_inventory_detail(sorted_items)


func _refresh_inventory_detail(items: Array[Dictionary]) -> void:
	var selected: Dictionary = {}
	for item: Dictionary in items:
		if String(item.get("id", "")) == _inventory_selected_id:
			selected = item
			break
	if selected.is_empty():
		_inventory_detail.text = "아이템을 획득하면 이곳에서\n능력치 비교 후 장착할 수 있습니다."
		_inventory_detail.add_theme_color_override("font_color", COLOR_MUTED)
		_inventory_equip_button.disabled = true
		_inventory_lock_button.disabled = true
		_inventory_lock_button.text = "잠금"
		_inventory_sell_button.disabled = true
		_inventory_sell_button.text = "판매"
		return
	var item_color := Color.from_string(String(selected.get("rarity_color", "ffffff")), Color.WHITE)
	_inventory_detail.text = "[%s] %s\n%s · iLv.%d\n%s\n%s" % [
		String(selected.get("rarity_name", "")), String(selected.get("name", "")),
		String(SLOT_NAMES.get(String(selected.get("slot", "")), "")), int(selected.get("item_level", 1)),
		_compact_stats(selected, true), _comparison_text(selected)
	]
	_inventory_detail.tooltip_text = _format_item_details(selected)
	_inventory_detail.add_theme_color_override("font_color", item_color)
	_inventory_equip_button.disabled = false
	_inventory_lock_button.disabled = false
	_inventory_lock_button.text = "잠금 해제" if bool(selected.get("locked", false)) else "잠금"
	_inventory_sell_button.disabled = bool(selected.get("locked", false))
	_inventory_sell_button.text = "잠금됨" if bool(selected.get("locked", false)) else "판매 %dG" % int(selected.get("sell_value", 0))


func _select_inventory_item(item_id: String) -> void:
	AudioManager.play_sfx("ui_click")
	_inventory_selected_id = item_id
	_refresh_inventory()


func _focus_inventory_item(item_id: String) -> void:
	if _inventory_selected_id == item_id:
		return
	_inventory_selected_id = item_id
	_refresh_inventory_detail(GameManager.inventory)


func _on_inventory_slot_input(event: InputEvent, item_id: String) -> void:
	if event is InputEventMouseButton:
		var mouse_event := event as InputEventMouseButton
		if mouse_event.button_index == MOUSE_BUTTON_LEFT and mouse_event.pressed and mouse_event.double_click:
			_inventory_selected_id = item_id
			_equip_selected_inventory()
			get_viewport().set_input_as_handled()


func _add_inventory_slot_labels(button: Button, item: Dictionary, selected: bool) -> void:
	var rarity_badge := Label.new()
	rarity_badge.position = Vector2(2, 1)
	rarity_badge.size = Vector2(38, 10)
	rarity_badge.mouse_filter = Control.MOUSE_FILTER_IGNORE
	rarity_badge.text = String(RARITY_BADGES.get(String(item.get("rarity_id", "normal")), "일반"))
	rarity_badge.add_theme_font_size_override("font_size", 5)
	rarity_badge.add_theme_color_override("font_color", Color.from_string(String(item.get("rarity_color", "ffffff")), Color.WHITE))
	button.add_child(rarity_badge)
	var level_badge := Label.new()
	level_badge.position = Vector2(22, 29)
	level_badge.size = Vector2(20, 10)
	level_badge.mouse_filter = Control.MOUSE_FILTER_IGNORE
	level_badge.text = "i%d" % int(item.get("item_level", 1))
	level_badge.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	level_badge.add_theme_font_size_override("font_size", 5)
	level_badge.add_theme_color_override("font_color", COLOR_MUTED)
	button.add_child(level_badge)
	if bool(item.get("locked", false)):
		var lock_badge := Label.new()
		lock_badge.position = Vector2(2, 19)
		lock_badge.size = Vector2(20, 9)
		lock_badge.mouse_filter = Control.MOUSE_FILTER_IGNORE
		lock_badge.text = "잠금"
		lock_badge.add_theme_font_size_override("font_size", 5)
		lock_badge.add_theme_color_override("font_color", COLOR_GOLD)
		button.add_child(lock_badge)
	if selected:
		var selected_badge := Label.new()
		selected_badge.position = Vector2(2, 29)
		selected_badge.size = Vector2(18, 10)
		selected_badge.mouse_filter = Control.MOUSE_FILTER_IGNORE
		selected_badge.text = "선택"
		selected_badge.add_theme_font_size_override("font_size", 5)
		selected_badge.add_theme_color_override("font_color", COLOR_GOLD)
		button.add_child(selected_badge)


func _equip_selected_inventory() -> void:
	if _inventory_selected_id.is_empty():
		return
	_equip(_inventory_selected_id)


func _sell_selected_inventory() -> void:
	if _inventory_selected_id.is_empty():
		return
	_sell(_inventory_selected_id)


func _toggle_selected_inventory_lock() -> void:
	if _inventory_selected_id.is_empty():
		return
	AudioManager.play_sfx("ui_click")
	GameManager.toggle_item_lock(_inventory_selected_id)


func _equipment_icon(slot: String) -> AtlasTexture:
	var atlas_cell: Vector2i = EQUIPMENT_REGIONS.get(slot, Vector2i.ZERO)
	var cell_size := Vector2(float(EQUIPMENT_ATLAS.get_width()) / 4.0, float(EQUIPMENT_ATLAS.get_height()) / 2.0)
	var icon := AtlasTexture.new()
	icon.atlas = EQUIPMENT_ATLAS
	icon.region = Rect2(Vector2(atlas_cell) * cell_size, cell_size)
	return icon


func _item_icon(item: Dictionary) -> AtlasTexture:
	var icon_index: int = int(item.get("icon_index", -1))
	if icon_index < 0 or icon_index >= 30:
		return _equipment_icon(String(item.get("slot", "weapon")))
	var cell_size := Vector2(float(ITEM_BASE_ATLAS.get_width()) / 6.0, float(ITEM_BASE_ATLAS.get_height()) / 5.0)
	var atlas_cell := Vector2i(icon_index % 6, floori(float(icon_index) / 6.0))
	var icon := AtlasTexture.new()
	icon.atlas = ITEM_BASE_ATLAS
	icon.region = Rect2(Vector2(atlas_cell) * cell_size, cell_size)
	return icon


func _refresh_skills() -> void:
	_clear_container(_skills_list)
	var hint := Label.new()
	hint.text = "%s의 지속 효과 · 골드로 강화" % GameManager.selected_class_name
	hint.add_theme_font_size_override("font_size", 7)
	hint.add_theme_color_override("font_color", COLOR_MUTED)
	_skills_list.add_child(hint)
	_add_skill_row("attack_boost", "ATK", "공격 강화", "모든 피해 +10% / Lv", 100, 80)
	_add_skill_row("defense_boost", "DEF", "방어 강화", "받는 피해 -5% / Lv", 120, 90)
	_add_skill_row("life_steal", "VAMP", "흡혈", "처치 시 최대 HP 3% / Lv 회복", 150, 100)


func _add_skill_row(skill_id: String, mark: String, title: String, description: String, base_cost: int, cost_step: int) -> void:
	var level: int = int(GameManager.skill_levels.get(skill_id, 0))
	var cost: int = base_cost + level * cost_step
	var panel := PanelContainer.new()
	panel.custom_minimum_size.y = 67
	panel.add_theme_stylebox_override("panel", _style_box(Color("171119"), Color("59443a"), 1, 0))
	_skills_list.add_child(panel)
	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 5)
	panel.add_child(row)
	var mark_panel := PanelContainer.new()
	mark_panel.custom_minimum_size = Vector2(50, 53)
	mark_panel.add_theme_stylebox_override("panel", _style_box(Color("251722"), COLOR_GOLD.darkened(0.28), 1, 0))
	row.add_child(mark_panel)
	var mark_label := Label.new()
	mark_label.text = mark
	mark_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	mark_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	mark_label.add_theme_font_size_override("font_size", 8)
	mark_label.add_theme_color_override("font_color", COLOR_GOLD)
	mark_panel.add_child(mark_label)
	var text_column := VBoxContainer.new()
	text_column.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	row.add_child(text_column)
	var title_label := Label.new()
	title_label.text = "%s  Lv.%d" % [title, level]
	title_label.add_theme_font_size_override("font_size", 9)
	text_column.add_child(title_label)
	var description_label := Label.new()
	description_label.text = description
	description_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	description_label.add_theme_font_size_override("font_size", 6)
	description_label.add_theme_color_override("font_color", COLOR_MUTED)
	text_column.add_child(description_label)
	var button := Button.new()
	button.text = "%dG\n강화" % cost
	button.custom_minimum_size = Vector2(70, 53)
	button.add_theme_font_size_override("font_size", 7)
	button.disabled = GameManager.gold < cost
	button.tooltip_text = "현재 골드: %dG" % GameManager.gold
	button.pressed.connect(_buy_skill.bind(skill_id, base_cost, cost_step))
	row.add_child(button)


func _refresh_rebirth() -> void:
	_clear_container(_rebirth_content)
	var info_panel := PanelContainer.new()
	info_panel.add_theme_stylebox_override("panel", _style_box(Color("171119"), Color("69432f"), 1, 0))
	_rebirth_content.add_child(info_panel)
	var left := VBoxContainer.new()
	info_panel.add_child(left)
	var info := Label.new()
	info.text = "환생 %d회  ·  영구 포인트 %d\n현재 Lv.%d / 필요 Lv.%d\n예상 보상  +%d 포인트\n레벨·골드·층·스킬·장비·가방 초기화" % [
		GameManager.rebirth_count, GameManager.rebirth_points, GameManager.level,
		RebirthManager.required_level(), RebirthManager.reward_points()
	]
	info.add_theme_font_size_override("font_size", 8)
	left.add_child(info)
	var rebirth_button := Button.new()
	rebirth_button.text = "환생하기"
	rebirth_button.custom_minimum_size.y = 26
	rebirth_button.disabled = not RebirthManager.can_rebirth()
	rebirth_button.pressed.connect(_rebirth)
	left.add_child(rebirth_button)

	var right := GridContainer.new()
	right.columns = 2
	right.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	right.add_theme_constant_override("h_separation", 5)
	right.add_theme_constant_override("v_separation", 5)
	_rebirth_content.add_child(right)
	_add_permanent_button(right, "atk", "ATK +2")
	_add_permanent_button(right, "def", "DEF +2")
	_add_permanent_button(right, "hp", "HP +10")
	_add_permanent_button(right, "spd", "SPD +0.05")


func _add_permanent_button(parent: GridContainer, stat_name: String, benefit: String) -> void:
	var button := Button.new()
	button.text = "%s\n강화 %d" % [benefit, int(GameManager.permanent_upgrades.get(stat_name, 0))]
	button.custom_minimum_size = Vector2(132, 43)
	button.disabled = GameManager.rebirth_points <= 0
	button.tooltip_text = "영구 포인트 1 소모"
	button.pressed.connect(_buy_permanent.bind(stat_name))
	parent.add_child(button)


func _refresh_stats() -> void:
	_stats_label.text = "모험 기록\n처치  %d     획득 골드  %dG\n최고 층  %d     환생  %d회     드롭  %d\n\n전투 능력\nATK  %d     DEF  %d\nHP  %d/%d     MP  %d/%d\nSPD  %.2f     CRIT  %.1f%%\n\n보조 능력\n흡혈  %.1f%%     경험치  +%.1f%%\n골드  +%.1f%%     관통  %.1f%%" % [
		int(GameManager.statistics.get("total_kills", 0)), int(GameManager.statistics.get("total_gold_earned", 0)),
		int(GameManager.statistics.get("highest_floor", 1)), GameManager.rebirth_count,
		int(GameManager.statistics.get("total_drops", 0)), GameManager.atk, GameManager.def,
		GameManager.hp, GameManager.max_hp, GameManager.mp, GameManager.max_mp, GameManager.spd,
		GameManager.crit, GameManager.vamp, GameManager.xp_bonus, GameManager.gold_bonus, GameManager.penetration
	]


func _compact_stats(item: Dictionary, include_affixes: bool = false) -> String:
	var parts := PackedStringArray()
	var base_stats: Dictionary = item.get("base_stats", {})
	for stat_name: Variant in base_stats.keys():
		parts.append("%s +%s" % [String(STAT_NAMES.get(String(stat_name), stat_name)), _format_value(String(stat_name), float(base_stats[stat_name]))])
	if include_affixes:
		for affix_data: Variant in Array(item.get("affixes", [])):
			if affix_data is Dictionary:
				parts.append("%s %s" % [String(affix_data.get("name", "")), _format_value(String(affix_data.get("stat", "")), float(affix_data.get("value", 0.0)))])
	return " · ".join(parts)


func _format_item_details(item: Dictionary) -> String:
	var lines := PackedStringArray([
		"[%s] %s" % [String(item.get("rarity_name", "")), String(item.get("name", ""))],
		"%s · 아이템 레벨 %d" % [String(SLOT_NAMES.get(String(item.get("slot", "")), "")), int(item.get("item_level", 1))],
		"기본: %s" % _compact_stats(item),
	])
	for affix_data: Variant in Array(item.get("affixes", [])):
		if affix_data is Dictionary:
			lines.append("%s %s +%s" % [
				String(affix_data.get("name", "")), String(STAT_NAMES.get(String(affix_data.get("stat", "")), affix_data.get("stat", ""))),
				_format_value(String(affix_data.get("stat", "")), float(affix_data.get("value", 0.0)))
			])
	lines.append("판매가: %dG%s" % [int(item.get("sell_value", 0)), " · 잠금" if bool(item.get("locked", false)) else ""])
	lines.append("장착 비교: %s" % _comparison_text(item))
	return "\n".join(lines)


func _comparison_text(item: Dictionary) -> String:
	var slot: String = String(item.get("slot", ""))
	var equipped: Dictionary = GameManager.equipment.get(slot, {})
	if equipped.is_empty():
		return "현재 슬롯 비어 있음 · 즉시 장착 가능"
	var candidate_stats: Dictionary = _item_stat_totals(item)
	var equipped_stats: Dictionary = _item_stat_totals(equipped)
	var changes := PackedStringArray()
	for stat_name: String in ["ATK", "DEF", "HP", "MP", "SPD", "CRIT", "VAMP", "PEN"]:
		var difference: float = float(candidate_stats.get(stat_name, 0.0)) - float(equipped_stats.get(stat_name, 0.0))
		if is_zero_approx(difference):
			continue
		var marker: String = "▲" if difference > 0.0 else "▼"
		var sign_text: String = "+" if difference > 0.0 else ""
		changes.append("%s %s %s%s" % [marker, String(STAT_NAMES.get(stat_name, stat_name)), sign_text, _format_value(stat_name, difference)])
		if changes.size() >= 4:
			break
	return "변화 없음" if changes.is_empty() else "  ".join(changes)


func _item_stat_totals(item: Dictionary) -> Dictionary:
	var totals: Dictionary = {}
	var base_stats: Dictionary = item.get("base_stats", {})
	for stat_name: Variant in base_stats.keys():
		totals[String(stat_name)] = float(totals.get(String(stat_name), 0.0)) + float(base_stats[stat_name])
	for affix_data: Variant in Array(item.get("affixes", [])):
		if affix_data is Dictionary:
			var stat_name: String = String(affix_data.get("stat", ""))
			totals[stat_name] = float(totals.get(stat_name, 0.0)) + float(affix_data.get("value", 0.0))
	return totals


func _format_value(stat_name: String, value: float) -> String:
	return "%.2f" % value if stat_name == "SPD" else "%d" % roundi(value)


func _on_stats_changed() -> void:
	_refresh_hud()
	_refresh_skills()
	_refresh_rebirth()
	_refresh_stats()


func _on_equipment_changed() -> void:
	_refresh_equipment()
	_refresh_inventory()
	_refresh_stats()


func _on_rebirth_changed() -> void:
	_refresh_all()


func _on_game_state_changed(state: GameManager.GameState) -> void:
	_apply_game_state_visibility(state)
	_class_selection.visible = state == GameManager.GameState.CLASS_SELECTION
	if _class_selection.visible:
		_class_selection.refresh()


func _apply_game_state_visibility(state: GameManager.GameState) -> void:
	var show_game_ui: bool = state != GameManager.GameState.CLASS_SELECTION
	if _hud_panel != null:
		_hud_panel.visible = show_game_ui
	if _bottom_panel != null:
		_bottom_panel.visible = show_game_ui
	if not show_game_ui:
		_management_open = false
	if _management_window != null:
		_management_window.visible = show_game_ui and _management_open
	_sync_dock_buttons()


func _on_loot_filter_selected(index: int) -> void:
	GameManager.set_loot_min_rarity(index)
	SaveManager.save_game()


func _manual_save() -> void:
	var err: Error = SaveManager.save_game()
	if err == OK:
		_show_notification("게임 저장 완료", COLOR_GREEN)
	else:
		_show_notification("저장 실패", Color("ff6b6b"))


func _manual_load() -> void:
	var data: Dictionary = SaveManager.load_game()
	if data.is_empty():
		_show_notification("불러올 저장 데이터가 없습니다.", Color("ffb86b"))
	else:
		_refresh_all()
		_show_notification("저장 데이터 불러오기 완료", COLOR_GREEN)


func _save_and_quit() -> void:
	SaveManager.save_game()
	get_tree().quit()


func _set_speed(multiplier: float) -> void:
	AudioManager.play_sfx("ui_click")
	GameManager.set_speed_multiplier(multiplier)


func _equip(item_id: String) -> void:
	AudioManager.play_sfx("ui_click")
	GameManager.equip_item(item_id)


func _unequip(slot: String) -> void:
	AudioManager.play_sfx("ui_click")
	GameManager.unequip_item(slot)


func _sell(item_id: String) -> void:
	AudioManager.play_sfx("ui_click")
	GameManager.sell_item(item_id)


func _sell_all_normal() -> void:
	AudioManager.play_sfx("ui_click")
	GameManager.sell_all_normal()


func _buy_skill(skill_id: String, base_cost: int, cost_step: int) -> void:
	AudioManager.play_sfx("ui_click")
	if GameManager.buy_skill(skill_id, base_cost, cost_step):
		_show_notification("%s 강화 완료" % skill_id, COLOR_GREEN)


func _rebirth() -> void:
	AudioManager.play_sfx("ui_click")
	RebirthManager.rebirth()


func _buy_permanent(stat_name: String) -> void:
	AudioManager.play_sfx("ui_click")
	RebirthManager.purchase_permanent_upgrade(stat_name)


func _show_notification(message: String, color: Color) -> void:
	_notification_label.text = message
	_notification_label.add_theme_color_override("font_color", color)
	_notification_label.modulate.a = 1.0
	if _notification_tween != null and _notification_tween.is_valid():
		_notification_tween.kill()
	_notification_tween = create_tween()
	_notification_tween.tween_interval(1.15)
	_notification_tween.tween_property(_notification_label, "modulate:a", 0.0, 0.35)


func _clear_container(container: Container) -> void:
	if container == null:
		return
	for child: Node in container.get_children():
		container.remove_child(child)
		child.queue_free()
