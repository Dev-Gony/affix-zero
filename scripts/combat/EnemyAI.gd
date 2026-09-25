extends Node2D
class_name EnemyAI

const ENEMY_TEXTURES := {
	"slime": preload("res://assets/cc0/pixelboy/slime.png"),
	"bat": preload("res://assets/cc0/pixelboy/bat.png"),
	"skeleton": preload("res://assets/cc0/pixelboy/skeleton.png"),
	"goblin": preload("res://assets/cc0/pixelboy/goblin.png"),
	"dark_knight": preload("res://assets/cc0/pixelboy/dark_knight.png"),
	"lich": preload("res://assets/cc0/pixelboy/lich.png"),
	"dragon": preload("res://assets/cc0/pixelboy/dragon.png"),
	"demon_lord": preload("res://assets/cc0/pixelboy/demon_lord.png"),
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
var behavior: String = "chaser"
var attack_range: float = 0.0
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


static func floor_scaling(current_floor: int, is_boss: bool = false) -> Dictionary:
	var depth: float = maxf(0.0, current_floor - 1)
	var hp_scale: float = 1.0 + depth * 0.22 + depth * depth * 0.025
	var attack_scale: float = 1.0 + depth * 0.16 + depth * depth * 0.018
	var defense_scale: float = 1.0 + depth * 0.12 + depth * depth * 0.010
	if is_boss:
		hp_scale *= 2.4
		attack_scale *= 1.22
		defense_scale *= 1.25
	var late_depth: float = maxf(0.0, depth - 9.0)
	return {
		"hp": hp_scale,
		"attack": attack_scale,
		"defense": defense_scale,
		# Flat pressure starts after floor 10 so the first run remains readable,
		# while persistent rebirth gear cannot trivialize later floors forever.
		"flat_attack": late_depth * 15.0,
		"flat_defense": late_depth * 2.0,
	}


func setup(data: EnemyData, current_floor: int, player_target: Node2D, arena_bounds: Rect2 = Rect2()) -> void:
	enemy_data = data
	target = player_target
	movement_bounds = arena_bounds
	var scaling: Dictionary = floor_scaling(current_floor, data.behavior == "boss")
	max_hp = data.base_hp * float(scaling.get("hp", 1.0))
	hp = max_hp
	attack = data.base_atk * float(scaling.get("attack", 1.0)) + float(scaling.get("flat_attack", 0.0))
	defense = data.base_def * float(scaling.get("defense", 1.0)) + float(scaling.get("flat_defense", 0.0))
	var depth: float = maxf(0.0, current_floor - 1)
	move_speed = data.move_speed * minf(1.65, 1.0 + depth * 0.010)
	attack_cooldown = maxf(0.45, data.attack_cooldown * maxf(0.55, 1.0 - depth * 0.008))
	behavior = data.behavior
	var base_attack_range: float = data.attack_range if data.attack_range > 0.0 else data.radius + 11.0
	attack_range = base_attack_range + minf(24.0, depth * 0.35)
	_attack_windup_duration = data.attack_windup * maxf(0.50, 1.0 - depth * 0.006)
	radius = data.radius
	xp_reward = maxi(1, roundi(data.xp_reward * (1.0 + (current_floor - 1) * 0.12)))
	gold_reward = maxi(1, roundi(data.gold_reward * (1.0 + (current_floor - 1) * 0.08)))
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
	var effective_attack_range: float = attack_range if attack_range > 0.0 else radius + 11.0
	var toward_target: Vector2 = global_position.direction_to(target.global_position)
	var move_direction := Vector2.ZERO
	if distance > effective_attack_range:
		move_direction = toward_target
	elif behavior == "caster":
		if distance < effective_attack_range * 0.62:
			move_direction = -toward_target
		else:
			move_direction = Vector2(-toward_target.y, toward_target.x) * (1.0 if get_instance_id() % 2 == 0 else -1.0)
	elif behavior == "skirmisher" and distance < effective_attack_range * 0.68:
		move_direction = -toward_target
	if not move_direction.is_zero_approx():
		_move_with_behavior(move_direction, delta)
	if distance <= effective_attack_range and _attack_time_left <= 0.0:
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


func _move_with_behavior(direction: Vector2, delta: float) -> void:
	var adjusted_direction: Vector2 = direction.normalized()
	var speed_multiplier: float = 1.0
	match behavior:
		"hopper":
			speed_multiplier = 0.30 + maxf(0.0, sin(_motion_clock * 8.0)) * 1.35
		"zigzag":
			var tangent := Vector2(-adjusted_direction.y, adjusted_direction.x)
			adjusted_direction = (adjusted_direction + tangent * sin(_motion_clock * 9.0) * 0.72).normalized()
			speed_multiplier = 1.08
		"skirmisher":
			speed_multiplier = 1.12
		"brute":
			speed_multiplier = 0.82
		"caster":
			speed_multiplier = 0.78
		"charger":
			speed_multiplier = 1.90 if fmod(_motion_clock, 2.4) < 0.42 else 0.58
		"boss":
			speed_multiplier = 0.72 + sin(_motion_clock * 2.2) * 0.10
	global_position += adjusted_direction * move_speed * speed_multiplier * delta
	_clamp_to_movement_bounds()


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
	var texture: Texture2D = ENEMY_TEXTURES.get(enemy_id, ENEMY_TEXTURES["slime"])
	var reveal: float = clampf(1.0 - _spawn_reveal_left / 0.32, 0.0, 1.0)
	var death_alpha: float = clampf(_death_time_left / _death_duration, 0.0, 1.0) if _dead else 1.0
	var alpha: float = reveal * death_alpha
	var distance_to_target: float = global_position.distance_to(target.global_position) if target != null and is_instance_valid(target) else 0.0
	var moving: bool = not _dead and _spawn_reveal_left <= 0.0 and distance_to_target > radius + 11.0
	var step_wave: float = sin(_motion_clock * (10.0 if enemy_id == "bat" else 7.0)) if moving else sin(_motion_clock * 3.0) * 0.25
	var bob: float = roundf(step_wave * (2.0 if enemy_id == "bat" else 0.8))
	var size_map := {
		"slime": 22.0, "bat": 24.0, "skeleton": 26.0, "goblin": 25.0,
		"dark_knight": 34.0, "lich": 32.0, "dragon": 44.0, "demon_lord": 48.0,
	}
	var sprite_size: float = float(size_map.get(enemy_id, 26.0))
	var sprite_modulate := Color(1.45, 1.45, 1.45, alpha) if _hit_flash_left > 0.0 else Color(1.0, 1.0, 1.0, alpha)
	draw_circle(Vector2(0, sprite_size * 0.28), sprite_size * 0.25, Color(0, 0, 0, 0.34 * alpha))
	if _spawn_reveal_left > 0.0:
		draw_arc(Vector2.ZERO, 9.0 + reveal * 8.0, 0.0, TAU, 20, Color(body_color, 0.75 * (1.0 - reveal)), 2.0)
	if _is_targeted and not _dead:
		var target_pulse: float = 0.55 + sin(_motion_clock * 8.0) * 0.12
		draw_arc(Vector2.ZERO, sprite_size * 0.48, 0.20, PI - 0.20, 18, Color("f6c85f", target_pulse), 1.5)
	if _attack_windup_left > 0.0:
		var warning_progress: float = 1.0 - _attack_windup_left / _attack_windup_duration
		draw_arc(Vector2.ZERO, sprite_size * 0.52, -PI * 0.5, -PI * 0.5 + TAU * warning_progress, 24, Color("ff4d5a", 0.90), 2.5)
	var death_scale_y: float = maxf(0.15, death_alpha) if _dead else 1.0
	draw_set_transform(Vector2(0, bob), 0.0, Vector2(1.0, death_scale_y))
	draw_texture_rect(texture, Rect2(-sprite_size * 0.5, -sprite_size * 0.62, sprite_size, sprite_size), false, sprite_modulate)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
	var bar_width: float = maxf(20.0, sprite_size * 0.72)
	var bar_y: float = -sprite_size * 0.58 - 5.0
	if hp < max_hp or _is_targeted:
		draw_rect(Rect2(-bar_width * 0.5, bar_y, bar_width, 3), Color("351822", alpha), true)
		draw_rect(Rect2(-bar_width * 0.5, bar_y, bar_width * clampf(hp / max_hp, 0.0, 1.0), 3), Color("ef4444", alpha), true)
