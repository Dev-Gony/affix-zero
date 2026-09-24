extends Node

# Manual visual-QA harness. Run with Movie Maker and an isolated APPDATA path.
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
	for index: int in 12:
		GameManager.add_inventory_item(LootManager._generator.generate_item(8 + index, index / 4))
	var game_ui: GameUI = main.get_node("UILayer/GameUI")
	game_ui._main_tabs.current_tab = 1

