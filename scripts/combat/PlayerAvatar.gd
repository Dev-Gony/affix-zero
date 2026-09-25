extends Node2D
class_name PlayerAvatar

const CLASS_ATLAS: Texture2D = preload("res://assets/sprites/class_atlas_alpha.png")
const CLASS_REGIONS: Dictionary = {
	"warrior": Vector2i(0, 0),
	"mage": Vector2i(1, 0),
	"knight": Vector2i(2, 0),
	"sage": Vector2i(0, 1),
	"assassin": Vector2i(1, 1),
	"saint": Vector2i(2, 1),
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
var _weapon_base_id: String = ""
var _weapon_rarity_color: Color = Color("d7dde8")
var _weapon_enhancement: int = 0
const ATTACK_WINDUP: float = 0.12
const ATTACK_DURATION: float = 0.36


func configure(next_class_id: String, color: Color) -> void:
	class_id = next_class_id
	body_color = color
	visible = true
	queue_redraw()


func set_equipment_visual(weapon_item: Dictionary) -> void:
	if weapon_item.is_empty():
		_weapon_base_id = ""
		_weapon_rarity_color = Color("d7dde8")
		_weapon_enhancement = 0
	else:
		_weapon_base_id = String(weapon_item.get("base_id", ""))
		_weapon_rarity_color = Color.from_string(String(weapon_item.get("rarity_color", "d7dde8")), Color("d7dde8"))
		_weapon_enhancement = int(weapon_item.get("enhancement_level", 0))
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
		if _motion_kind == "idle":
			_facing = _move_direction
	queue_redraw()


func play_attack(world_target: Vector2) -> void:
	_face_target(world_target)
	_start_motion("attack", ATTACK_DURATION)


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
	var bob: float = roundf(step_wave * 1.0)
	draw_ellipse(Vector2(0, 11), Vector2(9.5, 3.2), Color(0, 0, 0, 0.50))
	if not _weapon_base_id.is_empty():
		var aura_alpha: float = 0.10 + minf(0.18, float(_weapon_enhancement) * 0.006)
		draw_arc(Vector2.ZERO, 14.5, 0.0, TAU, 28, Color(_weapon_rarity_color, aura_alpha + sin(pulse * 3.0) * 0.03), 1.2)

	var motion_progress: float = 1.0
	if _motion_duration > 0.0 and _motion_time_left > 0.0:
		motion_progress = 1.0 - (_motion_time_left / _motion_duration)
	var motion_offset := Vector2.ZERO
	var motion_rotation: float = 0.0
	var motion_scale := Vector2.ONE
	match _motion_kind:
		"attack":
			var elapsed: float = motion_progress * ATTACK_DURATION
			var strike: float = 0.0
			if elapsed < ATTACK_WINDUP:
				motion_offset = -_facing * (elapsed / ATTACK_WINDUP) * 2.5
			else:
				var release: float = clampf((elapsed - ATTACK_WINDUP) / (ATTACK_DURATION - ATTACK_WINDUP), 0.0, 1.0)
				strike = pow(1.0 - release, 2.0)
				motion_offset = _facing * strike * (2.0 if is_ranged() else 6.0)
				if not is_ranged():
					_draw_attack_swing(release, strike)
			if is_ranged():
				var charge: float = sin(motion_progress * PI)
				draw_circle(_facing * 10.0 + Vector2(0, -7), 2.5 + charge * 2.0, Color(body_color, charge * 0.65))
		"skill":
			var charge: float = sin(motion_progress * PI)
			motion_scale = Vector2(1.0 + charge * 0.08, 1.0 - charge * 0.05)
			draw_arc(Vector2.ZERO, 13.0 + charge * 4.0, -PI * 0.2, PI * 1.2, 20, Color(_skill_color, 0.72 * charge), 1.6)
		"hit":
			motion_offset.x = -1.5 if int(pulse * 60.0) % 2 == 0 else 1.5

	var atlas_cell: Vector2i = CLASS_REGIONS.get(class_id, Vector2i.ZERO)
	var cell_size := Vector2(float(CLASS_ATLAS.get_width()) / 3.0, float(CLASS_ATLAS.get_height()) / 2.0)
	var source := Rect2(Vector2(atlas_cell) * cell_size, cell_size)
	var horizontal_facing: float = -1.0 if _visual_facing.x < -0.08 else 1.0
	draw_set_transform(motion_offset + Vector2(0, bob), motion_rotation, Vector2(horizontal_facing * motion_scale.x, motion_scale.y))
	draw_texture_rect_region(CLASS_ATLAS, Rect2(-18, -26, 36, 36), source)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _draw_attack_swing(progress: float, strength: float) -> void:
	var aim_angle: float = _visual_facing.angle()
	var sweep_center: float = aim_angle + lerpf(-1.05, 1.05, smoothstep(0.0, 1.0, progress))
	var alpha: float = sin(progress * PI)
	var rarity_mix: Color = _weapon_rarity_color if not _weapon_base_id.is_empty() else Color("fff0c2")
	var attack_color := Color(rarity_mix.lightened(0.22), alpha * 0.94)
	draw_arc(_visual_facing * 6.0, 21.0 + strength * 3.0, sweep_center - 0.46, sweep_center + 0.46, 12, attack_color, 2.5, true)
	draw_arc(_visual_facing * 5.0, 17.0 + strength * 2.0, sweep_center - 0.34, sweep_center + 0.34, 10, Color(body_color, alpha * 0.55), 1.2, true)


func is_ranged() -> bool:
	return class_id in ["mage", "sage", "saint"]


func _draw_equipped_weapon(_motion_progress: float) -> void:
	# The approved atlas already includes the hero's weapon/staff.
	# A second procedural weapon must not replace the character's attack identity.
	pass


func _weapon_visual_color() -> Color:
	if _weapon_base_id.contains("magic"):
		return Color("b56dff")
	if _weapon_base_id.contains("divine"):
		return Color("ffd866")
	if _weapon_base_id.contains("axe"):
		return Color("e4924a")
	if _weapon_base_id.contains("dagger"):
		return Color("dce6ee")
	return _weapon_rarity_color.lightened(0.10)


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
