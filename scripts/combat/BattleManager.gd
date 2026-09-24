extends Node2D
class_name BattleManager

const BATTLE_RECT := Rect2(22, 58, 596, 132)
const PLAYER_POSITION := Vector2(320, 126)
const ENEMY_RESOURCE_DIRECTORY: String = "res://resources/enemies/"
const DUNGEON_TEXTURE: Texture2D = preload("res://assets/sprites/dungeon_courtyard.png")

@onready var player: PlayerAvatar = $Player
@onready var enemies_root: Node2D = $Enemies
@onready var projectiles_root: Node2D = $Projectiles
@onready var effects: EffectLayer = $Effects

var _enemy_resources: Array[EnemyData] = []
var _enemies: Array[EnemyAI] = []
var _attack_time_left: float = 0.0
var _skill_time_left: float = 3.0
var _respawning: bool = false
var _shake_time_left: float = 0.0
var _shake_intensity: float = 0.0
var _last_death_position: Vector2 = PLAYER_POSITION


func _ready() -> void:
	randomize()
	_load_enemy_resources()
	player.position = PLAYER_POSITION
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
	_update_target_marker()
	_attack_time_left -= delta
	_skill_time_left -= delta
	if _attack_time_left <= 0.0:
		_attack_time_left = 1.0 / maxf(0.2, GameManager.spd)
		_perform_auto_attack()
	if _skill_time_left <= 0.0:
		_skill_time_left = 3.0
		_perform_auto_skill()


func _draw() -> void:
	var theme_tint := Color(0.82, 0.74, 0.68) if GameManager.floor <= 5 else (Color(0.64, 0.67, 0.86) if GameManager.floor <= 15 else Color(0.90, 0.56, 0.59))
	draw_texture_rect(DUNGEON_TEXTURE, Rect2(0, -54, 640, 360), false, theme_tint)
	draw_rect(Rect2(0, 0, 640, 216), Color(0.025, 0.02, 0.04, 0.16), true)
	draw_rect(Rect2(0, 0, 640, 42), Color(0.02, 0.015, 0.025, 0.70), true)
	draw_line(Vector2(0, 41), Vector2(640, 41), Color("8f5a3a"), 1.0)
	draw_line(Vector2(0, 214), Vector2(640, 214), Color("9a6240"), 2.0)


func _start_battle() -> void:
	_respawning = false
	player.visible = true
	_configure_player_visual()
	_attack_time_left = 0.15
	_skill_time_left = 3.0
	AudioManager.play_bgm_for_floor(GameManager.floor)
	if _enemies.is_empty():
		_spawn_wave()


func _spawn_wave() -> void:
	if _respawning or GameManager.game_state != GameManager.GameState.RUNNING:
		return
	var eligible: Array[EnemyData] = []
	for enemy_resource: EnemyData in _enemy_resources:
		if enemy_resource.unlock_floor <= GameManager.floor:
			eligible.append(enemy_resource)
	if eligible.is_empty():
		return
	var wave_size: int = mini(3 + floori(GameManager.floor * 0.5), 12)
	for index: int in wave_size:
		var enemy := EnemyAI.new()
		enemy.name = "Enemy_%d_%d" % [GameManager.floor, index]
		enemies_root.add_child(enemy)
		enemy.global_position = _random_edge_position()
		enemy.setup(eligible.pick_random(), GameManager.floor, player, BATTLE_RECT)
		enemy.died.connect(_on_enemy_died)
		enemy.attacked_player.connect(_on_enemy_attack)
		enemy.damage_received.connect(_on_enemy_damage_received)
		_enemies.append(enemy)
	if wave_size > 0:
		GameManager.notification_requested.emit("적 증원 %d마리 접근" % wave_size, Color("e5b06a"))


