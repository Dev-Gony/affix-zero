extends Node2D
class_name PlayerBasicProjectile

signal impacted(target: EnemyAI, result: Dictionary, world_position: Vector2)

var target: EnemyAI
var result: Dictionary = {}
var projectile_color: Color = Color("9fdcff")
var speed: float = 240.0
var projectile_style: String = "arcane"
var _clock: float = 0.0
var _direction: Vector2 = Vector2.RIGHT


func setup(start: Vector2, enemy: EnemyAI, damage_result: Dictionary, color: Color, style: String = "arcane") -> void:
	global_position = start
	target = enemy
	result = damage_result.duplicate(true)
	projectile_color = color
	projectile_style = style
	match projectile_style:
		"lightning":
			speed = 320.0
		"holy":
			speed = 250.0
		_:
			speed = 240.0
	if target != null and is_instance_valid(target):
		_direction = global_position.direction_to(target.global_position)
	queue_redraw()


func _process(delta: float) -> void:
	_clock += delta
	if target == null or not is_instance_valid(target):
		queue_free()
		return
	var destination: Vector2 = target.global_position
	var distance: float = global_position.distance_to(destination)
	if distance <= 6.0:
		impacted.emit(target, result, global_position)
		queue_free()
		return
	_direction = global_position.direction_to(destination)
	global_position = global_position.move_toward(destination, speed * delta)
	queue_redraw()


func _draw() -> void:
	var pulse: float = 1.0 + sin(_clock * 18.0) * 0.10
	match projectile_style:
		"lightning":
			var normal := Vector2(-_direction.y, _direction.x)
			var points := PackedVector2Array([
				-_direction * 16.0,
				-_direction * 8.0 + normal * 3.0,
				Vector2.ZERO,
				_direction * 5.0 - normal * 2.0,
			])
			draw_polyline(points, Color(projectile_color, 0.82), 2.4, true)
			draw_polyline(points, Color("f5ecff", 0.62), 0.9, true)
			draw_circle(Vector2.ZERO, 4.0 * pulse, Color(projectile_color, 0.16))
		"holy":
			for index: int in 4:
				var angle: float = _clock * 3.5 + TAU * float(index) / 4.0
				draw_circle(Vector2(cos(angle), sin(angle)) * 5.0, 1.2, Color("fff8c9", 0.72))
			draw_circle(Vector2.ZERO, 7.0 * pulse, Color(projectile_color, 0.10))
			draw_circle(Vector2.ZERO, 3.8 * pulse, Color("fff2a1"))
		_:
			for index: int in range(1, 6):
				var tail: Vector2 = -_direction * float(index) * 4.0
				draw_circle(tail, 3.8 - index * 0.42, Color(projectile_color, 0.20 * (1.0 - float(index) / 6.0)))
			draw_circle(Vector2.ZERO, 6.5 * pulse, Color(projectile_color, 0.13))
			draw_circle(Vector2.ZERO, 3.7 * pulse, projectile_color)
			draw_circle(Vector2(-0.8, -0.8), 1.0, Color("ffffff"))
