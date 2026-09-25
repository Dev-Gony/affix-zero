class_name V0Main
extends Node2D

const PlayerScript = preload("res://vslice/v0_player.gd")
const EnemyScript = preload("res://vslice/v0_enemy.gd")
const PickupScript = preload("res://vslice/v0_pickup.gd")
const Loader = preload("res://vslice/v0_asset_loader.gd")

const ARENA := Rect2(36, 54, 568, 270)
const TARGET_KILLS := 30

var background: Texture2D
var class_atlas: Texture2D
var enemy_atlas: Texture2D
var player: V0Player
var enemies: Array[V0Enemy] = []
var pickups: Array[V0Pickup] = []

var elapsed: float = 0.0
var spawn_timer: float = 0.4
var spawn_serial: int = 0
var kills: int = 0
var gold: int = 0
var xp: int = 0
var level: int = 1
var xp_to_level: int = 45
var boss_spawned: bool = false
var boss_dead: bool = false
var banner_text: String = ""
var banner_left: float = 0.0
var hit_sparks: Array[Dictionary] = []
var damage_texts: Array[Dictionary] = []

func _ready() -> void:
	randomize()
	y_sort_enabled = true
	background = Loader.load_repo_texture("assets/sprites/dungeon_courtyard.png")
	class_atlas = Loader.load_repo_texture("assets/sprites/class_atlas_alpha.png")
	enemy_atlas = Loader.load_repo_texture("assets/sprites/enemy_atlas_alpha.png")
	if background == null or class_atlas == null or enemy_atlas == null:
		push_error("V0 requires the repository production atlases under assets/sprites.")
		return

	player = PlayerScript.new() as V0Player
	player.name = "Warrior"
	add_child(player)
	player.global_position = ARENA.get_center()
	player.configure(class_atlas)
	player.attack_landed.connect(_on_player_attack_landed)
	player.died.connect(_on_player_died)
	_show_banner("STAGE 1 · RUINED COURTYARD", 2.0)
	queue_redraw()

func _process(delta: float) -> void:
	if player == null:
		return
	elapsed += delta
	banner_left = maxf(0.0, banner_left - delta)
	spawn_timer -= delta
	_update_effects(delta)
	_cleanup_lists()
	_clamp_actors()

	if not player.dead and not boss_dead:
		_assign_player_target()
		if not boss_spawned and kills < TARGET_KILLS and spawn_timer <= 0.0:
			spawn_timer = maxf(0.55, 1.45 - elapsed * 0.008)
			_spawn_pack(2 if enemies.size() < 10 else 1)
		if kills >= TARGET_KILLS and not boss_spawned:
			boss_spawned = true
			_spawn_boss()

	queue_redraw()

func _spawn_pack(count: int) -> void:
	for i in count:
		if enemies.size() >= 18:
			return
		var kind_index := (spawn_serial + i) % 4
		var id: StringName
		var cell: Vector2i
		match kind_index:
			0:
				id = &"slime"; cell = Vector2i(0,0)
			1:
				id = &"bat"; cell = Vector2i(1,0)
			2:
				id = &"skeleton"; cell = Vector2i(2,0)
			_:
				id = &"goblin"; cell = Vector2i(3,0)
		_spawn_enemy(id, cell, false)
		spawn_serial += 1

func _spawn_boss() -> void:
	_show_banner("ELITE · DARK KNIGHT", 2.0)
	_spawn_enemy(&"dark_knight", Vector2i(0,1), true)

func _spawn_enemy(id: StringName, cell: Vector2i, boss: bool) -> void:
	var enemy := EnemyScript.new() as V0Enemy
	enemy.name = "Enemy_%s_%d" % [String(id), spawn_serial]
	add_child(enemy)
	enemy.global_position = _edge_spawn_position()
	enemy.configure(enemy_atlas, id, cell, player, boss)
	match id:
		&"bat":
			enemy.max_hp = 48; enemy.hp = 48; enemy.speed = 74; enemy.damage = 5; enemy.reward_xp = 6
		&"skeleton":
			enemy.max_hp = 82; enemy.hp = 82; enemy.speed = 46; enemy.damage = 10; enemy.reward_xp = 10
		&"goblin":
			enemy.max_hp = 70; enemy.hp = 70; enemy.speed = 60; enemy.damage = 8; enemy.reward_xp = 9
	if not boss and elapsed > 35.0:
		enemy.max_hp = roundi(enemy.max_hp * 1.30)
		enemy.hp = enemy.max_hp
		enemy.damage = roundi(enemy.damage * 1.18)
	enemy.died.connect(_on_enemy_died)
	enemy.attacked_player.connect(_on_enemy_attacked_player)
	enemies.append(enemy)

