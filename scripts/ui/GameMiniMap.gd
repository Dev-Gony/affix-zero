extends Control
class_name GameMiniMap

var current_floor: int = 1
var pulse: float = 0.0


func set_floor(floor_number: int) -> void:
	current_floor = maxi(1, floor_number)
	queue_redraw()


func _process(delta: float) -> void:
	pulse += delta
	queue_redraw()


func _draw() -> void:
	var panel := Rect2(Vector2.ZERO, size)
	draw_rect(panel, Color(0.018, 0.026, 0.038, 0.92), true)
	draw_rect(panel, Color("4c5d73"), false, 1.0)

	var pad := 7.0
	var grid_size := Vector2(float(WorldLayout.GRID_SIZE.x), float(WorldLayout.GRID_SIZE.y))
	var map_size := size - Vector2(pad * 2.0, pad * 2.0)
	var step := Vector2(
		map_size.x / maxf(1.0, grid_size.x - 1.0),
		map_size.y / maxf(1.0, grid_size.y - 1.0)
	)
	var current_room: int = WorldLayout.room_index_for_floor(current_floor)
	var next_room: int = WorldLayout.room_index_for_floor(current_floor + 1)
	var previous_room: int = WorldLayout.room_index_for_floor(maxi(1, current_floor - 1))

	# Draw the actual dungeon route first, not a decorative cell grid.
	for pair: Vector2i in WorldLayout.connected_room_pairs():
		var a_grid := WorldLayout.room_grid(pair.x)
		var b_grid := WorldLayout.room_grid(pair.y)
		var a := Vector2(pad + a_grid.x * step.x, pad + a_grid.y * step.y)
		var b := Vector2(pad + b_grid.x * step.x, pad + b_grid.y * step.y)
		draw_line(a, b, Color("34455a"), 2.0)

	for room_index: int in WorldLayout.active_room_indices():
		var grid := WorldLayout.room_grid(room_index)
		var center := Vector2(pad + grid.x * step.x, pad + grid.y * step.y)
		var half := Vector2(5.0, 4.0)
		var rect := Rect2(center - half, half * 2.0)
		var fill := Color("101720")
		var border := Color("53657a")
		if room_index == previous_room and current_floor > 1:
			fill = Color("17202b")
			border = Color("6b7e95")
		if room_index == next_room:
			fill = Color("132338")
			border = Color("6bb5e8")
		if room_index == current_room:
			var pulse_alpha: float = 0.86 + sin(pulse * 5.0) * 0.10
			fill = Color("6a4715")
			border = Color("ffc44f", pulse_alpha)
		draw_rect(rect, fill, true)
		draw_rect(rect, border, false, 1.0)
		if room_index == current_room:
			draw_circle(center, 2.1, Color("ffe18b"))
		elif room_index == next_room:
			draw_circle(center, 1.7, Color("7ed2ff"))

	if current_floor % 10 == 0:
		draw_circle(Vector2(size.x - 8, 8), 4.0, Color("5c1118"))
		draw_circle(Vector2(size.x - 8, 8), 2.0, Color("ff6d78"))
