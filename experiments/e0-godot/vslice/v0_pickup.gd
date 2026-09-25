class_name V0Pickup
extends Node2D

signal collected(kind: StringName, amount: int)

var kind: StringName = &"xp"
var amount: int = 1
var target: V0Player
var age: float = 0.0
var velocity := Vector2.ZERO
var collected_flag: bool = false

func setup(pickup_kind: StringName, pickup_amount: int, player: V0Player) -> void:
	kind = pickup_kind
	amount = pickup_amount
	target = player
	velocity = Vector2(randf_range(-18, 18), randf_range(-32, -16))
	queue_redraw()

func _process(delta: float) -> void:
	if collected_flag or target == null or target.dead:
		return
	age += delta
	if age < 0.28:
		velocity.y += 85.0 * delta
		global_position += velocity * delta
	else:
		var distance := global_position.distance_to(target.global_position)
		if distance < 120.0:
			var desired := global_position.direction_to(target.global_position) * minf(360.0, 110.0 + (120.0 - distance) * 2.4)
			velocity = velocity.lerp(desired, minf(1.0, delta * 7.0))
			global_position += velocity * delta
		if distance <= 12.0:
			collected_flag = true
			collected.emit(kind, amount)
			queue_free()
	queue_redraw()

func _draw() -> void:
	var color := Color("68e58f") if kind == &"xp" else Color("f6c85f")
	var r := 4.5 if kind == &"xp" else 3.6
	var points := PackedVector2Array([Vector2(0,-r),Vector2(r,0),Vector2(0,r),Vector2(-r,0)])
	draw_colored_polygon(points, color)
	draw_polyline(PackedVector2Array([points[0],points[1],points[2],points[3],points[0]]), color.lightened(0.35), 1.2)
