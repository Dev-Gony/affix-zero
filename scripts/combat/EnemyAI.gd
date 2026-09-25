extends Node2D
class_name EnemyAI

const ENEMY_ATLAS: Texture2D = preload("res://assets/sprites/enemy_atlas_alpha.png")
const ENEMY_REGIONS: Dictionary = {
	"slime": Vector2i(0, 0),
	"bat": Vector2i(1, 0),
	"skeleton": Vector2i(2, 0),
	"goblin": Vector2i(3, 0),
	"dark_knight": Vector2i(0, 1),
	"lich": Vector2i(1, 1),
	"dragon": Vector2i(2, 1),
	"demon_lord": Vector2i(3, 1),
}

const ELITE_AFFIXES: Dictionary = {
	"brutal": {
		"display_name": "폭군",
		"hp": 1.65,
		"attack": 1.45,
		"defense": 1.10,
		"speed": 0.92,
		"cooldown": 1.00,
		"reward": 2.60,
		"color": "ff6b6b",
	},
	"swift": {
		"display_name": "질풍",
		"hp": 1.35,
		"attack": 1.25,
		"defense": 1.00,
		"speed": 1.32,
		"cooldown": 0.76,
		"reward": 2.40,
		"color": "67e8f9",
	},
	"bulwark": {
		"display_name": "철벽",
		"hp": 2.10,
		"attack": 1.12,
		"defense": 1.80,
		"speed": 0.84,
		"cooldown": 1.08,
		"reward": 2.80,
		"color": "f6c85f",
	},
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
var is_elite: bool = false
var elite_affix_id: String = ""
var elite_display_name: String = ""
var elite_color: Color = Color.WHITE
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
var _attack_recovery_left: float = 0.0
var _last_move_direction: Vector2 = Vector2.LEFT
var _stride_phase: float = 0.0
var _attack_pose: float = 0.0


static func floor_scaling(current_floor: int, is_boss: bool = false) -> Dictionary:
	var depth: float = maxf(0.0, current_floor - 1)
	var hp_scale: float = 1.0 + depth * 0.22 + depth * depth * 0.025
	var attack_scale: float = 1.0 + depth * 0.16 + depth * depth * 0.018
	var defense_scale: float = 1.0 + depth * 0.12 + depth * depth * 0.010
	if is_boss:
		hp_scale *= 3.4
		attack_scale *= 1.70
		defense_scale *= 1.35
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


func setup(data: EnemyData, current_floor: int, player_target: Node2D, arena_bounds: Rect2 = Rect2(), elite_affix: String = "") -> void:
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
	if data.behavior != "boss" and ELITE_AFFIXES.has(elite_affix):
		_apply_elite_affix(elite_affix)
	queue_redraw()


func _apply_elite_affix(affix_id: String) -> void:
	var affix: Dictionary = Dictionary(ELITE_AFFIXES.get(affix_id, {}))
	if affix.is_empty():
		return
	is_elite = true
	elite_affix_id = affix_id
	elite_display_name = String(affix.get("display_name", "엘리트"))
	elite_color = Color.from_string(String(affix.get("color", "ffffff")), Color.WHITE)
	max_hp *= float(affix.get("hp", 1.0))
	hp = max_hp
	attack *= float(affix.get("attack", 1.0))
	defense *= float(affix.get("defense", 1.0))
	move_speed *= float(affix.get("speed", 1.0))
	attack_cooldown = maxf(0.32, attack_cooldown * float(affix.get("cooldown", 1.0)))
	var reward_multiplier: float = float(affix.get("reward", 2.0))
	xp_reward = maxi(1, roundi(xp_reward * reward_multiplier))
	gold_reward = maxi(1, roundi(gold_reward * reward_multiplier))
	radius *= 1.12
	body_color = body_color.lerp(elite_color, 0.45)


func elite_title() -> String:
	if not is_elite:
		return enemy_data.display_name if enemy_data != null else ""
	return "%s %s" % [elite_display_name, enemy_data.display_name if enemy_data != null else "몬스터"]


func _process(delta: float) -> void:
	_motion_clock += delta
	_stride_phase = fmod(_stride_phase + delta * maxf(2.0, move_speed * 0.12), TAU)
	if _attack_windup_left > 0.0:
		_attack_pose = clampf(1.0 - _attack_windup_left / maxf(0.001, _attack_windup_duration), 0.0, 1.0)
	elif _attack_recovery_left > 0.0:
		_attack_pose = clampf(_attack_recovery_left / 0.18, 0.0, 1.0)
	else:
		_attack_pose = move_toward(_attack_pose, 0.0, delta * 6.0)
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
	_attack_recovery_left = maxf(0.0, _attack_recovery_left - delta)
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
		_last_move_direction = move_direction.normalized()
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
	var atlas_cell: Vector2i = ENEMY_REGIONS.get(enemy_id, Vector2i.ZERO)
	var atlas_cell_size := Vector2(float(ENEMY_ATLAS.get_width()) / 4.0, float(ENEMY_ATLAS.get_height()) / 2.0)
	var atlas_source := Rect2(Vector2(atlas_cell) * atlas_cell_size, atlas_cell_size)
	var reveal: float = clampf(1.0 - _spawn_reveal_left / 0.32, 0.0, 1.0)
	var death_alpha: float = clampf(_death_time_left / _death_duration, 0.0, 1.0) if _dead else 1.0
	var alpha: float = reveal * death_alpha
	var distance_to_target: float = global_position.distance_to(target.global_position) if target != null and is_instance_valid(target) else 0.0
	var moving: bool = not _dead and _spawn_reveal_left <= 0.0 and distance_to_target > radius + 11.0
	var step_speed: float = 12.0 if enemy_id in ["bat", "dragon"] else (8.5 if enemy_id in ["goblin", "skeleton"] else 6.5)
	var step_wave: float = sin(_stride_phase * step_speed * 0.15) if moving else sin(_motion_clock * 3.0) * 0.25
	var bob_amount: float = 3.0 if enemy_id in ["bat", "dragon"] else (1.4 if enemy_id == "slime" else 1.0)
	var bob: float = step_wave * bob_amount
	var size_map := {
		"slime": 22.0, "bat": 24.0, "skeleton": 26.0, "goblin": 25.0,
		"dark_knight": 34.0, "lich": 32.0, "dragon": 44.0, "demon_lord": 48.0,
	}
	var sprite_size: float = float(size_map.get(enemy_id, 26.0)) * (1.16 if is_elite else 1.0)
	var sprite_modulate := Color(1.45, 1.45, 1.45, alpha) if _hit_flash_left > 0.0 else Color(1.0, 1.0, 1.0, alpha)
	draw_circle(Vector2(0, sprite_size * 0.28), sprite_size * 0.25, Color(0, 0, 0, 0.34 * alpha))
	if is_elite and not _dead:
		var elite_pulse: float = 0.62 + sin(_motion_clock * 5.0) * 0.16
		draw_arc(Vector2.ZERO, sprite_size * 0.58, 0.0, TAU, 28, Color(elite_color, elite_pulse * alpha), 2.4)
		draw_arc(Vector2.ZERO, sprite_size * 0.67, 0.0, TAU, 28, Color(elite_color, elite_pulse * 0.42 * alpha), 1.2)
	if _spawn_reveal_left > 0.0:
		draw_arc(Vector2.ZERO, 9.0 + reveal * 8.0, 0.0, TAU, 20, Color(body_color, 0.75 * (1.0 - reveal)), 2.0)
	if _is_targeted and not _dead:
		var target_pulse: float = 0.55 + sin(_motion_clock * 8.0) * 0.12
		draw_arc(Vector2.ZERO, sprite_size * 0.48, 0.20, PI - 0.20, 18, Color("f6c85f", target_pulse), 1.5)
	if _attack_windup_left > 0.0:
		var warning_progress: float = 1.0 - _attack_windup_left / _attack_windup_duration
		draw_arc(Vector2.ZERO, sprite_size * 0.52, -PI * 0.5, -PI * 0.5 + TAU * warning_progress, 24, Color("ff4d5a", 0.90), 2.5)
	var death_scale_y: float = maxf(0.15, death_alpha) if _dead else 1.0
	var facing_scale: float = -1.0 if _last_move_direction.x < -0.08 else 1.0
	var walk_squash: float = 1.0
	if moving:
		match enemy_id:
			"slime":
				walk_squash = 1.0 + step_wave * 0.10
			"bat", "dragon":
				walk_squash = 1.0 + absf(step_wave) * 0.025
			_:
				walk_squash = 1.0 + absf(step_wave) * 0.045
	var attack_progress: float = _attack_pose
	var target_dir: Vector2 = global_position.direction_to(target.global_position) if target != null and is_instance_valid(target) else _last_move_direction
	var lunge_strength: float = 8.0
	if enemy_id in ["goblin", "skeleton"]:
		lunge_strength = 12.0
	elif enemy_id in ["dark_knight", "dragon", "demon_lord"]:
		lunge_strength = 16.0
	var lunge: Vector2 = target_dir * sin(attack_progress * PI) * lunge_strength
	var attack_rotation: float = 0.0
	if enemy_id in ["skeleton", "goblin", "dark_knight"]:
		attack_rotation = sin(attack_progress * PI) * 0.16 * (-1.0 if facing_scale < 0.0 else 1.0)
	elif enemy_id == "bat":
		attack_rotation = sin(attack_progress * PI) * 0.26
	elif behavior == "boss":
		attack_rotation = sin(attack_progress * PI) * 0.12
	var scale_y: float = death_scale_y * walk_squash
	var scale_x_abs: float = 2.0 - walk_squash
	if enemy_id == "slime" and moving:
		scale_x_abs += absf(step_wave) * 0.08
	var scale_x: float = facing_scale * scale_x_abs
	draw_set_transform(Vector2(0, bob) + lunge, attack_rotation, Vector2(scale_x, scale_y))
	draw_texture_rect_region(
		ENEMY_ATLAS,
		Rect2(-sprite_size * 0.58, -sprite_size * 0.72, sprite_size * 1.16, sprite_size * 1.16),
		atlas_source,
		sprite_modulate
	)
	_draw_enemy_animation_overlay(enemy_id, sprite_size, alpha, moving, attack_progress)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
	if (_attack_windup_left > 0.0 or _attack_recovery_left > 0.0) and not _dead:
		_draw_attack_motion(sprite_size, target_dir, alpha)
	var bar_width: float = maxf(20.0, sprite_size * 0.72)
	var bar_y: float = -sprite_size * 0.58 - 5.0
	if hp < max_hp or _is_targeted or is_elite or behavior == "boss":
		draw_rect(Rect2(-bar_width * 0.5, bar_y, bar_width, 3), Color("351822", alpha), true)
		var hp_color: Color = elite_color if is_elite else (Color("ff5b67") if behavior == "boss" else Color("ef4444"))
		draw_rect(Rect2(-bar_width * 0.5, bar_y, bar_width * clampf(hp / max_hp, 0.0, 1.0), 3), Color(hp_color, alpha), true)
	if is_elite and not _dead:
		var elite_label: String = elite_title()
		var label_width: float = ThemeDB.fallback_font.get_string_size(elite_label, HORIZONTAL_ALIGNMENT_LEFT, -1, 7).x
		draw_string(
			ThemeDB.fallback_font,
			Vector2(-label_width * 0.5, bar_y - 4),
			elite_label,
			HORIZONTAL_ALIGNMENT_LEFT,
			-1,
			7,
			Color(elite_color, alpha)
		)
	if behavior == "boss" and not _dead:
		var boss_label: String = enemy_data.display_name if enemy_data != null else "보스"
		var boss_width: float = ThemeDB.fallback_font.get_string_size(boss_label, HORIZONTAL_ALIGNMENT_LEFT, -1, 8).x
		draw_string(
			ThemeDB.fallback_font,
			Vector2(-boss_width * 0.5, bar_y - 5),
			boss_label,
			HORIZONTAL_ALIGNMENT_LEFT,
			-1,
			8,
			Color("ff9aa4", alpha)
		)



func _draw_enemy_animation_overlay(enemy_id: String, sprite_size: float, alpha: float, moving: bool, attack_progress: float) -> void:
	var wing_wave: float = sin(_motion_clock * 12.0)
	var step_wave: float = sin(_stride_phase * 1.2)
	match enemy_id:
		"slime":
			var squash: float = 1.0 + sin(_motion_clock * 7.0) * 0.12
			draw_arc(Vector2(0, 2), sprite_size * 0.26 * squash, 0.0, PI, 14, Color("c5f7ff", 0.22 * alpha), 1.2)
			if attack_progress > 0.0:
				draw_circle(Vector2(0, -1), 5.0 + attack_progress * 4.0, Color("9bf6ff", 0.12 * alpha))
		"bat":
			var wing_span: float = sprite_size * (0.42 + wing_wave * 0.08)
			draw_line(Vector2(-3, -2), Vector2(-wing_span, -7 - wing_wave * 3.0), Color(body_color.lightened(0.18), 0.65 * alpha), 2.2)
			draw_line(Vector2(3, -2), Vector2(wing_span, -7 - wing_wave * 3.0), Color(body_color.lightened(0.18), 0.65 * alpha), 2.2)
		"skeleton":
			if moving:
				draw_line(Vector2(-5, 8), Vector2(-8 + step_wave * 3.0, 14), Color("e8e0d2", 0.45 * alpha), 1.5)
				draw_line(Vector2(5, 8), Vector2(8 - step_wave * 3.0, 14), Color("e8e0d2", 0.45 * alpha), 1.5)
			if attack_progress > 0.0:
				var a: float = lerpf(-1.15, 0.75, attack_progress)
				var tip := Vector2.RIGHT.rotated(a) * (sprite_size * 0.58)
				draw_line(Vector2(2, -1), tip, Color("dfe7ef", 0.9 * alpha), 2.0)
				draw_circle(tip, 1.8, Color("ffffff", 0.8 * alpha))
		"goblin":
			if moving:
				draw_line(Vector2(-5, 8), Vector2(-8 + step_wave * 3.5, 13), Color("8bd66f", 0.40 * alpha), 1.8)
				draw_line(Vector2(5, 8), Vector2(8 - step_wave * 3.5, 13), Color("8bd66f", 0.40 * alpha), 1.8)
			if attack_progress > 0.0:
				var spear_dir := Vector2.RIGHT.rotated(lerpf(-0.45, 0.15, attack_progress))
				draw_line(Vector2(4, 1), spear_dir * (sprite_size * 0.70), Color("cf9f60", 0.85 * alpha), 2.0)
		"dark_knight":
			if attack_progress > 0.0:
				var cleave_angle: float = lerpf(-1.35, 0.95, attack_progress)
				draw_arc(Vector2.ZERO, sprite_size * 0.66, cleave_angle - 0.18, cleave_angle + 0.18, 8, Color("ff8e6b", 0.75 * alpha), 3.0)
			draw_circle(Vector2(5, -5), 1.5, Color("ff4d5a", (0.45 + sin(_motion_clock * 6.0) * 0.25) * alpha))
		"lich":
			var orb := Vector2(8, -10) + Vector2(cos(_motion_clock * 3.4), sin(_motion_clock * 3.4)) * 3.0
			draw_circle(orb, 3.5, Color("b685ff", 0.20 * alpha))
			draw_circle(orb, 1.7, Color("e6d3ff", 0.85 * alpha))
			if attack_progress > 0.0:
				draw_arc(Vector2.ZERO, sprite_size * (0.44 + attack_progress * 0.20), 0.0, TAU, 18, Color("9f7aea", 0.28 * alpha), 1.5)
		"dragon":
			var wing_y: float = wing_wave * 5.0
			draw_colored_polygon(PackedVector2Array([
				Vector2(-6, -6), Vector2(-sprite_size * 0.62, -13 + wing_y), Vector2(-sprite_size * 0.48, 4)
			]), Color("ff6a45", 0.36 * alpha))
			draw_colored_polygon(PackedVector2Array([
				Vector2(6, -6), Vector2(sprite_size * 0.62, -13 + wing_y), Vector2(sprite_size * 0.48, 4)
			]), Color("ff6a45", 0.36 * alpha))
			if attack_progress > 0.0:
				draw_circle(Vector2(sprite_size * 0.32, -4), 3.0 + attack_progress * 4.0, Color("ff9a3c", 0.32 * alpha))
		"demon_lord":
			var pulse: float = 0.65 + sin(_motion_clock * 5.0) * 0.18
			draw_arc(Vector2.ZERO, sprite_size * 0.58, _motion_clock * 0.8, _motion_clock * 0.8 + PI * 1.45, 20, Color("ff315f", pulse * 0.35 * alpha), 2.0)
			draw_circle(Vector2(-7, -9), 2.0, Color("ff4d5a", pulse * alpha))
			draw_circle(Vector2(7, -9), 2.0, Color("ff4d5a", pulse * alpha))


func _draw_attack_motion(sprite_size: float, target_dir: Vector2, alpha: float) -> void:
	if target_dir.is_zero_approx():
		target_dir = Vector2.RIGHT
	var tangent := Vector2(-target_dir.y, target_dir.x)
	var progress: float = 1.0 - (_attack_windup_left / maxf(0.001, _attack_windup_duration)) if _attack_windup_left > 0.0 else (1.0 - _attack_recovery_left / 0.18)
	match behavior:
		"caster":
			var orb_pos: Vector2 = target_dir * (sprite_size * 0.52)
			var orb_radius: float = 2.0 + progress * 4.5
			draw_circle(orb_pos, orb_radius + 4.0, Color("9f7aea", 0.12 * alpha))
			draw_circle(orb_pos, orb_radius, Color("c4a7ff", 0.85 * alpha))
			draw_line(orb_pos - tangent * 4.0, orb_pos + tangent * 4.0, Color("f1e8ff", 0.55 * alpha), 1.2)
		"boss":
			var arc_center: float = target_dir.angle()
			draw_arc(Vector2.ZERO, sprite_size * 0.72, arc_center - 0.65, arc_center + 0.65, 14, Color("ff645e", 0.78 * alpha), 3.2)
			draw_arc(Vector2.ZERO, sprite_size * 0.90, arc_center - 0.46, arc_center + 0.46, 12, Color("ffb078", 0.32 * alpha), 1.4)
		_:
			var start: Vector2 = target_dir * (sprite_size * 0.18) - tangent * 5.0
			var finish: Vector2 = target_dir * (sprite_size * 0.68) + tangent * 7.0
			draw_line(start, finish, Color("ffcf9e", 0.80 * alpha), 2.0)
			draw_circle(finish, 2.2, Color("fff1db", 0.78 * alpha))
