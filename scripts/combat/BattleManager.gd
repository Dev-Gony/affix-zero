extends Node2D
class_name BattleManager

const WORLD_RECT := Rect2(0, 0, 1920, 1200)
const PLAYER_POSITION := Vector2(960, 600)
const BOSS_FLOOR_INTERVAL: int = 10
const PLAYER_BASE_MOVE_SPEED: float = 54.0
const MELEE_ATTACK_RANGE: float = 42.0
const RANGED_ATTACK_RANGE: float = 112.0
const WANDER_RESELECT_TIME: float = 1.8
const FLOOR_TILE: Texture2D = preload("res://assets/cc0/tiny_dungeon/floor.png")
const BRICK_TILE: Texture2D = preload("res://assets/cc0/tiny_dungeon/brick_floor.png")
const WALL_TILE: Texture2D = preload("res://assets/cc0/tiny_dungeon/wall.png")
const RUBBLE_TILE: Texture2D = preload("res://assets/cc0/tiny_dungeon/rubble.png")
const SAND_FLOOR_TILE: Texture2D = preload("res://assets/cc0/tiny_dungeon/sand_floor.png")
const SAND_DETAIL_TILE: Texture2D = preload("res://assets/cc0/tiny_dungeon/sand_detail.png")
const BLUE_WALL_TILE: Texture2D = preload("res://assets/cc0/tiny_dungeon/blue_wall.png")
const SHRINE_TILE: Texture2D = preload("res://assets/cc0/tiny_dungeon/shrine.png")
const ENEMY_RESOURCE_PATHS: Array[String] = [
	"res://resources/enemies/slime.tres",
	"res://resources/enemies/bat.tres",
	"res://resources/enemies/skeleton.tres",
	"res://resources/enemies/goblin.tres",
	"res://resources/enemies/dark_knight.tres",
	"res://resources/enemies/lich.tres",
	"res://resources/enemies/dragon.tres",
	"res://resources/enemies/demon_lord.tres",
]

@onready var player: PlayerAvatar = $Player
@onready var camera: Camera2D = $Player/Camera2D
@onready var enemies_root: Node2D = $Enemies
@onready var projectiles_root: Node2D = $Projectiles
@onready var effects: EffectLayer = $Effects

var _enemy_resources: Array[EnemyData] = []
var _boss_resource: EnemyData
var _enemies: Array[EnemyAI] = []
var _attack_time_left: float = 0.0
var _skill_time_left: float = 3.0
var _respawning: bool = false
var _shake_time_left: float = 0.0
var _shake_intensity: float = 0.0
var _last_death_position: Vector2 = PLAYER_POSITION
var _wander_target: Vector2 = PLAYER_POSITION
var _wander_time_left: float = 0.0
var _current_room: int = 4
var _combat_rect: Rect2 = Rect2()
var _traveling: bool = false
var _travel_target_room: int = 4
var _travel_waypoints: Array[Vector2] = []
var _travel_index: int = 0


func _ready() -> void:
	randomize()
	_load_enemy_resources()
	_current_room = WorldLayout.room_index_for_floor(GameManager.floor)
	_combat_rect = WorldLayout.walk_rect(_current_room)
	player.position = WorldLayout.room_center(_current_room)
	camera.limit_left = int(WORLD_RECT.position.x)
	camera.limit_top = int(WORLD_RECT.position.y)
	camera.limit_right = int(WORLD_RECT.end.x)
	camera.limit_bottom = int(WORLD_RECT.end.y)
	player.visible = not GameManager.selected_class.is_empty()
	GameManager.class_selected.connect(_on_class_selected)
	GameManager.player_died.connect(_on_player_died)
	GameManager.floor_changed.connect(_on_floor_changed)
	GameManager.game_state_changed.connect(_on_game_state_changed)
	LevelManager.level_up.connect(_on_level_up)
	LootManager.item_dropped.connect(_on_item_dropped)
	queue_redraw()
	if GameManager.game_state == GameManager.GameState.RUNNING and not GameManager.selected_class.is_empty():
		call_deferred("_start_battle")


