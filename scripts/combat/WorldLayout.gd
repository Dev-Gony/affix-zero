extends RefCounted
class_name WorldLayout

const ROOM_SIZE := Vector2(640, 400)
const GRID_SIZE := Vector2i(3, 3)
const WORLD_RECT := Rect2(Vector2.ZERO, Vector2(ROOM_SIZE.x * GRID_SIZE.x, ROOM_SIZE.y * GRID_SIZE.y))
const WALK_MARGIN := Vector2(72, 54)
const CORRIDOR_HALF_WIDTH: float = 34.0
const FLOOR_PATH: Array[int] = [4, 5, 2, 1, 0, 3, 6, 7, 8, 7, 6, 3, 0, 1, 2, 5]


static func room_index_for_floor(floor_number: int) -> int:
	return FLOOR_PATH[(maxi(1, floor_number) - 1) % FLOOR_PATH.size()]


static func room_grid(room_index: int) -> Vector2i:
	return Vector2i(room_index % GRID_SIZE.x, floori(float(room_index) / float(GRID_SIZE.x)))


static func room_rect(room_index: int) -> Rect2:
	var grid := room_grid(room_index)
	return Rect2(Vector2(grid.x * ROOM_SIZE.x, grid.y * ROOM_SIZE.y), ROOM_SIZE)


static func walk_rect(room_index: int) -> Rect2:
	var room := room_rect(room_index)
	return Rect2(room.position + WALK_MARGIN, room.size - WALK_MARGIN * 2.0)


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
		return Rect2(
			Vector2(left.end.x, left.get_center().y - CORRIDOR_HALF_WIDTH),
			Vector2(right.position.x - left.end.x, CORRIDOR_HALF_WIDTH * 2.0)
		)
	if grid_a.x == grid_b.x and absi(grid_a.y - grid_b.y) == 1:
		var top := walk_a if grid_a.y < grid_b.y else walk_b
		var bottom := walk_b if grid_a.y < grid_b.y else walk_a
		return Rect2(
			Vector2(top.get_center().x - CORRIDOR_HALF_WIDTH, top.end.y),
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
		var y := walk_a.get_center().y
		if grid_b.x > grid_a.x:
			points.append(Vector2(walk_a.end.x - 10.0, y))
			points.append(Vector2(walk_b.position.x + 10.0, y))
		else:
			points.append(Vector2(walk_a.position.x + 10.0, y))
			points.append(Vector2(walk_b.end.x - 10.0, y))
	elif grid_a.x == grid_b.x and absi(grid_a.y - grid_b.y) == 1:
		var x := walk_a.get_center().x
		if grid_b.y > grid_a.y:
			points.append(Vector2(x, walk_a.end.y - 10.0))
			points.append(Vector2(x, walk_b.position.y + 10.0))
		else:
			points.append(Vector2(x, walk_a.position.y + 10.0))
			points.append(Vector2(x, walk_b.end.y - 10.0))
	points.append(walk_b.get_center())
	return points


static func connected_room_pairs() -> Array[Vector2i]:
	var pairs: Array[Vector2i] = []
	for y in GRID_SIZE.y:
		for x in GRID_SIZE.x:
			var index := y * GRID_SIZE.x + x
			if x < GRID_SIZE.x - 1:
				pairs.append(Vector2i(index, index + 1))
			if y < GRID_SIZE.y - 1:
				pairs.append(Vector2i(index, index + GRID_SIZE.x))
	return pairs
