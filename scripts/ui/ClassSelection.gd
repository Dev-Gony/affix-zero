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
	for resource_path: String in CLASS_PATHS:
		var class_data: ClassData = load(resource_path)
		if class_data == null:
			continue
		var unlocked: bool = GameManager.unlocked_classes.has(class_data.id)
		var card := Button.new()
		card.custom_minimum_size = Vector2(145, 82)
		card.disabled = not unlocked
		card.text = _class_card_text(class_data, unlocked)
		card.tooltip_text = "%s\nHP %d · MP %d · ATK %d · DEF %d · SPD %.1f · CRIT %.0f%%" % [
			class_data.description, class_data.base_hp, class_data.base_mp, class_data.base_atk,
			class_data.base_def, class_data.base_spd, class_data.base_crit
		]
		card.add_theme_color_override("font_color", class_data.color if unlocked else Color("65657c"))
		card.add_theme_color_override("font_disabled_color", Color("65657c"))
		if unlocked:
			card.pressed.connect(_select_class.bind(class_data))
		_grid.add_child(card)
	visible = GameManager.game_state == GameManager.GameState.CLASS_SELECTION


func _build_layout() -> void:
	var outer := VBoxContainer.new()
	outer.set_anchors_and_offsets_preset(Control.PRESET_CENTER)
	outer.position = Vector2(-250, -155)
	outer.size = Vector2(500, 310)
	outer.add_theme_constant_override("separation", 8)
	add_child(outer)

	var title := Label.new()
	title.text = "AFFIX: ZERO"
	title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	title.add_theme_font_size_override("font_size", 22)
	title.add_theme_color_override("font_color", Color("f3f4ff"))
	outer.add_child(title)

	var subtitle := Label.new()
	subtitle.text = "운명을 선택하세요 · 자동 전투가 곧 시작됩니다"
	subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	subtitle.add_theme_color_override("font_color", Color("a9abc4"))
	outer.add_child(subtitle)

	_grid = GridContainer.new()
	_grid.columns = 3
	_grid.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_grid.add_theme_constant_override("h_separation", 8)
	_grid.add_theme_constant_override("v_separation", 8)
	outer.add_child(_grid)

	var hint := Label.new()
	hint.text = "환생 횟수가 늘어나면 새로운 직업이 해금됩니다"
	hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	hint.add_theme_font_size_override("font_size", 9)
	hint.add_theme_color_override("font_color", Color("8b8da8"))
	outer.add_child(hint)


func _class_card_text(class_data: ClassData, unlocked: bool) -> String:
	var status: String = "선택 가능" if unlocked else "환생 %d회 해금" % class_data.unlock_rebirths
	return "%s\n%s\nHP %d  ATK %d  DEF %d\nSPD %.1f  CRIT %.0f%%\n%s" % [
		class_data.display_name, class_data.description, class_data.base_hp, class_data.base_atk,
		class_data.base_def, class_data.base_spd, class_data.base_crit, status
	]


func _select_class(class_data: ClassData) -> void:
	AudioManager.play_sfx("ui_click")
	GameManager.select_class(class_data)
	visible = false
	class_chosen.emit(class_data.id)
