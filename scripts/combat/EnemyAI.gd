extends Node2D
class_name EnemyAI

const ENEMY_ATLAS: Texture2D = preload("res://assets/sprites/enemy_atlas_alpha.png")
const ENEMY_REGIONS: Dictionary = {
	"slime": Vector2i(0, 0), "bat": Vector2i(1, 0), "skeleton": Vector2i(2, 0), "goblin": Vector2i(3, 0),
	"dark_knight": Vector2i(0, 1), "lich": Vector2i(1, 1), "dragon": Vector2i(2, 1), "demon_lord": Vector2i(3, 1),
}

signal died(enemy: EnemyAI, world_position: Vector2, fragment_color: Color, xp_reward: int, gold_reward: int)
signal attacked_player(enemy: EnemyAI, raw_damage: float)
signal damage_received(world_position: Vector2, damage: int, critical: bool)

var enemy_data: EnemyData
var target: Node2D
var max_hp: float = 1.0
var hp: float = 1.0
var attack: float = 1.0
var defense: float = 0.0
var move_speed: float = 20.0
var attack_cooldown: float = 1.0
var radius: float = 8.0
var xp_reward: int = 1
var gold_reward: int = 1
var body_color: Color = Color.WHITE
var movement_bounds: Rect2 = Rect2()
var _attack_time_left: float = 0.0
var _dead: bool = false
var _hit_flash_left: float = 0.0
var _spawn_reveal_left: float = 0.32
var _motion_clock: float = 0.0
var _attack_windup_left: float = 0.0
var _attack_windup_duration: float = 0.22
var _knockback_velocity: Vector2 = Vector2.ZERO
var _death_time_left: float = 0.0
var _death_duration: float = 0.28
var _is_targeted: bool = false


func setup(data: EnemyData, current_floor: int, player_target: Node2D, arena_bounds: Rect2 = Rect2()) -> void:
	enemy_data = data
	target = player_target
	movement_bounds = arena_bounds
	var scale_factor: float = 1.0 + (current_floor - 1) * 0.3
	max_hp = data.base_hp * scale_factor
	hp = max_hp
	attack = data.base_atk * scale_factor
	defense = data.base_def * scale_factor
	move_speed = data.move_speed
	attack_cooldown = data.attack_cooldown
	radius = data.radius
	xp_reward = maxi(1, roundi(data.xp_reward * (1.0 + (current_floor - 1) * 0.12)))
	gold_reward = maxi(1, roundi(data.gold_reward * (1.0 + (current_floor - 1) * 0.10)))
	body_color = data.color
	queue_redraw()


func _process(delta: float) -> void:
	_motion_clock += delta
	if _dead:
		_death_time_left = maxf(0.0, _death_time_left - delta)
		queue_redraw()
		if is_zero_approx(_death_time_left):
			queue_free()
		return
	if target == null or not is_instance_valid(target):
		return
	if _hit_flash_left > 0.0:
		_hit_flash_left = maxf(0.0, _hit_flash_left - delta)
		queue_redraw()
	_attack_time_left = maxf(0.0, _attack_time_left - delta)
	if not _knockback_velocity.is_zero_approx():
		global_position += _knockback_velocity * delta
		_clamp_to_movement_bounds()
		_knockback_velocity = _knockback_velocity.lerp(Vector2.ZERO, minf(1.0, delta * 18.0))
	if _spawn_reveal_left > 0.0:
		_spawn_reveal_left = maxf(0.0, _spawn_reveal_left - delta)
		queue_redraw()
		return
	if _attack_windup_left > 0.0:
		_attack_windup_left = maxf(0.0, _attack_windup_left - delta)
		queue_redraw()
		if is_zero_approx(_attack_windup_left):
			_attack_time_left = attack_cooldown
			attacked_player.emit(self, attack)
		return
	var distance: float = global_position.distance_to(target.global_position)
	if distance > radius + 11.0:
		global_position += global_position.direction_to(target.global_position) * move_speed * delta
		_clamp_to_movement_bounds()
	elif _attack_time_left <= 0.0:
		_attack_windup_left = _attack_windup_duration
		queue_redraw()


func set_targeted(value: bool) -> void:
	if _is_targeted == value:
		return
	_is_targeted = value
	queue_redraw()


func _clamp_to_movement_bounds() -> void:
	if movement_bounds.size.is_zero_approx():
		return
	global_position = Vector2(
		clampf(global_position.x, movement_bounds.position.x, movement_bounds.end.x - 1.0),
		clampf(global_position.y, movement_bounds.position.y, movement_bounds.end.y - 1.0)
	)


