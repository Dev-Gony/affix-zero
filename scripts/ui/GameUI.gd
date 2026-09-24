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

var _hp_bar: ProgressBar
var _mp_bar: ProgressBar
var _xp_bar: ProgressBar
var _hud_info: Label
var _speed_buttons: Dictionary = {}
var _equipment_row: HBoxContainer
var _inventory_grid: GridContainer
var _inventory_count: Label
var _inventory_detail: Label
var _inventory_equip_button: Button
var _inventory_sell_button: Button
var _inventory_selected_id: String = ""
var _skills_list: VBoxContainer
var _rebirth_content: HBoxContainer
var _stats_label: Label
var _notification_label: Label
var _notification_tween: Tween
var _class_selection: ClassSelection
var _hud_panel: Panel
var _bottom_panel: Panel
var _main_tabs: TabContainer


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
	panel.size = Vector2(640, 42)
	panel.add_theme_stylebox_override("panel", _style_box(Color(0.035, 0.025, 0.045, 0.94), Color("8f5a3a"), 2, 0))
	add_child(panel)
	var row := HBoxContainer.new()
	row.position = Vector2(7, 3)
	row.size = Vector2(626, 35)
	row.add_theme_constant_override("separation", 8)
	panel.add_child(row)

	var bars := VBoxContainer.new()
	bars.custom_minimum_size = Vector2(232, 34)
	bars.add_theme_constant_override("separation", 1)
	row.add_child(bars)
	_hp_bar = _add_bar(bars, "HP", Color("dc2626"))
	_mp_bar = _add_bar(bars, "MP", Color("2563eb"))
	_xp_bar = _add_bar(bars, "XP", Color("22c55e"))

	_hud_info = Label.new()
	_hud_info.custom_minimum_size = Vector2(230, 34)
	_hud_info.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	_hud_info.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_hud_info.add_theme_font_size_override("font_size", 11)
	row.add_child(_hud_info)

	var speed_row := HBoxContainer.new()
	speed_row.custom_minimum_size = Vector2(146, 34)
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
		speed_button.custom_minimum_size = Vector2(32, 28)
		speed_button.toggle_mode = true
		speed_button.tooltip_text = "전투 속도를 x%d로 변경" % int(multiplier)
		speed_button.pressed.connect(_set_speed.bind(multiplier))
		speed_row.add_child(speed_button)
		_speed_buttons[multiplier] = speed_button


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
	var panel := Panel.new()
	_bottom_panel = panel
	panel.position = Vector2(0, 214)
	panel.size = Vector2(640, 146)
	panel.add_theme_stylebox_override("panel", _style_box(Color("100c12"), Color("9a6240"), 2, 0))
	add_child(panel)
	var tabs := TabContainer.new()
	_main_tabs = tabs
	tabs.name = "MainTabs"
	tabs.position = Vector2(4, 3)
	tabs.size = Vector2(632, 140)
	tabs.tab_changed.connect(func(_tab: int) -> void: AudioManager.play_sfx("ui_click"))
	panel.add_child(tabs)

	_build_equipment_tab(tabs)
	_build_inventory_tab(tabs)
	_build_skills_tab(tabs)
	_build_rebirth_tab(tabs)
	_build_stats_tab(tabs)


func _unhandled_key_input(event: InputEvent) -> void:
	if not event is InputEventKey or not event.pressed or event.echo:
		return
	if GameManager.game_state != GameManager.GameState.RUNNING:
		return
	match event.keycode:
		KEY_1, KEY_2, KEY_3, KEY_4, KEY_5:
			_main_tabs.current_tab = int(event.keycode - KEY_1)
			AudioManager.play_sfx("ui_click")
		KEY_Z: _set_speed(1.0)
		KEY_X: _set_speed(2.0)
		KEY_C: _set_speed(5.0)


