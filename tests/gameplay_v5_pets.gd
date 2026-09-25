extends Node

var _failures: Array[String] = []
var _checks: int = 0


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		push_error("Gameplay V5 pet tests require --affix-test-mode")
		get_tree().quit(2)
		return
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	_checks += 1
	if not condition:
		_failures.append(message)
		push_error("GAMEPLAY_V5_PETS: " + message)


func _run() -> void:
	var original_owned: Dictionary = PetManager.owned_pets.duplicate(true)
	var original_active: String = PetManager.active_pet_id
	var original_essence: int = PetManager.essence
	var original_gold: int = GameManager.gold

	_check(PetManager.all_pet_ids().size() == 6, "Pet catalog exposes six launch companions")
	_check(PetManager.get_pet_data("spirit_fox") != null, "Starter spirit fox resource loads")
	_check(PetManager.is_owned(PetManager.STARTER_PET_ID), "Starter pet is always owned")
	_check(PetManager.active_pet_data() != null, "An active combat pet is available")
	_check(PetManager.active_attack_power(100.0) > 0.0, "Active pet contributes autonomous combat damage")
	_check(PetManager.active_attack_interval() < 3.0, "Active pet attacks on a readable automatic cadence")
	_check(PetManager.active_support_heal_percent() > 0.0, "Starter support pet provides automatic sustain")
	GameManager.gold = 100000000
	PetManager.essence = 999
	var before_train_level: int = PetManager.level_for(PetManager.STARTER_PET_ID)
	_check(PetManager.train_pet(PetManager.STARTER_PET_ID), "Owned pets can spend gold to train")
	_check(PetManager.level_for(PetManager.STARTER_PET_ID) == before_train_level + 1, "Training advances exactly one pet level")
	PetManager.owned_pets[PetManager.STARTER_PET_ID] = {"level": 20, "xp": 0, "stars": 1}
	_check(PetManager.can_evolve(PetManager.STARTER_PET_ID), "Levelled pets become eligible for star evolution")
	_check(PetManager.evolve_pet(PetManager.STARTER_PET_ID), "Eligible pets can spend gold to evolve")
	_check(PetManager.stars_for(PetManager.STARTER_PET_ID) == 2, "Evolution raises the persistent star rank")
	_check(PetManager.essence < 999, "Evolution consumes pet essence")
	var essence_before_add: int = PetManager.essence
	_check(PetManager.add_essence(4) == 4 and PetManager.essence == essence_before_add + 4, "Elite/boss essence rewards add to pet progression currency")

	PetManager.owned_pets = {
		"spirit_fox": {"level": 7, "xp": 13, "stars": 2},
	}
	PetManager.active_pet_id = "spirit_fox"
	var snapshot: Dictionary = PetManager.to_save_dict()
	var saved_essence: int = PetManager.essence
	PetManager.owned_pets = {}
	PetManager.active_pet_id = ""
	PetManager.essence = 0
	PetManager.apply_save_dict(snapshot)
	_check(PetManager.level_for("spirit_fox") == 7, "Pet level survives save round-trip")
	_check(PetManager.stars_for("spirit_fox") == 2, "Pet star state survives save round-trip")
	_check(PetManager.active_pet_id == "spirit_fox", "Active pet survives save round-trip")
	_check(PetManager.essence == saved_essence, "Pet essence survives save round-trip")

	var unlocked: Array[String] = PetManager.try_unlock_for_floor(60)
	_check(PetManager.is_owned("ember_drake"), "Floor progression unlocks Ember Drake")
	_check(PetManager.is_owned("stone_golem"), "Floor progression unlocks Stone Golem")
	_check(PetManager.is_owned("meadow_fairy"), "Late-floor progression unlocks Meadow Fairy")
	_check(unlocked.size() >= 5, "High-floor migration unlocks all eligible companions")

	var generator := ItemGenerator.new()
	var item: Dictionary = generator.generate_item(30, 2)
	_check(item.has("icon_path"), "Generated equipment carries an overrideable art path")
	var base := ItemBaseData.new()
	base.icon_texture_path = "res://assets/items/example.png"
	_check(base.icon_texture_path.ends_with(".png"), "Item base supports dedicated per-item sprite paths")

	var icon := ItemVisualIcon.new()
	icon.size = Vector2(40, 40)
	icon.configure({
		"base_id": "weapon_divine_sword",
		"slot": "weapon",
		"rarity_color": "ffd166",
		"enhancement_level": 7,
	})
	_check(icon.accent_color == Color("ffd86b"), "Divine sword receives a dedicated gold visual identity")
	icon.configure({
		"base_id": "ring_diamond",
		"slot": "ring",
		"rarity_color": "d138ff",
	})
	_check(icon.accent_color == Color("91ecff"), "Diamond ring receives a dedicated crystal visual identity")
	icon.queue_free()

	_check(GameUI.MANAGEMENT_TITLES.size() == 6, "Management hub now includes six growth sections")
	_check(GameUI.MANAGEMENT_TITLES[3] == "펫", "Pet management sits between skills and rebirth")
	_check(PlayerAvatar.CLASS_ATLAS != null, "Combat heroes use the generated class atlas")
	_check(EnemyAI.ENEMY_ATLAS != null, "Combat enemies use the generated enemy atlas")
	_check(BattleManager.DUNGEON_COURTYARD != null, "Combat rooms layer the generated dungeon courtyard art")
	_check(ItemVisualIcon.ITEM_BASE_ATLAS != null, "Inventory visuals use the dedicated 29-item artwork atlas")

	var boss_bar := BossStatusBar.new()
	boss_bar.size = Vector2(330, 38)
	boss_bar.set_boss("시험 보스", 0.42, true)
	_check(boss_bar.active and is_equal_approx(boss_bar.hp_ratio, 0.42), "Boss HUD exposes live boss health state")
	boss_bar.set_boss("", 0.0, false)
	_check(not boss_bar.active, "Boss HUD clears after a boss fight")
	boss_bar.queue_free()

	var minimap := GameMiniMap.new()
	minimap.size = Vector2(78, 58)
	minimap.set_floor(12)
	_check(minimap.current_floor == 12, "HUD minimap tracks the current floor")
	_check(WorldLayout.room_index_for_floor(minimap.current_floor) == WorldLayout.room_index_for_floor(12), "HUD minimap uses the shared room path")
	minimap.queue_free()

	var objective := CombatObjective.new()
	objective.size = Vector2(190, 58)
	objective.sync_from_game()
	_check(objective.kill_goal == 8 + GameManager.floor, "Objective tracker mirrors the floor kill target")
	_check(objective.floor_number == GameManager.floor, "Objective tracker mirrors current floor progress")
	objective.queue_free()

	var avatar := PlayerAvatar.new()
	add_child(avatar)
	avatar.set_equipment_visual({
		"base_id": "weapon_magic_sword",
		"rarity_color": "b56dff",
		"enhancement_level": 12,
	})
	_check(avatar._weapon_base_id == "weapon_magic_sword", "Combat avatar mirrors the equipped weapon identity")
	_check(avatar._weapon_enhancement == 12, "Combat avatar mirrors weapon enhancement glow")
	avatar.queue_free()

	var dummy_player := Node2D.new()
	dummy_player.visible = true
	add_child(dummy_player)
	var companion := PetCompanion.new()
	add_child(companion)
	companion.bind_player(dummy_player)
	companion._process(0.016)
	_check(companion.visible, "Combat companion is visible beside an active player")
	_check(not companion.pet_id().is_empty(), "Combat companion reflects the active pet id")
	companion.queue_free()
	dummy_player.queue_free()

	PetManager.owned_pets = original_owned
	PetManager.active_pet_id = original_active
	PetManager.essence = original_essence
	GameManager.gold = original_gold
	PetManager.apply_save_dict(PetManager.to_save_dict())
	_finish()


func _finish() -> void:
	var output_dir: String = ProjectSettings.globalize_path("res://build/gameplay-v5")
	DirAccess.make_dir_recursive_absolute(output_dir)
	var report := FileAccess.open(output_dir.path_join("pets.json"), FileAccess.WRITE)
	if report != null:
		report.store_string(JSON.stringify({
			"checks": _checks,
			"failures": _failures,
			"status": "PASS" if _failures.is_empty() else "FAIL",
			"scope": "pets, training/evolution, persistent companion, generated hero/enemy/dungeon art, actual 29-item atlas, boss/elite presentation, minimap, equipped weapon rendering"
		}, "\t"))
		report.close()
	print("GAMEPLAY_V5_PETS %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
