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
	draw_rect(panel, Color(0.025, 0.035, 0.05, 0.88), true)
	draw_rect(panel, Color("405168"), false, 1.0)

	var pad := 6.0
	var gap := 2.0
	var available := size - Vector2(pad * 2.0, pad * 2.0)
	var cell := Vector2(
		(available.x - gap * 2.0) / 3.0,
		(available.y - gap * 2.0) / 3.0
	)
	var current_room: int = WorldLayout.room_index_for_floor(current_floor)
	var next_room: int = WorldLayout.room_index_for_floor(current_floor + 1)
	var previous_room: int = WorldLayout.room_index_for_floor(maxi(1, current_floor - 1))

	for room_index: int in 9:
		var grid := WorldLayout.room_grid(room_index)
		var pos := Vector2(pad + grid.x * (cell.x + gap), pad + grid.y * (cell.y + gap))
		var rect := Rect2(pos, cell)
		var fill := Color("111821")
		var border := Color("2c3a49")
		if room_index == previous_room and current_floor > 1:
			fill = Color("18202a")
			border = Color("4d647d")
		if room_index == next_room:
			fill = Color("18243a")
			border = Color("5e8fb9")
		if room_index == current_room:
			var pulse_alpha: float = 0.82 + sin(pulse * 5.0) * 0.12
			fill = Color("4a3516")
			border = Color("f0b84b", pulse_alpha)
		draw_rect(rect, fill, true)
		draw_rect(rect, border, false, 1.0)

		if room_index == current_room:
			draw_circle(rect.get_center(), minf(cell.x, cell.y) * 0.18, Color("f5d06f"))
		elif room_index == next_room:
			var c := rect.get_center()
			draw_colored_polygon(PackedVector2Array([
				c + Vector2(0, -3),
				c + Vector2(3, 2),
				c + Vector2(-3, 2),
			]), Color("72c6ff"))

	if current_floor % 10 == 0:
		draw_rect(Rect2(size.x - 12, 3, 8, 8), Color("5c1118"), true)
		draw_rect(Rect2(size.x - 12, 3, 8, 8), Color("ff5b67"), false, 1.0)
		draw_circle(Vector2(size.x - 8, 7), 1.5, Color("ffced2"))