func _edge_spawn_position() -> Vector2:
	var side := randi() % 4
	var pad := 8.0
	match side:
		0: return Vector2(randf_range(ARENA.position.x + 20, ARENA.end.x - 20), ARENA.position.y + pad)
		1: return Vector2(ARENA.end.x - pad, randf_range(ARENA.position.y + 20, ARENA.end.y - 20))
		2: return Vector2(randf_range(ARENA.position.x + 20, ARENA.end.x - 20), ARENA.end.y - pad)
		_: return Vector2(ARENA.position.x + pad, randf_range(ARENA.position.y + 20, ARENA.end.y - 20))

func _assign_player_target() -> void:
	if player.target != null and is_instance_valid(player.target):
		if player.target.has_method("is_dead") and not bool(player.target.call("is_dead")):
			return
	var nearest: V0Enemy
	var best := INF
	for enemy in enemies:
		if enemy == null or not is_instance_valid(enemy) or enemy.is_dead():
			continue
		var d := player.global_position.distance_squared_to(enemy.global_position)
		if d < best:
			best = d
			nearest = enemy
	player.set_target(nearest)

func _clamp_actors() -> void:
	if player != null:
		player.global_position = Vector2(
			clampf(player.global_position.x, ARENA.position.x + 16.0, ARENA.end.x - 16.0),
			clampf(player.global_position.y, ARENA.position.y + 18.0, ARENA.end.y - 14.0)
		)
	for enemy in enemies:
		if enemy == null or not is_instance_valid(enemy):
			continue
		enemy.global_position = Vector2(
			clampf(enemy.global_position.x, ARENA.position.x + 8.0, ARENA.end.x - 8.0),
			clampf(enemy.global_position.y, ARENA.position.y + 10.0, ARENA.end.y - 8.0)
		)

func separation_for(self_enemy: V0Enemy) -> Vector2:
	var push := Vector2.ZERO
	var seen := 0
	for other in enemies:
		if other == null or other == self_enemy or not is_instance_valid(other) or other.is_dead():
			continue
		var offset := self_enemy.global_position - other.global_position
		var distance := offset.length()
		if distance > 0.1 and distance < 30.0:
			push += offset.normalized() * ((30.0 - distance) / 30.0)
			seen += 1
	if seen > 0:
		push /= float(seen)
	return push

func _on_player_attack_landed(_target: Node, damage: int, world_position: Vector2) -> void:
	_spawn_hit_sparks(world_position, Color("ffd17a"), 7)
	damage_texts.append({"pos":world_position + Vector2(-8,-18),"text":"%d" % damage,"life":0.55})

func _on_enemy_attacked_player(damage: int) -> void:
	player.take_damage(damage)
	_spawn_hit_sparks(player.global_position, Color("ff6b6b"), 5)

func _on_enemy_died(enemy: V0Enemy, reward_xp: int, reward_gold: int, world_position: Vector2) -> void:
	kills += 1
	player.kills = kills
	_spawn_hit_sparks(world_position, Color("f5c46b"), 10 if enemy.is_boss else 5)
	var orb_count := maxi(1, mini(4, ceili(float(reward_xp) / 4.0)))
	var xp_per_orb := maxi(1, ceili(float(reward_xp) / float(orb_count)))
	for i in orb_count:
		_spawn_pickup(&"xp", xp_per_orb, world_position + Vector2(randf_range(-6,6),randf_range(-4,4)))
	if reward_gold > 0:
		_spawn_pickup(&"gold", reward_gold, world_position)
	if enemy.is_boss:
		boss_dead = true
		_show_banner("STAGE CLEAR", 999.0)
	enemies.erase(enemy)
	if player.target == enemy:
		player.set_target(null)

func _spawn_pickup(kind: StringName, amount: int, position: Vector2) -> void:
	var pickup := PickupScript.new() as V0Pickup
	add_child(pickup)
	pickup.global_position = position
	pickup.setup(kind, amount, player)
	pickup.collected.connect(_on_pickup_collected)
	pickups.append(pickup)

func _on_pickup_collected(kind: StringName, amount: int) -> void:
	if kind == &"xp":
		xp += amount
		while xp >= xp_to_level:
			xp -= xp_to_level
			level += 1
			xp_to_level = roundi(float(xp_to_level) * 1.28)
			player.attack_damage += 5
			player.max_hp += 16
			player.heal(40)
			_show_banner("LEVEL UP · ATK +5", 1.2)
	else:
		gold += amount

func _on_player_died() -> void:
	_show_banner("DEFEAT · R TO RETRY", 999.0)

