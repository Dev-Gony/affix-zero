extends Node2D
class_name PetCompanion

var player_target: Node2D
var _pet_data: PetData
var _clock: float = 0.0
var _last_position: Vector2 = Vector2.ZERO
var _action_kind: String = ""
var _action_time_left: float = 0.0
var _action_duration: float = 0.0
var _action_direction: Vector2 = Vector2.RIGHT


func _ready() -> void:
	PetManager.active_pet_changed.connect(_on_active_pet_changed)
	PetManager.pet_state_changed.connect(_sync_pet)
	_sync_pet()


func bind_player(player: Node2D) -> void:
	player_target = player
	if player_target != null:
		global_position = player_target.global_position + Vector2(-24, -16)
		_last_position = global_position


func _process(delta: float) -> void:
	_clock += delta
	_action_time_left = maxf(0.0, _action_time_left - delta)
	if player_target == null or not is_instance_valid(player_target) or not player_target.visible or _pet_data == null:
		visible = false
		return
	visible = true
	var side: float = -1.0 if int(_clock / 3.2) % 2 == 0 else 1.0
	var orbit_y: float = -18.0 + sin(_clock * 2.8) * 4.0
	var follow_offset := Vector2(side * _pet_data.follow_distance, orbit_y)
	var desired: Vector2 = player_target.global_position + follow_offset
	var follow_lerp: float = 1.0 - exp(-delta * 6.4)
	global_position = global_position.lerp(desired, follow_lerp)
	_last_position = global_position
	queue_redraw()


func play_attack(world_target: Vector2) -> void:
	_action_kind = "attack"
	_action_duration = 0.30
	_action_time_left = _action_duration
	var direction := global_position.direction_to(world_target)
	if not direction.is_zero_approx():
		_action_direction = direction
	queue_redraw()


func play_support() -> void:
	_action_kind = "support"
	_action_duration = 0.48
	_action_time_left = _action_duration
	queue_redraw()


func _on_active_pet_changed(_pet_id: String) -> void:
	_sync_pet()


func _sync_pet() -> void:
	_pet_data = PetManager.active_pet_data()
	visible = _pet_data != null
	queue_redraw()


func pet_color() -> Color:
	return _pet_data.color if _pet_data != null else Color.WHITE


func pet_id() -> String:
	return _pet_data.id if _pet_data != null else ""


