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
	draw_rect(panel, Color(0.025, 0.035, 0.05, 0.90), true)
	draw_rect(panel, Color("405168"), false, 1.0)

	var pad := 6.0
	var gap := 2.0
	var available := size - Vector2(pad * 2.0, pad * 2.0)
	var cell := Vector2(
		(available.x - gap * float(WorldLayout.GRID_SIZE.x - 1)) / float(WorldLayout.GRID_SIZE.x),
		(available.y - gap * float(WorldLayout.GRID_SIZE.y - 1)) / float(WorldLayout.GRID_SIZE.y)
	)
	var current_room: int = WorldLayout.room_index_for_floor(current_floor)
	var current_route_pos: int = WorldLayout.route_position(current_room)

	# Corridors first, so the minimap reads like a dungeon graph rather than a board.
	for pair: Vector2i in WorldLayout.connected_room_pairs():
		var a_grid := WorldLayout.room_grid(pair.x)
		var b_grid := WorldLayout.room_grid(pair.y)
		var a_center := Vector2(
			pad + a_grid.x * (cell.x + gap) + cell.x * 0.5,
			pad + a_grid.y * (cell.y + gap) + cell.y * 0.5
		)
		var b_center := Vector2(
			pad + b_grid.x * (cell.x + gap) + cell.x * 0.5,
			pad + b_grid.y * (cell.y + gap) + cell.y * 0.5
		)
		draw_line(a_center, b_center, Color("314156"), maxf(1.0, minf(cell.x, cell.y) * 0.22))

	for room_index: int in WorldLayout.visible_rooms():
		var grid := WorldLayout.room_grid(room_index)
		var pos := Vector2(pad + grid.x * (cell.x + gap), pad + grid.y * (cell.y + gap))
		var rect := Rect2(pos, cell)
		var route_pos: int = WorldLayout.route_position(room_index)
		var fill := Color("111821")
		var border := Color("263545")
		if route_pos >= 0 and current_route_pos >= 0 and route_pos < current_route_pos:
			fill = Color("18202a")
			border = Color("4d647d")
		elif route_pos == current_route_pos + 1:
			fill = Color("18243a")
			border = Color("5e8fb9")
		if room_index == current_room:
			var pulse_alpha: float = 0.84 + sin(pulse * 5.0) * 0.10
			fill = Color("4a3516")
			border = Color("f0b84b", pulse_alpha)
		draw_rect(rect, fill, true)
		draw_rect(rect, border, false, 1.0)
		if room_index == current_room:
			draw_circle(rect.get_center(), minf(cell.x, cell.y) * 0.20, Color("f5d06f"))

	if current_floor % 10 == 0:
		draw_circle(Vector2(size.x - 8, 8), 4.0, Color("ff5b67"))
