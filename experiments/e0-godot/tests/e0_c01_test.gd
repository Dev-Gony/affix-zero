extends Node

const MainScene = preload("res://main.tscn")

func _ready() -> void:
	var main := MainScene.instantiate() as E0Main
	add_child(main)
	await get_tree().process_frame

	var warrior := main.get_node("Warrior") as E0Warrior
	var enemy := main.get_node("MeleeEnemy") as E0MeleeEnemy
	assert(warrior != null)
	assert(enemy != null)
	assert(warrior.has_multiframe_contract())
	assert(enemy.has_multiframe_contract())

	var deadline := Time.get_ticks_msec() + 9000
	while Time.get_ticks_msec() < deadline and not enemy.is_dead() and not warrior.is_dead():
		await get_tree().physics_frame

	assert(not warrior.is_dead(), "warrior should survive E0-C01")
	assert(enemy.is_dead(), "melee enemy should die within the contract window")
	assert(warrior.distance_travelled > 20.0, "warrior must actually approach")
	assert(enemy.distance_travelled > 20.0, "enemy must actually approach")
	assert(warrior.total_attacks_resolved >= 3, "warrior must resolve real attack events")
	assert(enemy.damage_events_received == warrior.total_attacks_resolved, "one attack instance must map to one damage event")
	assert(main.player_damage_events == warrior.total_attacks_resolved, "main combat receipt must match warrior resolver")
	print("E0_C01_TEST PASSED")
	get_tree().quit()
