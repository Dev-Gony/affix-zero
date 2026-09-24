extends Node2D
class_name PlayerAvatar

var class_id: String = "warrior"
var body_color: Color = Color("dc3d33")
var pulse: float = 0.0
var _motion_kind: String = "idle"
var _motion_time_left: float = 0.0
var _motion_duration: float = 0.0
var _facing: Vector2 = Vector2.RIGHT
var _visual_facing: Vector2 = Vector2.RIGHT
var _skill_color: Color = Color("ffd166")
var _move_direction: Vector2 = Vector2.ZERO


func configure(next_class_id: String, color: Color) -> void:
	class_id = next_class_id
	body_color = color
	visible = true
	queue_redraw()


func _process(delta: float) -> void:
	pulse += delta
	if not _move_direction.is_zero_approx() and _motion_kind == "idle":
		_facing = _move_direction
	var facing_blend: float = 1.0 - exp(-delta * 20.0)
	var blended_facing: Vector2 = _visual_facing.lerp(_facing, facing_blend)
	if not blended_facing.is_zero_approx():
		_visual_facing = blended_facing.normalized()
	if _motion_time_left > 0.0:
		_motion_time_left = maxf(0.0, _motion_time_left - delta)
		if is_zero_approx(_motion_time_left):
			_motion_kind = "idle"
	queue_redraw()


func set_move_direction(direction: Vector2) -> void:
	_move_direction = direction.normalized() if not direction.is_zero_approx() else Vector2.ZERO
	if not _move_direction.is_zero_approx():
		_facing = _move_direction
	queue_redraw()


func play_attack(world_target: Vector2) -> void:
	_face_target(world_target)
	_start_motion("attack", 0.20)


func play_skill(skill_type: String, world_target: Vector2) -> void:
	_face_target(world_target)
	_skill_color = _skill_visual_color(skill_type)
	_start_motion("skill", 0.30)


func play_hit() -> void:
	_start_motion("hit", 0.14)


func _face_target(world_target: Vector2) -> void:
	var direction: Vector2 = global_position.direction_to(world_target)
	if not direction.is_zero_approx():
		_facing = direction


func _start_motion(kind: String, duration: float) -> void:
	_motion_kind = kind
	_motion_duration = duration
	_motion_time_left = duration
	queue_redraw()


