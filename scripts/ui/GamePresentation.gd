extends GameUI

# Runtime presentation extension: persistent reward receipts live outside the
# management tab subtree, which is rebuilt when combat updates pet currency.
var summon_results: SummonResultPanel
var _summon_busy: bool = false


func _ready() -> void:
	super._ready()
	summon_results = SummonResultPanel.new()
	add_child(summon_results)
	summon_results.dismissed.connect(func() -> void: _refresh_pets())
	_notification_label.z_index = 110


func _refresh_hud() -> void:
	super._refresh_hud()
	_hud_info.text = "◆ %d층  ·  Lv.%d  ·  %dG\nAUTO  처치 %d/%d  ·  %s" % [
		GameManager.floor, GameManager.level, GameManager.gold, GameManager.kills_on_floor,
		WorldLayout.encounter_kill_goal(GameManager.floor),
		GameManager.selected_class_name if not GameManager.selected_class_name.is_empty() else "직업 선택",
	]


func _refresh_objective() -> void:
	super._refresh_objective()
	if _hud_info != null:
		_refresh_hud()


func _refresh_pets() -> void:
	super._refresh_pets()
	if _pet_content == null:
		return
	var history := Button.new()
	history.name = "LastSummonReceipt"
	history.text = "최근 소환 결과 다시 보기"
	history.disabled = PetManager.last_summon_receipt.is_empty()
	history.custom_minimum_size.y = 25
	history.pressed.connect(_show_last_summon)
	_pet_content.add_child(history)
	_pet_content.move_child(history, 2)


func _add_pet_card(parent: GridContainer, pet_id: String) -> void:
	super._add_pet_card(parent, pet_id)
	if PetManager.get_pet_data(pet_id) == null or parent.get_child_count() == 0:
		return
	var card := parent.get_child(parent.get_child_count() - 1) as Button
	var column: Node = card.get_child(0).get_child(1)
	var shards := Label.new()
	shards.name = "Shards_" + pet_id
	shards.text = "보유 조각 %d" % PetManager.shards_for(pet_id)
	shards.add_theme_font_size_override("font_size", 6)
	shards.add_theme_color_override("font_color", Color("cbb1ff"))
	shards.mouse_filter = Control.MOUSE_FILTER_IGNORE
	column.add_child(shards)


func _summon_pet_once() -> void:
	_run_summon(1)


func _summon_pet_ten() -> void:
	_run_summon(10)


func _run_summon(count: int) -> void:
	if _summon_busy or summon_results == null or summon_results.visible:
		return
	_summon_busy = true
	AudioManager.play_sfx("ui_click")
	var results: Array[Dictionary] = []
	if count == 10:
		results = PetManager.summon_ten()
	else:
		var result: Dictionary = PetManager.summon_once()
		if not result.is_empty():
			results.append(result)
	if results.is_empty():
		var message: String = "소환석이 부족합니다."
		if PetManager.last_summon_error != "insufficient_crystals":
			message = "소환 데이터를 불러오지 못했습니다. 소환석은 사용되지 않았습니다."
		_show_notification(message, Color("ff8b82"))
		_summon_busy = false
		return
	var save_status: String = "test" if not SaveManager.persistence_enabled else ("saved" if SaveManager.save_game() else "failed")
	summon_results.present(PetManager.last_summon_receipt, save_status)
	_summon_busy = false


func _show_last_summon() -> void:
	if summon_results != null and not PetManager.last_summon_receipt.is_empty():
		summon_results.present(PetManager.last_summon_receipt, "history")


func _show_notification(message: String, color: Color) -> void:
	super._show_notification(message, color)
	_notification_label.z_index = 110
	_notification_label.position = Vector2(140, 338)
