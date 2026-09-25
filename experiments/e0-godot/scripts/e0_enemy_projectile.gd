class_name E0EnemyProjectile
extends Node2D

signal impacted(projectile: E0EnemyProjectile, travel_distance: float)
signal expired(projectile: E0EnemyProjectile)

var projectile_id: int = 0
var target: E0Warrior
var direction: Vector2 = Vector2.LEFT
var damage: int = 1
var speed: float = 230.0
var hit_radius: float = 14.0
var ttl: float = 2.6
var travel_distance: float = 0.0
var _finished: bool = false

func setup(id: int, spawn_position: Vector2, shot_direction: Vector2, target_actor: E0Warrior, raw_damage: int) -> void:
	projectile_id = id
	global_position = spawn_position
	direction = shot_direction.normalized()
	target = target_actor
	damage = raw_damage
	rotation = direction.angle()
	queue_redraw()

func _physics_process(delta: float) -> void:
	if _finished:
		return
	ttl -= delta
	if ttl <= 0.0 or target == null or not is_instance_valid(target) or target.is_dead():
		_finish_expired()
		return

	var start := global_position
	var finish := start + direction * speed * delta
	travel_distance += start.distance_to(finish)

	if _distance_point_to_segment(target.global_position, start, finish) <= hit_radius:
		global_position = finish
		target.take_damage(damage)
		_finished = true
		impacted.emit(self, travel_distance)
		queue_free()
		return

	global_position = finish
	queue_redraw()

func _finish_expired() -> void:
	_finished = true
	expired.emit(self)
	queue_free()

func _distance_point_to_segment(point: Vector2, segment_start: Vector2, segment_end: Vector2) -> float:
	var segment := segment_end - segment_start
	var length_squared := segment.length_squared()
	if is_zero_approx(length_squared):
		return point.distance_to(segment_start)
	var t := clampf((point - segment_start).dot(segment) / length_squared, 0.0, 1.0)
	return point.distance_to(segment_start + segment * t)

func _draw() -> void:
	draw_circle(Vector2.ZERO, 5.0, Color("dba4ff"))
	draw_circle(Vector2(-7, 0), 3.0, Color(0.55, 0.27, 0.82, 0.55))
	draw_line(Vector2(-13, 0), Vector2(-5, 0), Color(0.63, 0.36, 0.91, 0.60), 3.0)
