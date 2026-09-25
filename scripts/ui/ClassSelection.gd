extends Panel
class_name ClassSelection

signal class_chosen(class_id: String)

const CLASS_PATHS: Array[String] = [
	"res://resources/classes/warrior.tres",
	"res://resources/classes/mage.tres",
	"res://resources/classes/knight.tres",
	"res://resources/classes/sage.tres",
	"res://resources/classes/assassin.tres",
	"res://resources/classes/saint.tres",
]
const CLASS_ATLAS: Texture2D = preload("res://assets/sprites/class_atlas_alpha.png")
const CLASS_REGIONS: Dictionary = {
	"warrior": Vector2i(0, 0), "mage": Vector2i(1, 0), "knight": Vector2i(2, 0),
	"sage": Vector2i(0, 1), "assassin": Vector2i(1, 1), "saint": Vector2i(2, 1),
}

var _grid: GridContainer


func _ready() -> void:
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_STOP
	z_index = 50
	_build_layout()
	refresh()


func refresh() -> void:
	if _grid == null:
		return
	for child: Node in _grid.get_children():
		_grid.remove_child(child)
		child.queue_free()
	var first_unlocked_card: Button = null
	for resource_path: String in CLASS_PATHS:
		var class_data: ClassData = load(resource_path)
		if class_data == null:
			continue
		var unlocked: bool = GameManager.unlocked_classes.has(class_data.id)
		var card := Button.new()
		card.custom_minimum_size = Vector2(184, 102)
		card.disabled = not unlocked
		card.text = _class_card_text(class_data, unlocked)
		card.icon = _make_class_icon(class_data)
		card.expand_icon = true
		card.alignment = HORIZONTAL_ALIGNMENT_LEFT
		card.tooltip_text = "%s\nHP %d · MP %d · ATK %d · DEF %d · SPD %.1f · CRIT %.0f%%" % [
			class_data.description, class_data.base_hp, class_data.base_mp, class_data.base_atk,
			class_data.base_def, class_data.base_spd, class_data.base_crit
		]
		card.add_theme_color_override("font_color", Color("edf2f7") if unlocked else Color("667386"))
		card.add_theme_color_override("font_disabled_color", Color("667386"))
		card.add_theme_font_size_override("font_size", 9)
		card.add_theme_stylebox_override("normal", _card_style(Color("111821"), class_data.color.darkened(0.10), 2))
		card.add_theme_stylebox_override("hover", _card_style(Color("1d2733"), class_data.color.lightened(0.12), 2))
		card.add_theme_stylebox_override("pressed", _card_style(Color("27313e"), Color("f0b84b"), 3))
		card.add_theme_stylebox_override("disabled", _card_style(Color("0b1016"), Color("2b3542"), 1))
		card.add_theme_stylebox_override("focus", _card_style(Color(0, 0, 0, 0), Color("f0b84b"), 2))
		if unlocked:
			card.pressed.connect(_select_class.bind(class_data))
			if first_unlocked_card == null:
				first_unlocked_card = card
		_grid.add_child(card)
	visible = GameManager.game_state == GameManager.GameState.CLASS_SELECTION
	if visible and first_unlocked_card != null:
		call_deferred("_focus_card", first_unlocked_card)


func _focus_card(card: Button) -> void:
	if is_instance_valid(card) and card.is_inside_tree() and card.visible and not card.disabled:
		card.grab_focus()


func _build_layout() -> void:
	var outer := VBoxContainer.new()
	outer.set_anchors_and_offsets_preset(Control.PRESET_CENTER)
	outer.position = Vector2(-296, -168)
	outer.size = Vector2(592, 336)
	outer.add_theme_constant_override("separation", 10)
	add_child(outer)

	var title := Label.new()
	title.text = "AFFIX: ZERO  ·  영웅 선택"
	title.custom_minimum_size.y = 30
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 24)
	title.add_theme_color_override("font_color", Color("f0b84b"))
	outer.add_child(title)

	var subtitle := Label.new()
	subtitle.text = "자동사냥 성장형 ARPG  ·  직업마다 기본 능력과 성장 방향이 다릅니다"
	subtitle.custom_minimum_size.y = 18
	subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	subtitle.add_theme_color_override("font_color", Color("9aa8ba"))
	outer.add_child(subtitle)

	_grid = GridContainer.new()
	_grid.columns = 3
	_grid.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_grid.add_theme_constant_override("h_separation", 10)
	_grid.add_theme_constant_override("v_separation", 10)
	outer.add_child(_grid)

	var hint := Label.new()
	hint.text = "첫 플레이 추천: 전사 / 마법사   ·   잠긴 직업은 환생 누적으로 해금"
	hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	hint.add_theme_font_size_override("font_size", 9)
	hint.add_theme_color_override("font_color", Color("8492a6"))
	outer.add_child(hint)


func _class_card_text(class_data: ClassData, unlocked: bool) -> String:
	var status: String = "선택 가능" if unlocked else "환생 %d회 해금" % class_data.unlock_rebirths
	return "%s\n%s\nHP %d   ATK %d   DEF %d\nSPD %.1f   CRIT %.0f%%\n%s" % [
		class_data.display_name, class_data.description, class_data.base_hp, class_data.base_atk,
		class_data.base_def, class_data.base_spd, class_data.base_crit, status
	]


func _card_style(background: Color, border: Color, width: int) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = background
	style.border_color = border
	style.set_border_width_all(width)
	style.content_margin_left = 7
	style.content_margin_right = 7
	style.content_margin_top = 6
	style.content_margin_bottom = 6
	style.set_corner_radius_all(3)
	style.anti_aliasing = false
	return style


func _make_class_icon(class_data: ClassData) -> Texture2D:
	var atlas_cell: Vector2i = CLASS_REGIONS.get(class_data.id, Vector2i.ZERO)
	var cell_size := Vector2(float(CLASS_ATLAS.get_width()) / 3.0, float(CLASS_ATLAS.get_height()) / 2.0)
	var icon := AtlasTexture.new()
	icon.atlas = CLASS_ATLAS
	icon.region = Rect2(Vector2(atlas_cell) * cell_size, cell_size)
	return icon


func _select_class(class_data: ClassData) -> void:
	AudioManager.play_sfx("ui_click")
	GameManager.select_class(class_data)
	visible = false
	class_chosen.emit(class_data.id)
