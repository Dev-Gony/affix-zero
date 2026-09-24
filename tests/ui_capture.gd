extends Node

# Manual visual-QA harness. Run with Movie Maker and an isolated APPDATA path.
var _game_ui: GameUI
var _elapsed: float = 0.0
var _capture_stage: int = 0


func _ready() -> void:
	SaveManager.set_persistence_enabled(false)
	call_deferred("_prepare_inventory")


func _prepare_inventory() -> void:
	var main_scene: PackedScene = load("res://scenes/main.tscn")
	var main: Node = main_scene.instantiate()
	add_child(main)
	await get_tree().process_frame
	GameManager.reset_run_progress()
	var warrior: ClassData = load("res://resources/classes/warrior.tres")
	GameManager.select_class(warrior)
	var equipped_slots: Dictionary = {}
	for attempt: int in 160:
		var candidate: Dictionary = LootManager._generator.generate_item(18 + attempt, 4)
		var candidate_slot: String = String(candidate.get("slot", ""))
		if not equipped_slots.has(candidate_slot):
			GameManager.add_inventory_item(candidate)
			GameManager.equip_item(String(candidate.get("id", "")))
			equipped_slots[candidate_slot] = true
		if equipped_slots.size() == 7:
			break
	for index: int in 12:
		GameManager.add_inventory_item(LootManager._generator.generate_item(8 + index, index / 4))
	_game_ui = main.get_node("UILayer/GameUI")
	_game_ui._toggle_management(0)


func _process(delta: float) -> void:
	if _game_ui == null:
		return
	_elapsed += delta
	if _capture_stage == 0 and _elapsed >= 1.25:
		_capture_stage = 1
		_game_ui._toggle_management(1)
	elif _capture_stage == 1 and _elapsed >= 2.5:
		_capture_stage = 2
		_game_ui._toggle_management(2)
