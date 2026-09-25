extends Node

const MainScene = preload("res://main.tscn")

func _ready() -> void:
	var main := MainScene.instantiate() as E0Main
	add_child(main)
	await get_tree().process_frame

	if not _check(main.ranged_enemy.has_multiframe_contract(), "ranged enemy frame contract missing"):
		return

	var deadline := Time.get_ticks_msec() + 20000
	while Time.get_ticks_msec() < deadline and not main.combat_complete and not main.warrior.is_dead():
		await get_tree().physics_frame

	if not _check(not main.warrior.is_dead(), "warrior should survive E0-C02"):
		return
	if not main.combat_complete:
		print("E0_C02 SNAPSHOT warrior_hp=%d melee_hp=%d ranged_hp=%d deaths=%d drops=%d/%d projectiles=%d/%d warrior_state=%s ranged_state=%s warrior_pos=%s ranged_pos=%s" % [
			main.warrior.hp,
			main.melee_enemy.hp,
			main.ranged_enemy.hp,
			main.death_events,
			main.drop_collect_count,
			main.drop_spawn_count,
			main.projectile_hit_count,
			main.projectile_spawn_count,
			main.warrior.state_name(),
			main.ranged_enemy.state_name(),
			str(main.warrior.global_position),
			str(main.ranged_enemy.global_position)
		])
	if not _check(main.combat_complete, "combat and collection should complete"):
		return
	if not _check(main.death_events == 2, "exactly two enemy death events expected"):
		return
	if not _check(main.melee_enemy.death_signal_count == 1 and main.ranged_enemy.death_signal_count == 1, "each enemy must die exactly once"):
		return
	if not _check(main.ranged_enemy.projectiles_requested >= 1, "ranged enemy must request a projectile"):
		return
	if not _check(main.projectile_spawn_count == main.ranged_enemy.projectiles_requested, "every request must create one projectile"):
		return
	if not _check(main.projectile_hit_count >= 1, "at least one projectile must collide with the warrior"):
		return
	if not _check(main.projectile_min_travel_distance > 40.0, "projectile damage must happen after visible travel, not at spawn"):
		return
	if not _check(main.ledger_total_at_first_drop == 0, "enemy death must not directly grant reward"):
		return
	if not _check(main.drop_spawn_count == 2 and main.drop_collect_count == 2, "both deaths must spawn and collect one drop"):
		return
	if not _check(main.reward_ledger.total_collections == 2, "ledger must record exactly two collections"):
		return
	if not _check(main.reward_ledger.gold == 20 and main.reward_ledger.xp == 10, "ledger reward totals must match drop definitions"):
		return
	if not _check(main.warrior.pickup_distance_travelled > 10.0, "warrior must physically move to collect at least one drop"):
		return

	var gold_before := main.reward_ledger.gold
	var xp_before := main.reward_ledger.xp
	var duplicate_accepted := main.reward_ledger.collect(&"drop_melee", 999, 999)
	if not _check(not duplicate_accepted, "duplicate drop id must be rejected"):
		return
	if not _check(main.reward_ledger.gold == gold_before and main.reward_ledger.xp == xp_before, "duplicate collection must not change totals"):
		return

	print("E0_C02_TEST PASSED")
	get_tree().quit(0)

func _check(condition: bool, message: String) -> bool:
	if condition:
		return true
	push_error("E0_C02_TEST FAILED: %s" % message)
	print("E0_C02_TEST FAILED: %s" % message)
	get_tree().quit(1)
	return false
