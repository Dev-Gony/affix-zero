extends RefCounted
class_name WorldLayout

# V6 abandons the old 3x3 arena board. The world is now a winding dungeon
# route spread over a 5x4 canvas. Empty cells remain true void, which makes
# the camera read corridors and rooms as a dungeon rather than a tiled board.
const ROOM_SIZE := Vector2(640, 400)
const GRID_SIZE := Vector2i(5, 4)
const WORLD_RECT := Rect2(Vector2.ZERO, Vector2(ROOM_SIZE.x * GRID_SIZE.x, ROOM_SIZE.y * GRID_SIZE.y))
const WALK_MARGIN := Vector2(58, 46)
const CORRIDOR_HALF_WIDTH: float = 27.0

# One 15-room dungeon circuit. Floor 16 starts the next themed circuit.
# Every consecutive pair is orthogonally adjacent so AUTO travel remains real
# movement through corridors instead of teleporting between combat boxes.
const FLOOR_PATH: Array[int] = [
	10, 11, 6, 1, 2,
	3, 8, 13, 12, 17,
	18, 19, 14, 9, 4,
]


static func room_index_for_floor(floor_number: int) -> int:
	return FLOOR_PATH[(maxi(1, floor_number) - 1) % FLOOR_PATH.size()]


static func dungeon_cycle_for_floor(floor_number: int) -> int:
	return floori(float(maxi(1, floor_number) - 1) / float(FLOOR_PATH.size()))


static func active_room_indices() -> Array[int]:
	return FLOOR_PATH.duplicate()


static func room_grid(room_index: int) -> Vector2i:
	return Vector2i(room_index % GRID_SIZE.x, floori(float(room_index) / float(GRID_SIZE.x)))


static func room_rect(room_index: int) -> Rect2:
	var grid := room_grid(room_index)
	return Rect2(Vector2(grid.x * ROOM_SIZE.x, grid.y * ROOM_SIZE.y), ROOM_SIZE)


static func walk_rect(room_index: int) -> Rect2:
	var room := room_rect(room_index)
	var variant: int = absi(room_index * 37) % 4
	var extra := Vector2.ZERO
	match variant:
		0:
			extra = Vector2(22, 10)
		1:
			extra = Vector2(8, 26)
		2:
			extra = Vector2(30, 18)
		_:
			extra = Vector2(14, 14)
	var margin := WALK_MARGIN + extra
	return Rect2(room.position + margin, room.size - margin * 2.0)


static func room_center(room_index: int) -> Vector2:
	return walk_rect(room_index).get_center()


static func corridor_rect(room_a: int, room_b: int) -> Rect2:
	var grid_a := room_grid(room_a)
	var grid_b := room_grid(room_b)
	var walk_a := walk_rect(room_a)
	var walk_b := walk_rect(room_b)
	if grid_a.y == grid_b.y and absi(grid_a.x - grid_b.x) == 1:
		var left := walk_a if grid_a.x < grid_b.x else walk_b
		var right := walk_b if grid_a.x < grid_b.x else walk_a
		var y: float = (left.get_center().y + right.get_center().y) * 0.5
		return Rect2(
			Vector2(left.end.x, y - CORRIDOR_HALF_WIDTH),
			Vector2(right.position.x - left.end.x, CORRIDOR_HALF_WIDTH * 2.0)
		)
	if grid_a.x == grid_b.x and absi(grid_a.y - grid_b.y) == 1:
		var top := walk_a if grid_a.y < grid_b.y else walk_b
		var bottom := walk_b if grid_a.y < grid_b.y else walk_a
		var x: float = (top.get_center().x + bottom.get_center().x) * 0.5
		return Rect2(
			Vector2(x - CORRIDOR_HALF_WIDTH, top.end.y),
			Vector2(CORRIDOR_HALF_WIDTH * 2.0, bottom.position.y - top.end.y)
		)
	return Rect2()


static func travel_waypoints(room_a: int, room_b: int) -> Array[Vector2]:
	var points: Array[Vector2] = []
	var grid_a := room_grid(room_a)
	var grid_b := room_grid(room_b)
	var walk_a := walk_rect(room_a)
	var walk_b := walk_rect(room_b)
	if grid_a.y == grid_b.y and absi(grid_a.x - grid_b.x) == 1:
		var y: float = (walk_a.get_center().y + walk_b.get_center().y) * 0.5
		if grid_b.x > grid_a.x:
			points.append(Vector2(walk_a.end.x - 8.0, y))
			points.append(Vector2(walk_b.position.x + 8.0, y))
		else:
			points.append(Vector2(walk_a.position.x + 8.0, y))
			points.append(Vector2(walk_b.end.x - 8.0, y))
	elif grid_a.x == grid_b.x and absi(grid_a.y - grid_b.y) == 1:
		var x: float = (walk_a.get_center().x + walk_b.get_center().x) * 0.5
		if grid_b.y > grid_a.y:
			points.append(Vector2(x, walk_a.end.y - 8.0))
			points.append(Vector2(x, walk_b.position.y + 8.0))
		else:
			points.append(Vector2(x, walk_a.position.y + 8.0))
			points.append(Vector2(x, walk_b.end.y - 8.0))
	points.append(walk_b.get_center())
	return points


static func connected_room_pairs() -> Array[Vector2i]:
	var pairs: Array[Vector2i] = []
	for index: int in FLOOR_PATH.size() - 1:
		pairs.append(Vector2i(FLOOR_PATH[index], FLOOR_PATH[index + 1]))
	return pairs


static func is_active_room(room_index: int) -> bool:
	return FLOOR_PATH.has(room_index)