func _build_equipment_tab(tabs: TabContainer) -> void:
	var tab := MarginContainer.new()
	tab.name = "장비"
	tab.add_theme_constant_override("margin_left", 4)
	tab.add_theme_constant_override("margin_right", 4)
	tab.add_theme_constant_override("margin_top", 4)
	tab.add_theme_constant_override("margin_bottom", 4)
	tabs.add_child(tab)
	var scroll := ScrollContainer.new()
	scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	scroll.vertical_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	tab.add_child(scroll)
	_equipment_row = HBoxContainer.new()
	_equipment_row.add_theme_constant_override("separation", 5)
	scroll.add_child(_equipment_row)


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
	var sell_all := Button.new()
	sell_all.text = "일반 일괄 판매"
	sell_all.custom_minimum_size = Vector2(105, 21)
	sell_all.pressed.connect(_sell_all_normal)
	header.add_child(sell_all)
	var content := HBoxContainer.new()
	content.size_flags_vertical = Control.SIZE_EXPAND_FILL
	content.add_theme_constant_override("separation", 5)
	tab.add_child(content)
	var grid_panel := PanelContainer.new()
	grid_panel.custom_minimum_size = Vector2(452, 0)
	grid_panel.add_theme_stylebox_override("panel", _style_box(Color("0b0910"), Color("352b38"), 1, 0))
	content.add_child(grid_panel)
	_inventory_grid = GridContainer.new()
	_inventory_grid.columns = 10
	_inventory_grid.add_theme_constant_override("h_separation", 2)
	_inventory_grid.add_theme_constant_override("v_separation", 2)
	grid_panel.add_child(_inventory_grid)
	var detail_panel := PanelContainer.new()
	detail_panel.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	detail_panel.add_theme_stylebox_override("panel", _style_box(Color("171119"), Color("69432f"), 1, 0))
	content.add_child(detail_panel)
	var detail_column := VBoxContainer.new()
	detail_column.add_theme_constant_override("separation", 2)
	detail_panel.add_child(detail_column)
	_inventory_detail = Label.new()
	_inventory_detail.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_inventory_detail.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	_inventory_detail.add_theme_font_size_override("font_size", 7)
	detail_column.add_child(_inventory_detail)
	var actions := HBoxContainer.new()
	actions.add_theme_constant_override("separation", 3)
	detail_column.add_child(actions)
	_inventory_equip_button = Button.new()
	_inventory_equip_button.text = "장착"
	_inventory_equip_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_inventory_equip_button.custom_minimum_size.y = 22
	_inventory_equip_button.pressed.connect(_equip_selected_inventory)
	actions.add_child(_inventory_equip_button)
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
	_rebirth_content = HBoxContainer.new()
	_rebirth_content.add_theme_constant_override("separation", 8)
	tab.add_child(_rebirth_content)


func _build_stats_tab(tabs: TabContainer) -> void:
	var tab := MarginContainer.new()
	tab.name = "정보"
	tab.add_theme_constant_override("margin_left", 8)
	tab.add_theme_constant_override("margin_right", 8)
	tab.add_theme_constant_override("margin_top", 7)
	tabs.add_child(tab)
	_stats_label = Label.new()
	_stats_label.vertical_alignment = VERTICAL_ALIGNMENT_TOP
	_stats_label.add_theme_font_size_override("font_size", 10)
	tab.add_child(_stats_label)