func _draw() -> void:
	if _pet_data == null:
		return
	var color: Color = _pet_data.color
	var rarity_color: Color = PetManager.rarity_color(_pet_data.rarity_index)
	var stars: int = PetManager.stars_for(_pet_data.id)
	var evolution_scale: float = 1.0 + float(stars - 1) * 0.055
	var idle_bob: float = sin(_clock * 4.0) * 1.8
	var action_progress: float = 1.0
	if _action_duration > 0.0 and _action_time_left > 0.0:
		action_progress = 1.0 - _action_time_left / _action_duration
	var action_offset := Vector2.ZERO
	var action_scale := Vector2.ONE
	var action_rotation: float = 0.0
	if _action_kind == "attack" and _action_time_left > 0.0:
		var lunge: float = sin(action_progress * PI)
		action_offset = _action_direction * lunge * 7.0
		action_rotation = _action_direction.x * lunge * 0.15
		action_scale = Vector2(1.0 + lunge * 0.10, 1.0 - lunge * 0.06)
	elif _action_kind == "support" and _action_time_left > 0.0:
		var pulse: float = sin(action_progress * PI)
		action_scale = Vector2.ONE * (1.0 + pulse * 0.16)
		draw_arc(Vector2(0, idle_bob), 14.0 + pulse * 9.0, 0.0, TAU, 24, Color("8cf5c4", 0.58 * pulse), 1.7)

	# Ground contact and rarity aura make the pet belong to the same dark ARPG scene.
	draw_ellipse(Vector2(0, 9), Vector2(10.0, 3.0), Color(0, 0, 0, 0.46))
	var aura_alpha: float = 0.07 + float(_pet_data.rarity_index) * 0.025 + float(stars - 1) * 0.02
	draw_circle(Vector2(0, idle_bob), 13.0 + stars, Color(rarity_color, aura_alpha))
	if _pet_data.rarity_index >= 3:
		draw_arc(Vector2(0, idle_bob), 13.0 + stars * 0.8, 0.0, TAU, 24, Color(rarity_color, 0.26), 1.0)
	if stars >= 4:
		draw_arc(Vector2(0, idle_bob), 17.0 + sin(_clock * 3.0) * 1.0, -0.8, PI + 0.8, 22, Color(rarity_color.lightened(0.18), 0.26), 1.2)

	draw_set_transform(action_offset + Vector2(0, idle_bob), action_rotation, action_scale * evolution_scale)
	match _pet_data.id:
		"spirit_fox":
			_draw_fox(color)
		"ember_drake":
			_draw_drake(color)
		"ghost_slime":
			_draw_slime(color)
		"stone_golem":
			_draw_golem(color)
		"night_bat":
			_draw_bat(color)
		"meadow_fairy":
			_draw_fairy(color)
		_:
			draw_circle(Vector2.ZERO, 7.0, color)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _draw_fox(color: Color) -> void:
	var dark := color.darkened(0.38)
	var light := color.lightened(0.30)
	for index: int in 3:
		var swing: float = sin(_clock * 3.4 + index * 0.8) * 0.24
		draw_arc(Vector2(-5, 6), 11.0 + index * 2.0, -1.05 + index * 0.22 + swing, -0.10 + index * 0.22 + swing, 12, Color(color, 0.64), 2.4)
	draw_ellipse(Vector2(0, 5), Vector2(7.0, 5.4), dark)
	draw_circle(Vector2(0, -2), 7.6, color)
	draw_colored_polygon(PackedVector2Array([Vector2(-6,-6),Vector2(-4,-14),Vector2(-1,-7)]), color.lightened(0.08))
	draw_colored_polygon(PackedVector2Array([Vector2(6,-6),Vector2(4,-14),Vector2(1,-7)]), color.lightened(0.08))
	draw_colored_polygon(PackedVector2Array([Vector2(-4,-7),Vector2(-3,-11),Vector2(-1,-7)]), dark)
	draw_colored_polygon(PackedVector2Array([Vector2(4,-7),Vector2(3,-11),Vector2(1,-7)]), dark)
	draw_ellipse(Vector2(0, 1), Vector2(3.7, 2.7), light)
	draw_circle(Vector2(-2.7, -3), 1.0, Color("e8fbff"))
	draw_circle(Vector2(2.7, -3), 1.0, Color("e8fbff"))
	draw_circle(Vector2(-2.7, -3), 0.45, Color("10243b"))
	draw_circle(Vector2(2.7, -3), 0.45, Color("10243b"))
	draw_circle(Vector2(0, 0.1), 0.8, dark)


func _draw_drake(color: Color) -> void:
	var dark := color.darkened(0.42)
	var hot := Color("ffb13b")
	var flap: float = 2.0 + absf(sin(_clock * 7.0)) * 3.0
	draw_colored_polygon(PackedVector2Array([Vector2(-4,1),Vector2(-17,-7-flap),Vector2(-14,4),Vector2(-8,8)]), dark)
	draw_colored_polygon(PackedVector2Array([Vector2(4,1),Vector2(17,-7-flap),Vector2(14,4),Vector2(8,8)]), dark)
	draw_colored_polygon(PackedVector2Array([Vector2(-6,1),Vector2(-14,-4-flap*0.4),Vector2(-11,4)]), color)
	draw_colored_polygon(PackedVector2Array([Vector2(6,1),Vector2(14,-4-flap*0.4),Vector2(11,4)]), color)
	draw_ellipse(Vector2(0, 5), Vector2(7.2, 6.0), dark)
	draw_circle(Vector2(0, -3), 7.4, color)
	draw_colored_polygon(PackedVector2Array([Vector2(-4,-8),Vector2(-7,-14),Vector2(-1,-10)]), hot)
	draw_colored_polygon(PackedVector2Array([Vector2(4,-8),Vector2(7,-14),Vector2(1,-10)]), hot)
	draw_circle(Vector2(-2.6, -4), 1.0, Color("fff3b0"))
	draw_circle(Vector2(2.6, -4), 1.0, Color("fff3b0"))
	draw_circle(Vector2(-2.6, -4), 0.45, Color("4a120c"))
	draw_circle(Vector2(2.6, -4), 0.45, Color("4a120c"))
	draw_arc(Vector2(0, 6), 10.0, 0.0, 0.95, 10, Color(color, 0.72), 2.0)


