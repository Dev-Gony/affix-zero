extends Node2D
class_name PetCompanion

var player_target: Node2D
var _pet_data: PetData
var _clock: float = 0.0
var _last_position: Vector2 = Vector2.ZERO


func _ready() -> void:
	PetManager.active_pet_changed.connect(_on_active_pet_changed)
	PetManager.pet_state_changed.connect(_sync_pet)
	_sync_pet()


func bind_player(player: Node2D) -> void:
	player_target = player
	if player_target != null:
		global_position = player_target.global_position + Vector2(-18, -12)
		_last_position = global_position


func _process(delta: float) -> void:
	_clock += delta
	if player_target == null or not is_instance_valid(player_target) or not player_target.visible or _pet_data == null:
		visible = false
		return
	visible = true
	var side: float = -1.0 if int(_clock / 3.0) % 2 == 0 else 1.0
	var follow_offset := Vector2(side * _pet_data.follow_distance, -16.0 + sin(_clock * 3.0) * 3.0)
	var desired: Vector2 = player_target.global_position + follow_offset
	global_position = global_position.lerp(desired, 1.0 - exp(-delta * 7.5))
	_last_position = global_position
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
	var bob: float = sin(_clock * 4.0) * 1.5
	var stars: int = PetManager.stars_for(_pet_data.id)
	var evolution_scale: float = 1.0 + float(stars - 1) * 0.07
	var aura_alpha: float = 0.05 + float(stars - 1) * 0.035
	draw_circle(Vector2(0, 7 + bob), 8.5 * evolution_scale, Color(color, 0.10))
	if stars >= 2:
		draw_arc(Vector2(0, bob), 10.0 + stars * 1.5, 0.0, TAU, 20, Color(color, aura_alpha), 1.0 + stars * 0.18)
	if stars >= 4:
		draw_arc(Vector2(0, bob), 15.0 + sin(_clock * 3.0) * 1.5, -0.6, PI + 0.6, 22, Color(color.lightened(0.25), aura_alpha * 1.35), 1.2)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE * evolution_scale)
	match _pet_data.id:
		"spirit_fox":
			_draw_fox(color, bob)
		"ember_drake":
			_draw_drake(color, bob)
		"ghost_slime":
			_draw_slime(color, bob)
		"stone_golem":
			_draw_golem(color, bob)
		"night_bat":
			_draw_bat(color, bob)
		"meadow_fairy":
			_draw_fairy(color, bob)
		_:
			draw_circle(Vector2(0, bob), 6.0, color)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _draw_fox(color: Color, bob: float) -> void:
	draw_circle(Vector2(0, bob), 5.0, color)
	draw_colored_polygon(PackedVector2Array([Vector2(-5, -3+bob), Vector2(-3, -10+bob), Vector2(0, -4+bob)]), color.lightened(0.08))
	draw_colored_polygon(PackedVector2Array([Vector2(5, -3+bob), Vector2(3, -10+bob), Vector2(0, -4+bob)]), color.lightened(0.08))
	for i: int in 3:
		var angle: float = -0.9 + float(i) * 0.9
		draw_arc(Vector2(-2, 4+bob), 8.0 + i * 1.5, angle, angle + 0.85, 8, Color(color, 0.65), 2.0)
	draw_circle(Vector2(-1.7, -1+bob), 0.8, Color("07111f"))
	draw_circle(Vector2(1.7, -1+bob), 0.8, Color("07111f"))


func _draw_drake(color: Color, bob: float) -> void:
	draw_circle(Vector2(0, bob), 5.5, color)
	draw_colored_polygon(PackedVector2Array([Vector2(-4, 0+bob), Vector2(-12, -5+bob), Vector2(-9, 4+bob)]), color.darkened(0.08))
	draw_colored_polygon(PackedVector2Array([Vector2(4, 0+bob), Vector2(12, -5+bob), Vector2(9, 4+bob)]), color.darkened(0.08))
	draw_colored_polygon(PackedVector2Array([Vector2(-2, -4+bob), Vector2(0, -10+bob), Vector2(2, -4+bob)]), color.lightened(0.18))


func _draw_slime(color: Color, bob: float) -> void:
	draw_circle(Vector2(0, 1+bob), 6.5, Color(color, 0.88))
	draw_rect(Rect2(-6, 1+bob, 12, 5), Color(color, 0.88), true)
	draw_circle(Vector2(-2, 0+bob), 1.0, Color("07111f"))
	draw_circle(Vector2(2, 0+bob), 1.0, Color("07111f"))


func _draw_golem(color: Color, bob: float) -> void:
	draw_rect(Rect2(-5, -5+bob, 10, 11), color, true)
	draw_rect(Rect2(-8, -2+bob, 3, 7), color.darkened(0.12), true)
	draw_rect(Rect2(5, -2+bob, 3, 7), color.darkened(0.12), true)
	draw_rect(Rect2(-2, -1+bob, 4, 2), Color("70d6ff"), true)


func _draw_bat(color: Color, bob: float) -> void:
	draw_circle(Vector2(0, bob), 3.5, color)
	draw_colored_polygon(PackedVector2Array([Vector2(-3, bob), Vector2(-11, -5+bob), Vector2(-8, 4+bob)]), color)
	draw_colored_polygon(PackedVector2Array([Vector2(3, bob), Vector2(11, -5+bob), Vector2(8, 4+bob)]), color)
	draw_circle(Vector2(0, -1+bob), 0.8, Color("ff6b6b"))


func _draw_fairy(color: Color, bob: float) -> void:
	draw_circle(Vector2(0, bob), 3.0, color)
	draw_circle(Vector2(-5, -1+bob), 3.0, Color(color.lightened(0.2), 0.55))
	draw_circle(Vector2(5, -1+bob), 3.0, Color(color.lightened(0.2), 0.55))
	draw_circle(Vector2(0, bob), 7.0, Color(color, 0.08))