func _build_notification() -> void:
	_notification_label = Label.new()
	_notification_label.position = Vector2(145, 47)
	_notification_label.size = Vector2(350, 24)
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
	for slot: String in GameManager.EQUIPMENT_SLOTS:
		var item: Dictionary = GameManager.equipment.get(slot, {})
		var card := PanelContainer.new()
		card.custom_minimum_size = Vector2(83, 84)
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
		slot_label.text = "◆ %s" % String(SLOT_NAMES.get(slot, slot))
		slot_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		slot_label.add_theme_font_size_override("font_size", 6)
		slot_label.add_theme_color_override("font_color", COLOR_MUTED)
		content.add_child(slot_label)
		var icon := TextureRect.new()
		icon.texture = _equipment_icon(slot)
		icon.custom_minimum_size = Vector2(0, 22)
		icon.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		icon.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		icon.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
		icon.modulate = Color.WHITE if not item.is_empty() else Color(0.32, 0.30, 0.38, 0.65)
		content.add_child(icon)
		var item_label := Label.new()
		item_label.size_flags_vertical = Control.SIZE_EXPAND_FILL
		item_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		item_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
		item_label.custom_minimum_size.y = 16
		item_label.clip_text = true
		item_label.text_overrun_behavior = TextServer.OVERRUN_TRIM_ELLIPSIS
		item_label.add_theme_font_size_override("font_size", 6)
		if item.is_empty():
			item_label.text = "비어 있음"
			item_label.add_theme_color_override("font_color", Color("70708a"))
		else:
			item_label.text = "%s · %d" % [String(item.get("name", "")), int(item.get("item_level", 1))]
			item_label.tooltip_text = _format_item_details(item)
			item_label.add_theme_color_override("font_color", border_color)
		content.add_child(item_label)
		var action := Button.new()
		action.text = "해제"
		action.custom_minimum_size.y = 18
		action.add_theme_font_size_override("font_size", 6)
		action.add_theme_stylebox_override("normal", _compact_style_box(Color("211720"), Color("69432f")))
		action.add_theme_stylebox_override("hover", _compact_style_box(Color("38202a"), Color("c17a43")))
		action.add_theme_stylebox_override("pressed", _compact_style_box(Color("4b1f28"), Color("e04f46"), 2))
		action.add_theme_stylebox_override("disabled", _compact_style_box(Color("100d13"), Color("35271f")))
		action.add_theme_stylebox_override("focus", _compact_style_box(Color(0, 0, 0, 0), COLOR_GOLD, 2))
		action.disabled = item.is_empty()
		action.pressed.connect(_unequip.bind(slot))
		content.add_child(action)
		_equipment_row.add_child(card)


func _refresh_inventory() -> void:
	_inventory_count.text = "가방  %d/%d" % [GameManager.inventory.size(), GameManager.INVENTORY_CAPACITY]
	_clear_container(_inventory_grid)
	var sorted_items: Array[Dictionary] = GameManager.inventory.duplicate(true)
	sorted_items.sort_custom(func(a: Dictionary, b: Dictionary) -> bool: return int(a.get("rarity_index", 0)) > int(b.get("rarity_index", 0)))
	if not sorted_items.any(func(item: Dictionary) -> bool: return String(item.get("id", "")) == _inventory_selected_id):
		_inventory_selected_id = String(sorted_items[0].get("id", "")) if not sorted_items.is_empty() else ""
	for index: int in GameManager.INVENTORY_CAPACITY:
		if index < sorted_items.size():
			var item: Dictionary = sorted_items[index]
			var item_color := Color.from_string(String(item.get("rarity_color", "ffffff")), Color.WHITE)
			var slot_button := Button.new()
			slot_button.custom_minimum_size = Vector2(42, 42)
			slot_button.icon = _equipment_icon(String(item.get("slot", "")))
			slot_button.expand_icon = true
			slot_button.tooltip_text = _format_item_details(item)
			slot_button.add_theme_stylebox_override("normal", _style_box(Color("151018"), item_color, 1, 0))
			slot_button.add_theme_stylebox_override("hover", _style_box(Color("2b1b25"), item_color.lightened(0.2), 2, 0))
			if String(item.get("id", "")) == _inventory_selected_id:
				slot_button.add_theme_stylebox_override("normal", _style_box(Color("3b2028"), COLOR_GOLD, 2, 0))
			slot_button.pressed.connect(_select_inventory_item.bind(String(item.get("id", ""))))
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
	_inventory_sell_button.disabled = false
	_inventory_sell_button.text = "판매 %dG" % int(selected.get("sell_value", 0))


