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

	_check(PetManager.all_pet_ids().size() == 6, "Pet catalog exposes six launch companions")
	_check(PetManager.get_pet_data("spirit_fox") != null, "Starter spirit fox resource loads")
	_check(PetManager.is_owned(PetManager.STARTER_PET_ID), "Starter pet is always owned")
	_check(PetManager.active_pet_data() != null, "An active combat pet is available")
	_check(PetManager.active_attack_power(100.0) > 0.0, "Active pet contributes autonomous combat damage")
	_check(PetManager.active_attack_interval() < 3.0, "Active pet attacks on a readable automatic cadence")
	_check(PetManager.active_support_heal_percent() > 0.0, "Starter support pet provides automatic sustain")

	PetManager.owned_pets = {
		"spirit_fox": {"level": 7, "xp": 13, "stars": 2},
	}
	PetManager.active_pet_id = "spirit_fox"
	var snapshot: Dictionary = PetManager.to_save_dict()
	PetManager.owned_pets = {}
	PetManager.active_pet_id = ""
	PetManager.apply_save_dict(snapshot)
	_check(PetManager.level_for("spirit_fox") == 7, "Pet level survives save round-trip")
	_check(PetManager.stars_for("spirit_fox") == 2, "Pet star state survives save round-trip")
	_check(PetManager.active_pet_id == "spirit_fox", "Active pet survives save round-trip")

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
			"scope": "pet catalog, active companion, progression unlock, persistence, combat contribution, item-art override contract"
		}, "\t"))
		report.close()
	print("GAMEPLAY_V5_PETS %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
