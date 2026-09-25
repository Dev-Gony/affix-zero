extends Node2D
class_name PlayerAvatar

const CLASS_TEXTURES := {
	"warrior": preload("res://assets/cc0/tiny_dungeon/warrior.png"),
	"mage": preload("res://assets/cc0/tiny_dungeon/mage.png"),
	"knight": preload("res://assets/cc0/tiny_dungeon/knight.png"),
	"sage": preload("res://assets/cc0/tiny_dungeon/sage.png"),
	"assassin": preload("res://assets/cc0/tiny_dungeon/assassin.png"),
	"saint": preload("res://assets/cc0/tiny_dungeon/saint.png"),
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
			var strike: float = sin(smoothstep(0.0, 1.0, motion_progress) * PI)
			motion_offset = _visual_facing * strike * 4.0
			motion_rotation = lerpf(-0.08, 0.08, motion_progress)
			_draw_attack_swing(motion_progress, strike)
		"skill":
			var charge: float = sin(motion_progress * PI)
			motion_scale = Vector2(1.0 + charge * 0.08, 1.0 - charge * 0.05)
			draw_arc(Vector2.ZERO, 13.0 + charge * 4.0, -PI * 0.2, PI * 1.2, 20, Color(_skill_color, 0.72 * charge), 1.6)
		"hit":
			motion_offset.x = -1.5 if int(pulse * 60.0) % 2 == 0 else 1.5

	var texture: Texture2D = CLASS_TEXTURES.get(class_id, CLASS_TEXTURES["warrior"])
	var horizontal_facing: float = -1.0 if _visual_facing.x < -0.08 else 1.0
	draw_set_transform(motion_offset + Vector2(0, bob), motion_rotation, Vector2(horizontal_facing * motion_scale.x, motion_scale.y))
	draw_texture_rect(texture, Rect2(-14, -21, 28, 28), false)
	_draw_equipped_weapon(motion_progress)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _draw_attack_swing(progress: float, strength: float) -> void:
	var aim_angle: float = _visual_facing.angle()
	var sweep_center: float = aim_angle + lerpf(-1.05, 1.05, smoothstep(0.0, 1.0, progress))
	var alpha: float = sin(progress * PI)
	var rarity_mix: Color = _weapon_rarity_color if not _weapon_base_id.is_empty() else Color("fff0c2")
	var attack_color := Color(rarity_mix.lightened(0.22), alpha * 0.94)
	draw_arc(_visual_facing * 6.0, 21.0 + strength * 3.0, sweep_center - 0.46, sweep_center + 0.46, 12, attack_color, 2.5, true)
	draw_arc(_visual_facing * 5.0, 17.0 + strength * 2.0, sweep_center - 0.34, sweep_center + 0.34, 10, Color(body_color, alpha * 0.55), 1.2, true)


func _draw_equipped_weapon(motion_progress: float) -> void:
	if _weapon_base_id.is_empty():
		return
	var color: Color = _weapon_visual_color()
	var pivot := Vector2(8, -2)
	var angle: float = -0.55
	if _motion_kind == "attack":
		angle = lerpf(-1.25, 1.05, smoothstep(0.0, 1.0, motion_progress))
	elif _motion_kind == "skill":
		angle = -0.35 + sin(motion_progress * PI) * 0.55
	draw_set_transform(pivot, angle, Vector2.ONE)
	if _weapon_base_id.contains("axe"):
		draw_rect(Rect2(-1.5, -1, 3, 18), Color("82552f"), true)
		draw_colored_polygon(PackedVector2Array([
			Vector2(-1, -3), Vector2(9, -7), Vector2(10, 1), Vector2(0, 4)
		]), color)
	elif _weapon_base_id.contains("dagger"):
		draw_rect(Rect2(-1.2, 0, 2.4, 11), Color("7a4a24"), true)
		draw_colored_polygon(PackedVector2Array([
			Vector2(-2.5, -1), Vector2(0, -13), Vector2(2.5, -1)
		]), color)
	else:
		draw_rect(Rect2(-1.4, 0, 2.8, 11), Color("7a4a24"), true)
		draw_rect(Rect2(-6, -1, 12, 2.5), Color("d3aa52"), true)
		draw_colored_polygon(PackedVector2Array([
			Vector2(-2.1, -1), Vector2(-2.8, -17), Vector2(0, -22), Vector2(2.8, -17), Vector2(2.1, -1)
		]), color)
		if _weapon_base_id.contains("magic"):
			draw_circle(Vector2(0, -13), 2.4, Color("bf8bff"))
		elif _weapon_base_id.contains("divine"):
			draw_circle(Vector2(0, -13), 2.6, Color("fff4a8"))
	if _weapon_enhancement >= 10:
		draw_arc(Vector2(0, -9), 9.0, 0.0, TAU, 18, Color(_weapon_rarity_color, 0.18 + minf(0.35, float(_weapon_enhancement) * 0.012)), 1.2)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


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
