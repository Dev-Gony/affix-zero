extends Node2D
class_name EnemyProjectile

signal impacted(world_position: Vector2, raw_damage: float, is_boss: bool)

var destination: Vector2
var target_node: Node2D
var raw_damage: float = 1.0
var is_boss: bool = false
var projectile_color: Color = Color("c084fc")
var travel_speed: float = 150.0
var projectile_kind: String = "orb"
var curve_strength: float = 0.0
var _clock: float = 0.0
var _start_position: Vector2
var _travel_distance: float = 1.0


func setup(start: Vector2, target: Node2D, damage: float, boss: bool, color: Color = Color("c084fc"), kind: String = "orb") -> void:
	global_position = start
	_start_position = start
	target_node = target
	destination = target.global_position if target != null and is_instance_valid(target) else start
	raw_damage = damage
	is_boss = boss
	projectile_color = color
	projectile_kind = kind
	match projectile_kind:
		"bone":
			travel_speed = 210.0
		"shadow":
			travel_speed = 145.0
			curve_strength = 16.0
		"fire":
			travel_speed = 135.0 if boss else 155.0
		"meteor":
			travel_speed = 105.0
		_:
			travel_speed = 125.0 if boss else 165.0
	_travel_distance = maxf(1.0, start.distance_to(destination))
	queue_redraw()


func _process(delta: float) -> void:
	_clock += delta
	if target_node != null and is_instance_valid(target_node):
		destination = target_node.global_position
	var distance: float = global_position.distance_to(destination)
	if distance <= 5.0:
		global_position = destination
		impacted.emit(global_position, raw_damage, is_boss)
		queue_free()
		return
	var next_position: Vector2 = global_position.move_toward(destination, travel_speed * delta)
	if curve_strength > 0.0:
		var direction: Vector2 = global_position.direction_to(destination)
		var normal := Vector2(-direction.y, direction.x)
		next_position += normal * sin(_clock * 7.0) * curve_strength * delta
	global_position = next_position
	queue_redraw()


func _draw() -> void:
	var direction: Vector2 = global_position.direction_to(destination)
	if direction.is_zero_approx():
		direction = Vector2.RIGHT
	var pulse: float = 1.0 + sin(_clock * 13.0) * 0.12
	var progress: float = clampf(_start_position.distance_to(global_position) / _travel_distance, 0.0, 1.0)
	match projectile_kind:
		"bone":
			draw_set_transform(Vector2.ZERO, direction.angle(), Vector2.ONE)
			draw_line(Vector2(-8, 0), Vector2(8, 0), Color("efe4cf"), 3.0)
			draw_circle(Vector2(-8, 0), 2.4, Color("d8c9ab"))
			draw_circle(Vector2(8, 0), 2.4, Color("d8c9ab"))
			draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
		"shadow":
			for index: int in range(1, 6):
				var tail := -direction * float(index) * 5.0
				draw_circle(tail, 4.8 - index * 0.55, Color(projectile_color, 0.20 * (1.0 - float(index) / 6.0)))
			draw_circle(Vector2.ZERO, 8.0 * pulse, Color(projectile_color, 0.13))
			draw_circle(Vector2.ZERO, 4.6 * pulse, Color(projectile_color, 0.92))
			draw_arc(Vector2.ZERO, 10.0, _clock * 2.2, _clock * 2.2 + PI, 14, Color("e0c2ff", 0.40), 1.5)
		"fire":
			for index: int in range(1, 7):
				var tail := -direction * float(index) * 4.5
				var col := Color("ff6a32", 0.24 * (1.0 - float(index) / 7.0))
				draw_circle(tail, 5.0 - index * 0.45, col)
			draw_circle(Vector2.ZERO, 9.0 * pulse, Color("ff4d20", 0.13))
			draw_circle(Vector2.ZERO, 5.8 * pulse, Color("ff7a2f"))
			draw_circle(Vector2(-1, -1), 2.4, Color("ffd166"))
		"meteor":
			for index: int in range(1, 9):
				var tail := -direction * float(index) * 5.5
				draw_circle(tail, 6.5 - index * 0.55, Color("ff3f2f", 0.25 * (1.0 - float(index) / 9.0)))
			draw_circle(Vector2.ZERO, 11.0 * pulse, Color("ff2d2d", 0.15))
			draw_circle(Vector2.ZERO, 7.2 * pulse, Color("8b1e1e"))
			draw_circle(Vector2(-2, -2), 3.0, Color("ffb347"))
		_:
			for index: int in range(1, 7):
				var tail := -direction * float(index) * (4.0 if not is_boss else 5.0)
				var tail_alpha: float = (0.22 if not is_boss else 0.30) * (1.0 - float(index) / 7.0)
				draw_circle(tail, (4.6 if is_boss else 3.4) - index * 0.32, Color(projectile_color, tail_alpha))
			if is_boss:
				draw_arc(Vector2.ZERO, 11.0 * pulse, _clock * 2.0, _clock * 2.0 + PI * 1.5, 18, Color("ff8b67", 0.52), 2.0)
				draw_circle(Vector2.ZERO, 7.2 * pulse, Color(projectile_color, 0.90))
				draw_circle(Vector2.ZERO, 3.0, Color("fff0d6"))
			else:
				draw_circle(Vector2.ZERO, 7.0 * pulse, Color(projectile_color, 0.12))
				draw_circle(Vector2.ZERO, 4.2 * pulse, Color(projectile_color, 0.92))
				draw_circle(Vector2.ZERO, 1.6, Color("f6edff"))
	if progress > 0.65:
		draw_arc(Vector2.ZERO, 8.0 + pulse * 2.0, 0.0, TAU, 18, Color(projectile_color, 0.14), 1.0)
