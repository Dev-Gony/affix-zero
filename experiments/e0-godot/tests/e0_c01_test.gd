extends Node

const MainScene = preload("res://main.tscn")

func _ready() -> void:
	var main := MainScene.instantiate() as E0Main
	add_child(main)
	await get_tree().process_frame

	var warrior := main.get_node("Warrior") as E0Warrior
	var enemy := main.get_node("MeleeEnemy") as E0MeleeEnemy
	if not _check(warrior != null, "warrior scene missing"):
		return
	if not _check(enemy != null, "enemy scene missing"):
		return
	if not _check(warrior.has_multiframe_contract(), "warrior frame contract missing"):
		return
	if not _check(enemy.has_multiframe_contract(), "enemy frame contract missing"):
		return

	var deadline := Time.get_ticks_msec() + 9000
	while Time.get_ticks_msec() < deadline and not enemy.is_dead() and not warrior.is_dead():
		await get_tree().physics_frame

	if not _check(not warrior.is_dead(), "warrior should survive E0-C01"):
		return
	if not _check(enemy.is_dead(), "melee enemy should die within the contract window"):
		return
	if not _check(warrior.distance_travelled > 20.0, "warrior must actually approach"):
		return
	if not _check(enemy.distance_travelled > 20.0, "enemy must actually approach"):
		return
	if not _check(warrior.total_attacks_resolved >= 3, "warrior must resolve real attack events"):
		return
	if not _check(enemy.damage_events_received == warrior.total_attacks_resolved, "one attack instance must map to one damage event"):
		return
	if not _check(main.player_damage_events == warrior.total_attacks_resolved, "main combat receipt must match warrior resolver"):
		return
	print("E0_C01_TEST PASSED")
	get_tree().quit(0)

func _check(condition: bool, message: String) -> bool:
	if condition:
		return true
	push_error("E0_C01_TEST FAILED: %s" % message)
	print("E0_C01_TEST FAILED: %s" % message)
	get_tree().quit(1)
	return false