func _process(delta: float) -> void:
	_update_camera_shake(delta)
	if _respawning or GameManager.game_state != GameManager.GameState.RUNNING:
		return
	if _traveling:
		_update_room_travel(delta)
		return
	_update_target_marker()
	_update_auto_hunt(delta)
	_attack_time_left -= delta
	_skill_time_left -= delta
	if _attack_time_left <= 0.0:
		_attack_time_left = 1.0 / maxf(0.2, GameManager.spd)
		_perform_auto_attack()
	if _skill_time_left <= 0.0:
		_skill_time_left = 3.0
		_perform_auto_skill()


func _draw() -> void:
	draw_rect(WORLD_RECT, Color("17131c"), true)
	for room_index in 9:
		var room := WorldLayout.room_rect(room_index)
		var walk := WorldLayout.walk_rect(room_index)
		match room_index % 3:
			0:
				_draw_tiled_rect(room, FLOOR_TILE, Color("4d3438"))
				_draw_tiled_rect(walk, BRICK_TILE, Color("80666d"))
				_draw_room_walls(walk, WALL_TILE, Color("9aa0aa"))
				draw_texture_rect(SHRINE_TILE, Rect2(walk.get_center() - Vector2(20, 20), Vector2(40, 40)), false, Color("d6b36b"))
				draw_arc(walk.get_center(), 58.0, 0.0, TAU, 36, Color(0.72, 0.22, 0.19, 0.28), 2.0)
			1:
				_draw_tiled_rect(room, SAND_FLOOR_TILE, Color("6b4c3e"))
				_draw_tiled_rect(walk, SAND_FLOOR_TILE, Color("b28767"))
				_draw_room_walls(walk, WALL_TILE, Color("7e8792"))
				draw_texture_rect(SAND_DETAIL_TILE, Rect2(walk.position + Vector2(58, 42), Vector2(42, 42)), false, Color("caa785"))
				draw_texture_rect(SAND_DETAIL_TILE, Rect2(walk.end - Vector2(106, 84), Vector2(38, 38)), false, Color("a37e66"))
			_:
				_draw_tiled_rect(room, FLOOR_TILE, Color("303844"))
				_draw_tiled_rect(walk, BRICK_TILE, Color("586579"))
				_draw_room_walls(walk, BLUE_WALL_TILE, Color("9bb0c4"))
				draw_texture_rect(RUBBLE_TILE, Rect2(walk.position + Vector2(48, 46), Vector2(30, 30)), false, Color("9ba8b5"))
				draw_texture_rect(RUBBLE_TILE, Rect2(walk.end - Vector2(86, 72), Vector2(26, 26)), false, Color("7e8b97"))
				draw_arc(walk.get_center(), 46.0, 0.0, TAU, 32, Color(0.24, 0.55, 0.72, 0.22), 2.0)
	for pair: Vector2i in WorldLayout.connected_room_pairs():
		var corridor := WorldLayout.corridor_rect(pair.x, pair.y)
		if corridor.size.is_zero_approx():
			continue
		var corridor_texture: Texture2D = BRICK_TILE if pair.x % 2 == 0 else SAND_FLOOR_TILE
		var corridor_tint: Color = Color("6a5960") if pair.x % 2 == 0 else Color("9a735c")
		_draw_tiled_rect(corridor, corridor_texture, corridor_tint)
	draw_rect(WORLD_RECT, Color(0.015, 0.01, 0.02, 0.10), true)


func _draw_tiled_rect(area: Rect2, texture: Texture2D, modulate: Color) -> void:
	var tile_size := Vector2(16, 16)
	var y: float = area.position.y
	while y < area.end.y:
		var x: float = area.position.x
		while x < area.end.x:
			var size := Vector2(minf(tile_size.x, area.end.x - x), minf(tile_size.y, area.end.y - y))
			draw_texture_rect(texture, Rect2(Vector2(x, y), size), false, modulate)
			x += tile_size.x
		y += tile_size.y


