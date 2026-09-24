extends Node2D
class_name PlayerAvatar

const CLASS_ATLAS: Texture2D = preload("res://assets/sprites/class_atlas_alpha.png")
const CLASS_REGIONS: Dictionary = {
	"warrior": Vector2i(0, 0), "mage": Vector2i(1, 0), "knight": Vector2i(2, 0),
	"sage": Vector2i(0, 1), "assassin": Vector2i(1, 1), "saint": Vector2i(2, 1),
}

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
	_start_motion("attack", 0.26)


func play_skill(skill_type: String, world_target: Vector2) -> void:
	_face_target(world_target)
	_skill_color = _skill_visual_color(skill_type)
	_start_motion("skill", 0.34)


func play_hit() -> void:
	_start_motion("hit", 0.16)


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
	var step_wave: float = sin(pulse * 12.0) if moving else 0.0
	var bob: float = roundf((-absf(sin(pulse * TAU / 0.82)) * 0.6) + step_wave * 1.2)
	draw_ellipse(Vector2(0, 8), Vector2(10, 3.5), Color(0, 0, 0, 0.46))
	var glow_alpha: float = 0.05 + sin(pulse * 3.0) * 0.02
	draw_circle(Vector2.ZERO, 13.0, Color(body_color, glow_alpha))
	if moving:
		draw_line(Vector2(-5, 11), Vector2(-2, 12 + step_wave * 2.0), Color(1, 1, 1, 0.18), 1.0)
	var motion_progress: float = 1.0
	if _motion_duration > 0.0 and _motion_time_left > 0.0:
		motion_progress = 1.0 - (_motion_time_left / _motion_duration)
	var motion_offset := Vector2.ZERO
	var motion_rotation: float = 0.0
	var motion_scale := Vector2.ONE
	match _motion_kind:
		"attack":
			var strike: float = sin(smoothstep(0.0, 1.0, motion_progress) * PI)
			motion_offset = _visual_facing * strike * 8.0
			motion_rotation = lerpf(-0.08, 0.08, motion_progress) * signf(_visual_facing.x if not is_zero_approx(_visual_facing.x) else 1.0)
			_draw_attack_swing(motion_progress, strike)
		"skill":
			var charge: float = sin(motion_progress * PI)
			motion_scale = Vector2(1.0 + charge * 0.10, 1.0 - charge * 0.07)
			draw_arc(Vector2.ZERO, 17.0 + charge * 5.0, -PI * 0.15, PI * 1.35, 24, Color(_skill_color, 0.82 * charge), 2.0)
		"hit":
			motion_offset.x = -2.0 if int(pulse * 60.0) % 2 == 0 else 2.0
			motion_scale = Vector2(1.08, 0.92)
	var atlas_cell: Vector2i = CLASS_REGIONS.get(class_id, Vector2i.ZERO)
	var source := Rect2(atlas_cell.x * 512, atlas_cell.y * 512, 512, 512)
	var horizontal_facing: float = -1.0 if _visual_facing.x < -0.08 else 1.0
	draw_set_transform(motion_offset, motion_rotation, Vector2(horizontal_facing * motion_scale.x, motion_scale.y))
	draw_texture_rect_region(CLASS_ATLAS, Rect2(-18, -23 + bob, 36, 36), source)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _draw_attack_swing(progress: float, strength: float) -> void:
	var aim_angle: float = _visual_facing.angle()
	var sweep_center: float = aim_angle + lerpf(-0.82, 0.82, smoothstep(0.0, 1.0, progress))
	var alpha: float = sin(progress * PI)
	var attack_color := Color("fff0c2", alpha * 0.90)
	draw_arc(_visual_facing * 7.0, 23.0 + strength * 3.0, sweep_center - 0.48, sweep_center + 0.48, 12, attack_color, 2.4, true)
	draw_arc(_visual_facing * 7.0, 19.0 + strength * 2.0, sweep_center - 0.38, sweep_center + 0.38, 10, Color("dc3d33", alpha * 0.58), 1.2, true)
	var weapon_tip: Vector2 = Vector2.RIGHT.rotated(sweep_center) * (25.0 + strength * 3.0)
	draw_line(_visual_facing * 5.0, weapon_tip, Color("ffffff", alpha * 0.72), 1.2, true)


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
