extends Node2D
class_name EnemyProjectile

signal impacted(world_position: Vector2, raw_damage: float, is_boss: bool)

var destination: Vector2
var raw_damage: float = 1.0
var is_boss: bool = false
var projectile_color: Color = Color("c084fc")
var travel_speed: float = 150.0
var _clock: float = 0.0
var _start_position: Vector2
var _travel_distance: float = 1.0


func setup(start: Vector2, target: Vector2, damage: float, boss: bool, color: Color = Color("c084fc")) -> void:
	global_position = start
	_start_position = start
	destination = target
	raw_damage = damage
	is_boss = boss
	projectile_color = color
	travel_speed = 125.0 if boss else 165.0
	_travel_distance = maxf(1.0, start.distance_to(target))
	queue_redraw()


func _process(delta: float) -> void:
	_clock += delta
	var distance: float = global_position.distance_to(destination)
	if distance <= 5.0:
		global_position = destination
		impacted.emit(global_position, raw_damage, is_boss)
		queue_free()
		return
	global_position = global_position.move_toward(destination, travel_speed * delta)
	queue_redraw()


func _draw() -> void:
	var direction: Vector2 = global_position.direction_to(destination)
	if direction.is_zero_approx():
		direction = Vector2.RIGHT
	var pulse: float = 1.0 + sin(_clock * 13.0) * 0.12
	var progress: float = clampf(_start_position.distance_to(global_position) / _travel_distance, 0.0, 1.0)
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