func _draw_room_walls(walk: Rect2, texture: Texture2D, tint: Color) -> void:
	var wall := 16.0
	_draw_tiled_rect(Rect2(walk.position - Vector2(wall, wall), Vector2(walk.size.x + wall * 2.0, wall)), texture, tint.lightened(0.08))
	_draw_tiled_rect(Rect2(Vector2(walk.position.x - wall, walk.end.y), Vector2(walk.size.x + wall * 2.0, wall)), texture, tint.darkened(0.12))
	_draw_tiled_rect(Rect2(Vector2(walk.position.x - wall, walk.position.y), Vector2(wall, walk.size.y)), texture, tint)
	_draw_tiled_rect(Rect2(Vector2(walk.end.x, walk.position.y), Vector2(wall, walk.size.y)), texture, tint)


func _start_battle() -> void:
	_respawning = false
	_traveling = false
	_current_room = WorldLayout.room_index_for_floor(GameManager.floor)
	_combat_rect = WorldLayout.walk_rect(_current_room)
	player.visible = true
	player.position = WorldLayout.room_center(_current_room)
	player.set_move_direction(Vector2.ZERO)
	_configure_player_visual()
	_attack_time_left = 0.15
	_skill_time_left = 3.0
	AudioManager.play_bgm_for_floor(GameManager.floor)
	if _enemies.is_empty():
		_spawn_wave()


func _spawn_wave() -> void:
	if _respawning or GameManager.game_state != GameManager.GameState.RUNNING:
		return
	if is_boss_floor():
		_spawn_boss()
		return
	var eligible: Array[EnemyData] = []
	for enemy_resource: EnemyData in _enemy_resources:
		if enemy_resource.behavior != "boss" and enemy_resource.unlock_floor <= GameManager.floor:
			eligible.append(enemy_resource)
	if eligible.is_empty():
		return
	var wave_size: int = mini(3 + floori(GameManager.floor * 0.5), 12)
	for index: int in wave_size:
		var enemy := EnemyAI.new()
		enemy.name = "Enemy_%d_%d" % [GameManager.floor, index]
		enemies_root.add_child(enemy)
		enemy.global_position = _random_spawn_position()
		enemy.setup(eligible.pick_random(), GameManager.floor, player, _combat_rect)
		_connect_enemy(enemy)
		_enemies.append(enemy)
	if wave_size > 0:
		GameManager.notification_requested.emit("적 증원 %d마리 접근" % wave_size, Color("e5b06a"))


func is_boss_floor(floor_number: int = GameManager.floor) -> bool:
	return floor_number > 0 and floor_number % BOSS_FLOOR_INTERVAL == 0


func _spawn_boss() -> void:
	if _boss_resource == null:
		push_error("Boss resource is not available")
		return
	var boss := EnemyAI.new()
	boss.name = "Boss_%d" % GameManager.floor
	enemies_root.add_child(boss)
	boss.global_position = _random_spawn_position()
	boss.setup(_boss_resource, GameManager.floor, player, _combat_rect)
	_connect_enemy(boss)
	_enemies.append(boss)
	GameManager.notification_requested.emit("보스 출현 · %s" % _boss_resource.display_name, Color("ff6b6b"))


func _connect_enemy(enemy: EnemyAI) -> void:
	enemy.died.connect(_on_enemy_died)
	enemy.attacked_player.connect(_on_enemy_attack)
	enemy.damage_received.connect(_on_enemy_damage_received)


