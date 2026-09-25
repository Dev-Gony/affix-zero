extends Node2D
class_name SkillProjectile

signal impacted(world_position: Vector2)

var destination: Vector2
var travel_speed: float = 230.0
var projectile_color: Color = Color("ff7b2e")
var _clock: float = 0.0
var _trail: Array[Vector2] = []


func setup(start_position: Vector2, end_position: Vector2, color: Color = Color("ff7b2e")) -> void:
	global_position = start_position
	destination = end_position
	projectile_color = color
	queue_redraw()


func _process(delta: float) -> void:
	_clock += delta
	_trail.push_front(global_position)
	if _trail.size() > 7:
		_trail.pop_back()
	global_position = global_position.move_toward(destination, travel_speed * delta)
	queue_redraw()
	if global_position.distance_to(destination) <= 2.0:
		impacted.emit(destination)
		queue_free()


func _draw() -> void:
	var direction: Vector2 = global_position.direction_to(destination)
	if direction.is_zero_approx():
		direction = Vector2.RIGHT
	for index: int in range(1, 6):
		var trail_pos: Vector2 = -direction * float(index) * 4.0
		var trail_alpha: float = 0.20 * (1.0 - float(index) / 6.0)
		draw_circle(trail_pos, 4.5 - index * 0.45, Color(projectile_color, trail_alpha))
	var pulse: float = 1.0 + sin(_clock * 16.0) * 0.12
	draw_circle(Vector2.ZERO, 8.0 * pulse, Color(projectile_color, 0.10))
	draw_circle(Vector2.ZERO, 5.4 * pulse, Color("ffd166"))
	draw_circle(Vector2.ZERO, 3.2 * pulse, projectile_color.lightened(0.08))
	draw_circle(Vector2(-1.0, -1.0), 1.2, Color("fff5cf"))
