extends Node2D
class_name PlayerBasicProjectile

signal impacted(target: EnemyAI, result: Dictionary, world_position: Vector2)

var target: EnemyAI
var result: Dictionary = {}
var projectile_color: Color = Color("9fdcff")
var speed: float = 240.0
var _clock: float = 0.0
var _direction: Vector2 = Vector2.RIGHT


func setup(start: Vector2, enemy: EnemyAI, damage_result: Dictionary, color: Color) -> void:
	global_position = start
	target = enemy
	result = damage_result.duplicate(true)
	projectile_color = color
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
	for index: int in range(1, 5):
		var tail: Vector2 = -_direction * float(index) * 4.0
		draw_circle(tail, 3.6 - index * 0.45, Color(projectile_color, 0.18 * (1.0 - float(index) / 5.0)))
	draw_circle(Vector2.ZERO, 6.0 * pulse, Color(projectile_color, 0.12))
	draw_circle(Vector2.ZERO, 3.5 * pulse, projectile_color)
	draw_circle(Vector2(-0.8, -0.8), 1.0, Color("ffffff"))