func _perform_auto_attack() -> void:
	var target: EnemyAI = _nearest_enemy()
	if target == null:
		return
	if player.global_position.distance_to(target.global_position) > _attack_range():
		return
	var attack_power: float = GameManager.atk * GameManager.skill_damage_multiplier()
	var result: Dictionary = DamageCalculator.calculate_damage(attack_power, target.defense, 0, GameManager.penetration, GameManager.crit, false)
	player.play_attack(target.global_position)
	effects.show_attack(player.global_position, target.global_position, bool(result.get("critical", false)))
	target.take_hit(result)
	AudioManager.play_sfx("critical_hit" if bool(result.get("critical", false)) else "basic_attack")
	if bool(result.get("critical", false)):
		_start_shake(2.5, 0.15)


func _update_auto_hunt(delta: float) -> void:
	var target: EnemyAI = _nearest_enemy()
	if target != null:
		var distance: float = player.global_position.distance_to(target.global_position)
		var desired_range: float = _attack_range() * 0.82
		if distance > desired_range:
			_move_player_toward(target.global_position, delta)
		else:
			player.set_move_direction(Vector2.ZERO)
		return

	_wander_time_left -= delta
	if _wander_time_left <= 0.0 or player.global_position.distance_to(_wander_target) < 8.0:
		_wander_time_left = WANDER_RESELECT_TIME
		_wander_target = Vector2(
			randf_range(_combat_rect.position.x + 24.0, _combat_rect.end.x - 24.0),
			randf_range(_combat_rect.position.y + 24.0, _combat_rect.end.y - 24.0)
		)
	_move_player_toward(_wander_target, delta)


func _move_player_toward(world_target: Vector2, delta: float) -> void:
	var direction: Vector2 = player.global_position.direction_to(world_target)
	if direction.is_zero_approx():
		player.set_move_direction(Vector2.ZERO)
		return
	var move_speed: float = PLAYER_BASE_MOVE_SPEED * clampf(GameManager.spd, 0.75, 2.2)
	var next_position: Vector2 = player.global_position + direction * move_speed * delta
	player.global_position = Vector2(
		clampf(next_position.x, _combat_rect.position.x + 14.0, _combat_rect.end.x - 14.0),
		clampf(next_position.y, _combat_rect.position.y + 16.0, _combat_rect.end.y - 14.0)
	)
	player.set_move_direction(direction)


func _begin_room_travel(from_room: int, to_room: int) -> void:
	_clear_enemies()
	_travel_target_room = to_room
	_travel_waypoints = WorldLayout.travel_waypoints(from_room, to_room)
	_travel_index = 0
	_traveling = not _travel_waypoints.is_empty()
	player.set_move_direction(Vector2.ZERO)
	if _traveling:
		GameManager.notification_requested.emit("다음 구역으로 이동 중", Color("8be0f1"))
	else:
		_current_room = to_room
		_combat_rect = WorldLayout.walk_rect(_current_room)
		player.position = WorldLayout.room_center(_current_room)
		_spawn_wave()


func _update_room_travel(delta: float) -> void:
	if _travel_index >= _travel_waypoints.size():
		_finish_room_travel()
		return
	var waypoint: Vector2 = _travel_waypoints[_travel_index]
	var distance: float = player.global_position.distance_to(waypoint)
	if distance <= 5.0:
		player.global_position = waypoint
		_travel_index += 1
		if _travel_index >= _travel_waypoints.size():
			_finish_room_travel()
		return
	var direction: Vector2 = player.global_position.direction_to(waypoint)
	var move_speed: float = PLAYER_BASE_MOVE_SPEED * 1.45 * clampf(GameManager.spd, 0.8, 2.0)
	player.global_position += direction * minf(move_speed * delta, distance)
	player.set_move_direction(direction)


func _finish_room_travel() -> void:
	_traveling = false
	_current_room = _travel_target_room
	_combat_rect = WorldLayout.walk_rect(_current_room)
	player.position = WorldLayout.room_center(_current_room)
	player.set_move_direction(Vector2.ZERO)
	_wander_time_left = 0.0
	_spawn_wave()