func _perform_auto_attack() -> void:
	var target: EnemyAI = _nearest_enemy()
	if target == null:
		return
	var attack_power: float = GameManager.atk * (1.0 + float(GameManager.skill_levels.get("attack_boost", 0)) * 0.10)
	var result: Dictionary = DamageCalculator.calculate_damage(attack_power, target.defense, 0, GameManager.penetration, GameManager.crit, false)
	player.play_attack(target.global_position)
	effects.show_attack(player.global_position, target.global_position, bool(result.get("critical", false)))
	target.take_hit(result)
	AudioManager.play_sfx("critical_hit" if bool(result.get("critical", false)) else "basic_attack")
	if bool(result.get("critical", false)):
		_start_shake(2.5, 0.15)


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
	var attack_power: float = GameManager.atk * power_scale * (1.0 + float(GameManager.skill_levels.get("attack_boost", 0)) * 0.10)
	var result: Dictionary = DamageCalculator.calculate_damage(attack_power, enemy.defense, skill_level, GameManager.penetration, GameManager.crit, true)
	enemy.take_hit(result)
	if bool(result.get("critical", false)):
		_start_shake(3.0, 0.18)


func _on_enemy_died(enemy: EnemyAI, world_position: Vector2, fragment_color: Color, xp_reward: int, gold_reward: int) -> void:
	if not _enemies.has(enemy):
		return
	_enemies.erase(enemy)
	_last_death_position = world_position
	effects.spawn_fragments(world_position, fragment_color, randi_range(8, 12), 68.0)
	AudioManager.play_sfx("monster_death")
	GameManager.record_kill()
	LevelManager.add_xp(xp_reward)
	var adjusted_gold: int = maxi(1, roundi(gold_reward * RebirthManager.gold_multiplier() * (1.0 + GameManager.gold_bonus * 0.01)))
	GameManager.add_gold(adjusted_gold)
	effects.show_gold(world_position, adjusted_gold)
	LootManager.try_drop()
	if GameManager.kills_on_floor >= 8 + GameManager.floor:
		_clear_enemies()
		GameManager.advance_floor()
		_spawn_wave()
	elif _enemies.is_empty():
		_spawn_wave()


func _on_enemy_attack(attacker: EnemyAI, raw_damage: float) -> void:
	if _respawning:
		return
	player.play_hit()
	effects.show_enemy_attack(attacker.global_position if is_instance_valid(attacker) else player.global_position + Vector2.LEFT * 12.0, player.global_position)
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
	for file_name: String in DirAccess.get_files_at(ENEMY_RESOURCE_DIRECTORY):
		if not file_name.ends_with(".tres"):
			continue
		var resource: Resource = load(ENEMY_RESOURCE_DIRECTORY + file_name)
		if resource is EnemyData:
			_enemy_resources.append(resource as EnemyData)
	_enemy_resources.sort_custom(func(a: EnemyData, b: EnemyData) -> bool: return a.unlock_floor < b.unlock_floor)


func _random_edge_position() -> Vector2:
	var right: float = BATTLE_RECT.end.x - 1.0
	var bottom: float = BATTLE_RECT.end.y - 1.0
	match randi_range(0, 3):
		0: return Vector2(randf_range(BATTLE_RECT.position.x, right), BATTLE_RECT.position.y)
		1: return Vector2(right, randf_range(BATTLE_RECT.position.y, bottom))
		2: return Vector2(randf_range(BATTLE_RECT.position.x, right), bottom)
		_: return Vector2(BATTLE_RECT.position.x, randf_range(BATTLE_RECT.position.y, bottom))


func _start_shake(intensity: float, duration: float) -> void:
	_shake_intensity = maxf(_shake_intensity, intensity)
	_shake_time_left = maxf(_shake_time_left, duration)


func _update_camera_shake(delta: float) -> void:
	if _shake_time_left > 0.0:
		_shake_time_left -= delta
		position = Vector2(randf_range(-_shake_intensity, _shake_intensity), randf_range(-_shake_intensity, _shake_intensity)).round()
	else:
		position = Vector2.ZERO
		_shake_intensity = 0.0
