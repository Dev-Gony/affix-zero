extends Node

const WarriorScene = preload("res://scenes/warrior.tscn")
const MeleeScene = preload("res://scenes/melee_enemy.tscn")

func _ready() -> void:
	var warrior := WarriorScene.instantiate() as E0Warrior
	var enemy := MeleeScene.instantiate() as E0MeleeEnemy
	add_child(warrior)
	add_child(enemy)
	warrior.position = Vector2(120, 180)
	enemy.position = Vector2(430, 180)
	warrior.set_target(enemy)
	enemy.set_target(warrior)

	await get_tree().process_frame
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
	if not _check(enemy.death_signal_count == 1, "enemy death signal must be unique"):
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