func _attack_range() -> float:
	if GameManager.selected_class in ["mage", "sage", "saint"]:
		return RANGED_ATTACK_RANGE
	return MELEE_ATTACK_RANGE


func _perform_auto_skill() -> void:
	if _enemies.is_empty():
		return
	var skill_type: String = GameManager.selected_skill_type
	var visual_target: EnemyAI = _nearest_enemy()
	player.play_skill(skill_type, visual_target.global_position if visual_target != null else player.global_position + Vector2.RIGHT)
	AudioManager.play_sfx(skill_type)
	match skill_type:
		"melee_spin": _cast_melee_spin()
		"fireball": _cast_fireball()
		"shield_charge": _cast_shield_charge()
		"chain_lightning": _cast_chain_lightning()
		"multi_slash": _cast_multi_slash()
		"holy_nova": _cast_holy_nova()


func _cast_melee_spin() -> void:
	effects.show_melee_spin(player.global_position)
	for enemy: EnemyAI in _enemies.duplicate():
		if is_instance_valid(enemy) and enemy.global_position.distance_to(player.global_position) <= 82.0:
			_deal_skill_damage(enemy)


func _cast_fireball() -> void:
	var target: EnemyAI = _nearest_enemy()
	if target == null:
		return
	var projectile := SkillProjectile.new()
	projectiles_root.add_child(projectile)
	projectile.setup(player.global_position, target.global_position)
	projectile.impacted.connect(_on_fireball_impacted)


func _on_fireball_impacted(impact_position: Vector2) -> void:
	effects.show_fireball_explosion(impact_position)
	for enemy: EnemyAI in _enemies.duplicate():
		if is_instance_valid(enemy) and enemy.global_position.distance_to(impact_position) <= 48.0:
			_deal_skill_damage(enemy)


func _cast_shield_charge() -> void:
	var target: EnemyAI = _nearest_enemy()
	if target == null:
		return
	var direction: Vector2 = player.global_position.direction_to(target.global_position)
	var end_position: Vector2 = player.global_position + direction * 115.0
	effects.show_shield_charge(player.global_position, end_position)
	for enemy: EnemyAI in _enemies.duplicate():
		if not is_instance_valid(enemy):
			continue
		var relative: Vector2 = enemy.global_position - player.global_position
		var projection: float = relative.dot(direction)
		var line_distance: float = absf(relative.cross(direction))
		if projection >= 0.0 and projection <= 115.0 and line_distance <= 25.0:
			_deal_skill_damage(enemy)


func _cast_chain_lightning() -> void:
	var targets: Array[EnemyAI] = _valid_enemies_sorted()
	if targets.size() > 5:
		targets.resize(5)
	var points := PackedVector2Array([player.global_position])
	for enemy: EnemyAI in targets:
		points.append(enemy.global_position)
		_deal_skill_damage(enemy)
	effects.show_chain_lightning(points)


func _cast_multi_slash() -> void:
	var targets: Array[EnemyAI] = _valid_enemies_sorted()
	if targets.is_empty():
		return
	var center: Vector2 = targets[0].global_position
	effects.show_multi_slash(center)
	for index: int in 5:
		var target: EnemyAI = targets[index % targets.size()]
		if is_instance_valid(target):
			_deal_skill_damage(target, 0.46)


func _cast_holy_nova() -> void:
	effects.show_holy_nova(player.global_position)
	for enemy: EnemyAI in _enemies.duplicate():
		if is_instance_valid(enemy) and enemy.global_position.distance_to(player.global_position) <= 125.0:
			_deal_skill_damage(enemy)


func _deal_skill_damage(enemy: EnemyAI, power_scale: float = 1.0) -> void:
	if not is_instance_valid(enemy):
		return
	var skill_level: int = 1 + floori(float(GameManager.level - 1) / 5.0)
	var attack_power: float = GameManager.atk * power_scale * GameManager.skill_damage_multiplier()
	var result: Dictionary = DamageCalculator.calculate_damage(attack_power, enemy.defense, skill_level, GameManager.penetration, GameManager.crit, true)
	enemy.take_hit(result)
	if bool(result.get("critical", false)):
		_start_shake(3.0, 0.18)


