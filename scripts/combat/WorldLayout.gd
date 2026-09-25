extends RefCounted
class_name WorldLayout

# G6 dungeon topology:
# a larger winding floor plan instead of the old fully-connected 3x3 arena grid.
const ROOM_SIZE := Vector2(520, 340)
const GRID_SIZE := Vector2i(5, 4)
const WORLD_RECT := Rect2(Vector2.ZERO, Vector2(ROOM_SIZE.x * GRID_SIZE.x, ROOM_SIZE.y * GRID_SIZE.y))
const WALK_MARGIN := Vector2(62, 48)
const CORRIDOR_HALF_WIDTH: float = 30.0

# Main expedition route. Consecutive rooms are always orthogonally adjacent.
# It snakes through the dungeon, doubles back, and creates long traversal reads.
const DUNGEON_PATH: Array[int] = [
	10, 11, 6, 7, 8, 3, 4, 9, 14, 13, 12, 17, 18, 19,
]

# Floor progression walks the route forward and back so the wrap point
# remains physically adjacent instead of teleporting across the dungeon.
const FLOOR_TRAVERSAL: Array[int] = [
	10, 11, 6, 7, 8, 3, 4, 9, 14, 13, 12, 17, 18, 19,
	18, 17, 12, 13, 14, 9, 4, 3, 8, 7, 6, 11,
]

# Small side chambers visually sell a dungeon instead of a board.
const SIDE_CONNECTIONS: Array[Vector2i] = [
	Vector2i(6, 5),
	Vector2i(8, 13),
	Vector2i(3, 2),
	Vector2i(14, 15),
	Vector2i(12, 16),
	Vector2i(18, 17),
]


static func room_index_for_floor(floor_number: int) -> int:
	return FLOOR_TRAVERSAL[(maxi(1, floor_number) - 1) % FLOOR_TRAVERSAL.size()]


static func encounter_kill_goal(floor_number: int) -> int:
	# A room is a short ARPG encounter, not an 80+ kill stationary arena.
	# Difficulty rises mostly through enemy stats/elite chance, while traversal remains frequent.
	return 8 + mini(6, floori(float(maxi(1, floor_number) - 1) / 15.0))


static func room_grid(room_index: int) -> Vector2i:
	return Vector2i(room_index % GRID_SIZE.x, floori(float(room_index) / float(GRID_SIZE.x)))


static func room_rect(room_index: int) -> Rect2:
	var grid := room_grid(room_index)
	return Rect2(Vector2(grid.x * ROOM_SIZE.x, grid.y * ROOM_SIZE.y), ROOM_SIZE)


static func walk_rect(room_index: int) -> Rect2:
	var room := room_rect(room_index)
	var margin := WALK_MARGIN
	match room_index % 4:
		0:
			margin = Vector2(88, 62)
		1:
			margin = Vector2(48, 74)
		2:
			margin = Vector2(108, 40)
		_:
			margin = Vector2(62, 48)
	if route_position(room_index) < 0:
		margin += Vector2(26, 18)
	return Rect2(room.position + margin, room.size - margin * 2.0)


static func room_center(room_index: int) -> Vector2:
	return walk_rect(room_index).get_center()


static func room_is_used(room_index: int) -> bool:
	if DUNGEON_PATH.has(room_index):
		return true
	for pair: Vector2i in SIDE_CONNECTIONS:
		if pair.x == room_index or pair.y == room_index:
			return true
	return false


static func route_position(room_index: int) -> int:
	return DUNGEON_PATH.find(room_index)


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
			points.append(Vector2(walk_a.end.x - 8.0, y))
			points.append(Vector2(walk_b.position.x + 8.0, y))
		else:
			points.append(Vector2(walk_a.position.x + 8.0, y))
			points.append(Vector2(walk_b.end.x - 8.0, y))
	elif grid_a.x == grid_b.x and absi(grid_a.y - grid_b.y) == 1:
		var x := walk_a.get_center().x
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
	for index: int in DUNGEON_PATH.size() - 1:
		pairs.append(Vector2i(DUNGEON_PATH[index], DUNGEON_PATH[index + 1]))
	for pair: Vector2i in SIDE_CONNECTIONS:
		pairs.append(pair)
	return pairs


static func visible_rooms() -> Array[int]:
	var rooms: Array[int] = []
	for room_index: int in DUNGEON_PATH:
		if not rooms.has(room_index):
			rooms.append(room_index)
	for pair: Vector2i in SIDE_CONNECTIONS:
		if not rooms.has(pair.x):
			rooms.append(pair.x)
		if not rooms.has(pair.y):
			rooms.append(pair.y)
	return rooms
