extends Node2D
class_name SkillProjectile

signal impacted(world_position: Vector2)

var destination: Vector2
var travel_speed: float = 230.0
var projectile_color: Color = Color("ff7b2e")


func setup(start_position: Vector2, end_position: Vector2, color: Color = Color("ff7b2e")) -> void:
	global_position = start_position
	destination = end_position
	projectile_color = color
	queue_redraw()


func _process(delta: float) -> void:
	global_position = global_position.move_toward(destination, travel_speed * delta)
	queue_redraw()
	if global_position.distance_to(destination) <= 2.0:
		impacted.emit(destination)
		queue_free()


func _draw() -> void:
	draw_circle(Vector2(-5, 0), 3.0, Color(projectile_color, 0.25))
	draw_circle(Vector2.ZERO, 5.0, Color("ffd166"))
	draw_circle(Vector2.ZERO, 3.0, projectile_color)