func _on_enemy_died(enemy: EnemyAI, world_position: Vector2, fragment_color: Color, xp_reward: int, gold_reward: int) -> void:
	if not _enemies.has(enemy):
		return
	var defeated_boss: bool = enemy.behavior == "boss" and is_boss_floor()
	_enemies.erase(enemy)
	_last_death_position = world_position
	effects.spawn_fragments(world_position, fragment_color, randi_range(8, 12), 68.0)
	AudioManager.play_sfx("monster_death")
	GameManager.record_kill()
	LevelManager.add_xp(xp_reward)
	var adjusted_gold: int = maxi(1, roundi(gold_reward * RebirthManager.gold_multiplier() * (1.0 + GameManager.gold_bonus * 0.01)))
	GameManager.add_gold(adjusted_gold)
	effects.show_gold(world_position, adjusted_gold)
	if defeated_boss:
		var boss_reward: Dictionary = LootManager.drop_boss_reward()
		var reward_text: String = String(boss_reward.get("name", "보상 골드"))
		GameManager.notification_requested.emit("보스 격파 · %s 획득" % reward_text, Color("ffd166"))
		var previous_room: int = _current_room
		GameManager.advance_floor()
		_begin_room_travel(previous_room, WorldLayout.room_index_for_floor(GameManager.floor))
		return
	LootManager.try_drop()
	if GameManager.kills_on_floor >= 8 + GameManager.floor:
		var previous_room: int = _current_room
		GameManager.advance_floor()
		_begin_room_travel(previous_room, WorldLayout.room_index_for_floor(GameManager.floor))
	elif _enemies.is_empty():
		_spawn_wave()


func _on_enemy_attack(attacker: EnemyAI, raw_damage: float) -> void:
	if _respawning:
		return
	player.play_hit()
	var ranged: bool = is_instance_valid(attacker) and attacker.behavior in ["caster", "boss"]
	effects.show_enemy_attack(attacker.global_position if is_instance_valid(attacker) else player.global_position + Vector2.LEFT * 12.0, player.global_position, ranged)
	var damage: int = GameManager.take_damage(raw_damage)
	effects.show_damage(player.global_position + Vector2(0, -12), damage, false)
	effects.spawn_fragments(player.global_position, Color("ff4d5a"), randi_range(3, 5), 45.0)
	_start_shake(2.0, 0.12)


func _on_enemy_damage_received(world_position: Vector2, damage: int, critical: bool) -> void:
	effects.show_damage(world_position, damage, critical)


func _on_player_died() -> void:
	if _respawning:
		return
	_respawning = true
	GameManager.set_game_state(GameManager.GameState.PAUSED)
	_clear_enemies()
	GameManager.notification_requested.emit("쓰러졌습니다 · 1층 후퇴 후 자동 부활", Color("ff6b6b"))
	await get_tree().create_timer(0.75).timeout
	GameManager.retreat_floor()
	GameManager.revive()
	effects.clear_effects()
	_current_room = WorldLayout.room_index_for_floor(GameManager.floor)
	_combat_rect = WorldLayout.walk_rect(_current_room)
	player.position = WorldLayout.room_center(_current_room)
	player.set_move_direction(Vector2.ZERO)
	_respawning = false
	GameManager.set_game_state(GameManager.GameState.RUNNING)
	_spawn_wave()


func _on_class_selected(_class_id: String) -> void:
	_clear_enemies()
	_start_battle()


func _on_floor_changed(new_floor: int) -> void:
	AudioManager.play_bgm_for_floor(new_floor)
	queue_redraw()