func take_hit(result: Dictionary) -> void:
	if _dead:
		return
	var damage: int = int(result.get("damage", 1))
	var critical: bool = bool(result.get("critical", false))
	hp -= damage
	_hit_flash_left = 0.09
	if target != null and is_instance_valid(target):
		_knockback_velocity = target.global_position.direction_to(global_position) * 48.0
	damage_received.emit(global_position, damage, critical)
	queue_redraw()
	if hp <= 0.0:
		_die()


func _die() -> void:
	if _dead:
		return
	_dead = true
	_death_time_left = _death_duration
	_attack_windup_left = 0.0
	died.emit(self, global_position, body_color, xp_reward, gold_reward)
	queue_redraw()


func _draw() -> void:
	var enemy_id: String = enemy_data.id if enemy_data != null else "slime"
	var atlas_cell: Vector2i = ENEMY_REGIONS.get(enemy_id, Vector2i.ZERO)
	var source := Rect2(atlas_cell.x * 384, atlas_cell.y * 512, 384, 512)
	var sprite_size: float = clampf(radius * 3.7, 28.0, 54.0)
	var reveal: float = clampf(1.0 - _spawn_reveal_left / 0.32, 0.0, 1.0)
	var death_alpha: float = clampf(_death_time_left / _death_duration, 0.0, 1.0) if _dead else 1.0
	var alpha: float = reveal * death_alpha
	var sprite_modulate := Color(1.65, 1.65, 1.65, reveal) if _hit_flash_left > 0.0 else Color(1.0, 1.0, 1.0, reveal)
	sprite_modulate.a = alpha
	var distance_to_target: float = global_position.distance_to(target.global_position) if target != null and is_instance_valid(target) else 0.0
	var moving: bool = not _dead and _spawn_reveal_left <= 0.0 and distance_to_target > radius + 11.0
	var step_wave: float = sin(_motion_clock * (10.0 if enemy_id == "bat" else 7.0)) if moving else sin(_motion_clock * 3.0) * 0.25
	var bob: float = roundf(step_wave * (2.0 if enemy_id == "bat" else 1.0))
	var squash := Vector2(1.0, 1.0)
	if enemy_id == "slime":
		squash = Vector2(1.0 + step_wave * 0.06, 1.0 - step_wave * 0.06)
	if _dead:
		squash = Vector2(1.0 + (1.0 - death_alpha) * 0.45, maxf(0.12, death_alpha))
	draw_circle(Vector2(0, sprite_size * 0.28), sprite_size * 0.28, Color(0, 0, 0, 0.35 * alpha))
	if _spawn_reveal_left > 0.0:
		draw_arc(Vector2.ZERO, 10.0 + reveal * 9.0, 0.0, TAU, 24, Color(body_color, 0.75 * (1.0 - reveal)), 2.0)
	if _is_targeted and not _dead:
		var target_pulse: float = 0.55 + sin(_motion_clock * 8.0) * 0.12
		draw_arc(Vector2.ZERO, sprite_size * 0.43, 0.20, PI - 0.20, 18, Color("f6c85f", target_pulse), 1.5)
		draw_line(Vector2(-sprite_size * 0.30, -sprite_size * 0.35), Vector2(-sprite_size * 0.18, -sprite_size * 0.25), Color("f6c85f", target_pulse), 2.0)
		draw_line(Vector2(sprite_size * 0.30, -sprite_size * 0.35), Vector2(sprite_size * 0.18, -sprite_size * 0.25), Color("f6c85f", target_pulse), 2.0)
	if _attack_windup_left > 0.0:
		var warning_progress: float = 1.0 - _attack_windup_left / _attack_windup_duration
		draw_arc(Vector2.ZERO, sprite_size * 0.46, -PI * 0.5, -PI * 0.5 + TAU * warning_progress, 24, Color("ff4d5a", 0.90), 2.5)
	draw_set_transform(Vector2(0, bob), 0.0, squash)
	draw_texture_rect_region(ENEMY_ATLAS, Rect2(-sprite_size * 0.5, -sprite_size * 0.58, sprite_size, sprite_size), source, sprite_modulate)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
	var bar_width: float = maxf(20.0, sprite_size * 0.7)
	var bar_y: float = -sprite_size * 0.52 - 5.0
	if hp < max_hp or _is_targeted:
		draw_rect(Rect2(-bar_width * 0.5, bar_y, bar_width, 3), Color("351822", alpha), true)
		draw_rect(Rect2(-bar_width * 0.5, bar_y, bar_width * clampf(hp / max_hp, 0.0, 1.0), 3), Color("ef4444", alpha), true)