func _draw_slime(color: Color) -> void:
	var squash: float = sin(_clock * 5.0) * 0.8
	var body := Color(color, 0.90)
	var center := Vector2(0, 2 + squash * 0.4)
	draw_ellipse(center, Vector2(9.5 - squash * 0.4, 8.2 + squash * 0.5), body)
	draw_rect(Rect2(-9.0, 2.0, 18.0, 6.0), body, true)
	draw_circle(Vector2(-3.2, -1), 1.3, Color("e7f4ff"))
	draw_circle(Vector2(3.2, -1), 1.3, Color("e7f4ff"))
	draw_circle(Vector2(-3.2, -1), 0.55, Color("101b2b"))
	draw_circle(Vector2(3.2, -1), 0.55, Color("101b2b"))
	draw_arc(Vector2(0, 1), 3.4, 0.25, PI - 0.25, 9, Color("29425f"), 1.0)
	draw_circle(Vector2(-3.8, -5), 2.6, Color("ffffff", 0.13))


func _draw_golem(color: Color) -> void:
	var dark := color.darkened(0.30)
	var rune := Color("66dcff")
	var arm_swing: float = sin(_clock * 4.5) * 1.5
	draw_rect(Rect2(-8, -9, 16, 18), color, true)
	draw_rect(Rect2(-13, -3 + arm_swing, 5, 12), dark, true)
	draw_rect(Rect2(8, -3 - arm_swing, 5, 12), dark, true)
	draw_rect(Rect2(-7, 9, 6, 5), dark, true)
	draw_rect(Rect2(1, 9, 6, 5), dark, true)
	draw_colored_polygon(PackedVector2Array([Vector2(-7,-9),Vector2(-3,-14),Vector2(0,-9)]), color.lightened(0.10))
	draw_colored_polygon(PackedVector2Array([Vector2(7,-9),Vector2(3,-14),Vector2(0,-9)]), color.lightened(0.10))
	draw_rect(Rect2(-4, -3, 8, 4), Color("17222b"), true)
	draw_rect(Rect2(-3, -2, 2, 2), rune, true)
	draw_rect(Rect2(1, -2, 2, 2), rune, true)
	draw_arc(Vector2(0, 4), 4.5, 0.0, TAU, 12, Color(rune, 0.68), 1.2)


func _draw_bat(color: Color) -> void:
	var wing := color.darkened(0.26)
	var flap: float = sin(_clock * 10.0) * 5.0
	draw_colored_polygon(PackedVector2Array([Vector2(-4,0),Vector2(-18,-7-flap),Vector2(-15,1),Vector2(-18,8+flap*0.35),Vector2(-7,5)]), wing)
	draw_colored_polygon(PackedVector2Array([Vector2(4,0),Vector2(18,-7-flap),Vector2(15,1),Vector2(18,8+flap*0.35),Vector2(7,5)]), wing)
	draw_circle(Vector2.ZERO, 5.6, color)
	draw_colored_polygon(PackedVector2Array([Vector2(-4,-4),Vector2(-7,-10),Vector2(-1,-6)]), color)
	draw_colored_polygon(PackedVector2Array([Vector2(4,-4),Vector2(7,-10),Vector2(1,-6)]), color)
	draw_circle(Vector2(-2.2, -1.5), 1.0, Color("ff6b83"))
	draw_circle(Vector2(2.2, -1.5), 1.0, Color("ff6b83"))


func _draw_fairy(color: Color) -> void:
	var glow_color := color.lightened(0.30)
	var wing_wave: float = sin(_clock * 8.0) * 1.5
	draw_ellipse(Vector2(-7-wing_wave, -4), Vector2(5, 8), Color(glow_color, 0.34))
	draw_ellipse(Vector2(7+wing_wave, -4), Vector2(5, 8), Color(glow_color, 0.34))
	draw_ellipse(Vector2(-6-wing_wave*0.6, 4), Vector2(4, 6), Color(glow_color, 0.26))
	draw_ellipse(Vector2(6+wing_wave*0.6, 4), Vector2(4, 6), Color(glow_color, 0.26))
	draw_circle(Vector2(0, -3), 4.8, Color("ffe5c7"))
	draw_ellipse(Vector2(0, 4), Vector2(4.0, 6.5), color)
	draw_colored_polygon(PackedVector2Array([Vector2(-5,-7),Vector2(0,-13),Vector2(5,-7),Vector2(3,-2),Vector2(-3,-2)]), color.darkened(0.18))
	draw_circle(Vector2(-1.8, -3), 0.7, Color("15331f"))
	draw_circle(Vector2(1.8, -3), 0.7, Color("15331f"))


func draw_ellipse(center: Vector2, radii: Vector2, color: Color) -> void:
	var points := PackedVector2Array()
	for index: int in 20:
		var angle: float = TAU * float(index) / 20.0
		points.append(center + Vector2(cos(angle) * radii.x, sin(angle) * radii.y))
	draw_colored_polygon(points, color)
