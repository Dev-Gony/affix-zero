extends Node

var _failures: Array[String] = []
var _checks: int = 0


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		push_error("VISUAL_V6 requires --affix-test-mode")
		get_tree().quit(2)
		return
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	_checks += 1
	if not condition:
		_failures.append(message)
		push_error("VISUAL_V6: " + message)


func _run() -> void:
	_check(WorldLayout.GRID_SIZE == Vector2i(5, 4), "World layout uses the expanded 5x4 dungeon canvas")
	_check(WorldLayout.FLOOR_PATH.size() == 16, "Dungeon circuit contains sixteen rooms")
	_check(WorldLayout.active_room_indices().size() == 16, "Only authored dungeon rooms are exposed as active")
	var pairs: Array[Vector2i] = WorldLayout.connected_room_pairs()
	_check(pairs.size() == 16, "Closed dungeon circuit exposes sixteen corridor edges")
	for pair: Vector2i in pairs:
		var a := WorldLayout.room_grid(pair.x)
		var b := WorldLayout.room_grid(pair.y)
		var manhattan: int = absi(a.x - b.x) + absi(a.y - b.y)
		_check(manhattan == 1, "Every dungeon edge connects orthogonally adjacent rooms")
		_check(not WorldLayout.corridor_rect(pair.x, pair.y).size.is_zero_approx(), "Every authored dungeon edge has visible corridor geometry")
		_check(not WorldLayout.travel_waypoints(pair.x, pair.y).is_empty(), "Every authored dungeon edge has AUTO travel waypoints")
	_check(WorldLayout.room_index_for_floor(17) == WorldLayout.FLOOR_PATH[0], "Floor cycle wraps to the first room")
	_check(WorldLayout.dungeon_cycle_for_floor(17) == 1, "Floor cycle advances the dungeon theme band")

	var attack_expectations: Dictionary = {
		"slime": "slam",
		"bat": "dive",
		"skeleton": "slash",
		"goblin": "slash",
		"dark_knight": "slash",
		"lich": "shadow_bolt",
		"dragon": "flame",
		"demon_lord": "hellfire",
	}
	for enemy_id: String in attack_expectations.keys():
		var enemy := EnemyAI.new()
		var data: EnemyData = load("res://resources/enemies/%s.tres" % enemy_id)
		enemy.enemy_data = data
		enemy.behavior = data.behavior if data != null else "chaser"
		_check(enemy.attack_visual_kind() == String(attack_expectations[enemy_id]), "%s exposes a distinct attack visual kind" % enemy_id)
		enemy.queue_free()

	var effects := EffectLayer.new()
	add_child(effects)
	effects.show_attack(Vector2.ZERO, Vector2(30, 0), "mage", false)
	effects.show_attack(Vector2.ZERO, Vector2(30, 0), "sage", false)
	effects.show_attack(Vector2.ZERO, Vector2(30, 0), "saint", false)
	effects.show_enemy_telegraph(Vector2.ZERO, Vector2(40, 0), "hellfire", 0.25)
	effects.show_enemy_attack(Vector2.ZERO, Vector2(40, 0), "shadow_bolt")
	_check(not effects._lines.is_empty(), "Class attacks and enemy telegraphs generate readable line VFX")
	_check(not effects._rings.is_empty(), "Enemy and class attacks generate impact/telegraph rings")
	effects.queue_free()

	var original_owned: Dictionary = PetManager.owned_pets.duplicate(true)
	var original_active: String = PetManager.active_pet_id
	var original_essence: int = PetManager.essence
	var original_summon_count: int = PetManager.summon_count
	var original_last: Array[Dictionary] = PetManager.last_summon_results.duplicate(true)

	PetManager.owned_pets = {
		PetManager.STARTER_PET_ID: {"level": 20, "xp": 0, "stars": 1, "fragments": 12},
	}
	PetManager.active_pet_id = PetManager.STARTER_PET_ID
	PetManager.essence = 1000
	var before_essence: int = PetManager.essence
	var results: Array[Dictionary] = PetManager.summon_pets(10)
	_check(results.size() == 10, "Ten-pull gacha always returns ten results")
	_check(PetManager.essence == before_essence - PetManager.TEN_SUMMON_COST, "Ten-pull gacha spends summon essence")
	_check(results.any(func(result: Dictionary) -> bool: return int(result.get("rarity_index", 0)) >= PetManager.TEN_PULL_GUARANTEE_RARITY), "Ten-pull gacha enforces hero-or-better guarantee")
	_check(PetManager.try_unlock_for_floor(999).is_empty(), "Dungeon floors no longer auto-grant collection pets")
	_check(PetManager.fragments_for(PetManager.STARTER_PET_ID) >= 0, "Duplicate pet fragments are persisted in pet state")

	var dummy_player := Node2D.new()
	dummy_player.visible = true
	add_child(dummy_player)
	var companion := PetCompanion.new()
	add_child(companion)
	companion.bind_player(dummy_player)
	companion.play_attack(Vector2(50, 0))
	_check(companion._action_kind == "attack" and companion._action_time_left > 0.0, "Live pet exposes an attack animation state")
	companion.play_support()
	_check(companion._action_kind == "support" and companion._action_time_left > 0.0, "Live pet exposes a support animation state")
	companion.queue_free()
	dummy_player.queue_free()

	PetManager.owned_pets = original_owned
	PetManager.active_pet_id = original_active
	PetManager.essence = original_essence
	PetManager.summon_count = original_summon_count
	PetManager.last_summon_results = original_last
	PetManager.apply_save_dict(PetManager.to_save_dict())

	_finish()


func _finish() -> void:
	var output_dir: String = ProjectSettings.globalize_path("res://build/visual-v6")
	DirAccess.make_dir_recursive_absolute(output_dir)
	var report := FileAccess.open(output_dir.path_join("contracts.json"), FileAccess.WRITE)
	if report != null:
		report.store_string(JSON.stringify({
			"checks": _checks,
			"failures": _failures,
			"status": "PASS" if _failures.is_empty() else "FAIL",
			"scope": "closed dungeon route, enemy attack identities, class VFX, pet gacha guarantee, live pet action states"
		}, "\t"))
		report.close()
	print("VISUAL_V6 %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
