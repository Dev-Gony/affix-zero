extends Node

var _failures: Array[String] = []
var _checks: int = 0

func _ready() -> void:
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		get_tree().quit(2)
		return
	call_deferred("_run")

func _check(ok: bool, message: String) -> void:
	_checks += 1
	if not ok:
		_failures.append(message)
		push_error("PLAYTEST_RECOVERY: " + message)

func _run() -> void:
	_check(not SaveManager.persistence_enabled, "Isolated test mode protects the real player save")
	seed(62025)
	PetManager.summon_crystals = 6000
	PetManager.summon_pity = 0
	PetManager.owned_pets = {"spirit_fox": {"level": 37, "stars": 2, "xp": 18, "shards": 0}}
	var first: Dictionary = PetManager.summon_once(false, 5)
	_check(first.get("pet_id", "") == "ember_drake" and not first.get("duplicate", true), "Forced eligible new summon grants a new pet")
	_check(PetManager.summon_crystals == 5700, "One pull charges exactly 300 crystals")
	PetManager.owned_pets["ember_drake"]["level"] = 18
	var again: Dictionary = PetManager.summon_once(false, 5)
	_check(again.get("duplicate", false) and int(again.get("shards", 0)) == 60, "Duplicate grant explicitly describes sixty drake shards")
	_check(PetManager.shards_for("ember_drake") == 60 and PetManager.level_for("ember_drake") == 18, "Duplicate does not reset trained pet progression")
	var previous: Dictionary = PetManager.to_save_dict()
	var saved_catalog: Dictionary = PetManager._catalog.duplicate()
	PetManager._catalog.erase("night_bat")
	_check(PetManager.summon_ten().is_empty(), "Missing catalog prevents the whole batch")
	_check(PetManager.to_save_dict() == previous, "Catalog failure preserves crystals, pity, pets and prior receipt")
	PetManager._catalog = saved_catalog
	var saved_rarities: Dictionary = {}
	for id: String in PetManager._catalog:
		saved_rarities[id] = PetManager.get_pet_data(id).rarity_index
		PetManager.get_pet_data(id).rarity_index = 2
	_check(PetManager.summon_ten().is_empty(), "Unsatisfiable final guaranteed draw rejects the whole ten-pull")
	_check(PetManager.to_save_dict() == previous, "Late planning failure does not partially grant nine pets or debit funds")
	for id: String in saved_rarities:
		PetManager.get_pet_data(id).rarity_index = saved_rarities[id]
	PetManager.summon_pity = 29
	var pity_result: Dictionary = PetManager.summon_once()
	_check(int(pity_result.get("rarity_index", 0)) >= 4 and PetManager.summon_pity == 0, "Thirtieth pull guarantees legendary or better and resets pity")
	PetManager.summon_crystals = 2700
	var observed: Array[int] = [0]
	var listener := func(_result: Dictionary) -> void:
		_check(PetManager.summon_crystals == 0 and Array(PetManager.last_summon_receipt.get("results", [])).size() == 10, "Summon observer sees a fully committed ten-pull")
		observed[0] += 1
	PetManager.pet_summoned.connect(listener)
	var ten: Array[Dictionary] = PetManager.summon_ten()
	PetManager.pet_summoned.disconnect(listener)
	_check(ten.size() == 10 and observed[0] == 10, "Ten-pull grants exactly ten results and notifications")
	_check(int(ten.back().get("rarity_index", 0)) >= 3, "Tenth result satisfies hero guarantee")
	var ten_save: Dictionary = PetManager.to_save_dict()
	_check(PetManager.summon_once().is_empty() and PetManager.to_save_dict() == ten_save, "Insufficient funds do not change the last receipt or state")
	PetManager.last_summon_receipt = {}
	PetManager.apply_save_dict(ten_save)
	_check(PetManager.last_summon_receipt == ten_save["last_summon_receipt"], "Receipt survives save serialization")

	var main: Node = load("res://scenes/main.tscn").instantiate()
	get_tree().root.add_child(main)
	GameManager.select_class(load("res://resources/classes/warrior.tres"))
	await get_tree().process_frame
	await get_tree().process_frame
	var battle: BattleManager = main.get_node("BattleArea")
	var ui: GameUI = main.get_node("UILayer/GameUI")
	battle.set_process(false)
	battle.player.set_process(false)
	battle.pet.set_process(false)
	battle._clear_enemies()
	battle._traveling = false
	battle._respawning = false
	battle._pending_target = null
	battle.player._motion_kind = "idle"
	var enemy := EnemyAI.new()
	battle.enemies_root.add_child(enemy)
	enemy.setup(load("res://resources/enemies/slime.tres"), 1, battle.player)
	enemy.set_process(false)
	enemy.global_position = battle.player.global_position + Vector2(30, 0)
	enemy.hp = 1000000
	enemy.max_hp = enemy.hp
	enemy.defense = 0
	battle._enemies = [enemy]
	var hp_before: float = enemy.hp
	battle._perform_auto_attack()
	_check(enemy.hp == hp_before, "Melee anticipation deals no early damage")
	battle._tick_attack_release(0.06)
	_check(enemy.hp == hp_before, "Melee damage still waits halfway through windup")
	battle._tick_attack_release(0.061)
	_check(enemy.hp < hp_before and enemy._hit_flash_left > 0.0, "Melee release applies one hit and feedback")
	var after_hit: float = enemy.hp
	battle._tick_attack_release(0.20)
	_check(enemy.hp == after_hit, "A single scheduled attack never applies twice")
	battle.player._motion_kind = "idle"
	battle._perform_auto_attack()
	battle._traveling = true
	battle._tick_attack_release(1.0)
	_check(enemy.hp == after_hit, "Room transition cancels pending melee hit")
	battle._traveling = false
	GameManager.selected_class = "sage"
	battle.player.configure("sage", Color.WHITE)
	battle.player._motion_kind = "idle"
	enemy.global_position = battle.player.global_position + Vector2(100, 0)
	var children_before: int = battle.projectiles_root.get_child_count()
	battle._perform_auto_attack()
	battle._tick_attack_release(0.13)
	_check(battle.projectiles_root.get_child_count() == children_before + 1, "Caster release creates a real projectile")
	_check(enemy.hp == after_hit, "Projectile launch is not an immediate enemy hit")
	var shot := battle.projectiles_root.get_child(battle.projectiles_root.get_child_count() - 1) as PlayerBasicProjectile
	shot.set_process(false)
	for i: int in 100:
		if shot.is_queued_for_deletion():
			break
		shot._process(0.01)
	_check(enemy.hp < after_hit and shot.is_queued_for_deletion(), "Only projectile impact applies caster damage")
	_check(battle.player.is_ranged(), "Caster presentation does not use a melee weapon swing")

	battle.pet.bind_player(battle.player)
	for i: int in 120:
		battle.pet._process(0.05)
		_check(battle.pet.global_position.distance_to(battle.player.global_position) >= 24.99, "Pet keeps player clearance across old three-second side-switch boundaries")
	battle.pet.global_position = battle.player.global_position
	battle.pet._process(0.01)
	_check(battle.pet.global_position.distance_to(battle.player.global_position) >= 24.99, "Overlapped pet is separated even from zero displacement")
	var pet_hp_before: float = enemy.hp
	battle._pet_attack_time_left = 0.0
	battle._pet_support_time_left = 999.0
	battle._update_pet_combat(0.01)
	_check(enemy.hp == pet_hp_before, "Pet windup does not hit early")
	battle._update_pet_combat(0.13)
	_check(enemy.hp == pet_hp_before, "Pet launch waits for projectile impact")
	var pet_shot := battle.projectiles_root.get_child(battle.projectiles_root.get_child_count() - 1) as PlayerBasicProjectile
	pet_shot.set_process(false)
	for i: int in 150:
		if pet_shot.is_queued_for_deletion():
			break
		pet_shot._process(0.01)
	_check(enemy.hp < pet_hp_before, "Pet projectile impact applies damage")
	enemy._spawn_reveal_left = 0
	enemy._attack_windup_left = 0.01
	enemy._process(0.02)
	_check(enemy._attack_recovery_left > 0.0, "Enemy strike starts visible recovery instead of skipping it")
	enemy._process(0.19)
	_check(enemy._attack_recovery_left == 0.0, "Enemy recovery actually completes")

	GameManager.floor = 85
	GameManager.kills_on_floor = 0
	ui._refresh_hud()
	ui._refresh_objective()
	_check(ui._hud_info.text.contains("0/13") and ui._objective.kill_goal == 13, "Both floor-85 HUDs use the actual thirteen-kill goal")
	GameManager.inventory.clear()
	GameManager.set_loot_min_rarity(0)
	var item: Dictionary = LootManager._generator.generate_item(1, 0)
	_check(battle._collect_world_loot(item, battle.player.global_position), "Accepted drop commits immediately")
	battle.effects.clear_effects()
	_check(GameManager._find_inventory_index(String(item["id"])) >= 0, "Clearing loot VFX cannot delete owned reward")
	ui._toggle_management(3)
	PetManager.summon_crystals = 3000
	ui._refresh_pets()
	var button: Button = _find_button(ui._pet_content, "10")
	_check(button != null and not button.disabled, "Live pet tab has a usable ten-pull button")
	if button != null:
		button.pressed.emit()
	await get_tree().process_frame
	_check(ui.summon_results.visible and ui.summon_results.result_cards.size() == 10, "Real button handler opens ten persistent result cards above the management window")
	var displayed: Dictionary = ui.summon_results.receipt.duplicate(true)
	PetManager.add_essence(1)
	_check(ui.summon_results.visible and ui.summon_results.receipt == displayed, "Combat currency refresh cannot erase the result screen")
	var crystals_after: int = PetManager.summon_crystals
	ui._summon_pet_once()
	_check(PetManager.summon_crystals == crystals_after, "Open receipt blocks duplicate summon commands")
	ui.summon_results.close()
	ui._show_last_summon()
	_check(PetManager.summon_crystals == crystals_after and ui.summon_results.receipt == displayed, "Reopening receipt never charges or rerolls")
	ui.summon_results.close()
	_check(ui.find_child("Shards_spirit_fox", true, false) != null, "Roster exposes duplicate shard balance")
	print("PLAYTEST_RECOVERY %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	main.queue_free()
	await get_tree().process_frame
	get_tree().quit(0 if _failures.is_empty() else 1)

func _find_button(node: Node, prefix: String) -> Button:
	if node is Button and (node as Button).text.begins_with(prefix):
		return node as Button
	for child: Node in node.get_children():
		var found: Button = _find_button(child, prefix)
		if found != null:
			return found
	return null