func _unhandled_key_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo and event.keycode == KEY_R:
		if player.dead or boss_dead:
			get_tree().reload_current_scene()

func _show_banner(text: String, duration: float) -> void:
	banner_text = text
	banner_left = duration

func _spawn_hit_sparks(position: Vector2, color: Color, count: int) -> void:
	for i in count:
		hit_sparks.append({
			"pos": position,
			"vel": Vector2.RIGHT.rotated(randf()*TAU) * randf_range(28,82),
			"life": randf_range(0.18,0.34),
			"color": color
		})

func _update_effects(delta: float) -> void:
	for fx in hit_sparks:
		fx["life"] = float(fx["life"]) - delta
		fx["pos"] = Vector2(fx["pos"]) + Vector2(fx["vel"]) * delta
		fx["vel"] = Vector2(fx["vel"]) * 0.88
	for i in range(hit_sparks.size()-1,-1,-1):
		if float(hit_sparks[i]["life"]) <= 0:
			hit_sparks.remove_at(i)
	for text_data in damage_texts:
		text_data["life"] = float(text_data["life"]) - delta
		text_data["pos"] = Vector2(text_data["pos"]) + Vector2(0,-24)*delta
	for i in range(damage_texts.size()-1,-1,-1):
		if float(damage_texts[i]["life"]) <= 0:
			damage_texts.remove_at(i)

func _cleanup_lists() -> void:
	for i in range(pickups.size()-1,-1,-1):
		if pickups[i] == null or not is_instance_valid(pickups[i]):
			pickups.remove_at(i)
	for i in range(enemies.size()-1,-1,-1):
		if enemies[i] == null or not is_instance_valid(enemies[i]):
			enemies.remove_at(i)

func _draw() -> void:
	draw_rect(Rect2(0,0,640,360), Color("0b0c10"))
	if background != null:
		draw_texture_rect(background, Rect2(0,36,640,324), false, Color(0.82,0.82,0.88,1))
	draw_rect(Rect2(0,36,640,324), Color(0.03,0.02,0.04,0.20), true)
	draw_rect(ARENA, Color(0.75,0.76,0.82,0.12), false, 2.0)

	for fx in hit_sparks:
		var color: Color = fx["color"]
		color.a = clampf(float(fx["life"]) / 0.34, 0.0, 1.0)
		draw_circle(Vector2(fx["pos"]), 2.2, color)
	for text_data in damage_texts:
		var alpha := clampf(float(text_data["life"]) / 0.55, 0.0, 1.0)
		draw_string(ThemeDB.fallback_font, Vector2(text_data["pos"]), String(text_data["text"]), HORIZONTAL_ALIGNMENT_LEFT, -1, 12, Color(1,0.93,0.82,alpha))

	# Compact game HUD, deliberately unlike the E0 diagnostics wall of text.
	draw_rect(Rect2(0,0,640,36), Color(0.025,0.028,0.038,0.93), true)
	draw_string(ThemeDB.fallback_font, Vector2(14,23), "AFFIX: ZERO   STAGE 1", HORIZONTAL_ALIGNMENT_LEFT, -1, 15, Color("f2d083"))
	var hp_ratio := float(player.hp)/float(player.max_hp) if player != null else 0.0
	draw_rect(Rect2(178,10,134,9), Color("25171a"), true)
	draw_rect(Rect2(179,11,132*hp_ratio,7), Color("d95058"), true)
	draw_string(ThemeDB.fallback_font, Vector2(320,20), "Lv.%d  %dG" % [level,gold], HORIZONTAL_ALIGNMENT_LEFT, -1, 11, Color("d6dde8"))
	var objective := "BOSS" if boss_spawned and not boss_dead else ("%d/%d" % [mini(kills,TARGET_KILLS),TARGET_KILLS])
	draw_string(ThemeDB.fallback_font, Vector2(544,21), objective, HORIZONTAL_ALIGNMENT_LEFT, -1, 12, Color("f0b84b"))

	var xp_ratio := clampf(float(xp)/float(maxi(1,xp_to_level)),0,1)
	draw_rect(Rect2(0,356,640,4), Color("132018"), true)
	draw_rect(Rect2(0,356,640*xp_ratio,4), Color("58d885"), true)

	if banner_left > 0.0:
		var banner_width := minf(420.0, 80.0 + float(banner_text.length()) * 12.0)
		draw_rect(Rect2(320-banner_width*0.5,52,banner_width,34), Color(0.03,0.03,0.05,0.80), true)
		draw_string(ThemeDB.fallback_font, Vector2(320-banner_width*0.5+18,75), banner_text, HORIZONTAL_ALIGNMENT_CENTER, banner_width-36, 16, Color("ffe29a"))
