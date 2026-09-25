extends Control
class_name SummonResultPanel

signal dismissed

var receipt: Dictionary = {}
var result_cards: Array[Control] = []
var _panel: PanelContainer
var _close_button: Button


func _ready() -> void:
	name = "SummonResults"
	process_mode = Node.PROCESS_MODE_ALWAYS
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	z_index = 150
	mouse_filter = Control.MOUSE_FILTER_STOP
	visible = false


func present(data: Dictionary, save_status: String = "saved") -> void:
	receipt = data.duplicate(true)
	for child: Node in get_children():
		remove_child(child)
		child.queue_free()
	result_cards.clear()
	var shade := ColorRect.new()
	shade.color = Color(0.01, 0.02, 0.035, 0.94)
	shade.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	shade.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(shade)
	_panel = PanelContainer.new()
	_panel.position = Vector2(24, 22)
	_panel.size = Vector2(592, 356)
	_panel.add_theme_stylebox_override("panel", _style(Color("101722"), Color("d7aa51")))
	add_child(_panel)
	var column := VBoxContainer.new()
	column.add_theme_constant_override("separation", 7)
	_panel.add_child(column)
	var results: Array = Array(receipt.get("results", []))
	var title := Label.new()
	title.name = "Title"
	title.text = "소환 결과  %d회" % results.size()
	title.add_theme_font_size_override("font_size", 17)
	title.add_theme_color_override("font_color", Color("f0b84b"))
	column.add_child(title)
	var balance := Label.new()
	balance.name = "CurrencyReceipt"
	balance.text = "소환석  %d  -  %d  =  %d   |   전설 이상 확정까지 %d회" % [
		int(receipt.get("crystals_before", 0)), int(receipt.get("cost", 0)),
		int(receipt.get("crystals_after", 0)),
		PetManager.LEGENDARY_PITY - int(receipt.get("pity", 0)),
	]
	balance.add_theme_font_size_override("font_size", 8)
	column.add_child(balance)
	var grid := GridContainer.new()
	grid.name = "ResultGrid"
	grid.columns = 5 if results.size() > 1 else 1
	grid.size_flags_vertical = Control.SIZE_EXPAND_FILL
	grid.size_flags_horizontal = Control.SIZE_SHRINK_CENTER
	grid.add_theme_constant_override("h_separation", 5)
	grid.add_theme_constant_override("v_separation", 5)
	column.add_child(grid)
	for value: Variant in results:
		if value is Dictionary:
			var card: Control = _result_card(value as Dictionary, results.size() == 1)
			grid.add_child(card)
			result_cards.append(card)
	var status := Label.new()
	status.name = "SaveStatus"
	status.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	status.add_theme_font_size_override("font_size", 8)
	if save_status == "failed":
		status.text = "저장 실패: 보상은 현재 게임에 반영됐습니다. 종료 전 메뉴에서 저장을 다시 확인하세요."
		status.add_theme_color_override("font_color", Color("ff8b82"))
	elif save_status == "history":
		status.text = "최근 소환 내역입니다. 다시 열어도 재화는 소모되지 않습니다."
	else:
		status.text = "보상 반영 완료" + (" · 저장 완료" if save_status == "saved" else " · 테스트 모드")
		status.add_theme_color_override("font_color", Color("70dfab"))
	column.add_child(status)
	_close_button = Button.new()
	_close_button.name = "Confirm"
	_close_button.text = "확인 · 펫 목록으로   ESC"
	_close_button.custom_minimum_size.y = 27
	_close_button.pressed.connect(close)
	column.add_child(_close_button)
	visible = true
	_close_button.grab_focus()


func _result_card(result: Dictionary, single: bool) -> Control:
	var card := PanelContainer.new()
	card.name = "Result_%d" % result_cards.size()
	card.set_meta("result", result.duplicate(true))
	card.custom_minimum_size = Vector2(230, 210) if single else Vector2(106, 102)
	var color := Color.from_string(String(result.get("rarity_color", "b9c6d4")), Color.WHITE)
	card.add_theme_stylebox_override("panel", _style(Color("161e2b"), color))
	var column := VBoxContainer.new()
	column.add_theme_constant_override("separation", 1)
	card.add_child(column)
	var rarity := Label.new()
	rarity.text = "[%s]" % String(result.get("rarity_name", ""))
	rarity.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	rarity.add_theme_font_size_override("font_size", 10 if single else 7)
	rarity.add_theme_color_override("font_color", color)
	column.add_child(rarity)
	var portrait := PetPortrait.new()
	portrait.custom_minimum_size = Vector2(96, 120) if single else Vector2(48, 46)
	portrait.size_flags_vertical = Control.SIZE_EXPAND_FILL
	portrait.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var pet_id: String = String(result.get("pet_id", ""))
	var data: PetData = PetManager.get_pet_data(pet_id)
	portrait.configure(pet_id, data.color if data != null else color)
	column.add_child(portrait)
	var pet_name := Label.new()
	pet_name.text = String(result.get("name", pet_id))
	pet_name.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	pet_name.add_theme_font_size_override("font_size", 12 if single else 8)
	column.add_child(pet_name)
	var grant := Label.new()
	grant.name = "Grant"
	grant.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	grant.add_theme_font_size_override("font_size", 10 if single else 7)
	if bool(result.get("duplicate", false)):
		grant.text = "중복 → 조각 +%d" % int(result.get("shards", 0))
		grant.add_theme_color_override("font_color", Color("cbb1ff"))
	else:
		grant.text = "신규 펫 획득"
		grant.add_theme_color_override("font_color", Color("70dfab"))
	column.add_child(grant)
	var owned := Label.new()
	owned.text = "보유 조각 %d" % PetManager.shards_for(pet_id)
	owned.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	owned.add_theme_font_size_override("font_size", 8 if single else 6)
	owned.add_theme_color_override("font_color", Color("a9b7c8"))
	column.add_child(owned)
	return card


func _style(background: Color, border: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = background
	style.border_color = border
	style.set_border_width_all(1)
	style.content_margin_left = 10
	style.content_margin_right = 10
	style.content_margin_top = 7
	style.content_margin_bottom = 7
	return style


func _input(event: InputEvent) -> void:
	if not visible or not event is InputEventKey:
		return
	var key := event as InputEventKey
	if key.pressed and not key.echo and key.keycode == KEY_ESCAPE:
		close()
	# Other management hotkeys cannot switch tabs underneath an open receipt.
	if key.keycode != KEY_ENTER and key.keycode != KEY_KP_ENTER and key.keycode != KEY_TAB:
		get_viewport().set_input_as_handled()


func close() -> void:
	visible = false
	dismissed.emit()
