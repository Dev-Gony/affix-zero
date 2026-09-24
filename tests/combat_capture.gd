extends Node

# Manual visual-QA harness. Run with Movie Maker and an isolated APPDATA path.
func _ready() -> void:
	SaveManager.set_persistence_enabled(false)
	call_deferred("_prepare_combat")


func _prepare_combat() -> void:
	var main_scene: PackedScene = load("res://scenes/main.tscn")
	var main: Node = main_scene.instantiate()
	add_child(main)
	await get_tree().process_frame
	GameManager.reset_run_progress()
	var warrior: ClassData = load("res://resources/classes/warrior.tres")
	GameManager.select_class(warrior)
	GameManager.set_speed_multiplier(1.0)
	await get_tree().process_frame
	var battle: BattleManager = main.get_node("BattleArea")
	for index: int in battle._enemies.size():
		var angle: float = TAU * float(index) / float(maxi(1, battle._enemies.size()))
		battle._enemies[index].global_position = battle.player.global_position + Vector2(75.0, 36.0).rotated(angle)
		battle._enemies[index]._spawn_reveal_left = 0.0
