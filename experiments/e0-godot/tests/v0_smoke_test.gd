extends Node

const V0Scene = preload("res://vslice/v0_main.tscn")

func _ready() -> void:
	var scene := V0Scene.instantiate() as V0Main
	add_child(scene)
	await get_tree().process_frame
	if scene.background == null or scene.class_atlas == null or scene.enemy_atlas == null:
		push_error("V0_SMOKE_TEST FAILED: production assets not loaded")
		get_tree().quit(1)
		return
	if scene.player == null:
		push_error("V0_SMOKE_TEST FAILED: player missing")
		get_tree().quit(1)
		return
	var deadline := Time.get_ticks_msec()+2500
	while Time.get_ticks_msec() < deadline and scene.enemies.is_empty():
		await get_tree().physics_frame
	if scene.enemies.is_empty():
		push_error("V0_SMOKE_TEST FAILED: enemies did not spawn")
		get_tree().quit(1)
		return
	print("V0_SMOKE_TEST PASSED")
	get_tree().quit(0)