func _draw() -> void:
	var moving: bool = not _move_direction.is_zero_approx() and _motion_kind == "idle"
	var step_wave: float = sin(pulse * 13.0) if moving else 0.0
	var bob: float = roundf(step_wave * 1.1)
	draw_ellipse(Vector2(0, 9), Vector2(8.5, 3.0), Color(0, 0, 0, 0.48))

	var motion_progress: float = 1.0
	if _motion_duration > 0.0 and _motion_time_left > 0.0:
		motion_progress = 1.0 - (_motion_time_left / _motion_duration)
	var motion_offset := Vector2.ZERO
	var motion_rotation: float = 0.0
	var motion_scale := Vector2.ONE
	match _motion_kind:
		"attack":
			var strike: float = sin(smoothstep(0.0, 1.0, motion_progress) * PI)
			motion_offset = _visual_facing * strike * 6.0
			motion_rotation = lerpf(-0.10, 0.10, motion_progress)
			_draw_attack_swing(motion_progress, strike)
		"skill":
			var charge: float = sin(motion_progress * PI)
			motion_scale = Vector2(1.0 + charge * 0.08, 1.0 - charge * 0.05)
			draw_arc(Vector2.ZERO, 14.0 + charge * 4.0, -PI * 0.2, PI * 1.2, 20, Color(_skill_color, 0.72 * charge), 1.6)
		"hit":
			motion_offset.x = -1.5 if int(pulse * 60.0) % 2 == 0 else 1.5

	var horizontal_facing: float = -1.0 if _visual_facing.x < -0.08 else 1.0
	draw_set_transform(motion_offset + Vector2(0, bob), motion_rotation, Vector2(horizontal_facing * motion_scale.x, motion_scale.y))
	_draw_class_avatar(step_wave if moving else 0.0)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _draw_class_avatar(step_wave: float) -> void:
	var outline := Color("171219")
	var skin := Color("e7b183")
	var cloth := body_color
	var cloth_dark := body_color.darkened(0.34)
	var metal := Color("c6c3bd")
	var accent := Color("f2d273")
	var leg_left: float = step_wave * 1.4
	var leg_right: float = -step_wave * 1.4

	# compact old-school RPG proportions: readable head, short torso, tiny feet
	draw_rect(Rect2(-5, -13, 10, 7), outline, true)
	draw_rect(Rect2(-4, -12, 8, 6), skin, true)
	draw_rect(Rect2(-6, -6, 12, 12), outline, true)
	draw_rect(Rect2(-5, -5, 10, 10), cloth_dark, true)
	draw_rect(Rect2(-4, -4, 8, 8), cloth, true)
	draw_line(Vector2(-3, 5), Vector2(-3 + leg_left, 10), outline, 3.0)
	draw_line(Vector2(3, 5), Vector2(3 + leg_right, 10), outline, 3.0)

	match class_id:
		"warrior":
			draw_rect(Rect2(-6, -16, 12, 4), outline, true)
			draw_rect(Rect2(-5, -15, 10, 3), Color("7b2f2a"), true)
			draw_line(Vector2(5, -2), Vector2(11, 5), metal, 2.0)
			draw_line(Vector2(10, 4), Vector2(12, 7), accent, 1.0)
		"mage":
			draw_colored_polygon(PackedVector2Array([Vector2(-7,-12), Vector2(0,-20), Vector2(7,-12)]), outline)
			draw_colored_polygon(PackedVector2Array([Vector2(-5,-12), Vector2(0,-18), Vector2(5,-12)]), Color("4c3b8f"))
			draw_line(Vector2(6, -3), Vector2(10, 8), Color("7d5b3f"), 2.0)
			draw_circle(Vector2(6, -4), 2.2, Color("8be0f1"))
		"knight":
			draw_rect(Rect2(-6, -16, 12, 5), outline, true)
			draw_rect(Rect2(-5, -15, 10, 4), metal, true)
			draw_rect(Rect2(4, -3, 5, 8), outline, true)
			draw_rect(Rect2(5, -2, 3, 6), Color("7d8994"), true)
		"sage":
			draw_rect(Rect2(-5, -15, 10, 3), Color("6a4f32"), true)
			draw_line(Vector2(6, -4), Vector2(10, 8), Color("75573d"), 2.0)
			draw_circle(Vector2(6, -5), 2.0, Color("c9a7ff"))
		"assassin":
			draw_colored_polygon(PackedVector2Array([Vector2(-6,-12), Vector2(0,-17), Vector2(6,-12)]), outline)
			draw_rect(Rect2(-4, -11, 8, 2), Color("2b2430"), true)
			draw_line(Vector2(5, 0), Vector2(11, 4), Color("d7d4cf"), 1.5)
			draw_line(Vector2(-5, 0), Vector2(-11, 4), Color("d7d4cf"), 1.5)
		"saint":
			draw_circle(Vector2(0, -18), 4.2, Color("f6e58d", 0.35))
			draw_arc(Vector2(0, -18), 4.2, 0.0, TAU, 18, Color("f6e58d"), 1.0)
			draw_line(Vector2(6, -2), Vector2(6, 8), Color("e8ddbd"), 2.0)
			draw_line(Vector2(3, 1), Vector2(9, 1), Color("e8ddbd"), 1.5)


func _draw_attack_swing(progress: float, strength: float) -> void:
	var aim_angle: float = _visual_facing.angle()
	var sweep_center: float = aim_angle + lerpf(-0.95, 0.95, smoothstep(0.0, 1.0, progress))
	var alpha: float = sin(progress * PI)
	var attack_color := Color("fff0c2", alpha * 0.92)
	draw_arc(_visual_facing * 5.0, 18.0 + strength * 2.0, sweep_center - 0.42, sweep_center + 0.42, 10, attack_color, 2.0, true)
	draw_arc(_visual_facing * 5.0, 15.0 + strength * 1.5, sweep_center - 0.32, sweep_center + 0.32, 8, Color(body_color, alpha * 0.52), 1.0, true)


func _skill_visual_color(skill_type: String) -> Color:
	match skill_type:
		"melee_spin": return Color("ff8066")
		"fireball": return Color("ff9f43")
		"shield_charge": return Color("8be0f1")
		"chain_lightning": return Color("c9a7ff")
		"multi_slash": return Color("ff72b6")
		"holy_nova": return Color("fff2a1")
	return Color("ffd166")


func draw_ellipse(center: Vector2, radius: Vector2, color: Color) -> void:
	var points := PackedVector2Array()
	for index: int in 16:
		var angle: float = TAU * index / 16.0
		points.append(center + Vector2(cos(angle) * radius.x, sin(angle) * radius.y))
	draw_colored_polygon(points, color)
