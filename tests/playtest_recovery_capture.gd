extends Node

const OUTPUT: String = "res://build/recovery/render"
var _failed: bool = false
var _enemy_draws: int = 0

func _ready() -> void:
	if not OS.get_cmdline_user_args().has("--affix-test-mode") or DisplayServer.get_name() == "headless":
		get_tree().quit(2)
		return
	call_deferred("_run")

func _run() -> void:
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(OUTPUT))
	var main: Node = load("res://scenes/main.tscn").instantiate()
	get_tree().root.add_child(main)
	GameManager.unlocked_classes = ["warrior", "mage", "sage"]
	GameManager.floor = 85
	GameManager.level = 81
	GameManager.rebirth_count = 12
	GameManager.gold = 2605232
	GameManager.class_base_stats["hp"] = 100000
	GameManager.select_class(load("res://resources/classes/sage.tres"))
	GameManager.hp = GameManager.max_hp
	await get_tree().process_frame
	await get_tree().process_frame
	var battle: BattleManager = main.get_node("BattleArea")
	var ui: GameUI = main.get_node("UILayer/GameUI")
	battle.set_process(false)
	battle._clear_enemies()
	battle._traveling = false
	battle._respawning = false
	battle.player.global_position = WorldLayout.room_center(battle._current_room)
	battle.pet.bind_player(battle.player)
	battle.camera.position_smoothing_enabled = false
	battle.camera.reset_smoothing()
	# Synthetic level-85 scene. No user save is read or modified.
	for i: int in 3:
		var ids: Array[String] = ["skeleton", "bat", "slime"]
		var enemy := EnemyAI.new()
		battle.enemies_root.add_child(enemy)
		enemy.setup(load("res://resources/enemies/%s.tres" % ids[i]), 85, battle.player)
		enemy.global_position = battle.player.global_position + Vector2(80 + i * 34, -18 + i * 35)
		enemy.hp = 1000000
		enemy.max_hp = enemy.hp
		enemy.attack = 1.0
		enemy._spawn_reveal_left = 0.0
		battle._enemies.append(enemy)
		if i == 0:
			enemy.draw.connect(func() -> void: _enemy_draws += 1)
	ui._refresh_all()
	await get_tree().create_timer(0.1).timeout
	await _shot("combat.png")
	for frame: int in 30:
		if frame % 10 == 0:
			battle._perform_auto_attack()
		battle._tick_attack_release(0.08)
		battle._update_pet_combat(0.08)
		await get_tree().create_timer(0.08).timeout
		await _shot("motion_%02d.png" % frame)
	if _enemy_draws < 15:
		_failed = true
		push_error("Enemy movement failed to request continuous redraw")
	ui._toggle_management(3)
	PetManager.owned_pets = {"spirit_fox": {"level": 37, "stars": 1, "xp": 5, "shards": 10}}
	PetManager.summon_crystals = 6000
	PetManager.summon_pity = 29
	ui._refresh_pets()
	seed(20260925)
	ui._summon_pet_once()
	await _shot("summon_single.png")
	_assert_inside(ui.summon_results)
	ui.summon_results.close()
	for pet_id: String in PetManager.all_pet_ids():
		PetManager.unlock_pet(pet_id)
	ui._summon_pet_ten()
	await _shot("summon_ten.png")
	_assert_inside(ui.summon_results)
	PetManager.add_essence(1)
	await _shot("summon_ten_after_combat_refresh.png")
	if not ui.summon_results.visible or ui.summon_results.result_cards.size() != 10:
		_failed = true
	ui.summon_results.close()
	await _shot("pet_shards.png")
	var metadata: Dictionary = {
		"fixture": "synthetic floor 85, sage, isolated pet account", "engine": Engine.get_version_info(),
		"renderer": DisplayServer.get_name(), "enemy_redraws": _enemy_draws,
		"viewport": str(get_viewport().get_visible_rect().size),
		"source": BuildInfo.read_info(), "windows_user_approval": "NOT_RUN",
		"art_limit": "Existing static character/enemy atlas and procedural pet art retained; not a frame-animation approval",
	}
	var out := FileAccess.open(OUTPUT.path_join("metadata.json"), FileAccess.WRITE)
	out.store_string(JSON.stringify(metadata, "\t"))
	out.close()
	print("PLAYTEST_RECOVERY_CAPTURE %s; enemy redraws %d" % ["FAILED" if _failed else "PASSED", _enemy_draws])
	get_tree().quit(1 if _failed else 0)

func _assert_inside(panel: SummonResultPanel) -> void:
	var viewport_rect: Rect2 = get_viewport().get_visible_rect()
	var targets: Array[Control] = [panel._panel, panel._close_button]
	targets.append_array(panel.result_cards)
	for control: Control in targets:
		if not viewport_rect.encloses(control.get_global_rect()):
			_failed = true
			push_error("Result content clipped: " + str(control.get_global_rect()))

func _shot(filename: String) -> void:
	await get_tree().process_frame
	await RenderingServer.frame_post_draw
	var image: Image = get_viewport().get_texture().get_image()
	if image.is_empty() or image.save_png(OUTPUT.path_join(filename)) != OK:
		_failed = true