func _on_game_state_changed(state: GameManager.GameState) -> void:
	if state == GameManager.GameState.CLASS_SELECTION:
		_clear_enemies()
		effects.clear_effects()
		player.visible = false


func _on_level_up(_new_level: int) -> void:
	effects.show_level_up(player.global_position)


func _on_item_dropped(item: Dictionary) -> void:
	effects.show_drop(_last_death_position, item)
	var is_legend: bool = String(item.get("rarity_id", "")) == "legend"
	AudioManager.play_sfx("legend_drop" if is_legend else "item_drop")
	GameManager.notification_requested.emit("[%s] %s 획득" % [String(item.get("rarity_name", "")), String(item.get("name", ""))], Color.from_string(String(item.get("rarity_color", "ffffff")), Color.WHITE))


func _nearest_enemy() -> EnemyAI:
	var sorted: Array[EnemyAI] = _valid_enemies_sorted()
	return sorted[0] if not sorted.is_empty() else null


func _update_target_marker() -> void:
	var nearest: EnemyAI = _nearest_enemy()
	for enemy: EnemyAI in _enemies:
		if is_instance_valid(enemy):
			enemy.set_targeted(enemy == nearest)


func _valid_enemies_sorted() -> Array[EnemyAI]:
	var valid: Array[EnemyAI] = []
	for enemy: EnemyAI in _enemies:
		if is_instance_valid(enemy):
			valid.append(enemy)
	valid.sort_custom(func(a: EnemyAI, b: EnemyAI) -> bool: return a.global_position.distance_squared_to(player.global_position) < b.global_position.distance_squared_to(player.global_position))
	return valid


func _clear_enemies() -> void:
	for enemy: EnemyAI in _enemies:
		if is_instance_valid(enemy):
			enemy.queue_free()
	_enemies.clear()
	for projectile: Node in projectiles_root.get_children():
		projectile.queue_free()


func _configure_player_visual() -> void:
	var class_resource: ClassData = load("res://resources/classes/%s.tres" % GameManager.selected_class)
	if class_resource != null:
		player.configure(class_resource.id, class_resource.color)


func _load_enemy_resources() -> void:
	_enemy_resources.clear()
	_boss_resource = null
	for resource_path: String in ENEMY_RESOURCE_PATHS:
		var resource: Resource = load(resource_path)
		if resource is EnemyData:
			var enemy_data := resource as EnemyData
			_enemy_resources.append(enemy_data)
			if enemy_data.behavior == "boss":
				_boss_resource = enemy_data
	if _enemy_resources.size() != ENEMY_RESOURCE_PATHS.size():
		push_error("Enemy resource catalog incomplete: loaded %d/%d" % [_enemy_resources.size(), ENEMY_RESOURCE_PATHS.size()])
	if _boss_resource == null:
		push_error("Enemy resource catalog has no boss")
	_enemy_resources.sort_custom(func(a: EnemyData, b: EnemyData) -> bool: return a.unlock_floor < b.unlock_floor)


func _random_spawn_position() -> Vector2:
	for _attempt in 10:
		var candidate := Vector2(
			randf_range(_combat_rect.position.x + 24.0, _combat_rect.end.x - 24.0),
			randf_range(_combat_rect.position.y + 24.0, _combat_rect.end.y - 24.0)
		)
		if candidate.distance_to(player.global_position) >= 110.0:
			return candidate
	return _combat_rect.position + Vector2(36.0, 36.0)


func _start_shake(intensity: float, duration: float) -> void:
	_shake_intensity = maxf(_shake_intensity, intensity)
	_shake_time_left = maxf(_shake_time_left, duration)


func _update_camera_shake(delta: float) -> void:
	if _shake_time_left > 0.0:
		_shake_time_left -= delta
		camera.offset = Vector2(randf_range(-_shake_intensity, _shake_intensity), randf_range(-_shake_intensity, _shake_intensity)).round()
	else:
		camera.offset = Vector2.ZERO
		_shake_intensity = 0.0