func _select_inventory_item(item_id: String) -> void:
	AudioManager.play_sfx("ui_click")
	_inventory_selected_id = item_id
	_refresh_inventory()


func _equip_selected_inventory() -> void:
	if _inventory_selected_id.is_empty():
		return
	_equip(_inventory_selected_id)


func _sell_selected_inventory() -> void:
	if _inventory_selected_id.is_empty():
		return
	_sell(_inventory_selected_id)


func _equipment_icon(slot: String) -> AtlasTexture:
	var atlas_cell: Vector2i = EQUIPMENT_REGIONS.get(slot, Vector2i.ZERO)
	var cell_size := Vector2(float(EQUIPMENT_ATLAS.get_width()) / 4.0, float(EQUIPMENT_ATLAS.get_height()) / 2.0)
	var icon := AtlasTexture.new()
	icon.atlas = EQUIPMENT_ATLAS
	icon.region = Rect2(Vector2(atlas_cell) * cell_size, cell_size)
	return icon


func _refresh_skills() -> void:
	_clear_container(_skills_list)
	_add_skill_row("attack_boost", "공격 강화", "피해량 +10%/Lv", 100, 80)
	_add_skill_row("defense_boost", "방어 강화", "받는 피해 -5%/Lv", 120, 90)
	_add_skill_row("life_steal", "흡혈", "처치 시 최대 HP 3%/Lv 회복", 150, 100)


func _add_skill_row(skill_id: String, title: String, description: String, base_cost: int, cost_step: int) -> void:
	var level: int = int(GameManager.skill_levels.get(skill_id, 0))
	var cost: int = base_cost + level * cost_step
	var row := HBoxContainer.new()
	row.custom_minimum_size.y = 27
	_skills_list.add_child(row)
	var label := Label.new()
	label.text = "%s  Lv.%d   %s" % [title, level, description]
	label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	row.add_child(label)
	var button := Button.new()
	button.text = "%dG 구매" % cost
	button.custom_minimum_size = Vector2(92, 25)
	button.disabled = GameManager.gold < cost
	button.tooltip_text = "현재 골드: %dG" % GameManager.gold
	button.pressed.connect(_buy_skill.bind(skill_id, base_cost, cost_step))
	row.add_child(button)


func _refresh_rebirth() -> void:
	_clear_container(_rebirth_content)
	var left := VBoxContainer.new()
	left.custom_minimum_size = Vector2(285, 100)
	_rebirth_content.add_child(left)
	var info := Label.new()
	info.text = "환생 %d회  ·  보유 포인트 %d\n필요 레벨: %d  ·  현재 레벨: %d\n예상 보상: 영구 포인트 +%d\n환생 시 레벨·골드·층·스킬·장비·가방 초기화" % [
		GameManager.rebirth_count, GameManager.rebirth_points, RebirthManager.required_level(),
		GameManager.level, RebirthManager.reward_points()
	]
	info.size_flags_vertical = Control.SIZE_EXPAND_FILL
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
	button.custom_minimum_size = Vector2(145, 43)
	button.disabled = GameManager.rebirth_points <= 0
	button.tooltip_text = "영구 포인트 1 소모"
	button.pressed.connect(_buy_permanent.bind(stat_name))
	parent.add_child(button)


func _refresh_stats() -> void:
	_stats_label.text = "누적 처치  %d     누적 골드  %dG     최고 층  %d     환생  %d회     총 드롭  %d\n\n현재 능력치    ATK %d     DEF %d     HP %d/%d     MP %d/%d     SPD %.2f     CRIT %.1f%%\n보조 능력치    흡혈 %.1f%%     경험치 +%.1f%%     골드 +%.1f%%     관통 %.1f%%" % [
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
	lines.append("판매가: %dG" % int(item.get("sell_value", 0)))
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
